using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ArrowBuster.Tests
{
    /// <summary>Confirms D-006/D-048 settings are live at runtime (in Play mode, not only in the project files).</summary>
    public class PhysicsRuntimeSmokeTests
    {
        [UnityTest]
        public IEnumerator FixedStep_And_Layers_AreActiveAtRuntime()
        {
            yield return new WaitForFixedUpdate();

            Assert.AreEqual(PhysicsSettingsSpec.FixedDeltaTime, Time.fixedDeltaTime, 1e-5f);
            Assert.AreEqual(PhysicsLayers.Debris, LayerMask.NameToLayer("Debris"));
            Assert.IsTrue(Physics.GetIgnoreLayerCollision(PhysicsLayers.Debris, PhysicsLayers.Structure),
                "Debris must never touch gameplay bodies (D-008).");
        }

        [UnityTest]
        public IEnumerator DebrisBody_FallsThroughStructure_ButLandsOnEnvironment()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.layer = PhysicsLayers.Environment;
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(10f, 1f, 2f);

            var structure = GameObject.CreatePrimitive(PrimitiveType.Cube);
            structure.layer = PhysicsLayers.Structure;
            structure.transform.position = new Vector3(0f, 1f, 0f);
            structure.transform.localScale = new Vector3(2f, 0.2f, 2f);

            var debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debris.layer = PhysicsLayers.Debris;
            debris.transform.position = new Vector3(0f, 3f, 0f);
            debris.transform.localScale = Vector3.one * 0.2f;
            debris.AddComponent<Rigidbody>();

            for (int i = 0; i < 120; i++) yield return new WaitForFixedUpdate();

            float y = debris.transform.position.y;
            Object.Destroy(ground);
            Object.Destroy(structure);
            Object.Destroy(debris);

            Assert.Less(y, 0.5f, "Debris should pass through the Structure plank.");
            Assert.Greater(y, -0.1f, "Debris should rest on the Environment ground.");
        }
    }
}
