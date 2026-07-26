using System.Collections;
using UnityEngine;

namespace KaijuRuin
{
    // Session state machine (D-022):
    //   first run: Title -> StoryIntro -> HowToPlay -> FightSelect -> ...
    //   later:     Title -> FightSelect -> ...
    //   Solo:      FightSelect -> CharacterSelect -> Fight -> Ending -> Title
    //   Online:    FightSelect -> MultiplayerMenu -> CharacterSelect -> Lobby -> Fight
    // The story intro sits in the FRONT END, before character select, so it plays
    // once on both paths instead of once per solo fight.
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

        void Start() { ShowTitle(); }

        // Buttonless title screen: tap anywhere to enter the front end.
        void ShowTitle()
        {
            var menu = gameObject.AddComponent<MainMenu>();
            menu.Show(onContinue: () => { Destroy(menu); AfterTitle(); });
        }

        void AfterTitle()
        {
            if (StoryIntro.Seen) ShowFightSelect();
            else StartCoroutine(FirstRunOnboarding());
        }

        // Once per install (StoryIntro.Seen is a PlayerPref), and skippable at any
        // beat. Both entry points stay on FightSelectMenu afterwards.
        IEnumerator FirstRunOnboarding()
        {
            yield return PlayStoryIntro();
            ShowHowToPlay(then: ShowFightSelect);
        }

        IEnumerator PlayStoryIntro()
        {
            var intro = gameObject.AddComponent<StoryIntro>();
            yield return intro.Run();
            Destroy(intro);
        }

        IEnumerator ReplayStoryIntro()
        {
            yield return PlayStoryIntro();
            ShowFightSelect();
        }

        // Layered over whatever is underneath (its own dim blocks input), so the
        // fight-select screen does not need tearing down to show the primer.
        void ShowHowToPlay(System.Action then)
        {
            if (GetComponent<HowToPlay>() != null) return;   // one primer at a time
            var hp = gameObject.AddComponent<HowToPlay>();
            hp.Show(() => { Destroy(hp); then?.Invoke(); });
        }

        void ShowFightSelect()
        {
            var fs = gameObject.AddComponent<FightSelectMenu>();
            fs.Show(
                onSolo: () => { Destroy(fs); ShowCharacterSelect(GameMode.Solo); },
                onMultiplayer: () => { Destroy(fs); ShowMultiplayer(); },
                onHowToPlay: () => ShowHowToPlay(then: null),
                onReplayStory: () => { Destroy(fs); StartCoroutine(ReplayStoryIntro()); },
                back: () => { Destroy(fs); ShowTitle(); });
        }

        void ShowMultiplayer()
        {
            var mp = gameObject.AddComponent<MultiplayerMenu>();
            mp.Show(
                proceed: () => { Destroy(mp); ShowCharacterSelect(GameMode.Online); },
                back: () => { Destroy(mp); ShowFightSelect(); });
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
                        StartFight();
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
                    if (mode == GameMode.Solo) ShowFightSelect(); else ShowMultiplayer();
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
                    StartFight();
                },
                back: () => { Destroy(lobby); ShowMultiplayer(); });
        }

        // No intro gate any more: the story intro is a front-end step (D-022), so a
        // fight (or a rematch) starts straight into the match.
        public void StartFight()
        {
            StartCoroutine(FightFlow());
        }

        IEnumerator FightFlow()
        {
            CleanupFight();   // never stack a second fight on the old one (rematch)

            fightRoot = new GameObject("Fight");
            var rm = fightRoot.AddComponent<RoundManager>();
            yield return rm.RunMatch();
        }

        public void TogglePause()
        {
            Paused = !Paused;
            // timeScale is TimeDirector's to write since D-026 — slow motion is a
            // second author of the same global, and pausing at 0.35x (or resuming to
            // 1.0 out of a cinematic that was mid-ramp) is what happens when two
            // systems both think they own it. Pause always wins, and it cancels any
            // running shot rather than layering on top of it.
            TimeDirector.SetPaused(Paused);
            TouchUI.I?.ShowPauseOverlay(Paused);
        }

        void CleanupFight()
        {
            Paused = false;
            TimeDirector.HardReset();     // never carry a cinematic (or a pause) into the next fight
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
            ShowTitle();
        }
    }
}
