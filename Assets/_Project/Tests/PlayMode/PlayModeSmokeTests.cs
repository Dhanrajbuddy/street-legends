using NUnit.Framework;
using UnityEngine;

namespace StreetLegends.Tests.PlayMode
{
    public sealed class PlayModeSmokeTests
    {
        [Test]
        public void Application_IsPlaying_InPlayModeTests()
        {
            Assert.IsTrue(Application.isPlaying);
        }
    }
}
