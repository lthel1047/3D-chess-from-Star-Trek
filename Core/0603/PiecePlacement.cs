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
        public GameObject prefab;        // 체스말 프리팹
        public Vector3Int boardPosition; // 보드상 좌표 (x, y, z)
        public bool isWhite;             // 화이트 여부
    }

    private void Awake()
    {
        boardManager = BoardManager.Instance;
        gameFlowManager = FindObjectOfType<GameFlowManager>();
    }

    /// <summary>
    /// 초기 배치 리스트에 맞춰 체스말을 생성 및 셀에 연결
    /// </summary>
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
}
