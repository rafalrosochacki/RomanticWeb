﻿using System.Collections.Generic;
using RomanticWeb.Ontologies;

namespace RomanticWeb
{
    public interface ITriplesSource
    {
        IEnumerable<RdfNode> GetObjectsForPredicate(EntityId entityId, Property predicate);
        bool TryGetListElements(RdfNode rdfList, out IEnumerable<RdfNode> listElements);
    }
}