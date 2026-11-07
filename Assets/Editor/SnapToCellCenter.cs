using UnityEditor;
using UnityEngine;

/// <summary>
/// Ставит выделенные объекты ровно в центр клетки, в которой они стоят (x.5, y.5).
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
            p.x = (Mathf.Floor(p.x / FarmGrid.CellSize) + 0.5f) * FarmGrid.CellSize;
            p.y = (Mathf.Floor(p.y / FarmGrid.CellSize) + 0.5f) * FarmGrid.CellSize;
            t.position = p;
        }
    }
}