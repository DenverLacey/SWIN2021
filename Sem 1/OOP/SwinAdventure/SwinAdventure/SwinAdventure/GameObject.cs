using System;
namespace SwinAdventure
{
    public abstract class GameObject : IdentifiableObject
    {
        private string description;
        private string name;

        public string Name { get => name; }
        public string ShortDescription { get => string.Format("{0} ({1})", name, FirstId); }
        public virtual string FullDescription { get => description; }

        public GameObject(string[] ids, string name, string description)
            : base(ids)
        {
            this.name = name;
            this.description = description;
        }
    }
}
