using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMew.Config
{
    public static class FileTools
    {
        /// <summary>
        /// Creates an empty file at the given path.
        /// </summary>
        /// <param name="file">The path to the file.</param>
        public static void CreateFile(string file)
        {
            File.Create(file).Close();
        }

        /// <summary>
        /// Creates a file if the files doesn't already exist.
        /// </summary>
        /// <param name="file">The path to the files</param>
        /// <param name="data">The data to write to the file.</param>
        public static void CreateIfNot(string file, string data = "")
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, data);
            }
        }

        /// <summary>
        /// Sets up the configuration file for all variables, and creates any missing files.
        /// </summary>
        public static void SetupConfig()
        {
            if (!Directory.Exists(ProjectMew.ConfigPath))
            {
                Directory.CreateDirectory(ProjectMew.ConfigPath);
            }

            var ConfigFilePath = Path.Combine(ProjectMew.ConfigPath, "Config.json");

            if (File.Exists(ConfigFilePath))
            {
                ProjectMew.Config = ConfigFile.Read(ConfigFilePath);
            }

            // Add all the missing config properties in the json file
            ProjectMew.Config.Write(ConfigFilePath);
        }
    }
}
