using MetroidJumper.Cameras;
using MetroidJumper.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MetroidJumper.EditorTools
{
    public static class SetupCameraFollow
    {
        [MenuItem("Tools/Senna/Setup Camera Follow")]
        private static void Setup()
        {
            GameObject player = GameObject.Find("player");
            Camera mainCamera = Camera.main;

            if (player == null || mainCamera == null)
            {
                Debug.LogError("SetupCameraFollow: could not find 'player' and/or a Main Camera in the open scene.");
                return;
            }

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement == null)
            {
                Debug.LogError("SetupCameraFollow: player has no PlayerMovement component. Run Setup Player Movement (2D) first.");
                return;
            }

            GameObject cameraObject = mainCamera.gameObject;
            CameraFollow follow = cameraObject.GetComponent<CameraFollow>();
            if (follow == null) follow = cameraObject.AddComponent<CameraFollow>();

            var serializedFollow = new SerializedObject(follow);
            serializedFollow.FindProperty("target").objectReferenceValue = player.transform;
            serializedFollow.FindProperty("playerMovement").objectReferenceValue = movement;
            serializedFollow.ApplyModifiedProperties();

            EditorUtility.SetDirty(cameraObject);
            EditorSceneManager.MarkSceneDirty(cameraObject.scene);

            Debug.Log("SetupCameraFollow: CameraFollow attached to Main Camera and wired to the player.");
        }
    }
}
