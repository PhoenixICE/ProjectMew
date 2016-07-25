using Newtonsoft.Json;
using PokemonGoDesktop.API.Proto;
using System;
using System.Collections.Generic;
using System.IO;

namespace ProjectMew.Config
{
    /// <summary>ConfigFile - The config file class, which contains the configuration for a server that is serialized into JSON and deserialized on load.</summary>
    public class ConfigFile
    {
        public string RefreshToken { get; set; }
        public string PtcUserName { get; set; }
        public string PtcPassword { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }
        public List<PokemonId> EvolveList { get; set; }
        public bool EvolveListIsBlack { get; set; }
        /// <summary>
        /// Reads a configuration file from a given path
        /// </summary>
        /// <param name="path">string path</param>
        /// <returns>ConfigFile object</returns>
        public static ConfigFile Read(string path)
        {
            if (!File.Exists(path))
                return new ConfigFile();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

        /// <summary>
        /// Reads the configuration file from a stream
        /// </summary>
        /// <param name="stream">stream</param>
        /// <returns>ConfigFile object</returns>
        public static ConfigFile Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<ConfigFile>(sr.ReadToEnd());
                if (ConfigRead != null)
                    ConfigRead(cf);
                return cf;
            }
        }

        /// <summary>
        /// Writes the configuration to a given path
        /// </summary>
        /// <param name="path">string path - Location to put the config file</param>
        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Write(fs);
            }
        }

        /// <summary>
        /// Writes the configuration to a stream
        /// </summary>
        /// <param name="stream">stream</param>
        public void Write(Stream stream)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }

        /// <summary>
        /// On config read hook
        /// </summary>
        public static Action<ConfigFile> ConfigRead;
    }
}