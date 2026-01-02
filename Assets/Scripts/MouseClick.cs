using UnityEngine;
public struct Board
{
    public ushort blocks;
    public byte p1;
    public byte p2;

}
public class MouseClick : MonoBehaviour
{
    public byte index;

    public static Transform[] queens;
    public static Board board = new Board
    {
        blocks = 0,
        p1 = 0xDE,
        p2 = 0x3
    };
    private void Awake()
    {
        queens = new Transform[4];
        for (int i = 1; i <= 4; i++)
            queens[i] = GameObject.FindGameObjectWithTag("queen" + i).transform;
    }
    void OnMouseDown()
    {
        Debug.Log(index);
        

    }
}
