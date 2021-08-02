using System;
using ClockNamspace;
using NUnit.Framework;
namespace Tests
{
    [TestFixture]
    public class ClockTests
    {
        [Test]
        public void TestInit()
        {
            var clock = new Clock();
            int expected = 0;
            int actual = clock.Seconds;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestTickOnce()
        {
            var clock = new Clock();
            int old = clock.Seconds;
            int expected = old + 1;

            clock.Tick();

            int actual = clock.Seconds;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestTickMany()
        {
            var clock = new Clock();
            int old = clock.Seconds;
            const int ticks = 30;
            int expected = old + ticks;

            for (int i = 0; i < ticks; i++)
            {
                clock.Tick();
            }

            int actual = clock.Seconds;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestReset()
        {
            var clock = new Clock();
            for (int i = 0; i < 10; i++)
            {
                clock.Tick();
            }

            clock.Reset();

            Assert.AreEqual(0, clock.Seconds);
        }

        [Test]
        public void TestFromString()
        {
            var clock = new Clock("3:17:32");

            Assert.AreEqual(32, clock.Seconds);
            Assert.AreEqual(17, clock.Minutes);
            Assert.AreEqual(3, clock.Hours);
        }
    }
}
