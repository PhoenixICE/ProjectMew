using ProjectMew.Config;
using ProjectMew.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ProjectMew.Hooks;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;

namespace ProjectMew
{
    class ProjectMew
    {
        /// <summary>
        /// VersionNum - Lazy way to do versioning
        /// </summary>
        public static readonly Version VersionNum = Assembly.GetExecutingAssembly().GetName().Version;
        /// <summary>
        /// VersionCodeName - To denote Version Types
        /// </summary>
        public static object VersionCodename = "Alpha";
        /// <summary>
        /// ConfigPath - Subfolder location for the ConfigFile
        /// </summary>
        public static readonly string ConfigPath = "Config";
        /// <summary>
        /// PluginDirectoryPath - Subfolder for plugins
        /// </summary>
        private static string PluginsDirectoryPath = "Plugin";
        /// <summary>
        /// Config - Static reference to the config system, for accessing values set in users' config files.
        /// </summary>
        public static ConfigFile Config { get; set; }

        public static ILog Log;
        private static readonly Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();
        private static readonly List<PluginContainer> plugins = new List<PluginContainer>();
        public static ReadOnlyCollection<PluginContainer> Plugins
        {
            get { return new ReadOnlyCollection<PluginContainer>(plugins); }
        }

        public static PSPlayer Player { get; set; }

        static void Main(string[] args)
        {
            string logFilename = "ProjectMew.log";
            Log = new TextLog(logFilename, false);

            Log.ConsoleInfo("Initializing One moment...");

            Config = new ConfigFile();
            Initialize();

            Log.ConsoleInfo("Initialization Completed. Awaiting Commands...");

            while (true)
            {
                string cmd = Console.ReadLine();
                Commands.HandleCommand(Player, cmd);
            }
        }

        public static void Initialize()
        {
            Player = new PSPlayer();
            Commands.InitCommands();

            if (!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);

            ConfigFile.ConfigRead += OnConfigRead;
            FileTools.SetupConfig();

            Hooks.UserHooks.UserPreLogin += OnUserPreLogin;
            Hooks.UserHooks.UserPostLogin += OnUserLogin;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            LoadPlugins();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string fileName = args.Name.Split(',')[0];
            string path = Path.Combine(PluginsDirectoryPath, fileName + ".dll");
            try
            {
                if (File.Exists(path))
                {
                    Assembly assembly;
                    if (!loadedAssemblies.TryGetValue(fileName, out assembly))
                    {
                        assembly = Assembly.Load(File.ReadAllBytes(path));
                        loadedAssemblies.Add(fileName, assembly);
                    }
                    return assembly;
                }
            }
            catch (Exception ex)
            {
                Log.ConsoleError(string.Format("Error on resolving assembly \"{0}.dll\":\n{1}", fileName, ex));
            }
            return null;
        }

        internal static void LoadPlugins()
        {
            if (!Directory.Exists(PluginsDirectoryPath))
            {
                Directory.CreateDirectory(PluginsDirectoryPath);
            }

            List<FileInfo> fileInfos = new DirectoryInfo(PluginsDirectoryPath).GetFiles("*.dll").ToList();
            fileInfos.AddRange(new DirectoryInfo(PluginsDirectoryPath).GetFiles("*.dll-plugin"));

            foreach (FileInfo fileInfo in fileInfos)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);

                try
                {
                    Assembly assembly;
                    // The plugin assembly might have been resolved by another plugin assembly already, so no use to
                    // load it again, but we do still have to verify it and create plugin instances.
                    if (!loadedAssemblies.TryGetValue(fileNameWithoutExtension, out assembly))
                    {
                        try
                        {
                            assembly = Assembly.Load(File.ReadAllBytes(fileInfo.FullName));
                        }
                        catch (BadImageFormatException)
                        {
                            continue;
                        }
                        loadedAssemblies.Add(fileNameWithoutExtension, assembly);
                    }

                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (!type.IsSubclassOf(typeof(ApiPlugin)) || !type.IsPublic || type.IsAbstract)
                            continue;

                        ApiPlugin pluginInstance;
                        try
                        {
                            pluginInstance = (ApiPlugin)Activator.CreateInstance(type);
                        }
                        catch (Exception ex)
                        {
                            // Broken plugins better stop the entire server init.
                            throw new InvalidOperationException(
                                string.Format("Could not create an instance of plugin class \"{0}\".", type.FullName), ex);
                        }
                        plugins.Add(new PluginContainer(pluginInstance));
                    }
                }
                catch (Exception ex)
                {
                    // Broken assemblies / plugins better stop the entire server init.
                    throw new InvalidOperationException(
                        string.Format("Failed to load assembly \"{0}\".", fileInfo.Name), ex);
                }
            }
            IOrderedEnumerable<PluginContainer> orderedPluginSelector =
                from x in Plugins
                orderby x.Plugin.Order, x.Plugin.Name
                select x;

            foreach (PluginContainer current in orderedPluginSelector)
            {
                try
                {
                    current.Initialize();
                }
                catch (Exception ex)
                {
                    // Broken plugins better stop the entire server init.
                    throw new InvalidOperationException(string.Format(
                        "Plugin \"{0}\" has thrown an exception during initialization.", current.Plugin.Name), ex);
                }
            }
        }

        internal static void UnloadPlugins()
        {
            foreach (PluginContainer pluginContainer in plugins)
            {
                try
                {
                    pluginContainer.DeInitialize();
                }
                catch (Exception ex)
                {
                    Log.ConsoleError(string.Format("Plugin \"{0}\" has thrown an exception while being deinitialized:\n{1}", pluginContainer.Plugin.Name, ex));
                }
            }

            foreach (PluginContainer pluginContainer in plugins)
            {
                try
                {
                    pluginContainer.Dispose();
                }
                catch (Exception ex)
                {
                    Log.ConsoleError(string.Format("Plugin \"{0}\" has thrown an exception while being disposed:\n{1}", pluginContainer.Plugin.Name, ex));
                }
            }
        }

        private static void OnUserLogin(UserPostLoginEventArgs args)
        {
        }

        private static void OnUserPreLogin(UserPreLoginEventArgs args)
        {
        }

        private static void OnConfigRead(ConfigFile File)
        {
        }
    }
}
