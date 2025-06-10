using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AIDifficulty
{
    Easy,
    Medium
}

public class AIPlayer : MonoBehaviour
{
    public AIDifficulty difficulty = AIDifficulty.Easy;

    private BoardManager bm;
    private GameFlowManager gm;
    private bool isThinking = false;

    void Start()
    {
        bm = BoardManager.Instance;
        gm = FindObjectOfType<GameFlowManager>();
    }

    public void SetDifficulty(int level)
    {
        difficulty = (AIDifficulty)level;
        Debug.Log("AI 난이도 설정: " + difficulty);
    }

    public void TakeTurn()
    {
        if (isThinking) return;
        isThinking = true;

        StartCoroutine(difficulty == AIDifficulty.Easy ? ExecuteEasyAI() : ExecuteStrategicAI());
    }

    #region EASY AI
    /// 무작위로 이동 가능한 기물을 골라 이동한다.
    private IEnumerator ExecuteEasyAI()
    {
        yield return new WaitForSeconds(1f);

        foreach (var piece in Shuffle(bm.GetAllPieces(false)))
        {
            foreach (var dest in Shuffle(piece.GetLegalMoves()))
            {
                if (gm.TryMovePiece(piece, dest))
                {
                    yield return FinishTurn();
                    yield break;
                }
            }
        }

        FinishTurn();
    }
    #endregion

    #region STRATEGIC AI
    /// 기물/서브보드 이동을 모두 평가하여 점수가 가장 높은 행동을 수행
    private IEnumerator ExecuteStrategicAI()
    {
        yield return new WaitForSeconds(1f);

        var options = EvaluateAllOptions();
        if (options.Count == 0)
        {
            Debug.Log("AI 가능한 행동 없음");
            yield return FinishTurn();
            yield break;
        }

        var best = options.OrderByDescending(o => o.Score).First();
        ExecuteOption(best);

        yield return new WaitForSeconds(0.5f);
        FinishTurn();
    }

    private void ExecuteOption(AIMoveOption option)
    {
        switch (option.Type)
        {
            case AIMoveOption.MoveType.PieceMove:
                gm.TryMovePiece(option.Piece, option.Target);
                break;
            case AIMoveOption.MoveType.SubBoardMove:
                bm.MoveSubLevel(option.SubBoardFrom, option.SubBoardTo);
                break;
        }
    }

    private List<AIMoveOption> EvaluateAllOptions()
    {
        var options = new List<AIMoveOption>();

        foreach (var piece in bm.GetAllPieces(false))
        {
            foreach (var dest in piece.GetLegalMoves())
            {
                options.Add(new AIMoveOption
                {
                    Type = AIMoveOption.MoveType.PieceMove,
                    Piece = piece,
                    Target = dest,
                    Score = EvaluatePieceMove(piece, dest)
                });
            }
        }

        foreach (var from in bm.GetAllSubBoardCornerPositions())
        {
            if (!bm.IsSubLevelPositionOccupied(from)) continue;

            foreach (var to in bm.GetValidSubBoardDestinations(from))
            {
                if (!bm.IsSubLevelPositionOccupied(to))
                {
                    options.Add(new AIMoveOption
                    {
                        Type = AIMoveOption.MoveType.SubBoardMove,
                        SubBoardFrom = from,
                        SubBoardTo = to,
                        Score = EvaluateSubBoardMove(from, to)
                    });
                }
            }
        }

        return options;
    }

    private int EvaluatePieceMove(Piece piece, Vector3Int dest)
    {
        int score = 0;
        var target = bm.GetCell(dest)?.OccupiedPiece;

        if (target != null && target.IsWhite)
            score += 10;

        if (dest.z >= 2 && dest.z <= 4) // 중앙 진입 보너스
            score += 1;

        return score;
    }

    private int EvaluateSubBoardMove(Vector3Int from, Vector3Int to)
    {
        int score = 5;
        if (Mathf.Abs(from.y - to.y) == 1)
            score += 2;

        return score;
    }
    #endregion

    #region COMMON
    private IEnumerator FinishTurn()
    {
        yield return null;
        gm.EndTurn();
        isThinking = false;
    }

    private List<T> Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
        return list;
    }

    private class AIMoveOption
    {
        public enum MoveType { PieceMove, SubBoardMove }
        public MoveType Type;
        public Piece Piece;
        public Vector3Int Target;
        public Vector3Int SubBoardFrom;
        public Vector3Int SubBoardTo;
        public int Score;
    }
    #endregion
}
