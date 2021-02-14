using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Taken from the following answer:
//https://answers.unity.com/questions/1581454/what-is-the-most-efficient-messageevent-management.html

public static class EventNotifier
{
    static Dictionary<Type, List<Action<BaseEvent>>> registeredNotifications;

    public static void RegisterNotification(System.Type notification, Action<BaseEvent> callback)
    {
        if (registeredNotifications == null)
        {
            registeredNotifications = new Dictionary<System.Type, List<Action<BaseEvent>>>();
        }

        // Notification key exists - add the callback
        if (registeredNotifications.ContainsKey(notification))
        {
            // No dupe
            List<Action<BaseEvent>> actions = registeredNotifications[notification];
            if (actions.Contains(callback) == false)
            {
                actions.Add(callback);
            }
        }
        // Notification key missing - add key and callback
        else
        {
            List<Action<BaseEvent>> actions = new List<Action<BaseEvent>>();
            actions.Add(callback);

            registeredNotifications.Add(notification, actions);
        }
    }


    public static void UnRegisterNotification(System.Type notification, Action<BaseEvent> callback)
    {
        if (registeredNotifications != null && registeredNotifications.ContainsKey(notification))
        {
            List<Action<BaseEvent>> actions = registeredNotifications[notification];
            actions.Remove(callback);
        }
    }


    public static void Notify(System.Type notification, BaseEvent data)
    {
        if (registeredNotifications != null && registeredNotifications.ContainsKey(notification))
        {
            List<Action<BaseEvent>> actions = registeredNotifications[notification];

            int actionCount = actions.Count;
            for (int i = 0; i < actionCount; i++)
            {
                actions[i].Invoke(data);
            }
        }
    }


    public static void UnRegisterAll()
    {
        registeredNotifications = null;
    }

}

// I like to keep the events in the same script so I know all the ones I can use.
public abstract class BaseEvent
{ }

//public class StructureCreatedEvent : BaseEvent
//{
//    public Structure structure;
//}
