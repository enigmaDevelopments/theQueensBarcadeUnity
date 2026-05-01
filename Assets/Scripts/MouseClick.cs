using System.Collections;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
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
    private bool clicked;
    private static bool clicking;
    private static new Camera camera; 
    private static bool initialized = false;
    private static byte totalClicked = 0;
    private static byte selected = 16;
    
    private static BitVector32 board;
    private static int[] positions = new int[16];
    private static BitVector32.Section[] queenLocations = new BitVector32.Section[4];

    void Awake()
    {
        boarder.color = originalBoarderColor;
        tile.color = originalTileColor;
        if (initialized) return;
        initialized = true;
        #region bitvector setup
        board = new BitVector32(0);
        positions[0] = BitVector32.CreateMask();
        for (int i = 1; i < 16; i++)
            positions[i] = BitVector32.CreateMask(positions[i - 1]);
        BitVector32.Section lower = BitVector32.CreateSection(0xFF);
        lower = BitVector32.CreateSection(0xFF, lower);
        queenLocations[0] = BitVector32.CreateSection(15, lower);
        queenLocations[1] = BitVector32.CreateSection(15, queenLocations[0]);
        queenLocations[2] = BitVector32.CreateSection(15, queenLocations[1]);
        queenLocations[3] = BitVector32.CreateSection(15, queenLocations[2]);
        #endregion

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
        if (clicking)
        {
            clicked = true;
            clicking = false;
        }
    }
    void OnMouseExit()
    {
        mouseOver = false;
        clicking = false;
    }
    public void Click()
    {
        totalClicked++;
        Debug.Log(clicked);
        if (!mouseOver || !clicked) return;
        if (board[positions[index]])
            return;
        else if (selected == index)
        {
            boarder.color = originalBoarderColor;
            selected = 16;
            return;
        }
        for (int i = 0; i < 4; i++)
            if (board[queenLocations[i]] == index)
            {
                boarder.color = selectedBoarderColor;
                selected = index;
                return;
            }

        board[positions[index]] = true;
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
        if (Input.GetMouseButtonDown(0))
            clicking = true;
        if (Input.GetMouseButtonUp(0))
        {
            Click();
            clicked = false;
        }
        if (totalClicked == 16)
        {
            StartCoroutine(DisableCamera());
            totalClicked = 0;
        }
    }

    public void moveQueen (int queen, int position)
    {
        board[queenLocations[queen]] = position;
        Vector2 pos = new Vector2((position % 4), -(position / 4));
        queens[queen].localPosition = pos;
    }
}
