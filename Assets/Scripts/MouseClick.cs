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

    private int[] blockPositions =
    {
        0b1, 0b10, 0b100, 0b1000,
        0b1_0000, 0b10_0000, 0b100_0000, 0b1000_0000,
        0b1_0000_0000, 0b10_0000_0000, 0b100_0000_0000, 0b1000_0000_0000,
        0b1_0000_0000_0000, 0b10_0000_0000_0000, 0b100_0000_0000_0000, 0b1000_0000_0000_0000
    };
    
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
        board |= blockPositions[index];
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
            board = (position << 16) | (board & 0xFFF0FF);
        else if (queen == 1)
            board = (position << 20) | (board & 0xFF0FFF);
        else if (queen == 2)
            board = (position << 24) | (board & 0xF0FFFF);
        else if (queen == 3)
            board = (position << 28) | (board & 0x0FFFFF);

        Vector2 pos = new Vector2((position % 4), -(position / 4));
        Debug.Log(pos);
        queens[queen].localPosition = pos;
    }
}
