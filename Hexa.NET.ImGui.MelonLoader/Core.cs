using Hexa.NET.ImGui.Utilities;
using Hexa.NET.ImGui.Widgets;
using Il2CppScheduleOne.UI;
using MelonLoader;
using System.Collections;
using UnityEngine;

[assembly: MelonInfo(typeof(Hexa.NET.ImGui.MelonLoader.Core), "Hexa.NET.ImGui.MelonLoader", "1.0.0", "Juna", null)]
[assembly: MelonGame("Juna", null)]

namespace Hexa.NET.ImGui.MelonLoader
{
    public class Core : MelonMod
    {
        public const string PluginsFolder = "Mods/ImGuiPlugins";
        private ImGuiController controller;
        private PluginManager manager;
        private static bool show = false;
        private static bool processInput = false;

        public static bool Shown => show;

        public override unsafe void OnInitializeMelon()
        {
            controller = new(io =>
            {
                io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.DockingEnable;
                ImGuiFontBuilder builder = new();
                builder
                    .AddFontFromFileTTF("Mods/arialuni.ttf", 16, [0x1, 0x1FFFF])
                    .SetOption(config =>
                    {
                        config.GlyphMinAdvanceX = 18;
                        config.GlyphOffset = new(0, 4);
                        config.MergeMode = true;
                    })
                    .AddFontFromFileTTF("Mods/MaterialSymbolsRounded.ttf", 16.0f, [0xe003, 0xF8FF])
                    .Build();
            });

            manager = new();

            WidgetManager.Init();
            WidgetManager.FirstWindowIsMainWindow = false;

            Titlebar
                .AddMenuItem("Unpause", () => SwitchInput(false))
                .AddMenuItem("Plugins", () =>
                {
                    PluginWindow pluginWindow = new(manager);
                    pluginWindow.Show();
                });

            manager.LoadFromFolder(PluginsFolder);
        }

        public override void OnDeinitializeMelon()
        {
            manager.Unload();
            WidgetManager.Dispose();
            controller.Dispose();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            controller ??= new(io =>
            {
                io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.DockingEnable;
                ImGuiFontBuilder builder = new();
                builder
                    .AddFontFromFileTTF("Mods/arialuni.ttf", 16, [0x1, 0x1FFFF])
                    .SetOption(config =>
                    {
                        config.GlyphMinAdvanceX = 18;
                        config.GlyphOffset = new(0, 4);
                        config.MergeMode = true;
                    })
                    .AddFontFromFileTTF("Mods/MaterialSymbolsRounded.ttf", 16.0f, [0xe003, 0xF8FF])
                    .Build();
            });
            controller?.Reinitialize();

            manager.DoAll((buildIndex, sceneName), (x, p) => p.OnSceneWasInitialized(x.buildIndex, x.sceneName));
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            manager.DoAll((buildIndex, sceneName), (x, p) => p.OnSceneWasLoaded(x.buildIndex, x.sceneName));
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            manager.DoAll((buildIndex, sceneName), (x, p) => p.OnSceneWasUnloaded(x.buildIndex, x.sceneName));
        }

        public override void OnFixedUpdate()
        {
            manager.DoAll(p => p.OnFixedUpdate());
        }

        private bool cursorVisible;
        private CursorLockMode cursorLockMode;

        public override void OnUpdate()
        {
            if (processInput)
            {
                if (!Cursor.visible)
                {
                    cursorVisible = Cursor.visible;
                    Cursor.visible = true;
                }

                if (Cursor.lockState != CursorLockMode.None)
                {
                    cursorLockMode = Cursor.lockState;
                    Cursor.lockState = CursorLockMode.None;
                }

                controller.UpdateInput();
            }
            if (Input.GetKeyDown(KeyCode.End))
            {
                if (!processInput && show)
                {
                    SwitchInput(show);
                    return;
                }
                show = !show;
                SwitchInput(show);
            }
            manager.DoAll(p => p.OnUpdate());
        }

        public override void OnLateUpdate()
        {
            manager.DoAll(p => p.OnLateUpdate());
        }

        private void SwitchInput(bool state)
        {
            processInput = state;
            if (state)
            {
                UnityEngine.Object.FindObjectOfType<PauseMenu>()?.Pause();
            }
            else
            {
                Cursor.visible = cursorVisible;
                Cursor.lockState = cursorLockMode;
                UnityEngine.Object.FindObjectOfType<PauseMenu>()?.Resume();
            }
        }

        public static readonly Titlebar Titlebar = new();

        public override void OnGUI()
        {
            if (show)
            {
                controller.NewFrame();

                try
                {
                    if (processInput)
                    {
                        Titlebar.Draw();
                    }
                    WidgetManager.Draw();
                    manager.DoAll(p => p.OnGUI());
                }
                finally
                {
                    controller.EndFrame();
                }
            }
        }
    }

    public class Titlebar : TitlebarItemCollection
    {
        public Titlebar AddMenuItem(string title, Action callback)
        {
            TitlebarMenuItem item = new(title, callback);
            Add(item);
            return this;
        }

        public override void Draw()
        {
            if (ImGui.BeginMainMenuBar())
            {
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Draw();
                }
                ImGui.EndMainMenuBar();
            }
        }
    }

    public abstract class TitlebarItem
    {
        public abstract void Draw();
    }

    public class TitlebarMenuBuilder
    {
        private readonly TitlebarItemCollection parent;

        public TitlebarMenuBuilder(TitlebarItemCollection parent)
        {
            this.parent = parent;
        }

        public TitlebarMenuBuilder AddMenu(string title)
        {
            TitlebarMenu menu = new(title);
            parent.Add(menu);
            return new(menu);
        }

        public TitlebarMenuBuilder AddMenuItem(string title, Action callback)
        {
            TitlebarMenuItem item = new(title, callback);
            parent.Add(item);
            return this;
        }
    }

    public abstract class TitlebarItemCollection : TitlebarItem, IList<TitlebarItem>
    {
        protected readonly List<TitlebarItem> items = [];

        public TitlebarItem this[int index] { get => items[index]; set => items[index] = value; }

        public int Count => items.Count;

        public bool IsReadOnly { get; }

        public TitlebarMenuBuilder CreateMenu(string title)
        {
            TitlebarMenu menu = new(title);
            Add(menu);
            TitlebarMenuBuilder builder = new(menu);
            return builder;
        }

        public void Add(TitlebarItem item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(TitlebarItem item)
        {
            return items.Contains(item);
        }

        public void CopyTo(TitlebarItem[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(TitlebarItem item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, TitlebarItem item)
        {
            items.Insert(index, item);
        }

        public bool Remove(TitlebarItem item)
        {
            return items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public IEnumerator<TitlebarItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class TitlebarMenu : TitlebarItemCollection
    {
        private readonly string title;

        public TitlebarMenu(string title)
        {
            this.title = title;
        }

        public override void Draw()
        {
            if (ImGui.BeginMenu(title))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Draw();
                }
                ImGui.EndMenu();
            }
        }
    }

    public class TitlebarMenuItem : TitlebarItem
    {
        private readonly string title;
        private Action callback;

        public TitlebarMenuItem(string title, Action callback)
        {
            this.title = title;
            this.callback = callback;
        }

        public override void Draw()
        {
            if (ImGui.MenuItem(title))
            {
                callback();
            }
        }
    }
}