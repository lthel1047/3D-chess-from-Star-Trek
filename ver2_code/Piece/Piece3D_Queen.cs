using System.Collections.Generic;
using UnityEngine;

public class Piece3D_Queen : Piece
{
    public override List<Vector3Int> GetLegalMoves()
    {
        // Rook + Bishop а╤гу
        var moves = new List<Vector3Int>();
        moves.AddRange(new Piece3D_Rook { CurrentPos = this.CurrentPos, IsWhite = this.IsWhite }.GetLegalMoves());
        moves.AddRange(new Piece3D_Bishop { CurrentPos = this.CurrentPos, IsWhite = this.IsWhite }.GetLegalMoves());

        moves.AddRange(GetMainBoardVerticalMoves(CurrentPos));

        moves.AddRange(GetSubBoardBridgeMoves(CurrentPos));

        return moves;
    }
}
