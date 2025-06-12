using ProtoScript.Interpretter;
using Ontology.Simulation;
using System.Reflection;

namespace Ontology.Tests
{
    [TestClass]
    public sealed class ReflectionUtilTests
    {
        [TestInitialize]
        public void Init()
        {
            Initializer.Initialize();
        }

        private static class OverloadTarget
        {
            public static string Echo(int v) => "int";
            public static string Echo(string v) => "string";
            public static string Echo(object v) => "object";

            public static string Add(int a, int b) => "int-int";
            public static string Add(object a, object b) => "obj-obj";
        }

        [TestMethod]
        public void GetMethod_PicksStringOverObject()
        {
            MethodInfo? info = ReflectionUtil.GetMethod(
                typeof(OverloadTarget),
                "Echo",
                new List<Type> { typeof(string) });
            Assert.IsNotNull(info);
            Assert.AreEqual(typeof(string), info.GetParameters()[0].ParameterType);
        }

        [TestMethod]
        public void GetMethod_PicksIntOverObjectWhenUsingWrapper()
        {
            MethodInfo? info = ReflectionUtil.GetMethod(
                typeof(OverloadTarget),
                "Echo",
                new List<Type> { typeof(IntWrapper) });
            Assert.IsNotNull(info);
            Assert.AreEqual(typeof(int), info.GetParameters()[0].ParameterType);
        }

        [TestMethod]
        public void GetMethod_HandlesMultipleParameters()
        {
            MethodInfo? info = ReflectionUtil.GetMethod(
                typeof(OverloadTarget),
                "Add",
                new List<Type> { typeof(IntWrapper), typeof(IntWrapper) });
            Assert.IsNotNull(info);
            ParameterInfo[] p = info.GetParameters();
            Assert.AreEqual(typeof(int), p[0].ParameterType);
            Assert.AreEqual(typeof(int), p[1].ParameterType);
        }
    }
}
