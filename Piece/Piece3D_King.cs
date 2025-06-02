using System.Collections.Generic;
using UnityEngine;

public class Piece3D_King : Piece
{
    public override List<Vector3Int> GetLegalMoves()
    {
        var moves = new List<Vector3Int>();
        var bm = BoardManager.Instance;

        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;
                    Vector3Int p = CurrentPos + new Vector3Int(dx, dy, dz);
                    Cell c = bm.GetCell(p);
                    if (c != null && (c.OccupiedPiece == null || c.OccupiedPiece.IsWhite != IsWhite))
                        moves.Add(p);
                }
        return moves;
    }
}
