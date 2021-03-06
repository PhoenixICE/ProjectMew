﻿using System;

namespace ProjectMew
{
    public class PluginContainer : IDisposable
    {
        public ApiPlugin Plugin
        {
            get;
            protected set;
        }
        public bool Initialized
        {
            get;
            protected set;
        }
        public bool Dll
        {
            get;
            set;
        }

        public PluginContainer(ApiPlugin plugin) : this(plugin, true)
        {
        }

        public PluginContainer(ApiPlugin plugin, bool dll)
        {
            this.Plugin = plugin;
            this.Initialized = false;
            this.Dll = dll;
        }

        public void Initialize()
        {
            this.Plugin.Initialize();
            this.Initialized = true;
        }

        public void DeInitialize()
        {
            this.Initialized = false;
        }

        public void Dispose()
        {
            this.Plugin.Dispose();
        }
    }
}
