using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBlocks.Core
{
    public class ExecutionContext
    {
        public int CurrentCommandId { get; set; }
        public int CommandsExecuted { get; set; }
        public float StartTime { get; set; }
        public Dictionary<string, object> Variables { get; private set; }
        public bool IsCancelled { get; private set; }
        public string StopReason { get; private set; }
        public CodeBlocks.Core.IMovementDecisionProvider MovementDecisionProvider { get; set; }

        public ExecutionContext()
        {
            Variables = new Dictionary<string, object>();
            CurrentCommandId = -1;
            CommandsExecuted = 0;
            StartTime = Time.time;
            IsCancelled = false;
            StopReason = string.Empty;
        }

        public void Cancel(string reason = "")
        {
            IsCancelled = true;
            StopReason = reason ?? string.Empty;
        }

        // For future loop/conditional support
        public void SetVariable(string key, object value)
        {
            Variables[key] = value;
        }

        public T GetVariable<T>(string key)
        {
            if (Variables.TryGetValue(key, out object value))
            {
                return (T)value;
            }
            return default(T);
        }

        public bool HasVariable(string key)
        {
            return Variables.ContainsKey(key);
        }

        public void Clear()
        {
            Variables.Clear();
            CurrentCommandId = -1;
            CommandsExecuted = 0;
            IsCancelled = false;
            StopReason = string.Empty;
        }
    }
}
