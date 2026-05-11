using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
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
        if (!mouseOver || !clicked) return;
        if (board[positions[index]])
            return;
        else if (selected == index)
        {
            boarder.color = originalBoarderColor;
            selected = 16;
            return;
        }
        else if (selected != 16)
        {
            Debug.Log(true);
            if (!checkMove(selected, index))
                return;
            int queen = 0;
            if (board[queenLocations[1]] == selected) 
                queen = 1;
            moveQueen(queen, index);
            selected = 16;
            return;
        }
        for (int i = 0; i < 2; i++)
            if (board[queenLocations[i]] == index)
            {
                boarder.color = selectedBoarderColor;
                selected = index;
                return;
            }
        for (int i = 2; i < 4; i++)
            if (board[queenLocations[i]] == index)
                return;

        board[positions[index]] = true;
        boarder.color = selectedBoarderColor;
        tile.color = blockedTileColor;
    }
    IEnumerator DisableCamera()
    {
        yield return new WaitForEndOfFrame();
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
        if (boarder.color == selectedBoarderColor && selected != index)
            boarder.color = originalBoarderColor;
    }

    public void moveQueen(int queen, int position)
    {
        for (int i = 0; i < 4; i++)
            if (board[queenLocations[i]] == position)
            {
                board[queenLocations[i]] = board[queenLocations[(i/2)+((i+1)%2)]];
                queens[i].gameObject.SetActive(false);
            }
        board[queenLocations[queen]] = position;
        Vector2 pos = new Vector2((position % 4), -(position / 4));
        queens[queen].localPosition = pos;
    }

    public bool[] checkWin()
    {
        bool[] lose = { false, false };
        for (int i = 0; i < 4; i += 2)
        {
            ushort blocks = (ushort)board.Data;
            uint queen = (uint)1 << board[queenLocations[i]];
            queen |= (uint)1 << board[queenLocations[i + 1]];
            uint surrounding = (((uint)0b10_0000_0000_0010_0010 << board[queenLocations[i]]) | ((uint)0b10_0000_0000_0010_0010 << board[queenLocations[i + 1]])) & 0b1_0001_0001_1111_0001_0001_0001_0001;
            surrounding |= ((queen * 0b1000_0000_0000_1000) | (queen >> 1)) & 0b1000_1000_1000_1111_1000_1000_1000_1000;
            surrounding |= (queen * 0b1_0000_0000_0001_0000) & 0b1111_0000_0000_0000_0000;
            ushort smallSurounding = (ushort)(surrounding | (surrounding >> 20));
            blocks = (ushort)~(blocks | queen);
            smallSurounding &= blocks;
            lose[i / 2] = smallSurounding == 0;
        }
        return lose;
    }

    public bool checkMove(int queen, int position)
    {
        byte queenPosition;
        if (board[queenLocations[0]] == queen)
            queenPosition = (byte)board[queenLocations[1]];
        else
            queenPosition = (byte)board[queenLocations[0]];
        int lower = Mathf.Min(queen, position);
        int upper = Mathf.Max(queen, position);
        foreach (int direction in new int[] {5, 4, 3, 1})
            if ((upper-lower) % direction == 0)
            {
                bool vaild = true;
                bool cantMove = false;
                for (int i = lower + direction; i <= upper; i += direction)
                    if (board[positions[i]] || i == queenPosition || cantMove || (direction % 6 == 1 && i % 4 == 0) || (direction == 3 && i % 4 == 3))
                    {
                        vaild = false;
                        break;
                    }
                    else if (i == board[queenLocations[2]] || i == board[queenLocations[3]])
                        cantMove = true;
                if (vaild)
                    return true;
            }
        return false;
    }
}
