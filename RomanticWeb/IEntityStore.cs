﻿using System;
using System.Collections.Generic;
using RomanticWeb.Entities;
using RomanticWeb.Model;

namespace RomanticWeb
{
    /// <summary>Represents an in-memory quad cache, which are organized in per-entity quads.</summary>
    public interface IEntityStore
    {
        /// <summary>Gets all quads from the store.</summary>
        IEnumerable<EntityQuad> Quads { get; }

        /// <summary>Gets all changes made to the triple store.</summary>
        DatasetChanges Changes { get; }

        /// <summary>Gets all objects for predicate for a given entity.</summary>
        IEnumerable<Node> GetObjectsForPredicate(EntityId entityId, Uri predicate, Uri graph);

        /// <summary>Gets all quads describing given entity.</summary>
        /// <param name="entityId">Entity identifier for which to retrieve quads.</param>
        /// <param name="includeBlankNodes">Includes all quads for blank-node entities.</param>
        /// <returns>Enumeration of quads describing given entity.</returns>
        IEnumerable<EntityQuad> GetEntityQuads(EntityId entityId, bool includeBlankNodes = true);

        /// <summary>Adds a triple to the store.</summary>
        void AssertEntity(EntityId entityId, IEnumerable<EntityQuad> entityTriples);

        /// <summary>Removes the current triple(s) for subject/predicate and replaces it with triples with given object(s).</summary>
        /// <param name="id">the subject</param>
        /// <param name="propertyUri">the predicate</param>
        /// <param name="valueNode">new object node(s)</param>
        /// <param name="graphUri">destination graph</param>
        void ReplacePredicateValues(EntityId id, Node propertyUri, Func<IEnumerable<Node>> valueNode, Uri graphUri);

        /// <summary>Marks an entity for deletion.</summary>
        /// <param name="entityId">Identifier of the entity to be removed.</param>
        /// <param name="deleteBehaviour">Optional parameter telling how to tread other related entities.</param>
        void Delete(EntityId entityId, DeleteBehaviours deleteBehaviour = DeleteBehaviours.DeleteVolatileChildren | DeleteBehaviours.NullifyVolatileChildren);

        /// <summary>Forces the store to use current state as it's initial state.</summary>
        void ResetState();
    }
}