using System.Collections;
using UnityEngine;

namespace KaijuRuin
{
    // Session state machine: Menu -> StoryIntro -> Fight -> Ending -> Menu.
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        // Owned here so the whole session (input, AI, future Android back-button)
        // can read one authoritative pause state (DESIGN_BRIEF: PAUSE ->
        // GameManager.TogglePause()).
        public static bool Paused { get; private set; }

        GameObject fightRoot;

        static readonly string[] FightObjectNames =
            { "FightCamera", "KeyLight", "Stage", "HUD", "FightHud", "Ending", "kest_model.glb", "tengi_model.glb" };

        void Awake() { I = this; }

        void Start() { ShowMenu(); }

        void ShowMenu()
        {
            var menu = gameObject.AddComponent<MainMenu>();
            menu.Show(
                onSolo: () => { Destroy(menu); ShowCharacterSelect(GameMode.Solo); },
                onMultiplayer: () => { Destroy(menu); ShowMultiplayer(); });
        }

        // Front-end flow (D-017):
        //   Solo:   Menu -> CharacterSelect -> Fight
        //   Online: Menu -> MultiplayerMenu -> CharacterSelect -> Lobby -> Fight
        void ShowMultiplayer()
        {
            var mp = gameObject.AddComponent<MultiplayerMenu>();
            mp.Show(
                proceed: () => { Destroy(mp); ShowCharacterSelect(GameMode.Online); },
                back: () => { Destroy(mp); ShowMenu(); });
        }

        void ShowCharacterSelect(GameMode mode)
        {
            var cs = gameObject.AddComponent<CharacterSelectMenu>();
            cs.Show(mode,
                confirm: (localId, oppId) =>
                {
                    Destroy(cs);
                    if (mode == GameMode.Solo)
                    {
                        MatchConfig.SetSolo(localId, oppId);
                        StartFight(skipIntro: false);
                    }
                    else
                    {
                        MatchConfig.LocalCharId = localId;
                        MatchConfig.OpponentCharId = oppId;   // preference; the matchmaker may override
                        ShowLobby();
                    }
                },
                back: () =>
                {
                    Destroy(cs);
                    if (mode == GameMode.Solo) ShowMenu(); else ShowMultiplayer();
                });
        }

        void ShowLobby()
        {
            var lobby = gameObject.AddComponent<LobbyMenu>();
            lobby.Show(
                start: () =>
                {
                    Destroy(lobby);
                    NetService.I?.OpenTransport();       // no-op under loopback; live path for a real backend
                    StartFight(skipIntro: true);          // online skips the single-player story intro
                },
                back: () => { Destroy(lobby); ShowMultiplayer(); });
        }

        public void StartFight(bool skipIntro)
        {
            StartCoroutine(FightFlow(skipIntro));
        }

        IEnumerator FightFlow(bool skipIntro)
        {
            CleanupFight();   // never stack a second fight on the old one (rematch)

            if (!skipIntro)
            {
                var intro = gameObject.AddComponent<StoryIntro>();
                yield return intro.Run();
                Destroy(intro);
            }

            fightRoot = new GameObject("Fight");
            var rm = fightRoot.AddComponent<RoundManager>();
            yield return rm.RunMatch();
        }

        public void TogglePause()
        {
            Paused = !Paused;
            Time.timeScale = Paused ? 0f : 1f;
            TouchUI.I?.ShowPauseOverlay(Paused);
        }

        void CleanupFight()
        {
            Paused = false;
            Time.timeScale = 1f;
            RoundManager.RoundFrozen = true;
            if (fightRoot != null) { Destroy(fightRoot); fightRoot = null; }
            // Cleanup runs before every fight starts, so at most one set exists;
            // a single Find per name is sufficient (Destroy is deferred, so a
            // re-Find loop in the same frame would spin).
            foreach (var name in FightObjectNames)
            {
                var go = GameObject.Find(name);
                if (go != null) Destroy(go);
            }
        }

        public void BackToTitle()
        {
            StopAllCoroutines();
            CleanupFight();
            NetService.I?.CloseTransport();   // release any online session before returning to the menu
            ShowMenu();
        }
    }
}
