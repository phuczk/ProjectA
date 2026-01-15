using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CursedListUI : MonoBehaviour
{
    public CursedList cursedList;
    public CursedItemUI itemPrefab;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;
    public Transform contentRoot;
    public ScrollRect scrollRect;

    void Start()
    {
        Refresh();
        StartCoroutine(ResetScroll());
    }

    void Refresh()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var cursed in cursedList.CursedObjects)
        {
            var item = Instantiate(itemPrefab, contentRoot);
            item.Bind(cursed);
        }
    }

    IEnumerator ResetScroll()
    {
        yield return null; // chờ 1 frame cho layout xong
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f; // 1 = top
    }
}
