using UnityEngine;

/// <summary>
/// Выбирает картинку персонажа по двум значениям: предмет (из InventoryModel) и направление (из PlayerMovement).
/// Только меняет спрайт. Ничего не расходует, грядку не трогает, размер/позицию/коллайдер не меняет.
/// </summary>
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private InventoryModel inventory;
    [SerializeField] private PlayerSpriteSet spriteSet;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Reset()
    {
        movement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (inventory == null || spriteSet == null || movement == null || spriteRenderer == null)
        {
            Debug.LogError("PlayerVisual: не назначены Inventory, Sprite Set, Movement или Sprite Renderer. Проверьте компонент в инспекторе.", this);
            enabled = false;
            return;
        }

        inventory.Changed += Refresh;
        movement.FacingChanged += OnFacingChanged;
        Refresh();
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.Changed -= Refresh;
        if (movement != null) movement.FacingChanged -= OnFacingChanged;
    }

    private void OnFacingChanged(Direction4 direction)
    {
        Refresh();
    }

    private void Refresh()
    {
        ItemType? held = inventory.IsHoldingSomething ? inventory.Selected : (ItemType?)null;
        Sprite sprite = spriteSet.GetSprite(held, movement.Facing);

        if (sprite == null)
        {
            Debug.LogError($"PlayerVisual: в Sprite Set нет картинки для предмета '{(held.HasValue ? held.Value.ToString() : "пустые руки")}' и направления '{movement.Facing}'.", spriteSet);
            return;
        }

        spriteRenderer.sprite = sprite;
    }
}
