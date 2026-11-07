using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Единое место для чтения мыши и клавиши E (работает и со старым, и с новым Input System).
/// </summary>
public static class GameInput
{
    public static Vector2 MouseScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null ? mouse.position.ReadValue() : new Vector2(-10000f, -10000f);
#else
        return Input.mousePosition;
#endif
    }

    /// <summary>True только в кадре нажатия левой кнопки мыши (для кнопок окна сна).</summary>
    public static bool LeftClickPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    /// <summary>True только в кадре нового нажатия E. Удержание не считается.</summary>
    public static bool InteractPressedThisFrame()
    {
        // Пока открыто окно сна, действия в мире заблокированы.
        if (InputGate.IsBlocked)
            return false;

#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && kb.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}