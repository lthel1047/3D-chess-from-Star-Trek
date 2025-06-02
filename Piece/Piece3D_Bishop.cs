using System.Collections.Generic;
using UnityEngine;

public class Piece3D_Bishop : Piece
{
    public override List<Vector3Int> GetLegalMoves()
    {
        var moves = new List<Vector3Int>();
        var bm = BoardManager.Instance;

        // ��� �밢�� 12���� (XY, XZ, YZ)
        Vector3Int[] dirs = new[]
        {
            // XY ���
            new Vector3Int(1,1,0), new Vector3Int(1,-1,0), new Vector3Int(-1,1,0), new Vector3Int(-1,-1,0),
            // XZ ���
            new Vector3Int(1,0,1), new Vector3Int(1,0,-1), new Vector3Int(-1,0,1), new Vector3Int(-1,0,-1),
            // YZ ���
            new Vector3Int(0,1,1), new Vector3Int(0,1,-1), new Vector3Int(0,-1,1), new Vector3Int(0,-1,-1)
        };
        foreach (var d in dirs)
        {
            Vector3Int p = CurrentPos;
            while (true)
            {
                p += d;
                Cell c = bm.GetCell(p);
                if (c == null) break;
                if (c.OccupiedPiece != null)
                {
                    if (c.OccupiedPiece.IsWhite != IsWhite)
                        moves.Add(p);
                    break;
                }
                moves.Add(p);
            }
        }
        return moves;
    }
}
