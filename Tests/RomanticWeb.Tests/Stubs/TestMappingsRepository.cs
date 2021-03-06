﻿using System;
using System.Collections.Generic;
using System.Linq;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Model;

namespace RomanticWeb.Tests.Stubs
{
    public class TestMappingsRepository : IMappingsRepository
    {
        private readonly List<IEntityMapping> _mappings;

        public TestMappingsRepository(params IEntityMapping[] entityMaps)
        {
            _mappings = entityMaps.ToList();
        }

        private List<IEntityMapping> Mappings
        {
            get
            {
                return _mappings;
            }
        }

        public void RebuildMappings(MappingContext mappingContext)
        {
        }

        public IEntityMapping MappingFor<TEntity>()
        {
            return MappingFor(typeof(TEntity));
        }

        public IEntityMapping MappingFor(Type entityType)
        {
            return (from mapping in Mappings
                    where mapping.EntityType == entityType
                    select mapping).SingleOrDefault();
        }

        public IPropertyMapping MappingForProperty(Uri predicateUri)
        {
            throw new NotImplementedException();
        }

        internal void Add(IEntityMapping mapping)
        {
            _mappings.Add(mapping);
        }
    }
}