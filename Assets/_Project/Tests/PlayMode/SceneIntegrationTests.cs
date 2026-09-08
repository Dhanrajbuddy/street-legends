using System.Collections;
using System.Reflection;
using NUnit.Framework;
using StreetLegends.Core;
using StreetLegends.Gameplay.Ball;
using StreetLegends.Gameplay.Character;
using StreetLegends.Gameplay.Court;
using StreetLegends.Gameplay.Match;
using StreetLegends.Presentation.Hud;
using StreetLegends.Presentation.Screens;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StreetLegends.Tests.PlayMode
{
    /// <summary>
    /// Loads the real build scenes and asserts the wiring the Android player depends on.
    /// Any Debug.LogError during load (e.g. unassigned EnvironmentConfig) fails the test.
    /// </summary>
    public class SceneIntegrationTests
    {
        [UnityTest]
        public IEnumerator BootScene_LoadsMainMenu_WithoutErrors()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return null;
            yield return null;
            yield return null;

            Assert.AreEqual(SceneNames.MainMenu, SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator MainMenuScene_HasTitlePlayButtonAndController()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.MainMenu, LoadSceneMode.Single);
            yield return null;

            Assert.IsNotNull(Camera.main, "Main Camera missing");
            Assert.IsNotNull(Object.FindFirstObjectByType<EventSystem>(), "EventSystem missing");
            Assert.IsNotNull(Object.FindFirstObjectByType<Canvas>(), "Canvas missing");

            var controller = Object.FindFirstObjectByType<MainMenuController>();
            Assert.IsNotNull(controller, "MainMenuController missing");
            var playButton = GetPrivate<Button>(controller, "playButton");
            Assert.IsNotNull(playButton, "playButton not wired");
            Assert.IsTrue(playButton.interactable && playButton.gameObject.activeInHierarchy);
            Assert.AreEqual("PLAY", playButton.GetComponentInChildren<Text>().text);

            bool titleFound = false;
            foreach (var t in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
            {
                Assert.IsNotNull(t.font, $"Text '{t.name}' has no font");
                if (t.text == "STREET LEGENDS") titleFound = true;
            }
            Assert.IsTrue(titleFound, "Title text missing");
        }

        [UnityTest]
        public IEnumerator MatchScene_HasAllGameplayObjectsAndWiring()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Match, LoadSceneMode.Single);
            yield return null;

            Assert.IsNotNull(Camera.main, "Main Camera missing");
            Assert.IsNotNull(Object.FindFirstObjectByType<Light>(), "Light missing");
            Assert.IsNotNull(Object.FindFirstObjectByType<EventSystem>(), "EventSystem missing");

            var court = Object.FindFirstObjectByType<CourtBehaviour>();
            Assert.IsNotNull(court, "Court missing");
            Assert.IsNotNull(court.transform.Find("Floor"), "Floor missing");
            foreach (string wall in new[] { "WallNorth", "WallSouth", "WallEast", "WallWest" })
                Assert.IsNotNull(court.transform.Find(wall), $"{wall} missing");

            var goals = Object.FindObjectsByType<GoalTrigger>(FindObjectsSortMode.None);
            Assert.AreEqual(2, goals.Length, "Expected 2 goal triggers");

            var ball = Object.FindFirstObjectByType<BallBehaviour>();
            Assert.IsNotNull(ball, "Ball missing");
            Assert.IsTrue(ball.CompareTag("Ball"), "Ball is not tagged 'Ball' (goals would never register)");
            Assert.IsNotNull(ball.GetComponent<Rigidbody>(), "Ball Rigidbody missing");

            var characters = Object.FindObjectsByType<CharacterBehaviour>(FindObjectsSortMode.None);
            Assert.AreEqual(2, characters.Length, "Expected Player + AI");
            foreach (var c in characters)
            {
                Assert.IsNotNull(GetPrivate<Object>(c, "definition"), $"{c.name} definition not wired");
                Assert.IsNotNull(GetPrivate<Object>(c, "inputSourceBehaviour"), $"{c.name} input not wired");
            }

            var match = Object.FindFirstObjectByType<MatchController>();
            Assert.IsNotNull(match, "MatchController missing");
            foreach (string f in new[] { "matchConfig", "aiDifficultyConfig", "court", "ball", "player", "ai", "playerGoalTrigger", "aiGoalTrigger", "aiInputSource" })
                Assert.IsNotNull(GetPrivate<Object>(match, f), $"MatchController.{f} not wired");

            var hud = Object.FindFirstObjectByType<MatchHud>();
            Assert.IsNotNull(hud, "MatchHud missing");
            foreach (string f in new[] { "matchController", "scoreText", "timerText", "stateText", "resultPanel", "resultText", "restartButton", "menuButton" })
                Assert.IsNotNull(GetPrivate<Object>(hud, f), $"MatchHud.{f} not wired");

            var touch = Object.FindFirstObjectByType<TouchControls>();
            Assert.IsNotNull(touch, "TouchControls missing");
            foreach (string f in new[] { "joystick", "inputSource", "kickButton", "tackleButton", "sprintButton" })
                Assert.IsNotNull(GetPrivate<Object>(touch, f), $"TouchControls.{f} not wired");

            var joystick = Object.FindFirstObjectByType<VirtualJoystick>();
            Assert.IsNotNull(GetPrivate<Object>(joystick, "background"), "Joystick background not wired");
            Assert.IsNotNull(GetPrivate<Object>(joystick, "handle"), "Joystick handle not wired");

            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                Assert.IsNotNull(r.sharedMaterial, $"{r.name} has no material");
                Assert.IsNotNull(r.sharedMaterial.shader, $"{r.name} material has no shader");
                Assert.IsTrue(r.sharedMaterial.shader.isSupported, $"{r.name} shader '{r.sharedMaterial.shader.name}' unsupported");
            }

            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(MatchState.Countdown, match.State);
        }

        private static T GetPrivate<T>(object target, string field) where T : class
        {
            FieldInfo fi = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(fi, $"Field '{field}' not found on {target.GetType().Name}");
            return fi.GetValue(target) as T;
        }
    }
}
