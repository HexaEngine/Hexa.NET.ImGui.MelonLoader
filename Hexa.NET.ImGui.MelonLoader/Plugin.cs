namespace Hexa.NET.ImGui.MelonLoader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;

    public abstract class Plugin
    {
        public abstract string Name { get; }

        public virtual void OnInitialized()
        {
        }

        public virtual void OnUnload()
        {
        }

        /// <summary>
        /// Runs when a new Scene is loaded.
        /// </summary>
        public virtual void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
        }

        /// <summary>
        /// Runs once a Scene is initialized.
        /// </summary>
        public virtual void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
        }

        /// <summary>
        /// Runs once a Scene unloads.
        /// </summary>
        public virtual void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
        }

        /// <summary>
        /// Runs once per frame.
        /// </summary>
        public virtual void OnUpdate()
        {
        }

        /// <summary>
        /// Can run multiple times per frame. Mostly used for Physics.
        /// </summary>
        public virtual void OnFixedUpdate()
        {
        }

        /// <summary>
        /// Runs once per frame, after <see cref="OnUpdate"/>.
        /// </summary>
        public virtual void OnLateUpdate()
        {
        }

        /// <summary>
        /// Can run multiple times per frame. Mostly used for Unity's IMGUI.
        /// </summary>
        public virtual void OnGUI()
        {
        }
    }

    public class PluginInstance
    {
        private AssemblyLoadContext assemblyLoadContext;
        private Assembly assembly;
        private Plugin plugin;
        private readonly string path;
        private readonly string name;
        private object _lock = new();

        public PluginInstance(string path)
        {
            this.path = path;
            name = Path.GetFileName(path);
        }

        public string Name => plugin.Name;

        public Assembly Assembly => assembly;

        public Plugin Plugin => plugin;

        public bool Loaded => assemblyLoadContext != null;

        public bool Failed { get; private set; }

        public Exception? FailedException { get; private set; }

        public bool Load()
        {
            lock (_lock)
            {
                if (assemblyLoadContext != null) return false;
                Failed = false;
                FailedException = null;
                string folder = Path.GetDirectoryName(path);
                string pdb = Path.Combine(folder, Path.GetFileNameWithoutExtension(name) + ".pdb");

                assemblyLoadContext = new AssemblyLoadContext(name, false);
                if (File.Exists(pdb))
                {
                    using FileStream fs = File.OpenRead(path);
                    using FileStream fsPdb = File.OpenRead(pdb);

                    try
                    {
                        assembly = assemblyLoadContext.LoadFromStream(fs, fsPdb);
                    }
                    catch (Exception ex)
                    {
                        Failed = true;
                        FailedException = ex;
                        Console.WriteLine(ex);
                        Unload();
                        return false;
                    }
                    finally
                    {
                        fs.Close();
                        fsPdb?.Close();
                    }
                }
                else
                {
                    using FileStream fs = File.OpenRead(path);

                    try
                    {
                        assembly = assemblyLoadContext.LoadFromStream(fs);
                    }
                    catch (Exception ex)
                    {
                        Failed = true;
                        FailedException = ex;
                        Console.WriteLine(ex);
                        Unload();
                        return false;
                    }
                    finally
                    {
                        fs.Close();
                    }
                }

                try
                {
                    var types = assembly.GetTypes();
                    var pluginType = types.FirstOrDefault(x => x.IsAssignableTo(typeof(Plugin)));
                    plugin = (Plugin)Activator.CreateInstance(pluginType);
                    plugin.OnInitialized();
                }
                catch (Exception ex)
                {
                    Failed = true;
                    FailedException = ex;
                    Console.WriteLine(ex);
                    Unload();
                    return false;
                }

                return true;
            }
        }

        public void Reload()
        {
            Unload();
            Load();
        }

        public void Do(Action<Plugin> action)
        {
            lock (_lock)
            {
                if (plugin == null) return;
                action(plugin);
            }
        }

        public void Do<T>(T payload, Action<T, Plugin> action)
        {
            lock (_lock)
            {
                if (plugin == null) return;
                action(payload, plugin);
            }
        }

        public void Unload()
        {
            lock (_lock)
            {
                if (assemblyLoadContext == null) return;
                try
                {
                    plugin?.OnUnload();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                plugin = null;
                assembly = null;

                GC.Collect();

                assemblyLoadContext.Unload();
                assemblyLoadContext = null;
            }
        }
    }

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