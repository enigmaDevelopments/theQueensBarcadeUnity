using UnityEngine;

public class CameraUpdate : MonoBehaviour
{
    public new Camera camera;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            camera.enabled = true;
    }
}
