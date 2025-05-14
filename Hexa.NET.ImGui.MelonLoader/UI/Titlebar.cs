namespace Hexa.NET.ImGui.MelonLoader.UI
{
    using Hexa.NET.ImGui;

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
}