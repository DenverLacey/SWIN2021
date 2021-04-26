using System;

namespace SemesterTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Book dune  = new Book("Dune", "Frank Herbert", "978");
            Book _1984 = new Book("1984", "George Orwell", "118");
            _1984.OnLoan = true;

            Game braid = new Game("Braid", "Thekla", "9/10");
            Game doom  = new Game("Doom", "id", "8/10");
            doom.OnLoan = true;

            Library lib = new Library("The Library");
            lib.AddResource(dune);
            lib.AddResource(_1984);
            lib.AddResource(braid);
            lib.AddResource(doom);

            bool hasDune = lib.HasResource("Dune");
            Console.WriteLine(hasDune);

            bool hasDoom = lib.HasResource("Doom");
            Console.WriteLine(hasDoom);
        }
    }
}
