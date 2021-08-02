using System;

namespace SemesterTest
{
    public class Game : LibraryResource
    {
        private string contentRating;

        public string ContentRating { get => contentRating; }

        public Game(string name, string creator, string contentRating)
            : base(name, creator)
        {
            this.contentRating = contentRating;
        }
    }
}
