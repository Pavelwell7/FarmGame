using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 4f;

    /// <summary>Последнее направление взгляда. Сохраняется после остановки.</summary>
    public Direction4 Facing { get; private set; } = Direction4.Down;

    /// <summary>Вызывается, когда направление взгляда изменилось.</summary>
    public event Action<Direction4> FacingChanged;

    private Rigidbody2D body;
    private Vector2 direction;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        direction = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            direction.y += 1;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            direction.y -= 1;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            direction.x -= 1;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            direction.x += 1;
#else
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            direction.y += 1;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            direction.y -= 1;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            direction.x -= 1;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            direction.x += 1;
#endif

        // Направление взгляда считаем по "сырому" вводу, до нормализации.
        UpdateFacing(direction);

        direction = direction.normalized;
    }

    private void FixedUpdate()
    {
        body.linearVelocity = direction * speed;
    }

    private void OnDisable()
    {
        if (body != null)
            body.linearVelocity = Vector2.zero;
    }

    /// <summary>Вернуть взгляд вниз (для сброса тестовой сессии).</summary>
    public void ResetFacing()
    {
        SetFacing(Direction4.Down);
    }

    private void UpdateFacing(Vector2 raw)
    {
        bool hasX = raw.x != 0f;
        bool hasY = raw.y != 0f;

        // Стоим на месте: взгляд не меняется.
        if (!hasX && !hasY)
            return;

        Direction4 next;

        if (hasX && hasY)
        {
            // Диагональ. Если текущий взгляд среди нажатых направлений - оставляем его,
            // иначе выбираем вертикальное (правило из ТЗ).
            if (IsAmongPressed(Facing, raw))
                return;

            next = raw.y > 0f ? Direction4.Up : Direction4.Down;
        }
        else if (hasY)
        {
            next = raw.y > 0f ? Direction4.Up : Direction4.Down;
        }
        else
        {
            next = raw.x > 0f ? Direction4.Right : Direction4.Left;
        }

        SetFacing(next);
    }

    private static bool IsAmongPressed(Direction4 facing, Vector2 raw)
    {
        switch (facing)
        {
            case Direction4.Up: return raw.y > 0f;
            case Direction4.Down: return raw.y < 0f;
            case Direction4.Left: return raw.x < 0f;
            case Direction4.Right: return raw.x > 0f;
            default: return false;
        }
    }

    private void SetFacing(Direction4 next)
    {
        if (next == Facing)
            return;

        Facing = next;
        FacingChanged?.Invoke(Facing);
    }
}