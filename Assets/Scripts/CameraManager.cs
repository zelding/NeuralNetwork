using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera[] cameras;

    public void SwitchToTop()
    {
        cameras[0].enabled = true;
        cameras[1].enabled = false;
    }

    public void SwitchToTps()
    {
        cameras[0].enabled = false;
        cameras[1].enabled = true;
    }

    private void Start()
    {
        SwitchToTop();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.F1))
        {
            SwitchToTop();
        }

        if (Input.GetKey(KeyCode.F2))
        {
            SwitchToTps();
        }
    }
}
