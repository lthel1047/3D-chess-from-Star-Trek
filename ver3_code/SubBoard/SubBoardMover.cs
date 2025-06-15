using System.Collections.Generic;
using UnityEngine;

public class SubBoardMover : MonoBehaviour
{
    public Material highlightMaterial;
    private BoardManager bm;
    private GameFlowManager gm;

    private Vector3Int selectedCorner = new(-99, -99, -99);
    private List<Cell> highlightedCells = new();

    void Start()
    {
        bm = BoardManager.Instance;
        gm = FindObjectOfType<GameFlowManager>();
    }

    void Update()
    {
        if (gm == null || gm.currentPlayer != GameFlowManager.Player.White)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var t = hit.collider.transform;
                while (t != null)
                {
                    if (t.name.StartsWith("SubBoard_Level_"))
                    {
                        string[] tokens = t.name.Split('_');
                        selectedCorner = new Vector3Int(
                            Mathf.FloorToInt(t.transform.position.x),
                            Mathf.FloorToInt(t.transform.position.y),
                            Mathf.FloorToInt(t.transform.position.z)
                        );
                        ShowHighlights(selectedCorner);
                        break;
                    }
                    t = t.parent;
                }
            }
        }

        if (Input.GetMouseButtonDown(0) && selectedCorner.y >= 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null)
                {
                    Vector3Int destCellPos = cell.BoardPosition;

                    //if (SubBoardPathDatabase.IsValidMove(selectedCorner, candidateCorner) && !bm.IsSubLevelPositionOccupied(candidateCorner))
                    if (SubBoardPathDatabase.IsValidMove(selectedCorner, destCellPos))
                    {
                        bm.MoveSubLevel(selectedCorner, destCellPos);

                        ClearHighlights();
                        selectedCorner = new Vector3Int(-99, -99, -99);

                        gm.EndTurn();
                    }
                    else
                    {
                        Debug.Log("이동 실패");
                    }
                }
            }
        }
    }

    /// <summary>  
    /// 서브보드 코너 기준으로 하이라이트할 셀 표시  
    /// </summary>  
    private void ShowHighlights(Vector3Int fromCorner)
    {
        ClearHighlights();

        var validDestinations = SubBoardPathDatabase.GetValidDestinations(fromCorner);

        foreach (var destCorner in validDestinations)
        {
            List<Cell> targetCells = bm.GetSubBoardCells(destCorner);
            foreach (var cell in targetCells)
            {
                if (cell != null)
                {
                    cell.Highlight(highlightMaterial);
                    highlightedCells.Add(cell);
                }
            }
        }
    }

    private void ClearHighlights()
    {
        foreach (var cell in highlightedCells)
            cell.RemoveHighlight();
        highlightedCells.Clear();
    }
}
