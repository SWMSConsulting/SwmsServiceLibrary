using Newtonsoft.Json.Linq;

namespace SwmsServiceLibrary
{
    public class EnvironmentVariableService
    {
        private static readonly string _configPath = "appsettings.json";

        static EnvironmentVariableService()
        {
            EnsureConfigFileExists();
        }

        private static void EnsureConfigFileExists()
        {
            if (!File.Exists(_configPath))
            {
                var defaultConfig = new JObject(new JObject());
                File.WriteAllText(_configPath, defaultConfig.ToString());
            }
        }

        public static string GetSavedEnvironmentVariable(string key)
        {
            if (File.Exists(_configPath))
            {
                var config = JObject.Parse(File.ReadAllText(_configPath));
                return config["EnvironmentVariables"]?[key]?.ToString();
            }
            return null;
        }

        public static string GetEnvironmentVariable(string variableName)
        {
            var savedValue = GetSavedEnvironmentVariable(variableName);
            return savedValue ?? GetRequiredStringFromENV(variableName);
        }

        public static void SaveEnvironmentVariable(string key, string value)
        {
            var config = JObject.Parse(File.ReadAllText(_configPath));

            if (config["EnvironmentVariables"] == null)
            {
                config["EnvironmentVariables"] = new JObject();
            }

            config["EnvironmentVariables"][key] = value;
            File.WriteAllText(_configPath, config.ToString());
        }

        public void LoadFromEnvironment()
        {
            var config = JObject.Parse(File.ReadAllText(_configPath));

            if (config["EnvironmentVariables"] == null)
            {
                config["EnvironmentVariables"] = new JObject();
            }

            foreach (var envVar in Environment.GetEnvironmentVariables().Keys)
            {
                string key = envVar.ToString();
                string value = Environment.GetEnvironmentVariable(key);
                config["EnvironmentVariables"][key] = value;
            }

            File.WriteAllText(_configPath, config.ToString());
        }
        public static int GetRequiredIntFromENV(string variableName)
        {
            var intString = Environment.GetEnvironmentVariable(variableName);
            return intString == null ? throw new ArgumentException($"ENV variable {variableName} is missing!") : int.Parse(intString);
        }

        public static bool GetRequiredBoolFromENV(string variableName)
        {
            var boolString = Environment.GetEnvironmentVariable(variableName);
            return boolString == null ? throw new ArgumentException($"ENV variable {variableName} is missing!") : bool.Parse(boolString);
        }

        public static string GetRequiredStringFromENV(string variableName)
        {
            var envString = Environment.GetEnvironmentVariable(variableName);
            return envString == null ? throw new ArgumentException($"ENV variable {variableName} is missing!") : envString;
        }

    }
}
