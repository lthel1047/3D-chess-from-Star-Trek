using System.Collections.Generic;
using UnityEngine;

/// 게임 흐름과 턴 관리, 플레이어 행동 제어
public class GameFlowManager : MonoBehaviour
{
    public enum Player { White, Black }
    public Player currentPlayer = Player.White;

    private BoardManager bm;
    private Piece3D_King whiteKing, blackKing;

    void Awake()
    {
        bm = BoardManager.Instance;
    }

    void Start()
    {
        currentPlayer = Player.White;
    }

    public void PieceSettingEnd()
    {
        whiteKing = FindKing(true);
        blackKing = FindKing(false);
    }

    private Piece3D_King FindKing(bool isWhite)
    {
        foreach (var piece in bm.GetAllPieces(isWhite))
        {
            if (piece is Piece3D_King)
                return (Piece3D_King)piece;
        }
        Debug.LogError($"King not found for {(isWhite ? "White" : "Black")}");
        return null;
    }

    // 말 이동 시 호출
    public bool TryMovePiece(Piece piece, Vector3Int dest)
    {
        if ((piece.IsWhite && currentPlayer != Player.White)
         || (!piece.IsWhite && currentPlayer != Player.Black))
            return false;

        var legal = piece.GetLegalMoves();
        if (!legal.Contains(dest)) return false;

        // 임시 이동
        var origin = piece.CurrentPos;
        var captured = bm.GetCell(dest).OccupiedPiece;
        bm.GetCell(origin).OccupiedPiece = null;
        piece.Setup(piece.IsWhite, dest);
        bm.GetCell(dest).OccupiedPiece = piece;

        // 이동 후 체크 여부 확인
        if (IsInCheck(currentPlayer))
        {
            // 불법: 체크 상태 유지 -> 롤백
            piece.Setup(piece.IsWhite, origin);
            bm.GetCell(origin).OccupiedPiece = piece;
            bm.GetCell(dest).OccupiedPiece = captured;
            return false;
        }

        EndTurn();
        return true;
    }

    // 턴 종료와 교대
    private void EndTurn()
    {
        currentPlayer = (currentPlayer == Player.White) ? Player.Black : Player.White;
        // 상대가 체크메이트인지 확인
        if (IsCheckmate(currentPlayer))

            // Fixed the problematic line causing multiple syntax errors.  
            Debug.Log($"{(currentPlayer == Player.White ? "White" : "Black")} is checkmated!");
    }

    // 특정 플레이어가 체크 상태인지
    private bool IsInCheck(Player p)
    {
        var kingPos = (p == Player.White ? whiteKing : blackKing).CurrentPos;
        foreach (var kv in bm.GetAllPieces(!p.Equals(Player.White)))
        {
            if (kv.GetLegalMoves().Contains(kingPos))
                return true;
        }
        return false;
    }

    // 체크메이트: 체크 상태이고, 탈출 수가 없는 경우
    private bool IsCheckmate(Player p)
    {
        if (!IsInCheck(p)) return false;
        foreach (var piece in bm.GetAllPieces(p == Player.White))
        {
            foreach (var mv in piece.GetLegalMoves())
            {
                // 시뮬레이트 이동
                var orig = piece.CurrentPos;
                var targetCell = bm.GetCell(mv);
                var captured = targetCell.OccupiedPiece;

                bm.GetCell(orig).OccupiedPiece = null;
                piece.Setup(piece.IsWhite, mv);
                targetCell.OccupiedPiece = piece;

                bool stillInCheck = IsInCheck(p);

                // 롤백
                piece.Setup(piece.IsWhite, orig);
                bm.GetCell(orig).OccupiedPiece = piece;
                targetCell.OccupiedPiece = captured;

                if (!stillInCheck)
                    return false;
            }
        }
        return true;
    }
}
