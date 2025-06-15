using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Board Settings")]
    public GameObject whiteCellPrefab, blackCellPrefab;
    public int mainBoardSize = 4;
    public int[] mainLevels = { 0, 1, 2 };
    public float[] mainBoardZOffsets = { 0f, 2f, 4f };
    public float mainBoardYGap = 4f;

    public int subBoardSize = 2;
    public int[] subLevels = { 0, 2 };
    public float subBoardYGap = 3f;

    [Serializable]
    private struct SubBoardMapping { public int level, subX, subZ, mainX, mainZ; public float rotY; }
    private List<SubBoardMapping> subMappings = new();

    public Dictionary<Vector3Int, Cell> boardCells = new();
    private PiecePlacement piecePlacement;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (piecePlacement == null)
            piecePlacement = FindObjectOfType<PiecePlacement>();
    }

    private void Start()
    {
        //StartCoroutine(GenerateBoardsAndSetupPieces());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StopAllCoroutines();
            ResetBoard();
        }
    }

    #region Board Generation
    public void ResetBoard()
    {
        foreach (Transform t in transform) Destroy(t.gameObject);
        boardCells.Clear();
        StartCoroutine(GenerateBoardsAndSetupPieces());
    }

    private IEnumerator GenerateBoardsAndSetupPieces()
    {
        yield return StartCoroutine(GenerateMainBoards());
        yield return StartCoroutine(GenerateSubBoards());
        yield return new WaitForSeconds(1f);
        piecePlacement.PlaceAllPieces();
    }

    private IEnumerator GenerateMainBoards()
    {
        for (int i = 0; i < mainLevels.Length; i++)
        {
            int level = mainLevels[i];
            float zOff = i < mainBoardZOffsets.Length ? mainBoardZOffsets[i] : i * (mainBoardSize + 1);
            var parent = new GameObject($"MainBoard_Level_{level}");
            parent.transform.parent = transform;
            Vector3 origin = new Vector3(0, level * mainBoardYGap, zOff);

            for (int x = 0; x < mainBoardSize; x++)
            {
                for (int z = 0; z < mainBoardSize; z++)
                {
                    var prefab = ((x + z) % 2 == 0) ? blackCellPrefab : whiteCellPrefab;
                    var obj = Instantiate(prefab, origin + new Vector3(x, 0, z), Quaternion.identity, parent.transform);
                    obj.SetActive(false);

                    var cell = obj.GetComponent<Cell>();
                    if (cell != null)
                    {
                        Vector3Int bp = new Vector3Int(Mathf.RoundToInt(obj.transform.position.x),
                                                       Mathf.RoundToInt(obj.transform.position.y),
                                                       Mathf.RoundToInt(obj.transform.position.z));
                        cell.BoardPosition = bp;
                        boardCells[bp] = cell;
                    }
                    StartCoroutine(ActivateAfterDelay(obj, 1f));
                }
            }
            yield return null;
        }
    }

    private IEnumerator GenerateSubBoards()
    {
        // Define default subboard positions
        var maps = new (int level, int subX, int subZ, int mainX, int mainZ, float rotY)[]
        {
            (0,1,1,0,0,0f),
            (0,1,1,4,0,0f),
            (2,1,1,0,4,0f),
            (2,1,1,4,4,0f)
        };

        foreach (var map in maps)
        {
            subMappings.Add(new SubBoardMapping
            {
                level = map.level,
                subX = map.subX,
                subZ = map.subZ,
                mainX = map.mainX,
                mainZ = map.mainZ,
                rotY = map.rotY
            });

            Vector3 relRef = new Vector3(map.mainX, map.level * subBoardYGap, mainBoardZOffsets[Array.IndexOf(mainLevels, map.level)]);
            Vector3 subPos = relRef + new Vector3(0, 0, map.mainZ) - new Vector3(map.subX, 0, map.subZ);

            var subParent = new GameObject($"SubBoard_Level_{map.level}_{map.mainX}_{map.mainZ}");
            subParent.transform.parent = transform;
            subParent.transform.position = subPos;
            subParent.transform.rotation = Quaternion.Euler(0, map.rotY, 0);

            subParent.AddComponent<SubBoard>();
            Vector3Int pos = new Vector3Int(Mathf.RoundToInt(subParent.transform.position.x),
                Mathf.RoundToInt(subParent.transform.position.y), Mathf.RoundToInt(subParent.transform.position.z));
            subParent.GetComponent<SubBoard>().BoardCorner = pos;

            for (int dx = 0; dx < subBoardSize; dx++)
            {
                for (int dz = 0; dz < subBoardSize; dz++)
                {
                    int x = map.mainX + dx - map.subX;
                    int z = map.mainZ + dz - map.subZ;
                    var prefab = ((x + z + map.level) % 2 == 0) ? blackCellPrefab : whiteCellPrefab;
                    var obj = Instantiate(prefab, subParent.transform);
                    obj.transform.localPosition = new Vector3(dx, 2, dz);
                    obj.SetActive(false);

                    var cell = obj.GetComponent<Cell>();
                    if (cell != null)
                    {
                        cell.IsPartOfSubBoard = true;

                        Vector3Int bp = new Vector3Int(
                            Mathf.RoundToInt(obj.transform.position.x),
                            Mathf.RoundToInt(obj.transform.position.y),
                            Mathf.RoundToInt(obj.transform.position.z)
                        );
                        cell.BoardPosition = bp;
                        boardCells[bp] = cell;
                    }
                    StartCoroutine(ActivateAfterDelay(obj, 0.5f));
                }
            }
            yield return null;
        }
    }

    #endregion

    #region Sub board Movement
    /// 서브보드 오브젝트와 내부 셀, 기물 좌표를 함께 갱신
    public bool MoveSubLevelData(Vector3Int fromCorner, Vector3Int toCorner)
    {
        if (IsSubLevelPositionOccupied(toCorner)) return false;

        var fromCells = GetSubBoardCells(fromCorner);
        var toCells = GetSubBoardCells(toCorner);
        if (fromCells.Count != toCells.Count) return false;

        for (int i = 0; i < fromCells.Count; i++)
        {
            var fromCell = fromCells[i];
            var toCell = toCells[i];

            var piece = fromCell.OccupiedPiece;
            toCell.OccupiedPiece = piece;
            fromCell.OccupiedPiece = null;

            //if (piece != null)
            //{
            //    piece.CurrentPos = toCell.BoardPosition;
            //    // 위치는 나중에 보정
            //}
        }

        return true;
    }

    public void MoveSubLevel(Vector3Int fromCorner, Vector3Int toCorner)
    {
        foreach (Transform child in transform)
        {
            SubBoard sb = child.GetComponent<SubBoard>();
            if (sb != null && sb.BoardCorner == fromCorner)
            {
                if (toCorner.y == 4)
                    toCorner.z -= 1;
                child.position = CalculateSubBoardWorldPosition(toCorner.x, toCorner.y, toCorner.z);
                sb.SetCorner(toCorner);

                int shiftZ = toCorner.z - fromCorner.z;
                foreach (Transform c in child)
                {
                    Cell cell = c.GetComponent<Cell>();
                    if (cell == null) continue;

                    Vector3Int oldPos = cell.BoardPosition;
                    Vector3Int newPos = new Vector3Int(oldPos.x, toCorner.y, oldPos.z + shiftZ);

                    boardCells.Remove(oldPos);
                    boardCells[newPos] = cell;
                    cell.BoardPosition = newPos;
                    if (cell.OccupiedPiece != null)
                    {
                        var piece = cell.OccupiedPiece;
                        piece.CurrentPos = newPos;
                        piece.transform.position = cell.transform.position;
                    }
                }
                break;
            }
        }
    }




    /// 원하는 레벨/코너로 바뀐 서브보드의 새로운 World Position 계산.
    private Vector3 CalculateSubBoardWorldPosition(int targetCornerX, int level, int targetCornerZ)
    {
        float x = targetCornerX;
        float y = level;
        float z = targetCornerZ;

        if (targetCornerX == 0)
            x -= 1;

        return new Vector3(x, y, z);
    }

    private float GetZOffsetForLevel(int level)
    {
        int idx = Array.IndexOf(mainLevels, level);
        return (idx >= 0 && idx < mainBoardZOffsets.Length)
            ? mainBoardZOffsets[idx]
            : level * (mainBoardSize + 1);
    }

    public List<Cell> GetSubBoardCells(Vector3Int corner)
    {
        List<Cell> cells = new();
        for (int dx = 0; dx < subBoardSize; dx++)
        {
            for (int dz = 0; dz < subBoardSize; dz++)
            {
                Vector3Int pos = new Vector3Int(corner.x + dx, corner.y, corner.z + dz);
                Cell cell = GetCell(pos);
                if (cell != null)
                    cells.Add(cell);
            }
        }
        return cells;
    }
    #endregion

    #region AI SubBoard

    /// 현재 모든 서브보드 코너 위치 수집
    public List<Vector3Int> GetAllSubBoardCornerPositions()
    {
        List<Vector3Int> subBoardCorners = new();

        int size = mainBoardSize;
        int subSize = subBoardSize;

        for (int x = 0; x <= size - subSize; x++)
        {
            for (int z = 0; z <= size - subSize; z++)
            {
                for (int y = 0; y < 3; y++) // 3층
                {
                    // 기준 위치에 기물이 있는지 → 서브보드 존재 판단
                    Vector3Int corner = new(x, y, z);
                    bool hasSubBoard = true;

                    for (int dx = 0; dx < subSize; dx++)
                    {
                        for (int dz = 0; dz < subSize; dz++)
                        {
                            Vector3Int pos = new(x + dx, y, z + dz);
                            if (GetCell(pos)?.IsPartOfSubBoard != true)
                            {
                                hasSubBoard = false;
                                break;
                            }
                        }
                    }

                    if (hasSubBoard)
                        subBoardCorners.Add(corner);
                }
            }
        }

        return subBoardCorners;
    }

    /// 한 서브보드가 이동 가능한 위치 목록 반환
    public List<Vector3Int> GetValidSubBoardDestinations(Vector3Int from)
    {
        List<Vector3Int> valid = new();

        // z축 앞/뒤 이동
        Vector3Int forward = new(from.x, from.y, from.z + 3);
        Vector3Int backward = new(from.x, from.y, from.z - 3);

        if (!IsSubLevelPositionOccupied(forward)) valid.Add(forward);
        if (!IsSubLevelPositionOccupied(backward)) valid.Add(backward);

        // 층 이동 (y ± 1) + z 보정
        int[] yLevels = { from.y - 1, from.y + 1 };
        foreach (int y in yLevels)
        {
            if (y < 0 || y >= 3) continue; // 레벨 제한

            Vector3Int upForward = new(from.x, y, from.z + 3);
            Vector3Int upBackward = new(from.x, y, from.z - 3);

            if (!IsSubLevelPositionOccupied(upForward)) valid.Add(upForward);
            if (!IsSubLevelPositionOccupied(upBackward)) valid.Add(upBackward);
        }

        return valid;
    }


    #endregion

    public Piece GetPieceAt(Vector3Int boardPosition)
    {
        Cell cell = GetCell(boardPosition);
        if (cell != null)
            return cell.OccupiedPiece;
        return null;
    }

    #region Utility Methods
    private IEnumerator ActivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }

    public Cell GetCell(Vector3Int pos)
        => boardCells.TryGetValue(pos, out var c) ? c : null;

    public Vector3 GetWorldPosition(Vector3Int bp)
    {
        float x = bp.x * 1;
        float y = bp.y;
        float z = bp.z * 1;

        return new Vector3(x, y, z);
    }

    public List<Piece> GetAllPieces(bool isWhite)
    {
        var list = new List<Piece>();
        foreach (var cell in boardCells.Values)
        {
            if (cell.OccupiedPiece != null && cell.OccupiedPiece.IsWhite == isWhite)
                list.Add(cell.OccupiedPiece);
        }
        return list;
    }

    public bool IsSubLevelPositionOccupied(Vector3Int corner)
        => subMappings.Exists(m => m.level == corner.y && m.mainX == corner.x && m.mainZ == corner.z);

    public List<Vector3Int> GetSubBoardHighlightPositions(int level)
    {
        int edge = mainBoardSize - subBoardSize;
        var edges = new int[] { 0, edge };
        var lista = new List<Vector3Int>();

        foreach (int x in edges)
            foreach (int z in edges)
                if (!IsSubLevelPositionOccupied(new Vector3Int(x, level, z)))
                    lista.Add(new Vector3Int(x, level, z));

        return lista;
    }

    public bool SubBoardExistAt(Vector3Int corner)
    {
        for (int dx = 0; dx < subBoardSize; dx++)
        {
            for (int dz = 0; dz < subBoardSize; dz++)
            {
                var pos = new Vector3Int(corner.x + dx, corner.y, corner.z + dz);
                if (!boardCells.ContainsKey(pos))
                    return false;
            }
        }
        return true;
    }
    #endregion
}
