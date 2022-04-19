﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ECS.Components;
using ECS.Utilities;

namespace ECS.Entities
{
    public abstract class SubWorld : IEnumerable<EntityId>
    {
        private readonly Dictionary<Type, TypeDescriptor> _descriptorCache;
        private readonly Stack<short> _poolIndex;
        private readonly Dictionary<Type, int> _typeIndex;
        private readonly EntitiesMap _entities;
        private readonly SharedComponentTable _sharedComponentTable;
        private ITableRawData[] _tablesList;
        private int _typeTableIndex = 0;
        private short _entityIndex;

        public World State { get; }

        protected SubWorld(World state)
        {
            State = state;
            _entityIndex = 0;
            _tablesList = new ITableRawData[8];
            _descriptorCache = new Dictionary<Type, TypeDescriptor>();
            _poolIndex = new Stack<short>(Constants.StartEntitiesCapacity);
            _typeIndex = new Dictionary<Type, int>();
            _entities = new EntitiesMap(this);
            _sharedComponentTable = new SharedComponentTable();
        }

        public SharedComponentTable Shared => _sharedComponentTable;

        public bool Contains(EntityId entityId) => _entities.Contains(entityId);

        protected ITable<TComponent> RegisterTable<TComponent>(TableHook<TComponent> hook = null)
            where TComponent : struct, IComponent
        {
            var table = new TableListImpl<TComponent>(this, _typeTableIndex, _entities, hook);
            _typeIndex.Add(typeof(TComponent), _typeTableIndex);

            if (_typeTableIndex == _tablesList.Length)
            {
                Array.Resize(ref _tablesList, _tablesList.Length << 1);
            }

            _tablesList[_typeTableIndex] = table;

            ++_typeTableIndex;

            return table;
        }

        public int Count => _entities.Count;

        public bool HasComponent<TComponent>(EntityId entityId) where TComponent : struct, IComponent
        {
            ITable table = _tablesList[GetTypeIndex(typeof(TComponent))];
            return table.Contains(entityId);
        }

        public void GetComponents(EntityId entityId, List<IComponent> components)
        {
            foreach (var data in _tablesList)
            {
                if (data == null) break;
                if (data.Contains(entityId))
                {
                    components.Add(data.GetComponent(entityId.Index));
                }
            }
        }

        public bool GetComponent<TComponent>(EntityId entityId, out TComponent component)
            where TComponent : struct, IComponent
        {
            ITable table = _tablesList[GetTypeIndex(typeof(TComponent))];
            if (table.Contains(entityId))
            {
                component = ((ITable<TComponent>)table)[entityId];
                return true;
            }

            component = default(TComponent);
            return false;
        }

        public void SetComponent<TComponent>(EntityId entityId, TComponent component)
            where TComponent : struct, IComponent
        {
            var table = (ITable<TComponent>)_tablesList[_typeIndex[typeof(TComponent)]];
            table.Set(entityId, component);
        }

        public void DeleteComponent<TComponent>(EntityId entityId) where TComponent : struct, IComponent
        {
            var table = (ITable<TComponent>)_tablesList[_typeIndex[typeof(TComponent)]];
            table.Delete(entityId);
        }

        public UnsafeDirectComponent<T> UnsafeGetDirectComponents<T>() where T : struct, IComponent
        {
            var index = GetTypeIndex<T>();
            var rawTable = _tablesList[index];
            var result = new UnsafeDirectComponent<T>
            {
                Components = (T[])rawTable.GetComponents(),
                Contains = rawTable.GetContains(),
                Ids = _entities.EntityIds,
                Count = rawTable.Count
            };
            return result;
        }

        public UnsafeDirectComponent<T> UnsafeGetDirectComponents<T>(int typeIndex) where T : struct, IComponent
        {
            var index = typeIndex;
            var rawTable = _tablesList[index];
            var result = new UnsafeDirectComponent<T>
            {
                Components = (T[])rawTable.GetComponents(),
                Contains = rawTable.GetContains(),
                Ids = _entities.EntityIds,
                Count = rawTable.Count
            };
            return result;
        }

        public int GetTypeIndex<TComponent>() => _typeIndex[typeof(TComponent)];

        public int GetTypeIndex(Type type) => _typeIndex[type];

        public EntityFilter<T> WhenAll<T>() where T : struct, IComponent
        {
            return new EntityFilter<T>(GetTypeDescriptor(typeof(EntityFilter<T>)), new EntityFilter
            {
                Manager = this
            }, State.StatePool);
        }

        public EntityFilter<T0, T1> WhenAll<T0, T1>() where T0 : struct, IComponent where T1 : struct, IComponent
        {
            return new EntityFilter<T0, T1>(GetTypeDescriptor(typeof(EntityFilter<T0, T1>)), new EntityFilter
            {
                Manager = this
            }, State.StatePool);
        }

