using System;
using ECS.Components;

namespace ECS.Entities
{
    public struct EntityFilter<T> where T : struct, IComponent
    {
        public T[] Components;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _statePool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool statePool)
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _statePool = statePool;
            Components = (T[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[0]].GetComponents();
        }

        public void ForEach(Action<EntityId, T> action)
        {
            var entities = _statePool.PopEntityIds(this);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                action(entityId, Components[entityId.Index]);
            }

            _statePool.Return(entities);
        }
    }

    public struct EntityFilter<T0, T1> where T0 : struct, IComponent where T1 : struct, IComponent
    {
        public T0[] Components0;
        public T1[] Components1;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _statePool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool statePool) : this()
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _statePool = statePool;

            Components0 = (T0[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[0]].GetComponents();
            Components1 = (T1[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[1]].GetComponents();
        }

        public void ForEach(Action<EntityId, T0, T1> action)
        {
            var entities = _statePool.PopEntityIds(this);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                var index = entityId.Index;
                action(entityId, Components0[index], Components1[index]);
            }

            _statePool.Return(entities);
        }
    }

    public struct EntityFilter<T0, T1, T2> where T0 : struct, IComponent
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        public T0[] Components0;
        public T1[] Components1;
        public T2[] Components2;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _statePool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool statePool) : this()
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _statePool = statePool;

            Components0 = (T0[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[0]].GetComponents();
            Components1 = (T1[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[1]].GetComponents();
            Components2 = (T2[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[2]].GetComponents();
        }

        public void ForEach(Action<EntityId, T0, T1, T2> action)
        {
            var entities = _statePool.PopEntityIds(this);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                var index = entityId.Index;
                action(entityId, Components0[index], Components1[index], Components2[index]);
            }

            _statePool.Return(entities);
        }
    }

    public struct EntityFilter<T0, T1, T2, T3> where T0 : struct, IComponent
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        public T0[] Components0;
        public T1[] Components1;
        public T2[] Components2;
        public T3[] Components3;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _statePool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool statePool) : this()
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _statePool = statePool;

            Components0 = (T0[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[0]].GetComponents();
            Components1 = (T1[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[1]].GetComponents();
            Components2 = (T2[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[2]].GetComponents();
            Components3 = (T3[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[3]].GetComponents();
        }

        public void ForEach(Action<EntityId, T0, T1, T2, T3> action)
        {
            var entities = _statePool.PopEntityIds(this);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                var index = entityId.Index;
                action(entityId, Components0[index], Components1[index], Components2[index], Components3[index]);
            }

            _statePool.Return(entities);
        }
    }

    public struct EntityFilter<T0, T1, T2, T3, T4> where T0 : struct, IComponent
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        public T0[] Components0;
        public T1[] Components1;
        public T2[] Components2;
        public T3[] Components3;
        public T4[] Components4;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _statePool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool statePool) : this()
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _statePool = statePool;

            Components0 = (T0[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[0]].GetComponents();
            Components1 = (T1[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[1]].GetComponents();
            Components2 = (T2[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[2]].GetComponents();
            Components3 = (T3[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[3]].GetComponents();
            Components4 = (T4[])_entityFilter.Tables[_typeDescriptor.IncludeTypes[4]].GetComponents();
        }

        public void ForEach(Action<EntityId, T0, T1, T2, T3, T4> action)
        {
            var entities = _statePool.PopEntityIds(this);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                var index = entityId.Index;
                action(entityId, Components0[index], Components1[index], Components2[index], Components3[index],
                    Components4[index]);
            }

            _statePool.Return(entities);
        }
    }
}