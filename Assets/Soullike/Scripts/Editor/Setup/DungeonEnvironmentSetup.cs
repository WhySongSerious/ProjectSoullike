using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSoullike.Editor
{
    // Fab "Dungeon Environment / 135+ Assets" 패키지를 이 URP 프로젝트에서 바로 쓸 수 있게 정리한다.
    // 패키지 원본은 저장소에 포함하지 않으므로 각자 Fab에서 가져온 뒤 이 메뉴를 한 번 실행한다.
    //  1) Assets/Dungeon_Environment → Assets/ThirdParty/Dungeon_Environment 이동 (GUID 유지)
    //  2) 언리얼 변환 셰이더(Unreal/*, Built-in Standard/Particles) 머티리얼 → URP Lit / Particles Unlit
    //  3) 프리팹의 비어 있는 MeshCollider에 메시 연결
    // 여러 번 실행해도 결과가 같다.
    public static class DungeonEnvironmentSetup
    {
        public const string TargetRoot = "Assets/ThirdParty/Dungeon_Environment";
        private const string ImportedRoot = "Assets/Dungeon_Environment";
        private const string LogPrefix = "[Dungeon Environment Setup]";
        private const string LitShader = "Universal Render Pipeline/Lit";
        private const string ParticleShader = "Universal Render Pipeline/Particles/Unlit";

        [MenuItem("Tools/Project Soullike/Dungeon Environment/1. Set Up Imported Package")]
        public static void SetupFromMenu()
        {
            Run();
            EditorUtility.DisplayDialog("Dungeon Environment", "설정이 끝났습니다. 콘솔에서 결과를 확인하세요.", "확인");
        }

        public static bool IsSetUp()
        {
            Material sample = AssetDatabase.LoadAssetAtPath<Material>(TargetRoot + "/Materials/MI_Stone_Wall_00.mat");
            return sample != null && sample.shader != null && sample.shader.name == LitShader;
        }

        public static void Run()
        {
            string root = EnsureLocation();
            try
            {
                int packed = RepackMaskTextures(root);
                int materials = ConvertMaterials(root);
                FixCobweb(root);
                int colliders = FixColliders(root);
                AssetDatabase.SaveAssets();
                Debug.Log($"{LogPrefix} 완료 · 마스크 텍스처 {packed}개 생성, 머티리얼 {materials}개 변환, 콜라이더 {colliders}개 연결 · {root}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static string EnsureLocation()
        {
            if (AssetDatabase.IsValidFolder(TargetRoot))
            {
                return TargetRoot;
            }

            if (!AssetDatabase.IsValidFolder(ImportedRoot))
            {
                throw new InvalidOperationException($"{LogPrefix} {ImportedRoot} 폴더가 없습니다. Fab에서 'Dungeon Environment / 135+ Assets' Unity 패키지를 먼저 가져오세요.");
            }

            if (!AssetDatabase.IsValidFolder("Assets/ThirdParty"))
            {
                AssetDatabase.CreateFolder("Assets", "ThirdParty");
            }

            string error = AssetDatabase.MoveAsset(ImportedRoot, TargetRoot);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException($"{LogPrefix} 폴더 이동 실패: {error}");
            }

            return TargetRoot;
        }

        // 언리얼 ORM(R=AO, G=Roughness, B=Metallic) → URP 마스크(R=Metallic, G=AO, A=Smoothness)
        private static int RepackMaskTextures(string root)
        {
            string sourceDir = root + "/Textures";
            string outDir = sourceDir + "/URP";
            Directory.CreateDirectory(outDir);

            string[] sources = Directory.GetFiles(sourceDir, "*_AO_R_MT.png", SearchOption.TopDirectoryOnly);
            int created = 0;
            for (int i = 0; i < sources.Length; i++)
            {
                string outPath = MaskPathFor(root, sources[i]);
                if (File.Exists(outPath))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar("Dungeon Environment", "마스크 텍스처 변환 " + Path.GetFileName(sources[i]), (float)i / sources.Length);
                var src = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                var dst = default(Texture2D);
                try
                {
                    src.LoadImage(File.ReadAllBytes(sources[i]));
                    Color32[] px = src.GetPixels32();
                    for (int p = 0; p < px.Length; p++)
                    {
                        Color32 c = px[p];
                        px[p] = new Color32(c.b, c.r, 0, (byte)(255 - c.g));
                    }

                    dst = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false, true);
                    dst.SetPixels32(px);
                    dst.Apply();
                    File.WriteAllBytes(outPath, dst.EncodeToPNG());
                    created++;
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(src);
                    if (dst != null) UnityEngine.Object.DestroyImmediate(dst);
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string source in sources)
                {
                    string outPath = MaskPathFor(root, source);
                    if (!(AssetImporter.GetAtPath(outPath) is TextureImporter importer)) continue;
                    var srcImporter = AssetImporter.GetAtPath(source.Replace('\\', '/')) as TextureImporter;
                    int maxSize = srcImporter != null ? srcImporter.maxTextureSize : 2048;
                    if (!importer.sRGBTexture && importer.maxTextureSize == maxSize) continue;
                    importer.sRGBTexture = false;
                    importer.alphaIsTransparency = false;
                    importer.maxTextureSize = maxSize;
                    importer.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            return created;
        }

        private static string MaskPathFor(string root, string sourcePath)
        {
            string name = Path.GetFileNameWithoutExtension(sourcePath).Replace("_AO_R_MT", "_MS");
            return root + "/Textures/URP/" + name + ".png";
        }

        private static int ConvertMaterials(string root)
        {
            Shader lit = Shader.Find(LitShader);
            Shader particles = Shader.Find(ParticleShader);
            if (lit == null || particles == null)
            {
                throw new InvalidOperationException($"{LogPrefix} URP 셰이더를 찾지 못했습니다. URP 프로젝트에서 실행하세요.");
            }

            var materials = new List<(string path, Material mat)>();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                materials.Add((path, AssetDatabase.LoadAssetAtPath<Material>(path)));
            }

            int converted = 0;

            // 1차: 프리팹이 쓰는 언리얼 변환 머티리얼과 파티클
            foreach (var (path, mat) in materials)
            {
                string shaderName = mat.shader != null ? mat.shader.name : string.Empty;
                if (shaderName.StartsWith("Unreal/"))
                {
                    ConvertUnrealMaterial(root, mat, lit, particles);
                }
                else if (shaderName == "Particles/Standard Unlit")
                {
                    Texture main = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                    Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                    SetupParticle(mat, particles, main, color, additive: path.Contains("Fire"));
                }
                else
                {
                    continue;
                }

                EditorUtility.SetDirty(mat);
                converted++;
            }

            // 2차: Meshes 하위 Built-in Standard 머티리얼은 같은 이름의 변환본을 복사
            foreach (var (path, mat) in materials)
            {
                if (mat.shader == null || mat.shader.name != "Standard") continue;

                Material twin = AssetDatabase.LoadAssetAtPath<Material>(root + "/Materials/" + mat.name + ".mat")
                    ?? AssetDatabase.LoadAssetAtPath<Material>(root + "/Materials/" + mat.name + " - Copy.mat");
                if (twin != null && twin.shader == lit)
                {
                    mat.shader = twin.shader;
                    mat.CopyPropertiesFromMaterial(twin);
                    mat.shaderKeywords = twin.shaderKeywords;
                    mat.renderQueue = twin.renderQueue;
                }
                else
                {
                    Texture main = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                    Texture bump = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
                    Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                    mat.shader = lit;
                    mat.SetTexture("_BaseMap", main);
                    mat.SetColor("_BaseColor", color);
                    if (bump != null)
                    {
                        mat.SetTexture("_BumpMap", bump);
                        mat.EnableKeyword("_NORMALMAP");
                    }
                }

                EditorUtility.SetDirty(mat);
                converted++;
            }

            return converted;
        }

        private static void ConvertUnrealMaterial(string root, Material mat, Shader lit, Shader particles)
        {
            // 셰이더 프로퍼티 이름은 Material_Texture2D_N 이라 표시 이름(BC/N/AO_R_MT)으로 구분한다.
            Shader shader = mat.shader;
            var textures = new Dictionary<string, Texture>();
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                if (shader.GetPropertyType(i) != ShaderPropertyType.Texture) continue;
                Texture tex = mat.GetTexture(shader.GetPropertyName(i));
                if (tex != null) textures[shader.GetPropertyDescription(i)] = tex;
            }

            textures.TryGetValue("BC", out Texture baseColor);
            textures.TryGetValue("N", out Texture normal);
            textures.TryGetValue("AO_R_MT", out Texture orm);
            if (baseColor == null)
            {
                foreach (var pair in textures)
                {
                    if (pair.Key != "N" && pair.Key != "AO_R_MT" && pair.Key != "T" && pair.Key != "B") baseColor = pair.Value;
                }
            }

            if (shader.name.Contains("Fire"))
            {
                SetupParticle(mat, particles, baseColor, Color.white, additive: true);
                return;
            }

            mat.shader = lit;
            mat.SetTexture("_BaseMap", baseColor);
            mat.SetColor("_BaseColor", Color.white);
            // 언리얼 변환 메시는 V 좌표가 뒤집혀 있다.
            mat.SetTextureScale("_BaseMap", new Vector2(1f, -1f));
            mat.SetTextureOffset("_BaseMap", new Vector2(0f, 1f));

            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", 1f);
                mat.EnableKeyword("_NORMALMAP");
            }

            Texture2D mask = orm != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(MaskPathFor(root, AssetDatabase.GetAssetPath(orm))) : null;
            if (mask != null)
            {
                mat.SetTexture("_MetallicGlossMap", mask);
                mat.SetTexture("_OcclusionMap", mask);
                mat.SetFloat("_Metallic", 1f);
                mat.SetFloat("_Smoothness", 1f);
                mat.SetFloat("_OcclusionStrength", 1f);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.EnableKeyword("_OCCLUSIONMAP");
            }
            else
            {
                mat.SetFloat("_Metallic", 0f);
                mat.SetFloat("_Smoothness", 0.2f);
            }
        }

        private static void SetupParticle(Material mat, Shader particles, Texture texture, Color color, bool additive)
        {
            mat.shader = particles;
            mat.SetTexture("_BaseMap", texture);
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", additive ? 2f : 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Cull", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.SetOverrideTag("RenderType", "Transparent");
        }

        // 거미줄 텍스처는 알파 채널이 없어 회색값을 알파로 쓴다.
        private static void FixCobweb(string root)
        {
            if (AssetImporter.GetAtPath(root + "/Textures/T_Cobweb.png") is TextureImporter importer
                && importer.alphaSource != TextureImporterAlphaSource.FromGrayScale)
            {
                importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            foreach (string guid in AssetDatabase.FindAssets("Cobweb t:Material", new[] { root }))
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (mat.shader == null || mat.shader.name != LitShader) continue;
                mat.SetFloat("_AlphaClip", 1f);
                mat.SetFloat("_Cutoff", 0.15f);
                mat.SetFloat("_Cull", 0f);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.renderQueue = (int)RenderQueue.AlphaTest;
                mat.SetOverrideTag("RenderType", "TransparentCutout");
                EditorUtility.SetDirty(mat);
            }
        }

        // 패키지 프리팹은 루트에 메시 없는 MeshCollider가 있고 메시는 자식에 있다.
        private static int FixColliders(string root)
        {
            int fixedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { root + "/Prefabs" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Dungeon Environment", "콜라이더 연결 " + Path.GetFileName(path), (float)i / guids.Length);

                GameObject prefab = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool dirty = false;
                    foreach (MeshCollider collider in prefab.GetComponentsInChildren<MeshCollider>(true))
                    {
                        if (collider.sharedMesh != null) continue;

                        MeshFilter filter = collider.GetComponent<MeshFilter>();
                        if (filter == null) filter = collider.GetComponentInChildren<MeshFilter>(true);
                        if (filter == null || filter.sharedMesh == null) continue;

                        if (filter.transform != collider.transform)
                        {
                            MeshCollider target = filter.GetComponent<MeshCollider>();
                            if (target == null) target = filter.gameObject.AddComponent<MeshCollider>();
                            target.sharedMesh = filter.sharedMesh;
                            UnityEngine.Object.DestroyImmediate(collider, true);
                        }
                        else
                        {
                            collider.sharedMesh = filter.sharedMesh;
                        }

                        dirty = true;
                        fixedCount++;
                    }

                    if (dirty) PrefabUtility.SaveAsPrefabAsset(prefab, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefab);
                }
            }

            return fixedCount;
        }
    }
}
