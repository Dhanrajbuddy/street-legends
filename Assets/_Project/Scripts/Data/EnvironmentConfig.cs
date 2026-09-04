using UnityEngine;

namespace StreetLegends.Data
{
    public enum BuildEnvironment
    {
        Dev,
        Staging,
        Release
    }

    /// <summary>
    /// Per-environment settings selected at boot. One asset per environment lives under Assets/_Project/Data/Environment/.
    /// Secrets are never stored here.
    /// </summary>
    [CreateAssetMenu(menuName = "Street Legends/Environment Config", fileName = "EnvironmentConfig")]
    public sealed class EnvironmentConfig : ScriptableObject
    {
        [SerializeField] private BuildEnvironment _environment = BuildEnvironment.Dev;
        [SerializeField] private bool _useLocalServices = true;
        [SerializeField] private bool _verboseLogging = true;
        [SerializeField] private int _targetFrameRate = 60;

        public BuildEnvironment Environment => _environment;
        public bool UseLocalServices => _useLocalServices;
        public bool VerboseLogging => _verboseLogging;
        public int TargetFrameRate => _targetFrameRate;
    }
}
