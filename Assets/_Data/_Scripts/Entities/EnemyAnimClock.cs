using UnityEngine;

public static class EnemyAnimClock
{
    public static float BobTime { get; private set; }
    public static float SwayTime { get; private set; }

    // lookup table sin để giảm cost
    private const int TABLE_SIZE = 256;
    private static readonly float[] sinTable = new float[TABLE_SIZE];

    static EnemyAnimClock()
    {
        for(int i = 0; i < TABLE_SIZE; i++)
        {
            float t = (float)i / TABLE_SIZE * Mathf.PI * 2f;
            sinTable[i] = Mathf.Sin(t);
        }
    }

    public static void Tick(float dt)
    {
        BobTime += dt;
        SwayTime += dt;
    }

    public static float Sin(float value)
    {
        int index = (int)(value * 40f) & (TABLE_SIZE - 1);
        return sinTable[index];
    }
}
