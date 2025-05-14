using Hexa.NET.ImGui.MelonLoader.Plugins;
using Hexa.NET.ImGui.MelonLoader.UI;
using Hexa.NET.ImGui.Utilities;
using Hexa.NET.ImGui.Widgets;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Hexa.NET.ImGui.MelonLoader.Core), "Hexa.NET.ImGui.MelonLoader", "1.0.0", "Juna", null)]
[assembly: MelonGame("Juna", null)]

namespace Hexa.NET.ImGui.MelonLoader
{
    public sealed class Core : MelonMod
    {
        public const string PluginsFolder = "Mods/ImGuiPlugins";
        private ImGuiController controller;
        private PluginManager manager;
        private static bool show = false;
        private static bool processInput = false;

        public static bool Shown => show;

        public static bool BlockInput => processInput;

        public override unsafe void OnInitializeMelon()
        {
            InitController();

            manager = new();

            WidgetManager.Init();
            WidgetManager.FirstWindowIsMainWindow = false;

            Titlebar
                .AddMenuItem("Unpause", () => SwitchInput(false))
                .CreateMenu("Core")
                .AddMenuItem("Style Editor", () =>
                {
                    StyleEditorWindow window = new();
                    window.Show();
                })
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
            InitController();
            controller?.Reinitialize();
            InputManagerBlocker.ReloadInputActions();

            manager.DoAll((buildIndex, sceneName), (x, p) => p.OnSceneWasInitialized(x.buildIndex, x.sceneName));
        }

        private void InitController()
        {
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

                bool switchKeyPressed;
                processInput = false;
                try
                {
                    controller.UpdateInput();
                    switchKeyPressed = Input.GetKeyDown(KeyCode.End);
                }
                finally
                {
                    processInput = true;
                }
                HandleUISwitch(switchKeyPressed);
            }
            else
            {
                HandleUISwitch(Input.GetKeyDown(KeyCode.End));
            }
            manager.DoAll(p => p.OnUpdate());
        }

        private void HandleUISwitch(bool switchState)
        {
            if (switchState)
            {
                if (!processInput && show)
                {
                    SwitchInput(show);
                    return;
                }
                show = !show;
                SwitchInput(show);
            }
        }

        public override void OnLateUpdate()
        {
            manager.DoAll(p => p.OnLateUpdate());
        }

        private void SwitchInput(bool state)
        {
            processInput = state;
            InputManagerBlocker.SwitchInput(state);
            if (state)
            {
                cursorVisible = Cursor.visible;
                cursorLockMode = Cursor.lockState;
            }
            else
            {
                Cursor.visible = cursorVisible;
                Cursor.lockState = cursorLockMode;
            }

            manager.DoAll(state, (state, x) => x.OnInputSwitch(state));
        }

        public static readonly Titlebar Titlebar = [];

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
}