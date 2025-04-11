namespace S1Utils.Windows
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.Utilities.Text;
    using S1Utils.Extensions;
    using S1Utils.Tasks;
    using System.Numerics;

    public class TasklistWindow : ImWindow
    {
        private readonly Tasklist tasks = Tasklist.LoadFrom("tasks.json");
        private TaskItem? selectedTask;
        private float split = 150;

        public override string Name { get; } = "Tasklist";

        public override unsafe void DrawContent()
        {
            byte* buffer = stackalloc byte[1024];
            StrBuilder builder = new(buffer, 1024);

            var avail = ImGui.GetContentRegionAvail();

            if (ImGui.Button("+"))
            {
                CreateTaskPopup popup = new(tasks);
                popup.Show();
            }

            ImGui.BeginChild("##Panel1"u8, new Vector2(split, avail.Y));

            for (int i = 0; i < tasks.Count; i++)
            {
                TaskItem task = tasks[i];
                bool done = task.Status == TaskItemStatus.Done;
                if (CheckboxSmall(task.IdString, ref done))
                {
                    task.SwitchState(done);
                }
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, GetColor(task.Status));
                if (ImGui.Selectable(task.FullTitle, selectedTask == task))
                {
                    selectedTask = task;
                }
                ImGui.PopStyleColor();

                HandleTaskContextMenu(task, ref builder);
            }

            ImGui.EndChild();

            ImGuiSplitter.VerticalSplitter("##Splitter1"u8, ref split, 0, avail.X);

            SubPanel(ref builder);
        }

        private unsafe void SubPanel(ref StrBuilder builder)
        {
            ImGui.BeginChild("##Panel2"u8);
            if (selectedTask != null)
            {
                var avail = ImGui.GetContentRegionAvail();
                var lineHeight = ImGui.GetTextLineHeightWithSpacing();
                var draw = ImGui.GetWindowDrawList();
                int index = 0;
                Display(selectedTask, index, ref builder, ref draw, avail, lineHeight);
            }
            ImGui.EndChild();
        }

        private static uint GetColor(TaskItemStatus status)
        {
            // ABGR
            return status switch
            {
                TaskItemStatus.Pending => 0xFF09E6ED,
                TaskItemStatus.Done => 0xFF09ED46,
                TaskItemStatus.Canceled => 0xFF0045F5,
                _ => 0xFFFFFFFF
            };
        }

        private unsafe void Display(TaskItem task, int index, ref StrBuilder builder, ref ImDrawListPtr draw, Vector2 avail, float lineHeight)
        {
            var dropRectMin = ImGui.GetCursorScreenPos();

            builder.Reset();
            builder.Append("       "u8);
            builder.Append(task.FullTitle);
            builder.End();
            var cur = ImGui.GetCursorPos();
            var style = ImGui.GetStyle();
            ImGui.SetCursorPos(cur + new Vector2(style.FrameBorderSize * 2 + ImGui.GetFontSize() + style.ItemInnerSpacing.X, 0));

            bool done = task.Status == TaskItemStatus.Done;

            if (CheckboxSmall(task.IdString, ref done))
            {
                task.SwitchState(done);
            }
            ImGui.SetCursorPos(cur);

            bool open = ImGui.TreeNodeEx(builder, ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.DefaultOpen);

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                RenameTaskPopup popup = new(task);
                popup.Show();
            }

            HandleTaskContextMenu(task, ref builder);
            HandleDragDropMove(task, index, avail, lineHeight, ref draw, dropRectMin);

            if (open)
            {
                for (int i = 0; i < task.SubTasks.Count; i++)
                {
                    Display(task.SubTasks[i], i, ref builder, ref draw, avail, lineHeight);
                }

                if (ImGui.SmallButton("+"u8))
                {
                    CreateTaskPopup popup = new(task);
                    popup.Show();
                }
                ImGui.PushID(builder.IconId('+', task.IdString, 1));
                HandleDragDropTree(task);
                ImGui.PopID();

                ImGui.TreePop();
            }
        }

        private static unsafe bool CheckboxSmall(string strId, ref bool isChecked)
        {
            var pos = ImGui.GetCursorScreenPos();
            var draw = ImGui.GetWindowDrawList();

            var id = ImGui.GetID(strId);

            var height = ImGui.GetTextLineHeight();
            ImRect bb = new(pos, pos + new Vector2(height, height));

            ImGuiP.ItemSize(bb);
            if (!ImGuiP.ItemAdd(bb, id, &bb))
            {
                return false;
            }

            bool hovered;
            bool held;
            bool result = ImGuiP.ButtonBehavior(bb, id, &hovered, &held);

            if (result)
            {
                isChecked = !isChecked;
            }

            uint color = ImGui.GetColorU32(held ? ImGuiCol.FrameBgActive : hovered ? ImGuiCol.FrameBgHovered : ImGuiCol.FrameBg);
            draw.AddRectFilled(pos, bb.Max, color);

            const float padding = 1;

            if (isChecked)
            {
                ImGuiP.RenderCheckMark(draw, pos + new Vector2(padding), ImGui.GetColorU32(ImGuiCol.CheckMark), height - padding * 2);
            }

            return result;
        }

        private unsafe void HandleTaskContextMenu(TaskItem task, ref StrBuilder builder)
        {
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem(builder.Icon(MaterialIcons.Delete, " Delete"u8)))
                {
                    task.Delete();
                    if (task == selectedTask) selectedTask = null;
                }
                if (ImGui.MenuItem(builder.Icon(MaterialIcons.Edit, " Rename"u8)))
                {
                    RenameTaskPopup popup = new(task);
                    popup.Show();
                }
                ImGui.EndPopup();
            }
        }

        private static unsafe void HandleDragDropTree(TaskItem item)
        {
            if (item.Tasklist == null) return;

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("TaskItemIndex"u8);

                if (!payload.IsNull && payload.DataSize == sizeof(TaskItemIndex))
                {
                    TaskItemIndex value = *(TaskItemIndex*)payload.Data;
                    if (value.Id == item.Id)
                    {
                        ImGui.EndDragDropTarget();
                        return;
                    }

                    var source = item.Tasklist.Find(value.Id);
                    if (source != null)
                    {
                        item.Add(source);
                    }
                }
                ImGui.EndDragDropTarget();
            }
        }

        private struct TaskItemIndex
        {
            public Guid Id;
            public int Index;

            public TaskItemIndex(Guid id, int index)
            {
                Id = id;
                Index = index;
            }
        }

        private static unsafe void HandleDragDropMove(TaskItem item, int index, Vector2 avail, float lineHeight, ref ImDrawListPtr draw, Vector2 dropRectMin)
        {
            if (item.Tasklist == null) return;
            if (ImGui.BeginDragDropSource())
            {
                TaskItemIndex value = new(item.Id, index);
                ImGui.SetDragDropPayload("TaskItemIndex"u8, &value, (nuint)sizeof(TaskItemIndex));
                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("TaskItemIndex"u8, ImGuiDragDropFlags.AcceptNoDrawDefaultRect | ImGuiDragDropFlags.AcceptBeforeDelivery);

                if (!payload.IsNull && payload.DataSize == sizeof(TaskItemIndex))
                {
                    TaskItemIndex value = *(TaskItemIndex*)payload.Data;
                    Vector2 dropRectMax = dropRectMin + new Vector2(avail.X, lineHeight);
                    if (value.Index > index)
                    {
                        dropRectMax.Y = dropRectMin.Y;
                    }
                    else
                    {
                        dropRectMin.Y = dropRectMax.Y;
                    }

                    draw.AddLine(dropRectMin, dropRectMax, ImGui.GetColorU32(ImGuiCol.DragDropTarget), 3.5f);

                    if (payload.IsDelivery())
                    {
                        var source = item.Tasklist.Find(value.Id);
                        if (source != null)
                        {
                            if (source.Parent == item.Parent)
                            {
                                if (item.Parent != null)
                                {
                                    item.Parent.Move(source, index);
                                }
                                else
                                {
                                    item.Tasklist.Move(source, index);
                                }
                            }
                            else
                            {
                                item.Insert(index, source);
                            }
                        }
                    }
                }

                ImGui.EndDragDropTarget();
            }
        }
    }
}