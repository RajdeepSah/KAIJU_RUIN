using System;
using System.Collections;

namespace KaijuRuin
{
    // Where a matchmaking / connection attempt currently is, so the lobby can show
    // "Searching…", "Connecting…", "Opponent found".
    public enum NetStatus { Idle, Creating, Searching, Connecting, Connected, Failed }

    // One tick of a fighter's intent — the unit a real transport exchanges between
    // peers. Kept tiny and value-typed so it serializes cheaply. The local/loopback
    // path never sends these; they define the exact shape a backend fills (D-017).
    public struct FighterInputCmd
    {
        public float MoveAxis;
        public bool Tap, Heavy, Launcher, Sweep, BackDash;
        public bool BlockDown, BlockUp;
        public int Special;         // 0 none, else 1..3
    }

    // The result of a matchmaking request: who I am and who I face. A real backend
    // fills OpponentCharId from the peer's pick and sets RemoteOpponent = true.
    public class NetSessionInfo
    {
        public NetRole Role = NetRole.None;
        public string RoomCode = "";
        public string LocalCharId = "kest";
        public string OpponentCharId = "tengi";
        public bool RemoteOpponent = false;
    }

    // Pluggable matchmaking front. LocalMatchmaker simulates it fully offline; a real
    // backend (Unity Lobby, Photon, custom) implements the same three verbs. Each is
    // coroutine-shaped so the UI can animate status without threads.
    //   onStatus(status, humanMessage) — progress ticks for the lobby label.
    //   onDone(session or null)        — the negotiated session, or null on failure.
    public interface IMatchmaker
    {
        IEnumerator CreateRoom(string localCharId, Action<NetStatus, string> onStatus, Action<NetSessionInfo> onDone);
        IEnumerator JoinRoom(string code, string localCharId, Action<NetStatus, string> onStatus, Action<NetSessionInfo> onDone);
        IEnumerator QuickMatch(string localCharId, Action<NetStatus, string> onStatus, Action<NetSessionInfo> onDone);
    }

    // Pluggable in-fight transport. Carries FighterInputCmd between peers once a
    // session is live. LoopbackTransport is an offline no-op; a real backend swaps
    // in without touching gameplay — INetTransport + IMatchmaker are the only seams.
    public interface INetTransport
    {
        bool Connected { get; }
        void SendInput(FighterInputCmd cmd);
        bool TryGetRemoteInput(out FighterInputCmd cmd);   // false when nothing is queued
        void Tick(float dt);
        void Shutdown();
    }
}
