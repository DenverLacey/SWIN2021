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
            int actual = clock.Ticks;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestTickOnce()
        {
            var clock = new Clock();
            int old = clock.Ticks;
            int expected = old + 1;

            clock.Tick();

            int actual = clock.Ticks;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestTickMany()
        {
            var clock = new Clock();
            int old = clock.Ticks;
            const int ticks = 30;
            int expected = old + ticks;

            for (int i = 0; i < ticks; i++)
            {
                clock.Tick();
            }

            int actual = clock.Ticks;

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

            Assert.AreEqual(0, clock.Ticks);
        }

        [Test]
        public void TestFromString()
        {
            int expected = (3 * 60 * 60) + (17 * 60) + 32;
            var clock = new Clock("3:17:32");
            int actual = clock.Ticks;
            Assert.AreEqual(expected, actual);
        }
    }
}
