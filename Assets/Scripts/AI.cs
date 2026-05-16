using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class AI
{
    public static ComputeShader shader;
    public static readonly byte[] directions = {1, 3, 4, 5};
    private static readonly uint[] walls = { 0b1110_1110_1110_1110, 0b111_0111_0111_0111, uint.MaxValue, 0b1110_1110_1110_1110, 0b111_0111_0111_0111, 0b1110_1110_1110_1110, uint.MaxValue, 0b111_0111_0111_0111 };

    public static void bestMove(BitVector32 board)
    {
        BitVector32[] boards = {board};
        ComputeBuffer boardBuffer = new ComputeBuffer(boards.Length, 4);
        int kernel = shader.FindKernel("CSMain");
        boardBuffer.SetData(boards);
        shader.SetBuffer(kernel, "boards", boardBuffer);
        shader.SetInt("boardCount", boards.Length);
        shader.Dispatch(kernel, boards.Length/256, 1, 1);
        uint[] output = new uint[boards.Length];
        boardBuffer.GetData(output);
        Debug.Log(output[0]);
    }
    public static Queue<BitVector32> getMoves(BitVector32 board, bool p1Turn = false)
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
        output.TrimExcess();
        return output;
    }
}
