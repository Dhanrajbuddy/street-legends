using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using StreetLegends.Data.Match;
using StreetLegends.Data.Characters;
using StreetLegends.Gameplay.Match;
using StreetLegends.Gameplay.Court;
using StreetLegends.Gameplay.Ball;
using StreetLegends.Gameplay.Character;
using StreetLegends.Gameplay.Ai;
using StreetLegends.Gameplay.Input;
using StreetLegends.Presentation.Hud;
using StreetLegends.Presentation.Screens;

namespace StreetLegends.Editor.Setup
{
    public static class Phase1Setup
    {
        private const string DataMatchPath = "Assets/_Project/Data/Match";
        private const string DataCharactersPath = "Assets/_Project/Data/Characters";
        private const string PrefabGameplayPath = "Assets/_Project/Prefabs/Gameplay";

        [MenuItem("Street Legends/Phase 1/Setup Match Scene")]
        public static void Run()
        {
            CreateDataAssets();
            BuildMainMenuScene();
            BuildMatchScene();
            Debug.Log("[Phase1Setup] Complete.");
        }

        private static void CreateDataAssets()
        {
            EnsureDirectory(DataMatchPath);
            EnsureDirectory(DataCharactersPath);

            var matchConfig = CreateOrLoadAsset<MatchConfig>($"{DataMatchPath}/MatchConfig.asset");
            matchConfig.matchDurationSeconds = 120f;
            matchConfig.countdownDurationSeconds = 3f;
            matchConfig.goalCelebrationDurationSeconds = 2f;
            matchConfig.maxGoals = 0;
            matchConfig.overtimePolicy = OvertimePolicy.GoldenGoal;
            matchConfig.overtimeDurationSeconds = 60f;
            EditorUtility.SetDirty(matchConfig);

            var charDef = CreateOrLoadAsset<CharacterDefinition>($"{DataCharactersPath}/Rookie.asset");
            charDef.displayName = "Rookie";
            charDef.moveSpeed = 6f;
            charDef.sprintSpeed = 9f;
            charDef.kickPower = 12f;
            charDef.kickRange = 1.5f;
            charDef.ballControlRange = 2f;
            charDef.tackleRange = 1.8f;
            charDef.tackleCooldown = 2f;
            charDef.tackleDuration = 0.4f;
            charDef.rotationSpeed = 720f;
            charDef.acceleration = 30f;
            charDef.deceleration = 40f;
            EditorUtility.SetDirty(charDef);

            var aiEasy = CreateOrLoadAsset<AiDifficultyConfig>($"{DataMatchPath}/AiEasy.asset");
            aiEasy.skillLevel = 0.3f;
            aiEasy.decisionIntervalSeconds = 0.3f;
            aiEasy.reactionDelaySeconds = 0.2f;
            aiEasy.errorRate = 0.3f;
            aiEasy.aggression = 0.4f;
            aiEasy.defenseAwareness = 0.3f;
            aiEasy.positionErrorMargin = 2.5f;
            EditorUtility.SetDirty(aiEasy);

            var aiMedium = CreateOrLoadAsset<AiDifficultyConfig>($"{DataMatchPath}/AiMedium.asset");
            aiMedium.skillLevel = 0.5f;
            aiMedium.decisionIntervalSeconds = 0.2f;
            aiMedium.reactionDelaySeconds = 0.1f;
            aiMedium.errorRate = 0.15f;
            aiMedium.aggression = 0.6f;
            aiMedium.defenseAwareness = 0.5f;
            aiMedium.positionErrorMargin = 1.5f;
            EditorUtility.SetDirty(aiMedium);

            var aiHard = CreateOrLoadAsset<AiDifficultyConfig>($"{DataMatchPath}/AiHard.asset");
            aiHard.skillLevel = 0.8f;
            aiHard.decisionIntervalSeconds = 0.1f;
            aiHard.reactionDelaySeconds = 0.05f;
            aiHard.errorRate = 0.05f;
            aiHard.aggression = 0.8f;
            aiHard.defenseAwareness = 0.7f;
            aiHard.positionErrorMargin = 0.5f;
            EditorUtility.SetDirty(aiHard);

            AssetDatabase.SaveAssets();
        }

