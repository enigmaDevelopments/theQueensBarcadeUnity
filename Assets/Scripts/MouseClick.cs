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
    private static bool clicked = false;
    private static byte totalClicked = 0;
    private static BoardState board;
    private static int[][] positions = 
    {
        new int[]
        {
        BitVector32.CreateMask(0),
        BitVector32.CreateMask(1),
        BitVector32.CreateMask(2),
        BitVector32.CreateMask(3),
        BitVector32.CreateMask(4),
        BitVector32.CreateMask(5),
        BitVector32.CreateMask(6),
        BitVector32.CreateMask(7),
        BitVector32.CreateMask(8),
        BitVector32.CreateMask(9),
        BitVector32.CreateMask(10),
        BitVector32.CreateMask(11),
        BitVector32.CreateMask(12),
        BitVector32.CreateMask(13),
        BitVector32.CreateMask(14),
        BitVector32.CreateMask(15)
        },
        new int[]
        {
        BitVector32.CreateMask(16),
        BitVector32.CreateMask(17),
        BitVector32.CreateMask(18),
        BitVector32.CreateMask(19),
        BitVector32.CreateMask(20),
        BitVector32.CreateMask(21),
        BitVector32.CreateMask(22),
        BitVector32.CreateMask(23),
        BitVector32.CreateMask(24),
        BitVector32.CreateMask(25),
        BitVector32.CreateMask(26),
        BitVector32.CreateMask(27),
        BitVector32.CreateMask(28),
        BitVector32.CreateMask(29),
        BitVector32.CreateMask(30),
        BitVector32.CreateMask(31)
        }
    };
    private static BitVector32.Section[] queenLocations = new BitVector32.Section[4];

    void Awake()
    {
        boarder.color = originalBoarderColor;
        tile.color = originalTileColor;
        if (initialized) return;
        initialized = true;
        BitVector32.Section lower = BitVector32.CreateSection(-1);
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
        board.board[positions[0][index]] = true;
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
        int oldPosition = board.board[queenLocations[queen]];
        board.queens[positions[queen/2][oldPosition]] = false;
        board.board[positions[queen/2][position]] = true;
        board.board[queenLocations[queen]] = position;
        Vector2 pos = new Vector2((position % 4), -(position / 4));
        Debug.Log(pos);
        queens[queen].localPosition = pos;
    }
}
