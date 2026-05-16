using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

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
    public InputActionReference click;

    private SpaceData[,] spaces = new SpaceData[4, 4];
    private byte mouseOver;
    public new Camera camera;
    private SpaceData selected;
    private bool rendering = false;
    private bool working = false;

    private static BitVector32 board;
    public static int[] positions = new int[16];
    public static BitVector32.Section[] queenLocations = new BitVector32.Section[4];

    void Awake()
    {
        for (float i = -1.5f, k = 0; k < 4; i++, k++)
            for (float j = -1.5f, l = 0, m = 12; l < 4; j++, l++, m -= 4)
            {
                spaces[(int)k, (int)l] = Instantiate(space, new Vector3(i, j, 0), new Quaternion(0, 0, 0, 0), transform).GetComponent<SpaceData>();
                spaces[(int)k, (int)l].index = (byte)(m + k);
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
        moveQueen(0, 13);
        moveQueen(1, 14);
        moveQueen(3, 3);

        camera = Camera.main;
        renderFrame();
        Time.fixedDeltaTime = float.PositiveInfinity;

        click.action.Enable();
        click.action.started += Press;
        click.action.canceled += Release;
    }
    async void renderFrame()
    {
        rendering = true;
        Application.targetFrameRate = 0;
        OnDemandRendering.renderFrameInterval = 1;
        await Awaitable.EndOfFrameAsync();
        Application.targetFrameRate = 4;
        OnDemandRendering.renderFrameInterval = int.MaxValue;
        await Awaitable.NextFrameAsync();
        rendering = false;
    }

    void Press(InputAction.CallbackContext obj)
    {
        if (rendering)
            return;
        SpaceData data = getSpace(camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        if (data == null)
            return;
        mouseOver = data.index;
    }
    void Release(InputAction.CallbackContext obj)
    {
        if (rendering)
            return;
        SpaceData data = getSpace(camera.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        if (data == null || mouseOver != data.index)
            return;
        Click(data);
        bool[] winners = checkWin();
        if (winners[0] || winners[1])
        {
            winController.gameEnd(winners[1], winners[0]);
            click.action.started -= Press;
            click.action.canceled -= Release;
            click.action.started += ResetLevel;
        }
    }
    private void ResetLevel(InputAction.CallbackContext obj)
    {
        click.action.started -= ResetLevel;
        SceneManager.LoadScene(0);
        Destroy(this);
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
                renderFrame();
                return;
            }
        for (int i = 0; i < 2; i++)
            if (board[queenLocations[i]] == data.index)
            {
                if (selected == null)
                {
                    data.boarder.color = selectedBoarderColor;
                    selected = data;
                    renderFrame();
                }
                return;
            }
        if (working) return;
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
            makeMove();
            renderFrame();
            return;
        }
        for (int i = 2; i < 4; i++)
            if (board[queenLocations[i]] == data.index)
                return;

        board[positions[data.index]] = true;
        data.tile.color = blockedTileColor;
        makeMove();
        renderFrame();
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

    async void makeMove()
    {
        working = true;
        await AI.bestMove(board);
        working = false;
    }
}
