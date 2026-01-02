using UnityEngine;
public struct Board
{
    public ushort blocks;
    public byte p1;
    public byte p2;

}
public class MouseClick : MonoBehaviour
{
    public SpriteRenderer boarder;
    public SpriteRenderer tile;
    public Color originalBoarderColor;
    public Color selectedBoarderColor;
    public Color originalTileColor;
    public Color blockedTileColor;
    [Header("Tile Info")]
    public byte index;

    public static Transform[] queens;
    public static Board board = new Board
    {
        blocks = 0,
        p1 = 0xDE,
        p2 = 0x3
    };
    private static bool initialized = false;

    private void Awake()
    {
        boarder.color = originalBoarderColor;
        tile.color = originalTileColor;
        if (initialized) return;
        initialized = true;
        Debug.Log(index);
        queens = new Transform[4];
        for (int i = 0; i < 4; i++)
            queens[i] = GameObject.FindGameObjectWithTag("queen" + (i + 1)).transform;
    }
    void OnMouseDown()
    {
        Debug.Log(index);
        boarder.color = selectedBoarderColor;
        tile.color = blockedTileColor;
    }
}
