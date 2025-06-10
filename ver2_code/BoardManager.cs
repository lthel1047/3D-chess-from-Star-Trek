using System;
using System.Collections;
using System.Collections.Generic;
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
    public void MoveSubLevel(Vector3Int fromCorner, Vector3Int toCorner)
    {
        int lvl = fromCorner.y;

        for (int i = 0; i < subMappings.Count; i++)
        {
            var m = subMappings[i];
            if (m.level == lvl && m.mainX == fromCorner.x && m.mainZ == fromCorner.z)
            {
                string oldName = $"SubBoard_Level_{lvl}_{m.mainX}_{m.mainZ}";
                Transform subObj = transform.Find(oldName);
                if (subObj == null)
                {
                    Debug.LogError($"서브보드를 찾을 수 없음: {oldName}");
                    return;
                }

                // 새 위치 및 이름 설정
                subObj.name = $"SubBoard_Level_{lvl}_{toCorner.x}_{toCorner.z}";
                subObj.position = CalculateSubBoardWorldPosition(subObj.position, lvl, toCorner.z);

                int shiftZ = toCorner.z - fromCorner.z;

                foreach (Transform child in subObj)
                {
                    var cell = child.GetComponent<Cell>();
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

                subMappings[i] = new SubBoardMapping
                {
                    level = lvl,
                    subX = m.subX,
                    subZ = m.subZ,
                    mainX = toCorner.x,
                    mainZ = toCorner.z,
                    rotY = m.rotY
                };

                break;
            }
        }
    }



    /// 원하는 레벨/코너로 바뀐 서브보드의 새로운 World Position 계산.
    private Vector3 CalculateSubBoardWorldPosition(Vector3 basePos, int level, int targetCornerZ)
    {
        float y = level * subBoardYGap;
        float z = GetZOffsetForLevel(level) + targetCornerZ;
        return new Vector3(basePos.x, y, z);
    }

    private float GetZOffsetForLevel(int level)
    {
        int idx = Array.IndexOf(mainLevels, level);
        return (idx >= 0 && idx < mainBoardZOffsets.Length)
            ? mainBoardZOffsets[idx]
            : level * (mainBoardSize + 1);
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

    #region Utility Methods
    private IEnumerator ActivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }

    public Cell GetCell(Vector3Int pos)
        => boardCells.TryGetValue(pos, out var c) ? c : null;

    public Vector3 GetWorldPosition(Vector3Int bp)
        => new Vector3(bp.x, bp.y, bp.z);

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

    #endregion
}
