using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CursorControl
{
    public static void SetCursorState(CursorLockMode mode, bool isVisible)
    {
        Cursor.lockState = mode;
        Cursor.visible = isVisible;
    }
}
