using UnityEngine;

public class ParallaxByDepth : MonoBehaviour
{
    private Transform cam;
    private Vector3 lastCamPos;
    private float depth;

    void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
        else
        {
            Camera mainCam = Camera.FindObjectOfType<Camera>();
            if (mainCam != null)
            {
                cam = mainCam.transform;
            }
            else
            {
                GameObject camObj = GameObject.FindWithTag("MainCamera");
                if (camObj != null)
                {
                    cam = camObj.transform;
                }
                else
                {
                    enabled = false;
                    return;
                }
            }
        }
        
        if (cam != null)
        {
            lastCamPos = cam.position;
            depth = transform.position.z;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;
        
        float parallax = (cam.position.x - lastCamPos.x) * (depth);

        transform.position += new Vector3(parallax, 0, 0);

        lastCamPos = cam.position;
    }
}
