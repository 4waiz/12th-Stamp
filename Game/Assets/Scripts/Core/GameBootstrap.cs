using System;
using System.Collections;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// Entry point. The scene is empty; everything is created here. Parses OUR arguments (--mode=vr|desktop),
    /// starts XR only for VR mode, shows Retry / Desktop Mode / Quit if VR cannot start, then the welcome menu.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        GameObject m_Presentation;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<GameBootstrap>() != null) return;
            var go = new GameObject("TwelfthStamp");
            DontDestroyOnLoad(go);
            go.AddComponent<GameBootstrap>();
        }

        IEnumerator Start()
        {
            AppArgs.Parse();
            Game.IsAutoTest = AppArgs.AutoTest != null;
            if (!string.IsNullOrEmpty(AppArgs.SaveDir)) SaveSystem.OverrideDirectory(AppArgs.SaveDir);
            SaveSystem.WritesSuspended = Game.TestWritesBlocked;
            Game.Root = transform;
            Game.Settings = SaveSystem.LoadSettings();
            if (!Game.Settings.comfortSetupDone && SystemInfo.graphicsMemorySize >= 8000)
                Game.Settings.graphicsPreset = (AppArgs.Mode ?? GameMode.Desktop) == GameMode.VR ? 2 : 3;
            var requested = AppArgs.Mode ?? GameMode.Desktop;
            Debug.Log($"[TS-Boot] {Application.productName} {Application.version} | args: {string.Join(" ", Environment.GetCommandLineArgs())}");
            Debug.Log($"[TS-Boot] requested mode = {requested}" + (AppArgs.ModeArgumentPresent ? "" : " (no --mode argument: defaulting to desktop)") +
                      (AppArgs.InvalidModeValue != null ? $" (ignored invalid --mode value '{AppArgs.InvalidModeValue}')" : ""));

            AudioDirector.Create(transform);
            Narration.Create(transform);
            Game.Input = new GameInput();
            Game.Cells = CellManager.Create(transform);
            Game.Tears = TearController.Create(transform);
            PostFx.Create(transform);
            Game.Story = StoryDirector.Create(transform);
            Tutorial.Create(transform);
            MenuSystem.Create(transform);
            Branding.Create(transform);
            UiPointer.Create(transform);
            Game.Passenger = PassengerController.Create(transform);
            Physics.IgnoreLayerCollision(Layers.PlayerBody, Layers.Pocket, true);
            Physics.IgnoreLayerCollision(Layers.Passenger, Layers.PlayerBody, true);
            Physics.IgnoreLayerCollision(Layers.Interactable, Layers.PlayerBody, true);

            var mode = requested;
            if (requested == GameMode.VR)
            {
                while (true)
                {
                    Game.Mode = GameMode.VR;
                    yield return XRBoot.StartXR();
                    if (XRBoot.Running) break;
                    // Explain on the desktop window and let the player decide. Never claim VR is working.
                    int choice = -1;
                    BuildPresentation(GameMode.Desktop);
                    Game.Cells.Activate(CellId.Menu);
                    Game.Rig.PlaceAt(Game.Cells.Active.Arrival.position, Game.Cells.Active.Arrival.eulerAngles.y);
                    Fader.I.FadeTo(0f, 0.5f);
                    MenuSystem.I.ShowVrFailure(XRBoot.LastError, c => choice = c);
                    while (choice < 0) yield return null;
                    MenuSystem.I.HideAll();
                    if (choice == 2) { MenuSystem.Quit(); yield break; }
                    if (choice == 1) { mode = GameMode.Desktop; Debug.Log("[TS-Boot] VR unavailable; player chose Desktop mode"); break; }
                    TearDownPresentation();
                }
            }

            Game.Mode = mode;
            if (m_Presentation == null || Game.Rig == null || (mode == GameMode.VR) != (Game.Rig is VRRig))
            {
                TearDownPresentation();
                BuildPresentation(mode);
            }
            Debug.Log("[TS-Boot] running in " + mode + " mode" + (mode == GameMode.VR ? " via " + XRBoot.RuntimeName : ""));

            if (Game.IsAutoTest)
            {
                gameObject.AddComponent<AutoTest>().Begin(AppArgs.AutoTest, AppArgs.ReportDir); // plays and checks the intro itself
                yield break;
            }
            // The branded intro after the initial load (skippable; Art/Branding/BRANDING.md).
            if (!AppArgs.NoIntro) yield return Branding.I.Intro();
            if (AppArgs.Demo) { Fader.I.FadeTo(1f, 0f); Game.Story.StartDemo(); yield break; }
            if (!string.IsNullOrEmpty(AppArgs.StartCell) && Enum.TryParse(AppArgs.StartCell, true, out CellId start))
            {
                Game.Story.StartAt(start);
                yield break;
            }
            yield return MenuSystem.I.ShowMainMenuRoutine();
            if (!Game.Settings.comfortSetupDone) MenuSystem.I.ShowFirstRun();
        }

        void BuildPresentation(GameMode mode)
        {
            Game.Mode = mode;
            Game.Input.Enable(mode);
            m_Presentation = new GameObject("Presentation");
            m_Presentation.transform.SetParent(transform, false);
            Hud.Create(m_Presentation.transform);
            Game.Rig = mode == GameMode.VR ? (PlayerRig)VRRig.Create(m_Presentation.transform) : DesktopRig.Create(m_Presentation.transform);
            Fader.Create(m_Presentation.transform, Game.Rig.Cam);
            Fader.I.FadeTo(1f, 0f);
            Game.Passport = PassportController.Create(m_Presentation.transform);
            GraphicsQuality.Apply(Game.Settings);
        }

        void TearDownPresentation()
        {
            if (m_Presentation == null) return;
            Destroy(m_Presentation);
            m_Presentation = null;
            Game.Rig = null;
            Game.Passport = null;
        }

        void OnApplicationQuit()
        {
            // Only a story in progress is saved here (returning to the menu saves it already). At the menu Game.Campaign can
            // hold the demo's throwaway run or a blank one, which must never overwrite the player's story.
            if (Game.Story != null && Game.Story.InCampaign) Game.SaveNow();
            SaveSystem.SaveSettings(Game.Settings);
            XRBoot.Shutdown();
        }
    }
}
