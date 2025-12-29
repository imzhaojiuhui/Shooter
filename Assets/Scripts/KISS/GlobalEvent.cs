using System;
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace KISS
{
    public static class GlobalEvent
    {
        
    }

    [DisallowMultipleComponent]
    public class EventUnsubscriber<T>:MonoBehaviour
    {
        [HideInInspector]
        private readonly HashSet<(KEvent<T>, Action<T>)> _bindActions = new();
            
        private void OnDestroy()
        {
            foreach (var (e, a) in _bindActions)
            {
                e.Action -= a;
            }
            _bindActions.Clear();
        }

        public void EnsureBind(KEvent<T> e, Action<T> action)
        {
            _bindActions.Add((e, action));
        }
            
        // public void RemoveBind(KEvent<T> e, Action<T> action)
        // {
        //     _bindActions.Remove((e, action));
        // }
    }
    
    public class KEvent<T>
    {
        public Action<T> Action { get; set; }
        
        public void Bind(Action<T> action, GameObject go)
        {
            this.Action -= action;
            this.Action += action;
            var unsubscriber = go.EnsureComponent<EventUnsubscriber<T>>();
            unsubscriber.EnsureBind(this, action);
        }

        public void Bind(Action<T> action, MonoBehaviour mono)
        {
            Bind(action, mono.gameObject);
        }

        public void Invoke(T param)
        {
            this.Action?.Invoke(param);
        }
    }

    public static class EventExt
    {
        
    }
}