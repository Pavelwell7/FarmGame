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

    /// <summary>True только в кадре нового нажатия E. Удержание не считается.</summary>
    public static bool InteractPressedThisFrame()
    {
        // TODO этап 5: вернуть false, пока открыто окно сна.
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && kb.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
