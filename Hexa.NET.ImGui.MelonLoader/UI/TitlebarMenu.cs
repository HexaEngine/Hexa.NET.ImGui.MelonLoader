namespace Hexa.NET.ImGui.MelonLoader.UI
{
    public class TitlebarMenu : TitlebarItemCollection
    {
        private readonly string title;

        public TitlebarMenu(string title)
        {
            this.title = title;
        }

        public override void Draw()
        {
            if (ImGui.BeginMenu(title))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Draw();
                }
                ImGui.EndMenu();
            }
        }
    }
}