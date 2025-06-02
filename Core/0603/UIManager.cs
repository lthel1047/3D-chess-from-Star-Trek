using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CameraController cameraController;

    public float zoomDistance = 1f;

    private float zoomDistanceMin = 3f;
    private float zoomDistanceMax = 15f;

    public Button zoomInBtn;
    public Button zoomOutBtn;

    private void Start()
    {
        if(zoomInBtn != null)
        {
            zoomInBtn.onClick.AddListener(OnZoomIn);
        }
        else
        {
            zoomOutBtn.onClick.AddListener(OnZoomOut);
        }
    }

    private void OnZoomIn()
    {
        if (cameraController != null)
        {
            if(cameraController.distance + zoomDistance > zoomDistanceMax)
            {
                cameraController.distance = zoomDistanceMax;
                return;
            }
        }
    }

    private void OnZoomOut()
    {
        if (cameraController != null)
        {
            if(cameraController.distance - zoomDistance < zoomDistanceMin)
            {
                cameraController.distance = zoomDistanceMin;
                return;
            }
        }
    }
}
