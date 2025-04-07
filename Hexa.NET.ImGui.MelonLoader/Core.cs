using Hexa.NET.ImGui.Utilities;
using MelonLoader;
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
                    .Build();
            });
        }

        public override void OnUpdate()
        {
            controller.UpdateInput();
            if (Input.GetKeyDown(KeyCode.Home))
            {
                show = !show;
            }
        }

        public override void OnGUI()
        {
            controller.NewFrame();

            if (show)
            {
                ImGui.ShowDemoWindow();
            }

            controller.Render();
        }
    }
}