using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public Action OnDrop;
    public Action<int> OnMove;

    public Action OnDestroy;
    public Action<Vector2Int> OnMoveCursor;
    public Action isDestroyMode;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) OnMove?.Invoke(-1);
        if (Input.GetKeyDown(KeyCode.D)) OnMove?.Invoke(1);
        if (Input.GetKeyDown(KeyCode.Space)) OnDrop?.Invoke();

        if (Input.GetKeyDown(KeyCode.Tab))
            isDestroyMode?.Invoke();

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.A)) dir.x = -1;
        if (Input.GetKeyDown(KeyCode.D)) dir.x = 1;
        if (Input.GetKeyDown(KeyCode.W)) dir.y = 1;
        if (Input.GetKeyDown(KeyCode.S)) dir.y = -1;

        if (dir != Vector2Int.zero)
            OnMoveCursor?.Invoke(dir);

        //if (Input.GetKeyDown(KeyCode.Space))
        //    OnDestroy?.Invoke();
    }
}
