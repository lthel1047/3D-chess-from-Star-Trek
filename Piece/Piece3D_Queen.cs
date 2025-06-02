using System.Collections.Generic;
using UnityEngine;

public class Piece3D_Queen : Piece
{
    public override List<Vector3Int> GetLegalMoves()
    {
        var moves = new List<Vector3Int>();
        // ∑Ë + ∫ÒºÛ ∞·«’
        moves.AddRange(new Piece3D_Rook { IsWhite = IsWhite, CurrentPos = CurrentPos }.GetLegalMoves());
        moves.AddRange(new Piece3D_Bishop { IsWhite = IsWhite, CurrentPos = CurrentPos }.GetLegalMoves());
        return moves;
    }
}
