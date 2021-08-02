using System;
using System.Text;

namespace SwinAdventure
{
    public class Bag : Item, IHasInventory
    {
        private Inventory inventory;
        public Inventory Inventory { get => inventory; }

        public override string FullDescription
        {
            get
            {
                var builder = new StringBuilder();
                builder.AppendFormat("In the {0} you can see:\n", Name);

                foreach (var item in inventory.ItemList)
                {
                    builder.AppendLine(item.ShortDescription);
                }

                return builder.ToString();
            }
        }

        public Bag(string[] ids, string name, string description)
            : base(ids, name, description)
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
