using UnityEngine;
using UnityEngine.EventSystems;

namespace AndamRunner
{
    public sealed class SwipeInput : MonoBehaviour
    {
        private GameManager game;
        private Vector2 touchStart;
        private int finger = -1;
        private bool used;

        public void Initialize(GameManager manager) { game = manager; }
        public void ResetGesture() { finger = -1; used = true; }

        private void Update()
        {
            if (game == null) return;
            if (Input.GetKeyDown(KeyCode.Escape)) game.UI.HandleBack();
            if (Input.GetKeyDown(KeyCode.P)) game.TogglePause();
            if (game.State != RunState.Playing) { ResetGesture(); return; }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) game.Move(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) game.Move(1);
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) game.Jump();
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.LeftControl)) game.Duck();

            // Poll touches without allocating Input.touches every frame.
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && finger == -1)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) continue;
                    finger = touch.fingerId;
                    touchStart = touch.position;
                    used = false;
                }
                if (touch.fingerId != finger) continue;
                if (!used && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended))
                {
                    Vector2 movement = touch.position - touchStart;
                    float threshold = Mathf.Max(35f, Mathf.Min(Screen.width, Screen.height) * 0.045f);
                    if (movement.magnitude >= threshold)
                    {
                        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y)) game.Move(movement.x > 0 ? 1 : -1);
                        else if (movement.y > 0) game.Jump();
                        else game.Duck();
                        used = true;
                    }
                }
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) finger = -1;
            }
        }
    }
}
