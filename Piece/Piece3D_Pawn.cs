using System.Collections.Generic;
using UnityEngine;

public class Pawn3D : Piece
{
    // 처음 이동 여부를 기록하기 위한 필드
    private bool hasMoved = false;

    public override void Setup(bool isWhite, Vector3Int start)
    {
        base.Setup(isWhite, start);
        // 처음 Setup할 때는 hasMoved = false,
        // 그 이후 한 번이라도 이동했다면 true
        if (hasMoved == false && CurrentPos != start)
            hasMoved = true;
    }

    public override List<Vector3Int> GetLegalMoves()
    {
        var moves = new List<Vector3Int>();
        var bm = BoardManager.Instance;

        int dir = IsWhite ? 1 : -1;

        Vector3Int oneStep = CurrentPos + new Vector3Int(0, 0, dir);
        // 한 칸 전진 위치가 보드 범위 내이고 비어 있으면 추가
        Cell oneCell = bm.GetCell(oneStep);
        if (oneCell != null && oneCell.OccupiedPiece == null)
        {
            moves.Add(oneStep);

            //    아직 한 번도 이동하지 않았고, 두 칸 전진 위치도 비어 있으면 추가
            if (!hasMoved)
            {
                Vector3Int twoStep = CurrentPos + new Vector3Int(0, 0, 2 * dir);
                Cell twoCell = bm.GetCell(twoStep);
                // 중간 한 칸(oneStep)은 이미 비어 있고,
                // 두 칸 전진 위치도 비어 있어야 함
                if (twoCell != null && twoCell.OccupiedPiece == null)
                {
                    moves.Add(twoStep);
                }
            }
        }

        foreach (int dx in new[] { -1, 1 })
        {
            Vector3Int diag = CurrentPos + new Vector3Int(dx, 0, dir);
            Cell diagCell = bm.GetCell(diag);
            if (diagCell != null && diagCell.OccupiedPiece != null
                && diagCell.OccupiedPiece.IsWhite != IsWhite)
            {
                moves.Add(diag);
            }
        }

        return moves;
    }

    // 한 번이라도 이동한 이후 Setup()이 호출되면 hasMoved를 true로 설정
    void OnEnable()
    {
        hasMoved = false;
    }

    void OnDisable()
    {
        hasMoved = true;
    }
}
