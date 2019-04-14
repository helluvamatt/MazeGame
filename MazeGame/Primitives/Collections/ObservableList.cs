using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MazeGame.Primitives.Collections
{
    internal class ObservableList<T> : Collection<T>
    {
        public ObservableList() : base() { }

        public ObservableList(IList<T> list) : base(list) { }

        public event EventHandler<ListChangedEventArgs<T>> ListChanged;
        public event EventHandler ListClearing;

        protected void OnListChanged(ListChangedEventArgs<T> args)
        {
            ListChanged?.Invoke(this, args);
        }

        protected override void ClearItems()
        {
            ListClearing?.Invoke(this, EventArgs.Empty);
            base.ClearItems();
            OnListChanged(new ListChangedEventArgs<T>(ListChangedType.Reset));
        }

        protected override void RemoveItem(int index)
        {
            if (index > -1 && index < Count)
            {
                var item = this[index];
                base.RemoveItem(index);
                OnListChanged(new ListChangedEventArgs<T>(ListChangedType.Remove, index, default(T), item));
            }
        }

        protected override void InsertItem(int index, T item)
        {
            if (index > -1 && index <= Count)
            {
                base.InsertItem(index, item);
                OnListChanged(new ListChangedEventArgs<T>(ListChangedType.Add, index, item, default(T)));
            }
        }

        protected override void SetItem(int index, T item)
        {
            if (index > -1 && index < Count)
            {
                var oldItem = this[index];
                base.SetItem(index, item);
                OnListChanged(new ListChangedEventArgs<T>(ListChangedType.Replace, index, item, oldItem));
            }
        }
    }

    internal class ListChangedEventArgs<T> : EventArgs
    {
        public ListChangedEventArgs(ListChangedType type)
        {
            if (type != ListChangedType.Reset) throw new ArgumentException("This constructor is only for Reset");
            Type = type;
            Index = -1;
        }

        public ListChangedEventArgs(ListChangedType type, int index, T newItem, T oldItem)
        {
            Type = type;
            Index = index;
            NewItem = newItem;
            OldItem = oldItem;
        }

        public ListChangedType Type { get; }
        public int Index { get; }
        public T NewItem { get; }
        public T OldItem { get; }
    }

    internal enum ListChangedType { Reset, Add, Remove, Replace }

}
