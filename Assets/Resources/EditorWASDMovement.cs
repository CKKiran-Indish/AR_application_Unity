using UnityEngine;
using UnityEngine.InputSystem;

public class EditorWASDMovement : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
#if UNITY_EDITOR
        Vector2 move = Vector2.zero;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed) move.y += 1;
        if (keyboard.sKey.isPressed) move.y -= 1;
        if (keyboard.dKey.isPressed) move.x += 1;
        if (keyboard.aKey.isPressed) move.x -= 1;

        Vector3 direction = new Vector3(move.x, 0, move.y);

        transform.Translate(direction * speed * Time.deltaTime);
#endif
    }
}