using System;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// The walkthrough for each place: what to do, in order, how to tell each step is done (read from the encounter's
    /// public state, never changed), where the marker goes, and what the danger is. Guide shows it. Where nothing in the
    /// world gives the answer away (Japan's hook, Agra's pedestal, Riyadh's arch) the marker shows it: the player asked
    /// to be told what to do.
    /// </summary>
    public static class GuideBook
    {
        public static PlaceGuide For(Cell cell)
        {
            if (cell == null) return null;
            try
            {
                switch (cell.Id)
                {
                    case CellId.Menu:
                    case CellId.ApartmentNormal:   // the homecoming and the prologue tell their own story
                    case CellId.ShopNormal:
                    case CellId.Epilogue:
                        return null;
                    case CellId.ApartmentCorrupt: return Uae((ApartmentCell)cell);
                    case CellId.ShopCorrupt: return Morocco((ShopCell)cell);
                }
                switch (cell)
                {
                    case EgyptCell e: return Egypt(e);
                    case ParisCell p: return Paris(p);
                    case NewYorkCell n: return NewYork(n);
                    case JapanCell j: return Japan(j);
                    case RiyadhCell r: return Riyadh(r);
                    case AgraCell a: return Agra(a);
                    case RioCell r: return Rio(r);
                    case LondonCell l: return London(l);
                    case BerlinCell b: return Berlin(b);
                }
                return Generic(cell);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[TS-Guide] no guide for " + cell.Id + ": " + ex.Message);
                return null;
            }
        }

        // ------------------------------------------------------------------ shared pieces
        static string Key(Intent i) => Hud.KeyTag(i);
        static string Use => Key(Intent.Interact);
        static string Grab => Key(Intent.Grab);
        static bool Vr => Game.Mode == GameMode.VR;
        static string RunKey => Vr ? "push the left stick fully forward" : "hold <color=#EDCF99><b>SHIFT</b></color>";
        static string Activate => Vr ? "pull the trigger" : "click or press " + Use;
        static bool Verified(CountryId id) => Game.Campaign != null && Game.Campaign.IsVerified(id);
        static Vector3? Pos(Component c) => c != null && c.gameObject.activeInHierarchy ? c.transform.position : (Vector3?)null;
        static Vector3 W(Cell c, float x, float y, float z) => c.transform.TransformPoint(new Vector3(x, y, z));
        static float Dist(Vector3 p) => Game.Rig != null ? (p - Game.Rig.Feet).magnitude : 999f;
        static bool Holding(Grabbable g) => g != null && g.Holder != null;
        static bool TearOpen => Game.Tears != null && Game.Tears.Active != null;
        static Vector3? TearAt => TearOpen ? Game.Tears.Active.OpeningCenter : (Vector3?)null;
        static bool PassportOpen => Game.Passport != null && Game.Passport.IsOpen;

        /// <summary>Press the passport to the revealed trace.</summary>
        static GuideStep TraceStep(CountryId id, Func<TraceMark> trace, string where)
        {
            return new GuideStep
            {
                Text = "Walk to the glowing trace " + where + " and press your passport to it  " + Use,
                Hint = "The trace is a softly glowing stamp mark. Stand close, look straight at it and press " + Use + ". If it says something is in the way, deal with that first.",
                Done = () => Verified(id),
                Target = () =>
                {
                    var t = trace();
                    return t != null && t.Revealed ? t.transform.position : (Vector3?)null;
                },
                Label = "Trace",
            };
        }

        /// <summary>After verifying, the country's stamp appears; reaching it shifts the player on.</summary>
        static GuideStep StampStep(CountryId id, string chaser = "the Passenger")
        {
            return new GuideStep
            {
                Text = "Run to the stamp before " + chaser + " reaches you: it carries you to the next country  (run: " + RunKey + ")",
                Hint = "Follow the marker (and the gold star on the map). The stamp is a glowing disc; touch it to shift.",
                Done = () => Verified(id) && (Game.Story == null || Game.Story.WaitingStamp == null),
                Target = () => Game.Story != null && Game.Story.WaitingStamp != null ? Game.Story.WaitingStamp.transform.position : (Vector3?)null,
                Label = "Stamp",
            };
        }

        /// <summary>The way on when there is no stamp to run for: the passport and a tear.</summary>
        static GuideStep TravelStep(Cell cell)
        {
            return new GuideStep
            {
                Text = "Open your passport  " + Key(Intent.Passport) + ", choose where to go next  " + Key(Intent.Destination) +
                       ", aim at a seam and open a tear  " + Key(Intent.Tear) + ". Widen it to a doorway and walk through",
                Hint = "Seams are faint glowing lines on walls (teal circles on the map). Keep holding " + Key(Intent.Tear) + " until the tear is wide enough to step through.",
                Done = () => false,
                Target = () => NearestSeam(cell),
                Label = "Seam",
            };
        }

        static Vector3? NearestSeam(Cell cell, bool revealedOnly = true)
        {
            if (cell == null || Game.Rig == null) return null;
            Vector3? best = null;
            float bd = float.MaxValue;
            foreach (var s in cell.Seams)
            {
                if (s == null || !s.isActiveAndEnabled || (revealedOnly && !s.Revealed)) continue;
                float d = (s.Center - Game.Rig.Feet).sqrMagnitude;
                if (d < bd) { bd = d; best = s.Center; }
            }
            return best;
        }

        /// <summary>A warning when the Passenger is close or chasing.</summary>
        static string PassengerAlert(float range, string near)
        {
            var pc = Game.Passenger;
            if (pc == null || !pc.Visible || Game.Rig == null) return null;
            if (pc.Chasing) return "<b>RUN.</b> It is chasing you: " + RunKey + ".";
            float d = (pc.transform.position - Game.Rig.Feet).magnitude;
            return d < range ? near : null;
        }

        static string TearAlert()
        {
            if (!TearOpen) return null;
            float c = Game.Tears.Contamination;
            return c > 0.6f ? "The tear is straining. Seal it now: hold " + Key(Intent.Seal) + "." : null;
        }

        static string FirstOf(params Func<string>[] alerts)
        {
            foreach (var a in alerts)
            {
                var s = a();
                if (!string.IsNullOrEmpty(s)) return s;
            }
            return null;
        }

        /// <summary>For a place without its own walkthrough: the country's objective, then the stamp and travel.</summary>
        static PlaceGuide Generic(Cell cell)
        {
            var c = cell.Country;
            if (c == null) return null;
            var g = new PlaceGuide { Title = c.DisplayName + " · " + c.EncounterTitle, Briefing = c.Objective };
            g.Steps.Add(new GuideStep { Text = c.Objective, Done = () => Verified(c.Id), Target = () => NearestSeam(cell) });
            g.Steps.Add(StampStep(c.Id));
            g.Steps.Add(TravelStep(cell));
            g.Alert = () => PassengerAlert(10f, "The Passenger is close. Keep your distance.");
            return g;
        }

        // ------------------------------------------------------------------ UAE: the Margin apartment (the tutorial)
        static PlaceGuide Uae(ApartmentCell a)
        {
            const CountryId id = CountryId.UAE;
            var enc = a.Encounter as UaeEncounter;
            var g = new PlaceGuide
            {
                Title = "The Margin · The Apartment That Kept You",
                Briefing = "Your apartment, copied. Almost everything here is a forgery; one detail is genuine. Your passport can open tears into other countries, and light from them shows what is real.",
                Danger = "Soon something made of glass steps out of the bathroom. It is slow, but it kills at a touch: keep away from it and leave through a tear.",
            };
            Func<bool> traced = () => Verified(id) || (enc != null && enc.TraceShown) || (a.Trace != null && a.Trace.Revealed);
            g.Steps.Add(new GuideStep
            {
                Text = "Open your passport  " + Key(Intent.Passport),
                Hint = "Your passport is your key to every country. " + (Vr ? "Press X on the left controller." : "Press F."),
                Done = () => PassportOpen || traced(),
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Aim at a seam (a faint glowing line on the wall) and hold  " + Key(Intent.Tear) + "  to open a tear to Egypt",
                Hint = "Look at the window, the kitchen wall or the hall with the passport open: a seam lights up when you aim at it. Hold " + Key(Intent.Tear) + " until it opens.",
                Done = traced,
                Target = () => NearestSeam(a),
                Label = "Seam",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Egypt's light falls on the fridge. Find the one note with real handwriting and press your passport to it  " + Use,
                Hint = "The genuine note glows on the fridge door. Stand in front of it and press " + Use + ".",
                Done = () => Verified(id),
                Target = () => Pos(a.Trace),
                Label = "Genuine note",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Seal the tear: hold  " + Key(Intent.Seal),
                Hint = "Tears let things through. Close it before anything notices: hold " + Key(Intent.Seal) + ".",
                Done = () => !TearOpen || (enc != null && enc.ThreatStarted),
                Target = () => TearAt,
                Label = "Tear",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Get out: open a tear to Egypt at any seam, widen it to a <b>doorway</b> (keep holding  " + Key(Intent.Tear) + ") and walk through",
                Hint = "Don't let the glass figure reach you. Any seam works; the tear is a doorway once it is tall enough to walk into.",
                Done = () => false,
                Target = () => TearOpen ? TearAt : NearestSeam(a),
                Label = "Way out",
            });
            g.Alert = () => PassengerAlert(6f, "It is close. Move away and open your way out.");
            return g;
        }

        // ------------------------------------------------------------------ Egypt
        static PlaceGuide Egypt(EgyptCell e)
        {
            const CountryId id = CountryId.Egypt;
            var enc = e.Encounter as EgyptEncounter;
            EgyptCues.Nudges = false;   // the guide says it: the beam's real target is the shadow, not the figure
            var g = new PlaceGuide
            {
                Title = "Egypt · The Sun Without a Shadow",
                Briefing = "In this courtyard one shadow points <b>toward</b> the sun. It is false, and the real arrival trace is hidden under it.",
                Danger = "After a while a figure wrapped in linen rises from the sand behind you. Light only <b>holds it back</b>; it is not the target. Burn the shadow, or run (" + RunKey + ").",
            };
            Func<bool> revealed = () => Verified(id) || (enc != null && enc.Revealed);
            Func<Vector3> shadow = () => e.transform.TransformPoint(e.ShadowTargetLocal);
            g.Steps.Add(new GuideStep
            {
                Text = "Turn the bronze reflector  " + Use + "  to swing its sunbeam",
                Hint = "The reflector is the bronze mirror on its stand in the courtyard. Stand next to it and press " + Use + ".",
                Done = () => revealed() || (enc != null && enc.Cues != null && enc.Cues.ReflectorTurned),
                Target = () => e.Reflector.transform.position,
                Label = "Reflector",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Aim the beam at the <b>wrong shadow</b> by the pillar (it points at the sun). Keep turning  " + Use + "  until it smoulders and burns away",
                Hint = "Each press turns the mirror 15 degrees; watch the bright streak on the paving. The shadow sparks as the beam gets close and burns when it is on it. The figure from the sand is not the target: light only holds it back.",
                Done = revealed,
                Target = () => shadow(),
                Progress = () =>
                {
                    if (enc == null) return -1f;
                    float warm = enc.Cues != null ? Mathf.Clamp01(1f - enc.Cues.AimError / 32f) * 0.35f : 0f;
                    return Mathf.Max(enc.BurnProgress, warm);
                },
                Label = "Wrong shadow",
            });
            g.Steps.Add(TraceStep(id, () => e.Trace, "where the shadow was"));
            g.Steps.Add(StampStep(id, "the wrapped figure"));
            g.Steps.Add(TravelStep(e));
            g.Alert = () => PassengerAlert(12f, "The wrapped figure is close. Light only holds it back: burn the shadow by the pillar, or run (" + RunKey + ").");
            return g;
        }

        // ------------------------------------------------------------------ France: Paris
        static PlaceGuide Paris(ParisCell p)
        {
            const CountryId id = CountryId.France;
            var enc = p.Encounter as ParisEncounter;
            bool sun = Game.Campaign != null && Game.Campaign.HasResource(ResourceId.Sunlight);
            var g = new PlaceGuide
            {
                Title = "France · The Street That Folds",
                Briefing = "A figure in a raincoat stands at the verification desk, right on top of the trace. It hates sunlight" +
                           (sun ? ", and you carry Egypt's." : ". You don't have Egypt's sunlight yet, but a tear held open long enough brings out something it fears more."),
                Danger = "It kills at a touch. A tear left open too long rips wide open, and here that kills too: seal tears once they have done their job (hold " + Key(Intent.Seal) + ").",
            };
            Func<bool> cleared = () => Verified(id) || (enc != null && (enc.Cleared || enc.HandShown)) || (p.Trace != null && p.Trace.BlockedReason != null && p.Trace.BlockedReason() == null);
            g.Steps.Add(new GuideStep
            {
                Text = "Stand at the marked spot facing the festival poster board, open your passport  " + Key(Intent.Passport) + "  and choose <b>Egypt</b>  " + Key(Intent.Destination),
                Hint = "The poster board stands beside the desk. From the marked spot it is straight ahead; with the passport open, pick Egypt with " + Key(Intent.Destination) + ".",
                Done = () => cleared() || (PassportOpen && Game.Passport.Selected == CountryId.Egypt && Dist(p.TearVantage.position) < 2.5f) || TearOpen,
                Target = () => p.TearVantage.position,
                Label = "Stand here",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Aim at the poster board's seam and hold  " + Key(Intent.Tear) + "  to open a tear to Egypt. Widen it until the sunlight drives the figure off the desk",
                Hint = "The tear's sunbeam shines from the board toward the desk. The wider the tear, the stronger the light. Keep holding " + Key(Intent.Tear) + ".",
                Done = cleared,
                Target = () => p.DeskSeam != null ? p.DeskSeam.Center : (Vector3?)null,
                Label = "Seam",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Seal the tear before it rips: hold  " + Key(Intent.Seal),
                Hint = "Something reaches through tears that stay open. Sealing it is safe; the light has done its job.",
                Done = () => Verified(id) || !TearOpen,
                Target = () => TearAt,
                Label = "Tear",
            });
            g.Steps.Add(TraceStep(id, () => p.Trace, "on the desk's ledger"));
            g.Steps.Add(StampStep(id));
            g.Steps.Add(TravelStep(p));
            g.Alert = () => FirstOf(TearAlert, () => PassengerAlert(8f, "The figure is close. Keep the sunlight between you."));
            return g;
        }

        // ------------------------------------------------------------------ USA: New York
        static PlaceGuide NewYork(NewYorkCell n)
        {
            const CountryId id = CountryId.USA;
            var enc = n.Encounter as NewYorkEncounter;
            var g = new PlaceGuide
            {
                Title = "United States · The Last Arrival",
                Briefing = "Someone with lost luggage guards the arrival gate, standing on the trace. The security booth has power, a screen, and something that makes noise.",
                Danger = "It guards the gate and kills at a touch, but it is curious: noise draws it away.",
            };
            g.Steps.Add(new GuideStep
            {
                Text = "Switch on the security booth's power  " + Use,
                Hint = "The breaker is on the booth's west side, under the yellow hazard plate.",
                Done = () => Verified(id) || (enc != null && enc.Powered),
                Target = () => Pos(n.Breaker),
                Label = "Booth power",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Take the noisemaker from the booth  " + Grab,
                Done = () => Verified(id) || Holding(n.Noisemaker) || (enc != null && enc.NoiseOn),
                Target = () => Pos(n.Noisemaker),
                Label = "Noisemaker",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Switch it on (" + Activate + ") and put it down far from the gate, then step back",
                Hint = "Set it down on the other side of the hall, away from the gate. While it warbles the figure goes to investigate it. If you keep holding it, it comes to you.",
                Done = () => Verified(id) || (n.Trace != null && n.Trace.BlockedReason != null && n.Trace.BlockedReason() == null && enc != null && enc.NoiseOn && !Holding(n.Noisemaker)),
                Target = () => Holding(n.Noisemaker) ? W(n, -4.5f, 0.05f, 5.5f) : Pos(n.Noisemaker),
                Label = "Put it here",
            });
            g.Steps.Add(TraceStep(id, () => n.Trace, "at the arrival gate"));
            g.Steps.Add(StampStep(id));
            g.Steps.Add(TravelStep(n));
            g.Alert = () => PassengerAlert(6f, "It is close. Back off and let the noise do the work.");
            return g;
        }

        // ------------------------------------------------------------------ Japan
        static PlaceGuide Japan(JapanCell j)
        {
            const CountryId id = CountryId.Japan;
            var g = new PlaceGuide
            {
                Title = "Japan · The Empty Procession",
                Briefing = "Three route marks lie on the street, and a procession of shadows walks with no one casting it. Only the genuine mark keeps its shadow in the right light.",
                Danger = "A paper-lantern figure watches from the end of the street. Once the marks change it starts to stalk you: verify quickly, then run.",
            };
            Func<bool> solved = () => Verified(id) || (j.Trace != null && j.Trace.Revealed);
            g.Steps.Add(new GuideStep
            {
                Text = "Take the hand lantern  " + Grab,
                Done = () => solved() || Holding(j.HandLantern) || (j.Hooks.Count > j.CorrectHook && j.Hooks[j.CorrectHook].Current != null),
                Target = () => Pos(j.HandLantern),
                Label = "Lantern",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Hang it on the <b>middle hook</b>, where only the genuine route mark still casts a shadow  " + Use,
                Hint = "Hold the lantern up to a hook and press " + Use + ". Under the right hook two of the marks vanish; a wrong hook turns the shadows toward you, so take it down and try again.",
                Done = solved,
                Target = () => j.Hooks.Count > j.CorrectHook ? j.Hooks[j.CorrectHook].transform.position : (Vector3?)null,
                Label = "Middle hook",
            });
            g.Steps.Add(TraceStep(id, () => j.Trace, "on the genuine route mark (come at it from the south)"));
            g.Steps.Add(StampStep(id, "the lantern figure"));
            g.Steps.Add(TravelStep(j));
            g.Alert = () => PassengerAlert(7f, "The lantern figure is close. Keep moving.");
            return g;
        }

        // ------------------------------------------------------------------ Saudi Arabia: Riyadh
        static PlaceGuide Riyadh(RiyadhCell r)
        {
            const CountryId id = CountryId.SaudiArabia;
            var enc = r.Encounter as RiyadhEncounter;
            var g = new PlaceGuide
            {
                Title = "Saudi Arabia · The False Horizon",
                Briefing = "Three arches reflect the plaza, and two of them lie. The optical panel, set to your passport's registration mark, shows which one tells the truth.",
                Danger = "Nothing hunts you here until your stamp appears. A wrong arch folds the plaza back to the start and scrambles the panel.",
            };
            Func<bool> solved = () => Verified(id) || (enc != null && enc.Solved) || (r.Trace != null && r.Trace.Revealed);
            g.Steps.Add(new GuideStep
            {
                Text = "Turn the optical panel  " + Use + "  until it matches the registration mark on its plaque (the arrow)",
                Hint = "Each press turns it 45 degrees. From the start, three presses line it up; the reflections settle when it is right.",
                Done = () => solved() || (enc != null && enc.Aligned),
                Target = () => Pos(r.Panel),
                Label = "Optical panel",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Walk through the arch whose reflection shows the plaza as it is: the <b>right-hand arch</b>",
                Hint = "Only walk through an arch after the panel is set. The honest one shows the plaza without the false horizon.",
                Done = solved,
                Target = () => r.Arches.Count > r.HonestArch ? r.Arches[r.HonestArch].position : (Vector3?)null,
                Label = "Honest arch",
            });
            g.Steps.Add(TraceStep(id, () => r.Trace, "just beyond the arch"));
            g.Steps.Add(StampStep(id));
            g.Steps.Add(TravelStep(r));
            g.Alert = () => PassengerAlert(8f, "It is here now. Run for the stamp.");
            return g;
        }

        // ------------------------------------------------------------------ India: Agra
        static PlaceGuide Agra(AgraCell a)
        {
            const CountryId id = CountryId.India;
            var g = new PlaceGuide
            {
                Title = "India · The Reflection That Stayed",
                Briefing = "The garden pool reflects one person too many. Lamplight from the right pedestal shows what that extra reflection is standing on.",
                Danger = "When the reflection steps out of the pool it comes after you, and the lamp does not stop it. Verify fast, then run.",
            };
            Func<bool> solved = () => Verified(id) || (a.Trace != null && a.Trace.Revealed);
            g.Steps.Add(new GuideStep
            {
                Text = "Take the garden lamp  " + Grab,
                Done = () => solved() || Holding(a.Lamp) || (a.Pedestals.Count > a.CorrectPedestal && a.Pedestals[a.CorrectPedestal].Current != null),
                Target = () => Pos(a.Lamp),
                Label = "Lamp",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Set it on the <b>middle pedestal</b> by the pool, where its reflection shows what the extra person stands on  " + Use,
                Hint = "Hold the lamp over a pedestal and press " + Use + ". A wrong pedestal makes the reflected figure turn toward the light: take the lamp back and try the middle one.",
                Done = solved,
                Target = () => a.Pedestals.Count > a.CorrectPedestal ? a.Pedestals[a.CorrectPedestal].transform.position : (Vector3?)null,
                Label = "Middle pedestal",
            });
            g.Steps.Add(TraceStep(id, () => a.Trace, "on the pool's east kerb"));
            g.Steps.Add(StampStep(id, "the figure from the pool"));
            g.Steps.Add(TravelStep(a));
            g.Alert = () => PassengerAlert(8f, "The figure from the pool is coming. Verify and run.");
            return g;
        }

        // ------------------------------------------------------------------ Brazil: Rio
        static PlaceGuide Rio(RioCell r)
        {
            const CountryId id = CountryId.Brazil;
            var enc = r.Encounter as RioEncounter;
            var g = new PlaceGuide
            {
                Title = "Brazil · The Carnival Without Breathing",
                Briefing = "A frozen carnival. The float's shadow moves on its own and listens. Only the drum beat hides a stamp's sound.",
                Danger = "A stamp or a tear off the beat wakes the float's shadow: it slides toward you and drains you while it touches you.",
            };
            g.Steps.Add(new GuideStep
            {
                Text = "Go to the stamp plate in front of the float",
                Done = () => Verified(id) || (r.StampPlate != null && Dist(r.StampPlate.transform.position) < 2.2f) || (enc != null && enc.Hits > 0),
                Target = () => Pos(r.StampPlate),
                Label = "Stamp plate",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Stamp the plate  " + Use + "  exactly on the drum beat, <b>three times in a row</b> (the ring turns green on the beat)",
                Hint = "Watch the beater and the ring: press the moment the ring is smallest and green. An early or late stamp resets the count.",
                Done = () => Verified(id),
                Target = () => Pos(r.StampPlate),
                Progress = () => enc != null ? enc.Hits / 3f : -1f,
                Label = "On the beat",
            });
            g.Steps.Add(StampStep(id, "the float's shadow"));
            g.Steps.Add(TravelStep(r));
            g.Alert = () => enc != null && enc.AlertSeconds > 0f ? "The float's shadow heard you. Step away from it and wait for the beat." : null;
            return g;
        }

        // ------------------------------------------------------------------ United Kingdom: London
        static PlaceGuide London(LondonCell l)
        {
            const CountryId id = CountryId.UnitedKingdom;
            var enc = l.Encounter as LondonEncounter;
            var g = new PlaceGuide
            {
                Title = "United Kingdom · The Minute Before Midnight",
                Briefing = "The river is frozen at the minute before midnight, and something in a clock coat is listening. Every 16 seconds the clock chimes; only the chime hides the sound of a tear.",
                Danger = "It kills at a touch. A tear opened off the chime draws it straight to you: seal it and wait for the next chime.",
            };
            Func<bool> revealed = () => Verified(id) || (l.Trace != null && l.Trace.Revealed);
            g.Steps.Add(new GuideStep
            {
                Text = "Open your passport  " + Key(Intent.Passport) + "  and aim at the seam on the brick wall by the checkpoint",
                Hint = "The seam is on the wall to the left of the promenade, near the checkpoint kiosk. Any destination will do.",
                Done = () => revealed() || (PassportOpen && Game.Tears != null && Game.Tears.Target == l.CheckpointSeam),
                Target = () => l.CheckpointSeam != null ? l.CheckpointSeam.Center : (Vector3?)null,
                Label = "Checkpoint seam",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Wait for the chime (the bell glows just before), then open the tear  " + Key(Intent.Tear) + "  while it rings",
                Hint = "Keep aiming at the seam. The meter fills toward the next chime; start holding " + Key(Intent.Tear) + " as the bell rings.",
                Done = revealed,
                Target = () => l.CheckpointSeam != null ? l.CheckpointSeam.Center : (Vector3?)null,
                Progress = () => enc == null ? -1f : enc.Chiming ? 1f : Mathf.Clamp01(1f - enc.SecondsToChime / LondonCell.ChimeInterval),
                LabelNow = () => enc == null ? "Seam" : enc.Chiming ? "NOW" : "Chime in " + Mathf.CeilToInt(enc.SecondsToChime) + " s",
            });
            g.Steps.Add(TraceStep(id, () => l.Trace, "on the promenade"));
            g.Steps.Add(StampStep(id, "the figure in the clock coat"));
            g.Steps.Add(TravelStep(l));
            g.Alert = () => PassengerAlert(7f, "It is coming to the sound. Seal the tear and wait for the chime.");
            return g;
        }

        // ------------------------------------------------------------------ Morocco: the corrupted shop
        static PlaceGuide Morocco(ShopCell s)
        {
            const CountryId id = CountryId.Morocco;
            var enc = s.Encounter as MoroccoEncounter;
            var g = new PlaceGuide
            {
                Title = "Morocco · The Shop That Remembers",
                Briefing = "The optician's shop, as it was the day you bought the glasses. Three things have left the counter; put them back and the shop gives up its secret.",
                Danger = "The optician stalks the aisles once you pick something up and kills at a touch. The lamplight around the counter keeps it away.",
            };
            string[] names = { "postcard", "brass tray", "camera" };
            Func<int> placed = () =>
            {
                if (enc == null || enc.Marks == null) return 0;
                int n = 0;
                foreach (var m in enc.Marks) if (m != null && m.Current != null) n++;
                return n;
            };
            Func<bool> drawer = () => Verified(id) || (enc != null && enc.DrawerOpen);
            Func<int> nextItem = () =>
            {
                if (enc == null || enc.Marks == null || enc.Items == null) return -1;
                for (int i = 0; i < enc.Marks.Count && i < enc.Items.Count; i++)
                    if (enc.Marks[i] != null && enc.Marks[i].Current == null) return i;
                return -1;
            };
            g.Steps.Add(new GuideStep
            {
                Text = "Return the three things to their outlines on the counter: the postcard, the brass tray and the camera  " + Grab,
                Hint = "They are down the aisles: the postcard on a rack to the west, the tray on the floor to the east, the camera on a high shelf at the far east end. Carry each to its outline and press " + Use + ".",
                Done = drawer,
                Target = () =>
                {
                    int i = nextItem();
                    if (i < 0) return null;
                    var item = enc.Items[i];
                    return Holding(item) ? enc.Marks[i].transform.position : Pos(item);
                },
                LabelNow = () =>
                {
                    int i = nextItem();
                    if (i < 0) return "";
                    return Holding(enc.Items[i]) ? names[i] + "'s outline" : names[i];
                },
                Progress = () => placed() / 3f,
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Take the calibration insert from the drawer  " + Grab + "  and fit it inside the glasses case on the counter  " + Use,
                Done = () => Verified(id) || (enc != null && enc.NotesRead) || (Game.Campaign != null && Game.Campaign.HasFlag(StoryFlags.OpticianNotesRead)),
                Target = () => enc == null ? null : enc.Insert != null && !Holding(enc.Insert) ? Pos(enc.Insert) : Pos(enc.CaseSocket),
                LabelNow = () => enc != null && enc.Insert != null && Holding(enc.Insert) ? "Glasses case" : "Insert",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Listen to the optician's notes. The trace appears on the counter when they end",
                Done = () => Verified(id) || (s.Trace != null && s.Trace.Revealed),
                Target = () => s.CounterTop != null ? s.CounterTop.position : (Vector3?)null,
                Label = "Counter",
            });
            g.Steps.Add(TraceStep(id, () => s.Trace, "on the counter"));
            g.Steps.Add(StampStep(id, "the optician"));
            g.Steps.Add(TravelStep(s));
            g.Alert = () => PassengerAlert(7f, "The optician is close. Get back into the lamplight by the counter.");
            return g;
        }

        // ------------------------------------------------------------------ Germany: Berlin, the finale
        static PlaceGuide Berlin(BerlinCell b)
        {
            const CountryId id = CountryId.Germany;
            var enc = b.Encounter as BerlinEncounter;
            var g = new PlaceGuide
            {
                Title = "Germany · The Departure That Never Happened",
                Briefing = "The way home is here, and so is a forged one. Validate every seal, expose the forgery in real daylight, cancel the forged HOME stamp, then open the genuine way home.",
                Danger = "Once the seals are validated it guards the way home. Noise draws it; a home tear left open pulls it to the threshold.",
            };
            g.Steps.Add(new GuideStep
            {
                Text = "Validate the eleven seals at the registration plinth  " + Use + "  (you need the other ten seals and Morocco's calibration insert)",
                Hint = "If the plinth says something is missing, go back for it: open your passport and tear a way to the countries you haven't verified.",
                Done = () => Verified(id),
                Target = () => Pos(b.Plinth),
                Label = "Plinth",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Draw it away with the noisemaker, then open a tear to <b>Egypt</b> at the seam facing the doors: sunlight exposes the forgery",
                Hint = "Stand east of the seam facing the doors, choose Egypt in your passport and widen the tear until the light falls on the middle door.",
                Done = () => enc != null && enc.Exposed,
                Target = () => b.LightSeam != null ? b.LightSeam.Center : (Vector3?)null,
                Label = "Seam facing the doors",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Take the VOID stamp  " + Grab + "  and use it (" + Activate + ") to cancel the forged HOME seal",
                Done = () => Game.Campaign != null && Game.Campaign.HasFlag(StoryFlags.ForgedCancelled),
                Target = () => Holding(b.VoidStamp) ? (Vector3?)null : Pos(b.VoidStamp),
                Label = "VOID stamp",
            });
            g.Steps.Add(new GuideStep
            {
                Text = "Seal the sunlight tear (hold  " + Key(Intent.Seal) + "), then open the real way home on the <b>blank wall</b>, widen it to a doorway and step through",
                Hint = "Not the door that looks like home. The genuine seam is on the blank wall; be quick, it walks to an open home tear.",
                Done = () => false,
                Target = () => b.HomeSeam != null ? b.HomeSeam.Center : (Vector3?)null,
                Label = "The way home",
            });
            g.Alert = () => FirstOf(TearAlert, () => PassengerAlert(7f, "It guards the way home. Use the noise to pull it away."));
            return g;
        }
    }
}
