using System.Collections.Generic;
using UnityEngine;

public class BulletPool : Singleton<BulletPool>
{
    [SerializeField] private int _initialSize = 24;
    private readonly Dictionary<GameObject, Stack<GameObject>> _pools = new();

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!_pools.TryGetValue(prefab, out var stack))
        {
            stack = new Stack<GameObject>();
            _pools[prefab] = stack;
            Prewarm(prefab, _initialSize);
        }

        GameObject go = null;

        while (stack.Count > 0)
        {
            go = stack.Pop();
            if (go != null) break;
        }

        if (go == null)
            go = CreateInstance(prefab);

        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        return go;
    }

    public void Release(GameObject go)
    {
        var refComp = go.GetComponent<BulletPoolRef>();
        if (refComp == null || refComp.prefab == null)
        {
            Destroy(go);
            return;
        }

        go.SetActive(false);
        if (_pools.TryGetValue(refComp.prefab, out var stack)) stack.Push(go);
        else
        {
            var newStack = new Stack<GameObject>();
            newStack.Push(go);
            _pools[refComp.prefab] = newStack;
        }
    }

    private void Prewarm(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var go = CreateInstance(prefab);
            go.SetActive(false);
            _pools[prefab].Push(go);
        }
    }

    private GameObject CreateInstance(GameObject prefab)
    {
        var go = Instantiate(prefab);
        var refComp = go.GetComponent<BulletPoolRef>();
        if (refComp == null) refComp = go.AddComponent<BulletPoolRef>();
        refComp.prefab = prefab;
        return go;
    }
}

public class BulletPoolRef : MonoBehaviour
{
    public GameObject prefab;
}
