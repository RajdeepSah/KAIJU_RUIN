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
    //
    // Session 10 rework (D-024) — the mix-up set needs more input space, taken from
    // two axes that were already under the thumbs:
    //  - The left half gains a VERTICAL read. Its horizontal drag still walks; drag
    //    away past half deflection also raises a standing guard (back = retreat and
    //    block, as in any 2D fighter), drag down crouches, and down-and-away is the
    //    crouch guard. The stance is pushed to PlayerController every frame the
    //    finger is down and cleared when it lifts.
    //  - The right half's four swipe directions become eight. A swipe counts as a
    //    diagonal only when its shorter axis is at least DiagonalRatio of its longer
    //    one, so a sloppy cardinal flick still reads as the cardinal it meant.
    public class TouchInput : MonoBehaviour
    {
        public PlayerController Player;

        const float HoldTime = 0.12f;
        const float SwipeMinPixelsFactor = 0.04f;   // fraction of screen width

        // How square a swipe must be to count as a diagonal (1 = exactly 45 degrees).
        // Lower = diagonals are easier but cardinals are easier to fire by accident.
        public static float DiagonalRatio = 0.55f;

        // Left-thumb stance thresholds, as fractions of the drag origin.
        const float GuardAxis = 0.5f;               // away-deflection that raises a guard
        const float CrouchPixelsFactor = 0.045f;    // downward travel (fraction of screen HEIGHT) to crouch

        // Left-half movement state
        bool moveActive;
        int moveFingerId = -1;
        Vector2 moveOrigin;

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
                var p = CurrentPos(moveFingerId);
                float axis = Mathf.Clamp((p.x - moveOrigin.x) / (Screen.width * 0.06f), -1f, 1f);

                // Away from the opponent, past half deflection, is a guard as well as
                // a retreat; downward travel sinks into a crouch. Both are STANCE, so
                // they are pushed every frame rather than fired as events.
                bool away = (Player.Self.FacingRight ? -axis : axis) >= GuardAxis;
                bool down = (moveOrigin.y - p.y) >= Screen.height * CrouchPixelsFactor;
                Player.SetStance(down, away);
                Player.Move(axis);
            }

            // Phase-independent: resolve swipe-at-threshold / hold every frame.
            if (actionActive && !gestureConsumed && !holdFired) ProcessActiveAction();
        }

        Vector2 CurrentPos(int fingerId)
        {
            for (int i = 0; i < Input.touchCount; i++)
                if (Input.GetTouch(i).fingerId == fingerId) return Input.GetTouch(i).position;
            return Input.mousePosition;
        }

        void HandleTouch(Touch t)
        {
            bool left = t.position.x < Screen.width * 0.5f;
            switch (t.phase)
            {
                case TouchPhase.Began:
                    if (left && !moveActive)
                    {
                        moveActive = true; moveFingerId = t.fingerId; moveOrigin = t.position;
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
                if (left) { moveActive = true; moveFingerId = -1; moveOrigin = pos; TouchUI.I?.TouchFeedback(pos, true); }
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

        // Eight directions, resolved in the fighter's own frame (toward / away, up /
        // down) so a mirror match plays identically. Cardinals keep their session-4
        // and D-016 bindings; the four diagonals are the D-024 additions.
        void FireSwipe(Vector2 delta)
        {
            PerfMonitor.MarkInput();
            float toward = (Player.Self.FacingRight ? delta.x : -delta.x);
            float ax = Mathf.Abs(toward), ay = Mathf.Abs(delta.y);
            bool diagonal = Mathf.Min(ax, ay) >= Mathf.Max(ax, ay) * DiagonalRatio;

            if (diagonal)
            {
                if (toward > 0f)
                {
                    // Toward + up: the overhead comes down over a crouching guard.
                    // Toward + down: drive the haunch in at point-blank.
                    if (delta.y > 0f) Player.ClawSlam(); else Player.HaunchBash();
                }
                else
                {
                    // Away + up: haul them off their feet — the command grab.
                    // Away + down: a raking low that beats a standing guard.
                    if (delta.y > 0f) Player.CommandGrab(); else Player.LegSweep();
                }
                return;
            }

            if (ay > ax)
            {
                if (delta.y > 0f) Player.Launcher(); else Player.Sweep();
            }
            else if (toward > 0f) Player.HeavyAttack();
            else Player.BackDash();              // swipe away = evasive back-dash (D-016)
        }

        void EndMove()
        {
            if (!moveActive) return;
            moveActive = false;
            moveFingerId = -1;
            Player.SetStance(false, false);      // lifting the thumb drops the stance
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
