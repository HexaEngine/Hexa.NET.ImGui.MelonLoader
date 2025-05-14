namespace Hexa.NET.ImGui.MelonLoader.UI
{
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
}