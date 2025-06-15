using UnityEngine;

public class SubBoard : MonoBehaviour
{
    public Vector3Int BoardCorner;

    public void SetCorner(Vector3Int newCorner)
    {
        BoardCorner = newCorner;
        RefreshName();
    }

    public Vector3Int getSubPos() => BoardCorner;

    public void RefreshName()
    {
        gameObject.name = $"SubBoard_Level_{BoardCorner.y}_{BoardCorner.x}_{BoardCorner.z}";
    }
}
