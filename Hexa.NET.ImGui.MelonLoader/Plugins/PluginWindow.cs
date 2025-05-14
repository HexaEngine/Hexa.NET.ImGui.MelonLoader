using Hexa.NET.ImGui.MelonLoader.Plugins;
using Hexa.NET.ImGui.Widgets;
using Hexa.NET.Utilities.Text;

namespace Hexa.NET.ImGui.MelonLoader
{
    public class PluginWindow : ImWindow
    {
        private readonly PluginManager manager;
        private bool reloadingEnabled = false;

        public PluginWindow(PluginManager manager)
        {
            this.manager = manager;
        }

        public override string Name { get; } = "Plugins";

        public override unsafe void DrawContent()
        {
            ImGui.BeginDisabled(!reloadingEnabled);
            if (ImGui.Button("Reload All"u8))
            {
                manager.Unload();
                manager.LoadFromFolder(Core.PluginsFolder);
            }
            ImGui.EndDisabled();

            ImGui.SeparatorText("Plugins"u8);

            byte* buffer = stackalloc byte[1024];
            StrBuilder builder = new(buffer, 1024);
            foreach (var plugin in manager.Plugins)
            {
                builder.Reset();
                if (plugin.Loaded)
                {
                    builder.Append((byte)'L');
                }
                if (plugin.Failed)
                {
                    builder.Append((byte)'F');
                }
                builder.End();
                ImGui.Text(builder);
                ImGui.SameLine();
                ImGui.BeginDisabled(!reloadingEnabled);
                if (ImGui.Button(plugin.Name))
                {
                    plugin.Reload();
                }
                ImGui.EndDisabled();
                if (plugin.Failed)
                {
                    ImGui.TextColored(new(1, 0, 0, 1), plugin.FailedException.Message);
                }
            }
        }
    }
}