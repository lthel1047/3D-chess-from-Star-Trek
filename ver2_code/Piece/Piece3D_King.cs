using System.Collections.Generic;
using UnityEngine;

public class Piece3D_King : Piece
{
    public override List<Vector3Int> GetLegalMoves()
    {
        List<Vector3Int> moves = new();
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;
                    Vector3Int dest = CurrentPos + new Vector3Int(dx, dy, dz);
                    var cell = BoardManager.Instance.GetCell(dest);
                    if (cell != null && (cell.OccupiedPiece == null || cell.OccupiedPiece.IsWhite != this.IsWhite))
                        moves.Add(dest);
                }

        moves.AddRange(GetMainBoardVerticalMoves(CurrentPos));

        moves.AddRange(GetSubBoardBridgeMoves(CurrentPos));

        return moves;
    }
}
