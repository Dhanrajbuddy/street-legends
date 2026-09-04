using System;
using NUnit.Framework;
using StreetLegends.Core;
using StreetLegends.Services;
using StreetLegends.Services.Local;

namespace StreetLegends.Tests.EditMode
{
    public sealed class ServiceRegistryTests
    {
        [Test]
        public void Register_ThenGet_ReturnsSameInstance()
        {
            var registry = new ServiceRegistry();
            var time = new SystemTimeService();

            registry.Register<ITimeService>(time);

            Assert.AreSame(time, registry.Get<ITimeService>());
            Assert.AreEqual(1, registry.Count);
        }

        [Test]
        public void Register_Twice_Throws()
        {
            var registry = new ServiceRegistry();
            registry.Register<ITimeService>(new SystemTimeService());

            Assert.Throws<InvalidOperationException>(() => registry.Register<ITimeService>(new SystemTimeService()));
        }

        [Test]
        public void Get_Unregistered_Throws()
        {
            var registry = new ServiceRegistry();

            Assert.Throws<InvalidOperationException>(() => registry.Get<ITimeService>());
        }

        [Test]
        public void TryGet_Unregistered_ReturnsFalse()
        {
            var registry = new ServiceRegistry();

            bool found = registry.TryGet<ITimeService>(out ITimeService instance);

            Assert.IsFalse(found);
            Assert.IsNull(instance);
        }
    }
}
