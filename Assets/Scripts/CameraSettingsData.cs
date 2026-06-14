// CameraSettingsData.cs
// ScriptableObject that stores a complete Cinemachine camera profile.
//
// DESIGN RATIONALE:
// Camera feel is a tuning problem, not a code problem. Storing these values in
// an asset means a designer can create multiple profiles (e.g., "Corridor",
// "Wide Room", "Vent Crawl") and swap them at runtime without rebuilding.
//
// The default values below match the suggested Phase 1 settings from the brief.
// Adjust them in the Inspector on the asset — never hardcode them in C# logic.
//
// CREATE:  Right-click in Project → SciFiGame → Camera → Camera Settings

using UnityEngine;
using Unity.Cinemachine;

namespace SciFiGame.Camera
{
    [CreateAssetMenu(fileName = "CameraSettings_Default",
                     menuName  = "Game/Camera/Camera Settings")]
    public class CameraSettingsData : ScriptableObject
    {
        [Header("Third Person Follow")]
        [Tooltip("Lateral/vertical/depth offset of the shoulder pivot from the Follow target.")]
        public Vector3 ShoulderOffset    = new Vector3(0.45f, 0.2f, -0.15f);

        [Tooltip("Controls how high the arm pivot sits. Affects vertical framing of the player.")]
        public float   VerticalArmLength = 0.25f;

        [Tooltip("Distance from the pivot to the lens. Collision may shorten this dynamically.")]
        public float   CameraDistance    = 1.6f;

        [Tooltip("0 = left shoulder, 1 = right shoulder.")]
        [Range(0f, 1f)]
        public float   CameraSide        = 1f;

        [Header("Lens")]
        [Tooltip("Vertical field of view in degrees.")]
        [Range(30f, 110f)]
        public float   FieldOfView       = 65f;

        [Header("Noise")]
        [Tooltip("Cinemachine noise profile asset (Basic Multi Channel Perlin).")]
        public NoiseSettings NoiseProfile;

        [Range(0f, 2f)]
        public float   NoiseAmplitude    = 0.15f;

        [Range(0f, 2f)]
        public float   NoiseFrequency    = 0.5f;
    }
}
