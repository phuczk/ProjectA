using UnityEngine;

public class PageUI : MonoBehaviour
{
    public GameObject frontContents;
    public GameObject backContents;
    private bool swapped = false;

    public void ResetState()
    {
        swapped = false;
        frontContents.SetActive(true);
        backContents.SetActive(false);
    }

    public void ShowFrontOnly() { frontContents.SetActive(true); backContents.SetActive(false); }
    public void ShowBackOnly() { frontContents.SetActive(false); backContents.SetActive(true); }

    public void PrepareToFlip()
    {
        swapped = false;
        frontContents.SetActive(true);
        backContents.SetActive(true);
    }

    public void CheckAndSwap(float currentAngle, bool isForward)
    {
        if (swapped) return;

        if (isForward && (currentAngle >= 90f && currentAngle < 270f))
        {
            frontContents.SetActive(false);
            swapped = true;
        }
        else if (!isForward && (currentAngle <= 90f || currentAngle > 270f))
        {
            backContents.SetActive(false);
            swapped = true;
        }
    }

    public void HideAll() 
    { 
        frontContents.SetActive(false); 
        backContents.SetActive(false); 
    }
}
