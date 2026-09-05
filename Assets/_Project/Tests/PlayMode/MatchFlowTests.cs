using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using StreetLegends.Gameplay.Match;
using StreetLegends.Gameplay.Court;
using StreetLegends.Gameplay.Ball;
using StreetLegends.Gameplay.Character;
using StreetLegends.Data.Match;
using StreetLegends.Data.Characters;

namespace StreetLegends.Tests.PlayMode
{
    public class MatchFlowTests
    {
        private GameObject _courtObj;
        private GameObject _ballObj;
        private CourtBehaviour _court;
        private BallBehaviour _ball;
        private MatchConfig _matchConfig;
        private CharacterDefinition _charDef;

        [SetUp]
        public void SetUp()
        {
            _courtObj = new GameObject("Court");
            _court = _courtObj.AddComponent<CourtBehaviour>();

            _ballObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _ballObj.name = "Ball";
            _ballObj.tag = "Ball";
            _ballObj.transform.position = Vector3.zero;
            var rb = _ballObj.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezePositionY;
            _ball = _ballObj.AddComponent<BallBehaviour>();

            _matchConfig = ScriptableObject.CreateInstance<MatchConfig>();
            _matchConfig.matchDurationSeconds = 5f;
            _matchConfig.countdownDurationSeconds = 1f;
            _matchConfig.goalCelebrationDurationSeconds = 0.5f;
            _matchConfig.maxGoals = 0;
            _matchConfig.overtimePolicy = OvertimePolicy.None;

            _charDef = ScriptableObject.CreateInstance<CharacterDefinition>();
            _charDef.moveSpeed = 6f;
            _charDef.sprintSpeed = 9f;
            _charDef.kickPower = 12f;
            _charDef.kickRange = 1.5f;
            _charDef.ballControlRange = 2f;
            _charDef.tackleRange = 1.8f;
            _charDef.tackleCooldown = 2f;
            _charDef.tackleDuration = 0.4f;
            _charDef.rotationSpeed = 720f;
            _charDef.acceleration = 30f;
            _charDef.deceleration = 40f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_courtObj);
            Object.DestroyImmediate(_ballObj);
            Object.DestroyImmediate(_matchConfig);
            Object.DestroyImmediate(_charDef);
        }

        [UnityTest]
        public IEnumerator MatchController_CountdownTransitionsToPlaying()
        {
            var controllerObj = new GameObject("MatchController");
            var controller = controllerObj.AddComponent<MatchController>();

            SetPrivateField(controller, "matchConfig", _matchConfig);
            SetPrivateField(controller, "court", _court);
            SetPrivateField(controller, "ball", _ball);

            yield return new WaitForSeconds(2f);

            Assert.AreEqual(MatchState.Playing, controller.State);

            Object.Destroy(controllerObj);
        }

        [UnityTest]
        public IEnumerator MatchController_TimeUpNoOvertime_FinishesMatch()
        {
            var controllerObj = new GameObject("MatchController");
            var controller = controllerObj.AddComponent<MatchController>();

            SetPrivateField(controller, "matchConfig", _matchConfig);
            SetPrivateField(controller, "court", _court);
            SetPrivateField(controller, "ball", _ball);

            yield return new WaitForSeconds(7f);

            Assert.AreEqual(MatchState.Finished, controller.State);
            Assert.AreEqual(MatchResult.Draw, controller.Result);

            Object.Destroy(controllerObj);
        }

        [UnityTest]
        public IEnumerator ScoreBoard_PlayerGoal_IncrementsPlayerScore()
        {
            var scoreboard = new ScoreBoard();
            scoreboard.PlayerGoal();
            Assert.AreEqual(1, scoreboard.PlayerScore);
            Assert.AreEqual(0, scoreboard.AiScore);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ScoreBoard_AiGoal_IncrementsAiScore()
        {
            var scoreboard = new ScoreBoard();
            scoreboard.AiGoal();
            Assert.AreEqual(0, scoreboard.PlayerScore);
            Assert.AreEqual(1, scoreboard.AiScore);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ScoreBoard_Reset_ClearsScores()
        {
            var scoreboard = new ScoreBoard();
            scoreboard.PlayerGoal();
            scoreboard.AiGoal();
            scoreboard.AiGoal();
            scoreboard.Reset();
            Assert.AreEqual(0, scoreboard.PlayerScore);
            Assert.AreEqual(0, scoreboard.AiScore);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterMotor_MovesTowardInput()
        {
            var stats = new CharacterRuntimeStats(_charDef);
            var motor = new CharacterMotor(stats, Vector3.zero);
            var bounds = new Bounds(Vector3.zero, new Vector3(12f, 0f, 20f));

            motor.Update(new Vector2(0f, 1f), false, 0.5f, bounds);

            Assert.Greater(motor.Position.z, 0f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterMotor_ClampsToCourtBounds()
        {
            var stats = new CharacterRuntimeStats(_charDef);
            var bounds = new Bounds(Vector3.zero, new Vector3(12f, 0f, 20f));
            var motor = new CharacterMotor(stats, new Vector3(0f, 0f, 9f));

            motor.Update(new Vector2(0f, 1f), true, 1f, bounds);

            Assert.LessOrEqual(motor.Position.z, 10f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterActions_TackleThenCooldown_CannotTackleDuringCooldown()
        {
            var stats = new CharacterRuntimeStats(_charDef);
            var actions = new CharacterActions(stats);

            Assert.IsTrue(actions.TryTackle());
            Assert.IsFalse(actions.CanTackle);
            Assert.IsTrue(actions.IsTackling);

            actions.Update(0.5f);
            Assert.IsFalse(actions.IsTackling);

            actions.Update(2f);
            Assert.IsTrue(actions.CanTackle);

            yield return null;
        }

        [UnityTest]
        public IEnumerator CourtBehaviour_IsInPlayerGoalArea_DetectsCorrectly()
        {
            Assert.IsTrue(_court.IsInPlayerGoalArea(new Vector3(0f, 0f, -10f)));
            Assert.IsFalse(_court.IsInPlayerGoalArea(new Vector3(0f, 0f, 5f)));
            yield return null;
        }

        [UnityTest]
        public IEnumerator CourtBehaviour_IsInAiGoalArea_DetectsCorrectly()
        {
            Assert.IsTrue(_court.IsInAiGoalArea(new Vector3(0f, 0f, 10f)));
            Assert.IsFalse(_court.IsInAiGoalArea(new Vector3(0f, 0f, -5f)));
            yield return null;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
