using System.Collections;
using System.Linq;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// --autotest=shift: an art-review pass over the glasses moment. The glasses rise to the eyes, the dimension shift tears
    /// the apartment apart (props tearing loose, the walls peeling into pages, the Between's light through the cracks, the
    /// inhale, the arrival), the Between (the red-void causeway, the runner and its shadow, the three stamp pieces, the seal)
    /// and the arrival in the Margin, each framed for a screenshot, with checks that the pieces work: the shift swaps at its
    /// peak and puts every prop back, the stamps collect, the seal opens only after all three, the runner gains on a standing
    /// player and catching throws him back, and leaving restores the apartment. Then a tear-travel-length shift into Egypt
    /// and a Gentle one. Desktop only; the camera is placed by hand.
    /// </summary>
    public partial class AutoTest
    {
        IEnumerator ShiftFlow()
        {
            Note("--- Shift: the glasses go on, the dimension shift, the Between ---");
            MenuSystem.I.HideAll();
            var rig = Game.Rig as DesktopRig;
            Check(rig != null, "shift review runs on the desktop rig");
            if (rig == null) yield break;

            var apt = (ApartmentCell)Game.Cells.Activate(CellId.ApartmentNormal);
            yield return Frame(apt, new Vector3(0.6f, 1.6f, -1.6f), new Vector3(0.4f, 1.1f, 0.4f), null);
            rig.enabled = false;
            var head = Game.Rig.Head;
            var home = Game.Rig.Feet;
            float homeYaw = head.eulerAngles.y;

            // The glasses rise from in front of the player to the eyes.
            var props = new GameObject("ShiftReviewProps").transform;
            props.SetParent(apt.transform, false);
            var glasses = PropLibrary.Glasses(props, false).transform;
            glasses.position = head.position + head.forward * 0.55f - head.up * 0.25f;
            glasses.rotation = Quaternion.LookRotation(head.forward, Vector3.up) * Quaternion.Euler(-60f, 0f, 0f);
            var rise = StartCoroutine(HomecomingChapter.GlassesOn(glasses));
            yield return new WaitForSeconds(0.8f);
            yield return Shot("s01_glasses_rising");
            yield return new WaitForSeconds(0.85f);
            yield return Shot("s02_glasses_worn");
            yield return rise;
            Check(!glasses.gameObject.activeSelf, "the glasses end up worn (hidden once on)");
            Destroy(props.gameObject);

            // The shift: the room comes apart; the swap happens behind the fade at the peak.
            bool swapped = false;
            var shift = StartCoroutine(DimensionShift.Run(() =>
            {
                var margin = Game.Cells.Activate(CellId.ApartmentCorrupt);
                BetweenRun.Enter(margin);
                swapped = true;
            }, 4.5f, CellId.None, BetweenRun.Glimpse));
            yield return new WaitForSeconds(1.3f);
            yield return Shot("s03a_shift_tearing_loose");
            yield return new WaitForSeconds(0.9f);
            yield return Shot("s03_shift_building");
            var report = DimensionShift.Last;
            var hidden = DimensionShift.LastHidden.ToList();
            Check(report.Loose >= 3 && hidden.Count >= report.Loose && hidden.All(r => r != null && !r.enabled),
                "props tear loose: " + report.Loose + " stand-ins (" + hidden.Count + " pieces) circle him, the originals hidden");
            Check(report.Pages >= 6, "the walls peel away into pages (" + report.Pages + ")");
            Check(report.Tears >= 3 && report.Shafts >= 3, "the other place's light comes through " + report.Tears + " cracks, in shafts");
            yield return new WaitForSeconds(1.9f);
            yield return Shot("s04_shift_peak");
            yield return WaitUntil(() => DimensionShift.Phase == DimensionShift.Stage.Inhale, 2f, "the build-up ends in the inhale");
            yield return new WaitForSeconds(0.25f);
            yield return Shot("s04b_shift_inhale");
            yield return WaitUntil(() => DimensionShift.Phase == DimensionShift.Stage.Arrival, 3f, "the slam swaps him through");
            yield return new WaitForSeconds(0.6f);
            yield return Shot("s04c_between_assembling");
            yield return shift;
            Check(swapped && BetweenRun.Current != null, "the shift swaps into the Between at its peak");
            Check(hidden.All(r => r != null && r.enabled), "every prop that tore loose is back in its place at home");
            Check(DimensionShift.Last.InhaleLight <= 0.1f, "the inhale drains the room to " + (DimensionShift.Last.InhaleLight * 100f).ToString("0") + "% of its light before the slam");
            Check(DimensionShift.Last.Settled >= 10, "the debris that came through settles in the Between (" + DimensionShift.Last.Settled + " pieces)");
            Note("shift sounds: " + DimensionShift.Last.Sounds);
            yield return null; // its last pieces are destroyed at the end of the frame it ends in
            Check(GameObject.Find("DimensionShift") == null && GameObject.Find("ShiftArrival") == null, "nothing of the shift is left behind");
            Check(Game.InputLocks == 0, "input is unlocked after the shift");

            var run = BetweenRun.Current;
            var cell = Game.Cells.Active;
            Vector3 B(float x, float y, float z) => cell.transform.InverseTransformPoint(run.transform.TransformPoint(new Vector3(x, y, z)));
            yield return Frame(cell, B(0f, 1.6f, 3f), B(0f, 1.2f, 40f), "s05_between_start");
            yield return Frame(cell, B(3.2f, 4.5f, 10f), B(-30f, 10f, 120f), "s06_between_landmarks");

            // The runner: it gains on a standing player, its shadow ahead of it; caught, he is thrown back to the start.
            run.AutoPilot = false;
            var chase = StartCoroutine(BetweenRun.Play());
            yield return Frame(cell, B(0f, 1.6f, 3f), B(0f, 1.0f, -10f), null);
            yield return new WaitForSeconds(2.6f);
            yield return Shot("s07_runner_behind");
            float before = Vector3.Distance(run.Runner.transform.position, Game.Rig.Feet);
            yield return new WaitForSeconds(1f);
            yield return Shot("s08_runner_close");
            float after = Vector3.Distance(run.Runner.transform.position, Game.Rig.Feet);
            Check(after < before, "the runner gains on a standing player (" + before.ToString("0.0") + " m -> " + after.ToString("0.0") + " m)");
            yield return WaitUntil(() => run.Catches > 0, 8f, "caught, he is thrown back to the start");
            yield return new WaitForSeconds(1.4f);
            run.Hold();

            // The pieces and the seal.
            Check(!run.Portal.Ready, "the seal stays shut until all three pieces are held");
            yield return Frame(cell, B(-1.2f, 1.6f, 31f), B(-1.2f, 1.5f, 36f), "s09_stamp_piece");
            foreach (var s in run.Stamps) s.Collect();
            yield return new WaitForSeconds(0.6f);
            Check(run.Collected == BetweenRun.Pieces && run.Portal.Ready, "three pieces collected; the seal opens");
            yield return Frame(cell, B(0f, 1.6f, 132f), B(0f, 2.3f, 146f), "s10_seal_open");
            run.Portal.Collect();
            yield return WaitUntil(() => run.Finished, 3f, "reaching the seal ends the run");
            yield return chase;

            // The seal throws him into the Margin: the apartment, copied, where he stood.
            var back = StartCoroutine(DimensionShift.Run(() => BetweenRun.Leave(home, homeYaw), 2f, CellId.ApartmentCorrupt));
            yield return WaitUntil(() => DimensionShift.Phase == DimensionShift.Stage.Arrival, 6f, "the seal's shift reaches the Margin");
            yield return new WaitForSeconds(0.6f);
            yield return Shot("s10b_margin_assembling");
            yield return back;
            Check(BetweenRun.Current == null && Game.Cells.Active.Id == CellId.ApartmentCorrupt, "the run ends in the Margin apartment, the Between removed");
            Check(Vector3.Distance(Game.Rig.Feet, home) < 0.6f, "back where he stood when he put the glasses on");
            yield return new WaitForSeconds(0.5f);
            yield return Shot("s11_margin_arrival");

            // Tear travel uses the same shift with a 1.8 s build-up: Egypt's sun through the cracks, then the arrival there.
            Cell egypt = null;
            var travel = StartCoroutine(DimensionShift.Run(() =>
            {
                egypt = Game.Cells.Activate(CellId.Egypt);
                Game.Rig.PlaceAt(egypt.Arrival.position, egypt.Arrival.eulerAngles.y);
            }, 1.8f, CellId.Egypt));
            yield return new WaitForSeconds(1.45f);
            yield return Shot("s12_short_shift_egypt_light");
            var quick = DimensionShift.Last;
            Check(quick.Glimpse == DimensionShift.GlimpseOf(CellId.Egypt) && quick.Loose > 0 && quick.Pages > 0 && quick.Tears > 0,
                "a 1.8 s shift (tear travel) still tears the room loose (" + quick.Loose + " props, " + quick.Pages + " pages, " + quick.Tears + " cracks in Egypt's light)");
            yield return WaitUntil(() => DimensionShift.Phase == DimensionShift.Stage.Arrival, 3f, "the short shift reaches Egypt");
            yield return new WaitForSeconds(0.6f);
            yield return Shot("s13_egypt_assembling");
            yield return travel;
            Check(egypt != null && Game.Cells.Active == egypt && Game.InputLocks == 0 && !DimensionShift.Running, "arrived in Egypt, input unlocked");

            // Gentle: shorter and softer (no tunnel, no whispers, fewer pieces); the same beats.
            int scare = Game.Settings.scareIntensity;
            Game.Settings.scareIntensity = 0;
            var gentle = StartCoroutine(DimensionShift.Run(() => { }, 3f, CellId.Paris));
            yield return new WaitForSeconds(1.2f);
            yield return Shot("s14_gentle_peak");
            yield return gentle;
            Game.Settings.scareIntensity = scare;
            var soft = DimensionShift.Last;
            Check(soft.Loose <= 5 && soft.Pages * 2 <= report.Pages && soft.Tears <= 3 && Game.InputLocks == 0 && !DimensionShift.Running,
                "Gentle tears less loose (" + soft.Loose + " props, " + soft.Pages + " pages, " + soft.Tears + " cracks, against " + report.Loose + ", " + report.Pages + " and " + report.Tears + ") and finishes");
            rig.enabled = true;
        }
    }
}
