namespace S1Utils
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.MelonLoader;
    using Hexa.NET.ImGui.MelonLoader.Plugins;
    using Il2CppScheduleOne.PlayerScripts;
    using Il2CppScheduleOne.UI;
    using S1Utils.Windows;
    using System;
    using System.Numerics;

    public sealed class S1UtilsPlugin : Plugin
    {
        private readonly ProductsWindow productsWindow = new();
        private readonly TasklistWindow tasklistWindow = new();

        public override string Name { get; } = "Schedule 1 Utils";

        public override void OnInitialized()
        {
            var style = ImGui.GetStyle();
            style.WindowRounding = 8;
            style.FrameRounding = 8;
            style.GrabRounding = 8;
            var colors = style.Colors;
            colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.06f, 0.06f, 0.06f, 0.65f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.43f, 0.43f, 0.50f, 0.50f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.20f, 0.48f, 0.16f, 0.54f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.27f, 0.98f, 0.26f, 0.40f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.19f, 0.57f, 0.17f, 0.67f);
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.04f, 0.04f, 0.04f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.15f, 0.38f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.49f, 0.82f, 0.16f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.32f, 0.88f, 0.24f, 1.00f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.32f, 0.96f, 0.16f, 1.00f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.20f, 0.82f, 0.16f, 0.40f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.30f, 0.69f, 0.26f, 1.00f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.47f, 0.88f, 0.15f, 1.00f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.29f, 0.98f, 0.26f, 0.31f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.33f, 0.98f, 0.26f, 0.80f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.26f, 0.98f, 0.27f, 1.00f);
            colors[(int)ImGuiCol.Separator] = new Vector4(0.43f, 0.50f, 0.43f, 0.50f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.10f, 0.75f, 0.13f, 0.78f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.18f, 0.75f, 0.10f, 1.00f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.27f, 0.98f, 0.26f, 0.20f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.26f, 0.98f, 0.31f, 0.67f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.98f, 0.27f, 0.95f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.26f, 0.98f, 0.36f, 0.80f);
            colors[(int)ImGuiCol.Tab] = new Vector4(0.20f, 0.58f, 0.18f, 0.86f);
            colors[(int)ImGuiCol.TabSelected] = new Vector4(0.20f, 0.68f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.TabSelectedOverline] = new Vector4(0.26f, 0.98f, 0.36f, 1.00f);
            colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.07f, 0.15f, 0.07f, 0.97f);
            colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.16f, 0.42f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.TabDimmedSelectedOverline] = new Vector4(0.50f, 0.50f, 0.50f, 0.00f);
            colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.26f, 0.98f, 0.27f, 0.70f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.23f, 0.23f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
            colors[(int)ImGuiCol.TextLink] = new Vector4(0.31f, 0.98f, 0.26f, 1.00f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            colors[(int)ImGuiCol.NavCursor] = new Vector4(0.31f, 0.98f, 0.26f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);

            Core.Titlebar.CreateMenu("S1 Utils")
                .AddMenuItem("Products", productsWindow.Show)
                .AddMenuItem("Tasklist", tasklistWindow.Show)
                .AddMenuItem("Reset Workers", ResetWorkers);
        }

        private void ResetWorkers()
        {
            Player player = UnityEngine.Object.FindObjectOfType<Player>();
            if (player == null)
            {
                Console.WriteLine("Couldn't get player");
                return;
            }

            player.RecalculateCurrentProperty();
            var property = player.CurrentProperty;
            if (property == null)
            {
                Console.WriteLine("Couldn't get current property");
                return;
            }
            var employees = property.Employees;
            if (employees == null)
            {
                Console.WriteLine("Couldn't get employees");
                return;
            }
            var idlePoints = property.EmployeeIdlePoints;
            for (int i = 0; i < employees.Count; i++)
            {
                employees[i].transform.position = idlePoints[Math.Min(i, idlePoints.Count)].position;
            }
        }

        public override void OnUnload()
        {
            productsWindow.Close();
            tasklistWindow.Close();
            Console.WriteLine("Unloading");
        }

        public override void OnInputSwitch(bool active)
        {
            if (active)
            {
                UnityEngine.Object.FindObjectOfType<PauseMenu>()?.Pause();
            }
            else
            {
                UnityEngine.Object.FindObjectOfType<PauseMenu>()?.Resume();
            }
        }
    }
}