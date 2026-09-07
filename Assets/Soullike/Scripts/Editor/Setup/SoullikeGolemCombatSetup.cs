using System;
using ProjectSoullike;
using UnityEditor;
using UnityEngine;

namespace ProjectSoullike.Editor
{
    public static class SoullikeGolemCombatSetup
    {
        private const string GolemPrefabPath = "Assets/Prefabs/Golem.prefab";
        private const string LogPrefix = "[Soullike Golem Combat Setup]";

        [MenuItem("Tools/Project Soullike/Set Up Temporary Golem Health")]
        public static void SetupFromMenu()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(GolemPrefabPath);
            if (root == null)
            {
                throw new InvalidOperationException($"Could not load prefab at {GolemPrefabPath}.");
            }

            try
            {
                GolemHealth health = root.GetComponent<GolemHealth>();
                if (health == null)
                {
                    health = root.AddComponent<GolemHealth>();
                }

                if (root.GetComponentInChildren<Collider>(includeInactive: true) == null)
                {
                    BoxCollider hitCollider = root.AddComponent<BoxCollider>();
                    ConfigureCollider(root.transform, hitCollider);
                    EditorUtility.SetDirty(hitCollider);
                }

                EditorUtility.SetDirty(health);
                EditorUtility.SetDirty(root);

                if (!PrefabUtility.SaveAsPrefabAsset(root, GolemPrefabPath, out bool saved) || !saved)
                {
                    throw new InvalidOperationException($"Could not save prefab at {GolemPrefabPath}.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidatePrefab();
        }

        private static void ConfigureCollider(Transform root, BoxCollider hitCollider)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("The Golem prefab does not contain a Renderer.");
            }

            bool initialized = false;
            Bounds localBounds = default;

            foreach (Renderer targetRenderer in renderers)
            {
                Bounds worldBounds = targetRenderer.bounds;
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

            hitCollider.center = localBounds.center;
            hitCollider.size = localBounds.size;
        }

        private static void ValidatePrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GolemPrefabPath);
            GolemHealth health = prefab == null ? null : prefab.GetComponent<GolemHealth>();

            if (health == null)
            {
                throw new InvalidOperationException("Golem prefab is missing GolemHealth after saving.");
            }

            Debug.Log($"{LogPrefix} COMPLETE Player attacks can now damage {prefab.name}.");
        }
    }
}
