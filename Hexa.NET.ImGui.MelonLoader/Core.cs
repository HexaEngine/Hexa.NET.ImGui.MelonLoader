using Hexa.NET.ImGui.Utilities;
using Hexa.NET.ImGui.Widgets;
using Hexa.NET.ImGui.Widgets.Dialogs;
using MelonLoader;
using System.Runtime.InteropServices;
using UnityEngine;

[assembly: MelonInfo(typeof(Hexa.NET.ImGui.MelonLoader.Core), "Hexa.NET.ImGui.MelonLoader", "1.0.0", "Juna", null)]
[assembly: MelonGame("Juna", null)]

namespace Hexa.NET.ImGui.MelonLoader
{
    public class Core : MelonMod
    {
        private ImGuiController controller;
        private bool show = false;

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

            WidgetManager.Init();
            WidgetManager.FirstWindowIsMainWindow = false;
            MainWindow window = new();
            window.Show();
        }

        public override void OnDeinitializeMelon()
        {
            WidgetManager.Dispose();
            controller.Dispose();
        }

        public override void OnUpdate()
        {
            controller.UpdateInput();
            if (Input.GetKeyDown(KeyCode.End))
            {
                show = !show;
            }
        }

        public override void OnGUI()
        {
            controller.NewFrame();

            if (show)
            {
                WidgetManager.Draw();
            }

            controller.EndFrame();
        }
    }

    public class MainWindow : ImWindow
    {
        public override string Name { get; } = "MainWindow";

        public override void DrawContent()
        {
            if (ImGui.Button("Open file dialog"))
            {
                OpenFileDialog dialog = new();
                dialog.Show();
            }
        }
    }
}