namespace Hexa.NET.ImGui.MelonLoader.Plugins
{
    using System;
    using System.Collections.Generic;

    public class PluginManager
    {
        private List<PluginInstance> plugins = [];

        public PluginManager()
        {
        }

        public IReadOnlyList<PluginInstance> Plugins => plugins;

        public void LoadFromFolder(string path)
        {
            if (!Directory.Exists(path)) return;
            foreach (var file in Directory.EnumerateFiles(path, "*.dll"))
            {
                Load(file);
            }
        }

        public void ReloadAll()
        {
            foreach (var plugin in plugins)
            {
                plugin.Reload();
            }
        }

        public void Load(string path)
        {
            PluginInstance instance = new(path);
            instance.Load();
            plugins.Add(instance);
        }

        public void DoAll(Action<Plugin> action)
        {
            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.Do(action);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void DoAll<T>(T payload, Action<T, Plugin> action)
        {
            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.Do(payload, action);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void Unload()
        {
            foreach (var plugin in plugins)
            {
                plugin.Unload();
            }
            plugins.Clear();
        }
    }
}