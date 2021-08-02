using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var myMessage = new Message("Hello World - from Message Object!");
            myMessage.Print();

            var messages = new Message[]
            {
                new Message("No. I am your father."),
                new Message("Sounds like a legend!"),
                new Message("Great name"),
                new Message("That's a silly name")
            };

            var name = Console.ReadLine();
            var lowered = name.ToLower();

            if (lowered == "luke")
            {
                messages[0].Print();
            }
            else if (lowered == "denver")
            {
                messages[1].Print();
            }
            else if (lowered == "afridi")
            {
                messages[2].Print();
            }
            else
            {
                messages[3].Print();
            }
        }
    }
}
