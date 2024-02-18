using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.Internals.Events
{
    public class Event
    {
        public Event() { }
    }
    
    public class EventData
    {
        public object Sender;

        public EventData(object sender) 
        {
            Sender = sender;
        }
    }

    public struct QueuedEvent
    {
        public Type EventType;
        public EventData Data;

        public QueuedEvent(Type eventType, EventData data)
        {
            EventType = eventType;
            Data = data;
        }
    }

    public static class EventHandler
    {
        public static Dictionary<Type, List<Action<EventData>>> EventHandlers = new Dictionary<Type, List<Action<EventData>>>();
        public static Queue<QueuedEvent> EventQueue = new Queue<QueuedEvent>();

        public static void Subscribe<T>(Action<EventData> handler) where T : Event
        {
            var eventType = typeof(T);

            if (!EventHandlers.ContainsKey(eventType))
            {
                EventHandlers.Add(eventType, new List<Action<EventData>>());
            }

            EventHandlers[eventType].Add(handler);
        }

        public static void Unsubscribe<T>(Action<EventData> handler) where T : Event
        {
            var eventType = typeof(T);

            if (EventHandlers.ContainsKey(eventType))
            {
                EventHandlers[eventType].Remove(handler);
            }
        }

        public static void Invoke<T>(EventData data) where T : Event
        {
            var eventType = typeof(T);

            if (EventHandlers.ContainsKey(eventType))
            {
                if(data != null)
                {
                    EventQueue.Enqueue(new QueuedEvent(eventType, data));
                }
            }
        }

        public static void PollEvents()
        {
            while(EventQueue.Count != 0)
            {
                var qEvent = EventQueue.Dequeue();

                // no need to check for the type in EventHandlers before this. It has already been done in Invoke<T>.
                foreach (var handler in EventHandlers[qEvent.EventType])
                {
                    handler(qEvent.Data);
                }
            }
        }
    }
}
