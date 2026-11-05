using UnityEngine;

/// <summary>
/// Рисует линии сетки в окне Scene, чтобы видеть клетки. В игре ничего не делает.
/// Повесьте на пустой объект. В Scene должна быть включена кнопка Gizmos.
/// </summary>
public class FarmGridGizmo : MonoBehaviour
{
    [SerializeField] private int halfWidth = 20;
    [SerializeField] private int halfHeight = 12;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);

        for (int x = -halfWidth; x <= halfWidth; x++)
            Gizmos.DrawLine(new Vector3(x, -halfHeight, 0f), new Vector3(x, halfHeight, 0f));

        for (int y = -halfHeight; y <= halfHeight; y++)
            Gizmos.DrawLine(new Vector3(-halfWidth, y, 0f), new Vector3(halfWidth, y, 0f));
    }
}
