﻿using Mono.Cecil;

namespace Comparable.Fody
{
    public interface IComparableModuleDefine
    {
        TypeReference Int32 { get; }
        
        TypeReference Object { get; }
        InterfaceImplementation ComparableInterface { get; }
        MethodReference ArgumentExceptionConstructor { get; }

        IComparableTypeDefinition FindComparableTypeDefinition(TypeReference typeReference);

        TypeReference ImportReference(TypeReference typeReference);

        MethodReference ImportReference(MethodReference methodReference);
    }
}