        private static void BuildMainMenuScene()
        {
            Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 10f, -10f);
            camObj.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            var cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();

            CreateEventSystem();

            var canvasObj = CreateCanvas("MenuCanvas");

            var titleText = CreateText(canvasObj.transform, "TitleText", "STREET LEGENDS", 72);
            SetAnchors(titleText.rectTransform, AnchorPresets.TopCenter);
            titleText.rectTransform.anchoredPosition = new Vector2(0f, -120f);
            titleText.color = Color.yellow;
            titleText.alignment = TextAnchor.MiddleCenter;

            var playButton = CreateButton(canvasObj.transform, "PlayButton", "PLAY", new Vector2(0f, 0f), new Vector2(300f, 100f));
            SetAnchors(playButton.GetComponent<RectTransform>(), AnchorPresets.MiddleCenter);

            var controllerObj = new GameObject("MainMenuController");
            var controller = controllerObj.AddComponent<MainMenuController>();
            SetPrivateField(controller, "playButton", playButton);

            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene, "Assets/_Project/Scenes/MainMenu.unity");
        }

        private static void BuildMatchScene()
        {
            EnsureDirectory(PrefabGameplayPath);

            var matchConfig = AssetDatabase.LoadAssetAtPath<MatchConfig>($"{DataMatchPath}/MatchConfig.asset");
            var charDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"{DataCharactersPath}/Rookie.asset");
            var aiConfig = AssetDatabase.LoadAssetAtPath<AiDifficultyConfig>($"{DataMatchPath}/AiMedium.asset");

            Scene matchScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject courtObj = CreateCourt();
            GameObject ballObj = CreateBall();
            TouchInputSource playerInput = null;
            GameObject playerObj = CreateCharacter("Player", charDef, isPlayer: true, out playerInput);
            GameObject aiObj = CreateCharacter("AI", charDef, isPlayer: false, out _);
            GameObject cameraObj = CreateCamera();

            CreateEventSystem();

            var canvasObj = CreateCanvas("MatchCanvas");
            MatchHud hud = CreateHud(canvasObj.transform);
            TouchControls touchControls = CreateTouchControls(canvasObj.transform, playerInput);

            GameObject matchControllerObj = CreateMatchController(matchConfig, aiConfig, courtObj, ballObj, playerObj, aiObj);

            SetPrivateField(hud, "matchController", matchControllerObj.GetComponent<MatchController>());

            EditorSceneManager.MarkSceneDirty(matchScene);
            string scenePath = "Assets/_Project/Scenes/Match.unity";
            EditorSceneManager.SaveScene(matchScene, scenePath);

            RegisterScenesInBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateCourt()
        {
            var go = new GameObject("Court");
            var court = go.AddComponent<CourtBehaviour>();

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(go.transform, false);
            floor.transform.localScale = new Vector3(1.2f, 1f, 2f);
            var floorRenderer = floor.GetComponent<Renderer>();
            if (floorRenderer != null)
            {
                floorRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = new Color(0.2f, 0.5f, 0.2f, 1f)
                };
            }

            CreateWall(go.transform, "WallNorth", new Vector3(0f, 1f, 10f), new Vector3(12f, 2f, 0.5f));
            CreateWall(go.transform, "WallSouth", new Vector3(0f, 1f, -10f), new Vector3(12f, 2f, 0.5f));
            CreateWall(go.transform, "WallEast", new Vector3(6f, 1f, 0f), new Vector3(0.5f, 2f, 20f));
            CreateWall(go.transform, "WallWest", new Vector3(-6f, 1f, 0f), new Vector3(0.5f, 2f, 20f));

            CreateGoal(go.transform, "PlayerGoal", new Vector3(0f, 0.5f, -10f), GoalSide.PlayerGoal);
            CreateGoal(go.transform, "AiGoal", new Vector3(0f, 0.5f, 10f), GoalSide.AiGoal);

            return go;
        }

        private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            var col = wall.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;
            var renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = new Color(0.3f, 0.3f, 0.3f, 1f)
                };
            }
        }

        private static void CreateGoal(Transform parent, string name, Vector3 pos, GoalSide side)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;

            var leftPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPost.name = "LeftPost";
            leftPost.transform.SetParent(go.transform, false);
            leftPost.transform.localPosition = new Vector3(-2f, 0.5f, 0f);
            leftPost.transform.localScale = new Vector3(0.2f, 1f, 0.2f);

            var rightPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPost.name = "RightPost";
            rightPost.transform.SetParent(go.transform, false);
            rightPost.transform.localPosition = new Vector3(2f, 0.5f, 0f);
            rightPost.transform.localScale = new Vector3(0.2f, 1f, 0.2f);

            var crossbar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crossbar.name = "Crossbar";
            crossbar.transform.SetParent(go.transform, false);
            crossbar.transform.localPosition = new Vector3(0f, 1f, 0f);
            crossbar.transform.localScale = new Vector3(4.2f, 0.2f, 0.2f);

            var triggerObj = new GameObject("GoalTrigger");
            triggerObj.transform.SetParent(go.transform, false);
            triggerObj.transform.localPosition = Vector3.zero;
            triggerObj.transform.localScale = new Vector3(4f, 2f, 1f);
            var triggerCol = triggerObj.AddComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            var goalTrigger = triggerObj.AddComponent<GoalTrigger>();
            SetPrivateField(goalTrigger, "side", side);

            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = Color.white
                };
            }
        }

        private static GameObject CreateBall()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Ball";
            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one * 0.5f;
            go.tag = "Ball";

            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            rb.mass = 0.4f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezePositionY;

            var col = go.GetComponent<Collider>();
            col.isTrigger = false;

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = Color.white
                };
            }

            go.AddComponent<BallBehaviour>();
            return go;
        }

        private static GameObject CreateCharacter(string name, CharacterDefinition charDef, bool isPlayer, out TouchInputSource touchInput)
        {
            touchInput = null;
            var go = new GameObject(name);
            go.transform.position = isPlayer ? new Vector3(0f, 0f, -5f) : new Vector3(0f, 0f, 5f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(go.transform, false);
            body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            var renderer = body.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = isPlayer ? Color.blue : Color.red
                };
            }

            var col = go.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.radius = 0.5f;
            col.center = new Vector3(0f, 1f, 0f);

            var charBehaviour = go.AddComponent<CharacterBehaviour>();
            SetPrivateField(charBehaviour, "definition", charDef);

            if (isPlayer)
            {
                var inputObj = new GameObject("TouchInput");
                inputObj.transform.SetParent(go.transform, false);
                touchInput = inputObj.AddComponent<TouchInputSource>();
                SetPrivateField(charBehaviour, "inputSourceBehaviour", touchInput);
            }
            else
            {
                var inputObj = new GameObject("AiInput");
                inputObj.transform.SetParent(go.transform, false);
                var aiInput = inputObj.AddComponent<AiInputSource>();
                SetPrivateField(charBehaviour, "inputSourceBehaviour", aiInput);
            }

            return go;
        }

        private static GameObject CreateCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 15f, -18f);
            go.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            return go;
        }

        private static void CreateEventSystem()
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static MatchHud CreateHud(Transform canvasTransform)
        {
            var hudObj = new GameObject("HudController");
            hudObj.transform.SetParent(canvasTransform, false);
            var hud = hudObj.AddComponent<MatchHud>();

            var scoreText = CreateText(canvasTransform, "ScoreText", "0 - 0", 56);
            SetAnchors(scoreText.rectTransform, AnchorPresets.TopCenter);
            scoreText.rectTransform.anchoredPosition = new Vector2(0f, -20f);
            scoreText.color = Color.white;
            scoreText.alignment = TextAnchor.MiddleCenter;
            SetPrivateField(hud, "scoreText", scoreText);

            var timerText = CreateText(canvasTransform, "TimerText", "2:00", 40);
            SetAnchors(timerText.rectTransform, AnchorPresets.TopCenter);
            timerText.rectTransform.anchoredPosition = new Vector2(0f, -80f);
            timerText.color = Color.white;
            timerText.alignment = TextAnchor.MiddleCenter;
            SetPrivateField(hud, "timerText", timerText);

            var stateText = CreateText(canvasTransform, "StateText", "", 80);
            SetAnchors(stateText.rectTransform, AnchorPresets.MiddleCenter);
            stateText.color = Color.yellow;
            stateText.alignment = TextAnchor.MiddleCenter;
            stateText.gameObject.SetActive(false);
            SetPrivateField(hud, "stateText", stateText);

            var resultPanel = new GameObject("ResultPanel");
            resultPanel.transform.SetParent(canvasTransform, false);
            var panelRect = resultPanel.AddComponent<RectTransform>();
            SetAnchors(panelRect, AnchorPresets.StretchAll);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImage = resultPanel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.7f);
            SetPrivateField(hud, "resultPanel", resultPanel);

            var resultText = CreateText(resultPanel.transform, "ResultText", "", 64);
            SetAnchors(resultText.rectTransform, AnchorPresets.TopCenter);
            resultText.rectTransform.anchoredPosition = new Vector2(0f, -200f);
            resultText.color = Color.white;
            resultText.alignment = TextAnchor.MiddleCenter;
            SetPrivateField(hud, "resultText", resultText);

            var restartBtn = CreateButton(resultPanel.transform, "RestartButton", "RESTART", new Vector2(0f, -50f), new Vector2(300f, 80f));
            SetAnchors(restartBtn.GetComponent<RectTransform>(), AnchorPresets.MiddleCenter);
            SetPrivateField(hud, "restartButton", restartBtn);

            var menuBtn = CreateButton(resultPanel.transform, "MenuButton", "MAIN MENU", new Vector2(0f, -160f), new Vector2(300f, 80f));
            SetAnchors(menuBtn.GetComponent<RectTransform>(), AnchorPresets.MiddleCenter);
            SetPrivateField(hud, "menuButton", menuBtn);

            resultPanel.SetActive(false);

            return hud;
        }

        private static TouchControls CreateTouchControls(Transform canvasTransform, TouchInputSource inputSource)
        {
            var controlsObj = new GameObject("TouchControls");
            controlsObj.transform.SetParent(canvasTransform, false);
            var controls = controlsObj.AddComponent<TouchControls>();
            SetPrivateField(controls, "inputSource", inputSource);

            var joystickBg = new GameObject("JoystickBackground");
            joystickBg.transform.SetParent(canvasTransform, false);
            var bgRect = joystickBg.AddComponent<RectTransform>();
            SetAnchors(bgRect, AnchorPresets.BottomLeft);
            bgRect.anchoredPosition = new Vector2(180f, 180f);
            bgRect.sizeDelta = new Vector2(240f, 240f);
            var bgImage = joystickBg.AddComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.2f);
            bgImage.raycastTarget = true;

            var joystickHandle = new GameObject("JoystickHandle");
            joystickHandle.transform.SetParent(joystickBg.transform, false);
            var handleRect = joystickHandle.AddComponent<RectTransform>();
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = new Vector2(100f, 100f);
            var handleImage = joystickHandle.AddComponent<Image>();
            handleImage.color = new Color(1f, 1f, 1f, 0.5f);
            handleImage.raycastTarget = false;

            var joystick = joystickBg.AddComponent<VirtualJoystick>();
            SetPrivateField(joystick, "background", bgRect);
            SetPrivateField(joystick, "handle", handleRect);
            SetPrivateField(joystick, "handleRange", 100f);
            SetPrivateField(controls, "joystick", joystick);

            var kickBtn = CreateButton(canvasTransform, "KickButton", "KICK", new Vector2(-180f, 180f), new Vector2(160f, 160f));
            SetAnchors(kickBtn.GetComponent<RectTransform>(), AnchorPresets.BottomRight);
            SetPrivateField(controls, "kickButton", kickBtn);

            var tackleBtn = CreateButton(canvasTransform, "TackleButton", "TACKLE", new Vector2(-180f, 360f), new Vector2(160f, 120f));
            SetAnchors(tackleBtn.GetComponent<RectTransform>(), AnchorPresets.BottomRight);
            SetPrivateField(controls, "tackleButton", tackleBtn);

            var sprintObj = new GameObject("SprintButton");
            sprintObj.transform.SetParent(canvasTransform, false);
            var sprintRect = sprintObj.AddComponent<RectTransform>();
            SetAnchors(sprintRect, AnchorPresets.BottomRight);
            sprintRect.anchoredPosition = new Vector2(-360f, 180f);
            sprintRect.sizeDelta = new Vector2(140f, 140f);
            var sprintImage = sprintObj.AddComponent<Image>();
            sprintImage.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
            sprintImage.raycastTarget = true;
            var sprintBtn = sprintObj.AddComponent<SprintButton>();

            var sprintLabel = CreateText(sprintObj.transform, "SprintLabel", "SPRINT", 24);
            SetAnchors(sprintLabel.rectTransform, AnchorPresets.StretchAll);
            sprintLabel.rectTransform.offsetMin = Vector2.zero;
            sprintLabel.rectTransform.offsetMax = Vector2.zero;
            sprintLabel.color = Color.white;
            sprintLabel.alignment = TextAnchor.MiddleCenter;
            sprintLabel.raycastTarget = false;

            SetPrivateField(controls, "sprintButton", sprintBtn);

            return controls;
        }

        private static GameObject CreateMatchController(MatchConfig matchConfig, AiDifficultyConfig aiConfig,
            GameObject court, GameObject ball, GameObject player, GameObject ai)
        {
            var go = new GameObject("MatchController");
            var controller = go.AddComponent<MatchController>();

            SetPrivateField(controller, "matchConfig", matchConfig);
            SetPrivateField(controller, "aiDifficultyConfig", aiConfig);
            SetPrivateField(controller, "court", court.GetComponent<CourtBehaviour>());
            SetPrivateField(controller, "ball", ball.GetComponent<BallBehaviour>());
            SetPrivateField(controller, "player", player.GetComponent<CharacterBehaviour>());
            SetPrivateField(controller, "ai", ai.GetComponent<CharacterBehaviour>());

            var playerGoalTrigger = court.transform.Find("PlayerGoal/GoalTrigger")?.GetComponent<GoalTrigger>();
            var aiGoalTrigger = court.transform.Find("AiGoal/GoalTrigger")?.GetComponent<GoalTrigger>();

            SetPrivateField(controller, "playerGoalTrigger", playerGoalTrigger);
            SetPrivateField(controller, "aiGoalTrigger", aiGoalTrigger);

            var aiInput = ai.GetComponentInChildren<AiInputSource>();
            SetPrivateField(controller, "aiInputSource", aiInput);

            if (aiInput != null)
            {
                aiInput.Initialize(aiConfig, ball.GetComponent<BallBehaviour>(), player.GetComponent<CharacterBehaviour>(), court.GetComponent<CourtBehaviour>());
            }

            return go;
        }

        #region UI Helpers

        private enum AnchorPresets
        {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, MiddleCenter, MiddleRight,
            BottomLeft, BottomCenter, BottomRight,
            StretchAll
        }

        private static void SetAnchors(RectTransform rt, AnchorPresets preset)
        {
            switch (preset)
            {
                case AnchorPresets.TopCenter:
                    rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f); break;
                case AnchorPresets.MiddleCenter:
                    rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f); break;
                case AnchorPresets.BottomLeft:
                    rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(0f, 0f);
                    rt.pivot = new Vector2(0f, 0f); break;
                case AnchorPresets.BottomRight:
                    rt.anchorMin = new Vector2(1f, 0f); rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f); break;
                case AnchorPresets.StretchAll:
                    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                    rt.pivot = new Vector2(0.5f, 0.5f); break;
                default:
                    rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f); break;
            }
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            SetAnchors(rect, AnchorPresets.MiddleCenter);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.8f, 0.6f);
            image.raycastTarget = true;

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.4f, 0.8f, 0.6f);
            colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f, 0.8f);
            colors.pressedColor = new Color(0.1f, 0.3f, 0.7f, 1f);
            button.colors = colors;

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(go.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            SetAnchors(labelRect, AnchorPresets.StretchAll);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.fontSize = 32;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.raycastTarget = false;

            return button;
        }

        #endregion

        private static void RegisterScenesInBuildSettings()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene("Assets/_Project/Scenes/Boot.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/Match.unity", true)
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
                string folderName = Path.GetFileName(path);
                if (parent != null && AssetDatabase.IsValidFolder(parent))
                {
                    AssetDatabase.CreateFolder(parent, folderName);
                }
                else
                {
                    Directory.CreateDirectory(path.Replace('/', '\\'));
                    AssetDatabase.Refresh();
                }
            }
        }

        private static T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
