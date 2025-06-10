using System.Collections.Generic;
using UnityEngine;

public class Piece3D_Rook : Piece
{
    public override List<Vector3Int> GetLegalMoves()
    {
        List<Vector3Int> moves = new();
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.right, Vector3Int.left,
            Vector3Int.forward, Vector3Int.back,
            Vector3Int.up, Vector3Int.down
        };

        foreach (var dir in directions)
        {
            Vector3Int pos = CurrentPos;
            while (true)
            {
                pos += dir;
                var cell = BoardManager.Instance.GetCell(pos);
                if (cell == null) break;

                if (cell.OccupiedPiece == null)
                    moves.Add(pos);
                else
                {
                    if (cell.OccupiedPiece.IsWhite != this.IsWhite)
                        moves.Add(pos);
                    break;
                }
            }
        }

        moves.AddRange(GetMainBoardVerticalMoves(CurrentPos));

        moves.AddRange(GetSubBoardBridgeMoves(CurrentPos));

        return moves;
    }
}
