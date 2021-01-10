﻿using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;

namespace Comparable.Fody
{
    public class ComparableTypeReference : IComparableTypeReference
    {
        private readonly TypeReference _selfReference;
        private readonly TypeDefinition _selfDefinition;
        private readonly List<ICompareByMemberReference> _members;

        public ComparableTypeReference(TypeReference selfReference, TypeDefinition selfDefinition, IReferenceProvider referenceProvider)
        {
            _selfReference = selfReference;
            _selfDefinition = selfDefinition;
            if (selfDefinition.HasCompareAttribute())
            {
                var fields =
                    selfDefinition
                        .Fields
                        .Where(x => MemberDefinitionExtensions.HasCompareByAttribute(x))
                        .Select(referenceProvider.Resolve);

                var properties =
                    selfDefinition
                        .Properties
                        .Where(x => x.HasCompareByAttribute())
                        .Select(referenceProvider.Resolve);

                _members = fields.Union(properties).ToList();

                if (_members.Empty())
                {
                    throw new WeavingException($"Specify CompareByAttribute for the any property of Type {selfDefinition.FullName}.");
                }

                if (1 < _members
                    .GroupBy(x => x.Priority)
                    .Max(x => x.Count()))
                {
                    throw new WeavingException($"Type {selfDefinition.FullName} defines multiple CompareBy with equal priority.");
                }
            }
            else
            {
                _members = new();
            }

        }

        public string FullName => _selfReference.FullName;

        public int Depth =>
            _members.Empty() ? 0 : _members.Max(x => x.Depth) + 1;

        public IComparableTypeDefinition Resolve(IComparableModuleDefine comparableModuleDefine)
            => new ComparableTypeDefinition(comparableModuleDefine, _selfDefinition, _selfReference);
    }
}