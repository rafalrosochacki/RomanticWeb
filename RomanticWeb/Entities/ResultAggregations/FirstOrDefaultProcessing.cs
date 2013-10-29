﻿using System.Collections.Generic;
using System.Linq;

namespace RomanticWeb.Entities.ResultAggregations
{
    [ResultAggregationStrategy(ProcessingOperation.FirstOrDefault)]
    internal class FirstOrDefaultProcessing : IResultProcessingStrategy
    {
        public object Process(IEnumerable<object> objects)
        {
            return objects.FirstOrDefault();
        }
    }
}