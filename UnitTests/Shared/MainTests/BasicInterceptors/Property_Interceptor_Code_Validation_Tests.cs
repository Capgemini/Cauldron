using UnitTest_InterceptorsForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTests.BasicInterceptors
{
    [TestClass]
    public class Property_Interceptor_Code_Validation_Tests
    {
        private string _APropertyWithoutGetter;
        private string _myCustomBackingField;

        [ExternalLockablePropertyInterceptor]
        public static string StaticLockableProperty { get; set; }

        [TestPropertyInterceptor]
        public static double StaticProperty { get; set; }

        [TestPropertyInterceptor]
        public double APropertyWithoutABackingField
        {
            get => StaticProperty;
            set => StaticProperty = value;
        }

        [InterceptorWithSyncRoot]
        public int Property_With_SyncRoot { get; set; }

        [InterceptorWithoutInstance]
        [InterceptorWithSyncRoot]
        public int Property_With_SyncRoot_And_Local_Intercptor { get; set; }

        [InterceptorWithoutInstance]
        public int Property_With_Local_Interceptor { get; set; }

        [InterceptorWithoutInstance]
        public static int Static_Property_With_Local_Interceptor { get; set; }

        [TestPropertyInterceptor]
        public List<long> APropertyWithoutBackingField { get => this.Blub(); }

        [TestPropertyInterceptor]
        public List<long> APropertyWithoutBackingFieldAndGetter { set { } }

        [TestPropertyInterceptor]
        public double APropertyWithoutGetter { set => StaticProperty = value; }

        [TestPropertyInterceptor]
        public String APropertyWithoutGetter2 { set { this._APropertyWithoutGetter = value; } }

        [TestPropertyInterceptor]
        public double APropertyWithoutSetter => 40.0;

        [TestPropertyInterceptor]
        public String APropertyWithoutSetter2 { get; }

        [TestPropertyInterceptor]
        public long[] ArrayProperty { get; set; }

        [TestPropertyInterceptor]
        public DisposableTestClass DisposablePropertyWithoutBackingField
        {
            set { }
            get { return null; }
        }

        [TestPropertyInterceptor]
        public ITestInterface InterfaceProperty { get; set; }

        [TestPropertyInterceptor]
        public List<long> ListProperty { get; set; }

        [ExternalLockablePropertyInterceptor]
        public string LockableProperty { get; set; }

        [EnumPropertyInterceptor]
        public TestEnum PropertyWithEnumValue { get; private set; }

        [TestPropertyInterceptor]
        [ExternalLockablePropertyInterceptor]
        [EnumPropertyInterceptor]
        public int PropertyWithMultipleInterceptors { get; set; }

        [TestPropertyInterceptor]
        public string ThisIsMyProperty
        {
            get
            {
                if (this._myCustomBackingField == null)
                    this._myCustomBackingField = "NULL";

                return this._myCustomBackingField;
            }
            set
            {
                if (this._myCustomBackingField == value)
                    return;

                this._myCustomBackingField = value;
            }
        }

        [TestPropertyInterceptor]
        public long ValueTypeProperty { get; set; }

        [TestPropertyInterceptor]
        public long ValueTypePropertyPrivateSetter { get; private set; }

        [TestPropertyInterceptor]
        public int? NullableProperty { get; set; }

        [TestMethod]
        public void Array_Property_Setter()
        {
            this.ArrayProperty = new long[0];

            Assert.AreEqual(4, this.ArrayProperty.Length);
            Assert.AreEqual(5643, this.ArrayProperty[2]);
        }

        [TestMethod]
        public void EnumProperty_Property_Getter()
        {
            this.PropertyWithEnumValue = (TestEnum)20;
            Assert.AreEqual((TestEnum)45, this.PropertyWithEnumValue);

            this.PropertyWithEnumValue = (TestEnum)5;
            Assert.AreEqual(TestEnum.Two, this.PropertyWithEnumValue);

            this.PropertyWithEnumValue = (TestEnum)12;
            Assert.AreEqual((TestEnum)232, this.PropertyWithEnumValue);
        }

        [TestMethod]
        public void EnumProperty_Property_Setter()
        {
            try
            {
                this.PropertyWithEnumValue = TestEnum.Three;
                this.PropertyWithEnumValue = TestEnum.Three;

                Assert.IsTrue(false);
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void List_Property_Setter()
        {
            this.ListProperty = new List<long>();

            Assert.AreEqual(4, this.ListProperty.Count);
            Assert.AreEqual(5643, this.ListProperty[2]);
        }

        [TestMethod]
        public void LockableProperties()
        {
            this.LockableProperty = "Hello";
            StaticLockableProperty = "Computer";
            Assert.AreEqual("Hello", this.LockableProperty);
            Assert.AreEqual("Computer", StaticLockableProperty);
        }

        [TestMethod]
        public void Non_Auto_Property_Not_Broken_Test()
        {
            Assert.AreEqual("NULL", this.ThisIsMyProperty);

            this.ThisIsMyProperty = "bla";
            Assert.AreEqual("bla", this.ThisIsMyProperty);
        }

        [TestMethod]
        public void Property_With_Multiple_interceptors()
        {
            this.PropertyWithMultipleInterceptors = 2;
        }

        [TestMethod]
        public void Static_Property()
        {
            StaticProperty = 4.6;
            Assert.AreEqual(4.6, StaticProperty);

            StaticProperty = 66;
            Assert.AreEqual(78344.796875, StaticProperty);
        }

        [TestMethod]
        public void ValueType_Property()
        {
            this.ValueTypeProperty = 50;
            Assert.AreEqual(50, this.ValueTypeProperty);

            this.ValueTypeProperty = 30;
            Assert.AreEqual(9999, this.ValueTypeProperty);
        }

        [TestMethod]
        public void Nullable_Property()
        {
            this.NullableProperty = 113;
            Assert.IsNull(this.NullableProperty);

            this.NullableProperty = 121;
            Assert.AreEqual(121, this.NullableProperty);

            this.NullableProperty = null;
            Assert.IsNull(this.NullableProperty);
        }

        private List<long> Blub()
        {
            return new List<long>();
        }

        private void OnAssignMethodPropertyTester()
        {
        }
    }
}