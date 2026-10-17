using System.Collections;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// Marrakech, remembered. While Saeed goes through his bag at home, the glasses case brings back the corner shop
    /// where he bought them: a short story told in still shots with a warm memory grade. The player does not walk or
    /// do chores here (VR players can still look around); it is skippable, and every shot is a fixed vantage (no camera
    /// motion, for comfort).
    /// </summary>
    public static class PrologueChapter
    {
        static bool s_Skip;

        public static IEnumerator Flashback()
        {
            var rig = Game.Rig;
            var c = Game.Campaign;
            var homeCell = Game.Cells.Active;
            var homeFeet = rig.Feet;
            float homeYaw = rig.Head.eulerAngles.y;
            s_Skip = false;
            Game.InputLocks++;
            Game.Passport?.Close();
            Hud.I?.SetObjective("", false);

            AudioDirector.I?.Play("pp_seam_shimmer", rig.Head.position + rig.Head.forward, 0.7f);
            yield return Fader.I.FadeOut(1f);

            // The memory: the same shop the player will later see corrupted, in its warm, ordinary evening.
            var shop = (ShopCell)Game.Cells.Activate(CellId.ShopNormal);
            var memory = shop.Lighting.Clone();
            memory.Saturation = -42f;
            memory.ColorFilter = new Color(1f, 0.86f, 0.68f);
            memory.Vignette = 0.46f;
            memory.PostExposure += 0.15f;
            memory.Bloom += 0.2f;
            PostFx.I?.SetPreset(memory);

            var props = new GameObject("FlashbackProps").transform;
            props.SetParent(shop.transform, false);
            var glasses = PropLibrary.Glasses(props, true).transform;
            glasses.position = shop.DisplaySpot.position + Vector3.up * 0.015f;
            glasses.rotation = Quaternion.Euler(-90f, 180f, 0f);
            var caseGo = PropLibrary.GlassesCase(props, true);
            caseGo.transform.position = shop.CounterTop.position + new Vector3(0f, 0f, 0.05f);
            caseGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var token = PropLibrary.Token(props).transform;
            token.position = shop.CounterTop.position + new Vector3(0.85f, 0.004f, -0.2f);
            shop.Recache();
            shop.RefreshProbe();

            var keeper = shop.Keeper;
            Vector3 L(float x, float y, float z) => shop.transform.TransformPoint(new Vector3(x, y, z));

            // Shot 1: the lane at dusk, the shop sign ahead.
            Frame(L(0f, 0f, -8.6f), L(0f, 2.7f, -3.2f));
            Hud.I?.ShowTitleCard("MARRAKECH", "A few days earlier. The last stop before home.", 4.5f);
            yield return Fader.I.FadeIn(1.4f);
            yield return Line("pro_01", null, 7f);

            // Shot 2: inside, at the counter. The shopkeeper.
            if (!s_Skip)
            {
                yield return Cut(() => Frame(L(0.2f, 0f, -1.05f), keeper != null ? keeper.transform.position + Vector3.up * 1.5f : L(0.55f, 1.5f, 0.75f)));
                if (keeper != null) keeper.Gesture();
                yield return Line("shop_01", keeper != null ? keeper.transform : null, 7f);
            }

            // Shot 3: the old optician's frames in the display.
            if (!s_Skip)
            {
                yield return Cut(() => Frame(L(-0.6f, 0f, -0.6f), glasses.position));
                AudioDirector.I?.Play("glasses_unfold", glasses.position, 0.5f);
                yield return Line("shop_02", keeper != null ? keeper.transform : null, 7f);
            }

            // Shot 4: the purchase. A festival token in the brass tray; the frames go into their case; the lid closes.
            if (!s_Skip)
            {
                yield return Cut(() => Frame(L(0.15f, 0f, -0.8f), caseGo.transform.position));
                yield return Move(token, shop.TraySpot.position + Vector3.up * 0.006f, Quaternion.identity, 0.7f);
                AudioDirector.I?.Play("coin_token", shop.TraySpot.position, 0.9f);
                if (keeper != null) keeper.Gesture();
                yield return Move(glasses, caseGo.transform.position + Vector3.up * 0.022f, caseGo.transform.rotation * Quaternion.Euler(-90f, 0f, 0f), 1.1f);
                yield return Line("shop_03", keeper != null ? keeper.transform : null, 8f);
            }
            if (!s_Skip)
            {
                PropLibrary.SetCaseOpen(caseGo, false);
                AudioDirector.I?.Play("case_close", caseGo.transform.position, 0.9f);
                yield return Line("shop_04", null, 5f);
                if (!s_Skip) yield return Wait(0.8f);
            }

            c.SetFlag(StoryFlags.ShopPurchased);
            c.SetFlag(StoryFlags.PrologueDone);
            Narration.I?.StopAll();

            // Back to the present.
            yield return Fader.I.FadeOut(1f);
            UnityEngine.Object.Destroy(props.gameObject);
            Game.Cells.Activate(homeCell != null ? homeCell.Id : CellId.ApartmentNormal);
            Game.Cells.Active.RefreshProbe();
            rig.PlaceAt(homeFeet, homeYaw);
            Game.InputLocks = Mathf.Max(0, Game.InputLocks - 1);
            yield return Fader.I.FadeIn(1.2f);
        }

        /// <summary>Places the player at a fixed vantage looking at a point (desktop also tilts the view).</summary>
        static void Frame(Vector3 feet, Vector3 lookAt)
        {
            var rig = Game.Rig;
            var flat = lookAt - feet; flat.y = 0f;
            float yaw = flat.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(flat).eulerAngles.y : 0f;
            rig.PlaceAt(feet, yaw);
            if (rig is DesktopRig desk)
            {
                float dist = Mathf.Max(0.05f, flat.magnitude);
                desk.SetPitch(Mathf.Atan2(rig.Head.position.y - lookAt.y, dist) * Mathf.Rad2Deg);
            }
        }

        static IEnumerator Cut(System.Action frame)
        {
            yield return Fader.I.FadeOut(0.45f);
            frame();
            yield return Fader.I.FadeIn(0.55f);
        }

        /// <summary>Plays a line and waits for it (or a timeout if the clip is missing), unless the player skips.</summary>
        static IEnumerator Line(string id, Transform speaker, float timeout)
        {
            if (s_Skip) yield break;
            bool done = false;
            Narration.I?.Say(id, speaker, false, () => done = true);
            if (Narration.I == null) done = true;
            float t = 0f;
            while (!done && t < timeout)
            {
                if (SkipPressed()) { s_Skip = true; yield break; }
                t += Time.deltaTime;
                yield return null;
            }
        }

        static IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds) { if (SkipPressed()) { s_Skip = true; yield break; } t += Time.deltaTime; yield return null; }
        }

        static IEnumerator Move(Transform t, Vector3 to, Quaternion rot, float seconds)
        {
            var from = t.position; var r0 = t.rotation;
            for (float k = 0f; k < 1f; )
            {
                if (SkipPressed()) { s_Skip = true; break; }
                k = Mathf.Min(1f, k + Time.deltaTime / seconds);
                float e = Mathf.SmoothStep(0f, 1f, k);
                t.position = Vector3.Lerp(from, to, e) + Vector3.up * Mathf.Sin(e * Mathf.PI) * 0.06f;
                t.rotation = Quaternion.Slerp(r0, rot, e);
                yield return null;
            }
            t.position = to; t.rotation = rot;
        }

        static bool SkipPressed()
        {
            var input = Game.Input;
            if (input == null) return false;
            if (input.Skip.WasPressedThisFrame()) return true;
            return Game.Mode == GameMode.VR && (input.RTrigger.WasPressedThisFrame() || input.LTrigger.WasPressedThisFrame());
        }
    }
}
