using System;

namespace SwinAdventure
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Player player = CreatePlayer();
            InitialisePlayerInventory(player);

            bool exit = false;
            var cmd = new LookCommand();
            while (!exit)
            {
                string input = Console.ReadLine();
                string[] text = input.Split(' ');

                if (text[0] == "exit")
                {
                    exit = true;
                }
                else
                {
                    string response = cmd.Execute(player, text);
                    Console.WriteLine(response);
                }
            }
        }

        static Player CreatePlayer()
        {
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();

            Console.Write("Describe yourself: ");
            string desc = Console.ReadLine();

            return new Player(name, desc);
        }

        static void InitialisePlayerInventory(Player player)
        {
            var gem = new Item(new string[] { "gem" }, "gem", "a shiny red gem");
            var key = new Item(new string[] { "key" }, "key", "a small rusted key");
            var coin = new Item(new string[] { "coin" }, "coin", "a golden coin");

            var bag = new Bag(new string[] { "bag" }, "bag", "a leather bag");
            bag.Inventory.Put(coin);

            player.Inventory.Put(gem);
            player.Inventory.Put(key);
            player.Inventory.Put(bag);
        }
    }
}
