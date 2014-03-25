﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RomanticWeb.ComponentModel.Composition;

namespace RomanticWeb.Converters
{
    /// <summary>
    /// Default implementation of <see cref="IConverterCatalog"/>
    /// </summary>
    public sealed class ConverterCatalog:IConverterCatalog
    {
        private static readonly Lazy<IReadOnlyCollection<ILiteralNodeConverter>> LiteralConverters;
        private static readonly Lazy<IReadOnlyCollection<IUriNodeConverter>> ComplexConverters;

        static ConverterCatalog()
        {
            LiteralConverters=new Lazy<IReadOnlyCollection<ILiteralNodeConverter>>(GetLiteralConverters);
            ComplexConverters = new Lazy<IReadOnlyCollection<IUriNodeConverter>>(GetComplexConverters);
        }

        internal ConverterCatalog()
        {
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IUriNodeConverter> UriNodeConverters
        {
            get
            {
                return ComplexConverters.Value;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<ILiteralNodeConverter> LiteralNodeConverters
        {
            get
            {
                return LiteralConverters.Value;
            }
        }

        private static IReadOnlyCollection<IUriNodeConverter> GetComplexConverters()
        {
            return new ReadOnlyCollection<IUriNodeConverter>(ContainerFactory.GetInstancesImplementing<IUriNodeConverter>().ToList());
        }

        private static IReadOnlyCollection<ILiteralNodeConverter> GetLiteralConverters()
        {
            return new ReadOnlyCollection<ILiteralNodeConverter>(ContainerFactory.GetInstancesImplementing<ILiteralNodeConverter>().ToList());
        }
    }
}