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
                    Debug.Log($"���� �� �� �Դϴ�. - {gm.currentPlayer}");
                }
            }
        }
    }

    #region Piece Selection & Movement

    /// �⹰�� �����ϰ� �̵� ������ ���� ���̶���Ʈ
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
                Debug.LogWarning($"���� ã�� �� ����: {pos}");
                continue;
            }

            target.Highlight(highlightMaterial);
            highlightedCells.Add(target);
        }
    }

    /// �� Ŭ�� �� �̵� ���� ���θ� �Ǵ��ϰ� �̵�
    private void HandleCellClick(Cell cell)
    {
        if (selectedPiece != null && highlightedCells.Contains(cell))
        {
            Vector3Int originPos = selectedPiece.CurrentPos;
            Cell originCell = BoardManager.Instance.GetCell(originPos);
            Vector3Int targetPos = cell.BoardPosition;

            // �̵� ó��
            originCell.OccupiedPiece = null;
            selectedPiece.CurrentPos = targetPos;
            selectedPiece.transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z);
            cell.OccupiedPiece = selectedPiece;

            ClearHighlights();
            selectedPiece = null;

            gm.EndTurn(); // �� ����
        }
        else
        {
            ClearHighlights();
            selectedPiece = null;
        }
    }

    #endregion

    #region Highlight Management

    /// ���̶���Ʈ�� ������ ��� �ʱ�ȭ
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
