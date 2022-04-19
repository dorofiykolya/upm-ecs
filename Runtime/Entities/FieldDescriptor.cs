using System;

namespace ECS.Entities
{
    public struct FieldDescriptor
    {
        public int ComponentIndex;
        public Action<object, object> SetFieldValue;
    }
}