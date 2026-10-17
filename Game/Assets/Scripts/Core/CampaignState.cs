using System;
using System.Collections.Generic;
using System.Linq;

namespace TwelfthStamp
{
    /// <summary>
    /// Persistent campaign progress. Visited, verified, resources and narrative flags are tracked separately so that
    /// visiting a country early, leaving a puzzle unfinished or revisiting never loses or fakes progress.
    /// Pure C# (no Unity objects) so it is unit-testable.
    /// </summary>
    [Serializable]
    public class CampaignState
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public List<string> visited = new List<string>();
        public List<string> verified = new List<string>();
        public List<string> resources = new List<string>();
        public List<string> flags = new List<string>();
        public string checkpointCell = CellId.ShopNormal.ToString();
        public float vitality = 1f;

        // Replay statistics.
        public float playTimeSeconds;
        public int forcedReturns;
        public int falseExits;
        public int failures;
        public int tearsOpened;

        public static CampaignState NewGame() => new CampaignState();

        // ------------------------------------------------------------- verification
        public bool IsVerified(CountryId id) => verified.Contains(CountryDatabase.Get(id).StableId);

        public bool IsVisited(CountryId id) => visited.Contains(CountryDatabase.Get(id).StableId);

        public int VerifiedCount => CountryDatabase.All.Count(c => verified.Contains(c.StableId));

        public bool AllElevenVerified => VerifiedCount == CountryDatabase.CountryCount;

        /// <summary>Returns true if this call newly verified the seal.</summary>
        public bool Verify(CountryId id)
        {
            var def = CountryDatabase.Get(id);
            MarkVisited(id);
            if (verified.Contains(def.StableId)) return false;
            verified.Add(def.StableId);
            if (def.Provides.HasValue) AddResource(def.Provides.Value);
            return true;
        }

        public bool MarkVisited(CountryId id)
        {
            var sid = CountryDatabase.Get(id).StableId;
            if (visited.Contains(sid)) return false;
            visited.Add(sid);
            return true;
        }

        // ------------------------------------------------------------- resources / flags
        public bool HasResource(ResourceId r) => resources.Contains(r.ToString());

        public void AddResource(ResourceId r)
        {
            if (!resources.Contains(r.ToString())) resources.Add(r.ToString());
        }

        public bool HasFlag(string f) => flags.Contains(f);

        public bool SetFlag(string f)
        {
            if (flags.Contains(f)) return false;
            flags.Add(f);
            return true;
        }

        // ------------------------------------------------------------- availability rules
        /// <summary>Destinations become available once the apartment tutorial has revealed the seam.</summary>
        public bool IsDestinationAvailable(CountryId id)
        {
            if (!HasFlag(StoryFlags.SeamRevealed)) return false;
            if (!HasFlag(StoryFlags.TutorialDone))
                return id == CountryId.Egypt; // the tutorial teaches one tear, to Egypt
            return true;
        }

        /// <summary>Whether a resource can actually flow through a tear to the given destination.</summary>
        public bool ResourceFlowsFrom(CountryId destination, ResourceId r)
        {
            if (r == ResourceId.Sunlight) return destination == CountryId.Egypt && HasResource(ResourceId.Sunlight);
            return false;
        }

        /// <summary>The counterfeit HOME seal is revealed after the first verified foreign country.</summary>
        public bool ShouldRevealCounterfeit => VerifiedCount >= 3 || HasFlag(StoryFlags.CounterfeitRevealed);

        /// <summary>The genuine home tear can only open with all eleven seals, the insert, and the forged stamp cancelled.</summary>
        public bool CanOpenGenuineHome =>
            AllElevenVerified && HasResource(ResourceId.CalibrationInsert) && HasFlag(StoryFlags.ForgedCancelled);

        /// <summary>Germany's own trace can only validate once the other ten seals are verified and the insert is held.</summary>
        public bool CanValidateFinalPattern =>
            CountryDatabase.All.Where(c => c.Id != CountryId.Germany).All(c => verified.Contains(c.StableId))
            && HasResource(ResourceId.CalibrationInsert);

        public CountryDef NextRecommended()
        {
            return CountryDatabase.InRecommendedOrder.FirstOrDefault(c => !verified.Contains(c.StableId));
        }

        // ------------------------------------------------------------- validation (for loaded saves)
        public bool Sanitize()
        {
            if (version < 1 || version > CurrentVersion) return false;
            visited = (visited ?? new List<string>()).Where(s => CountryDatabase.ByStableId(s) != null).Distinct().ToList();
            verified = (verified ?? new List<string>()).Where(s => CountryDatabase.ByStableId(s) != null).Distinct().ToList();
            resources = (resources ?? new List<string>()).Where(s => Enum.TryParse(s, out ResourceId _)).Distinct().ToList();
            flags = (flags ?? new List<string>()).Distinct().ToList();
            if (!Enum.TryParse(checkpointCell ?? "", out CellId _)) checkpointCell = CellId.ShopNormal.ToString();
            if (float.IsNaN(vitality) || vitality <= 0f || vitality > 1f) vitality = 1f;
            if (float.IsNaN(playTimeSeconds) || playTimeSeconds < 0) playTimeSeconds = 0;
            return true;
        }

        public CellId CheckpointCell
        {
            get => Enum.TryParse(checkpointCell, out CellId c) ? c : CellId.ShopNormal;
            set => checkpointCell = value.ToString();
        }

        public CampaignState Clone()
        {
            var c = (CampaignState)MemberwiseClone();
            c.visited = new List<string>(visited);
            c.verified = new List<string>(verified);
            c.resources = new List<string>(resources);
            c.flags = new List<string>(flags);
            return c;
        }
    }
}
