using System;
using NUnit.Framework;
using SemesterTest;

namespace Tests
{
    [TestFixture]
    public class LibraryResourceTests
    {
        Book book;
        Game game;

        [SetUp]
        public void SetUp()
        {
            book = new Book("Dune", "Frank Herbert", "978");
            game = new Game("Braid", "Thekla", "9/10");
        }

        [Test]
        public void TestBookName()
        {
            StringAssert.AreEqualIgnoringCase("Dune", book.Name);
        }

        [Test]
        public void TestBookCreator()
        {
            StringAssert.AreEqualIgnoringCase("Frank Herbert", book.Creator);
        }

        [Test]
        public void TestBookISBN()
        {
            StringAssert.AreEqualIgnoringCase("978", book.ISBN);
        }

        [Test]
        public void TestBookOnLoad()
        {
            Assert.IsFalse(book.OnLoan);
        }

        [Test]
        public void TestGameName()
        {
            StringAssert.AreEqualIgnoringCase("Braid", game.Name);
        }

        [Test]
        public void TestGameCreator()
        {
            StringAssert.AreEqualIgnoringCase("Thekla", game.Creator);
        }

        [Test]
        public void TestGameContentRating()
        {
            StringAssert.AreEqualIgnoringCase("9/10", game.ContentRating);
        }

        [Test]
        public void TestGameOnLoad()
        {
            Assert.IsFalse(game.OnLoan);
        }
    }
}
