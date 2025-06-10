using System.Collections.Generic;
using UnityEngine;

public class Piece3D_Bishop : Piece
{
    public override List<Vector3Int> GetLegalMoves()
    {
        List<Vector3Int> moves = new();
        Vector3Int[] directions = new Vector3Int[]
        {
            new(1, 0, 1), new(-1, 0, 1), new(1, 0, -1), new(-1, 0, -1),
            new(1, 1, 1), new(-1, 1, 1), new(1, -1, -1), new(-1, -1, -1),
            new(1, 1, -1), new(-1, 1, -1), new(1, -1, 1), new(-1, -1, 1)
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
