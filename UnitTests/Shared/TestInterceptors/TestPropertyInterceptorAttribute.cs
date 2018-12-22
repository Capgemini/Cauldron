using Cauldron.Interception;
using System;
using Cauldron;
using System.Collections.Generic;

namespace UnitTest_InterceptorsForTest
{
    using Cauldron;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TestPropertyInterceptorAttribute : Attribute, IPropertyGetterInterceptor, IPropertySetterInterceptor
    {
        public bool OnException(Exception e) => true;

        public void OnExit()
        {
        }

        public void OnGet(PropertyInterceptionInfo propertyInterceptionInfo, object value)
        {
            if (Comparer.Equals(value, (long)30))
                propertyInterceptionInfo.SetValue(9999);

            if (Comparer.Equals(value, (double)66))
                propertyInterceptionInfo.SetValue(78344.8f);

            if (Comparer.Equals(value, (int)113))
                propertyInterceptionInfo.SetValue(null);
        }

        public bool OnSet(PropertyInterceptionInfo propertyInterceptionInfo, object oldValue, object newValue)
        {
            if (propertyInterceptionInfo.PropertyType.IsArray || propertyInterceptionInfo.PropertyType.AreReferenceAssignable(typeof(List<long>)))
            {
                var passed = new List<long>();
                passed.Add(2);
                passed.Add(232);
                passed.Add(5643);
                passed.Add(52435);
                propertyInterceptionInfo.SetValue(passed);
                return true;
            }

            if (propertyInterceptionInfo.PropertyType == typeof(long) && Convert.ToInt64(newValue) == 66L)
            {
                propertyInterceptionInfo.SetValue(99L);
                return true;
            }

            return false;
        }
    }
}