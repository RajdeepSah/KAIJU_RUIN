using UnityEngine;

namespace KaijuRuin
{
    // Persistent networking facade (created in Bootstrap, DontDestroyOnLoad). Owns
    // the active matchmaker + the in-fight transport, and is the ONE place the
    // backend is selected. The lobby drives matchmaking through Matchmaker;
    // RoundManager reads Transport when a live remote match is running (D-017).
    public class NetService : MonoBehaviour
    {
        public static NetService I { get; private set; }

        public enum Backend { Local, Relay }
        // Flip to Backend.Relay after wiring RelayTransport.cs to go live worldwide.
        // Local = offline stand-ins (room codes + search sim vs AI); ships by default.
        public Backend Active = Backend.Local;

        IMatchmaker matchmaker;
        public IMatchmaker Matchmaker => matchmaker ??= NewMatchmaker();

        public INetTransport Transport { get; private set; }
        public NetSessionInfo CurrentSession { get; private set; }

        void Awake() { I = this; }

        IMatchmaker NewMatchmaker() =>
            Active == Backend.Relay ? (IMatchmaker)new RelayMatchmaker() : new LocalMatchmaker();

        INetTransport NewTransport() =>
            Active == Backend.Relay ? (INetTransport)new RelayTransport() : new LoopbackTransport();

        // The lobby calls this once matchmaking resolves; it also mirrors the result
        // into MatchConfig so the fight can read a single source of truth.
        public void SetSession(NetSessionInfo s)
        {
            CurrentSession = s;
            MatchConfig.ApplySession(s);
        }

        // Open/close the in-fight transport around an online match.
        public INetTransport OpenTransport()
        {
            Transport?.Shutdown();
            Transport = NewTransport();
            return Transport;
        }

        public void CloseTransport()
        {
            Transport?.Shutdown();
            Transport = null;
            CurrentSession = null;
        }

        void Update()
        {
            // Pump the transport during a live match (no-op under loopback).
            if (Transport != null && !GameManager.Paused) Transport.Tick(Time.deltaTime);
        }
    }
}
