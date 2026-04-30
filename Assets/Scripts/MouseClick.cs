using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
public struct BoardState
{
    public BitVector32 board;
    public BitVector32 queens;
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

    private bool mouseOver;
    private static new Camera camera; 
    private static bool initialized = false;
    private static byte totalClicked = 0;
    private static byte selected = 16;
    private static BoardState board;
    private static int[][] positions = {new int[16],new int[16]};
    private static BitVector32.Section[] queenLocations = new BitVector32.Section[4];

    void Awake()
    {
        boarder.color = originalBoarderColor;
        tile.color = originalTileColor;
        if (initialized) return;
        initialized = true;
        board.board = new BitVector32(0);
        board.queens = new BitVector32(0);

        positions[0][0] = BitVector32.CreateMask();
        for (int i = 0; i < 2; i++)
        {
            for (int j = 1; j < 16; j++)
                positions[i][j] = BitVector32.CreateMask(positions[i][j - 1]);
            positions[1][0] = BitVector32.CreateMask(positions[0][15]);
        }

        BitVector32.Section lower = BitVector32.CreateSection(0xFF);
        lower = BitVector32.CreateSection(0xFF, lower);
        queenLocations[0] = BitVector32.CreateSection(15, lower);
        queenLocations[1] = BitVector32.CreateSection(15, queenLocations[0]);
        queenLocations[2] = BitVector32.CreateSection(15, queenLocations[1]);
        queenLocations[3] = BitVector32.CreateSection(15, queenLocations[2]);


        queens = new Transform[4];
        for (int i = 0; i < 4; i++)
            queens[i] = GameObject.FindGameObjectWithTag("queen" + (i + 1)).transform;
        moveQueen(0, 13);
        moveQueen(1, 14);
        moveQueen(3, 3);
        moveQueen(2, 0);

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
        if (!mouseOver) return;
        if (board.board[positions[0][index]])
            return;
        else if (selected == index)
        {
            boarder.color = originalBoarderColor;
            selected = 16;
            return;
        }
        else if (board.queens[positions[0][index]] || board.queens[positions[1][index]])
        {
            boarder.color = selectedBoarderColor;
            selected = index;
                return;
        }
        board.board[positions[0][index]] = true;
        boarder.color = selectedBoarderColor;
        tile.color = blockedTileColor;
    }
    IEnumerator DisableCamera()
    {
        yield return new WaitForEndOfFrame();
        camera.enabled = false;
    }
    void LateUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Click();
        }
        if (totalClicked == 16)
        {
            StartCoroutine(DisableCamera());
            totalClicked = 0;
        }
    }

    public void moveQueen (int queen, int position)
    {
        int oldPosition = board.board[queenLocations[queen]];
        board.queens[positions[queen/2][oldPosition]] = false;
        board.queens[positions[queen/2][position]] = true;
        board.board[queenLocations[queen]] = position;
        Vector2 pos = new Vector2((position % 4), -(position / 4));
        queens[queen].localPosition = pos;
    }
}
