﻿using System;
using System.Collections.Generic;
using RomanticWeb.Converters;
using RomanticWeb.Entities;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Mapping.Providers;

namespace RomanticWeb.Mapping.Conventions
{
    /// <summary>
    /// Convention, which sets converter types based the property type
    /// </summary>
    public class DefaultConvertersConvention : IPropertyConvention, ICollectionConvention
    {
        private readonly IDictionary<Type, Type> _defaultConverters;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConvertersConvention"/> class.
        /// </summary>
        public DefaultConvertersConvention()
        {
            _defaultConverters = new Dictionary<Type, Type>();
        }

        /// <inheritdoc/>
        bool IConvention<IPropertyMappingProvider>.ShouldApply(IPropertyMappingProvider target)
        {
            return (target.ConverterType == null) && (GetConverterType(target.PropertyInfo.PropertyType) != null);
        }

        /// <inheritdoc/>
        void IConvention<IPropertyMappingProvider>.Apply(IPropertyMappingProvider target)
        {
            var isRdfList = (target is ICollectionMappingProvider) && (target as ICollectionMappingProvider).StoreAs == StoreAs.RdfList;

            if (isRdfList)
            {
                target.ConverterType = typeof(AsEntityConverter<IEntity>);
            }
            else
            {
                target.ConverterType = GetConverterType(target.PropertyInfo.PropertyType);
            }
        }

        /// <inheritdoc/>
        bool IConvention<ICollectionMappingProvider>.ShouldApply(ICollectionMappingProvider target)
        {
            return (target.StoreAs == StoreAs.RdfList) && (target.ElementConverterType == null) && (GetConverterType(target.PropertyInfo.PropertyType.FindItemType()) != null);
        }

        /// <inheritdoc/>
        void IConvention<ICollectionMappingProvider>.Apply(ICollectionMappingProvider target)
        {
            target.ElementConverterType = GetConverterType(target.PropertyInfo.PropertyType.FindItemType());
        }

        /// <summary>
        /// Sets a default converter for a given property type.
        /// </summary>
        /// <typeparam name="T">Typ of property</typeparam>
        /// <typeparam name="TConverter">The type of the converter.</typeparam>
        public DefaultConvertersConvention SetDefault<T, TConverter>() where TConverter : INodeConverter, new()
        {
            SetDefault(typeof(T), typeof(TConverter));
            return this;
        }

        /// <summary>
        /// Sets a default converter for multiple property <paramref name="types"/>.
        /// </summary>
        public DefaultConvertersConvention SetDefault<TConverter>(params Type[] types) where TConverter : INodeConverter, new()
        {
            foreach (var type in types)
            {
                SetDefault(type, typeof(TConverter));
            }

            return this;
        }

        private void SetDefault(Type type, Type converterType)
        {
            _defaultConverters[type] = converterType;
        }

        private Type GetConverterType(Type propertyType)
        {
            if (!_defaultConverters.ContainsKey(propertyType))
            {
                propertyType = propertyType.FindItemType();
            }

            if (_defaultConverters.ContainsKey(propertyType))
            {
                return _defaultConverters[propertyType];
            }

            return null;
        }
    }
}