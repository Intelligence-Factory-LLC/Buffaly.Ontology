using ProtoScript.Interpretter;

namespace Ontology.Tests
{
    [TestClass]
    public sealed class ReflectionUtilTests
    {
        private interface IBase { }
        private interface IDerived : IBase { }
        private class BaseClass { }
        private class DerivedClass : BaseClass { }
        private class InterfaceImplementation : IBase { }

        [TestMethod]
        public void HasBaseType_ClassInheritance()
        {
            Assert.IsTrue(ReflectionUtil.HasBaseType(typeof(DerivedClass), typeof(BaseClass)));
            Assert.IsFalse(ReflectionUtil.HasBaseType(typeof(BaseClass), typeof(DerivedClass)));
        }

        [TestMethod]
        public void HasBaseType_InterfaceInheritance()
        {
            Assert.IsTrue(ReflectionUtil.HasBaseType(typeof(IDerived), typeof(IBase)));
            Assert.IsTrue(ReflectionUtil.HasBaseType(typeof(InterfaceImplementation), typeof(IBase)));
        }

        [TestMethod]
        public void HasBaseType_NullTarget()
        {
            Assert.IsFalse(ReflectionUtil.HasBaseType(null, typeof(IBase)));
        }
    }
}
