using UnityEngine;

/// <summary>
/// Сетка фермы: клетки 1x1 единица Unity, как в Unity Grid (клетка = округление вниз).
/// Центр клетки (0,0) находится в точке (0.5, 0.5).
/// </summary>
public static class FarmGrid
{
    public const float CellSize = 1f;

    public static Vector2Int WorldToCell(Vector2 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / CellSize),
            Mathf.FloorToInt(worldPosition.y / CellSize));
    }

    public static Vector2 CellCenter(Vector2Int cell)
    {
        return new Vector2((cell.x + 0.5f) * CellSize, (cell.y + 0.5f) * CellSize);
    }

    public static Vector2Int DirectionToOffset(Direction4 direction)
    {
        switch (direction)
        {
            case Direction4.Up:    return new Vector2Int(0, 1);
            case Direction4.Down:  return new Vector2Int(0, -1);
            case Direction4.Left:  return new Vector2Int(-1, 0);
            case Direction4.Right: return new Vector2Int(1, 0);
            default:               return Vector2Int.zero;
        }
    }
}
