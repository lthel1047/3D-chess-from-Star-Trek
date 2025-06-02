using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Cell : MonoBehaviour
{
    // 그리드 상의 좌표(x, y, z)
    public Vector3Int BoardPosition;

    // 셀 위에 놓인 기물 참조
    public Piece OccupiedPiece;

    // 하이라이트용 원본 머티리얼 저장
    private Material defaultMaterial;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            // 인스턴스화된 머티리얼을 복제해 두어, 공유 머티리얼이 아닌 개별 머티리얼로 저장
            defaultMaterial = new Material(rend.material);
    }

    public void Highlight(Material highlightMaterial)
    {
        if (rend == null || highlightMaterial == null)
            return;

        rend.material = highlightMaterial;
    }

    public void RemoveHighlight()
    {
        if (rend == null || defaultMaterial == null)
            return;

        rend.material = defaultMaterial;
    }
}
