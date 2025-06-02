using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Board Settings")]
    public GameObject whiteCellPrefab;
    public GameObject blackCellPrefab;

    public int mainBoardSize = 4;
    public int[] mainLevels = { 0, 1, 2 };
    public float[] mainBoardZOffsets = { 0f, 2f, 4f };
    public float mainBoardYGap = 3f;

    public int subBoardSize = 2;
    public int[] subLevels = { 0, 2 };
    public float subBoardYGap = 3f;

    // 서브보드 매핑 정보 저장
    [Serializable]
    private struct SubBoardMapping { public int level, subX, subZ, mainX, mainZ; public float rotY; }
    private List<SubBoardMapping> subMappings = new List<SubBoardMapping>();

    private Dictionary<Vector3Int, Cell> boardCells = new Dictionary<Vector3Int, Cell>();

    private PiecePlacement piecePlacement;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (piecePlacement == null)
            piecePlacement = GameObject.FindObjectOfType<PiecePlacement>();
    }

    void Start()
    {
        StartCoroutine(GenerateBoardsAndSetupPieces());
    }

    void Update()
    {
        // 리셋 테스트 코드
        if (Input.GetKeyDown(KeyCode.R))
        {
            StopAllCoroutines();
            ResetBoard();
        }
    }

    private void ResetBoard()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
        boardCells.Clear();
        StartCoroutine(GenerateBoardsAndSetupPieces());
    }

    private IEnumerator GenerateBoardsAndSetupPieces()
    {
        yield return StartCoroutine(GenerateMainBoards());
        if (this == null)
            yield break;
        yield return StartCoroutine(GenerateSubBoards());

        yield return new WaitForSeconds(1.0f);
        if(this == null)
            yield break;
        piecePlacement.PlaceAllPieces();
    }

    private IEnumerator GenerateMainBoards()
{
    for (int idx = 0; idx < mainLevels.Length; idx++)
    {
        int level = mainLevels[idx];
        float zOffset = mainBoardZOffsets.Length > idx ? mainBoardZOffsets[idx] : idx * (mainBoardSize + 1);

        var parent = new GameObject($"MainBoard_Level_{level}");
        parent.transform.parent = transform;
        Vector3 origin = new Vector3(0f, level * mainBoardYGap, zOffset);

        if (this == null) yield break;

        for (int x = 0; x < mainBoardSize; x++)
        {
            for (int z = 0; z < mainBoardSize; z++)
            {
                GameObject prefab = ((x + z) % 2 == 0) ? blackCellPrefab : whiteCellPrefab;
                Vector3 pos = origin + new Vector3(x, 0f, z);

                GameObject obj = Instantiate(prefab, pos, Quaternion.identity, parent.transform);
                obj.SetActive(false);

                var cell = obj.GetComponent<Cell>();
                if (cell != null)
                {
                    //Vector3Int boardPos = new Vector3Int(x, level, z);
                    //cell.BoardPosition = boardPos;
                    //boardCells[boardPos] = cell;

                    // 셀의 월드 좌표를 보드 좌표로 변환
                    Vector3 worldPos = obj.transform.position;
                    Vector3Int boardPos = new Vector3Int(
                        Mathf.RoundToInt(worldPos.x),
                        Mathf.RoundToInt(worldPos.y),
                        Mathf.RoundToInt(worldPos.z)
                    );
                    cell.BoardPosition = boardPos;
                    boardCells[boardPos] = cell;

                    // Debug 확인용
                    // Debug.Log($"[MainBoard] 셀 등록: {boardPos}");
                }

                StartCoroutine(ActivateAfterDelay(obj, 1f));
                yield return null;
            }
        }
    }
}


    private IEnumerator GenerateSubBoards()
    {
        float spacing = 1f;
        var mappings = new (int level, int subX, int subZ, int mainX, int mainZ, float rotY)[]
        {
        (0, 1, 1, 0, 0, 0f),  // 서브보드0: 0층, sub(1,1) == main(0,0)
        (0, 1, 1, 4, 0, 0f),  // 서브보드1: 0층, sub(1,1) == main(0,1)
        (2, 1, 1, 0, 4, 0f),  // 서브보드2: 2층, sub(1,1) == main(0,2)
        (2, 1, 1, 4, 4, 0f)   // 서브보드3: 2층, sub(1,1) == main(0,3)
        };

        foreach (var map in mappings)
        {
            float yOffset = map.level * subBoardYGap;
            int idxMain = Array.IndexOf(mainLevels, map.level);
            float zOffset = (idxMain >= 0 && idxMain < mainBoardZOffsets.Length)
                ? mainBoardZOffsets[idxMain]
                : map.level * (mainBoardSize + 1);

            // 기준 reference 월드 좌표
            Vector3 reference = new Vector3(0f, yOffset, zOffset)
                                + new Vector3(map.mainX * spacing, 0f, map.mainZ * spacing);
            // 서브보드 원점
            Vector3 subOrigin = reference
                                - new Vector3(map.subX * spacing, 0f, map.subZ * spacing);

            var subParent = new GameObject($"SubBoard_Level_{map.level}_{map.mainX}_{map.mainZ}");
            subParent.transform.parent = transform;
            subParent.transform.position = subOrigin;
            subParent.transform.rotation = Quaternion.Euler(0f, map.rotY, 0f);

            for (int dx = 0; dx < subBoardSize; dx++)
            {
                for (int dz = 0; dz < subBoardSize; dz++)
                {
                    int x = map.mainX + dx - map.subX;
                    int z = map.mainZ + dz - map.subZ;
                    GameObject prefab = ((x + z + map.level) % 2 == 0)
                        ? blackCellPrefab
                        : whiteCellPrefab;
                    Vector3 localPos = new Vector3(dx * spacing, 2f, dz * spacing);
                    var obj = Instantiate(prefab, subParent.transform);
                    obj.transform.localPosition = localPos;
                    obj.transform.localRotation = Quaternion.identity;
                    obj.SetActive(false);

                    var cell = obj.GetComponent<Cell>();
                    //cell.BoardPosition = new Vector3Int(x, map.level, z);
                    //boardCells[cell.BoardPosition] = cell;

                    Vector3 worldPos = obj.transform.position;
                    Vector3Int boardPos = new Vector3Int(
                        Mathf.RoundToInt(worldPos.x),
                        Mathf.RoundToInt(worldPos.y),
                        Mathf.RoundToInt(worldPos.z)
                    );
                    cell.BoardPosition = boardPos;
                    boardCells[boardPos] = cell;

                    StartCoroutine(ActivateAfterDelay(obj, 0.5f));
                }
                yield return null;
            }
        }
    }




    private IEnumerator ActivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }

    /// 주어진 서브레벨에 속한 모든 셀 반환
    public List<Cell> GetCellsOnSubLevel(int level)
    {
        var list = new List<Cell>();
        foreach (var kv in boardCells)
        {
            if (kv.Key.y == level && Array.IndexOf(subLevels, level) >= 0)
                list.Add(kv.Value);
        }
        return list;
    }

    /// 해당 좌표에 이미 서브보드가 있는지 검사
    public bool IsSubLevelPositionOccupied(Vector3Int corner)
    {
        foreach (var map in subMappings)
            if (map.level == corner.y && map.mainX == corner.x && map.mainZ == corner.z)
                return true;
        return false;
    }

    /// 서브보드 위치 이동: 매핑 업데이트 후 재생성
    public void MoveSubLevel(int levelIndex, Vector3Int toCorner)
    {
        for (int i = 0; i < subMappings.Count; i++)
        {
            if (subMappings[i].level == levelIndex)
            {
                var m = subMappings[i];
                m.mainX = toCorner.x;
                m.mainZ = toCorner.z;
                subMappings[i] = m;
            }
        }
        // 기존 서브보드 제거
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c.name.StartsWith($"SubBoard_Level_{levelIndex}_"))
                Destroy(c.gameObject);
        }
        // 새로 생성
        StartCoroutine(GenerateSubBoards());
    }

    // 지정 색의 모든 기물 리스트
    public List<Piece> GetAllPieces(bool white)
    {
        var list = new List<Piece>();
        foreach (var kv in boardCells)
        {
            var pc = kv.Value.OccupiedPiece;
            if (pc != null && pc.IsWhite == white)
                list.Add(pc);
        }
        return list;
    }

    /// 보드 좌표 -> 월드 좌표 변환
    public Vector3 GetWorldPosition(Vector3Int bp)
    {
        float spacing = 1f;
        return new Vector3(bp.x * spacing, bp.y, bp.z * spacing);
    }

    public Cell GetCell(Vector3Int bp)
    {
        boardCells.TryGetValue(bp, out Cell cell);
        return cell;
    }
}