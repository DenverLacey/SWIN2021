using System;
namespace Counter
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
