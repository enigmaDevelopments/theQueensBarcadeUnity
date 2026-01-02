using UnityEngine;

public class MouseClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log("Mouse clicked on " + gameObject.name);
    }
}
