using System;

namespace SemesterTest
{
    public class Book : LibraryResource
    {
        private string isbn;

        public string ISBN { get => isbn; }

        public Book(string name, string creator, string isbn)
            : base(name, creator)
        {
            this.isbn = isbn;
        }
    }
}
