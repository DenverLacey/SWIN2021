using System;
namespace ClockNamspace
{
    public class Counter
    {
        private int count;
        private string name;

        public int Ticks { get => count; }
        public string Name { get => name; set => name = value; }

        public Counter(string name)
        {
            this.name = name;
            count = 0;
        }

        public Counter(string name, int count)
            : this(name)
        {
            this.count = count;
        }

        public void Increment()
        {
            count++;
        }

        public void Reset()
        {
            count = 0;
        }
    }
}
