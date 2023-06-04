using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private readonly Dictionary<string, List<Action<object[]>>> events = new Dictionary<string, List<Action<object[]>>>();

    public void AddEvent(string eventName, Action<object[]> eventDelegate)
    {
        if (!events.TryGetValue(eventName, out var delegates))
            events.Add(eventName, delegates = new List<Action<object[]>>());
        delegates.Add(eventDelegate);
    }

    public void Dispatch(string eventName, params object[] pars)
    {
        if (events.TryGetValue(eventName, out var delegates))
        {
            foreach (var item in delegates)
                item.Invoke(pars);
        }
    }

    public void Remove(string eventName, Action<object[]> eventDelegate)
    {
        if (events.TryGetValue(eventName, out var delegates))
        {
            foreach (var item in delegates)
            {
                if (item.Equals(eventDelegate)) 
                {
                    delegates.Remove(item);
                    break;
                }
            }
        }
    }
}
