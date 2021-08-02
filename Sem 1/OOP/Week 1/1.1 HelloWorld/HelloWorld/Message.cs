using System;
namespace HelloWorld
{
    public class Message
    {
        private string text;

        public Message(string text)
        {
            this.text = text;
        }

        public void Print()
        {
            Console.WriteLine(text);
        }
    }
}
