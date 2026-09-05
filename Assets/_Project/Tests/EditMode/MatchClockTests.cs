using NUnit.Framework;
using StreetLegends.Gameplay.Match;

namespace StreetLegends.Tests.EditMode
{
    public class MatchClockTests
    {
        [Test]
        public void StartCountdown_SetsTimeAndRunning()
        {
            var clock = new MatchClock();
            clock.StartCountdown(3f);
            Assert.AreEqual(3f, clock.CountdownTime);
            Assert.IsTrue(clock.IsRunning);
        }

        [Test]
        public void TickCountdown_DecreasesTime()
        {
            var clock = new MatchClock();
            clock.StartCountdown(3f);
            clock.TickCountdown(1f);
            Assert.AreEqual(2f, clock.CountdownTime);
            Assert.IsTrue(clock.IsRunning);
        }

        [Test]
        public void TickCountdown_Complete_StopsRunning()
        {
            var clock = new MatchClock();
            clock.StartCountdown(1f);
            clock.TickCountdown(1f);
            Assert.IsTrue(clock.IsCountdownComplete);
            Assert.IsFalse(clock.IsRunning);
        }

        [Test]
        public void StartMatch_SetsTimeAndRunning()
        {
            var clock = new MatchClock();
            clock.StartMatch(120f);
            Assert.AreEqual(120f, clock.RemainingTime);
            Assert.IsTrue(clock.IsRunning);
        }

        [Test]
        public void Tick_DecreasesRemainingTime()
        {
            var clock = new MatchClock();
            clock.StartMatch(10f);
            clock.Tick(3f);
            Assert.AreEqual(7f, clock.RemainingTime);
        }

        [Test]
        public void Tick_TimeUp_StopsRunning()
        {
            var clock = new MatchClock();
            clock.StartMatch(5f);
            clock.Tick(5f);
            Assert.IsTrue(clock.IsTimeUp);
            Assert.IsFalse(clock.IsRunning);
        }

        [Test]
        public void Tick_DoesNotGoNegative()
        {
            var clock = new MatchClock();
            clock.StartMatch(5f);
            clock.Tick(10f);
            Assert.AreEqual(0f, clock.RemainingTime);
        }

        [Test]
        public void Stop_StopsRunning()
        {
            var clock = new MatchClock();
            clock.StartMatch(10f);
            clock.Stop();
            Assert.IsFalse(clock.IsRunning);
        }

        [Test]
        public void Reset_ClearsAllState()
        {
            var clock = new MatchClock();
            clock.StartMatch(10f);
            clock.StartCountdown(3f);
            clock.Reset();
            Assert.AreEqual(0f, clock.RemainingTime);
            Assert.AreEqual(0f, clock.CountdownTime);
            Assert.IsFalse(clock.IsRunning);
        }
    }
}
