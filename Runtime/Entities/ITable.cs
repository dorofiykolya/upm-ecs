﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ECS.Components;

namespace ECS.Entities
{
    public struct ComponentIndex<T> where T : struct, IComponent
    {
        public static implicit operator T(ComponentIndex<T> component)
        {
            return component.Component;
        }

        public EntityId Id;
        public T Component;
    }

    public interface ITable
    {
        bool Contains(EntityId entity);
        void Delete(EntityId entity);
        void DeleteAll();

        int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        void GetEntities(EntityList entities);
    }

    public interface ITable<T> : ITable, IEnumerable<ComponentIndex<T>> where T : struct, IComponent
    {
        T this[EntityId entityId] { get; set; }
        ref T this[short index] { get; }
        ref T GetRef(EntityId entityId);
        void Set(EntityId entityId, T component);
        bool TryGet(EntityId entityId, out T component);
        TableList<T>.Enumerator GetEnumerator();

        T[] RawComponents
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        bool[] RawContains
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }
    }

    public interface ITableRawData : ITable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Array GetComponents();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool[] GetContains();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ResizeTo(int newCapacity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IComponent GetComponent(int index);
    }

    public enum TableType
    {
        Array,
        Map
    }
}