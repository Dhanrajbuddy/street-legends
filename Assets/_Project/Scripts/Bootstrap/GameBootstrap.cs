using StreetLegends.Core;
using StreetLegends.Data;
using StreetLegends.Services;
using StreetLegends.Services.Local;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StreetLegends.Bootstrap
{
    /// <summary>
    /// Composition root. Lives only in the Boot scene, builds the service registry, then loads the main menu.
    /// Phase 0: registers the minimum services so the pipeline is proven end to end.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const string LogCategory = "Bootstrap";

        [SerializeField] private EnvironmentConfig _environment;

        public static ServiceRegistry Services { get; private set; }
        public static EventBus Events { get; private set; }

        private void Awake()
        {
            if (Services != null)
            {
                Log.Warn(LogCategory, "Bootstrap ran twice; destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Services = new ServiceRegistry();
            Events = new EventBus();

            if (_environment == null)
            {
                Log.Error(LogCategory, "EnvironmentConfig is not assigned on GameBootstrap. Falling back to defaults.", this);
            }
            else
            {
                Application.targetFrameRate = _environment.TargetFrameRate;
                Log.Info(LogCategory, $"Environment: {_environment.Environment}, localServices={_environment.UseLocalServices}");
            }

            RegisterServices();
        }

        private void Start()
        {
            Log.Info(LogCategory, $"Boot complete. {Services.Count} services registered. Loading {SceneNames.MainMenu}.");
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        private void RegisterServices()
        {
            Services.Register<ITimeService>(new SystemTimeService());
        }
    }
}
