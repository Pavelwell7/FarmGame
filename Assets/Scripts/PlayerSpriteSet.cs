using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Таблица картинок персонажа: предмет x направление. Команда назначает спрайты в инспекторе.
/// Создать вручную: Project -> ПКМ -> Create -> Farm -> Player Sprite Set.
/// </summary>
[CreateAssetMenu(menuName = "Farm/Player Sprite Set", fileName = "PlayerSpriteSet")]
public class PlayerSpriteSet : ScriptableObject
{
    [Serializable]
    public class DirectionalSprites
    {
        public Sprite up;
        public Sprite down;
        public Sprite left;
        public Sprite right;

        public Sprite Get(Direction4 direction)
        {
            switch (direction)
            {
                case Direction4.Up:    return up;
                case Direction4.Down:  return down;
                case Direction4.Left:  return left;
                case Direction4.Right: return right;
                default:               return null;
            }
        }
    }

    [Header("Пустые руки (например, морковь = 0)")]
    public DirectionalSprites emptyHands = new DirectionalSprites();

    [Header("Предметы в руках")]
    public DirectionalSprites hoe = new DirectionalSprites();
    public DirectionalSprites seeds = new DirectionalSprites();
    public DirectionalSprites wateringCan = new DirectionalSprites();
    public DirectionalSprites shovel = new DirectionalSprites();
    public DirectionalSprites carrot = new DirectionalSprites();

    /// <summary>item == null означает пустые руки.</summary>
    public Sprite GetSprite(ItemType? item, Direction4 direction)
    {
        return GetSet(item).Get(direction);
    }

    private DirectionalSprites GetSet(ItemType? item)
    {
        if (!item.HasValue)
            return emptyHands;

        switch (item.Value)
        {
            case ItemType.Hoe:         return hoe;
            case ItemType.Seeds:       return seeds;
            case ItemType.WateringCan: return wateringCan;
            case ItemType.Shovel:      return shovel;
            case ItemType.Carrot:      return carrot;
            default:                   return emptyHands;
        }
    }

    // Проверка при замене картинок: размер, pivot и масштаб должны совпадать у всех спрайтов,
    // иначе положение ног и размер персонажа "поедут".
    private void OnValidate()
    {
        var all = new List<KeyValuePair<string, Sprite>>();
        AddAll(all, "emptyHands", emptyHands);
        AddAll(all, "hoe", hoe);
        AddAll(all, "seeds", seeds);
        AddAll(all, "wateringCan", wateringCan);
        AddAll(all, "shovel", shovel);
        AddAll(all, "carrot", carrot);

        Sprite reference = null;
        string referenceName = null;

        foreach (var pair in all)
        {
            if (pair.Value == null)
                continue;

            if (reference == null)
            {
                reference = pair.Value;
                referenceName = pair.Key;
                continue;
            }

            Sprite s = pair.Value;
            bool sameSize = s.rect.size == reference.rect.size;
            bool samePivot = s.pivot == reference.pivot;
            bool samePpu = Mathf.Approximately(s.pixelsPerUnit, reference.pixelsPerUnit);

            if (!sameSize || !samePivot || !samePpu)
            {
                Debug.LogWarning(
                    $"PlayerSpriteSet: спрайт '{s.name}' ({pair.Key}) отличается от '{reference.name}' ({referenceName}) " +
                    "размером, pivot или Pixels Per Unit. Персонаж может сдвинуться или изменить размер.", this);
            }
        }
    }

    private static void AddAll(List<KeyValuePair<string, Sprite>> list, string group, DirectionalSprites sprites)
    {
        if (sprites == null) return;
        list.Add(new KeyValuePair<string, Sprite>(group + ".up", sprites.up));
        list.Add(new KeyValuePair<string, Sprite>(group + ".down", sprites.down));
        list.Add(new KeyValuePair<string, Sprite>(group + ".left", sprites.left));
        list.Add(new KeyValuePair<string, Sprite>(group + ".right", sprites.right));
    }
}
