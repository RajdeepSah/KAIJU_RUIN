using UnityEngine;

namespace KaijuRuin
{
    // The LOCAL send seam: mirrors the local player's issued commands onto the
    // transport so the peer's RemoteController can replay them. Attached to the local
    // player only in a live remote match (MatchConfig.RemoteOpponent). Under loopback
    // there is no peer, so SendInput is a no-op and this stays inert — the built,
    // documented counterpart to RemoteController (D-017).
    //
    // TouchInput/PlayerController call Push(...) as inputs are issued; this batches
    // per-frame intent and hands it to the transport in LateUpdate.
    public class NetInputRelay : MonoBehaviour
    {
        INetTransport transport;
        FighterInputCmd pending;
        bool dirty;

        public void Bind(INetTransport t) { transport = t; }

        public void SetMove(float axis) { pending.MoveAxis = axis; dirty = true; }
        public void Tap() { pending.Tap = true; dirty = true; }
        public void Heavy() { pending.Heavy = true; dirty = true; }
        public void Launcher() { pending.Launcher = true; dirty = true; }
        public void Sweep() { pending.Sweep = true; dirty = true; }
        public void BackDash() { pending.BackDash = true; dirty = true; }
        public void Block(bool down) { if (down) pending.BlockDown = true; else pending.BlockUp = true; dirty = true; }
        public void Special(int slot) { pending.Special = slot; dirty = true; }

        void LateUpdate()
        {
            if (transport == null || !dirty) return;
            transport.SendInput(pending);
            pending = default;
            dirty = false;
        }
    }
}
