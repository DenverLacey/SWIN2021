using System;

namespace SemesterTest
{
    public abstract class LibraryResource
    {
        private string name;
        private string creator;
        private bool onLoan;

        public virtual string Name { get => name; }
        public virtual string Creator { get => creator; }
        public virtual bool OnLoan { get => onLoan; set => onLoan = value; }

        public LibraryResource(string name, string creator)
        {
            this.name = name;
            this.creator = creator;
            onLoan = false;
        }
    }
}
