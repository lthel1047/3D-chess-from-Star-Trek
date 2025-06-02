using System.Collections.Generic;
using UnityEngine;

public class Piece3D_Knight : Piece
{
    public override List<Vector3Int> GetLegalMoves()
    {
        var moves = new List<Vector3Int>();
        var bm = BoardManager.Instance;

        // 같은 보드 내 L자 이동 8가지 (dy=0)
        Vector3Int[] offsets = new[]
        {
            new Vector3Int(2,0,1), new Vector3Int(2,0,-1), new Vector3Int(-2,0,1), new Vector3Int(-2,0,-1),
            new Vector3Int(1,0,2), new Vector3Int(1,0,-2), new Vector3Int(-1,0,2), new Vector3Int(-1,0,-2)
        };
        foreach (var d in offsets)
        {
            Vector3Int p = CurrentPos + d;
            Cell c = bm.GetCell(p);
            if (c != null && (c.OccupiedPiece == null || c.OccupiedPiece.IsWhite != IsWhite))
                moves.Add(p);
        }
        return moves;
    }
}
