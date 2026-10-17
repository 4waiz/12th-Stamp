using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>Data-driven definition of one genuine destination.</summary>
    public sealed class CountryDef
    {
        public CountryId Id;
        public string StableId;          // save-file id, never renamed
        public string DisplayName;       // passport tab label
        public string Place;             // city / site
        public string EncounterTitle;    // chapter card
        public CellId Cell;
        public string StampTexture;      // Resources/Art/<name>
        public int RecommendedOrder;     // 1-based route suggestion
        public string Objective;         // short on-screen objective
        public ResourceId? Provides;     // resource unlocked by verifying here
        public ResourceId[] Needs = new ResourceId[0]; // resources the local puzzle needs
        public Color Tint;               // tear rim / tab accent
    }

    public static class CountryDatabase
    {
        public const int CountryCount = 11;
        public const string CounterfeitStampTexture = "stamp_home_forged";
        public const string CounterfeitLabel = "HOME — ENTRY GRANTED";

        static readonly CountryDef[] s_All =
        {
            new CountryDef { Id = CountryId.UAE, StableId = "uae", DisplayName = "United Arab Emirates", Place = "Abu Dhabi", EncounterTitle = "The Apartment That Kept You",
                Cell = CellId.ApartmentCorrupt, StampTexture = "stamp_uae", RecommendedOrder = 1,
                Objective = "Find the one detail in the apartment the copy got right, and verify the homecoming seal.", Tint = new Color(0.86f, 0.72f, 0.52f) },
            new CountryDef { Id = CountryId.Egypt, StableId = "egypt", DisplayName = "Egypt", Place = "Giza exhibition court", EncounterTitle = "The Sun Without a Shadow",
                Cell = CellId.Egypt, StampTexture = "stamp_egypt", RecommendedOrder = 2, Provides = ResourceId.Sunlight,
                Objective = "Turn the reflector until sunlight burns away the shadow that points at the sun.", Tint = new Color(1f, 0.82f, 0.48f) },
            new CountryDef { Id = CountryId.France, StableId = "france", DisplayName = "France", Place = "Paris riverside checkpoint", EncounterTitle = "The Street That Folds",
                Cell = CellId.Paris, StampTexture = "stamp_france", RecommendedOrder = 3, Needs = new[] { ResourceId.Sunlight },
                Objective = "Borrow Egypt's sunlight to drive it off the verification desk, then seal and verify.", Tint = new Color(0.55f, 0.66f, 0.78f) },
            new CountryDef { Id = CountryId.USA, StableId = "usa", DisplayName = "United States", Place = "New York harbour terminal", EncounterTitle = "The Last Arrival",
                Cell = CellId.NewYork, StampTexture = "stamp_usa", RecommendedOrder = 4, Provides = ResourceId.Noisemaker,
                Objective = "Power the security booth, then use the noisemaker to draw it away from the arrival gate.", Tint = new Color(0.95f, 0.66f, 0.38f) },
            new CountryDef { Id = CountryId.Japan, StableId = "japan", DisplayName = "Japan", Place = "Lantern street", EncounterTitle = "The Empty Procession",
                Cell = CellId.Japan, StampTexture = "stamp_japan", RecommendedOrder = 5,
                Objective = "Move the lantern until only the genuine route mark still casts a shadow.", Tint = new Color(0.92f, 0.45f, 0.32f) },
            new CountryDef { Id = CountryId.SaudiArabia, StableId = "saudi", DisplayName = "Saudi Arabia", Place = "Riyadh festival plaza", EncounterTitle = "The False Horizon",
                Cell = CellId.Riyadh, StampTexture = "stamp_saudi", RecommendedOrder = 6,
                Objective = "Rotate the optical panel to match your passport's registration mark and find the honest arch.", Tint = new Color(0.9f, 0.7f, 0.5f) },
            new CountryDef { Id = CountryId.India, StableId = "india", DisplayName = "India", Place = "Agra garden checkpoint", EncounterTitle = "The Reflection That Stayed",
                Cell = CellId.Agra, StampTexture = "stamp_india", RecommendedOrder = 7,
                Objective = "Set the lamp where its reflection reveals the genuine trace in the pool.", Tint = new Color(0.98f, 0.8f, 0.7f) },
            new CountryDef { Id = CountryId.Brazil, StableId = "brazil", DisplayName = "Brazil", Place = "Rio carnival avenue", EncounterTitle = "The Carnival Without Breathing",
                Cell = CellId.Rio, StampTexture = "stamp_brazil", RecommendedOrder = 8,
                Objective = "Stamp only on the drum beat so the sound is masked from the moving shadow.", Tint = new Color(0.62f, 0.85f, 0.58f) },
            new CountryDef { Id = CountryId.UnitedKingdom, StableId = "uk", DisplayName = "United Kingdom", Place = "London riverside", EncounterTitle = "The Minute Before Midnight",
                Cell = CellId.London, StampTexture = "stamp_uk", RecommendedOrder = 9,
                Objective = "Have your passport ready and stamp the trace while the clock chime masks the sound.", Tint = new Color(0.6f, 0.68f, 0.85f) },
            new CountryDef { Id = CountryId.Morocco, StableId = "morocco", DisplayName = "Morocco", Place = "Marrakech, the shop", EncounterTitle = "The Shop That Remembers",
                Cell = CellId.ShopCorrupt, StampTexture = "stamp_morocco", RecommendedOrder = 10, Provides = ResourceId.CalibrationInsert,
                Objective = "Put the counter back the way it was, recover the calibration insert, and read the optician's notes.", Tint = new Color(0.4f, 0.72f, 0.66f) },
            new CountryDef { Id = CountryId.Germany, StableId = "germany", DisplayName = "Germany", Place = "Berlin festival pavilion", EncounterTitle = "The Departure That Never Happened",
                Cell = CellId.Berlin, StampTexture = "stamp_germany", RecommendedOrder = 11,
                Needs = new[] { ResourceId.CalibrationInsert, ResourceId.Sunlight, ResourceId.Noisemaker },
                Objective = "Validate the eleven seals, cancel the forged home stamp, and find the real way home.", Tint = new Color(0.82f, 0.84f, 0.88f) },
        };

        /// <summary>Not a country: the genuine way home (Berlin finale). Never counted among the eleven.</summary>
        public static readonly CountryDef Home = new CountryDef
        {
            Id = CountryId.UAE, StableId = "home", DisplayName = "Home", Place = "Abu Dhabi, the real apartment",
            EncounterTitle = "Home", Cell = CellId.ApartmentNormal, StampTexture = "stamp_uae", RecommendedOrder = 99,
            Objective = "", Tint = new Color(1f, 0.8f, 0.55f)
        };

        public static IReadOnlyList<CountryDef> All => s_All;

        public static CountryDef Get(CountryId id) => s_All[(int)id];

        public static CountryDef ByStableId(string stableId) => s_All.FirstOrDefault(c => c.StableId == stableId);

        public static CountryDef ByCell(CellId cell) => s_All.FirstOrDefault(c => c.Cell == cell);

        public static IEnumerable<CountryDef> InRecommendedOrder => s_All.OrderBy(c => c.RecommendedOrder);

        public static string ResourceLabel(ResourceId r)
        {
            switch (r)
            {
                case ResourceId.Sunlight: return "Egyptian sunlight";
                case ResourceId.Noisemaker: return "Noisemaker";
                case ResourceId.CalibrationInsert: return "Calibration insert";
                case ResourceId.VoidStamp: return "VOID stamp";
                default: return r.ToString();
            }
        }
    }
}
