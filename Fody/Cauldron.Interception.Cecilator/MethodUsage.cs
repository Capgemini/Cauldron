﻿using Cauldron.Interception.Cecilator.Coders;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Cauldron.Interception.Cecilator
{
    public class MethodUsage : CecilatorBase, IEquatable<MethodUsage>
    {
        [EditorBrowsable(EditorBrowsableState.Never), DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Instruction instruction;

        internal MethodUsage(Method method, Method hostMethod, Instruction instruction)
        {
            this.Method = method;
            this.HostMethod = hostMethod;
            this.Type = method.type;
            this.instruction = instruction;
        }

        public Method HostMethod { get; private set; }

        public Method Method { get; private set; }

        public Position Position => new Position(this.HostMethod, this.instruction);

        public BuilderType Type { get; private set; }

        public static implicit operator Instruction(MethodUsage methodUsage) => methodUsage.instruction;

        public static implicit operator MethodDefinition(MethodUsage methodUsage) => methodUsage.HostMethod.methodDefinition;

        public BuilderType GetGenericArgument(int index)
        {
            if (!(this.instruction.Operand is GenericInstanceMethod method))
                return null;

            return new BuilderType(method.GenericArguments[index]);
        }

        public BuilderType GetLastNewObjectType()
        {
            var interestingInstruction = this.instruction.Previous.Previous.Previous.Previous;

            if (interestingInstruction.OpCode == OpCodes.Newobj)
            {
                var ctor = interestingInstruction.Operand as MethodDefinition ?? interestingInstruction.Operand as MethodReference;
                return new BuilderType(ctor.DeclaringType);
            }

            return null;
        }

        public BuilderType GetPreviousInstructionObjectType()
        {
            TypeReference declaringType = null;
            var previousInstruction = this.instruction.Previous;

            if (previousInstruction.OpCode == OpCodes.Dup)
                previousInstruction = previousInstruction.Previous;

            if (previousInstruction.Operand is MethodReference)
            {
                var methodReference = previousInstruction.Operand as MethodReference;
                var declaringTypeDefinition = methodReference.DeclaringType.Resolve();

                if (declaringTypeDefinition.IsAbstract && declaringTypeDefinition.IsSealed)
                    declaringType = methodReference.Resolve().ReturnType.ResolveType(methodReference);
                else
                    declaringType = methodReference.DeclaringType;
            }
            else if (previousInstruction.OpCode == OpCodes.Ldarg_0 ||
              previousInstruction.OpCode == OpCodes.Ldarg_1 ||
              previousInstruction.OpCode == OpCodes.Ldarg_2 ||
              previousInstruction.OpCode == OpCodes.Ldarg_3 ||
              previousInstruction.OpCode == OpCodes.Ldarg_S)
                declaringType = this.HostMethod.methodReference.GetParameter(previousInstruction)?.ParameterType ?? this.HostMethod.OriginType.typeReference;
            else if (previousInstruction.OpCode == OpCodes.Ldloc_0 ||
                previousInstruction.OpCode == OpCodes.Ldloc_1 ||
                previousInstruction.OpCode == OpCodes.Ldloc_2 ||
                previousInstruction.OpCode == OpCodes.Ldloc_3 ||
                previousInstruction.OpCode == OpCodes.Ldloc_S)
                declaringType = this.HostMethod.methodDefinition.GetVariable(previousInstruction)?.VariableType;
            else if (previousInstruction.OpCode == OpCodes.Ldfld || previousInstruction.OpCode == OpCodes.Ldsfld)
            {
                var field = previousInstruction.Operand as FieldReference;
                declaringType = field.FieldType;
            }
            else
                throw new Exception($"'{ this.HostMethod.methodDefinition.Name}': The anonymous type was not found.");

            return new BuilderType(declaringType);
        }

        public void Replace(Method method, bool autoCast = true)
        {
            this.instruction.Operand = Builder.Import(method.methodReference);

            // If we know this method has only one param (Because easy to do), we should try to check
            // the type and and try a cast if needed
            var parameters = method.Parameters;
            if (parameters.Length == 1 && autoCast)
            {
                var previousType = this.GetPreviousInstructionObjectType();

                if (parameters[0].typeReference.FullName.GetHashCode() == previousType.typeReference.FullName.GetHashCode() && parameters[0].typeReference.FullName == previousType.typeReference.FullName)
                    return;

                var paramResult = new ParamResult();
                var processor = this.HostMethod.GetILProcessor();
                paramResult.Type = previousType.typeReference;
                var coder = method.NewCoder();
                InstructionBlock.CastOrBoxValues(coder.instructions, parameters[0]);
                processor.InsertBefore(this.instruction, paramResult.Instructions);
            }
        }

        public string ToHostMethodInstructionsString()
        {
            var sb = new StringBuilder();

            foreach (var item in this.HostMethod.methodDefinition.Body.Instructions)
                sb.AppendLine($"IL_{item.Offset.ToString("X4")}: {item.OpCode.ToString()} { (item.Operand is Instruction ? "IL_" + (item.Operand as Instruction).Offset.ToString("X4") : item.Operand?.ToString())} ");

            return sb.ToString();
        }

        #region Equitable stuff

        public static implicit operator string(MethodUsage value) => value.ToString();

        public static bool operator !=(MethodUsage a, MethodUsage b) => !(a == b);

        public static bool operator ==(MethodUsage a, MethodUsage b)
        {
            if (object.Equals(a, null) && object.Equals(b, null))
                return true;

            if (object.Equals(a, null))
                return false;

            return a.Equals(b);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (object.Equals(obj, null))
                return false;

            if (object.ReferenceEquals(obj, this))
                return true;

            if (obj is MethodUsage)
                return this.Equals(obj as MethodUsage);

            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(MethodUsage other)
        {
            if (object.Equals(other, null))
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return object.Equals(other, this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => this.HostMethod.GetHashCode() ^ this.Method.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => $"IL_{this.instruction.Offset.ToString("X4")} >> {this.HostMethod.methodDefinition.FullName} >> {this.Method.methodReference.Name}";

        #endregion Equitable stuff
    }
}