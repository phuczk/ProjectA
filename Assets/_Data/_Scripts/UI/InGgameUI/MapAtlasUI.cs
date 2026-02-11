using UnityEngine;
using UnityEngine.UI;

public class MapAtlasUI : MonoBehaviour
{
    private Material mapMaterial;
    private static readonly int RevealProp = Shader.PropertyToID("_Reveal");
    private static readonly int DirectionProp = Shader.PropertyToID("_Direction");

    private void Awake()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            mapMaterial = new Material(img.material);
            img.material = mapMaterial;
        }
    }

    public void SetReveal(float progress, int direction)
    {
        if (mapMaterial == null) return;
        
        mapMaterial.SetFloat(DirectionProp, direction);
        mapMaterial.SetFloat(RevealProp, Mathf.Clamp01(progress));
    }

    public void ResetMap() => SetReveal(0, 0);
}
