using UnityEditor;
using UnityEngine;

/// <summary>
/// Ставит выделенные объекты ровно в центр ближайшей клетки (x.5, y.5).
/// Меню: Farm -> Snap Selected To Cell Center. Файл лежит в папке Assets/Editor.
/// </summary>
public static class SnapToCellCenter
{
    [MenuItem("Farm/Snap Selected To Cell Center")]
    private static void Snap()
    {
        foreach (Transform t in Selection.transforms)
        {
            Undo.RecordObject(t, "Snap to cell center");

            Vector3 p = t.position;
            p.x = (Mathf.Round(p.x - 0.5f) + 0.5f) * FarmGrid.CellSize;
            p.y = (Mathf.Round(p.y - 0.5f) + 0.5f) * FarmGrid.CellSize;
            t.position = p;
        }
    }
}
