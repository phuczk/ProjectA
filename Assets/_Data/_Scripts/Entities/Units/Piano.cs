using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum MusicPatternType
{
    Random,
    Scale,
    Chord,
    Arpeggio,
    Melody,
    Blues,
    Jazz
}

public class Piano : MonoBehaviour
{
    [Header("Piano UI/Physical Settings")]
    [SerializeField] private GameObject pianoKeyPrefab;
    [SerializeField] private Transform pianoKeyContainer;
    [SerializeField] private int numberOfKeys = 18;
    [SerializeField] private float whiteKeyWidth = 1f;
    [SerializeField] private float whiteKeyHeight = 3f;
    [SerializeField] private float blackKeyWidth = 0.6f;
    [SerializeField] private float blackKeyHeight = 1.8f;
    [SerializeField] private float whiteKeySpacing = 0.05f;
    [SerializeField] private float blackKeyHeightOffset = 0.5f;

    [Header("Bullet Settings")]
    [SerializeField] private GameObject lineProjectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float colorChangeDuration = 0.2f;
    
    [Header("Density Control")]
    [Range(1, 10)]
    [SerializeField] private int maxSimultaneousNotes = 3;
    [SerializeField] private float restBeatsAfterRoutine = 1.5f;
    [SerializeField] private int maxScaleLength = 3;

    [Header("Music Pattern Core")]
    [SerializeField] private MusicPatternType patternType = MusicPatternType.Random;
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float progressionChance = 0.6f;
    [SerializeField] private int maxNoteJump = 12;

    [Header("Color Settings")]
    [SerializeField] private Color activeKeyColor = Color.cyan;
    [SerializeField] private Color blackKeyColor = new Color(0.1f, 0.1f, 0.1f);
    [SerializeField] private Color whiteKeyColor = Color.white;

    private List<PianoKey> pianoKeys = new List<PianoKey>();
    private int lastNoteIndex = -1;
    private MusicPatternType dynamicPattern;

    private void Start()
    {
        CreatePianoKeys();
        dynamicPattern = patternType;
        StartCoroutine(MusicPatternLoop());
    }

    private IEnumerator MusicPatternLoop()
    {
        while (true)
        {
            if (Random.value < progressionChance)
            {
                dynamicPattern = (MusicPatternType)Random.Range(0, System.Enum.GetValues(typeof(MusicPatternType)).Length);
            }

            float beatInterval = 60f / bpm;

            switch (dynamicPattern)
            {
                case MusicPatternType.Scale:
                    yield return StartCoroutine(PlayScaleRoutine());
                    yield return new WaitForSeconds(beatInterval * restBeatsAfterRoutine);
                    break;
                case MusicPatternType.Arpeggio:
                    yield return StartCoroutine(PlayArpeggioRoutine());
                    yield return new WaitForSeconds(beatInterval * restBeatsAfterRoutine);
                    break;
                case MusicPatternType.Chord:
                    PlayChord();
                    yield return new WaitForSeconds(beatInterval);
                    break;
                case MusicPatternType.Jazz:
                    PlayJazzChord();
                    yield return new WaitForSeconds(beatInterval * 2);
                    break;
                default:
                    PlayNaturalRandom();
                    yield return new WaitForSeconds(beatInterval);
                    break;
            }
        }
    }

    // CẢI TIẾN: Giúp đạn không bị kẹt ở giữa
    private void PlayNaturalRandom()
    {
        int nextIndex;
        // 20% cơ hội sẽ nhảy sang vùng hoàn toàn khác để tránh tập trung 1 chỗ
        if (Random.value < 0.2f || lastNoteIndex == -1)
        {
            nextIndex = Random.Range(0, pianoKeys.Count);
        }
        else
        {
            int min = Mathf.Max(0, lastNoteIndex - maxNoteJump);
            int max = Mathf.Min(pianoKeys.Count - 1, lastNoteIndex + maxNoteJump);
            nextIndex = Random.Range(min, max + 1);
        }

        lastNoteIndex = nextIndex;
        PlayNote(nextIndex);
    }

    // CẢI TIẾN: Tính toán dải nốt linh hoạt hơn cho Scale
    private IEnumerator PlayScaleRoutine()
    {
        bool up = Random.value > 0.5f;
        int length = Mathf.Min(8, maxScaleLength); 
        
        // Đảm bảo "start" có thể khiến Scale chạm đến nốt cuối cùng
        int start = Random.Range(0, pianoKeys.Count - length);
        int[] intervals = { 0, 2, 4, 5, 7, 9, 11, 12 };

        for (int i = 0; i < length; i++)
        {
            int intervalIdx = up ? i : (length - 1 - i);
            int noteIdx = start + (intervals[intervalIdx] % 12); // Dùng modulo để giữ trong quãng
            
            // Ép chỉ số nằm trong mảng
            noteIdx = Mathf.Clamp(noteIdx, 0, pianoKeys.Count - 1);
            
            PlayNote(noteIdx);
            yield return new WaitForSeconds(30f / bpm);
        }
    }

