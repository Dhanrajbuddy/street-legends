using NUnit.Framework;
using UnityEngine;
using StreetLegends.Gameplay.Ai;
using StreetLegends.Data.Match;

namespace StreetLegends.Tests.EditMode
{
    public class AiBrainTests
    {
        private AiDifficultyConfig _config;
        private AiBrain _brain;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<AiDifficultyConfig>();
            _config.skillLevel = 0.5f;
            _config.decisionIntervalSeconds = 0.01f;
            _config.reactionDelaySeconds = 0f;
            _config.errorRate = 0f;
            _config.aggression = 0.6f;
            _config.defenseAwareness = 0.5f;
            _config.positionErrorMargin = 1f;
            _brain = new AiBrain(_config);
        }

        [Test]
        public void Decide_BallFreeAndClose_ReturnsChaseBall()
        {
            var state = new AiGameState
            {
                BallPosition = Vector3.forward * 2f,
                OwnPosition = Vector3.zero,
                OpponentPosition = Vector3.forward * 10f,
                OwnGoalPosition = Vector3.forward * 10f,
                OpponentGoalPosition = Vector3.back * 10f,
                BallIsFree = true,
                BallIsPossessedBySelf = false,
                BallIsPossessedByOpponent = false,
                DistanceToBall = 2f,
                OpponentDistanceToBall = 10f,
                DistanceToOwnGoal = 10f,
                DistanceToOpponentGoal = 10f,
                DistanceToOpponent = 10f
            };

            Assert.AreEqual(AiAction.ChaseBall, _brain.Decide(state));
        }

        [Test]
        public void Decide_BallPossessedBySelf_ReturnsAttackWithBall()
        {
            var state = new AiGameState
            {
                BallPosition = Vector3.zero,
                OwnPosition = Vector3.zero,
                OpponentPosition = Vector3.forward * 8f,
                OwnGoalPosition = Vector3.forward * 10f,
                OpponentGoalPosition = Vector3.back * 10f,
                BallIsFree = false,
                BallIsPossessedBySelf = true,
                BallIsPossessedByOpponent = false,
                DistanceToBall = 0f,
                OpponentDistanceToBall = 8f,
                DistanceToOwnGoal = 10f,
                DistanceToOpponentGoal = 10f,
                DistanceToOpponent = 8f
            };

            Assert.AreEqual(AiAction.AttackWithBall, _brain.Decide(state));
        }

        [Test]
        public void Decide_OpponentHasBallAndClose_ReturnsPressOrTackle()
        {
            var state = new AiGameState
            {
                BallPosition = Vector3.forward * 2f,
                OwnPosition = Vector3.zero,
                OpponentPosition = Vector3.forward * 2f,
                OwnGoalPosition = Vector3.forward * 10f,
                OpponentGoalPosition = Vector3.back * 10f,
                BallIsFree = false,
                BallIsPossessedBySelf = false,
                BallIsPossessedByOpponent = true,
                DistanceToBall = 2f,
                OpponentDistanceToBall = 0f,
                DistanceToOwnGoal = 10f,
                DistanceToOpponentGoal = 10f,
                DistanceToOpponent = 2f
            };

            AiAction action = _brain.Decide(state);
            Assert.IsTrue(action == AiAction.PressOpponent || action == AiAction.Tackle || action == AiAction.DefendGoal);
        }

        [Test]
        public void Decide_OpponentHasBallNearOwnGoal_ReturnsDefendGoal()
        {
            var state = new AiGameState
            {
                BallPosition = Vector3.forward * 8f,
                OwnPosition = Vector3.forward * 7f,
                OpponentPosition = Vector3.forward * 8f,
                OwnGoalPosition = Vector3.forward * 10f,
                OpponentGoalPosition = Vector3.back * 10f,
                BallIsFree = false,
                BallIsPossessedBySelf = false,
                BallIsPossessedByOpponent = true,
                DistanceToBall = 1f,
                OpponentDistanceToBall = 0f,
                DistanceToOwnGoal = 3f,
                DistanceToOpponentGoal = 17f,
                DistanceToOpponent = 1f
            };

            AiAction action = _brain.Decide(state);
            Assert.IsTrue(action == AiAction.DefendGoal || action == AiAction.Tackle || action == AiAction.PressOpponent);
        }

        [Test]
        public void Reset_SetsActionToIdle()
        {
            _brain.Reset();
            Assert.AreEqual(AiAction.Idle, _brain.CurrentAction);
        }
    }
}
