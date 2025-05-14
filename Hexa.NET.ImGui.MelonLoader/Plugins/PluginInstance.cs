namespace Hexa.NET.ImGui.MelonLoader.Plugins
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;

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

        public Exception FailedException { get; private set; }

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
}