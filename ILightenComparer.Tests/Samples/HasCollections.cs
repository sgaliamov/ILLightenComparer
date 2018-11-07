﻿using System.Collections.Generic;

namespace ILightenComparer.Tests.Samples
{
    internal class HasCollections
    {
        public int[,,] ThreeDimensionalArray { get; set; }
        public int[][] ArrayOfArrays { get; set; }
        public string[] Strings { get; set; }
        public List<int> Integers { get; set; }
        public IEnumerable<FlatObject> SimpleItems { get; set; }
        public Dictionary<int, HierarchicalObject> Dictionary { get; set; }
    }
}
