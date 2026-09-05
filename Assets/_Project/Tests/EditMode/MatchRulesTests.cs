using NUnit.Framework;
using UnityEngine;
using StreetLegends.Gameplay.Match;
using StreetLegends.Data.Match;

namespace StreetLegends.Tests.EditMode
{
    public class MatchRulesTests
    {
        [Test]
        public void DetermineResult_PlayerHigherScore_ReturnsPlayerWin()
        {
            Assert.AreEqual(MatchResult.PlayerWin, MatchRules.DetermineResult(3, 1));
        }

        [Test]
        public void DetermineResult_AiHigherScore_ReturnsAiWin()
        {
            Assert.AreEqual(MatchResult.AiWin, MatchRules.DetermineResult(0, 2));
        }

        [Test]
        public void DetermineResult_EqualScore_ReturnsDraw()
        {
            Assert.AreEqual(MatchResult.Draw, MatchRules.DetermineResult(2, 2));
        }

        [Test]
        public void IsScoreCapReached_MaxGoalsZero_ReturnsFalse()
        {
            var config = ScriptableObject.CreateInstance<MatchConfig>();
            config.maxGoals = 0;
            Assert.IsFalse(MatchRules.IsScoreCapReached(5, 3, config));
        }

        [Test]
        public void IsScoreCapReached_PlayerAtCap_ReturnsTrue()
        {
            var config = ScriptableObject.CreateInstance<MatchConfig>();
            config.maxGoals = 5;
            Assert.IsTrue(MatchRules.IsScoreCapReached(5, 2, config));
        }

        [Test]
        public void IsScoreCapReached_AiAtCap_ReturnsTrue()
        {
            var config = ScriptableObject.CreateInstance<MatchConfig>();
            config.maxGoals = 5;
            Assert.IsTrue(MatchRules.IsScoreCapReached(2, 5, config));
        }

        [Test]
        public void ShouldEnterOvertime_NonePolicy_ReturnsFalse()
        {
            var config = ScriptableObject.CreateInstance<MatchConfig>();
            config.overtimePolicy = OvertimePolicy.None;
            Assert.IsFalse(MatchRules.ShouldEnterOvertime(1, 1, config));
        }

        [Test]
        public void ShouldEnterOvertime_GoldenGoalAndDraw_ReturnsTrue()
        {
            var config = ScriptableObject.CreateInstance<MatchConfig>();
            config.overtimePolicy = OvertimePolicy.GoldenGoal;
            Assert.IsTrue(MatchRules.ShouldEnterOvertime(1, 1, config));
        }

        [Test]
        public void ShouldEnterOvertime_NotDraw_ReturnsFalse()
        {
            var config = ScriptableObject.CreateInstance<MatchConfig>();
            config.overtimePolicy = OvertimePolicy.GoldenGoal;
            Assert.IsFalse(MatchRules.ShouldEnterOvertime(2, 1, config));
        }

        [Test]
        public void GetOvertimeDuration_ExtraTime_ReturnsDuration()
        {
            var config = ScriptableObject.CreateInstance<MatchConfig>();
            config.overtimePolicy = OvertimePolicy.ExtraTime;
            config.overtimeDurationSeconds = 60f;
            Assert.AreEqual(60f, MatchRules.GetOvertimeDuration(config));
        }

        [Test]
        public void GetOvertimeDuration_GoldenGoal_ReturnsZero()
        {
            var config = ScriptableObject.CreateInstance<MatchConfig>();
            config.overtimePolicy = OvertimePolicy.GoldenGoal;
            config.overtimeDurationSeconds = 60f;
            Assert.AreEqual(0f, MatchRules.GetOvertimeDuration(config));
        }
    }
}
