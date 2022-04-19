namespace ECS.Entities
{
    public class Matcher
    {
        public static void Fill<T>(T filter, TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter)
            where T : class
        {
            var tables = entityFilter.Tables;
            var descriptorsLength = typeDescriptor.Descriptors.Length;
            var descriptors = typeDescriptor.Descriptors;
            for (var ti = 0; ti < descriptorsLength; ti++)
            {
                var descriptor = descriptors[ti];
                var array = tables[descriptor.ComponentIndex].GetComponents();
                descriptor.SetFieldValue(filter, array);
            }
        }

        public static void GetEntities(EntityList result, TypeDescriptor typeDescriptor,
            SubWorld.EntityFilter entityFilter)
        {
            var tables = entityFilter.Tables;

            var includeTypesLength = typeDescriptor.IncludeTypes.Length;
            var excludeTypesLength = typeDescriptor.ExcludeTypes.Length;
            var includeTypes = typeDescriptor.IncludeTypes;
            var excludeTypes = typeDescriptor.ExcludeTypes;

            var entityIterator = 0;
            var entityLastIndex = entityFilter.EntityLastIndex;
            var entitiesItems = entityFilter.EntitiesCount;
            var hasEntities = entityFilter.HasEntities;
            var entityIds = entityFilter.Entities;

            for (int i = 0; i <= entityLastIndex && entityIterator < entitiesItems; ++i)
            {
                if (!hasEntities[i]) continue;
                ++entityIterator;
                var entityId = entityIds[i];
                var entityIndex = entityId.Index;
                var ok = true;

                for (var ii = 0; ii < includeTypesLength; ++ii)
                {
                    var includeTypeIndex = includeTypes[ii];
                    var array = tables[includeTypeIndex].GetContains();
                    if (array.Length > entityIndex && !array[entityIndex])
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    for (var ie = 0; ie < excludeTypesLength; ++ie)
                    {
                        var excludeTypeIndex = excludeTypes[ie];
                        var array = tables[excludeTypeIndex].GetContains();
                        if (array.Length > entityIndex && tables[excludeTypeIndex].GetContains()[entityIndex])
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                    {
                        result.Add(entityId);
                    }
                }
            }
        }
    }

    public class Matcher<T> where T : class, new()
    {
        private readonly T _filter;
        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;

        public Matcher(T filter, TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter)
        {
            _filter = filter;
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
        }

        public T Fill()
        {
            Matcher.Fill(_filter, _typeDescriptor, _entityFilter);
            return _filter;
        }

        public T GetEntities(EntityList result)
        {
            Matcher.Fill(_filter, _typeDescriptor, _entityFilter);
            Matcher.GetEntities(result, _typeDescriptor, _entityFilter);

            return _filter;
        }
    }
}