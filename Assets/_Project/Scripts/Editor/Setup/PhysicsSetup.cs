using UnityEditor;
using UnityEngine;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// Applies the D-006 physics and time settings from <see cref="PhysicsSettingsSpec"/> to the project.
    /// Menu: Arrow Buster ▸ Setup ▸ Physics. Safe to run more than once.
    /// </summary>
    public static class PhysicsSetup
    {
        private const string TimeManagerPath = "ProjectSettings/TimeManager.asset";
        private const string DynamicsManagerPath = "ProjectSettings/DynamicsManager.asset";

        [MenuItem("Arrow Buster/Setup/Physics", priority = 5)]
        public static void Apply()
        {
            var time = Load(TimeManagerPath);
            SetFixedTimestep(time, PhysicsSettingsSpec.FixedDeltaTime);
            SetFloat(time, "Maximum Allowed Timestep", PhysicsSettingsSpec.MaximumDeltaTime);
            time.ApplyModifiedPropertiesWithoutUndo();
            Time.fixedDeltaTime = PhysicsSettingsSpec.FixedDeltaTime;
            Time.maximumDeltaTime = PhysicsSettingsSpec.MaximumDeltaTime;

            var dynamics = Load(DynamicsManagerPath);
            SetInt(dynamics, "m_DefaultSolverIterations", PhysicsSettingsSpec.SolverIterations);
            SetInt(dynamics, "m_DefaultSolverVelocityIterations", PhysicsSettingsSpec.SolverVelocityIterations);
            SetBool(dynamics, "m_EnableEnhancedDeterminism", PhysicsSettingsSpec.EnhancedDeterminism);
            SetFloat(dynamics, "m_BounceThreshold", PhysicsSettingsSpec.BounceThreshold);
            SetFloat(dynamics, "m_SleepThreshold", PhysicsSettingsSpec.SleepThreshold);
            SetFloat(dynamics, "m_DefaultMaxAngularSpeed", PhysicsSettingsSpec.DefaultMaxAngularSpeed);
            SetFloat(dynamics, "m_DefaultMaxDepenetrationVelocity", PhysicsSettingsSpec.DefaultMaxDepenetrationVelocity);
            SetBool(dynamics, "m_AutoSyncTransforms", PhysicsSettingsSpec.AutoSyncTransforms);
            dynamics.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            Debug.Log("[Arrow Buster] Physics settings applied (D-006): fixed Δt 1/60, max Δt 0.1, solver 8/2, enhanced determinism on.");
        }

        private static SerializedObject Load(string path) =>
            new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(path)[0]);

        private static SerializedProperty Find(SerializedObject settings, string name)
        {
            SerializedProperty property = settings.FindProperty(name);
            if (property == null)
                Debug.LogError($"[Arrow Buster] '{name}' not found in {settings.targetObject.name}; the Unity settings format may have changed.");
            return property;
        }

        /// <summary>
        /// Unity 6 stores the fixed timestep as a rational (<c>m_Count</c> ticks at <c>m_Rate</c> ticks per second);
        /// older versions store a float. Handle both.
        /// </summary>
        private static void SetFixedTimestep(SerializedObject time, float seconds)
        {
            SerializedProperty property = Find(time, "Fixed Timestep");
            if (property == null) return;
            if (property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = seconds;
                return;
            }

            SerializedProperty count = property.FindPropertyRelative("m_Count");
            SerializedProperty numerator = property.FindPropertyRelative("m_Rate.m_Numerator");
            SerializedProperty denominator = property.FindPropertyRelative("m_Rate.m_Denominator");
            if (count == null || numerator == null || denominator == null || denominator.longValue == 0)
            {
                Debug.LogError("[Arrow Buster] Unrecognised 'Fixed Timestep' format; set it manually in Project Settings ▸ Time.");
                return;
            }
            double ticksPerSecond = (double)numerator.longValue / denominator.longValue;
            count.longValue = (long)System.Math.Round(seconds * ticksPerSecond);
        }

        private static void SetFloat(SerializedObject settings, string name, float value)
        {
            SerializedProperty property = Find(settings, name);
            if (property != null) property.floatValue = value;
        }

        private static void SetInt(SerializedObject settings, string name, int value)
        {
            SerializedProperty property = Find(settings, name);
            if (property != null) property.intValue = value;
        }

        private static void SetBool(SerializedObject settings, string name, bool value)
        {
            SerializedProperty property = Find(settings, name);
            if (property != null) property.boolValue = value;
        }
    }
}
