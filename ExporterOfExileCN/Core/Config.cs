using Newtonsoft.Json;
using System.IO;

namespace ExporterOfExileCN.Core
{
    class Config
    {
        private static readonly string CONFIG_FILE = "config.json";
        private static readonly int DEFAULT_LISTEN_PORT = 8655;

        public int ListenPort { get; set; }
        public string LastPatchedPath { get; set; }

        public static Config Load()
        {
            if (File.Exists(CONFIG_FILE)) {
                string configContent = File.ReadAllText(CONFIG_FILE);
                var config = JsonConvert.DeserializeObject<Config>(configContent);
                return config;
            }
            else
            {
                var config = new Config
                {
                    ListenPort = DEFAULT_LISTEN_PORT
                };

                Save(config);
                return config;
            }
        }

        public static void Save(Config config)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(CONFIG_FILE, FileMode.Create)))
            {
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                sw.Write(jsonString);
                sw.Flush();
            }
        }
    }
}
