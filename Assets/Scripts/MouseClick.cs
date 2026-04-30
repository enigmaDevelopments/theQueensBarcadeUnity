using UnityEngine;
using System.Collections;
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
    public int board;

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
        moveQueen(0, 13);
        moveQueen(1, 14);
        moveQueen(3, 3);

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

    public void moveQueen (int queen, int position)
    {
        if (queen == 0)
            board = (position << 20) | (board & 0xFFF0FF);
        if (queen == 2)
            board = (position << 8) | (board & 0xFFFF0F);
        else if (queen == 1)
            board = (position << 16) | (board & 0xFF0F);

        Vector2 pos = new Vector2((position % 4), -(position / 4));
        Debug.Log(pos);
        queens[queen].localPosition = pos;
    }
}
