using NUnit.Framework;
using StreetLegends.Core;

namespace StreetLegends.Tests.EditMode
{
    public sealed class EventBusTests
    {
        private struct TestEvent
        {
            public int Value;
        }

        [Test]
        public void Publish_WithSubscriber_InvokesHandler()
        {
            var bus = new EventBus();
            int received = 0;
            bus.Subscribe<TestEvent>(e => received = e.Value);

            bus.Publish(new TestEvent { Value = 42 });

            Assert.AreEqual(42, received);
        }

        [Test]
        public void Publish_AfterUnsubscribe_DoesNotInvokeHandler()
        {
            var bus = new EventBus();
            int calls = 0;
            void Handler(TestEvent e) => calls++;
            bus.Subscribe<TestEvent>(Handler);
            bus.Unsubscribe<TestEvent>(Handler);

            bus.Publish(new TestEvent());

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            var bus = new EventBus();

            Assert.DoesNotThrow(() => bus.Publish(new TestEvent()));
        }
    }
}
