﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ECS.Entities;

namespace ECS
{
    public abstract class World : SubWorld.WorldBase
    {
        private struct WorldWrapper
        {
            public short SubWorldId;
            public SubWorld SubWorld;
        }

        private readonly WorldPool _pool;
        private readonly Dictionary<Type, WorldWrapper> _map;
        private readonly SubWorld[] _subWorlds;
        private readonly SharedComponentTable _sharedComponentTable;
        private int _id;

        protected World()
        {
            _map = new Dictionary<Type, WorldWrapper>();
            _subWorlds = new SubWorld[Constants.EntitySubWorldCapacity];
            _pool = new WorldPool();
            _sharedComponentTable = new SharedComponentTable();
        }

        public SharedComponentTable Shared => _sharedComponentTable;

        public WorldPool Pool => _pool;

        public EntityId CreateEntity<T>() where T : SubWorld
        {
            var wrapper = GetSubWorldInternal<T>();

            var entity = CreateEntity(GenerateNextId(), wrapper.SubWorldId, wrapper.SubWorld);
            return entity;
        }

        public void DeleteEntity(EntityId id)
        {
            var subWorld = GetSubWorldByIdInternal(id);
#if DEBUG && !ENABLE_PROFILER
            ECS.Utilities.Contract.True(subWorld.Contains(id));
#endif
            DeleteEntity(id, subWorld);
        }

        public T GetSubWorldByGeneric<T>() where T : SubWorld
        {
            return (T)_map[typeof(T)].SubWorld;
        }

        public int GetSubWorldId<T>()
        {
            return _map[typeof(T)].SubWorldId;
        }

        public SubWorld GetSubWorld(EntityId entityId)
        {
            return GetSubWorldByIdInternal(entityId);
        }

        public bool Contains(EntityId entityId)
        {
            var subWorld = GetSubWorldByIdInternal(entityId);
            return subWorld.Contains(entityId);
        }

        protected TSubWorld RegisterSubWorld<TSubWorld>(TSubWorld subWorld) where TSubWorld : SubWorld
        {
            var wrapper = new WorldWrapper
            {
                SubWorldId = (short)_map.Count,
                SubWorld = subWorld
            };
            _map.Add(typeof(TSubWorld), wrapper);
            _subWorlds[wrapper.SubWorldId] = subWorld;
            return subWorld;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private WorldWrapper GetSubWorldInternal<T>() where T : SubWorld
        {
            return _map[typeof(T)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GenerateNextId()
        {
            return ++_id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SubWorld GetSubWorldByIdInternal(EntityId id)
        {
            return _subWorlds[id.SubWorldId];
        }
    }
}