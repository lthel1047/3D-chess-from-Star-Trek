using System;
using System.Collections.Generic;
using UnityEngine;

public class SubBoardMover : MonoBehaviour
{
    public Material highlightMaterial;

    private int selectedSubLevel = -1;
    private int selectedMainX = -1, selectedMainZ = -1;
    private readonly int subX = 1, subZ = 1;

    private List<Cell> highlightedCells = new();
    private List<Vector3> debugCubePositions = new();

    private BoardManager bm;
    private GameFlowManager gm;


    private void Start()
    {
        bm = BoardManager.Instance;
        gm = FindObjectOfType<GameFlowManager>();
    }

    private void Update()
    {
        if (gm == null || gm.currentPlayer != GameFlowManager.Player.White) return;
        if (Input.GetMouseButtonDown(1)) TrySelectSubBoard();
        if (Input.GetMouseButtonDown(0) && selectedSubLevel >= 0) TryMoveSubBoard();
    }

    #region Sub-board Selection & Movement
    // 우클릭으로 서브보드 선택
    void TrySelectSubBoard()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            for (var t = hit.collider.transform; t != null; t = t.parent)
            {
                if (t.name.StartsWith("SubBoard_Level_"))
                {
                    var tok = t.name.Split('_');
                    selectedSubLevel = int.Parse(tok[2]);
                    selectedMainX = int.Parse(tok[3]);
                    selectedMainZ = int.Parse(tok[4]);
                    var corner = new Vector3Int(selectedMainX, selectedSubLevel, selectedMainZ);
                    ShowHighlightableSubBoardSpots(selectedSubLevel, corner);
                    return;
                }
            }
        }
    }

    // 좌클릭으로 서브 이동
    void TryMoveSubBoard()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            var cell = hit.collider.GetComponent<Cell>();
            if (cell == null) return;

            var newCorner = cell.BoardPosition;
            // 이미 이동 대상인지 확인
            if (bm.IsSubLevelPositionOccupied(newCorner)) return;

            var fromCorner = new Vector3Int(selectedMainX, selectedSubLevel, selectedMainZ);
            bm.MoveSubLevel(fromCorner, newCorner);
            ClearHighlights();

            selectedSubLevel = -1;
            gm.EndTurn();
        }
    }

    #endregion

    #region Highlight Logic

    void OnDrawGizmos()
    {
        if (debugCubePositions.Count == 0) return;
        Gizmos.color = Color.red;
        foreach (var pos in debugCubePositions)
            Gizmos.DrawCube(pos, Vector3.one * 0.1f);
    }

    /// 유효한 서브보드 이동 위치에만 하이라이트.
    /// 화이트(x<2)면 좌측, 블랙이면 우측 모서리만 표시.
    void ShowHighlightableSubBoardSpots(int level, Vector3Int corner)
    {
        ClearHighlights();
        debugCubePositions.Clear();

        int b = bm.mainBoardSize;
        bool isWhite = corner.x < 2;
        var candidates = new List<Vector3Int>()
        {
            isWhite ? new Vector3Int(0,level,0) : new Vector3Int(b-2,level,0),
            isWhite ? new Vector3Int(0,level,b-2) : new Vector3Int(b-2,level,b-2)
        };

        foreach (var c in candidates)
        {
            if (bm.IsSubLevelPositionOccupied(c)) continue;
            if (!(c.y == corner.y || Math.Abs(c.y - corner.y) == 1)) continue;

            if (ValidateCorners(c, out List<Vector3Int> cells))
            {
                foreach (var p in cells)
                {
                    var cell = bm.GetCell(p);
                    cell?.Highlight(highlightMaterial);
                    highlightedCells.Add(cell);
                    debugCubePositions.Add(bm.GetWorldPosition(p));
                }
            }
        }
    }

    bool ValidateCorners(Vector3Int corner, out List<Vector3Int> cells)
    {
        cells = new();
        for (int dx = 0; dx < bm.subBoardSize; dx++)
            for (int dz = 0; dz < bm.subBoardSize; dz++)
                cells.Add(new Vector3Int(corner.x + dx, corner.y, corner.z + dz));

        foreach (var p in cells)
            if (bm.GetCell(p) == null) return false;

        return true;
    }

    /// 하이라이트 제거 및 내부 리스트 초기화
    void ClearHighlights()
    {
        foreach (var c in highlightedCells)
            c.RemoveHighlight();
        highlightedCells.Clear();
    }

    #endregion
}
