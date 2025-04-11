namespace S1Utils
{
    using Hexa.NET.ImGui.MelonLoader;
    using Il2CppScheduleOne.PlayerScripts;
    using S1Utils.Windows;
    using System;

    public class S1UtilsPlugin : Plugin
    {
        private readonly ProductsWindow productsWindow = new();
        private readonly TasklistWindow tasklistWindow = new();

        public override string Name { get; } = "Schedule 1 Utils";

        public override void OnInitialized()
        {
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
            if (player == null)
            {
                Console.WriteLine("Couldn't get current property");
                return;
            }
            foreach (var employee in property.Employees.ToArray())
            {
                employee.AssignProperty(property);
            }
        }

        public override void OnUnload()
        {
            productsWindow.Close();
            tasklistWindow.Close();
            Console.WriteLine("Unloading");
        }
    }
}