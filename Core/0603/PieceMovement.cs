using System.Collections.Generic;
using UnityEngine;

public class PieceMovement : MonoBehaviour
{
    public Camera mainCamera;
    public Material highlightMaterial;
    private Material originalMaterial;

    private Piece selectedPiece;
    private List<Cell> highlightedCells = new List<Cell>();

    void Update()
    {
        // ���콺 Ŭ������ �⹰ ����/�̵�
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var piece = hit.collider.GetComponent<Piece>();
                if (piece != null)
                {
                    SelectPiece(piece);
                    return;
                }

                var cell = hit.collider.GetComponent<Cell>();
                if (cell != null)
                    HandleCellClick(cell);
            }
        }
    }

    private void SelectPiece(Piece piece)
    {
        ClearHighlights();
        selectedPiece = piece;

        // �չ� �̵� ��� ��������
        List<Vector3Int> moves = selectedPiece.GetLegalMoves();
        foreach (var pos in moves)
        {
            Cell target = BoardManager.Instance.GetCell(pos);
            if (target == null)
            {
                Debug.LogWarning($"���� ã�� �� ����: {pos}");
                continue;
            }

            // ���̶���Ʈ ����
            target.Highlight(highlightMaterial);
            highlightedCells.Add(target);
        }
    }

    private void HandleCellClick(Cell cell)
    {
        if (selectedPiece != null && highlightedCells.Contains(cell))
        {
            Vector3Int originPos = selectedPiece.CurrentPos;
            Cell originCell = BoardManager.Instance.GetCell(originPos);

            // �̵��� ���� �ִ� �⹰(������) ����
            var captured = cell.OccupiedPiece;

            // �̵� ó��
            originCell.OccupiedPiece = null;
            selectedPiece.Setup(selectedPiece.IsWhite, cell.BoardPosition);
            cell.OccupiedPiece = selectedPiece;

            ClearHighlights();
            selectedPiece = null;
        }
        else
        {
            ClearHighlights();
            selectedPiece = null;
        }
    }

    private void ClearHighlights()
    {
        foreach (var c in highlightedCells)
        {
            var rend = c.GetComponent<Renderer>();
            if (rend != null)
                rend.material = originalMaterial;
        }
        highlightedCells.Clear();
    }
}
