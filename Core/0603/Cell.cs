using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Cell : MonoBehaviour
{
    // �׸��� ���� ��ǥ(x, y, z)
    public Vector3Int BoardPosition;

    // �� ���� ���� �⹰ ����
    public Piece OccupiedPiece;

    // ���̶���Ʈ�� ���� ��Ƽ���� ����
    private Material defaultMaterial;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            // �ν��Ͻ�ȭ�� ��Ƽ������ ������ �ξ�, ���� ��Ƽ������ �ƴ� ���� ��Ƽ����� ����
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
