using MetroidJumper.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MetroidJumper.EditorTools
{
    public static class SetupPlayerMovement
    {
        [MenuItem("Tools/Senna/Setup Player Movement (2D)")]
        private static void Setup()
        {
            GameObject player = GameObject.Find("player");
            GameObject floor = GameObject.Find("Square");

            if (player == null || floor == null)
            {
                Debug.LogError("SetupPlayerMovement: could not find 'player' and/or 'Square' in the open scene.");
                return;
            }

            player.layer = EnsureLayerExists("Player");

            CapsuleCollider2D capsule = ConvertPlayerPhysics(player);
            ConvertFloorPhysics(floor);
            WireGroundCheck(player, capsule);

            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(floor);
            EditorSceneManager.MarkSceneDirty(player.scene);

            Debug.Log("SetupPlayerMovement: player + floor converted to 2D physics, PlayerMovement attached and wired. Player moved to its own 'Player' layer so ground-check can't overlap its own collider.");
        }

        private static int EnsureLayerExists(string layerName)
        {
            var tagManagerObjects = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            var tagManager = new SerializedObject(tagManagerObjects[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName) return i;
            }

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(slot.stringValue))
                {
                    slot.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return i;
                }
            }

            Debug.LogError($"SetupPlayerMovement: no free layer slot found for '{layerName}'.");
            return 0;
        }

        private static CapsuleCollider2D ConvertPlayerPhysics(GameObject player)
        {
            Rigidbody oldRigidbody = player.GetComponent<Rigidbody>();
            if (oldRigidbody != null) Object.DestroyImmediate(oldRigidbody);

            Collider old3DCollider = player.GetComponent<Collider>();
            if (old3DCollider != null) Object.DestroyImmediate(old3DCollider);

            Rigidbody2D rb2D = player.GetComponent<Rigidbody2D>();
            if (rb2D == null) rb2D = player.AddComponent<Rigidbody2D>();
            rb2D.freezeRotation = true;
            rb2D.interpolation = RigidbodyInterpolation2D.Interpolate;

            CapsuleCollider2D capsule = player.GetComponent<CapsuleCollider2D>();
            if (capsule == null) capsule = player.AddComponent<CapsuleCollider2D>();

            SpriteRenderer sprite = player.GetComponent<SpriteRenderer>();
            if (sprite != null && sprite.sprite != null)
            {
                capsule.size = sprite.sprite.bounds.size;
                capsule.offset = sprite.sprite.bounds.center;
            }

            capsule.sharedMaterial = EnsureFrictionlessMaterial();

            if (player.GetComponent<PlayerMovement>() == null) player.AddComponent<PlayerMovement>();

            return capsule;
        }

        private static PhysicsMaterial2D EnsureFrictionlessMaterial()
        {
            const string path = "Assets/Physics/PlayerNoFriction.physicsMaterial2D";
            PhysicsMaterial2D material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (material != null) return material;

            if (!AssetDatabase.IsValidFolder("Assets/Physics"))
            {
                AssetDatabase.CreateFolder("Assets", "Physics");
            }

            material = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ConvertFloorPhysics(GameObject floor)
        {
            Collider old3DCollider = floor.GetComponent<Collider>();
            if (old3DCollider != null) Object.DestroyImmediate(old3DCollider);

            BoxCollider2D box2D = floor.GetComponent<BoxCollider2D>();
            if (box2D == null) box2D = floor.AddComponent<BoxCollider2D>();

            SpriteRenderer sprite = floor.GetComponent<SpriteRenderer>();
            if (sprite != null && sprite.sprite != null)
            {
                box2D.size = sprite.sprite.bounds.size;
                box2D.offset = sprite.sprite.bounds.center;
            }
        }

        private static void WireGroundCheck(GameObject player, CapsuleCollider2D capsule)
        {
            Transform groundCheck = player.transform.Find("GroundCheck");
            if (groundCheck == null)
            {
                var groundCheckObject = new GameObject("GroundCheck");
                groundCheck = groundCheckObject.transform;
                groundCheck.SetParent(player.transform, false);
            }

            float bottomY = capsule.offset.y - capsule.size.y * 0.5f;
            groundCheck.localPosition = new Vector3(capsule.offset.x, bottomY, 0f);

            var movement = player.GetComponent<PlayerMovement>();
            var serializedMovement = new SerializedObject(movement);

            serializedMovement.FindProperty("groundCheck").objectReferenceValue = groundCheck;

            SerializedProperty groundLayerProperty = serializedMovement.FindProperty("groundLayer");
            if (groundLayerProperty.intValue == 0)
            {
                groundLayerProperty.intValue = 1 << LayerMask.NameToLayer("Default");
            }

            serializedMovement.ApplyModifiedProperties();
        }
    }
}