        public EntityFilter<T0, T1, T2> WhenAll<T0, T1, T2>() where T0 : struct, IComponent
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            return new EntityFilter<T0, T1, T2>(GetTypeDescriptor(typeof(EntityFilter<T0, T1, T2>)), new EntityFilter
            {
                Manager = this
            }, State.StatePool);
        }

        public EntityFilter<T0, T1, T2, T3> WhenAll<T0, T1, T2, T3>() where T0 : struct, IComponent
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return new EntityFilter<T0, T1, T2, T3>(GetTypeDescriptor(typeof(EntityFilter<T0, T1, T2, T3>)),
                new EntityFilter
                {
                    Manager = this
                }, State.StatePool);
        }

        public EntityFilter<T0, T1, T2, T3, T4> WhenAll<T0, T1, T2, T3, T4>() where T0 : struct, IComponent
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return new EntityFilter<T0, T1, T2, T3, T4>(GetTypeDescriptor(typeof(EntityFilter<T0, T1, T2, T3, T4>)),
                new EntityFilter
                {
                    Manager = this
                }, State.StatePool);
        }

        public Matcher<TComponent> CreateMatcher<TComponent>() where TComponent : class, new()
        {
            TypeDescriptor typeDescriptor = GetTypeDescriptor(typeof(TComponent));

            var filter = new TComponent();
            var matcher = new Matcher<TComponent>(filter, typeDescriptor, new EntityFilter
            {
                Manager = this
            });
            return matcher;
        }

        public EntitiesMap.EntityIdEnumerator GetEnumerator() => _entities.GetEntitiesId();

        IEnumerator<EntityId> IEnumerable<EntityId>.GetEnumerator() => _entities.GetEntities();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private TypeDescriptor GetTypeDescriptor(Type type)
        {
            TypeDescriptor typeDescriptor;
            if (!_descriptorCache.TryGetValue(type, out typeDescriptor))
            {
                var descriptorList = new List<FieldDescriptor>();
                var includeList = State.StatePool.PopIntHashSet();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField |
                                            BindingFlags.SetField);
                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;
                    if (fieldType.IsArray)
                    {
                        var componentType = fieldType.GetElementType();
                        if (typeof(IComponent).IsAssignableFrom(componentType))
                        {
                            var componentIndex = _typeIndex[componentType];
                            includeList.Add(componentIndex);
                            var descriptor = new FieldDescriptor
                            {
                                SetFieldValue = field.SetValue,
                                ComponentIndex = componentIndex
                            };
                            descriptorList.Add(descriptor);
                        }
                        else
                        {
                            Contract.Throw("dot not support type: " + componentType);
                        }
                    }
                }

                var excludeList = State.StatePool.PopIntHashSet();
                var attributes = type.GetCustomAttributes(false);
                if (attributes.Length != 0)
                {
                    foreach (var attribute in attributes)
                    {
                        var excludeAttribute = attribute as ExcludeAttribute;
                        if (excludeAttribute != null)
                        {
                            excludeList.Add(_typeIndex[excludeAttribute.Type]);
                        }
                    }
                }

                _descriptorCache[type] = typeDescriptor = new TypeDescriptor
                {
                    Descriptors = descriptorList.ToArray(),
                    IncludeTypes = includeList.ToArray(),
                    ExcludeTypes = excludeList.ToArray()
                };

                State.StatePool.Return(includeList);
                State.StatePool.Return(excludeList);
            }

            return typeDescriptor;
        }

        private EntityId CreateEntityInternal(int id, short managerId)
        {
            var index = GetNextIndex();

            EntityId entityId = new EntityId(id, index, managerId);

            _entities.Add(entityId);

            var capacity = _entities.EntityIds.Length;
            foreach (var tableRawData in _tablesList)
            {
                if (tableRawData == null) break;
                tableRawData.ResizeTo(capacity);
            }

            return entityId;
        }

        private void DeleteEntityInternal(EntityId id)
        {
            foreach (var table in _tablesList)
            {
                if (table == null) break;
                table.Delete(id);
            }

            _entities.Remove(id);

            _poolIndex.Push(id.Index);
        }

        private short GetNextIndex()
        {
            var index = _entityIndex;
            if (_poolIndex.Count != 0)
            {
                index = _poolIndex.Pop();
            }
            else
            {
                ++_entityIndex;
            }

            return index;
        }

        public abstract class WorldBase
        {
            protected EntityId CreateEntity(int id, short managerId, SubWorld manager)
            {
                return manager.CreateEntityInternal(id, managerId);
            }

            protected void DeleteEntity(EntityId id, SubWorld manager)
            {
                manager.DeleteEntityInternal(id);
            }
        }

        public class EntitiesMap
        {
#if DEBUG
            private readonly object _owner;
#endif
            private const int DefaultSize = Constants.StartEntitiesCapacity;

            public int EntitiesCount;
            public int LastIndex;
            public EntityId[] EntityIds = new EntityId[DefaultSize];
            public bool[] HasEntities = new bool[DefaultSize];
            public int Version;

            public EntitiesMap(object owner)
            {
#if DEBUG
                _owner = owner;
#endif
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return EntitiesCount; }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(EntityId id)
            {
                if (id.Id == 0) return false;

                var index = id.Index;
                if (index >= EntityIds.Length) return false;
                unsafe
                {
                    fixed (EntityId* exist = &EntityIds[index])
                    {
                        if (exist->FullEquals(id)) return true;
                        return false;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Remove(EntityId id)
            {
                if (id.Id == 0) throw new ArgumentException("Entity id is not correct: " + id);
                if (id.Index >= EntityIds.Length) throw new ArgumentException("Entity is not found: " + id);

                int index = -1;

                unsafe
                {
                    index = id.Index;

                    fixed (EntityId* exist = &EntityIds[index])
                    {
                        if (!exist->FullEquals(id)) index = -1;
                    }
                }

                if (index == -1) throw new ArgumentException("Entity id not found: " + id);

                EntityIds[index] = EntityId.Empty;
                HasEntities[index] = false;
                if (LastIndex == index)
                {
                    LastIndex = LastIndex - 1 < 0 ? 0 : LastIndex - 1;
                }

                --EntitiesCount;
                ++Version;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(EntityId entity)
            {
                var index = entity.Index;

                if (index > LastIndex) LastIndex = index;
                if (index == EntityIds.Length)
                {
                    Array.Resize(ref EntityIds, EntityIds.Length << 1);
                    Array.Resize(ref HasEntities, EntityIds.Length);
                }

                HasEntities[index] = true;
                EntityIds[index] = entity;

                ++EntitiesCount;
                ++Version;
            }

            public IEnumerator<EntityId> GetEnumerator() => new Enumerator(this);

            public IEnumerator<EntityId> GetEntities() => new Enumerator(this);

            public EntityIdEnumerator GetEntitiesId() => new EntityIdEnumerator(this);

            public struct EntityIdEnumerator : IEnumerator<EntityId>
            {
                private EntitiesMap _entities;
                private int _index;
                private int _version;
                private int _count;

                public EntityIdEnumerator(EntitiesMap entities)
                {
                    _index = -1;
                    _count = 0;
                    _version = entities.Version;
                    _entities = entities;
                }

                public bool MoveNext()
                {
                    if (_version != _entities.Version) throw new InvalidOperationException();

                    do
                    {
                        ++_index;
                        if (_index > _entities.LastIndex || _count >= _entities.EntitiesCount) return false;
                    } while (!_entities.HasEntities[_index]);

                    ++_count;
                    return true;
                }

                public void Reset()
                {
                    _index = -1;
                    _count = 0;
                }

                public EntityId Current
                {
                    get
                    {
                        if (_version != _entities.Version) throw new InvalidOperationException();
                        return _entities.EntityIds[_index];
                    }
                }

                object IEnumerator.Current => Current;

                public void Dispose() => _entities = null;
            }

            private struct Enumerator : IEnumerator<EntityId>
            {
                private EntitiesMap _entities;
                private int _index;
                private int _version;
                private int _count;

                public Enumerator(EntitiesMap entities)
                {
                    _index = -1;
                    _count = 0;
                    _version = entities.Version;
                    _entities = entities;
                }

                public bool MoveNext()
                {
                    if (_version != _entities.Version) throw new InvalidOperationException();

                    do
                    {
                        ++_index;
                        if (_index > _entities.LastIndex || _count >= _entities.EntitiesCount) return false;
                    } while (!_entities.HasEntities[_index]);

                    ++_count;
                    return true;
                }

                public void Reset()
                {
                    _index = -1;
                    _count = 0;
                }

                public EntityId Current
                {
                    get
                    {
                        if (_version != _entities.Version) throw new InvalidOperationException();
                        return _entities.EntityIds[_index];
                    }
                }

                object IEnumerator.Current => Current;

                public void Dispose() => _entities = null;
            }
        }

        public struct EntityFilter
        {
            public SubWorld Manager;

            public bool[] HasEntities => Manager._entities.HasEntities;

            public EntityId[] Entities => Manager._entities.EntityIds;

            public int EntitiesCount => Manager._entities.Count;

            public int EntityLastIndex => Manager._entities.LastIndex;

            public ITableRawData[] Tables => Manager._tablesList;

            public int GetTypeIndex<T>() where T : struct, IComponent => Manager.GetTypeIndex<T>();
        }

        private class TableListImpl<T> : TableList<T> where T : struct, IComponent
        {
            public TableListImpl(SubWorld subWorld, int typeIndex, EntitiesMap entitiesMap, TableHook<T> hook) : base(
                subWorld, typeIndex, entitiesMap, hook)
            {
            }
        }
    }
}