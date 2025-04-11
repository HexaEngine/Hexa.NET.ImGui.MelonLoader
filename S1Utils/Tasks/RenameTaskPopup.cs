namespace S1Utils.Tasks
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;

    public class RenameTaskPopup : Dialog
    {
        private readonly TaskItem task;
        private string title;

        public RenameTaskPopup(TaskItem task)
        {
            this.task = task;
            title = task.Title;
        }

        public override string Name { get; } = "Rename";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.AlwaysAutoResize;

        protected override void DrawContent()
        {
            if (ImGui.InputText("Title"u8, ref title, 1024))
            {
            }
            if (ImGui.IsItemFocused() && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)))
            {
                RenameTask();
            }
            if (ImGui.Button("Cancel"u8))
            {
                Close(DialogResult.Cancel);
            }
            ImGui.SameLine();
            if (ImGui.Button("Rename"u8))
            {
                RenameTask();
            }
        }

        private void RenameTask()
        {
            task.Title = title;
            task.Save();
            Close(DialogResult.Ok);
        }
    }
}