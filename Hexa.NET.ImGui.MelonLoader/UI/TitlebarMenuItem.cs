namespace Hexa.NET.ImGui.MelonLoader.UI
{
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