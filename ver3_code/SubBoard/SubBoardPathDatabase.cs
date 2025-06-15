using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �� ���꺸�� �ڳ� ��ġ���� �̵� ������ ��ġ���� �����ϴ� ���� �����ͺ��̽�
/// </summary>
public static class SubBoardPathDatabase
{

    public static readonly Dictionary<Vector3Int, List<Vector3Int>> AllowedPaths = new()
    {
        // 000 ���꺸�� ���
        [new Vector3Int(-1, 0, -1)] = new List<Vector3Int> { new Vector3Int(-1, 0, 3) },
        [new Vector3Int(-1, 0, 3)] = new List<Vector3Int> { new Vector3Int(-1, 0, -1), new Vector3Int(-1, 4, 1) },
        [new Vector3Int(-1, 4, 1)] = new List<Vector3Int> { new Vector3Int(-1, 0, 3), new Vector3Int(-1, 4, 5) },
        [new Vector3Int(-1, 4, 5)] = new List<Vector3Int> { new Vector3Int(-1, 4, 1) },

        // 040 ���꺸�� ���� ���
        [new Vector3Int(3, 0, -1)] = new List<Vector3Int> { new Vector3Int(3, 0, 3) },
        [new Vector3Int(3, 0, 3)] = new List<Vector3Int> { new Vector3Int(3, 4, 1), new Vector3Int(3, 0, -1) },
        [new Vector3Int(3, 4, 1)] = new List<Vector3Int> { new Vector3Int(3, 4, 5), new Vector3Int(3, 0, 3) },
        [new Vector3Int(3, 4, 5)] = new List<Vector3Int> { new Vector3Int(3, 4, 1) },


        // 204 ��
        [new Vector3Int(-1, 8, 7)] = new List<Vector3Int> { new Vector3Int(-1, 8, 3) },
        [new Vector3Int(-1, 8, 3)] = new List<Vector3Int> { new Vector3Int(-1, 8, 7), new Vector3Int(-1, 4, 5) },
        [new Vector3Int(-1, 4, 5)] = new List<Vector3Int> { new Vector3Int(-1, 8, 3), new Vector3Int(-1, 4, 1) },
        [new Vector3Int(-1, 4, 1)] = new List<Vector3Int> { new Vector3Int(-1, 4, 5)},

        // 244 ��
        [new Vector3Int(3, 8, 7)] = new List<Vector3Int> { new Vector3Int(3, 8, 3) },
        [new Vector3Int(3, 8, 3)] = new List<Vector3Int> { new Vector3Int(3, 8, 7), new Vector3Int(3, 4, 5) },
        [new Vector3Int(3, 4, 5)] = new List<Vector3Int> { new Vector3Int(3, 4, 1), new Vector3Int(3, 8, 7) },
        [new Vector3Int(3, 4, 1)] = new List<Vector3Int> { new Vector3Int(3, 4, 5) },
    };

    /// <summary>
    /// Ư�� ��� ��ġ���� ���� ���� ��ġ ����� ��ȯ
    /// </summary>
    public static List<Vector3Int> GetValidDestinations(Vector3Int from)
    {
        return AllowedPaths.TryGetValue(from, out var list) ? list : new List<Vector3Int>();
    }

    /// <summary>
    /// Ư�� �̵��� ���Ǵ��� Ȯ��
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
