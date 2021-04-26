using System;
using System.Collections.Generic;

namespace SemesterTest
{
    public class Library
    {
        List<LibraryResource> resources;        

        public Library(string name) // Just following the UML from the task PDF
        {
            resources = new List<LibraryResource>();
        }

        public void AddResource(LibraryResource resource)
        {
            resources.Add(resource);
        }

        public bool HasResource(string name)
        {
            int index = resources.FindIndex(r => r.Name == name && !r.OnLoan);
            return index != -1;
        }
    }
}
