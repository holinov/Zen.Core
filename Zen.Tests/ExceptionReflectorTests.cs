using System;
using NUnit.Framework;

namespace Zen.Tests
{
    [TestFixture]
    public class ExceptionReflectorTests
    {
        public enum TestEnum
        {
            One,
            Two
        }

        public class TestException : Exception
        {
            private string[] _arr = new[] {"s1", "s2"};

            public TestException(string msg) : base(msg)
            {
            }

            public TestEnum Enum { get; set; }

            public string[] Arr
            {
                get { return _arr; }
                set { _arr = value; }
            }
        }

        [Test]
        public void ExceptionReflectorTest()
        {
            var ex = new ApplicationException("ex1", new TestException("ex2"));
            var refl = new ExceptionReflector(ex);
            Assert.IsTrue(refl.ReflectedText.Contains("ex1"));
            Assert.IsTrue(refl.ReflectedText.Contains("ex2"));
        }
    }
}