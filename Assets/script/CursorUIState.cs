using System.Collections.Generic;
using UnityEngine;

/// <summary>Giữ con trỏ mở khi có ít nhất một menu đang hoạt động.</summary>
public static class CursorUIState
{
    private static readonly HashSet<int> owners = new HashSet<int>();

    public static bool IsUIOpen => owners.Count > 0;

    public static void RequestFreeCursor(Object owner)
    {
        if (owner == null) return;
        owners.Add(owner.GetInstanceID());
        Apply();
    }

    public static void ReleaseFreeCursor(Object owner)
    {
        if (owner == null) return;
        owners.Remove(owner.GetInstanceID());
        Apply();
    }

    /// <summary>Đóng toàn bộ giao diện và buộc con trỏ trở về chế độ chơi.</summary>
    public static void ForceLockCursor()
    {
        owners.Clear();
        Apply();
    }

    public static void Apply()
    {
        Cursor.visible = IsUIOpen;
        Cursor.lockState = IsUIOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
