using System;
using System.Collections.Generic;

using SwinAdventure;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class InventoryTests
    {
        [Test]
        public void TestFindItem()
        {
            var inventory = new Inventory();
            inventory.Put(new Item(new string[] { "id1" }, "item1", "an item"));
            bool r = inventory.HasItem("id1");
            Assert.IsTrue(r);
        }

        [Test]
        public void TestNoItemFind()
        {
            var inventory = new Inventory();
            bool r = inventory.HasItem("no item");
            Assert.IsFalse(r);
        }

        [Test]
        public void TestFetchItem()
        {
            var inventory = new Inventory();
            inventory.Put(new Item(new string[] { "id2" }, "item2", "an item"));
            int count = inventory.ItemList.Count;

            Item item1 = inventory.Fetch("id2");
            Assert.NotNull(item1);

            Assert.AreEqual(count, inventory.ItemList.Count);
        }

        [Test]
        public void TestTakeItem()
        {
            var inventory = new Inventory();
            inventory.Put(new Item(new string[] { "id3" }, "item3", "an item"));
            int count = inventory.ItemList.Count;

            Item item1 = inventory.Take("id3");
            Assert.NotNull(item1);

            Assert.AreEqual(count - 1, inventory.ItemList.Count);
        }

        [Test]
        public void TestItemList()
        {
            var inventory = new Inventory();
            inventory.Put(new Item(new string[] { "id4" }, "item4", "an item"));

            List<Item> itemList = inventory.ItemList;

            foreach (var item in itemList)
            {
                StringAssert.AreEqualIgnoringCase("\titem4", item.Name);
                StringAssert.AreEqualIgnoringCase("id4", item.FirstId);
                StringAssert.AreEqualIgnoringCase("an item", item.FullDescription);
            }
        }
    }
}
