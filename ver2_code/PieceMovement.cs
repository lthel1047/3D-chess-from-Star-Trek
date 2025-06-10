using System.Collections.Generic;
using UnityEngine;

public class PieceMovement : MonoBehaviour
{
    public Camera mainCamera;
    public Material highlightMaterial;

    private Piece selectedPiece;
    private List<Cell> highlightedCells = new();
    private GameFlowManager gm;

    private void Start()
    {
        gm = FindObjectOfType<GameFlowManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if(gm.currentPlayer == GameFlowManager.Player.White)
                {
                    var piece = hit.collider.GetComponent<Piece>();
                    if (piece != null)
                    {
                        SelectPiece(piece);
                        return;
                    }

                    var cell = hit.collider.GetComponent<Cell>();
                    if (cell != null)
                    {
                        HandleCellClick(cell);
                    }
                }
                else
                {
                    Debug.Log($"현재 블랙 턴 입니다. - {gm.currentPlayer}");
                }
            }
        }
    }

    #region Piece Selection & Movement

    /// 기물을 선택하고 이동 가능한 셀을 하이라이트
    private void SelectPiece(Piece piece)
    {
        ClearHighlights();
        selectedPiece = piece;

        List<Vector3Int> moves = selectedPiece.GetLegalMoves();
        foreach (var pos in moves)
        {
            Cell target = BoardManager.Instance.GetCell(pos);
            if (target == null)
            {
                Debug.LogWarning($"셀을 찾을 수 없음: {pos}");
                continue;
            }

            target.Highlight(highlightMaterial);
            highlightedCells.Add(target);
        }
    }

    /// 셀 클릭 시 이동 가능 여부를 판단하고 이동
    private void HandleCellClick(Cell cell)
    {
        if (selectedPiece != null && highlightedCells.Contains(cell))
        {
            Vector3Int originPos = selectedPiece.CurrentPos;
            Cell originCell = BoardManager.Instance.GetCell(originPos);
            Vector3Int targetPos = cell.BoardPosition;

            // 이동 처리
            originCell.OccupiedPiece = null;
            selectedPiece.CurrentPos = targetPos;
            selectedPiece.transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z);
            cell.OccupiedPiece = selectedPiece;

            ClearHighlights();
            selectedPiece = null;

            gm.EndTurn(); // 턴 종료
        }
        else
        {
            ClearHighlights();
            selectedPiece = null;
        }
    }

    #endregion

    #region Highlight Management

    /// 하이라이트된 셀들을 모두 초기화
    private void ClearHighlights()
    {
        foreach (var c in highlightedCells)
        {
            c.RemoveHighlight();
        }
        highlightedCells.Clear();
    }

    #endregion
}
