using System;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>Persisted player settings. Every field here is wired to a working option.</summary>
    [Serializable]
    public class GameSettings
    {
        public const int CurrentVersion = 1;
        public int version = CurrentVersion;

        // Audio buses (0..1)
        public float master = 1f;
        public float music = 0.6f;
        public float ambience = 0.8f;
        public float sfx = 0.9f;
        public float voice = 1f;
        public float ui = 0.7f;
        public bool reducedDynamicRange = false;

        // Subtitles
        public bool subtitles = true;
        public int subtitleSize = 1;          // 0 small, 1 medium, 2 large
        public bool subtitleBackground = true;

        // Display
        public float brightness = 0f;         // -1..1 exposure offset (EV * 0.75)
        public int graphicsPreset = 2;        // 0 low, 1 medium, 2 high, 3 ultra
        public bool fullscreen = true;
        public int resolutionIndex = -1;      // -1 = native
        public bool reducedEffects = false;   // no chromatic aberration / grain / lens contamination overlay
        public bool showVitalityBar = true;

        // Desktop controls
        public float mouseSensitivity = 1f;
        public bool invertY = false;

        // VR comfort
        public bool smoothLocomotion = false; // teleport by default
        public bool smoothTurn = false;       // snap turn by default
        public float snapAngle = 45f;
        public bool leftHanded = false;
        public bool seated = false;
        public float seatedHeight = 0.45f;    // added when seated
        public float haptics = 1f;

        // Horror
        public int scareIntensity = 1;        // 0 gentle, 1 standard

        public bool comfortSetupDone = false;

        public GameSettings Sanitized()
        {
            master = Mathf.Clamp01(master); music = Mathf.Clamp01(music); ambience = Mathf.Clamp01(ambience);
            sfx = Mathf.Clamp01(sfx); voice = Mathf.Clamp01(voice); ui = Mathf.Clamp01(ui);
            subtitleSize = Mathf.Clamp(subtitleSize, 0, 2);
            brightness = Mathf.Clamp(brightness, -1f, 1f);
            graphicsPreset = Mathf.Clamp(graphicsPreset, 0, 3);
            mouseSensitivity = Mathf.Clamp(mouseSensitivity, 0.2f, 3f);
            snapAngle = Mathf.Clamp(snapAngle, 15f, 90f);
            seatedHeight = Mathf.Clamp(seatedHeight, 0f, 0.8f);
            haptics = Mathf.Clamp01(haptics);
            scareIntensity = Mathf.Clamp(scareIntensity, 0, 1);
            if (version < 1 || version > CurrentVersion) version = CurrentVersion;
            return this;
        }

        public GameSettings Clone() => (GameSettings)MemberwiseClone();
    }
}
