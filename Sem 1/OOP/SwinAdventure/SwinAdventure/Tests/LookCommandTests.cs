using System;
using SwinAdventure;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class LookCommandTests
    {
        LookCommand cmd;

        [SetUp]
        public void SetUp()
        {
            cmd = new LookCommand();
        }

        [Test]
        public void TestLookAtMe()
        {
            var player = new Player("Me", "The player");
            player.Inventory.Put(new Item(new string[] { "gem" }, "a gem", "a shiny red gem"));

            string expected = "You are carrying:\n\ta gem (gem)\n";
            string actual = cmd.Execute(player, new string[] { "look", "at", "inventory" });

            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void TestLookAtGem()
        {
            var player = new Player("Me", "The player");
            player.Inventory.Put(new Item(new string[] { "gem" }, "a gem", "a shiny red gem"));

            string expected = "a shiny red gem";
            string actual = cmd.Execute(player, new string[] { "look", "at", "gem" });

            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void TestLookAtUnk()
        {
            var player = new Player("Me", "The player");

            string expected = "I cannot find the gem";
            string actual = cmd.Execute(player, new string[] { "look", "at", "gem" });

            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void TestLookAtGemInMe()
        {
            var player = new Player("Me", "The player");
            player.Inventory.Put(new Item(new string[] { "gem" }, "a gem", "a shiny red gem"));

            string expected = "a shiny red gem";
            string actual = cmd.Execute(player, new string[] { "look", "at", "gem", "in", "inventory" });

            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void TestLookAtGemInBag()
        {
            var player = new Player("Me", "The player");
            var bag = new Bag(new string[] { "bag" }, "bag", "a leather bag");
            bag.Inventory.Put(new Item(new string[] { "gem" }, "a gem", "a shiny red gem"));
            player.Inventory.Put(bag);

            string expected = "a shiny red gem";
            string actual = cmd.Execute(player, new string[] { "look", "at", "gem", "in", "bag" });

            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void TestLookAtGemInNoBag()
        {
            var player = new Player("Me", "The player");
            player.Inventory.Put(new Item(new string[] { "gem" }, "a gem", "a shiny red gem"));

            string expected = "I cannot find the bag";
            string actual = cmd.Execute(player, new string[] { "look", "at", "gem", "in", "bag" });

            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void TestLookAtNoGemInBag()
        {
            var player = new Player("Me", "The player");
            var bag = new Bag(new string[] { "bag" }, "bag", "a leather bag");
            player.Inventory.Put(new Item(new string[] { "gem" }, "a gem", "a shiny red gem"));
            player.Inventory.Put(bag);

            string expected = "I cannot find the gem";
            string actual = cmd.Execute(player, new string[] { "look", "at", "gem", "in", "bag" });

            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void TestInvalidLook()
        {
            var player = new Player("Me", "The player");
            player.Inventory.Put(new Item(new string[] { "gem" }, "a gem", "a shiny red gem"));

            {
                string expected = "What do you want to look at?";
                string actual = cmd.Execute(player, new string[] { "look", "around" });
                StringAssert.AreEqualIgnoringCase(expected, actual);
            }

            {
                string expected = "What do you want to look in?";
                string actual = cmd.Execute(player, new string[] { "look", "at", "gem", "inside", "bag" });
                StringAssert.AreEqualIgnoringCase(expected, actual);
            }

            {
                string expected = "error in look input";
                string actual = cmd.Execute(player, new string[] { "hello" });
                StringAssert.AreEqualIgnoringCase(expected, actual);
            }
        }
    }
}
