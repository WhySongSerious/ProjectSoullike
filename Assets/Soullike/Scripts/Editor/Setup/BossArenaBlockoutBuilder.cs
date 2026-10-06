using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectSoullike.Editor
{
    // #29 레벨·맵 디자인 확인용 보스 아레나 블록아웃을 Dungeon Environment 프리팹으로 조립한다.
    // 씬 파일 대신 배치 데이터를 커밋해 각자의 Unity 버전에서 같은 맵을 생성한다.
    // 좌표 기준: 아레나 바닥 x[-12,12] z[0,24], 입구 복도 x[-2,2] z[-14,0], 벽 높이 5m.
    public static class BossArenaBlockoutBuilder
    {
        private const string ScenePath = "Assets/Scenes/BossArena_Blockout.unity";
        private const string SkyPath = "Assets/Soullike/Materials/BossArena_NightSky.mat";
        private const string LogPrefix = "[Boss Arena Blockout]";

        private static string prefabRoot;
        private static Transform group;

        [MenuItem("Tools/Project Soullike/Dungeon Environment/2. Build Boss Arena Blockout")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null
                && !EditorUtility.DisplayDialog("Boss Arena Blockout", ScenePath + " 를 다시 생성할까요? 직접 수정한 내용은 사라집니다.", "다시 생성", "취소"))
            {
                return;
            }

            Build();
        }

        public static void Build()
        {
            if (!DungeonEnvironmentSetup.IsSetUp()) DungeonEnvironmentSetup.Run();
            prefabRoot = DungeonEnvironmentSetup.TargetRoot + "/Prefabs/";

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var root = new GameObject("BossArena").transform;

            BuildFloor(Group(root, "Floor"));
            BuildWalls(Group(root, "Walls"));
            BuildProps(Group(root, "Props"));
            BuildTorches(Group(root, "Torches"));
            BuildGate(Group(root, "SealedGate_Bars"));
            BuildRoof(Group(root, "Roof"));
            BuildMarkers(Group(root, "Markers"));
            SetupLightingAndCamera();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"{LogPrefix} 생성 완료 · {ScenePath} · 오브젝트 {root.GetComponentsInChildren<Transform>(true).Length}개");
        }

        private static Transform Group(Transform root, string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(root, false);
            return t;
        }

        private static GameObject Put(string prefab, float x, float y, float z, float rx = 0f, float ry = 0f, float rz = 0f, float sx = 1f, float sy = 1f, float sz = 1f)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(prefabRoot + prefab + ".prefab");
            if (source == null) throw new InvalidOperationException($"{LogPrefix} 프리팹 없음: {prefab}");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(source);
            go.transform.SetParent(group, false);
            go.transform.SetPositionAndRotation(new Vector3(x, y, z), Quaternion.Euler(rx, ry, rz));
            go.transform.localScale = new Vector3(sx, sy, sz);
            return go;
        }

        private static void BuildFloor(Transform parent)
        {
            group = parent;
            for (int x = -11; x <= 11; x += 2)
                for (int z = 1; z <= 23; z += 2)
                    Put("SM_Ground_01", x, 0f, z);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -13; z <= -1; z += 2)
                    Put("SM_Ground_01", x, 0f, z);
        }

        private static void BuildWalls(Transform parent)
        {
            group = parent;
            foreach (float z in new[] { 3.21f, 9.63f, 16.05f, 22.47f })
            {
                Put("SM_Wall_01", -12.77f, 0f, z);
                Put("SM_Wall_01", 12.77f, 0f, z);
            }

            // 남쪽(입구)·북쪽(봉인문) 벽은 가운데 아치 자리를 비운다.
            foreach (float wallZ in new[] { -0.77f, 24.77f })
            {
                foreach (float x in new[] { 4.76f, 11.18f })
                {
                    Put("SM_Wall_01", x, 0f, wallZ, ry: 90f);
                    Put("SM_Wall_01", -x, 0f, wallZ, ry: 90f);
                }
                Put("SM_Arch", -0.6f, 0f, wallZ, ry: 90f).name = wallZ < 0f ? "EntranceArch" : "SealedGate_Arch";
            }

            foreach (float z in new[] { -3.21f, -9.63f, -12.79f })
            {
                Put("SM_Wall_01", -2.77f, 0f, z);
                Put("SM_Wall_01", 2.77f, 0f, z);
            }
            Put("SM_Wall_01", 0f, 0f, -14.77f, ry: 90f);
        }

        private static void BuildProps(Transform parent)
        {
            group = parent;
            // 입구 복도
            Put("SM_Dungeon_door", 0f, 0f, -14.17f);
            Put("SM_Medieval_Weapons_Rack_01", -1.3f, 0f, -7f);
            Put("SM_Dungeon_chest", 1.4f, 0f, -12.5f, ry: 270f);
            Put("SM_Skeleton_Pile_04", 1.2f, 0f, -5f, ry: 100f);

            // 아레나 가장자리 · 중앙 약 16m는 전투 공간으로 비워 둔다.
            Put("SM_Medieval_Weapons_Rack_01", -11.3f, 0f, 8f);
            Put("SM_Medieval_Weapons_Rack_01", 11.3f, 0f, 8f, ry: 180f);
            Put("SM_Medieval_Weapons_Rack_01", -11.3f, 0f, 16f);
            Put("SM_Medieval_Weapons_Rack_01", 11.3f, 0f, 16f, ry: 180f);
            Put("SM_Bench", -8f, 0f, 0.6f);
            Put("SM_Bench", 8f, 0f, 0.6f);
            Put("SM_Dungeon_chest", -11.2f, 0f, 2.5f, ry: 90f);
            Put("SM_Scribe_Chair", 10.8f, 0f, 2.4f, ry: 240f);
            Put("SM_Skeleton_Pile_00", -7f, 0f, 23f, ry: 10f);
            Put("SM_Skeleton_Pile_04", -11f, 0f, 7f, ry: 80f);
            Put("SM_Skeleton_Pile_00", 11f, 0f, 16.5f, ry: 250f);
            Put("SM_Skeleton_Pile_02", -10.4f, 0f, 21.5f, ry: 30f);
            Put("SM_Skull_01", -3.5f, 0f, 9f, ry: 30f);
            Put("SM_Skull_02", 4.2f, 0f, 14.5f, ry: 200f);
            Put("SM_Skeleton_Arm_01", 2.5f, 0f, 6f, ry: 60f);
            Put("SM_Skeleton_Leg_00", -5f, 0f, 17f, ry: 140f);

            // 북쪽 보스 제단
            Put("SM_Medieval_Weapons_Claymore", -3.5f, 0f, 23.5f);
            Put("SM_Medieval_Weapons_Espadon", 3.5f, 0f, 23.5f);
            foreach (float x in new[] { -6.5f, -5f, 5f, 6.5f }) Put("SM_Spear", x, 0f, 23.4f);
            Put("SM_Candles_Holder_03", -0.934f, 0f, 23.471f, ry: 237.9f);
            Put("SM_Candles_Holder_01", -3.582f, 0f, 22.966f, ry: 243.4f);
            Put("SM_Candle_Melted_02", -3.661f, 0f, 23.554f, ry: 303.7f);
            Put("SM_Candle_Melted_04", 2.84f, 0f, 23.049f, ry: 335.6f);
            Put("SM_Candle_Melted_07", -0.441f, 0f, 22.711f, ry: 317.5f);
            Put("SM_Candle_Melted_10", -0.784f, 0f, 23.405f, ry: 120.7f);
            Put("SM_Candles_Holder_03", 2.933f, 0f, 22.625f, ry: 268.9f);
            Put("SM_Candle_Melted_03", 0.523f, 0f, 22.775f, ry: 172.1f);
        }

        private static void BuildTorches(Transform parent)
        {
            group = parent;
            var fire = AssetDatabase.LoadAssetAtPath<GameObject>(prefabRoot + "PS_Fire_00.prefab");
            // (x, z, 벽에서 튀어나오는 방향 yaw)
            var torches = new (float x, float z, float yaw)[]
            {
                (-12f, 4f, 0f), (12f, 4f, 180f), (-12f, 12f, 0f), (12f, 12f, 180f), (-12f, 20f, 0f), (12f, 20f, 180f),
                (-6f, 24f, 90f), (6f, 24f, 90f), (-6f, 0f, 270f), (6f, 0f, 270f),
                (-2f, -4f, 0f), (2f, -4f, 180f), (-2f, -11f, 0f), (2f, -11f, 180f),
            };

            foreach (var t in torches)
            {
                Transform torch = Put("SM_Torch", t.x, 2.4f, t.z, ry: t.yaw).transform;
                Vector3 tip = torch.TransformPoint(new Vector3(0.3f, 0.35f, 0f));

                var lightGo = new GameObject("TorchLight");
                lightGo.transform.SetParent(torch, false);
                lightGo.transform.position = tip + Vector3.up * 0.1f;
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.6f, 0.3f);
                light.range = 8f;
                light.intensity = 2.2f;
                light.shadows = LightShadows.Soft;

                if (fire != null)
                {
                    var flame = (GameObject)PrefabUtility.InstantiatePrefab(fire);
                    flame.transform.SetParent(torch, true);
                    flame.transform.position = tip;
                }
            }
        }

        private static void BuildGate(Transform parent)
        {
            group = parent;
            Put("SM_Bars", 0.35f, 0f, 24.1f, ry: 90f);
            Put("SM_Bars", 0.35f, 2.3f, 24.1f, ry: 90f);
        }

        private static void BuildRoof(Transform parent)
        {
            group = parent;
            // 입구 복도는 완전히 덮고, 아레나는 벽 쪽 4m만 무너진 돌판 지붕으로 덮는다.
            foreach (float z in new[] { -2.4f, -7.2f, -12f }) Put("SM_Roof", 0f, 5.4f, z);

            var slabs = new (float x, float[] z)[]
            {
                (-11f, new[] { 1f, 3f, 5f, 7f, 9f, 11f, 13f, 15f, 17f, 19f, 21f, 23f }),
                (11f, new[] { 1f, 3f, 5f, 7f, 9f, 11f, 13f, 15f, 17f, 19f, 21f }),
                (9f, new[] { 1f, 3f, 7f, 9f, 11f, 13f, 15f, 21f, 23f }),
                (-9f, new[] { 9f, 11f, 13f, 15f, 19f, 21f, 23f }),
                (-7f, new[] { 1f, 3f, 21f, 23f }), (-5f, new[] { 1f, 3f, 21f, 23f }), (-3f, new[] { 1f, 3f, 21f, 23f }),
                (-1f, new[] { 23f }), (1f, new[] { 21f, 23f }), (3f, new[] { 1f, 3f }),
                (5f, new[] { 1f, 3f, 21f, 23f }), (7f, new[] { 1f, 3f, 21f, 23f }),
            };
            foreach (var column in slabs)
                foreach (float z in column.z)
                    Put("SM_Ground_01", column.x, 5.05f, z, rx: 180f);

            foreach (float z in new[] { 4f, 10f, 16f, 20f })
            {
                Put("SM_Wooden_Beam", -10f, 4.85f, z, ry: 90f, sx: 1.6f, sy: 1.6f, sz: 2.7f);
                Put("SM_Wooden_Beam", 10f, 4.85f, z, ry: 90f, sx: 1.6f, sy: 1.6f, sz: 2.7f);
            }
            foreach (float x in new[] { -6f, 6f })
            {
                Put("SM_Wooden_Beam", x, 4.85f, 22f, sx: 1.6f, sy: 1.6f, sz: 2.7f);
                Put("SM_Wooden_Beam", x, 4.85f, 2f, sx: 1.6f, sy: 1.6f, sz: 2.7f);
            }
            Put("SM_Wooden_Beam", -9.4f, 3.9f, 13.2f, rx: 35f, ry: 90f, sx: 1.6f, sy: 1.6f, sz: 2.2f);

            // 무너진 지붕 잔해
            Put("SM_Ground_01", -10.8f, 0.25f, 18.5f, 12f, 30f, 8f, 0.6f, 1f, 0.6f);
            Put("SM_Ground_01", 10.7f, 0.2f, 6.2f, -10f, -20f, 6f, 0.7f, 1f, 0.5f);
        }

        private static void BuildMarkers(Transform parent)
        {
            var player = new GameObject("PlayerSpawn").transform;
            player.SetParent(parent, false);
            player.position = new Vector3(0f, 0f, -2.5f);

            var boss = new GameObject("BossSpawn").transform;
            boss.SetParent(parent, false);
            boss.SetPositionAndRotation(new Vector3(0f, 0f, 18f), Quaternion.Euler(0f, 180f, 0f));

            // 승리 후 봉인문 너머로 켜질 기억의 빛 (기본 비활성)
            var reveal = new GameObject("VictoryReveal (disabled)");
            reveal.transform.SetParent(parent, false);
            AddPointLight(reveal.transform, "MemoryLight", new Vector3(0f, 2.5f, 25.5f), new Color(1f, 0.82f, 0.5f), 14f, 6f);
            reveal.SetActive(false);

            // 보스 위치의 절제된 타락의 붉은빛
            AddPointLight(parent, "CorruptionLight", new Vector3(0f, 0.6f, 19.5f), new Color(0.75f, 0.12f, 0.1f), 6f, 1.2f);
        }

        private static void AddPointLight(Transform parent, string name, Vector3 position, Color color, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
        }

        private static void SetupLightingAndCamera()
        {
            // 확정 서사 색 규칙: 차가운 청회색 석재·안개 / 따뜻한 기억의 빛 / 절제된 붉은빛
            // Unity 6.3/6.6 모두에서 경고 없이 동작하도록 씬 루트에서 직접 찾는다.
            Light sun = null;
            foreach (GameObject rootObject in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Light candidate = rootObject.GetComponent<Light>();
                if (candidate != null && candidate.type == LightType.Directional) sun = candidate;
            }

            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(62f, -25f, 0f);
                sun.color = new Color(0.6f, 0.7f, 0.85f);
                sun.intensity = 0.45f;
                RenderSettings.sun = sun;
            }

            RenderSettings.skybox = GetOrCreateSky();
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.18f, 0.22f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.22f, 0.26f, 0.33f);
            RenderSettings.fogDensity = 0.012f;

            // 팀 플레이어 카메라 기준(거리 4.6m, 피벗 1.45m)으로 입구에서 보스 쪽을 본 구도
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
                Vector3 pivot = new Vector3(0f, 1.45f, 4f);
                Vector3 dir = Quaternion.Euler(12f, 0f, 0f) * Vector3.forward;
                cam.transform.SetPositionAndRotation(pivot - dir * 4.6f, Quaternion.LookRotation(dir));
            }
        }

        private static Material GetOrCreateSky()
        {
            var sky = AssetDatabase.LoadAssetAtPath<Material>(SkyPath);
            if (sky != null) return sky;

            if (!AssetDatabase.IsValidFolder("Assets/Soullike/Materials")) AssetDatabase.CreateFolder("Assets/Soullike", "Materials");
            sky = new Material(Shader.Find("Skybox/Procedural"));
            sky.SetFloat("_SunDisk", 0f);
            sky.SetFloat("_SunSize", 0.02f);
            sky.SetFloat("_AtmosphereThickness", 0.45f);
            sky.SetColor("_SkyTint", new Color(0.28f, 0.33f, 0.45f));
            sky.SetColor("_GroundColor", new Color(0.1f, 0.11f, 0.13f));
            sky.SetFloat("_Exposure", 0.55f);
            AssetDatabase.CreateAsset(sky, SkyPath);
            return sky;
        }
    }
}
