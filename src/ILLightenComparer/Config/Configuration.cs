using System;
using System.Collections.Generic;
using System.Linq;

namespace ILLightenComparer.Config
{
    internal sealed class Configuration
    {
        public Configuration(Configuration configuration) : this(
            configuration.IgnoredMembers,
            configuration.IncludeFields,
            configuration.MembersOrder,
            configuration.StringComparisonType,
            configuration.DetectCycles,
            configuration.IgnoreCollectionOrder,
            configuration.HashSeed) { }

        public Configuration(
            IEnumerable<string> ignoredMembers,
            bool includeFields,
            IEnumerable<string> membersOrder,
            StringComparison stringComparisonType,
            bool detectCycles,
            bool ignoreCollectionOrder,
            long hashSeed
        )
        {
            IgnoredMembers = new HashSet<string>(ignoredMembers ?? throw new ArgumentNullException(nameof(ignoredMembers)));
            IncludeFields = includeFields;
            SetMembersOrder(membersOrder);
            StringComparisonType = stringComparisonType;
            DetectCycles = detectCycles;
            IgnoreCollectionOrder = ignoreCollectionOrder;
            HashSeed = hashSeed;
        }

        public bool DetectCycles { get; set; }
        public bool IgnoreCollectionOrder { get; set; }
        public HashSet<string> IgnoredMembers { get; }
        public bool IncludeFields { get; set; }
        public string[] MembersOrder { get; private set; }
        public StringComparison StringComparisonType { get; set; }
        public long HashSeed { get; set; }

        public void SetIgnoredMembers(IEnumerable<string> ignoredMembers)
        {
            if (ignoredMembers == null) {
                IgnoredMembers.Clear();
            } else {
                IgnoredMembers.UnionWith(ignoredMembers);
            }
        }

        public void SetMembersOrder(IEnumerable<string> value) => MembersOrder = value?.Distinct().ToArray() ?? throw new ArgumentNullException(nameof(value));
    }
}
