using System.Collections.Generic;
using UnityEngine;

/// ���� �帧�� �� ����, �÷��̾� �ൿ ����
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

    // �� �̵� �� ȣ��
    public bool TryMovePiece(Piece piece, Vector3Int dest)
    {
        if ((piece.IsWhite && currentPlayer != Player.White)
         || (!piece.IsWhite && currentPlayer != Player.Black))
            return false;

        var legal = piece.GetLegalMoves();
        if (!legal.Contains(dest)) return false;

        // �ӽ� �̵�
        var origin = piece.CurrentPos;
        var captured = bm.GetCell(dest).OccupiedPiece;
        bm.GetCell(origin).OccupiedPiece = null;
        piece.Setup(piece.IsWhite, dest);
        bm.GetCell(dest).OccupiedPiece = piece;

        // �̵� �� üũ ���� Ȯ��
        if (IsInCheck(currentPlayer))
        {
            // �ҹ�: üũ ���� ���� -> �ѹ�
            piece.Setup(piece.IsWhite, origin);
            bm.GetCell(origin).OccupiedPiece = piece;
            bm.GetCell(dest).OccupiedPiece = captured;
            return false;
        }

        EndTurn();
        return true;
    }

    // �� ����� ����
    private void EndTurn()
    {
        currentPlayer = (currentPlayer == Player.White) ? Player.Black : Player.White;
        // ��밡 üũ����Ʈ���� Ȯ��
        if (IsCheckmate(currentPlayer))

            // Fixed the problematic line causing multiple syntax errors.  
            Debug.Log($"{(currentPlayer == Player.White ? "White" : "Black")} is checkmated!");
    }

    // Ư�� �÷��̾ üũ ��������
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

    // üũ����Ʈ: üũ �����̰�, Ż�� ���� ���� ���
    private bool IsCheckmate(Player p)
    {
        if (!IsInCheck(p)) return false;
        foreach (var piece in bm.GetAllPieces(p == Player.White))
        {
            foreach (var mv in piece.GetLegalMoves())
            {
                // �ùķ���Ʈ �̵�
                var orig = piece.CurrentPos;
                var targetCell = bm.GetCell(mv);
                var captured = targetCell.OccupiedPiece;

                bm.GetCell(orig).OccupiedPiece = null;
                piece.Setup(piece.IsWhite, mv);
                targetCell.OccupiedPiece = piece;

                bool stillInCheck = IsInCheck(p);

                // �ѹ�
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
