using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Клавиши 1-5 выбирают предмет. Больше ничего не делает.
/// Вешается на тот же объект, что и InventoryModel.
/// </summary>
[RequireComponent(typeof(InventoryModel))]
public class InventoryInput : MonoBehaviour
{
    private InventoryModel inventory;

    private void Awake()
    {
        inventory = GetComponent<InventoryModel>();
    }

    private void Update()
    {
        // TODO этап 5: не принимать выбор, пока открыто окно сна.
        for (int i = 0; i < InventoryModel.Slots.Length; i++)
        {
            if (WasPressed(i))
            {
                inventory.Select(InventoryModel.Slots[i]);
                break;
            }
        }
    }

    private static bool WasPressed(int index)
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null)
            return false;

        switch (index)
        {
            case 0: return kb.digit1Key.wasPressedThisFrame;
            case 1: return kb.digit2Key.wasPressedThisFrame;
            case 2: return kb.digit3Key.wasPressedThisFrame;
            case 3: return kb.digit4Key.wasPressedThisFrame;
            case 4: return kb.digit5Key.wasPressedThisFrame;
            default: return false;
        }
#else
        return Input.GetKeyDown(KeyCode.Alpha1 + index);
#endif
    }
}
