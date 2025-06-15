using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 서브보드 코너 위치에서 이동 가능한 위치들을 정의하는 정적 데이터베이스
/// </summary>
public static class SubBoardPathDatabase
{

    public static readonly Dictionary<Vector3Int, List<Vector3Int>> AllowedPaths = new()
    {
        // 000 서브보드 경로
        [new Vector3Int(-1, 0, -1)] = new List<Vector3Int> { new Vector3Int(-1, 0, 3) },
        [new Vector3Int(-1, 0, 3)] = new List<Vector3Int> { new Vector3Int(-1, 0, -1), new Vector3Int(-1, 4, 1) },
        [new Vector3Int(-1, 4, 1)] = new List<Vector3Int> { new Vector3Int(-1, 0, 3), new Vector3Int(-1, 4, 5) },
        [new Vector3Int(-1, 4, 5)] = new List<Vector3Int> { new Vector3Int(-1, 4, 1) },

        // 040 서브보드 관련 경로
        [new Vector3Int(3, 0, -1)] = new List<Vector3Int> { new Vector3Int(3, 0, 3) },
        [new Vector3Int(3, 0, 3)] = new List<Vector3Int> { new Vector3Int(3, 4, 1), new Vector3Int(3, 0, -1) },
        [new Vector3Int(3, 4, 1)] = new List<Vector3Int> { new Vector3Int(3, 4, 5), new Vector3Int(3, 0, 3) },
        [new Vector3Int(3, 4, 5)] = new List<Vector3Int> { new Vector3Int(3, 4, 1) },


        // 204 흑
        [new Vector3Int(-1, 8, 7)] = new List<Vector3Int> { new Vector3Int(-1, 8, 3) },
        [new Vector3Int(-1, 8, 3)] = new List<Vector3Int> { new Vector3Int(-1, 8, 7), new Vector3Int(-1, 4, 5) },
        [new Vector3Int(-1, 4, 5)] = new List<Vector3Int> { new Vector3Int(-1, 8, 3), new Vector3Int(-1, 4, 1) },
        [new Vector3Int(-1, 4, 1)] = new List<Vector3Int> { new Vector3Int(-1, 4, 5)},

        // 244 흑
        [new Vector3Int(3, 8, 7)] = new List<Vector3Int> { new Vector3Int(3, 8, 3) },
        [new Vector3Int(3, 8, 3)] = new List<Vector3Int> { new Vector3Int(3, 8, 7), new Vector3Int(3, 4, 5) },
        [new Vector3Int(3, 4, 5)] = new List<Vector3Int> { new Vector3Int(3, 4, 1), new Vector3Int(3, 8, 7) },
        [new Vector3Int(3, 4, 1)] = new List<Vector3Int> { new Vector3Int(3, 4, 5) },
    };

    /// <summary>
    /// 특정 출발 위치에서 도착 가능 위치 목록을 반환
    /// </summary>
    public static List<Vector3Int> GetValidDestinations(Vector3Int from)
    {
        return AllowedPaths.TryGetValue(from, out var list) ? list : new List<Vector3Int>();
    }

    /// <summary>
    /// 특정 이동이 허용되는지 확인
    /// </summary>
    public static bool IsValidMove(Vector3Int from, Vector3Int to)
    {
        foreach (var kvp in AllowedPaths)
        {
            if (Vector3Int.Distance(kvp.Key, from) <= 3)
            {
                foreach (var target in kvp.Value)
                    if (Vector3Int.Distance(target, to) <= 3)
                        return true;
            }
        }
        return false;
    }
}
