using System.Collections.Generic;
using NUnit.Framework;
using StreetLegends.Editor.Build;

namespace StreetLegends.Tests.EditMode
{
    /// <summary>
    /// Regression guards for configuration failures that previously shipped to device
    /// (magenta rendering from stripped URP shaders, unassigned EnvironmentConfig).
    /// </summary>
    public class ProjectConfigurationTests
    {
        [Test]
        public void RenderPipeline_IsUrpWithRendererData()
        {
            var errors = new List<string>();
            BuildValidation.ValidateRenderPipeline(errors);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [Test]
        public void BootScene_HasGameBootstrapWithEnvironmentConfig()
        {
            var errors = new List<string>();
            BuildValidation.ValidateBootScene(errors);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [Test]
        public void Tags_RequiredTagsAreDefined()
        {
            var errors = new List<string>();
            BuildValidation.ValidateTags(errors);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [Test]
        public void BuildSettings_HasBootMainMenuMatchInOrder()
        {
            var errors = new List<string>();
            BuildValidation.ValidateBuildScenes(errors);
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }
    }
}
