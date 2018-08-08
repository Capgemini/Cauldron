﻿using Cauldron.Activator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Activator
{
    [TestClass]
    public class FactoryGenericSingletonTests
    {
        public interface IMyClass
        {
            string Value { get; set; }
        }

        [TestMethod]
        public void Create_Instance_Class()
        {
            MyClass.Current.Value = "Toast";

            var instance = Factory.Create<MyClass>();

            Assert.AreNotEqual(MyClass.Current.Value, instance.Value);
        }

        [TestMethod]
        public void Create_Instance_Interface()
        {
            MySecondClass.Current.Value = "Toast";

            var instance = Factory.Create<IMyClass>();

            Assert.AreNotEqual(MySecondClass.Current.Value, instance.Value);
        }

        public class MyClass : Factory<MyClass>
        {
            public string Value { get; set; }
        }

        public class MySecondClass : Factory<IMyClass>, IMyClass
        {
            public string Value { get; set; }
        }
    }
}