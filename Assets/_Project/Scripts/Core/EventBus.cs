using System;
using System.Collections.Generic;

namespace StreetLegends.Core
{
    /// <summary>
    /// Typed publish/subscribe bus for cross-module domain events (e.g. PlayerLeveledUp, CurrencyChanged).
    /// Systems publish; presentation subscribes. Systems never reference presentation.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list))
            {
                list = new List<Delegate>();
                _handlers[typeof(T)] = list;
            }

            list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (_handlers.TryGetValue(typeof(T), out List<Delegate> list))
            {
                list.Remove(handler);
            }
        }

        public void Publish<T>(T evt) where T : struct
        {
            if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list) || list.Count == 0)
            {
                return;
            }

            Delegate[] snapshot = list.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                try
                {
                    ((Action<T>)snapshot[i]).Invoke(evt);
                }
                catch (Exception ex)
                {
                    Log.Error("EventBus", $"Handler for {typeof(T).Name} threw: {ex}");
                }
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
