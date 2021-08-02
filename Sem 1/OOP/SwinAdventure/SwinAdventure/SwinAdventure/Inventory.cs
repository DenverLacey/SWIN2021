using System;
using System.Collections.Generic;

namespace SwinAdventure
{
    public class Inventory
    {
        List<Item> items;

        public List<Item> ItemList
        {
            get
            {
                var itemList = new List<Item>(items.Count);

                foreach (var item in items)
                {
                    var copy = new Item(new string[] { item.FirstId }, string.Format("\t{0}", item.Name), item.FullDescription);
                    itemList.Add(copy);
                }

                return itemList;
            }
        }

        public Inventory()
        {
            items = new List<Item>();
        }

        public bool HasItem(string id)
        {
            foreach (var item in items)
            {
                if (item.AreYou(id))
                {
                    return true;
                }
            }
            return false;
        }

        public void Put(Item item)
        {
            items.Add(item);
        }

        public Item Fetch(string id)
        {
            return items.Find(i => i.AreYou(id));
        }

        public Item Take(string id)
        {
            Item found = Fetch(id);
            items.Remove(found);
            return found;
        }
    }
}
