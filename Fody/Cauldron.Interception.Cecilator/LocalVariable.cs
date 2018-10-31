﻿using Mono.Cecil.Cil;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Cauldron.Interception.Cecilator
{
    public class LocalVariable : CecilatorBase, IEquatable<LocalVariable>
    {
        [EditorBrowsable(EditorBrowsableState.Never), DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal readonly VariableDefinition variable;

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly BuilderType type;

        internal LocalVariable(BuilderType type, VariableDefinition variable)
        {
            this.variable = variable;
            this.type = type;
            this.Name = variable.Index.ToString();
        }

        internal LocalVariable(BuilderType type, VariableDefinition variable, string name)
        {
            this.variable = variable;
            this.type = type;
            this.Name = name;
        }

        public string Name { get; }
        public BuilderType Type => new BuilderType(this.type, this.variable.VariableType);
        public int Index => this.variable.Index;
        public bool IsPinned => this.variable.IsPinned;

        #region Equitable stuff

        public static implicit operator string(LocalVariable value) => value?.ToString();

        public static bool operator !=(LocalVariable a, LocalVariable b) => !(a == b);

        public static bool operator ==(LocalVariable a, LocalVariable b)
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

            if (obj is LocalVariable)
                return this.Equals(obj as LocalVariable);

            if (obj is VariableDefinition)
                return this.variable == obj as VariableDefinition;

            if (obj is VariableReference)
                return this.variable == obj as VariableReference;

            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(LocalVariable other)
        {
            if (object.Equals(other, null))
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.variable == other.variable;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => this.variable.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => this.variable.Index.ToString();

        #endregion Equitable stuff
    }
}