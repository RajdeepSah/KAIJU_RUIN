namespace KaijuRuin
{
    public enum GameMode { Solo, Online }
    public enum MatchKind { Practice, PrivateRoom, QuickMatch }
    public enum NetRole { None, Host, Client }

    // The negotiated fight setup — a small shared blackboard the front-end fills
    // and the fight reads. Defaults reproduce the original single-player fight
    // (Kest vs Tengi) so entering the fight directly never regresses (D-017).
    public static class MatchConfig
    {
        public static GameMode Mode = GameMode.Solo;
        public static MatchKind Kind = MatchKind.Practice;
        public static NetRole Role = NetRole.None;
        public static string RoomCode = "";
        public static string LocalCharId = "kest";
        public static string OpponentCharId = "tengi";

        // True ONLY when a live remote transport drives the opponent fighter. Under
        // the shipping local/loopback path this stays false and the opponent is the
        // AI, so the APK plays offline with zero services (Vision §11 / ground rule 3).
        public static bool RemoteOpponent = false;

        public static CharacterDef Local => CharacterRoster.Get(LocalCharId);
        public static CharacterDef Opponent => CharacterRoster.Get(OpponentCharId);

        public static void SetSolo(string local, string opponent)
        {
            Mode = GameMode.Solo; Kind = MatchKind.Practice; Role = NetRole.None;
            RoomCode = ""; RemoteOpponent = false;
            LocalCharId = local; OpponentCharId = opponent;
        }

        public static void ApplySession(NetSessionInfo s)
        {
            if (s == null) return;
            Role = s.Role;
            RoomCode = s.RoomCode;
            LocalCharId = s.LocalCharId;
            OpponentCharId = s.OpponentCharId;
            RemoteOpponent = s.RemoteOpponent;
        }
    }
}
