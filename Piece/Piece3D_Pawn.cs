using System.Collections.Generic;
using UnityEngine;

public class Pawn3D : Piece
{
    // ó�� �̵� ���θ� ����ϱ� ���� �ʵ�
    private bool hasMoved = false;

    public override void Setup(bool isWhite, Vector3Int start)
    {
        base.Setup(isWhite, start);
        // ó�� Setup�� ���� hasMoved = false,
        // �� ���� �� ���̶� �̵��ߴٸ� true
        if (hasMoved == false && CurrentPos != start)
            hasMoved = true;
    }

    public override List<Vector3Int> GetLegalMoves()
    {
        var moves = new List<Vector3Int>();
        var bm = BoardManager.Instance;

        int dir = IsWhite ? 1 : -1;

        Vector3Int oneStep = CurrentPos + new Vector3Int(0, 0, dir);
        // �� ĭ ���� ��ġ�� ���� ���� ���̰� ��� ������ �߰�
        Cell oneCell = bm.GetCell(oneStep);
        if (oneCell != null && oneCell.OccupiedPiece == null)
        {
            moves.Add(oneStep);

            //    ���� �� ���� �̵����� �ʾҰ�, �� ĭ ���� ��ġ�� ��� ������ �߰�
            if (!hasMoved)
            {
                Vector3Int twoStep = CurrentPos + new Vector3Int(0, 0, 2 * dir);
                Cell twoCell = bm.GetCell(twoStep);
                // �߰� �� ĭ(oneStep)�� �̹� ��� �ְ�,
                // �� ĭ ���� ��ġ�� ��� �־�� ��
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

    // �� ���̶� �̵��� ���� Setup()�� ȣ��Ǹ� hasMoved�� true�� ����
    void OnEnable()
    {
        hasMoved = false;
    }

    void OnDisable()
    {
        hasMoved = true;
    }
}
