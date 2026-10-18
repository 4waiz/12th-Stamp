using System.Collections;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// --autotest=guide: every place has a walkthrough. Arriving shows the briefing; then the step banner, a marker on the
    /// first target and the minimap with its map; the full map opens on M. Egypt is played through: turning the
    /// reflector onto the false shadow moves the guide on to the trace, and verifying moves it on to the stamp.
    /// Screenshots of each. Desktop only.
    /// </summary>
    public partial class AutoTest
    {
        IEnumerator GuideFlow()
        {
            Note("--- Guide: walkthroughs, markers, minimap and map ---");
            MenuSystem.I.HideAll();
            yield return new WaitForSeconds(0.3f);
            Check(Guide.I != null && MapView.I != null, "the guide and the map are running");
            if (Guide.I == null || MapView.I == null) yield break;

            var places = new[] { CellId.ApartmentCorrupt, CellId.Egypt, CellId.Paris, CellId.NewYork, CellId.Japan, CellId.Riyadh, CellId.Agra, CellId.Rio, CellId.London, CellId.ShopCorrupt, CellId.Berlin };
            foreach (var id in places)
            {
                yield return Enter(id);
                yield return new WaitForSeconds(0.4f);
                var g = Guide.I.Current;
                Check(g != null && g.Steps.Count >= 2 && Guide.I.StepIndex >= 0, id + ": a walkthrough with " + (g != null ? g.Steps.Count : 0) + " steps, on step " + (Guide.I.StepIndex + 1));
                Check(Guide.I.BriefingShowing, id + ": the briefing shows on arrival");
                if (id == CellId.Egypt || id == CellId.Paris) yield return Shot("g_" + id + "_briefing");
                Guide.I.DismissBriefing();
                yield return new WaitForSeconds(0.3f);
                Check(Guide.I.BannerShowing, id + ": the step banner is on screen");
                Check(MapView.I.HasMap && MapView.I.MiniShowing, id + ": the minimap shows a map of the place (" + MapView.I.MapSize + ")");
                var st = g != null && Guide.I.StepIndex >= 0 && Guide.I.StepIndex < g.Steps.Count ? g.Steps[Guide.I.StepIndex] : null;
                Check(st != null && (st.Target == null || Guide.I.Target.HasValue), id + ": the current step's marker is placed" + (st != null && st.Target == null ? " (this step has no place)" : ""));
                var country = Game.Cells.Active.Country;
                if (country != null && Postcard.All.Exists(p => p != null && p.isActiveAndEnabled && p.Country == country.Id))
                    Check(MapView.I.MiniLabel.Contains("postcards"), id + ": the minimap counts this place's postcards (" + MapView.I.MiniLabel + ")");
                yield return Shot("g_" + id + "_banner");
                MapView.I.SetFull(true);
                yield return new WaitForSeconds(0.25f);
                if (id == CellId.Egypt || id == CellId.Japan || id == CellId.Riyadh || id == CellId.London || id == CellId.ShopCorrupt) yield return Shot("g_" + id + "_map");
                MapView.I.SetFull(false);
            }

            // Egypt, played through.
            yield return Enter(CellId.Egypt);
            Guide.I.DismissBriefing();
            var e = (EgyptCell)Game.Cells.Active;
            yield return new WaitForSeconds(0.3f);
            Check(Guide.I.StepIndex == 0, "Egypt starts on the reflector step");
            bool burning = false;
            for (int i = 0; i < e.Reflector.Steps && !burning; i++)
            {
                e.Reflector.SetIndex(i, true);
                yield return new WaitForSeconds(0.35f);
                var r = e.WrongShadow.GetComponent<Renderer>();
                burning = !e.WrongShadow.gameObject.activeSelf || r.sharedMaterial.GetColor("_Color").a < 0.7f;
            }
            Check(burning, "a reflector angle puts the beam on the false shadow");
            yield return Shot("g_Egypt_burning");
            int traceStep = Guide.I.Current.Steps.FindIndex(s => s.Label == "Trace");
            int stampStep = Guide.I.Current.Steps.FindIndex(s => s.Label == "Stamp");
            yield return WaitUntil(() => Guide.I.StepIndex == traceStep, 6f, "the shadow burns away and the guide moves on to the trace");
            yield return new WaitForSeconds(0.3f);
            yield return Shot("g_Egypt_trace");
            e.Trace.Use(Game.Rig.MainHand);
            yield return WaitUntil(() => Game.Campaign.IsVerified(CountryId.Egypt), 6f, "pressing the passport to the trace verifies Egypt");
            yield return WaitUntil(() => Guide.I.StepIndex == stampStep && Game.Story.WaitingStamp != null, 8f, "the guide moves on to the stamp");
            yield return new WaitForSeconds(0.4f);
            yield return Shot("g_Egypt_stamp");
            Check(Guide.I.Target.HasValue && Game.Story.WaitingStamp != null && (Guide.I.Target.Value - Game.Story.WaitingStamp.transform.position).magnitude < 0.5f,
                "the marker is on the stamp");
        }
    }
}
