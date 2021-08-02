using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SemesterTest;

namespace Tests
{
    [TestFixture]
    public class LibraryTests
    {
        Library lib;
        List<LibraryResource> resources;
        static BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        [SetUp]
        public void SetUp()
        {
            lib = new Library("The Library");
            resources = lib.GetType().GetField("resources", bindingFlags).GetValue(lib) as List<LibraryResource>;
        }

        [Test]
        public void TestAddResource()
        {
            Game game = new Game("Braid", "Thekla", "9/10");
            lib.AddResource(game);

            Assert.AreEqual(1, resources.Count);
            int index = resources.IndexOf(game);
            Assert.AreEqual(0, index);
        }

        [Test]
        public void TestHasResource()
        {
            Game game = new Game("Doom", "id", "8/10");
            lib.AddResource(game);

            bool hasDoom = lib.HasResource("Doom");
            Assert.IsTrue(hasDoom);
        }

        [Test]
        public void TestHasResourceOnLoan()
        {
            Book book = new Book("1984", "George Orwell", "118");
            book.OnLoan = true;

            bool has1984 = lib.HasResource("1984");
            Assert.IsFalse(has1984);
        }

        [Test]
        public void TestNotHasResource()
        {
            bool hasResource = lib.HasResource("phantasm");
            Assert.IsFalse(hasResource);
        }
    }
}
