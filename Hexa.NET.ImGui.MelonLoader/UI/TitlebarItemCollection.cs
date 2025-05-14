using System.Collections;

namespace Hexa.NET.ImGui.MelonLoader.UI
{
    public abstract class TitlebarItemCollection : TitlebarItem, IList<TitlebarItem>
    {
        protected readonly List<TitlebarItem> items = [];

        public TitlebarItem this[int index] { get => items[index]; set => items[index] = value; }

        public int Count => items.Count;

        public bool IsReadOnly { get; }

        public TitlebarMenuBuilder CreateMenu(string title)
        {
            TitlebarMenu menu = new(title);
            Add(menu);
            TitlebarMenuBuilder builder = new(menu);
            return builder;
        }

        public void Add(TitlebarItem item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(TitlebarItem item)
        {
            return items.Contains(item);
        }

        public void CopyTo(TitlebarItem[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(TitlebarItem item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, TitlebarItem item)
        {
            items.Insert(index, item);
        }

        public bool Remove(TitlebarItem item)
        {
            return items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public IEnumerator<TitlebarItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}