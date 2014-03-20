﻿using System;
using System.Diagnostics;
using NullGuard;

namespace RomanticWeb.Mapping.Model
{
    [NullGuard(ValidationFlags.All)]
    [DebuggerDisplay("Dictionary {Name}")]
    internal class DictionaryMapping:PropertyMapping,IDictionaryMapping
    {
        public DictionaryMapping(Type returnType,string name,Uri predicateUri,Uri keyPredicate,Uri valuePredicate)
            :base(returnType,name,predicateUri)
        {
            ValuePredicate=valuePredicate;
            KeyPredicate=keyPredicate;
        }

        public Uri KeyPredicate { get; private set; }

        public Uri ValuePredicate { get; private set; }
    }
}