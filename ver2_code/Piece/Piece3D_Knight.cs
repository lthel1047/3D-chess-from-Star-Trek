using System.Collections.Generic;
using UnityEngine;

public class Piece3D_Knight : Piece
{
    public override List<Vector3Int> GetLegalMoves()
    {
        List<Vector3Int> moves = new();
        Vector3Int[] offsets = new Vector3Int[]
        {
            new(1, 2, 0), new(1, -2, 0), new(-1, 2, 0), new(-1, -2, 0),
            new(2, 1, 0), new(2, -1, 0), new(-2, 1, 0), new(-2, -1, 0),
            new(0, 1, 2), new(0, 1, -2), new(0, -1, 2), new(0, -1, -2),
            new(0, 2, 1), new(0, 2, -1), new(0, -2, 1), new(0, -2, -1),
            new(1, 0, 2), new(-1, 0, 2), new(1, 0, -2), new(-1, 0, -2),
            new(2, 0, 1), new(-2, 0, 1), new(2, 0, -1), new(-2, 0, -1)
        };

        foreach (var offset in offsets)
        {
            Vector3Int dest = CurrentPos + offset;
            var cell = BoardManager.Instance.GetCell(dest);
            if (cell != null && (cell.OccupiedPiece == null || cell.OccupiedPiece.IsWhite != this.IsWhite))
                moves.Add(dest);
        }

        moves.AddRange(GetMainBoardVerticalMoves(CurrentPos));

        moves.AddRange(GetSubBoardBridgeMoves(CurrentPos));

        return moves;
    }
}
