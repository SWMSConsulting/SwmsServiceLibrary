using Newtonsoft.Json.Linq;

namespace SwmsServiceLibrary
{
    public class EnvironmentVariableService
    {
        private static readonly string _configPath = "config/environment.json";

        static EnvironmentVariableService()
        {
            EnsureConfigFileExists();
        }

        private static void EnsureConfigFileExists()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    var defaultConfig = new JObject(new JObject());
                    File.WriteAllText(_configPath, defaultConfig.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EnvironmentVariableService EnsureConfigFileExists: {ex.Message}");
            }
        }

        public static string? GetSavedEnvironmentVariable(string key)
        {
            if (File.Exists(_configPath))
            {
                var config = JObject.Parse(File.ReadAllText(_configPath));
                return config["EnvironmentVariables"]?[key]?.ToString();
            }
            return null;
        }

        public static string? GetStringEnvironmentVariable(string variableName, bool required)
        {
            var savedValue = GetSavedEnvironmentVariable(variableName);
            return savedValue ?? GetStringFromENV(variableName, required);
        }

        public static bool? GetBoolEnvironmentVariable(string variableName, bool required)
        {
            string? savedValue = GetSavedEnvironmentVariable(variableName);
            bool isValidBoolean = bool.TryParse(savedValue, out bool result);
            return isValidBoolean ? result : GetBoolFromENV(variableName, required);
        }

        public static int? GetIntEnvironmentVariable(string variableName, bool required)
        {
            string? savedValue = GetSavedEnvironmentVariable(variableName);
            bool isValidInt = int.TryParse(savedValue, out int result);
            return isValidInt ? result : GetIntFromENV(variableName, required);
        }

        /// <summary>
        /// To persist environment variables across container restarts, use a named volume.
        /// <example>
        /// <code>
        /// application_name:
        ///     image: some_image
        ///     volumes:
        ///       - app_data:/app/config
        ///
        /// volumes:
        ///    app_data:
        /// </code>
        /// </example>
        /// This setup mounts a named volume 'app_data' to '/app/config' in the container.
        /// </summary>
        public static void SaveEnvironmentVariable(string key, string value)
        {
            try
            {
                var config = JObject.Parse(File.ReadAllText(_configPath));

                if (config["EnvironmentVariables"] == null)
                {
                    config["EnvironmentVariables"] = new JObject();
                }

                config["EnvironmentVariables"][key] = value;
                File.WriteAllText(_configPath, config.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EnvironmentVariableService SaveEnvironmentVariable: {ex.Message}");
            }
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

        public static string? GetStringFromENV(string variableName, bool required)
        {
            return GetENV(variableName, required);
        }

        public static int? GetIntFromENV(string variableName, bool required)
        {
            var envString = GetENV(variableName, required);

            if (envString == null)
            {
                return null;
            }

            var isValidInt = int.TryParse(envString, out int result);

            if (isValidInt)
            {
                return result;
            }
            
            throw new ArgumentException($"ENV variable {variableName} is not a valid int! Value '{envString}'");
        }

        public static bool? GetBoolFromENV(string variableName, bool required)
        {
            var envString = GetENV(variableName, required);

            if (envString == null)
            {
                return null;
            }

            var isValidBool = bool.TryParse(envString, out bool result);

            if (isValidBool)
            {
                return result;
            }

            throw new ArgumentException($"ENV variable {variableName} is not a valid bool! Value '{envString}'");

        }

        private static string? GetENV(string variableName, bool required)
        {
            var envString = Environment.GetEnvironmentVariable(variableName);

            if (required && envString == null)
            {
                throw new ArgumentException($"ENV variable {variableName} is missing!");
            }
            return envString;
        }

    }
}
