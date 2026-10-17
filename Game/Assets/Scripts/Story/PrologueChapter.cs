using System.Collections;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// Marrakech, the last foreign stop. A calm, warm prologue that teaches look, move, grab, inspect and place:
    /// browse the counter, inspect the glasses, pay with a festival token, case the glasses (never trying them on),
    /// then a short skippable trip home.
    /// </summary>
    public static class PrologueChapter
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() => Prologue.Routine = Run;

        public static IEnumerator Run()
        {
            var story = StoryDirector.I;
            story.TearsAllowed = false;
            story.ThreatAllowed = false;
            story.ShowVitality = false;
            Game.Passport.Available = true;

            yield return story.EnterCell(CellId.ShopNormal, false);
            var shop = (ShopCell)Game.Cells.Active;
            Hud.I?.ShowTitleCard("MARRAKECH", "The last stop before home", 4f);
            Narration.I?.Say("pro_01", null, false, null, false, true);
            Hud.I?.SetObjective("Step into the shop.", false);
            Tutorial.I?.Begin(TutorialStep.Look, TutorialStep.Move);

            // Props on the counter.
            var props = new GameObject("PrologueProps").transform;
            props.SetParent(shop.transform, false);
            var card1 = PropLibrary.MakeGrabbable(PropLibrary.Postcard(props, 2), "postcard", "Look at the postcard");
            card1.transform.position = shop.CounterTop.position + new Vector3(-1.05f, 0.002f, -0.12f);
            card1.transform.rotation = Quaternion.Euler(0, 12f, 0);
            var card2 = PropLibrary.MakeGrabbable(PropLibrary.Postcard(props, 4), "postcard", "Look at the postcard");
            card2.transform.position = shop.CounterTop.position + new Vector3(-0.95f, 0.004f, 0.1f);
            card2.transform.rotation = Quaternion.Euler(0, -18f, 0);
            var glassesGo = PropLibrary.Glasses(props, true);
            glassesGo.transform.position = shop.DisplaySpot.position + Vector3.up * 0.015f;
            glassesGo.transform.rotation = Quaternion.Euler(-90, 180f, 0);
            glassesGo.AddComponent<BoxCollider>().size = new Vector3(0.15f, 0.06f, 0.06f);
            var glasses = PropLibrary.MakeGrabbable(glassesGo, "glasses", "Inspect the glasses");
            // Record actions from the moment props exist, so doing things "early" never stalls the chapter.
            bool inspected = false;
            glasses.Grabbed += (g, who) => inspected = true;
            var caseGo = PropLibrary.GlassesCase(props, true);
            caseGo.transform.position = shop.CounterTop.position + new Vector3(0.0f, 0f, 0.05f);
            caseGo.transform.rotation = Quaternion.Euler(0, 180f, 0);
            var tokenGo = PropLibrary.Token(props);
            tokenGo.transform.position = shop.CounterTop.position + new Vector3(0.85f, 0.004f, -0.2f);
            var token = PropLibrary.MakeGrabbable(tokenGo, "festival_token", "Take your festival token");
            foreach (var g in new[] { card1, card2, glasses, token }) g.SetHome();

            // Brass tray socket for the token, and a socket inside the open case for the glasses.
            var tray = new GameObject("TraySocket");
            tray.transform.SetParent(props, false);
            tray.transform.position = shop.TraySpot.position;
            var tcol = tray.AddComponent<BoxCollider>(); tcol.isTrigger = true; tcol.size = new Vector3(0.2f, 0.08f, 0.2f);
            var traySocket = tray.AddComponent<Socket>();
            traySocket.Accepts.Add("festival_token");
            traySocket.LockWhenFilled = true;
            var inCase = new GameObject("CaseSocket");
            inCase.transform.SetParent(caseGo.transform, false);
            inCase.transform.localPosition = new Vector3(0, 0.02f, 0);
            inCase.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            var ccol = inCase.AddComponent<BoxCollider>(); ccol.isTrigger = true; ccol.size = new Vector3(0.18f, 0.08f, 0.1f);
            var caseSocket = inCase.AddComponent<Socket>();
            caseSocket.Accepts.Add("glasses");
            caseSocket.LockWhenFilled = true;
            caseSocket.Usable = false;
            traySocket.Usable = false;
            shop.Recache();

            // 1) Enter.
            while (Game.Rig.Feet.z < shop.DoorPoint.position.z + 0.4f) yield return null; // still outside in the lane
            Narration.I?.Say("shop_01", shop.Keeper.transform, false, null, false, true);
            shop.Keeper.Gesture();
            Tutorial.I?.Begin(TutorialStep.Grab, TutorialStep.OpenPassport);
            Hud.I?.SetObjective("Browse the counter. Pick up a postcard, and open your passport to look at the festival insert.", false);
            bool grabbedSomething = false;
            foreach (var gr in new[] { card1, card2, token }) gr.Grabbed += (g, who) => grabbedSomething = true;
            while (!((grabbedSomething || inspected || Tutorial.I.IsDone(TutorialStep.Grab)) && (Tutorial.I.IsDone(TutorialStep.OpenPassport) || Game.Passport.IsOpen))) yield return null;
            yield return new WaitForSeconds(0.6f);

            // 2) The glasses.
            Hud.I?.SetObjective("Inspect the old optician's glasses in the display.", false);
            while (!inspected) yield return null;
            Narration.I?.Say("shop_02", shop.Keeper.transform, false, null, true);
            shop.Keeper.Gesture();
            yield return new WaitForSeconds(2.5f);

            // 3) Pay with a festival token (a game prop, not real money).
            traySocket.Usable = true;
            Hud.I?.SetObjective("Pay: place your festival token in the brass tray.", false);
            while (traySocket.Current == null) yield return null;
            AudioDirector.I?.Play("coin_token", shop.TraySpot.position, 0.9f);
            Narration.I?.Say("shop_03", shop.Keeper.transform, false, null, true);
            shop.Keeper.Gesture();
            Narration.I?.Say("shop_04");
            Game.Campaign.SetFlag(StoryFlags.ShopPurchased);

            // 4) Into the case, without trying them on.
            caseSocket.Usable = true;
            Hud.I?.SetObjective("Put the glasses in their case. (Don't try them on yet.)", false);
            while (caseSocket.Current == null) yield return null;
            AudioDirector.I?.Play("case_close", caseGo.transform.position, 0.9f);
            PropLibrary.SetCaseOpen(caseGo, false);
            yield return new WaitForSeconds(1.2f);
            Tutorial.I?.End();
            Hud.I?.SetObjective("", false);

            // 5) The short trip home (skippable, no camera motion).
            Game.Campaign.SetFlag(StoryFlags.PrologueDone);
            Game.SaveCheckpoint(CellId.ApartmentNormal);
            yield return Travel();
        }

        static IEnumerator Travel()
        {
            Game.InputLocks++;
            yield return Fader.I.FadeOut(1.2f);
            AudioDirector.I?.SetAmbience(null, 1.5f);
            AudioDirector.I?.Play("stamp_hit", null, 0.8f);
            Hud.I?.ShowTitleCard("MARRAKECH  →  ABU DHABI", "Eleven countries. One passport. Home tonight.", 5f);
            Hud.I?.ShowToast("Skip: " + Game.Input.Label(Intent.Skip), 4f);
            float t = 0f;
            while (t < 5.5f)
            {
                if (Game.Input.Skip.WasPressedThisFrame() || (Game.Mode == GameMode.VR && (Game.Input.RTrigger.WasPressedThisFrame() || Game.Input.LTrigger.WasPressedThisFrame()))) break;
                t += Time.deltaTime;
                yield return null;
            }
            Game.InputLocks = Mathf.Max(0, Game.InputLocks - 1);
        }
    }
}
