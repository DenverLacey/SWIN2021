using System;
using SwinAdventure;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class IdentifiableObjectTests
    {
        IdentifiableObject obj;

        [SetUp]
        public void SetUp()
        {
            obj = new IdentifiableObject(new string[] { "id1", "id2" });
        }

        [Test]
        public void TestAreYou()
        {
            bool r1 = obj.AreYou("id1");
            Assert.IsTrue(r1);

            bool r2 = obj.AreYou("id2");
            Assert.IsTrue(r2);
        }

        [Test]
        public void TestNotAreYou()
        {
            bool r = obj.AreYou("identifier");
            Assert.IsFalse(r);
        }

        [Test]
        public void TestCaseInsensitive()
        {
            bool r = obj.AreYou("ID1");
            Assert.IsTrue(r);
        }

        [Test]
        public void TestFirstId()
        {
            string firstId = obj.FirstId;
            StringAssert.AreEqualIgnoringCase("id1", firstId);
        }

        [Test]
        public void TestAddIdentifier()
        {
            obj.AddIdentifier("id3");
            bool r = obj.AreYou("id3");
            Assert.IsTrue(r);
        }
    }
}
