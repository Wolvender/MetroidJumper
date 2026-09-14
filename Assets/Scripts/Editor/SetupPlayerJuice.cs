using MetroidJumper.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MetroidJumper.EditorTools
{
    public static class SetupPlayerJuice
    {
        [MenuItem("Tools/Senna/Setup Player Juice")]
        private static void Setup()
        {
            GameObject player = GameObject.Find("player");
            if (player == null)
            {
                Debug.LogError("SetupPlayerJuice: could not find 'player' in the open scene.");
                return;
            }

            if (player.GetComponent<PlayerJuice>() == null) player.AddComponent<PlayerJuice>();

            ParticleSystem dustParticles = CreateDustParticles(player);
            WireDustParticles(player, dustParticles);

            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(player.scene);

            Debug.Log("SetupPlayerJuice: PlayerJuice + dust particle system attached to player.");
        }

        private static ParticleSystem CreateDustParticles(GameObject player)
        {
            Transform existing = player.transform.Find("DustParticles");
            GameObject dustObject = existing != null ? existing.gameObject : new GameObject("DustParticles");
            if (existing == null) dustObject.transform.SetParent(player.transform, false);

            ParticleSystem particles = dustObject.GetComponent<ParticleSystem>();
            if (particles == null) particles = dustObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 1.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            main.startColor = new Color(0.6f, 0.55f, 0.45f, 0.8f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            return particles;
        }

        private static void WireDustParticles(GameObject player, ParticleSystem dustParticles)
        {
            var juice = player.GetComponent<PlayerJuice>();
            var serializedJuice = new SerializedObject(juice);
            serializedJuice.FindProperty("dustParticles").objectReferenceValue = dustParticles;
            serializedJuice.ApplyModifiedProperties();
        }
    }
}
