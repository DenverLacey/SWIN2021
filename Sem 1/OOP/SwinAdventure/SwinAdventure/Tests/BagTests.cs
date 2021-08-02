using System;
using SwinAdventure;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class BagTests
    {
        [Test]
        public void TestBagLocatesItem()
        {
            var bag = new Bag(new string[] { "bag" }, "a bag", "a bag");
            bag.Inventory.Put(new Item(new string[] { "id1" }, "item1", "an item"));

            int count = bag.Inventory.ItemList.Count;

            GameObject actual = bag.Locate("id1");
            Assert.NotNull(actual);
            StringAssert.AreEqualIgnoringCase("id1", actual.FirstId);

            Assert.AreEqual(count, bag.Inventory.ItemList.Count);
        }

        [Test]
        public void TestBagLocatesItself()
        {
            var bag = new Bag(new string[] { "bag" }, "a bag", "a bag");
            GameObject obj = bag.Locate("bag");
            Assert.AreEqual(bag, obj);
        }

        [Test]
        public void TestBagLocatesNothing()
        {
            var bag = new Bag(new string[] { "bag" }, "a bag", "a bag");
            GameObject obj = bag.Locate("no item");
            Assert.IsNull(obj);
        }

        [Test]
        public void TestBagFullDescription()
        {
            var bag = new Bag(new string[] { "bag" }, "bag", "a bag");
            bag.Inventory.Put(new Item(new string[] { "id1" }, "item1", "an item"));

            string expected = "In the bag you can see:\n\titem1 (id1)\n";
            string actual = bag.FullDescription;
            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void TestBagInBag()
        {
            var b1 = new Bag(new string[] { "b1" }, "bag 1", "the first bag");
            var b2 = new Bag(new string[] { "b2" }, "bag 2", "the second bag");
            b1.Inventory.Put(b2);
            b1.Inventory.Put(new Item(new string[] { "id1" }, "item1", "an item"));
            b2.Inventory.Put(new Item(new string[] { "id2" }, "item2", "an item"));

            // b1 can locate b2?
            {
                var obj = b1.Locate("b2");
                Assert.AreEqual(b2, obj);
            }

            // b1 can locate other items
            {
                var obj = b1.Locate("id1");
                Assert.NotNull(obj);
                StringAssert.AreEqualIgnoringCase("id1", obj.FirstId);
            }

            // b1 cannot locate items in b2
            {
                var obj = b1.Locate("id2");
                Assert.IsNull(obj);
            }
        }
    }
}
