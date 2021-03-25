using System;
namespace ClockNamspace
{
    public class Clock
    {
        private Counter seconds;
        private Counter minutes;
        private Counter hours;

        public int Seconds { get => seconds.Ticks; }
        public int Minutes { get => minutes.Ticks; }
        public int Hours   { get => hours.Ticks; }

        public Clock()
        {
            seconds = new Counter("Seconds");
            minutes = new Counter("Minutes");
            hours   = new Counter("Hours");
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

            switch (segements.Length)
            {
                case 3:
                    if (int.TryParse(segements[0], out int h))
                    {
                        hours = new Counter("Hours", h);
                        goto case 2;
                    }
                    break;
                case 2:
                    if (int.TryParse(segements[segements.Length - 2], out int m))
                    {
                        minutes = new Counter("Minutes", m);
                        goto case 1;
                    }
                    break;
                case 1:
                    if (int.TryParse(segements[segements.Length - 1], out int s))
                    {
                        seconds = new Counter("Seconds", s);
                        return;
                    }
                    break;
            }

            throw new ArgumentException("Invalid time format");
        }

        public void Tick()
        {
            seconds.Increment();

            if (seconds.Ticks >= 60)
            {
                seconds.Reset();
                minutes.Increment();
            }

            if (minutes.Ticks >= 60)
            {
                minutes.Reset();
                hours.Increment();
            }

            if (hours.Ticks >= 24)
            {
                hours.Reset();
            }
        }

        public void Reset()
        {
            seconds.Reset();
            minutes.Reset();
            hours.Reset();
        }

        public override string ToString()
        {
            string s = Seconds.ToString("00");
            string m = Minutes.ToString("00");
            string h = Hours.ToString("00");
            return string.Format("{0}:{1}:{2}", h, m, s);
        }
    }
}
