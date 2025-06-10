using System.Collections.Generic;
using UnityEngine;

public class Piece3D_Pawn : Piece
{
    private bool hasMoved = false;

    public override void Setup(bool isWhite, Vector3Int start)
    {
        base.Setup(isWhite, start);
        if (hasMoved == false && CurrentPos != start)
            hasMoved = true;
    }

    public override List<Vector3Int> GetLegalMoves()
    {
        var moves = new List<Vector3Int>();
        var bm = BoardManager.Instance;
        int dir = IsWhite ? 1 : -1;

        // y 층 변경 고려
        int[] yOffsets = { -1, 0, 1 }; // 현재 층, 아래층, 위층

        foreach (int dy in yOffsets)
        {
            int targetY = CurrentPos.y + dy;

            // 전진 이동 (1칸)
            Vector3Int oneStep = new Vector3Int(CurrentPos.x, targetY, CurrentPos.z + dir);
            Cell oneCell = bm.GetCell(oneStep);
            if (oneCell != null && oneCell.OccupiedPiece == null)
            {
                moves.Add(oneStep);

                // 2칸 전진은 같은 y에서만 가능
                if (!hasMoved && dy == 0)
                {
                    Vector3Int twoStep = new Vector3Int(CurrentPos.x, targetY, CurrentPos.z + 2 * dir);
                    Cell twoCell = bm.GetCell(twoStep);
                    if (twoCell != null && twoCell.OccupiedPiece == null)
                        moves.Add(twoStep);
                }
            }

            // 대각 공격 (양 옆 대각선 + y층 포함)
            foreach (int dx in new int[] { -1, 1 })
            {
                Vector3Int diag = new Vector3Int(CurrentPos.x + dx, targetY, CurrentPos.z + dir);
                Cell diagCell = bm.GetCell(diag);
                if (diagCell != null && diagCell.OccupiedPiece != null && diagCell.OccupiedPiece.IsWhite != IsWhite)
                {
                    moves.Add(diag);
                }
            }
        }
        moves.AddRange(GetMainBoardVerticalMoves(CurrentPos));

        moves.AddRange(GetSubBoardBridgeMoves(CurrentPos));

        return moves;
    }

    void OnEnable()
    {
        hasMoved = false;
    }

    void OnDisable()
    {
        hasMoved = true;
    }
}
