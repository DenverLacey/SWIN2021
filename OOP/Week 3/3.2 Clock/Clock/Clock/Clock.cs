using System;
namespace ClockNamspace
{
    public class Clock
    {
        private Counter counter;

        public int Ticks { get => counter.Ticks; }

        public Clock()
        {
            counter = new Counter(null);
        }

        public Clock(string time)
            : this()
        {
            // split string into hours, minutes and seconds
            string[] segements = time.Split(':');

            // check that there aren't too many segments
            // 'Split()' always returns at least 1 string
            if (segements.Length > 3)
            {
                throw new ArgumentException("Invalid time format");
            }

            int units = 1;
            int ticks = 0;
            for (int i = segements.Length - 1; i >= 0; i--)
            {
                string segment = segements[i];
                bool success = int.TryParse(segment, out int value);
                if (!success)
                {
                    throw new ArgumentException("Invalid time format");
                }
                ticks += value * units;
                units *= 60;
            }

            for (int i = 0; i < ticks; i++)
            {
                counter.Increment();
            }
        }

        public void Tick()
        {
            counter.Increment();
        }

        public void Reset()
        {
            counter.Reset();
        }

        public override string ToString()
        {
            string seconds = (Ticks % 60).ToString("00");
            string minutes = (Ticks / 60 % 60).ToString("00");
            string hours = (Ticks / 60 / 60).ToString("00");
            return string.Format("{0}:{1}:{2}", hours, minutes, seconds);
        }
    }
}
