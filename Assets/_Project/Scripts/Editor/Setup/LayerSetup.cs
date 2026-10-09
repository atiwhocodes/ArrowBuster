using UnityEditor;
using UnityEngine;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// Applies the D-048 gameplay layer names (6–16) and the layer collision matrix from <see cref="PhysicsLayers"/>.
    /// Menu: Arrow Buster ▸ Setup ▸ Layers. Safe to run more than once.
    /// </summary>
    public static class LayerSetup
    {
        private const string TagManagerPath = "ProjectSettings/TagManager.asset";
        private const int LayerCount = 32;

        [MenuItem("Arrow Buster/Setup/Layers", priority = 4)]
        public static void Apply()
        {
            if (!ApplyLayerNames()) return;
            int changedPairs = ApplyCollisionMatrix();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Arrow Buster] Layers 6–16 applied (D-048); {changedPairs} collision pair(s) changed.");
        }

        private static bool ApplyLayerNames()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(TagManagerPath)[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            // Refuse to overwrite a slot that someone already uses for something else.
            for (int layer = PhysicsLayers.FirstGameplayLayer; layer <= PhysicsLayers.LastGameplayLayer; layer++)
            {
                string current = layers.GetArrayElementAtIndex(layer).stringValue;
                string wanted = PhysicsLayers.NameOf(layer);
                if (!string.IsNullOrEmpty(current) && current != wanted)
                {
                    Debug.LogError($"[Arrow Buster] Layer {layer} is named '{current}', expected '{wanted}'. Nothing was changed.");
                    return false;
                }
            }

            for (int layer = PhysicsLayers.FirstGameplayLayer; layer <= PhysicsLayers.LastGameplayLayer; layer++)
                layers.GetArrayElementAtIndex(layer).stringValue = PhysicsLayers.NameOf(layer);

            tagManager.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static int ApplyCollisionMatrix()
        {
            int changed = 0;
            for (int a = 0; a < LayerCount; a++)
            {
                for (int b = a; b < LayerCount; b++)
                {
                    // Pairs of built-in/unused layers keep whatever the project already has.
                    if (!PhysicsLayers.IsGameplayLayer(a) && !PhysicsLayers.IsGameplayLayer(b)) continue;

                    bool ignore = !PhysicsLayers.ShouldCollide(a, b);
                    if (Physics.GetIgnoreLayerCollision(a, b) == ignore) continue;

                    Physics.IgnoreLayerCollision(a, b, ignore);
                    changed++;
                }
            }
            return changed;
        }
    }
}
