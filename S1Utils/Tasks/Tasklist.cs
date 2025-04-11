namespace S1Utils.Tasks
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;

    public class Tasklist : IList<TaskItem>, IAddTask
    {
        private List<TaskItem> tasks = [];
        private readonly Dictionary<Guid, TaskItem> idToTask = [];
        private readonly string path;

        public Tasklist(string path)
        {
            this.path = path;
        }

        public int Count => tasks.Count;

        public bool IsReadOnly { get; }

        public TaskItem this[int index]
        {
            get => tasks[index];
            set
            {
                var old = tasks[index];
                old.Tasklist = null;
                RemoveTaskInternal(old);
                tasks[index] = value;
                value.Tasklist = this;
                AddTaskInternal(value);
            }
        }

        public void Add(TaskItem item)
        {
            item.Delete(false);
            tasks.Add(item);
            item.Tasklist = this;
            AddTaskInternal(item);
            Save();
        }

        public bool Remove(TaskItem item)
        {
            int index = IndexOf(item);
            if (index == -1) return false;
            RemoveAt(index);
            return true;
        }

        public void Move(TaskItem task, int index)
        {
            int oldIndex = tasks.IndexOf(task);
            if (oldIndex == -1) throw new ArgumentException("Item was not found in list.", nameof(task));
            tasks.RemoveAt(oldIndex);
            tasks.Insert(index, task);
            Save();
        }

        public void Insert(int index, TaskItem task)
        {
            task.Delete(false);
            tasks.Insert(index, task);
            task.Tasklist = this;
            AddTaskInternal(task);
            Save();
        }

        internal void AddTaskInternal(TaskItem item)
        {
            idToTask.Add(item.Id, item);
        }

        internal void RemoveTaskInternal(TaskItem item)
        {
            idToTask.Remove(item.Id);
        }

        public static Tasklist LoadFrom(string path)
        {
            Tasklist tasklist = new(path);
            if (File.Exists(path))
            {
                //try
                {
                    tasklist.tasks = JsonSerializer.Deserialize<List<TaskItem>>(File.ReadAllText(path)) ?? [];
                }
                //catch (Exception ex)
                {
                    // Console.WriteLine(ex);
                }

                foreach (var task in tasklist)
                {
                    task.Tasklist = tasklist;
                    task.BuildTree();
                }
            }

            return tasklist;
        }

        public void Save()
        {
            try
            {
                using var fs = File.Create(path);
                JsonSerializer.Serialize(fs, tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public TaskItem? Find(Guid id)
        {
            idToTask.TryGetValue(id, out var item);
            return item;
        }

        public bool TryGetTask(Guid id, [MaybeNullWhen(false)] out TaskItem? item)
        {
            return idToTask.TryGetValue(id, out item);
        }

        public int IndexOf(TaskItem item)
        {
            return tasks.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            var task = tasks[index];
            tasks.RemoveAt(index);
            task.Tasklist = null;
            RemoveTaskInternal(task);
            Save();
        }

        public void Clear()
        {
            tasks.Clear();
        }

        public bool Contains(TaskItem item)
        {
            return tasks.Contains(item);
        }

        public void CopyTo(TaskItem[] array, int arrayIndex)
        {
            tasks.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TaskItem> GetEnumerator()
        {
            return tasks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}