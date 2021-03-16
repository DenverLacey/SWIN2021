using System;

namespace ClockNamspace
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var clock = new Clock("3:15:27");
            Console.WriteLine(clock.ToString());
        }
    }
}
