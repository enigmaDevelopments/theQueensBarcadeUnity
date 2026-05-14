using Unity.VisualScripting;
using UnityEngine;

public class WinController : MonoBehaviour
{
    public GameObject p1Win;
    public GameObject p2Win;
    public GameObject Tie;
    public void gameEnd(bool p1, bool p2)
    {
        Tie.SetActive(p1 && p2);
        p1Win.SetActive(p1 && !p2);
        p2Win.SetActive(!p1 && p2);
    }
}
