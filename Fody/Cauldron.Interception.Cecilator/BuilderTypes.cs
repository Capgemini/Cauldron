﻿using Mono.Cecil;
using System.Collections.Generic;

namespace Cauldron.Interception.Cecilator
{
    public partial class BuilderTypeEnumerable
    {
        private Dictionary<TypeReference, Method> castMethod = new Dictionary<TypeReference, Method>();

        private Dictionary<TypeReference, Method> toArray = new Dictionary<TypeReference, Method>();
        private Dictionary<TypeReference, Method> toList = new Dictionary<TypeReference, Method>();

        public Method GetMethod_Cast(TypeReference child)
        {
            if (this.castMethod.TryGetValue(child, out Method value))
                return value;

            var result = this.builderType.GetMethod("Cast", 1, true).MakeGeneric(child).Import();
            this.castMethod.Add(child, result);
            return result;
        }

        public Method GetMethod_ToArray(TypeReference child)
        {
            if (this.toArray.TryGetValue(child, out Method value))
                return value;

            var result = this.builderType.GetMethod("ToArray", 1, true).MakeGeneric(child).Import();
            this.toArray.Add(child, result);
            return result;
        }

        public Method GetMethod_ToList(TypeReference child)
        {
            if (this.toList.TryGetValue(child, out Method value))
                return value;

            var result = this.builderType.GetMethod("ToList", 1, true).MakeGeneric(child).Import();
            this.toList.Add(child, result);
            return result;
        }
    }

    public partial class BuilderTypeEventHandler : TypeSystemExBase
    {
        private Method constructor;

        public Method GetConstructor()
        {
            if (this.constructor == null)
                this.constructor = this.builderType.GetMethod(".ctor", 2, true).Import();

            return this.constructor;
        }
    }

    public partial class BuilderTypeEventHandler1
    {
        private Method var_invoke_0_2;

        public Method GetMethod_Invoke()
        {
            if (this.var_invoke_0_2 == null)
                this.var_invoke_0_2 = this.builderType.GetMethod("Invoke", 2, true).Import();

            return this.var_invoke_0_2;
        }
    }

    public partial class BuilderTypeICollection1
    {
        private Method _add;

        public Method GetMethod_Add()
        {
            if (this._add == null)
                this._add = this.builderType.GetMethod("Add", 1, true).Import();

            return this._add;
        }
    }

    public partial class BuilderTypeMethodBase
    {
        public Method GetMethod_GetMethodFromHandle()
        {
            if (this.var_getmethodfromhandle_0_2 == null)
                this.var_getmethodfromhandle_0_2 = this.builderType.GetMethod("GetMethodFromHandle", 2, true).Import();

            return this.var_getmethodfromhandle_0_2;
        }
    }

    public partial class BuilderTypeMonitor : TypeSystemExBase
    {
        private Method var_enter_0_2;

        /// <summary>
        /// Represents the following method:
        /// <para />
        /// public static void Enter(object obj, ref bool lockTaken);<para/>
        /// </summary>
        public Method GetMethod_Enter()
        {
            if (this.var_enter_0_2 == null)
                this.var_enter_0_2 = this.builderType.GetMethod("Enter", 2, true).Import();

            return this.var_enter_0_2;
        }
    }

    public partial class BuilderTypeNotSupportedException
    {
        private Method constructor_string;

        public Method GetConstructor_String()
        {
            if (this.constructor_string == null)
                this.constructor_string = this.builderType.GetMethod(".ctor", true, (BuilderType)BuilderTypes.String).Import();

            return this.constructor_string;
        }
    }
}