using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using UnityEngine;

public class MouseClick : MonoBehaviour
{
    public GameObject space;
    public Color originalBoarderColor;
    public Color selectedBoarderColor;
    public Color originalTileColor;
    public Color blockedTileColor;
    public ComputeShader shader;

    public Transform[] queens;
    public WinController winController;

    private SpaceData[,] spaces = new SpaceData[4, 4];
    private byte mouseOver;
    private new Camera camera;
    private SpaceData selected;
    private Thread workThread;

    private static BitVector32 board;
    public static int[] positions = new int[16];
    public static BitVector32.Section[] queenLocations = new BitVector32.Section[4];

    void Awake()
    {
        for (float i = -1.5f, k = 0; k < 4; i++, k++)
            for (float j = -1.5f, l = 0, m = 12; l < 4; j++, l++, m -= 4)
            {
                spaces[(int)k, (int)l] = Instantiate(space, new Vector3(i, j, 0), new Quaternion(0, 0, 0, 0), transform).GetComponent<SpaceData>();
                spaces[(int)k, (int)l].index =  (byte)(m + k);
            }
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

        AI.shader = shader;
        workThread = new Thread(makeMove);
        moveQueen(0, 13);
        moveQueen(1, 14);
        moveQueen(3, 3);

        camera = Camera.main;

    }

    private void OnMouseDown()
    {
        SpaceData data = getSpace(camera.ScreenToWorldPoint(Input.mousePosition));
        if (data == null)
            return;
        mouseOver = data.index;
    }
    private void OnMouseUp()
    {
        SpaceData data = getSpace(camera.ScreenToWorldPoint(Input.mousePosition));
        if (data == null || mouseOver != data.index)
            return;
        Click(data);
        bool[] winners = checkWin();
        if (winners[0] || winners[1])
        {
            workThread.Abort();
            winController.gameEnd(winners[1], winners[0]);
        }
    }
    private SpaceData getSpace(Vector2 position)
    {
        if (position.x < -2f || position.x >= 2f || position.y < -2f || position.y >= 2f)
            return null;
        int k = Mathf.FloorToInt(position.x + 2f);
        int l = Mathf.FloorToInt(position.y + 2f);
        return spaces[k, l];
    }
    public void Click(SpaceData data)
    {
        if (selected != null)
            if (selected.index == data.index)
            {
                data.boarder.color = originalBoarderColor;
                selected.boarder.color = originalBoarderColor;
                selected = null;
                return;
            }
        for (int i = 0; i < 2; i++)
            if (board[queenLocations[i]] == data.index)
            {
                data.boarder.color = selectedBoarderColor;
                selected = data;
                return;
            }
        if (workThread.IsAlive) return;
        else if (board[positions[data.index]]) return;
        else if (selected != null)
        {
            if (!checkMove(selected.index, data.index))
                return;
            int queen = 0;
            if (board[queenLocations[1]] == selected.index)
                queen = 1;
            moveQueen(queen, data.index);
            selected.boarder.color = originalBoarderColor;
            selected = null;
            workThread = new Thread(makeMove);
            workThread.Start();
            return;
        }
        for (int i = 2; i < 4; i++)
            if (board[queenLocations[i]] == data.index)
                return;

        board[positions[data.index]] = true;
        data.boarder.color = selectedBoarderColor;
        data.tile.color = blockedTileColor;
        workThread = new Thread(makeMove);
        workThread.Start();
    }
    IEnumerator DisableCamera()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        camera.enabled = false;
    }

    public void moveQueen(int queen, int position)
    {
        for (int i = 0; i < 4; i++)
            if (board[queenLocations[i]] == position)
            {
                board[queenLocations[i]] = board[queenLocations[(i&0b10)+((i+1)&1)]];
                queens[i].gameObject.SetActive(false);
            }
        board[queenLocations[queen]] = position;
        Vector2 pos = new Vector2((position % 4), -(position / 4));
        queens[queen].localPosition = pos;
    }

    public bool[] checkWin()
    {
        if ((board[queenLocations[0]] == board[queenLocations[1]]) && (board[queenLocations[0]] == board[queenLocations[2]] || board[queenLocations[0]] == board[queenLocations[3]]))
            return new bool[] {true, false}
        ;
        if ((board[queenLocations[2]] == board[queenLocations[3]]) && (board[queenLocations[0]] == board[queenLocations[2]] || board[queenLocations[1]] == board[queenLocations[2]]))
            return new bool[] {false, true};
        bool[] lose = { false, false };
        for (int i = 0; i < 4; i += 2)
        {
            ushort blocks = (ushort)board.Data;
            uint queen = (uint)1 << board[queenLocations[i]];
            queen |= (uint)1 << board[queenLocations[i + 1]];
            uint surrounding = (((uint)0b10_0000_0000_0010_0010 << board[queenLocations[i]]) | ((uint)0b10_0000_0000_0010_0010 << board[queenLocations[i + 1]])) & 0b1110_1110_1110_0000_1110_1110_1110_1110;
            surrounding |= ((queen * 0b1000_0000_0000_1000) | (queen >> 1)) & 0b111_0111_0111_0000_0111_0111_0111_0111;
            surrounding |= (queen * 0b1_0000_0000_0001_0000) & 0b1111_1111_1111_0000_1111_1111_1111_1111;
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
        foreach (int direction in AI.directions)
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

    private void makeMove()
    {
        //AI.bestMove(board);
    }
}
