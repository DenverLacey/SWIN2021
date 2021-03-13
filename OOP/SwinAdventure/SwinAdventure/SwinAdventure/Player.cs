using System;
using System.Text;

namespace SwinAdventure
{
    public class Player : GameObject, IHasInventory
    {
        private Inventory inventory;

        public Inventory Inventory { get => inventory; }

        public override string FullDescription
        {
            get
            {
                var builder = new StringBuilder();

                builder.AppendLine("You are carrying:");

                foreach (var item in inventory.ItemList)
                {
                    builder.AppendLine(item.ShortDescription);
                }

                return builder.ToString();
            }
        }

        public Player(string name, string description)
            : base(new string[]{ "me", "inventory" }, name, description)
        {
            inventory = new Inventory();
        }

        public GameObject Locate(string id)
        {
            if (AreYou(id))
            {
                return this;
            }

            return inventory.Fetch(id);
        }
    }
}
