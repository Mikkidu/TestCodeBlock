using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Parameter
{
    public string key;
    public string value;
}

[System.Serializable]
public class GridObject
{
    public Vector2Int position;
    public string objectTypeId;
    public string objectInstanceId;

    // Serializable format for Unity Inspector
    [SerializeField] private List<Parameter> parametersList = new List<Parameter>();

    // Runtime accessor (lazy init from parametersList)
    private Dictionary<string, string> _parameters;
    public Dictionary<string, string> parameters
    {
        get
        {
            if (_parameters == null)
            {
                _parameters = new Dictionary<string, string>();
                foreach (var p in parametersList)
                {
                    if (p != null && !string.IsNullOrEmpty(p.key))
                        _parameters[p.key] = p.value;
                }
            }
            return _parameters;
        }
    }

    // Helper to sync Dictionary → List (for Editor scripts)
    public void SyncParameters()
    {
        parametersList.Clear();
        if (_parameters != null)
        {
            foreach (var kvp in _parameters)
                parametersList.Add(new Parameter { key = kvp.Key, value = kvp.Value });
        }
    }

    // Constructor for code-created objects
    public GridObject()
    {
        parametersList = new List<Parameter>();
    }

    // Helper to add parameter (chainable)
    public GridObject AddParameter(string key, string value)
    {
        parametersList.Add(new Parameter { key = key, value = value });
        _parameters = null; // Force rebuild on next access
        return this;
    }
}
