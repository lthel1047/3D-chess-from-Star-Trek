using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIPlayer : MonoBehaviour
{
    public enum Difficulty { Easy, Medium }
    public Difficulty aiDifficulty = Difficulty.Medium;

    private BoardManager bm;
    private GameFlowManager gm;
    private bool isThinking = false;

    private void Awake()
    {
        bm = BoardManager.Instance;
        gm = FindObjectOfType<GameFlowManager>();
    }

    public void SetDifficulty(int level)
    {
        aiDifficulty = (Difficulty)level;
        Debug.Log($"AI 난이도 설정됨: {aiDifficulty}");
    }

    public void TakeTurn()
    {
        if (isThinking) return;
        isThinking = true;

        if (aiDifficulty == Difficulty.Easy)
            StartCoroutine(ExecuteEasyAI());
        else
            StartCoroutine(ExecuteStrategicAI());
    }

    #region EASY AI

    private IEnumerator ExecuteEasyAI()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var piece in Shuffle(bm.GetAllPieces(false)))
        {
            foreach (var dest in Shuffle(piece.GetLegalMoves()))
            {
                if (gm.TryMovePiece(piece, dest))
                {
                    Debug.Log($"Easy AI moved: {piece.name} → {dest}");
                    yield return FinishTurn();
                    yield break;
                }
            }
        }

        yield return FinishTurn(); // 아무것도 못함
    }

    #endregion

    #region MEDIUM AI

    private IEnumerator ExecuteStrategicAI()
    {
        yield return new WaitForSeconds(0.75f);

        if (TryDefendKing()) { yield return FinishTurn(); yield break; }
        if (TryCaptureHighValue()) { yield return FinishTurn(); yield break; }
        if (TryStandardMove()) { yield return FinishTurn(); yield break; }
        if (TrySubBoardMovement()) { yield return FinishTurn(); yield break; }

        Debug.Log("Medium AI도 행동할 수 없어 턴 종료.");
        yield return FinishTurn();
    }

    private bool TryDefendKing()
    {
        var king = bm.GetAllPieces(false).FirstOrDefault(p => p is Piece3D_King);
        if (king == null) return false;

        foreach (var move in Shuffle(king.GetLegalMoves()))
        {
            if (gm.TryMovePiece(king, move))
            {
                Debug.Log($"Medium AI 킹 이동: {move}");
                return true;
            }
        }
        return false;
    }

    private bool TryCaptureHighValue()
    {
        Dictionary<System.Type, int> pieceValue = new()
        {
            { typeof(Piece3D_Queen), 9 },
            { typeof(Piece3D_Rook), 5 },
            { typeof(Piece3D_Bishop), 3 },
            { typeof(Piece3D_Knight), 3 },
            { typeof(Piece3D_Pawn), 1 }
        };

        foreach (var piece in bm.GetAllPieces(false))
        {
            foreach (var move in piece.GetLegalMoves())
            {
                var target = bm.GetPieceAt(move);
                if (target != null && target.IsWhite)
                {
                    int value = pieceValue.GetValueOrDefault(target.GetType(), 0);
                    if (value > 0 && gm.TryMovePiece(piece, move))
                    {
                        Debug.Log($"Medium AI 공격: {piece.name} → {target.name}");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool TryStandardMove()
    {
        foreach (var piece in Shuffle(bm.GetAllPieces(false)))
        {
            foreach (var move in Shuffle(piece.GetLegalMoves()))
            {
                if (gm.TryMovePiece(piece, move))
                {
                    Debug.Log($"Medium AI 이동: {piece.name} → {move}");
                    return true;
                }
            }
        }
        return false;
    }

    private bool TrySubBoardMovement()
    {
        foreach (var from in Shuffle(bm.GetAllSubBoardCornerPositions()))
        {
            foreach (var to in Shuffle(bm.GetValidSubBoardDestinations(from)))
            {
                if (bm.MoveSubLevelData(from, to))
                {
                    Debug.Log($"Medium AI 보드 이동: {from} → {to}");
                    return true;
                }
            }
        }
        return false;
    }

    #endregion

    #region 유틸리티

    private IEnumerator FinishTurn()
    {
        isThinking = false;
        yield return null;
        gm.EndTurn();
    }

    private List<T> Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
        return list;
    }

    #endregion
}
