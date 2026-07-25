using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace KaijuRuin
{
    // Zero-dependency offline stand-ins that let every multiplayer flow run and the
    // APK build with NO external service (Vision §11 / ground rule 3). The room
    // codes, the "Searching…" wait, and the session hand-off are all real; only the
    // remote peer is simulated — the opponent resolves to the local AI
    // (RemoteOpponent = false). Swap these for a backend-backed IMatchmaker /
    // INetTransport (see RelayTransport.cs) to light up live worldwide play;
    // nothing in the UI or the fight changes (D-017).

    public class LoopbackTransport : INetTransport
    {
        public bool Connected { get; private set; } = true;
        public void SendInput(FighterInputCmd cmd) { }                    // no peer to send to
        public bool TryGetRemoteInput(out FighterInputCmd cmd) { cmd = default; return false; }
        public void Tick(float dt) { }
        public void Shutdown() { Connected = false; }
    }

    public class LocalMatchmaker : IMatchmaker
    {
        // Unambiguous 6-char room code (no O/0/I/1). Random is fine here — this is
        // menu/UI code, never the deterministic fight sim.
        public static string NewRoomCode()
        {
            const string abc = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var sb = new StringBuilder(6);
            for (int i = 0; i < 6; i++) sb.Append(abc[UnityEngine.Random.Range(0, abc.Length)]);
            return sb.ToString();
        }

        public IEnumerator CreateRoom(string localCharId, Action<NetStatus, string> onStatus, Action<NetSessionInfo> onDone)
        {
            string code = NewRoomCode();
            onStatus?.Invoke(NetStatus.Creating, "Creating room…");
            yield return new WaitForSeconds(0.5f);
            onStatus?.Invoke(NetStatus.Searching, "Room " + code + " — waiting for a friend to join…");
            // Simulate a friend joining after a short beat (offline stand-in).
            yield return new WaitForSeconds(1.6f);
            onStatus?.Invoke(NetStatus.Connected, "Opponent joined");
            onDone?.Invoke(new NetSessionInfo {
                Role = NetRole.Host, RoomCode = code,
                LocalCharId = localCharId, OpponentCharId = CharacterRoster.Other(localCharId),
                RemoteOpponent = false,
            });
        }

        public IEnumerator JoinRoom(string code, string localCharId, Action<NetStatus, string> onStatus, Action<NetSessionInfo> onDone)
        {
            code = (code ?? "").Trim().ToUpperInvariant();
            if (code.Length < 4)
            {
                onStatus?.Invoke(NetStatus.Failed, "Enter a valid room code");
                onDone?.Invoke(null);
                yield break;
            }
            onStatus?.Invoke(NetStatus.Connecting, "Connecting to room " + code + "…");
            yield return new WaitForSeconds(1.2f);
            onStatus?.Invoke(NetStatus.Connected, "Connected");
            onDone?.Invoke(new NetSessionInfo {
                Role = NetRole.Client, RoomCode = code,
                LocalCharId = localCharId, OpponentCharId = CharacterRoster.Other(localCharId),
                RemoteOpponent = false,
            });
        }

        public IEnumerator QuickMatch(string localCharId, Action<NetStatus, string> onStatus, Action<NetSessionInfo> onDone)
        {
            onStatus?.Invoke(NetStatus.Searching, "Searching for an opponent worldwide…");
            yield return new WaitForSeconds(1.8f);
            onStatus?.Invoke(NetStatus.Connecting, "Opponent found — connecting…");
            yield return new WaitForSeconds(0.7f);
            onStatus?.Invoke(NetStatus.Connected, "Match ready");
            // The opponent's champion would arrive from the peer; the sim picks one.
            onDone?.Invoke(new NetSessionInfo {
                Role = NetRole.Client, RoomCode = "",
                LocalCharId = localCharId, OpponentCharId = CharacterRoster.RandomId(),
                RemoteOpponent = false,
            });
        }
    }
}
