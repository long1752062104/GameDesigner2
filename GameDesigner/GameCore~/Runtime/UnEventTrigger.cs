using Net.Client;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public enum UnEventMode
    {
        OnDestroy,
        OnDisable,
    }

    internal class UnEventData
    {
        internal UnEventMode eventMode;
        internal byte type;
        internal Enum eventType;
        internal object value;
    }

    internal class RpcUnRegister
    {
        internal ClientBase client;
        internal object target;

        public RpcUnRegister(ClientBase self, object target)
        {
            this.client = self;
            this.target = target;
        }
    }

    public class UnEventTrigger : MonoBehaviour
    {
        private readonly List<UnEventData> unEvents = new List<UnEventData>();

        private void OnDisable()
        {
            RemoveEvents(UnEventMode.OnDisable);
        }

        private void OnDestroy()
        {
            RemoveEvents(UnEventMode.OnDestroy);
        }

        public void RegisterUnEvents(UnEventMode eventMode, byte type, Enum eventType, object value)
        {
            unEvents.Add(new UnEventData { eventMode = eventMode, type = type, eventType = eventType, value = value });
        }

        private void RemoveEvents(UnEventMode eventMode)
        {
            for (int i = unEvents.Count - 1; i >= 0; i--)
            {
                var data = unEvents[i];
                if (data.eventMode != eventMode)
                    continue;
                if (data.type == 1)
                {
                    Global.Event.Remove(data.eventType, (Action<object[]>)data.value);
                    unEvents.RemoveAt(i);
                    continue;
                }
                if (data.type == 2)
                {
                    Global.Event.RemoveGet(data.eventType);
                    unEvents.RemoveAt(i);
                    continue;
                }
                if (data.type == 3)
                {
                    var unRegister = (RpcUnRegister)data.value;
                    unRegister.client.RemoveRpc(unRegister.target);
                    unEvents.RemoveAt(i);
                }
            }
        }
    }
}