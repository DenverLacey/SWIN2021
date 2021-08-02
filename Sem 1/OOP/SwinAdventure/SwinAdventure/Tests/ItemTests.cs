using System;
using SwinAdventure;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ItemTests
    {
        private Item item;

        [SetUp]
        public void SetUp()
        {
            item = new Item(new string[]{ "sword" }, "An iron sword", "A two handed iron sword");
        }

        [Test]
        public void TestItemIsIdentifiable()
        {
            bool r = item.AreYou("sword");
            Assert.IsTrue(r);
        }

        [Test]
        public void TestShortDescription()
        {
            string desc = item.ShortDescription;
            StringAssert.AreEqualIgnoringCase("An iron sword (sword)", desc);
        }

        [Test]
        public void TestFullDescription()
        {
            string desc = item.FullDescription;
            StringAssert.AreEqualIgnoringCase("A two handed iron sword", desc);
        }
    }
}
