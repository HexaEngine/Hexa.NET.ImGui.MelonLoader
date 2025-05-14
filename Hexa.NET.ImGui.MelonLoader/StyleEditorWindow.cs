namespace Hexa.NET.ImGui.MelonLoader
{
    using Hexa.NET.ImGui.Widgets;

    public class StyleEditorWindow : ImWindow
    {
        public override string Name { get; } = "Style Editor";

        public override void DrawContent()
        {
            ImGui.ShowStyleEditor();
        }
    }
}