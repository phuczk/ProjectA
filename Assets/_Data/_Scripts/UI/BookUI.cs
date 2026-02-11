using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BookUI : MonoBehaviour
{
    [SerializeField] float duration = 0.4f;
    [SerializeField] List<Transform> pages;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject forwardButton;
    //[SerializeField] private MapAtlasUI mapAtlas;
    private List<PageUI> pageScripts = new List<PageUI>();
    private int index = -1;
    private bool isRotating = false;

    private void Awake()
    {
        DOTween.SetTweensCapacity(500, 50);
        foreach (var p in pages)
            pageScripts.Add(p.GetComponent<PageUI>());
    }

    private void OnEnable() => RefreshButtonHierarchy();

    public void InitialState()
    {
        index = -1;
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].DOKill();
            pages[i].localRotation = Quaternion.identity;
            pageScripts[i].ResetState();
        }
        UpdateButtons();
        RefreshAllPagesVisibility();
    }

    public void RotateForward() => RotatePage(true);
    public void RotateBack() => RotatePage(false);

    private void RotatePage(bool forward)
    {
        if (isRotating) return;
        if (forward && index >= pages.Count - 1) return;
        if (!forward && index < 0) return;

        isRotating = true;
        
        int movingPageIndex = forward ? index + 1 : index;
        int contentIndex = forward ? index + 1 : index - 1;
        PageUI activePage = pageScripts[movingPageIndex];
        
        pages[movingPageIndex].gameObject.SetActive(true);
        activePage.PrepareToFlip();
        
        PrepareBackgroundPages(movingPageIndex, forward);

        pages[movingPageIndex].SetAsLastSibling();
        RefreshButtonHierarchy();

        Vector3 endRotation = forward ? new Vector3(0, 180f, 0) : Vector3.zero;

        pages[movingPageIndex].DOKill();
        //if (mapAtlas != null && contentIndex == mapIndex) mapAtlas.gameObject.SetActive(true);
        pages[movingPageIndex].DOLocalRotate(endRotation, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            // .OnUpdate(() => {
            //     float yAngle = pages[movingPageIndex].localEulerAngles.y;
            //     activePage.CheckAndSwap(yAngle, forward);
                
            //     float t = yAngle / 180f;

            //     if (mapAtlas == null) return;

            //     if (contentIndex == mapIndex + 1)
            //     {
            //         mapAtlas.SetReveal(1f - t, 0); 
            //     }
            //     else if (contentIndex == mapIndex - 1)
            //     {
            //         mapAtlas.SetReveal(t, 1);
            //     }
            //     else if (contentIndex == mapIndex)
            //     {
            //         float revealValue = 0.4f + (t * 0.6f);
            //         mapAtlas.SetReveal(forward ? revealValue : 1f - (t * 0.6f), forward ? 1 : 0);
            //     }
            // })
            .OnComplete(() => {
                if (forward) index++; else index--;
                isRotating = false;
                UpdateButtons();
                // if (contentIndex == mapIndex + 1 || contentIndex == mapIndex - 1)
                // {
                //     mapAtlas.gameObject.SetActive(false);
                // }else if (contentIndex == mapIndex)
                // {
                //     mapAtlas.gameObject.SetActive(true);
                // }
                RefreshAllPagesVisibility();
            })
            .OnKill(() => isRotating = false);

        UpdateButtons();
    }

    private void PrepareBackgroundPages(int currentMovingIndex, bool forward)
    {
        if (currentMovingIndex - 1 >= 0)
        {
            pages[currentMovingIndex - 1].gameObject.SetActive(true);
            pageScripts[currentMovingIndex - 1].ShowBackOnly();
        }
        if (currentMovingIndex + 1 < pages.Count)
        {
            pages[currentMovingIndex + 1].gameObject.SetActive(true);
            pageScripts[currentMovingIndex + 1].ShowFrontOnly();
        }
    }

    public void OpenToPage(int targetIndex)
    {
        this.gameObject.SetActive(true);
        index = targetIndex;
        isRotating = false;

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].DOKill();
            pages[i].localRotation = Quaternion.Euler(0, i <= index ? 180f : 0, 0);
            if (i <= index) pageScripts[i].ShowBackOnly(); 
            else pageScripts[i].ShowFrontOnly();
        }

        RefreshAllPagesVisibility();
        UpdateButtons();
    }

    public void UpdateButtons()
    {
        backButton.SetActive(index >= 0);
        forwardButton.SetActive(index < pages.Count - 1);
    }

    private void RefreshButtonHierarchy()
    {
        backButton.transform.SetAsLastSibling();
        forwardButton.transform.SetAsLastSibling();
    }

    public void RefreshAllPagesVisibility()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            bool isVisible = (i == index || i == index + 1);
            pages[i].gameObject.SetActive(isVisible);

            if (isVisible)
            {
                if (i == index) pageScripts[i].ShowBackOnly();
                else pageScripts[i].ShowFrontOnly();
            }
        }
        RefreshButtonHierarchy();
    }
}
