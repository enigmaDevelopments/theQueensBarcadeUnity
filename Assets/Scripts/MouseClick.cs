using UnityEngine;
public struct Board
{
    public ushort blocks;
    public byte p1;
    public byte p2;

}
public class MouseClick : MonoBehaviour
{
    public static Board board = new Board
    {
        blocks = 0,
        p1 = 0xDE,
        p2 = 0x3
    };


    void OnMouseDown()
    {
        Debug.Log("Mouse clicked on " + gameObject.name);
        

    }
}

