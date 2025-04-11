namespace S1Utils.Tasks
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Dialogs;

    public class CreateTaskPopup : Dialog
    {
        private readonly IAddTask targetList;
        private string title = "New Task";

        public CreateTaskPopup(IAddTask targetList)
        {
            this.targetList = targetList;
        }

        public override string Name { get; } = "Create Task";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.AlwaysAutoResize;

        protected override void DrawContent()
        {
            if (ImGui.InputText("Title"u8, ref title, 1024))
            {
            }
            if (ImGui.IsItemFocused() && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)))
            {
                CreateTask();
            }
            if (ImGui.Button("Cancel"u8))
            {
                Close(DialogResult.Cancel);
            }
            ImGui.SameLine();
            if (ImGui.Button("Create"u8))
            {
                CreateTask();
            }
        }

        private void CreateTask()
        {
            TaskItem item = new(title);
            targetList.Add(item);
            Close(DialogResult.Ok);
        }

        public override void Reset()
        {
        }
    }
}