using UnityEngine;
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

        private static void BuildMatchScene()
        {
            EnsureDirectory(PrefabGameplayPath);

            var matchConfig = AssetDatabase.LoadAssetAtPath<MatchConfig>($"{DataMatchPath}/MatchConfig.asset");
            var charDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>($"{DataCharactersPath}/Rookie.asset");
            var aiConfig = AssetDatabase.LoadAssetAtPath<AiDifficultyConfig>($"{DataMatchPath}/AiMedium.asset");

            Scene matchScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject courtObj = CreateCourt();
            GameObject ballObj = CreateBall();
            GameObject playerObj = CreateCharacter("Player", charDef, isPlayer: true);
            GameObject aiObj = CreateCharacter("AI", charDef, isPlayer: false);
            GameObject cameraObj = CreateCamera();
            GameObject hudObj = CreateHud();
            GameObject touchObj = CreateTouchControls();
            GameObject matchControllerObj = CreateMatchController(matchConfig, aiConfig, courtObj, ballObj, playerObj, aiObj);

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

            var playerGoal = CreateGoal(go.transform, "PlayerGoal", new Vector3(0f, 0.5f, -10f), GoalSide.PlayerGoal);
            var aiGoal = CreateGoal(go.transform, "AiGoal", new Vector3(0f, 0.5f, 10f), GoalSide.AiGoal);

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

        private static GameObject CreateGoal(Transform parent, string name, Vector3 pos, GoalSide side)
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
            var sideField = typeof(GoalTrigger).GetField("side", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            sideField?.SetValue(goalTrigger, side);

            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = Color.white
                };
            }

            return go;
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

        private static GameObject CreateCharacter(string name, CharacterDefinition charDef, bool isPlayer)
        {
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
            var defField = typeof(CharacterBehaviour).GetField("definition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            defField?.SetValue(charBehaviour, charDef);

            if (isPlayer)
            {
                var inputObj = new GameObject("KeyboardInput");
                inputObj.transform.SetParent(go.transform, false);
                var keyboardInput = inputObj.AddComponent<KeyboardInputSource>();
                var inputField = typeof(CharacterBehaviour).GetField("inputSourceBehaviour", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                inputField?.SetValue(charBehaviour, keyboardInput);
            }
            else
            {
                var inputObj = new GameObject("AiInput");
                inputObj.transform.SetParent(go.transform, false);
                var aiInput = inputObj.AddComponent<AiInputSource>();
                var inputField = typeof(CharacterBehaviour).GetField("inputSourceBehaviour", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                inputField?.SetValue(charBehaviour, aiInput);
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

        private static GameObject CreateHud()
        {
            var go = new GameObject("MatchHud");

            var canvas = new GameObject("Canvas");
            canvas.transform.SetParent(go.transform, false);
            var canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var hudObj = new GameObject("HudController");
            hudObj.transform.SetParent(canvas.transform, false);
            hudObj.AddComponent<MatchHud>();

            return go;
        }

        private static GameObject CreateTouchControls()
        {
            var go = new GameObject("TouchControls");
            return go;
        }

        private static GameObject CreateMatchController(MatchConfig matchConfig, AiDifficultyConfig aiConfig,
            GameObject court, GameObject ball, GameObject player, GameObject ai)
        {
            var go = new GameObject("MatchController");
            var controller = go.AddComponent<MatchController>();

            var configField = typeof(MatchController).GetField("matchConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField?.SetValue(controller, matchConfig);

            var aiConfigField = typeof(MatchController).GetField("aiDifficultyConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiConfigField?.SetValue(controller, aiConfig);

            var courtField = typeof(MatchController).GetField("court", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            courtField?.SetValue(controller, court.GetComponent<CourtBehaviour>());

            var ballField = typeof(MatchController).GetField("ball", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ballField?.SetValue(controller, ball.GetComponent<BallBehaviour>());

            var playerField = typeof(MatchController).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField?.SetValue(controller, player.GetComponent<CharacterBehaviour>());

            var aiField = typeof(MatchController).GetField("ai", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiField?.SetValue(controller, ai.GetComponent<CharacterBehaviour>());

            var playerGoalTrigger = court.transform.Find("PlayerGoal/GoalTrigger")?.GetComponent<GoalTrigger>();
            var aiGoalTrigger = court.transform.Find("AiGoal/GoalTrigger")?.GetComponent<GoalTrigger>();

            var pgtField = typeof(MatchController).GetField("playerGoalTrigger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pgtField?.SetValue(controller, playerGoalTrigger);

            var agtField = typeof(MatchController).GetField("aiGoalTrigger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            agtField?.SetValue(controller, aiGoalTrigger);

            var aiInputField = typeof(MatchController).GetField("aiInputSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiInputField?.SetValue(controller, ai.GetComponentInChildren<AiInputSource>());

            return go;
        }

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
    }
}
