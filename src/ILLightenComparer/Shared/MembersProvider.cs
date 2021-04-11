using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILLightenComparer.Config;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;

namespace ILLightenComparer.Shared
{
    internal sealed class MembersProvider
    {
        private static IVariable Create(MemberInfo memberInfo) =>
            PropertyMemberVariable.Create(memberInfo)
            ?? FieldMemberVariable.Create(memberInfo)
            ?? throw new ArgumentException(
                $"Can't create member variable from {memberInfo.DisplayName()}",
                nameof(memberInfo));

        private static IVariable[] Convert(IEnumerable<MemberInfo> members) => members.Select(Create).ToArray();

        private static IEnumerable<MemberInfo> DefaultOrder(IEnumerable<MemberInfo> members) =>
            members
                .OrderBy(x => x.DeclaringType?.FullName ?? string.Empty)
                .ThenBy(x => x.MemberType)
                .ThenBy(x => x.Name);

        private readonly IConfigurationProvider _configuration;

        public MembersProvider(IConfigurationProvider configuration)
        {
            _configuration = configuration;
        }

        private IEnumerable<MemberInfo> Filter(IReflect type) =>
            type
                .GetMembers(BindingFlags.Instance
                            | BindingFlags.FlattenHierarchy
                            | BindingFlags.Public)
                .Where(IncludeFields)
                .Where(IgnoredMembers);

        public IVariable[] GetMembers(Type type)
        {
            var filtered = Filter(type);
            var sorted = Sort(type, filtered);

            return Convert(sorted);
        }

        private bool IgnoredMembers(MemberInfo memberInfo)
        {
            var ignoredMembers = _configuration.Get(memberInfo.DeclaringType).IgnoredMembers;

            return !ignoredMembers.Contains(memberInfo.Name);
        }

        private bool IncludeFields(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Property) {
                return true;
            }

            var includeFields = _configuration.Get(memberInfo.DeclaringType).IncludeFields;

            return includeFields && memberInfo.MemberType == MemberTypes.Field;
        }

        private IEnumerable<MemberInfo> PredefinedOrder(Type ownerType, IEnumerable<MemberInfo> members)
        {
            var order = _configuration.Get(ownerType).MembersOrder;
            var dictionary = members.ToDictionary(x => x.Name);

            foreach (var item in order) {
                if (!dictionary.TryGetValue(item, out var memberInfo)) {
                    continue;
                }

                dictionary.Remove(item);

                yield return memberInfo;
            }

            foreach (var item in DefaultOrder(dictionary.Values)) {
                yield return item;
            }
        }

        private IEnumerable<MemberInfo> Sort(Type ownerType, IEnumerable<MemberInfo> members)
        {
            var order = _configuration.Get(ownerType).MembersOrder;

            if (order == null || order.Length == 0) {
                return DefaultOrder(members);
            }

            return PredefinedOrder(ownerType, members);
        }
    }
}
