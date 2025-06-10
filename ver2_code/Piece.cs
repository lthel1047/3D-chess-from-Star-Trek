using UnityEngine;
using System.Collections.Generic;

public abstract class Piece : MonoBehaviour
{
    public bool IsWhite;
    public Vector3Int CurrentPos;

    public virtual void Setup(bool isWhite, Vector3Int start)
    {
        IsWhite = isWhite;
        CurrentPos = start;
        transform.position = BoardManager.Instance.GetWorldPosition(start);
    }

    public abstract List<Vector3Int> GetLegalMoves();

    protected List<Vector3Int> GetMainBoardVerticalMoves(Vector3Int current)
    {
        List<Vector3Int> moves = new();

        foreach (int level in BoardManager.Instance.mainLevels)
        {
            if (level == current.y) continue;

            Vector3Int target = new(current.x, level, current.z);
            var cell = BoardManager.Instance.GetCell(target);
            if (cell != null && (cell.OccupiedPiece == null || cell.OccupiedPiece.IsWhite != IsWhite))
            {
                moves.Add(target);
            }
        }

        return moves;
    }

    protected List<Vector3Int> GetSubBoardBridgeMoves(Vector3Int current)
    {
        List<Vector3Int> moves = new List<Vector3Int>();

        foreach (int level in BoardManager.Instance.subLevels)
        {
            if (level == current.y) continue;

            Vector3Int target = new Vector3Int(current.x, level, current.z);
            var cell = BoardManager.Instance.GetCell(target);
            if (cell != null && (cell.OccupiedPiece == null || cell.OccupiedPiece.IsWhite != IsWhite))
            {
                moves.Add(target);
            }
        }
        return moves;
    }
}