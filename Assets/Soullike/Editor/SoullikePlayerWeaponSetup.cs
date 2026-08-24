using System;
using ProjectSoullike;
using UnityEditor;
using UnityEngine;

namespace ProjectSoullike.Editor
{
    public static class SoullikePlayerWeaponSetup
    {
        private const string LogPrefix = "[Soullike Weapon Setup]";
        private const string PlayerPrefabPath = "Assets/Prefabs/Mechanic_Girl.prefab";
        private const string SwordPrefabPath = "Assets/TC_Sword/TonySword_01.fbx";
        private const string BladeMaterialPath = "Assets/TC_Sword/Materials/Refelection_03.mat";
        private const string HandleMaterialPath = "Assets/TC_Sword/Materials/phongE1Handle.mat";
        private const string HiltMaterialPath = "Assets/TC_Sword/Materials/phongE2Hilt.mat";
        private const string SwordInstanceName = "TonySword_01";

        private static readonly Vector3 SwordLocalPosition = new Vector3(0.0323f, -0.0425f, 0.0031f);
        private static readonly Quaternion SwordLocalRotation = new Quaternion(
            0.5367262f,
            -0.25576058f,
            -0.29337004f,
            0.74862915f);

        [MenuItem("Tools/Project Soullike/Set Up Player Tony Sword")]
        public static void SetupFromMenu()
        {
            try
            {
                UpgradeSwordMaterials();
                AttachSwordToPlayer();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ValidateSetup();
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LogPrefix} Setup failed: {exception}");
                throw;
            }
        }

        private static void UpgradeSwordMaterials()
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                throw new InvalidOperationException("Universal Render Pipeline/Lit shader could not be found.");
            }

            UpgradeMaterial(BladeMaterialPath, urpLit, metallic: 0.9f, smoothness: 0.8f);
            UpgradeMaterial(HandleMaterialPath, urpLit, metallic: 0.15f, smoothness: 0.35f);
            UpgradeMaterial(HiltMaterialPath, urpLit, metallic: 0.75f, smoothness: 0.65f);
        }

        private static void UpgradeMaterial(
            string materialPath,
            Shader shader,
            float metallic,
            float smoothness)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                throw new InvalidOperationException($"Material not found at {materialPath}.");
            }

            Color baseColor = material.HasProperty("_BaseColor")
                ? material.GetColor("_BaseColor")
                : material.HasProperty("_Color")
                    ? material.GetColor("_Color")
                    : Color.white;
            Texture baseMap = material.HasProperty("_BaseMap")
                ? material.GetTexture("_BaseMap")
                : material.HasProperty("_MainTex")
                    ? material.GetTexture("_MainTex")
                    : null;
            Vector2 mapScale = material.HasProperty("_BaseMap")
                ? material.GetTextureScale("_BaseMap")
                : material.HasProperty("_MainTex")
                    ? material.GetTextureScale("_MainTex")
                    : Vector2.one;
            Vector2 mapOffset = material.HasProperty("_BaseMap")
                ? material.GetTextureOffset("_BaseMap")
                : material.HasProperty("_MainTex")
                    ? material.GetTextureOffset("_MainTex")
                    : Vector2.zero;

            material.shader = shader;
            material.SetColor("_BaseColor", baseColor);
            material.SetTexture("_BaseMap", baseMap);
            material.SetTextureScale("_BaseMap", mapScale);
            material.SetTextureOffset("_BaseMap", mapOffset);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
        }

        private static void AttachSwordToPlayer()
        {
            GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SwordPrefabPath);
            if (swordPrefab == null)
            {
                throw new InvalidOperationException($"Sword prefab not found at {SwordPrefabPath}.");
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (root == null)
            {
                throw new InvalidOperationException($"Player prefab not found at {PlayerPrefabPath}.");
            }

            try
            {
                Animator animator = root.GetComponentInChildren<Animator>(true);
                if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
                {
                    throw new InvalidOperationException("Player needs a valid Humanoid Animator.");
                }

                Transform gripBone = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                if (gripBone == null)
                {
                    throw new InvalidOperationException("Right middle proximal finger bone was not found.");
                }

                Transform existingSword = gripBone.Find(SwordInstanceName);
                if (existingSword != null)
                {
                    UnityEngine.Object.DestroyImmediate(existingSword.gameObject);
                }

                GameObject sword = PrefabUtility.InstantiatePrefab(swordPrefab, gripBone) as GameObject;
                if (sword == null)
                {
                    throw new InvalidOperationException("TonySword prefab instance could not be created.");
                }

                sword.name = SwordInstanceName;
                sword.transform.localPosition = SwordLocalPosition;
                sword.transform.localRotation = SwordLocalRotation;
                sword.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(sword.transform);
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

        private static void ValidateSetup()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            Animator animator = prefab == null ? null : prefab.GetComponentInChildren<Animator>(true);
            Transform gripBone = animator == null
                ? null
                : animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            Transform sword = gripBone == null ? null : gripBone.Find(SwordInstanceName);

            if (sword == null)
            {
                throw new InvalidOperationException("TonySword validation failed on the player prefab.");
            }

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(sword.gameObject);
            if (source == null || AssetDatabase.GetAssetPath(source) != SwordPrefabPath)
            {
                throw new InvalidOperationException("Attached sword does not reference the TonySword FBX asset.");
            }

            Material blade = AssetDatabase.LoadAssetAtPath<Material>(BladeMaterialPath);
            Material handle = AssetDatabase.LoadAssetAtPath<Material>(HandleMaterialPath);
            Material hilt = AssetDatabase.LoadAssetAtPath<Material>(HiltMaterialPath);
            if (blade == null || handle == null || hilt == null ||
                blade.shader.name != "Universal Render Pipeline/Lit" ||
                handle.shader.name != "Universal Render Pipeline/Lit" ||
                hilt.shader.name != "Universal Render Pipeline/Lit")
            {
                throw new InvalidOperationException("TonySword URP material validation failed.");
            }

            Debug.Log(
                $"{LogPrefix} COMPLETE Bone={gripBone.name}, Sword={sword.name}, " +
                $"LocalPosition={sword.localPosition}, LocalRotation={sword.localEulerAngles}, " +
                $"Shader={handle.shader.name}");
        }
    }
}
