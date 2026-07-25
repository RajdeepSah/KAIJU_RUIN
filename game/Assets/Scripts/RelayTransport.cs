using System;
using System.Collections;

namespace KaijuRuin
{
    // ============================================================================
    // LIVE-BACKEND SEAM — not wired, by design (gated per ARCHITECTURE.md; real-time
    // PvP is a separate, service-dependent project). These are the drop-in points
    // for actual worldwide play. They implement the same IMatchmaker / INetTransport
    // the whole game already talks to, so activating online play is: (1) implement
    // the TODOs below, (2) flip NetService.Backend to Backend.Relay. Zero UI or fight
    // changes (D-017).
    //
    // Recommended stack (Unity-native, matches Vision §11 "local APK, no bespoke
    // server"): Unity Gaming Services —
    //   • Authentication  (anonymous sign-in)
    //   • Lobby           (create/join by code  → CreateRoom / JoinRoom)
    //   • Matchmaker      (pool tickets          → QuickMatch)
    //   • Relay           (NAT-punch relay alloc → the INetTransport data path)
    //   • Netcode / Unity Transport (UTP) to carry FighterInputCmd
    // Alternatives that fit the same seam: Photon Fusion/Quantum, Epic EOS, or a
    // small authoritative WebSocket server.
    //
    // Netcode model for THIS game: the fight sim is deterministic on the X axis
    // (Fighter.cs), so the cheapest correct model is input-lockstep — exchange
    // FighterInputCmd per tick, both peers simulate, with a 2–3 frame input delay.
    // If full determinism proves fragile (Time.time / UnityEngine.Random in AI),
    // fall back to host-authoritative state sync. Either way the exchange unit is
    // FighterInputCmd and the seam is unchanged.
    // ============================================================================

    public class RelayMatchmaker : IMatchmaker
    {
        const string NotWired = "Online backend not configured (add Unity Relay/Lobby — see RelayTransport.cs)";

        public IEnumerator CreateRoom(string localCharId, Action<NetStatus, string> onStatus, Action<NetSessionInfo> onDone)
        {
            // TODO: LobbyService.CreateLobbyAsync → RelayService allocation → return the join code.
            onStatus?.Invoke(NetStatus.Failed, NotWired);
            onDone?.Invoke(null);
            yield break;
        }

        public IEnumerator JoinRoom(string code, string localCharId, Action<NetStatus, string> onStatus, Action<NetSessionInfo> onDone)
        {
            // TODO: LobbyService.JoinLobbyByCodeAsync(code) → RelayService.JoinAllocationAsync.
            onStatus?.Invoke(NetStatus.Failed, NotWired);
            onDone?.Invoke(null);
            yield break;
        }

        public IEnumerator QuickMatch(string localCharId, Action<NetStatus, string> onStatus, Action<NetSessionInfo> onDone)
        {
            // TODO: MatchmakerService ticket → poll → Relay join on assignment.
            onStatus?.Invoke(NetStatus.Failed, NotWired);
            onDone?.Invoke(null);
            yield break;
        }
    }

    // The live data path. When wired: push local FighterInputCmds through UTP and
    // surface the peer's via TryGetRemoteInput; RemoteController applies them to the
    // opponent Fighter. Until wired it reports "not connected" so a mis-flip fails
    // loudly rather than silently dropping inputs.
    public class RelayTransport : INetTransport
    {
        public bool Connected => false;
        public void SendInput(FighterInputCmd cmd) { /* TODO: NetworkTransport.Send(serialize(cmd)) */ }
        public bool TryGetRemoteInput(out FighterInputCmd cmd) { cmd = default; return false; }
        public void Tick(float dt) { /* TODO: pump UTP events, enqueue received cmds */ }
        public void Shutdown() { /* TODO: dispose Relay allocation + transport */ }
    }
}
