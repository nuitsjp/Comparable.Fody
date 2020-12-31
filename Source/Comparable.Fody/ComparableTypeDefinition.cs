using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Comparable.Fody
{
    public class ComparableTypeDefinition
    {
        private readonly List<ICompareByMemberDefinition> _members;

        public ComparableTypeDefinition(IComparableModuleDefine comparableModuleDefine, TypeDefinition typeDefinition)
        {
            ComparableModuleDefine = comparableModuleDefine;
            TypeDefinition = typeDefinition;
            _members = TypeDefinition
                .Fields.Where(x => x.HasCompareByAttribute()).Select(x => new CompareByFieldDefinition(ComparableModuleDefine, x)).Cast<ICompareByMemberDefinition>()
                .Union(TypeDefinition.Properties.Where(x => x.HasCompareByAttribute()).Select(x => new CompareByPropertyDefinition(ComparableModuleDefine, x)))
                .ToList();

            if (!_members.Any())
            {
                throw new WeavingException($"Specify CompareByAttribute for the any property of Type {FullName}.");
            }

            if (1 < _members
                .GroupBy(x => x.Priority)
                .Select(x => (Priority: x.Key, Count: x.Count()))
                .Max(x => x.Count))
            {
                throw new WeavingException($"Type {FullName} defines multiple CompareBy with equal priority.");
            }


        }
        public IComparableModuleDefine ComparableModuleDefine { get; }
        public TypeDefinition TypeDefinition { get; }

        public IEnumerable<ICompareByMemberDefinition> Members => _members;

        public string FullName => TypeDefinition.FullName;

        public bool IsClass => !TypeDefinition.IsStruct();
        
        private MethodDefinition CompareToByObject { get; set; }

        public void ImplementIComparable(InterfaceImplementation comparable)
        {
            TypeDefinition.Interfaces.Add(comparable);
        }

        public void AddMethod(MethodDefinition methodDefinition) => TypeDefinition.Methods.Add(methodDefinition);

        public void ImplementCompareTo()
        {
            ImplementIComparable(ComparableModuleDefine.ComparableInterface);

            ImplementCompareToByConcreteType();
            ImplementCompareToByObject();
        }


        private void ImplementCompareToByConcreteType()
        {
            CompareToByObject =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    ComparableModuleDefine.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj = new ParameterDefinition("value", ParameterAttributes.None, TypeDefinition);
            CompareToByObject.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(ComparableModuleDefine.Int32);
            CompareToByObject.Body.Variables.Add(localResult);
            foreach (var member in _members)
            {
                CompareToByObject.Body.Variables.Add(member.LocalVariable);
            }

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelReturn = Instruction.Create(OpCodes.Nop);

            var processor = CompareToByObject.Body.GetILProcessor();

            if (IsClass)
            {
                // if (obj == null)
                processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
                processor.Append(Instruction.Create(OpCodes.Ldnull));
                processor.Append(Instruction.Create(OpCodes.Ceq));
                processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentIsNotNull));

                // return 1;
                processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
                processor.Append(Instruction.Create(OpCodes.Ret));
            }

            // ImplementType implementType = (ImplementType)obj;
            processor.Append(labelArgumentIsNotNull);

            // return Value.CompareTo(withSingleProperty.Value);
            foreach (var member in _members)
            {
                member.AppendCompareTo(processor, argumentObj);
                processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
                if (_members.Last() != member)
                {
                    processor.Append(Instruction.Create(OpCodes.Ldloc_S, localResult));
                    processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
                    processor.Append(Instruction.Create(OpCodes.Ceq));
                    processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelReturn));
                }
            }

            processor.Append(labelReturn);
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, localResult));
            processor.Append(Instruction.Create(OpCodes.Ret));

            AddMethod(CompareToByObject);
        }


        private void ImplementCompareToByObject()
        {
            var compareToDefinition =
                new MethodDefinition(
                    nameof(IComparable.CompareTo),
                    MethodAttributes.Public
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual,
                    ComparableModuleDefine.Int32)
                {
                    Body =
                    {
                        MaxStackSize = 2,
                        InitLocals = true
                    }
                };

            // Init arguments.
            var argumentObj =
                new ParameterDefinition("obj", ParameterAttributes.None, ComparableModuleDefine.Object);
            compareToDefinition.Parameters.Add(argumentObj);

            // Init local variables.
            var localResult = new VariableDefinition(ComparableModuleDefine.Int32);
            compareToDefinition.Body.Variables.Add(localResult);

            // Labels for goto.
            var labelArgumentIsNotNull = Instruction.Create(OpCodes.Nop);
            var labelArgumentTypeMatched = Instruction.Create(OpCodes.Nop);
            var labelReturn = Instruction.Create(OpCodes.Nop);

            var processor = compareToDefinition.Body.GetILProcessor();

            // if (obj == null)
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentIsNotNull));

            // return 1;
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_1));
            processor.Append(Instruction.Create(OpCodes.Ret));

            // if (!(obj is StructWithSingleField))
            processor.Append(labelArgumentIsNotNull);
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            processor.Append(Instruction.Create(OpCodes.Isinst, TypeDefinition));
            processor.Append(Instruction.Create(OpCodes.Ldnull));
            processor.Append(Instruction.Create(OpCodes.Cgt_Un));
            processor.Append(Instruction.Create(OpCodes.Ldc_I4_0));
            processor.Append(Instruction.Create(OpCodes.Ceq));
            processor.Append(Instruction.Create(OpCodes.Brfalse_S, labelArgumentTypeMatched));

            // throw new ArgumentException("Object is not a WithSingleProperty");
            processor.Append(Instruction.Create(OpCodes.Ldstr, $"Object is not a {FullName}."));
            processor.Append(Instruction.Create(OpCodes.Newobj, ComparableModuleDefine.ArgumentExceptionConstructor));
            processor.Append(Instruction.Create(OpCodes.Throw));

            processor.Append(labelArgumentTypeMatched);
            // ImplementType implementType = (ImplementType)obj;
            processor.Append(Instruction.Create(OpCodes.Ldarg_0));
            processor.Append(Instruction.Create(OpCodes.Ldarg_S, argumentObj));
            processor.Append(IsClass
                ? Instruction.Create(OpCodes.Castclass, TypeDefinition)
                : Instruction.Create(OpCodes.Unbox_Any, TypeDefinition));

            processor.Append(Instruction.Create(OpCodes.Call, CompareToByObject));
            processor.Append(Instruction.Create(OpCodes.Stloc_S, localResult));
            processor.Append(Instruction.Create(OpCodes.Ldloc_S, localResult));
            processor.Append(Instruction.Create(OpCodes.Ret));

            AddMethod(compareToDefinition);
        }
    }
}