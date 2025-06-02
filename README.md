# 3D-chess-from-Star-Trek
개인 포트폴리오 스타트랙 3차워 체스 게임 제작

https://blog.naver.com/michael_1047

자세한 설명은 네이버 블로그 참고 바랍니다.

보드판 생성하는 메인 코드 일부 함수 발췌
```ruby
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
```
기물을 세팅하는 코드에 함수 
```ruby
public void PlaceAllPieces()
{
    foreach (var ip in initialPlacements)
    {
        // 보드 좌표 → 월드 좌표 변환
        Vector3 worldPos = boardManager.GetWorldPosition(ip.boardPosition);
        // 말 인스턴스화
        GameObject inst = Instantiate(ip.prefab, worldPos, Quaternion.identity, transform);
        inst.name = ip.prefab.name + "_" + ip.boardPosition.x + "_" + ip.boardPosition.y + "_" + ip.boardPosition.z;

        // Piece 컴포넌트 초기화
        var piece = inst.GetComponent<Piece>();
        if (piece != null)
        {
            piece.Setup(ip.isWhite, ip.boardPosition);
            // 해당 셀 OccupiedPiece 설정
            var cell = boardManager.GetCell(ip.boardPosition);
            if (cell != null) cell.OccupiedPiece = piece;
        }
        else
        {
            Debug.LogWarning("Prefab missing Piece component: " + ip.prefab.name);
        }
    }
    gameFlowManager.PieceSettingEnd();
}
```
