using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Rendering;

public class AI
{
    public static ComputeShader shader;
    public static readonly byte[] directions = {1, 3, 4, 5};
    private static readonly uint[] walls = { 0b1110_1110_1110_1110, 0b111_0111_0111_0111, uint.MaxValue, 0b1110_1110_1110_1110, 0b111_0111_0111_0111, 0b1110_1110_1110_1110, uint.MaxValue, 0b111_0111_0111_0111 };

    async public static Awaitable bestMove(BitVector32 board)
    {
        BitVector32[] boards = {board};

        ComputeBuffer boardBuffer = new ComputeBuffer(boards.Length, 4);
        ComputeBuffer indexBuffer = new ComputeBuffer(boards.Length+1, 4);
        ComputeBuffer indexBuffer2 = new ComputeBuffer(boards.Length+1, 4);

        int movesKernel = shader.FindKernel("CSMain");
        int setIndexKernel = shader.FindKernel("SetIndexStart");
        int scanKernel = shader.FindKernel("Scan");

        boardBuffer.SetData(boards);

        shader.SetBuffer(movesKernel, "boards", boardBuffer);
        shader.SetBuffer(movesKernel, "indexs", indexBuffer);


        shader.SetBuffer(setIndexKernel, "indexs", indexBuffer);

        shader.SetInt("boardCount", boards.Length);
        shader.SetBool("p1Turn", false);

        shader.Dispatch(movesKernel, (boards.Length + 255) / 256, 1, 1);
        shader.Dispatch(setIndexKernel, 1, 1, 1);

        for (int i = 1; i <= (boards.Length + 1); i *= 3)
        {

            shader.SetBuffer(scanKernel, "indexs", indexBuffer);
            shader.SetBuffer(scanKernel, "additionBuffer", indexBuffer2);
            shader.SetInt("offset", i);


            shader.Dispatch(scanKernel, (boards.Length/256)+1, 1, 1);

            ComputeBuffer temp = indexBuffer;
            indexBuffer = indexBuffer2;
            indexBuffer2 = temp;
        }


        uint[] output = new uint[boards.Length + 1];

        AsyncGPUReadbackRequest request = await AsyncGPUReadback.RequestAsync(indexBuffer);
        boardBuffer.Release();
        indexBuffer.Release();
        indexBuffer2.Release();
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }
        output = request.GetData<uint>().ToArray();
        arrayDebug(output);
        return;
    }
    public static BitVector32[] getMoves(BitVector32 board, bool p1Turn = false)
    {
        Queue<BitVector32> output = new Queue<BitVector32>(38);
        uint[] pieces;
        if (p1Turn)
            pieces = new uint[] {
                (uint)(1 << board[MouseClick.queenLocations[0]]),
                (uint)(1 << board[MouseClick.queenLocations[1]]),
                (uint)((1 << board[MouseClick.queenLocations[2]]) | (1 << board[MouseClick.queenLocations[3]]))
            };
        else
            pieces = new uint[] {
                (uint)(1 << board[MouseClick.queenLocations[2]]),
                (uint)(1 << board[MouseClick.queenLocations[3]]),
                (uint)((1 << board[MouseClick.queenLocations[0]]) | (1 << board[MouseClick.queenLocations[1]]))
            };
        uint semiBlock = (ushort)((uint)board.Data | pieces[0] | pieces[1]);
        uint block = semiBlock | pieces[2];
        for (uint i = 1; i <= 0b1000000000000000; i <<= 1)
            if ((block & i) == 0)
                output.Enqueue(new BitVector32(board.Data | (int)i));
        semiBlock = ~semiBlock;
        block = ~block;
        uint[] pieceMoves = { 0, 0 };

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                uint piece = pieces[i];
                for (int k = 0; k < 3; k++)
                {
                    piece <<= directions[j];
                    piece &= walls[j];
                    pieceMoves[i] |= piece & semiBlock;
                    piece &= block;
                    if (piece == 0)
                        break;
                }

            }
            if (pieces[0] == pieces[1])
                break;
        }
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                uint piece = pieces[i];
                for (int k = 0; k < 3; k++)
                {
                    piece >>= directions[j];
                    piece &= walls[j + 4];
                    pieceMoves[i] |= piece & semiBlock;
                    piece &= block;
                    if (piece == 0)
                        break;
                }
            }
            if (pieces[0] == pieces[1])
                break;
        }
        for (uint i = 1, j = 0; j < 16; i <<= 1, j++)
            for (int k = 0; k < 2; k++)
                if ((pieceMoves[k] & i) != 0)
                {
                    BitVector32 newBoard = new BitVector32(board.Data);
                    newBoard[MouseClick.queenLocations[(p1Turn ? 0 : 2) + k]] = (int)j;
                    for (int l = 0; l < 2; l++)
                        if (board[MouseClick.queenLocations[(p1Turn ? 2 : 0) + l]] == (int)j)
                            newBoard[MouseClick.queenLocations[(p1Turn ? 2 : 0) + l]] = board[MouseClick.queenLocations[(p1Turn ? 2 : 0) + ((l + 1) & 1)]];
                    output.Enqueue(newBoard);
                }
        return output.ToArray();
    }




    static void arrayDebug(uint[] array)
    {
        string output = "";
        for (int i = 0; i < array.Length; i++)
            output += array[i] + ", ";
        Debug.Log(output);
    }
}
