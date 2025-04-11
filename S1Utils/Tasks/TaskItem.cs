namespace S1Utils.Tasks
{
    using System.Text.Json.Serialization;

    public class TaskItem : IAddTask
    {
        private string? fullTitleCache;
        private string? idCache;
        private Guid id;
        private string title;
        private TaskItemStatus status;
        private readonly List<TaskItem> subTasks;

        public Guid Id
        {
            get => id; set
            {
                id = value;
                fullTitleCache = null;
                idCache = null;
            }
        }

        public string Title
        {
            get => title;
            set
            {
                title = value;
                fullTitleCache = null;
            }
        }

        [JsonIgnore]
        public string FullTitle => fullTitleCache ??= $"{Title}##{Id}";

        [JsonIgnore]
        public string IdString => idCache ??= $"##{id}";

        public TaskItemStatus Status { get => status; set => status = value; }

        [JsonIgnore]
        public TaskItem? Parent { get; set; }

        public IReadOnlyList<TaskItem> SubTasks => subTasks;

        [JsonIgnore]
        public Tasklist? Tasklist { get; internal set; }

        [JsonIgnore]
        public int Count => subTasks.Count;

        [JsonIgnore]
        public bool IsReadOnly { get; }

        public TaskItem this[int index]
        {
            get => subTasks[index];
            set
            {
                var old = subTasks[index];
                old.Parent = null;
                old.Tasklist = null;
                Tasklist?.RemoveTaskInternal(old);
                subTasks[index] = value;
                value.Tasklist = Tasklist;
                value.Parent = this;
                Tasklist?.AddTaskInternal(value);
                Save();
            }
        }

        public TaskItem(string title)
        {
            id = Guid.NewGuid();
            this.title = title;
            subTasks = [];
        }

        [JsonConstructor]
        public TaskItem(Guid id, string title, TaskItemStatus status, IReadOnlyList<TaskItem> subTasks)
        {
            this.id = id;
            this.title = title;
            this.status = status;
            this.subTasks = [.. subTasks];
        }

        public void Add(TaskItem item)
        {
            item.Delete(false);
            subTasks.Add(item);
            item.Parent = this;
            item.Tasklist = Tasklist;
            Tasklist?.AddTaskInternal(item);
            Save();
        }

        public void Insert(int index, TaskItem item)
        {
            item.Delete(false);
            subTasks.Insert(index, item);
            item.Parent = this;
            item.Tasklist = Tasklist;
            Tasklist?.AddTaskInternal(item);
            Save();
        }

        public bool Remove(TaskItem item)
        {
            int index = IndexOf(item);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }

        public void UpdateStatusBasedOnSubtasks()
        {
            if (SubTasks.Count > 0 && SubTasks.All(t => t.Status == TaskItemStatus.Done))
            {
                Status = TaskItemStatus.Done;
                Parent?.UpdateStatusBasedOnSubtasks();
            }
        }

        internal void BuildTree()
        {
            Tasklist!.AddTaskInternal(this);
            foreach (var subTask in subTasks)
            {
                subTask.Parent = this;
                subTask.Tasklist = Tasklist;
                subTask.BuildTree();
            }
        }

        public void Save()
        {
            Tasklist?.Save();
        }

        public void SwitchState(bool done)
        {
            Status = done ? TaskItemStatus.Done : TaskItemStatus.Pending;
            Parent?.UpdateStatusBasedOnSubtasks();
            Save();
        }

        public void Delete(bool save = true)
        {
            if (Parent != null)
            {
                Parent.Remove(this);
            }
            else
            {
                Tasklist?.Remove(this);
            }

            if (save)
            {
                Save();
            }
        }

        public void Move(TaskItem task, int index)
        {
            int oldIndex = subTasks.IndexOf(task);
            if (oldIndex == -1) throw new ArgumentException("Item was not found in list.", nameof(task));
            subTasks.RemoveAt(oldIndex);

            subTasks.Insert(index, task);

            Save();
        }

        public int IndexOf(TaskItem item)
        {
            return subTasks.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            var task = subTasks[index];
            task.Parent = null;
            task.Tasklist = null;
            subTasks.RemoveAt(index);
            Tasklist?.RemoveTaskInternal(task);
        }

        public void Clear()
        {
            subTasks.Clear();
        }

        public bool Contains(TaskItem item)
        {
            return subTasks.Contains(item);
        }

        public void CopyTo(TaskItem[] array, int arrayIndex)
        {
            subTasks.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TaskItem> GetEnumerator()
        {
            return subTasks.GetEnumerator();
        }
    }
}