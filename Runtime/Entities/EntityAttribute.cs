using System;
using ECS.Components;
using ECS.Utilities;

namespace ECS.Entities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExcludeAttribute : Attribute
    {
        public readonly Type Type;

        public ExcludeAttribute(Type type)
        {
            Contract.IsImplementInterface(type, typeof(IComponent));
            Contract.IsValueType(type);
            Type = type;
        }
    }
}