using System;
using System.Linq;
using RomanticWeb.Entities;
using RomanticWeb.Mapping;
using RomanticWeb.NamedGraphs;
using RomanticWeb.Ontologies;

namespace RomanticWeb
{
    /// <summary>Behavior that should be applied when deleting entities.</summary>
    [Flags]
    public enum DeleteBehaviours
    {
        /// <summary>Default delete behavior set to <see cref="DeleteBehaviours.DeleteVolatileChildren" /> and <see cref="DeleteBehaviours.NullifyVolatileChildren" />.</summary>
        Default = 0x0000011,

        /// <summary>Nothing special should happen.</summary>
        DoNothing = 0x00000000,

        /// <summary>Delete other blank node entities referenced by the deleted entity.</summary>
        DeleteVolatileChildren = 0x00000001,

        /// <summary>Delete other entities referenced by the deleted entity.</summary>
        DeleteChildren = 0x00000003,

        /// <summary>Remove statements that referenced removed blank node entities.</summary>
        NullifyVolatileChildren = 0x00000010,

        /// <summary>Remove statements that referenced removed entities.</summary>
        NullifyChildren = 0x00000030
    }

    /// <summary>Defines methods for factories, which produce <see cref="Entity"/> instances.</summary>
    public interface IEntityContext : IDisposable
    {
        /// <summary>Gets the underlying in-memory store.</summary>
        IEntityStore Store { get; }

        /// <summary>Gets a value indicating whether the underlying store has any changes.</summary>
        bool HasChanges { get; }

        /// <summary>Gets the blank identifier generator.</summary>
        /// <value>The blank identifier generator.</value>
        IBlankNodeIdGenerator BlankIdGenerator { get; }

        /// <summary>Gets the <see cref="IOntologyProvider" />.</summary>
        IOntologyProvider Ontologies { get; }

        /// <summary>Gets the <see cref="INamedGraphSelector" />.</summary>
        INamedGraphSelector GraphSelector { get; }

        /// <summary>Gets the transformer catalog.</summary>
        IResultTransformerCatalog TransformerCatalog { get; }

        /// <summary>Gets the <see cref="IMappingsRepository" />.</summary>
        IMappingsRepository Mappings { get; }

        /// <summary>Gets the <see cref="IBaseUriSelectionPolicy" />.</summary>
        IBaseUriSelectionPolicy BaseUriSelector { get; }

        /// <summary>Converts this context into a LINQ queryable data source.</summary>
        /// <returns>A LINQ querable data source.</returns>
        IQueryable<IEntity> AsQueryable();

        /// <summary>Converts this context into a LINQ queryable data source of entities of given type.</summary>
        /// <typeparam name="T">Type of entities to work with.</typeparam>
        /// <returns>A LINQ queryable data source of entities of given type.</returns>
        IQueryable<T> AsQueryable<T>() where T : class, IEntity;

        /// <summary>Loads an existing typed entity.</summary>
        /// <typeparam name="T">Type to be used when returning a typed entity.</typeparam>
        /// <param name="entityId">Entity identifier</param>
        /// <returns>Typed instance of an entity wih given identifier or null.</returns>
        T Load<T>(EntityId entityId) where T : class, IEntity;

        /// <summary>Creates a new typed entity.</summary>
        /// <typeparam name="T">Type to be used when returning a typed entity.</typeparam>
        /// <param name="entityId">Entity identifier</param>
        T Create<T>(EntityId entityId) where T : class, IEntity;

        /// <summary>Saves all changes to the underlying store.</summary>
        void Commit();

        /// <summary>Marks an entity for deletion.</summary>
        /// <param name="entityId">Target entity to be deleted.</param>
        void Delete(EntityId entityId);

        /// <summary>Marks an entity for deletion.</summary>
        /// <param name="entityId">Target entity to be deleted.</param>
        /// <param name="deleteBehaviour">Entity deletion behaviour.</param>
        void Delete(EntityId entityId, DeleteBehaviours deleteBehaviour);

        /// <summary>Initializes the enitity.</summary>
        /// <param name="entity">The entity.</param>
        void InitializeEnitity(IEntity entity);

        /// <summary>Wraps an entity as another entity type.</summary>
        /// <typeparam name="T">the <see cref="IEntity"/> type</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        T EntityAs<T>(IEntity entity) where T : class, IEntity;

        /// <summary>Checks if the entity exists.</summary>
        bool Exists(EntityId entityId);
    }
}