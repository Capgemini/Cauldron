﻿using Cauldron.Interception.Cecilator.Coders;

using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Cauldron.Interception.Cecilator
{
    public class Property : CecilatorBase, IEquatable<Property>
    {
        [EditorBrowsable(EditorBrowsableState.Never), DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal readonly PropertyDefinition propertyDefinition;

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal readonly BuilderType type;

        internal Property(BuilderType type, PropertyDefinition propertyDefinition)
        {
            this.type = type;
            this.propertyDefinition = propertyDefinition;
            this.Getter = propertyDefinition.GetMethod == null ? null : new Method(type, propertyDefinition.GetMethod.ResolveMethod(type.typeReference), propertyDefinition.GetMethod);
            this.Setter = propertyDefinition.SetMethod == null ? null : new Method(type, propertyDefinition.SetMethod.ResolveMethod(type.typeReference), propertyDefinition.SetMethod);

            this.RefreshBackingField();
        }

        public Field BackingField { get; private set; }

        public BuilderCustomAttributeCollection CustomAttributes => new BuilderCustomAttributeCollection(this.propertyDefinition);

        public string Fullname => this.propertyDefinition.FullName;

        public Method Getter { get; private set; }

        public bool IsAutoProperty => (this.propertyDefinition.GetMethod ?? this.propertyDefinition.SetMethod).CustomAttributes.Get("CompilerGeneratedAttribute") != null;

        public bool IsGenerated => this.propertyDefinition.Name.IndexOf('<') >= 0 ||
                                            this.propertyDefinition.Name.IndexOf('>') >= 0 ||
                                            this.type.typeDefinition.FullName.IndexOf('<') >= 0 ||
                                            this.type.typeDefinition.FullName.IndexOf('>') >= 0;

        public bool IsInternal => this.GetAttributes().HasFlag(MethodAttributes.Assembly);

        public bool IsOverride => this.GetterOrSetter.With(x => x.IsVirtual && x.IsHideBySig && !x.IsStatic);

        public bool IsPrivate => this.GetAttributes().HasFlag(MethodAttributes.Private);

        public bool IsProtected => this.GetAttributes().HasFlag(MethodAttributes.Family);

        public Modifiers Modifiers
        {
            get
            {
                Modifiers modifiers = 0;

                if (this.Getter != null)
                {
                    if (this.Getter.methodDefinition.Attributes.HasFlag(MethodAttributes.Private)) modifiers |= Modifiers.Private;
                    if (this.Getter.methodDefinition.Attributes.HasFlag(MethodAttributes.Static)) modifiers |= Modifiers.Static;
                    if (this.Getter.methodDefinition.Attributes.HasFlag(MethodAttributes.Public)) modifiers |= Modifiers.Public;
                    if (this.Getter.methodDefinition.Attributes.HasFlag(MethodAttributes.Virtual) &&
                        this.Getter.methodDefinition.Attributes.HasFlag(MethodAttributes.NewSlot)) modifiers |= Modifiers.Overrides;
                }

                if (this.Setter != null)
                {
                    if (this.Setter.methodDefinition.Attributes.HasFlag(MethodAttributes.Private)) modifiers |= Modifiers.Private;
                    if (this.Setter.methodDefinition.Attributes.HasFlag(MethodAttributes.Static)) modifiers |= Modifiers.Static;
                    if (this.Setter.methodDefinition.Attributes.HasFlag(MethodAttributes.Public)) modifiers |= Modifiers.Public;
                    if (this.Setter.methodDefinition.Attributes.HasFlag(MethodAttributes.Virtual) &&
                        this.Setter.methodDefinition.Attributes.HasFlag(MethodAttributes.NewSlot)) modifiers |= Modifiers.Overrides;
                }

                if (modifiers.HasFlag(Modifiers.Public) && modifiers.HasFlag(Modifiers.Private))
                    modifiers &= ~Modifiers.Private;

                return modifiers;
            }
        }

        /// <summary>
        /// Gets the type that inherited the property.
        /// </summary>
        public BuilderType OriginType => this.type;

        public BuilderType ReturnType
        {
            get
            {
                var type = this.Getter?.methodReference.ReturnType ?? this.propertyDefinition.PropertyType;

                if ((type.HasGenericParameters && !type.IsGenericInstance) || type.Resolve() == null)
                    return new BuilderType(this.type, type.ResolveType(this.type.typeReference));

                return new BuilderType(this.type, type);
            }
        }

        public Method Setter { get; private set; }

        /// <summary>
        /// Gets the type that contains the property.
        /// </summary>
        public BuilderType DeclaringType => new BuilderType(this.propertyDefinition.DeclaringType);

        public bool IsAbstract => this.GetterOrSetter.With(x => x.IsAbstract && x.IsHideBySig && x.IsNewSlot && x.IsVirtual);
        public bool IsPublic => this.GetterOrSetter.IsPublic;

        public bool IsStatic => this.GetterOrSetter.IsStatic;
        public string Name => this.propertyDefinition.Name;
        private MethodDefinition GetterOrSetter => this.propertyDefinition.GetMethod ?? this.propertyDefinition.SetMethod;

        public void AddSetter()
        {
            this.propertyDefinition.SetMethod = new MethodDefinition("set_" + this.Name, this.propertyDefinition.GetMethod.Attributes, this.ModuleDefinition.TypeSystem.Void);
            this.propertyDefinition.SetMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, this.ReturnType.typeReference));
            this.type.typeDefinition.Methods.Add(this.propertyDefinition.SetMethod);

            this.Setter = new Method(this.type, this.propertyDefinition.SetMethod);
            this.Setter.NewCoder().SetValue(this.BackingField, CodeBlocks.GetParameter(0)).Return().Replace();
        }

        public Field CreateField(Type fieldType, string name)
        {
            var field = this.OriginType.typeDefinition.Fields.Get(name);

            if (field != null)
                return new Field(this.type, field);

            return this.CreateField(this.ModuleDefinition.ImportReference(fieldType.GetTypeDefinition().ResolveType(this.OriginType.typeReference)), name);
        }

        public Field CreateField(Field field, string name) => this.CreateField(field.fieldRef.FieldType, name);

        public Field CreateField(BuilderType type, string name) => this.CreateField(type.typeReference, name);

        public Field CreateField(TypeReference typeReference, string name)
        {
            var field = this.OriginType.typeDefinition.Fields.Get(name);

            if (field != null)
                return new Field(this.type, field);

            return this.IsStatic ? this.OriginType.CreateField(Modifiers.PrivateStatic, typeReference, name) : this.OriginType.CreateField(Modifiers.Private, typeReference, name);
        }

        public void Overrides(Property property)
        {
            this.Getter?.methodDefinition.Overrides.Add(property.Getter.methodReference);
            this.Setter?.methodDefinition.Overrides.Add(property.Setter.methodReference);
        }

        public void Remove()
        {
            if (this.propertyDefinition.GetMethod != null)
                this.type.typeDefinition.Methods.Remove(this.propertyDefinition.GetMethod);

            if (this.propertyDefinition.SetMethod != null)
                this.type.typeDefinition.Methods.Remove(this.propertyDefinition.SetMethod);

            this.type.typeDefinition.Properties.Remove(this.propertyDefinition);

            InstructionBucket.Reset();
        }

        internal void RefreshBackingField()
        {
            var fieldInGetter = this.propertyDefinition.GetMethod?.Body?.Instructions.LastOrDefault(x => x.OpCode == OpCodes.Ldfld || x.OpCode == OpCodes.Ldsfld);
            var fieldInSetter = this.propertyDefinition.SetMethod?.Body?.Instructions.LastOrDefault(x => x.OpCode == OpCodes.Stfld || x.OpCode == OpCodes.Stsfld);
            object operand = null;

            /*
             * This is a very basic detection of the backing field...
             * If there is a setter and a getter we will try to get the fields (by LdFld and Stfld) used in the getter and setter.
             * If both fields are the same then we will asume that this is the correct backing field...
             * In other cases we will be just having wild guesses.
             */

            if (this.Getter != null && this.Setter != null && fieldInGetter == fieldInSetter)
                operand = fieldInSetter?.Operand;
            else if (this.Getter != null && fieldInGetter != null)
                operand = fieldInGetter?.Operand;
            else if (this.Setter != null && fieldInSetter != null)
                operand = fieldInSetter?.Operand;

            var field = operand as FieldDefinition ?? operand as FieldReference;
            if (field != null)
                this.BackingField = new Field(this.type, field.Resolve(), field);
        }

        private MethodAttributes GetAttributes()
        {
            MethodAttributes result = 0;

            if (this.propertyDefinition.GetMethod != null)
                result |= this.propertyDefinition.GetMethod.Attributes;

            if (this.propertyDefinition.SetMethod != null)
                result |= this.propertyDefinition.SetMethod.Attributes;

            return result;
        }

        #region Equitable stuff

        public static implicit operator string(Property value) => value.ToString();

        public static bool operator !=(Property a, Property b) => !(a == b);

        public static bool operator ==(Property a, Property b)
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

            if (obj is Property)
                return this.Equals(obj as Property);

            if (obj is PropertyDefinition)
                return this.propertyDefinition == obj as PropertyDefinition;

            if (obj is PropertyReference)
                return this.propertyDefinition == obj as PropertyReference;

            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(Property other)
        {
            if (object.Equals(other, null))
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.propertyDefinition == other.propertyDefinition;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => this.propertyDefinition.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => this.propertyDefinition.FullName;

        #endregion Equitable stuff
    }
}