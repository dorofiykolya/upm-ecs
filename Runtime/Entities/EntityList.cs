using System;
using System.Collections.Generic;
using ECS.Utilities;

namespace ECS.Entities
{
    public class EntityList : RawList<EntityId>
    {
        public object Context;

        public EntityList() : base()
        {
        }

        public EntityList(int capacity) : base(capacity)
        {
        }

        public EntityList(IEnumerable<EntityId> collection) : base(collection)
        {
        }

        public override string ToString()
        {
            return $"({nameof(EntityList)} Count:{Count})";
        }
    }

    [Serializable]
    public class RawList<T> : IList<T>, System.Collections.IList
    {
        private const int MaxArrayLength = 0X7FEFFFFF;
        private const int _defaultCapacity = 4;
        private static readonly T[] _emptyArray = new T[0];

        public T[] Items;
        private int _size;
        private int _version;

        public RawList()
        {
            Items = _emptyArray;
        }

        public RawList(int capacity)
        {
            if (capacity < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

            if (capacity == 0)
                Items = _emptyArray;
            else
                Items = new T[capacity];
        }

        public RawList(IEnumerable<T> collection)
        {
            if (collection == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);

            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                int count = c.Count;
                if (count == 0)
                {
                    Items = _emptyArray;
                }
                else
                {
                    Items = new T[count];
                    c.CopyTo(Items, 0);
                    _size = count;
                }
            }
            else
            {
                _size = 0;
                Items = _emptyArray;

                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Add(en.Current);
                    }
                }
            }
        }

        public int Capacity
        {
            get { return Items.Length; }
            set
            {
                if (value < _size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value,
                        ExceptionResource.ArgumentOutOfRange_SmallCapacity);
                }

                if (value != Items.Length)
                {
                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (_size > 0)
                        {
                            Array.Copy(Items, 0, newItems, 0, _size);
                        }

                        Items = newItems;
                    }
                    else
                    {
                        Items = _emptyArray;
                    }
                }
            }
        }

        public int Count => _size;

        bool ICollection<T>.IsReadOnly => false;

        bool System.Collections.IList.IsReadOnly => false;

        bool System.Collections.ICollection.IsSynchronized => false;

        public object SyncRoot => ThrowHelper.ThrowNotImplemented();

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                return Items[index];
            }

            set
            {
                if ((uint)index >= (uint)_size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                Items[index] = value;
                _version++;
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            return ((value is T) || (value == null && default(T) == null));
        }

        Object System.Collections.IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);

                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
                }
            }
        }

        public void Add(T item)
        {
            if (_size == Items.Length) EnsureCapacity(_size + 1);
            Items[_size++] = item;
            _version++;
        }

        int System.Collections.IList.Add(Object item)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);

            try
            {
                Add((T)item);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
            }

            return Count - 1;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(_size, collection);
        }

        public void Clear()
        {
            if (_size > 0)
            {
                Array.Clear(Items, 0,
                    _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
                _size = 0;
            }

            _version++;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < _size; i++)
                    if ((object)Items[i] == null)
                        return true;
                return false;
            }
            else
            {
                EqualityComparer<T> c = EqualityComparer<T>.Default;
                for (int i = 0; i < _size; i++)
                {
                    if (c.Equals(Items[i], item)) return true;
                }

                return false;
            }
        }

        bool System.Collections.IList.Contains(Object item)
        {
            if (IsCompatibleObject(item))
            {
                return Contains((T)item);
            }

            return false;
        }

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        void System.Collections.ICollection.CopyTo(Array array, int arrayIndex)
        {
            if ((array != null) && (array.Rank != 1))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
            }

            try
            {
                Array.Copy(Items, 0, array, arrayIndex, _size);
            }
            catch (ArrayTypeMismatchException)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
            }
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            if (_size - index < count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }

            Array.Copy(Items, index, array, arrayIndex, count);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(Items, 0, array, arrayIndex, _size);
        }

        private void EnsureCapacity(int min)
        {
            if (Items.Length < min)
            {
                int newCapacity = Items.Length == 0 ? _defaultCapacity : Items.Length * 2;

                if ((uint)newCapacity > MaxArrayLength) newCapacity = MaxArrayLength;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }

        public T Find(Predicate<T> match)
        {
            if (match == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            for (int i = 0; i < _size; i++)
            {
                if (match(Items[i]))
                {
                    return Items[i];
                }
            }

            return default(T);
        }

        public void ForEach(Action<T> action)
        {
            if (action == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            }

            int version = _version;

            for (int i = 0; i < _size; i++)
            {
                if (version != _version)
                {
                    break;
                }

                action(Items[i]);
            }

            if (version != _version)
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(Items, item, 0, _size);
        }

        int System.Collections.IList.IndexOf(Object item)
        {
            if (IsCompatibleObject(item))
            {
                return IndexOf((T)item);
            }

            return -1;
        }

        public int IndexOf(T item, int index)
        {
            if (index > _size)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index,
                    ExceptionResource.ArgumentOutOfRange_Index);

            return Array.IndexOf(Items, item, index, _size - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            if (index > _size)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index,
                    ExceptionResource.ArgumentOutOfRange_Index);

            if (count < 0 || index > _size - count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
                    ExceptionResource.ArgumentOutOfRange_Count);

            return Array.IndexOf(Items, item, index, count);
        }

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index,
                    ExceptionResource.ArgumentOutOfRange_ListInsert);
            }

            if (_size == Items.Length) EnsureCapacity(_size + 1);
            if (index < _size)
            {
                Array.Copy(Items, index, Items, index + 1, _size - index);
            }

            Items[index] = item;
            _size++;
            _version++;
        }

        void System.Collections.IList.Insert(int index, Object item)
        {
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(item, ExceptionArgument.item);

            try
            {
                Insert(index, (T)item);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongValueTypeArgumentException(item, typeof(T));
            }
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            }

            if ((uint)index > (uint)_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index,
                    ExceptionResource.ArgumentOutOfRange_Index);
            }

            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                // if collection is ICollection<T>
                int count = c.Count;
                if (count > 0)
                {
                    EnsureCapacity(_size + count);
                    if (index < _size)
                    {
                        Array.Copy(Items, index, Items, index + count, _size - index);
                    }

                    // If we're inserting a List into itself, we want to be able to deal with that.
                    if (this == c)
                    {
                        // Copy first part of _items to insert location
                        Array.Copy(Items, 0, Items, index, index);
                        // Copy last part of _items back to inserted location
                        Array.Copy(Items, index + count, Items, index * 2, _size - index);
                    }
                    else
                    {
                        T[] itemsToInsert = new T[count];
                        c.CopyTo(itemsToInsert, 0);
                        itemsToInsert.CopyTo(Items, index);
                    }

                    _size += count;
                }
            }
            else
            {
                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Insert(index++, en.Current);
                    }
                }
            }

            _version++;
        }

        public int LastIndexOf(T item)
        {
            if (_size == 0)
            {
                // Special case for empty list
                return -1;
            }
            else
            {
                return LastIndexOf(item, _size - 1, _size);
            }
        }

        public int LastIndexOf(T item, int index)
        {
            if (index >= _size)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index,
                    ExceptionResource.ArgumentOutOfRange_Index);

            return LastIndexOf(item, index, index + 1);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            if ((Count != 0) && (index < 0))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if ((Count != 0) && (count < 0))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (_size == 0)
            {
                // Special case for empty list
                return -1;
            }

            if (index >= _size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index,
                    ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
            }

            if (count > index + 1)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count,
                    ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
            }

            return Array.LastIndexOf(Items, item, index, count);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        void System.Collections.IList.Remove(Object item)
        {
            if (IsCompatibleObject(item))
            {
                Remove((T)item);
            }
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException();
            }

            _size--;
            if (index < _size)
            {
                Array.Copy(Items, index + 1, Items, index, _size - index);
            }

            Items[_size] = default(T);
            _version++;
        }

        public bool IsFixedSize => false;

        public T[] ToArray()
        {
            T[] array = new T[_size];
            Array.Copy(Items, 0, array, 0, _size);
            return array;
        }

        [Serializable]
        public struct Enumerator : IEnumerator<T>
        {
            private RawList<T> list;
            private int index;
            private int version;
            private T current;

            internal Enumerator(RawList<T> list)
            {
                this.list = list;
                index = 0;
                version = list._version;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                RawList<T> localList = list;

                if (version == localList._version && ((uint)index < (uint)localList._size))
                {
                    current = localList.Items[index];
                    index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (version != list._version)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }

                index = list._size + 1;
                current = default(T);
                return false;
            }

            public T Current => current;

            Object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == list._size + 1)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }

                    return Current;
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                if (version != list._version)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }

                index = 0;
                current = default(T);
            }
        }
    }
}