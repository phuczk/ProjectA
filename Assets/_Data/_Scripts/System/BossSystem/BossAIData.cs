using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BossAIData
{
    [SerializeField] public List<BossNodeData> Nodes = new List<BossNodeData>();
    [SerializeField] public List<BossConnectionData> Connections = new List<BossConnectionData>();
    [SerializeField] public string StartNodeGuid;
    [SerializeField] public Vector2 GraphOffset = Vector2.zero;
    
    public void Clear()
    {
        Nodes.Clear();
        Connections.Clear();
        StartNodeGuid = "";
        GraphOffset = Vector2.zero;
    }
    
    public BossNodeData GetNodeData(string guid)
    {
        return Nodes.Find(n => n.Guid == guid);
    }
    
    public bool ContainsNode(string guid)
    {
        return Nodes.Exists(n => n.Guid == guid);
    }
}

[Serializable]
public class BossNodeData
{
    public string Guid;
    public string NodeType;
    public Vector2 Position;
    public string JsonData; // Serialized node-specific data
    
    public T GetNodeData<T>() where T : class
    {
        if (string.IsNullOrEmpty(JsonData)) return null;
        try
        {
            return JsonUtility.FromJson<T>(JsonData);
        }
        catch
        {
            return null;
        }
    }
    
    public void SetNodeData<T>(T data) where T : class
    {
        if (data != null)
        {
            JsonData = JsonUtility.ToJson(data);
        }
        else
        {
            JsonData = "";
        }
    }
}

[Serializable]
public class BossConnectionData
{
    public string FromNodeGuid;
    public string FromPortName;
    public string ToNodeGuid;
    public string ToPortName;
}
