﻿using System;
using System.Linq;
using RomanticWeb.Converters;
using RomanticWeb.Mapping.Providers;

namespace RomanticWeb.Mapping.Model
{
    internal class MappingModelBuilder
    {
        private readonly MappingContext _mappingContext;

        private Type _currentType;

        public MappingModelBuilder(MappingContext mappingContext)
        {
            _mappingContext = mappingContext;
        }

        public IEntityMapping BuildMapping(IEntityMappingProvider mapping)
        {
            _currentType = mapping.EntityType;
            var classes = mapping.Classes.Select(BuildMapping);
            var properties = mapping.Properties.Select(BuildMapping);

            return new EntityMapping(mapping.EntityType, classes, properties);
        }

        private PropertyMapping BuildMapping(IPropertyMappingProvider mapping)
        {
            if (mapping is IDictionaryMappingProvider)
            {
                return BuildDictionaryMapping((IDictionaryMappingProvider)mapping);
            }

            if (mapping is ICollectionMappingProvider)
            {
                return BuildCollectionMapping((ICollectionMappingProvider)mapping);
            }

            return BuildPropertyMapping(mapping);
        }

        private ClassMapping BuildMapping(IClassMappingProvider mapping)
        {
            return new ClassMapping(mapping.GetTerm(_mappingContext.OntologyProvider), mapping.DeclaringEntityType != _currentType);
        }

        private PropertyMapping BuildPropertyMapping(IPropertyMappingProvider provider)
        {
            var propertyMapping = new PropertyMapping(
                provider.PropertyInfo.DeclaringType,
                provider.PropertyInfo.PropertyType,
                provider.PropertyInfo.Name,
                provider.GetTerm(_mappingContext.OntologyProvider));
            SetConverter(propertyMapping, provider);
            return propertyMapping;
        }

        private PropertyMapping BuildDictionaryMapping(IDictionaryMappingProvider provider)
        {
            var dictionaryMapping = new DictionaryMapping(
                provider.PropertyInfo.DeclaringType,
                provider.PropertyInfo.PropertyType,
                provider.PropertyInfo.Name,
                provider.GetTerm(_mappingContext.OntologyProvider),
                provider.Key.GetTerm(_mappingContext.OntologyProvider),
                provider.Value.GetTerm(_mappingContext.OntologyProvider));
            SetConverter(dictionaryMapping, provider);
            return dictionaryMapping;
        }

        private CollectionMapping BuildCollectionMapping(ICollectionMappingProvider provider)
        {
            var collectionMapping = new CollectionMapping(
                provider.PropertyInfo.DeclaringType,
                provider.PropertyInfo.PropertyType,
                provider.PropertyInfo.Name,
                provider.GetTerm(_mappingContext.OntologyProvider),
                provider.StoreAs);
            bool converterSet = SetConverter(collectionMapping, provider);
            if ((provider.ElementConverterType != null) && (!provider.ElementConverterType.ContainsGenericParameters))
            {
                collectionMapping.ElementConverter = (INodeConverter)Activator.CreateInstance(provider.ElementConverterType);
            }
            else if (converterSet)
            {
                collectionMapping.ElementConverter = collectionMapping.Converter;
            }

            return collectionMapping;
        }

        private bool SetConverter(PropertyMapping propertyMapping, IPropertyMappingProvider provider)
        {
            bool result = false;
            if ((provider.ConverterType != null) && (!provider.ConverterType.ContainsGenericParameters) && (!provider.ConverterType.IsInterface))
            {
                propertyMapping.Converter = (INodeConverter)Activator.CreateInstance(provider.ConverterType);
                result = true;
            }

            return result;
        }
    }
}