    private IEnumerator PlayArpeggioRoutine()
    {
        int length = Mathf.Min(6, maxScaleLength);
        int start = Random.Range(0, pianoKeys.Count - 4); // Thu hẹp offset để dễ chạm nốt cuối
        int[] arpeggio = { 0, 4, 7, 12, 7, 4 };

        for (int i = 0; i < length; i++)
        {
            int noteIdx = Mathf.Clamp(start + arpeggio[i], 0, pianoKeys.Count - 1);
            PlayNote(noteIdx);
            yield return new WaitForSeconds(45f / bpm);
        }
    }

    private void PlayChord()
    {
        // Chord có thể bắt đầu ở bất cứ đâu, kể cả phím cuối
        int root = Random.Range(0, pianoKeys.Count - 4);
        int[][] shapes = { new int[]{0,4,7}, new int[]{0,3,7}, new int[]{0,5,7} };
        int[] selected = shapes[Random.Range(0, shapes.Length)];

        foreach (int s in selected.Take(maxSimultaneousNotes))
        {
            PlayNote(Mathf.Clamp(root + s, 0, pianoKeys.Count - 1));
        }
    }

    private void PlayJazzChord()
    {
        // Jazz thường dùng nốt rải rác, giúp bao phủ toàn bộ đàn
        List<int> jazzIndices = new List<int>();
        for (int i = 0; i < maxSimultaneousNotes; i++)
        {
            jazzIndices.Add(Random.Range(0, pianoKeys.Count));
        }
        PlayMultipleNotes(jazzIndices.ToArray());
    }

    // --- GIỮ NGUYÊN CreatePianoKeys VÀ CÁC HÀM PHỤ ---
    private void CreatePianoKeys()
    {
        if (pianoKeyContainer == null) pianoKeyContainer = this.transform;
        int[] whitePattern = { 0, 2, 4, 5, 7, 9, 11 };
        int whiteCount = 0;
        
        for (int i = 0; i < numberOfKeys; i++)
        {
            int note = i % 12;
            if (whitePattern.Contains(note))
            {
                float xPos = whiteCount * (whiteKeyWidth + whiteKeySpacing);
                CreateKey(i, new Vector3(xPos, 0, 0), whiteKeyColor, whiteKeyWidth, whiteKeyHeight, 0); // White keys ở layer 0
                whiteCount++;
            }
        }

        whiteCount = 0;
        for (int i = 0; i < numberOfKeys; i++)
        {
            int note = i % 12;
            if (!whitePattern.Contains(note))
            {
                float xPos = (whiteCount - 1) * (whiteKeyWidth + whiteKeySpacing) + (whiteKeyWidth + whiteKeySpacing) * 0.5f;
                CreateKey(i, new Vector3(xPos, blackKeyHeightOffset, 0), blackKeyColor, blackKeyWidth, blackKeyHeight, 1); // Black keys ở layer 1 (trên)
            }
            else whiteCount++;
        }
        pianoKeys = pianoKeys.OrderBy(k => k.transform.position.x).ToList();
    }

    private void CreateKey(int id, Vector3 pos, Color col, float w, float h, int sort)
    {
        GameObject obj = Instantiate(pianoKeyPrefab, pianoKeyContainer);
        obj.transform.localPosition = pos;
        obj.transform.localScale = new Vector3(w, h, 1);
        
        // Set sorting order for proper layering
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = sort;
        }
        
        PianoKey key = obj.GetComponent<PianoKey>() ?? obj.AddComponent<PianoKey>();
        key.Initialize(id, col, activeKeyColor, colorChangeDuration, this);
        pianoKeys.Add(key);
    }

    public void PlayNote(int index)
    {
        if (index >= 0 && index < pianoKeys.Count) pianoKeys[index].SpawnBullet();
    }

    public void PlayMultipleNotes(int[] indices)
    {
        foreach (int i in indices) PlayNote(i);
    }

    public void SpawnBulletFromKey(Vector3 pos)
    {
        GameObject bullet = Instantiate(lineProjectilePrefab, pos, Quaternion.identity);
        if (bullet.TryGetComponent(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.down * projectileSpeed;
        }
    }
}
