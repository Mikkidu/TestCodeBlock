using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobotProgramming.Core
{
    public class ExecutionContext
    {
        public int CurrentCommandId { get; set; }
        public int CommandsExecuted { get; set; }
        public float StartTime { get; set; }
        public Dictionary<string, object> Variables { get; private set; }

        public ExecutionContext()
        {
            Variables = new Dictionary<string, object>();
            CurrentCommandId = -1;
            CommandsExecuted = 0;
            StartTime = Time.time;
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
        }
    }
}
