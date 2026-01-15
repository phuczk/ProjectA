using UnityEngine;

public static class PlayerSpawnService
{
    private static GameObject _player;
    private static PlayerController _prefab;

    public static void Init(PlayerController prefab)
    {
        _prefab = prefab;
    }

    public static GameObject GetOrCreate(Vector3 pos, bool dontDestroy)
    {
        if (_player != null) return _player;

        _player = Object.Instantiate(_prefab, pos, Quaternion.identity).gameObject;

        if (dontDestroy)
            Object.DontDestroyOnLoad(_player);

        return _player;
    }

    public static void MoveTo(Vector3 pos)
    {
        if (_player == null) return;

        var rb = _player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = pos;
        else _player.transform.position = pos;
    }

    public static bool Exists() => _player != null;
}
