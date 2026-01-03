using UnityEngine;
using System.Collections;
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

    private bool mouseOver;

    private static new Camera camera; 
    private static bool initialized = false;
    private static bool clicked = false;
    private static byte totalClicked = 0;

    void Awake()
    {
        boarder.color = originalBoarderColor;
        tile.color = originalTileColor;
        if (initialized) return;
        initialized = true;
        Debug.Log(index);
        queens = new Transform[4];
        for (int i = 0; i < 4; i++)
            queens[i] = GameObject.FindGameObjectWithTag("queen" + (i + 1)).transform;
        camera = Camera.main;
    }
    void OnMouseEnter()
    {
        mouseOver = true;
    }
    void OnMouseExit()
    {
        mouseOver = false;
    }
    public void Click()
    {
        totalClicked++;
        if (clicked || !mouseOver) return;
        clicked = true;
        boarder.color = selectedBoarderColor;
        tile.color = blockedTileColor;
        clicked = false;
        
    }
    IEnumerator DisableCamera()
    {
        yield return new WaitForEndOfFrame();
        camera.enabled = false;
    }
    void LateUpdate()
    {
        if (Input.GetMouseButtonUp(0))
            Click();
        if (totalClicked == 16)
        {
            StartCoroutine(DisableCamera());
            totalClicked = 0;
        }
    }
}
