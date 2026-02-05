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
    public Action<bool> OnDestroyModeChanged;

    private bool destroyMode = false;
    private bool isControlEnabled = true;

    private void Update()
    {
        if (!isControlEnabled) return;

        if (destroyMode)
        {
            HandleCursorMovement();
            HandleDestroy();
        }
        else
        {
            HandleDropAndMove();
        }
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
    public void OnClickDestroyButton()
    {
        ToggleDestroyMode();
    }

    private void ToggleDestroyMode()
    {
        destroyMode = !destroyMode;
        OnDestroyModeChanged?.Invoke(destroyMode);
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

    private void HandleDestroy()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            OnDestroy?.Invoke();
    }

    /// <summary>
    /// プレイヤー操作を止める
    /// </summary>
    public void StopControl()
    {
        isControlEnabled = false;
    }
}
