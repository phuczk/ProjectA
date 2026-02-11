using System.Collections.Generic;

public static class CollectionSync
{
    public static void SyncList(HashSet<string> set, List<string> list)
    {
        list.Clear();
        foreach (var v in set)
            list.Add(v);
    }

    public static void Cache(HashSet<string> set, List<string> list)
    {
        set.Clear();
        if (list == null) return;

        foreach (var v in list)
            set.Add(v);
    }
}
