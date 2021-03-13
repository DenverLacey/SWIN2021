using System;
namespace SwinAdventure
{
    public class Item : GameObject
    {
        public Item(string[] ids, string name, string description)
            : base(ids, name, description)
        {
        }
    }
}
