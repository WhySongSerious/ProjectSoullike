using System;
using ProjectSoullike;
using UnityEditor;
using UnityEngine;

namespace ProjectSoullike.Editor
{
    public static class SoullikeKeyboardControllerSetup
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Mechanic_Girl.prefab";
        private const string LogPrefix = "[Soullike Keyboard Setup]";

        [MenuItem("Tools/Project Soullike/Set Up Keyboard Player Controller")]
        public static void SetupFromMenu()
        {
            try
            {
                SetupPlayerPrefab();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ValidatePlayerPrefab();
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LogPrefix} Setup failed: {exception}");
                throw;
            }
        }

        private static void SetupPlayerPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (root == null)
            {
                throw new InvalidOperationException($"Could not load prefab at {PlayerPrefabPath}.");
            }

            try
            {
                Animator animator = root.GetComponentInChildren<Animator>(true);
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    throw new InvalidOperationException("The player prefab needs an Animator with a controller.");
                }

                if (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                {
                    throw new InvalidOperationException("The player prefab needs a valid Humanoid avatar.");
                }

                CharacterController characterController = root.GetComponent<CharacterController>();
                if (characterController == null)
                {
                    characterController = root.AddComponent<CharacterController>();
                }

                ConfigureCharacterController(root.transform, characterController);

                KeyboardPlayerController playerController = root.GetComponent<KeyboardPlayerController>();
                if (playerController == null)
                {
                    playerController = root.AddComponent<KeyboardPlayerController>();
                }

                EditorUtility.SetDirty(characterController);
                EditorUtility.SetDirty(playerController);
                EditorUtility.SetDirty(root);

                if (!PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath, out bool saved) || !saved)
                {
                    throw new InvalidOperationException($"Could not save prefab at {PlayerPrefabPath}.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureCharacterController(Transform root, CharacterController controller)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("The player prefab does not contain a renderer.");
            }

            Bounds localBounds = GetLocalRendererBounds(root, renderers);
            float height = Mathf.Max(localBounds.size.y, 1f);
            float horizontalExtent = Mathf.Min(localBounds.extents.x, localBounds.extents.z);
            float radius = Mathf.Clamp(horizontalExtent * 0.65f, 0.2f, height * 0.3f);

            controller.center = localBounds.center;
            controller.height = height;
            controller.radius = radius;
            controller.slopeLimit = 50f;
            controller.stepOffset = Mathf.Min(0.3f, height * 0.2f);
            controller.skinWidth = 0.05f;
            controller.minMoveDistance = 0f;
        }

        private static Bounds GetLocalRendererBounds(Transform root, Renderer[] renderers)
        {
            bool initialized = false;
            Bounds localBounds = default;

            foreach (Renderer renderer in renderers)
            {
                Bounds worldBounds = renderer.bounds;
                Vector3 min = worldBounds.min;
                Vector3 max = worldBounds.max;

                for (int x = 0; x <= 1; x++)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        for (int z = 0; z <= 1; z++)
                        {
                            Vector3 worldCorner = new Vector3(
                                x == 0 ? min.x : max.x,
                                y == 0 ? min.y : max.y,
                                z == 0 ? min.z : max.z);
                            Vector3 localCorner = root.InverseTransformPoint(worldCorner);

                            if (!initialized)
                            {
                                localBounds = new Bounds(localCorner, Vector3.zero);
                                initialized = true;
                            }
                            else
                            {
                                localBounds.Encapsulate(localCorner);
                            }
                        }
                    }
                }
            }

            return localBounds;
        }

        private static void ValidatePlayerPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Could not reload prefab at {PlayerPrefabPath}.");
            }

            CharacterController characterController = prefab.GetComponent<CharacterController>();
            KeyboardPlayerController playerController = prefab.GetComponent<KeyboardPlayerController>();
            Animator animator = prefab.GetComponentInChildren<Animator>(true);

            if (characterController == null || playerController == null)
            {
                throw new InvalidOperationException("Player prefab is missing its keyboard or character controller component.");
            }

            if (animator == null || animator.runtimeAnimatorController == null ||
                animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
            {
                throw new InvalidOperationException("Player Animator validation failed after saving the prefab.");
            }

            Debug.Log(
                $"{LogPrefix} COMPLETE Player={prefab.name}, " +
                $"Height={characterController.height:F2}, Radius={characterController.radius:F2}, " +
                $"AvatarValid={animator.avatar.isValid}, Humanoid={animator.avatar.isHuman}");
        }
    }
}
