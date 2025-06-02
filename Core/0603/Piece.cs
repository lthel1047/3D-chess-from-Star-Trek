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
}