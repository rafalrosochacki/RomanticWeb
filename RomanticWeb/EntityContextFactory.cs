﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Anotar.NLog;
using RomanticWeb.Mapping;
using RomanticWeb.Mapping.Model;
using RomanticWeb.Ontologies;

namespace RomanticWeb
{
    /// <summary>
    /// An entrypoint to RomanticWeb, which encapsulates modularity and creation of <see cref="IEntityContext"/>
    /// </summary>
    public class EntityContextFactory : IEntityContextFactory
    {
        private readonly CompositionContainer _container;
        private bool _isInitialized;

        [ImportMany(typeof(IOntologyProvider), AllowRecomposition = true)]
        private IEnumerable<IOntologyProvider> _importedOntologies;

        [ImportMany(typeof(IMappingsRepository), AllowRecomposition = true)]
        private IEnumerable<IMappingsRepository> _importedMappings;

        private Func<IEntitySource> _entitySourceFactory;
        private MappingContext _mappingContext;
        private Func<IEntityStore> _entityStoreFactory = () => new EntityStore();
        private IGraphSelectionStrategy _defaultGraphSelector = new DefaultGraphSelector();
        private IMappingsRepository _actualMappingsRepository;
        private IOntologyProvider _actualOntologyProvider;

        /// <summary>
        /// Creates a new instance of <see cref="EntityContextFactory"/>
        /// </summary>
        public EntityContextFactory()
        {
            var catalog = new DirectoryCatalog(AppDomain.CurrentDomain.GetPrimaryAssemblyPath());
            _container = new CompositionContainer(catalog, true);
            catalog.Changed += CatalogChanged;
            LogTo.Info("Created entity context factory");
        }

        /// <inheritdoc />
        public IOntologyProvider Ontologies
        {
            get
            {
                EnsureInitialized();
                return _actualOntologyProvider;
            }
        }

        /// <inheritdoc />
        public IMappingsRepository Mappings
        {
            get
            {
                EnsureInitialized();
                return _actualMappingsRepository;
            }
        }

        /// <summary>
        /// Creates a new instance of entity context
        /// </summary>
        public IEntityContext CreateContext()
        {
            LogTo.Debug("Creating entity context");

            EnsureComplete();
            EnsureInitialized();
            _mappingContext = new MappingContext(_actualOntologyProvider, _defaultGraphSelector);

            return new EntityContext(this, Mappings, _mappingContext, _entityStoreFactory(), _entitySourceFactory());
        }

        public EntityContextFactory WithEntitySource(Func<IEntitySource> entitySource)
        {
            _entitySourceFactory=entitySource;
            return this;
        }

        /// <inheritdoc />
        public void SatisfyImports(object obj)
        {
            _container.ComposeParts(obj);
        }

        public EntityContextFactory WithOntology(IOntologyProvider ontologyProvider)
        {
            _container.ComposeExportedValue(ontologyProvider);
            return this;
        }

        /// <summary>
        /// Exposes the method to register mapping repositories
        /// </summary>
        public EntityContextFactory WithMappings(Action<MappingBuilder> buildMappings)
        {
            var mappingBuilder = new MappingBuilder();
            buildMappings(mappingBuilder);

            foreach (var mappingsRepository in mappingBuilder)
            {
                _container.ComposeExportedValue(mappingsRepository);
            }

            return this;
        }

        public EntityContextFactory WithEntityStore(Func<IEntityStore> entityStoreFactory)
        {
            _entityStoreFactory = entityStoreFactory;
            return this;
        }

        public EntityContextFactory WithDefaultGraphSelector(IGraphSelectionStrategy graphSelector)
        {
            _defaultGraphSelector = graphSelector;
            return this;
        }

        private void EnsureInitialized()
        {
            LogTo.Info("Initializing entity context factory");
            if (_isInitialized)
            {
                return;
            }

            var batch = new CompositionBatch();
            batch.AddPart(this);

            _container.Compose(batch);
            EnsureOntologyProvider();
            EnsureMappingsRepository();
            EnsureMappingsRebuilt();

            _isInitialized=true;
        }

        private void CatalogChanged(object sender, ComposablePartCatalogChangeEventArgs changeEventArgs)
        {
            LogTo.Info("MEF catalog has changed");
            bool shouldRebuildMappings=false;
            
            if (changeEventArgs.AddedDefinitions.Any(def => def.Exports<IMappingsRepository>()))
            {
                LogTo.Info("Refreshing mapping repositories");
                EnsureMappingsRepository();
                shouldRebuildMappings=true;
            }

            if (changeEventArgs.AddedDefinitions.Any(def => def.Exports<IOntologyProvider>()))
            {
                LogTo.Info("Refreshing ontology providers");
                EnsureOntologyProvider();
                shouldRebuildMappings=true;
            }

            if (shouldRebuildMappings)
            {
                EnsureMappingsRebuilt();
            }
        }

        private void EnsureOntologyProvider()
        {
            if (_importedOntologies.Count() == 1)
            {
                _actualOntologyProvider = _importedOntologies.Single();
            }
            else
            {
                _actualOntologyProvider = new CompoundOntologyProvider(_importedOntologies);
            }
        }

        private void EnsureMappingsRepository()
        {
            if (_importedMappings.Count() == 1)
            {
                _actualMappingsRepository = _importedMappings.Single();
            }
            else
            {
                _actualMappingsRepository = new CompoundMappingsRepository(_importedMappings);
            }
        }

        private void EnsureMappingsRebuilt()
        {
            _actualMappingsRepository.RebuildMappings(new MappingContext(_actualOntologyProvider, _defaultGraphSelector));
        }

        private void EnsureComplete()
        {
            if (_entitySourceFactory==null)
            {
                throw new InvalidOperationException("Entity source factory wasn't set");
            }
        }
    }
}