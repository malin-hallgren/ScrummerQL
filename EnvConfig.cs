using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL
{
    internal class EnvConfig
    {
        public static (string token, string url, string connection) Config(bool isTest = false)
        {
            var envPath = Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                ".env"
            );

            string[] env = File.ReadAllLines(envPath);
            string? token = null;
            string? gitlabUrl = null;
            string connectionString = null;

            foreach (var line in env)
            {
                if (line.StartsWith("TOKEN: "))
                {
                    token = line.Substring("TOKEN: ".Length).Trim();
                }
                else if (line.StartsWith("PATH: "))
                {
                    gitlabUrl = line.Substring("PATH: ".Length).Trim();
                }
                else if (line.StartsWith("TEST_PATH: ") && isTest)
                {
                    gitlabUrl = line.Substring("TEST_PATH: ".Length).Trim();
                }
                else if (line.StartsWith("CONNECTION_STRING: "))
                {
                    connectionString = line.Substring("CONNECTION_STRING: ".Length).Trim();
                }
            }

            return (token, gitlabUrl, connectionString);
        }
        
    }
}
