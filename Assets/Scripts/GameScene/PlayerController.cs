using UnityEngine;
using System;

/// <summary>
/// プレイヤーの入力を受け取り、イベントを発行するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    public Action OnDrop;
    public Action<int> OnMove;

    public Action OnDestroy;
    public Action<Vector2Int> OnMoveCursor;
    public Action isDestroyMode;

    private void Update()
    {
        HandleDropAndMove();
        HandleDestroyMode();
        HandleCursorMovement();
    }

    /// <summary>
    /// 左右移動とドロップの入力処理
    /// </summary>
    private void HandleDropAndMove()
    {
        if (Input.GetKeyDown(KeyCode.A)) OnMove?.Invoke(-1);
        if (Input.GetKeyDown(KeyCode.D)) OnMove?.Invoke(1);
        if (Input.GetKeyDown(KeyCode.Space)) OnDrop?.Invoke();
    }

    /// <summary>
    /// 破壊モード切り替えの入力
    /// </summary>
    private void HandleDestroyMode()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            isDestroyMode?.Invoke();
    }

    /// <summary>
    /// カーソル移動の入力処理
    /// </summary>
    private void HandleCursorMovement()
    {
        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.A)) dir.x = -1;
        if (Input.GetKeyDown(KeyCode.D)) dir.x = 1;
        if (Input.GetKeyDown(KeyCode.W)) dir.y = 1;
        if (Input.GetKeyDown(KeyCode.S)) dir.y = -1;

        if (dir != Vector2Int.zero)
            OnMoveCursor?.Invoke(dir);
    }
}
