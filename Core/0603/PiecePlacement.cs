using UnityEngine;
using System.Collections.Generic;

public class PiecePlacement : MonoBehaviour
{
    private BoardManager boardManager;
    private GameFlowManager gameFlowManager;

    public List<InitialPiece> initialPlacements;

    [System.Serializable]
    public struct InitialPiece
    {
        public GameObject prefab;        // ü���� ������
        public Vector3Int boardPosition; // ����� ��ǥ (x, y, z)
        public bool isWhite;             // ȭ��Ʈ ����
    }

    private void Awake()
    {
        boardManager = BoardManager.Instance;
        gameFlowManager = FindObjectOfType<GameFlowManager>();
    }

    /// <summary>
    /// �ʱ� ��ġ ����Ʈ�� ���� ü������ ���� �� ���� ����
    /// </summary>
    public void PlaceAllPieces()
    {
        foreach (var ip in initialPlacements)
        {
            // ���� ��ǥ �� ���� ��ǥ ��ȯ
            Vector3 worldPos = boardManager.GetWorldPosition(ip.boardPosition);
            // �� �ν��Ͻ�ȭ
            GameObject inst = Instantiate(ip.prefab, worldPos, Quaternion.identity, transform);
            inst.name = ip.prefab.name + "_" + ip.boardPosition.x + "_" + ip.boardPosition.y + "_" + ip.boardPosition.z;

            // Piece ������Ʈ �ʱ�ȭ
            var piece = inst.GetComponent<Piece>();
            if (piece != null)
            {
                piece.Setup(ip.isWhite, ip.boardPosition);
                // �ش� �� OccupiedPiece ����
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
}
