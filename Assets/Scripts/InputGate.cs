/// <summary>
/// Блокировка ввода (движение, выбор предметов, действия в мире), пока открыто окно.
/// Счётчик, а не флаг: если блокировщиков несколько, ввод вернётся только когда сняты все.
/// </summary>
public static class InputGate
{
    private static int blockCount;

    public static bool IsBlocked => blockCount > 0;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        blockCount = 0;
    }

    public static void Block()
    {
        blockCount++;
    }

    public static void Unblock()
    {
        if (blockCount > 0)
            blockCount--;
    }
}
