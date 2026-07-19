using UnityEngine;

namespace KaijuRuin
{
    // Gesture recognition per DESIGN_BRIEF.md controls: left half drag = walk,
    // right half tap/swipe/hold = attacks/block. Mouse fallback in the editor.
    //
    // Session 4 rework:
    //  - Swipes commit the instant the threshold is crossed (not on touch-up),
    //    cutting 80-300 ms of finger-contact latency off heavy/launcher/sweep.
    //  - The 120 ms hold-block test is driven from Update() off a cached finger
    //    position, so it fires on time even if the OS coalesces Stationary events.
    //  - On a round freeze or pause, BOTH gesture channels reset so a finger held
    //    across the freeze cannot misfire the next round's first input.
    //  - Every touch-down gets a sub-80 ms ink acknowledgement (ART_DIRECTION 3).
    //  - A swipe AWAY from the opponent is a no-op instead of a phantom jab.
    public class TouchInput : MonoBehaviour
    {
        public PlayerController Player;

        const float HoldTime = 0.12f;
        const float SwipeMinPixelsFactor = 0.04f;   // fraction of screen width

        // Left-half movement state
        bool moveActive;
        int moveFingerId = -1;
        float moveOriginX;

        // Right-half gesture state
        bool actionActive;
        int actionFingerId = -1;
        Vector2 actionOrigin;
        Vector2 actionLastPos;
        float actionStartTime;
        bool holdFired;
        bool gestureConsumed;   // a swipe already committed; touch-up must not re-fire

        void Update()
        {
            if (RoundManager.RoundFrozen || GameManager.Paused) { ResetGestures(); return; }

            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++) HandleTouch(Input.GetTouch(i));
            }
            else
            {
                HandleMouse();
            }

            if (moveActive)
            {
                float x = CurrentX(moveFingerId);
                float axis = Mathf.Clamp((x - moveOriginX) / (Screen.width * 0.06f), -1f, 1f);
                Player.Move(axis);
            }

            // Phase-independent: resolve swipe-at-threshold / hold every frame.
            if (actionActive && !gestureConsumed && !holdFired) ProcessActiveAction();
        }

        float CurrentX(int fingerId)
        {
            for (int i = 0; i < Input.touchCount; i++)
                if (Input.GetTouch(i).fingerId == fingerId) return Input.GetTouch(i).position.x;
            return Input.mousePosition.x;
        }

        void HandleTouch(Touch t)
        {
            bool left = t.position.x < Screen.width * 0.5f;
            switch (t.phase)
            {
                case TouchPhase.Began:
                    if (left && !moveActive)
                    {
                        moveActive = true; moveFingerId = t.fingerId; moveOriginX = t.position.x;
                        TouchUI.I?.TouchFeedback(t.position, true);
                    }
                    else if (!left && !actionActive) BeginAction(t.fingerId, t.position);
                    break;
                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    if (actionActive && t.fingerId == actionFingerId) actionLastPos = t.position;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (moveActive && t.fingerId == moveFingerId) EndMove();
                    if (actionActive && t.fingerId == actionFingerId) { actionLastPos = t.position; EndAction(); }
                    break;
            }
        }

        void HandleMouse()
        {
            var pos = (Vector2)Input.mousePosition;
            bool left = pos.x < Screen.width * 0.5f;
            if (Input.GetMouseButtonDown(0))
            {
                if (left) { moveActive = true; moveFingerId = -1; moveOriginX = pos.x; TouchUI.I?.TouchFeedback(pos, true); }
                else BeginAction(-1, pos);
            }
            if (Input.GetMouseButton(0) && actionActive) actionLastPos = pos;
            if (Input.GetMouseButtonUp(0))
            {
                if (moveActive) EndMove();
                if (actionActive) { actionLastPos = pos; EndAction(); }
            }
        }

        void BeginAction(int fingerId, Vector2 pos)
        {
            if (TouchUI.I != null && TouchUI.I.PointerOverUi(pos)) return;
            actionActive = true;
            actionFingerId = fingerId;
            actionOrigin = pos;
            actionLastPos = pos;
            actionStartTime = Time.time;
            holdFired = false;
            gestureConsumed = false;
            TouchUI.I?.TouchFeedback(pos, false);
        }

        // Runs every frame while a right-half gesture is live and unresolved.
        void ProcessActiveAction()
        {
            var delta = actionLastPos - actionOrigin;
            float swipeMin = Screen.width * SwipeMinPixelsFactor;
            if (delta.magnitude >= swipeMin)
            {
                FireSwipe(delta);
                gestureConsumed = true;
                return;
            }
            if (Time.time - actionStartTime >= HoldTime)
            {
                holdFired = true;
                Player.SetBlock(true);
            }
        }

        void FireSwipe(Vector2 delta)
        {
            PerfMonitor.MarkInput();
            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            {
                if (delta.y > 0) Player.Launcher(); else Player.Sweep();
            }
            else
            {
                bool towardOpponent = Player.Self.FacingRight ? delta.x > 0 : delta.x < 0;
                if (towardOpponent) Player.HeavyAttack();
                // Swipe away is intentionally a no-op (retreat is a left-half
                // drag); it no longer commits a phantom jab + recovery lock.
            }
        }

        void EndMove()
        {
            if (!moveActive) return;
            moveActive = false;
            moveFingerId = -1;
            Player.Move(0f);
        }

        void EndAction()
        {
            bool wasHold = holdFired;
            bool wasConsumed = gestureConsumed;
            actionActive = false;
            actionFingerId = -1;
            holdFired = false;
            gestureConsumed = false;

            if (wasConsumed) return;              // swipe already fired
            if (wasHold) { Player.SetBlock(false); return; }

            // Sub-threshold, sub-hold release: a tap.
            var delta = actionLastPos - actionOrigin;
            if (delta.magnitude < Screen.width * SwipeMinPixelsFactor) { PerfMonitor.MarkInput(); Player.TapAttack(); return; }
            // Threshold crossed only on the up-frame (fast flick): resolve now.
            FireSwipe(delta);
        }

        // Cancel both channels when the round freezes or the game pauses.
        void ResetGestures()
        {
            EndMove();
            if (actionActive)
            {
                if (holdFired) Player.SetBlock(false);
                actionActive = false;
                actionFingerId = -1;
                holdFired = false;
                gestureConsumed = false;
            }
        }
    }
}
