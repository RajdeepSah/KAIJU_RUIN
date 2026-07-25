using UnityEngine;

namespace KaijuRuin
{
    // Drives the OPPONENT Fighter from FighterInputCmds pulled off the transport —
    // the counterpart to NetInputRelay on the local side. Reuses the exact same
    // verbs the local human uses (PlayerController), so remote play and local play
    // share one code path.
    //
    // Added to the opponent ONLY when MatchConfig.RemoteOpponent is true (a real
    // backend is connected). Under the shipping loopback path the opponent is the
    // AI and this component isn't attached — so it's the built, documented seam for
    // live PvP, not live code (D-017). Its PlayerController runs with Local=false so
    // it never writes to the local player's HUD.
    public class RemoteController : MonoBehaviour
    {
        public PlayerController Control;
        INetTransport transport;

        void Awake() { if (Control == null) Control = GetComponent<PlayerController>(); }

        public void Bind(INetTransport t) { transport = t; }

        void Update()
        {
            if (transport == null || Control == null) return;
            if (CombatFx.Frozen || GameManager.Paused || RoundManager.RoundFrozen) return;
            while (transport.TryGetRemoteInput(out var cmd)) Apply(cmd);
        }

        void Apply(FighterInputCmd c)
        {
            Control.Move(c.MoveAxis);
            if (c.BlockDown) Control.SetBlock(true);
            if (c.BlockUp) Control.SetBlock(false);
            if (c.Tap) Control.TapAttack();
            if (c.Heavy) Control.HeavyAttack();
            if (c.Launcher) Control.Launcher();
            if (c.Sweep) Control.Sweep();
            if (c.BackDash) Control.BackDash();
            if (c.Special > 0) Control.CastSpecial(c.Special);
        }
    }
}
