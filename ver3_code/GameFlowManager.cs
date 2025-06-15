using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public enum Player { White, Black }
    public Player currentPlayer = Player.White;

    private BoardManager bm;
    private Piece3D_King whiteKing, blackKing;

    private void Awake()
    {
        bm = BoardManager.Instance;
    }

    private void Start()
    {
        currentPlayer = Player.White;
    }


    #region Setup  King Detection
    public void PieceSettingEnd()
    {
        whiteKing = FindKing(true);
        blackKing = FindKing(false);
    }

    private Piece3D_King FindKing(bool isWhite)
    {
        foreach (var p in bm.GetAllPieces(isWhite))
            if (p is Piece3D_King king)
                return king;
        Debug.LogError($"King not found for {(isWhite ? "White" : "Black")}");
        return null;
    }

    #endregion

    #region Turn & Movement Logic
    public bool TryMovePiece(Piece piece, Vector3Int dest)
    {
        if ((piece.IsWhite && currentPlayer != Player.White) ||
            (!piece.IsWhite && currentPlayer != Player.Black))
            return false;

        var legal = piece.GetLegalMoves();
        if (!legal.Contains(dest)) return false;

        Vector3Int orig = piece.CurrentPos;
        var captured = bm.GetCell(dest).OccupiedPiece;

        // 실행 전 이동
        bm.GetCell(orig).OccupiedPiece = null;
        piece.Setup(piece.IsWhite, dest);
        bm.GetCell(dest).OccupiedPiece = piece;

        // 체크 & 체크메이트 확인
        if (IsInCheck(currentPlayer))
        {
            piece.Setup(piece.IsWhite, orig);
            bm.GetCell(orig).OccupiedPiece = piece;
            bm.GetCell(dest).OccupiedPiece = captured;
            return false;
        }

        EndTurn();
        return true;
    }

    private bool IsInCheck(Player p)
    {
        var kingPos = (p == Player.White ? whiteKing : blackKing)?.CurrentPos ?? new Vector3Int();
        foreach (var p2 in bm.GetAllPieces(p != Player.White))
            if (p2.GetLegalMoves().Contains(kingPos)) return true;
        return false;
    }

    private bool IsCheckmate(Player p)
    {
        if (!IsInCheck(p)) return false;
        foreach (var pi in bm.GetAllPieces(p == Player.White))
            foreach (var mv in pi.GetLegalMoves())
                if (CheckSimulatedEscape(pi, mv, p)) return false;
        return true;
    }

    private bool CheckSimulatedEscape(Piece pi, Vector3Int mv, Player p)
    {
        var orig = pi.CurrentPos;
        var targetCell = bm.GetCell(mv);
        var captured = targetCell.OccupiedPiece;

        bm.GetCell(orig).OccupiedPiece = null;
        pi.Setup(pi.IsWhite, mv);
        targetCell.OccupiedPiece = pi;

        bool stillIn = IsInCheck(p);

        pi.Setup(pi.IsWhite, orig);
        bm.GetCell(orig).OccupiedPiece = pi;
        targetCell.OccupiedPiece = captured;

        return !stillIn;
    }

    private bool isTurnEnding = false;
    public void EndTurn()
    {
        if (isTurnEnding) return;
        isTurnEnding = true;

        currentPlayer = (currentPlayer == Player.White) ? Player.Black : Player.White;
        FindObjectOfType<UIManager>()?.UpdateTurnUI(currentPlayer);

        StartCoroutine(DelayedAIAndReset());
    }

    private IEnumerator DelayedAIAndReset()
    {
        yield return new WaitForSeconds(0.5f);

        yield return null;

        if (currentPlayer == Player.Black)
        {
            AIPlayer ai = FindObjectOfType<AIPlayer>();
            if (ai != null) ai.TakeTurn();
        }

        isTurnEnding = false;
    }


    #endregion
}
