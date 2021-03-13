using System;
using SwinAdventure;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class PlayerTests
    {
        Player player;

        [SetUp]
        public void SetUp()
        {
            player = new Player("Player", "a player");
            player.Inventory.Put(new Item(new string[] { "sword" }, "An iron sword", "Two handed iron sword"));
        }

        [Test]
        public void TestPlayerIsIdentifiable()
        {
            bool r1 = player.AreYou("me");
            Assert.IsTrue(r1);

            bool r2 = player.AreYou("inventory");
            Assert.IsTrue(r2);
        }

        [Test]
        public void TestPlayerLocatesItem()
        {
            int count = player.Inventory.ItemList.Count;

            GameObject item = player.Locate("sword");
            Assert.NotNull(item);
            StringAssert.AreEqualIgnoringCase("sword", item.FirstId);

            Assert.AreEqual(count, player.Inventory.ItemList.Count);
        }

        [Test]
        public void TestPlayerLocatesItself()
        {
            GameObject i1 = player.Locate("me");
            Assert.AreEqual(player, i1);

            GameObject i2 = player.Locate("inventory");
            Assert.AreEqual(player, i2);
        }

        [Test]
        public void TestPlayerLocatesNothing()
        {
            GameObject item = player.Locate("no item");
            Assert.IsNull(item);
        }

        [Test]
        public void TestPlayerFullDescription()
        {
            string expected = "You are carrying:\n\tAn iron sword (sword)\n";
            string actual = player.FullDescription;
            StringAssert.AreEqualIgnoringCase(expected, actual);
        }
    }
}
