using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ProjectSoullike.Editor
{
    /// <summary>
    /// Imports the TC Sword Unity package, builds reusable Humanoid animator
    /// controllers, and assigns them to the project player and boss prefabs.
    /// </summary>
    public static class SoullikeCombatAnimationSetup
    {
        private const string LogPrefix = "[Soullike Animation Setup]";

        private const string PackageAssetPath =
            "Assets/TC_Sword_Free_Pack/Unity_Pack/Unity_TC_Sword_Free_Pack.unitypackage";

        private const string ImportedAnimationFolder = "Assets/TC_Sword/Animation";
        private const string IdlePath = ImportedAnimationFolder + "/KBS_Ready_Idle_001.fbx";
        private const string WalkPath = ImportedAnimationFolder + "/KBS_Walk_F_001_IP.fbx";
        private const string RunPath = ImportedAnimationFolder + "/KBS_Run_F_001_IP.fbx";
        private const string AttackPath = ImportedAnimationFolder + "/KBS_Sword_ATK_Combo_01_001_IP.fbx";

        private const string OutputFolder = "Assets/Soullike/Animations";
        private const string PlayerControllerPath = OutputFolder + "/PlayerCombat.controller";
        private const string GolemControllerPath = OutputFolder + "/GolemPlaceholder.controller";

        private const string PlayerPrefabPath = "Assets/Prefabs/Mechanic_Girl.prefab";
        private const string GolemPrefabPath = "Assets/Prefabs/Golem.prefab";

        private const string MoveSpeedParameter = "MoveSpeed";
        private const string AttackParameter = "Attack";

        private static bool _isImporting;
        private static bool _forceRebuild;

        [MenuItem("Tools/Project Soullike/Rebuild Combat Animation Setup")]
        public static void RebuildFromMenu()
        {
            BeginSetup(forceRebuild: true);
        }

        private static void BeginSetup(bool forceRebuild)
        {
            if (_isImporting)
            {
                return;
            }

            _forceRebuild = forceRebuild;

            if (RequiredImportedAssetsExist())
            {
                BuildAndAssignControllers(forceRebuild);
                return;
            }

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            string packagePath = projectRoot == null
                ? string.Empty
                : Path.Combine(projectRoot, PackageAssetPath.Replace('/', Path.DirectorySeparatorChar));

            if (string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath))
            {
                Debug.LogError($"{LogPrefix} Package not found: {PackageAssetPath}");
                return;
            }

            _isImporting = true;
            AssetDatabase.importPackageCompleted += OnPackageImportCompleted;
            AssetDatabase.importPackageCancelled += OnPackageImportCancelled;
            AssetDatabase.importPackageFailed += OnPackageImportFailed;

            Debug.Log($"{LogPrefix} Importing TC Sword package.");
            AssetDatabase.ImportPackage(packagePath, interactive: false);
        }

        private static void OnPackageImportCompleted(string packageName)
        {
            UnsubscribeFromPackageEvents();
            Debug.Log($"{LogPrefix} Imported package: {packageName}");
            EditorApplication.delayCall += () => BuildAndAssignControllers(_forceRebuild);
        }

        private static void OnPackageImportCancelled(string packageName)
        {
            UnsubscribeFromPackageEvents();
            Debug.LogError($"{LogPrefix} Package import was cancelled: {packageName}");
        }

        private static void OnPackageImportFailed(string packageName, string errorMessage)
        {
            UnsubscribeFromPackageEvents();
            Debug.LogError($"{LogPrefix} Package import failed ({packageName}): {errorMessage}");
        }

        private static void UnsubscribeFromPackageEvents()
        {
            AssetDatabase.importPackageCompleted -= OnPackageImportCompleted;
            AssetDatabase.importPackageCancelled -= OnPackageImportCancelled;
            AssetDatabase.importPackageFailed -= OnPackageImportFailed;
            _isImporting = false;
        }

        private static bool RequiredImportedAssetsExist()
        {
            return AssetDatabase.LoadMainAssetAtPath(IdlePath) != null &&
                   AssetDatabase.LoadMainAssetAtPath(WalkPath) != null &&
                   AssetDatabase.LoadMainAssetAtPath(RunPath) != null &&
                   AssetDatabase.LoadMainAssetAtPath(AttackPath) != null;
        }

        private static void BuildAndAssignControllers(bool forceRebuild)
        {
            try
            {
                if (!RequiredImportedAssetsExist())
                {
                    Debug.LogError($"{LogPrefix} Imported animation assets are incomplete.");
                    return;
                }

                ConfigureClipLooping(IdlePath, shouldLoop: true);
                ConfigureClipLooping(WalkPath, shouldLoop: true);
                ConfigureClipLooping(RunPath, shouldLoop: true);
                ConfigureClipLooping(AttackPath, shouldLoop: false);

                AnimationClip idle = LoadClip(IdlePath);
                AnimationClip walk = LoadClip(WalkPath);
                AnimationClip run = LoadClip(RunPath);
                AnimationClip attack = LoadClip(AttackPath);

                EnsureFolder(OutputFolder);

                AnimatorController playerController = CreateController(
                    PlayerControllerPath,
                    idle,
                    walk,
                    run,
                    attack,
                    locomotionSpeed: 1f,
                    attackSpeed: 1f,
                    forceRebuild: forceRebuild);

                AnimatorController golemController = CreateController(
                    GolemControllerPath,
                    idle,
                    walk,
                    run,
                    attack,
                    locomotionSpeed: 0.8f,
                    attackSpeed: 0.72f,
                    forceRebuild: forceRebuild);

                AssignControllerToPrefab(PlayerPrefabPath, playerController, "Player");
                AssignControllerToPrefab(GolemPrefabPath, golemController, "Golem placeholder");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                ValidatePrefab(PlayerPrefabPath, playerController, "Player");
                ValidatePrefab(GolemPrefabPath, golemController, "Golem");

                Debug.Log(
                    $"{LogPrefix} COMPLETE - Player and Golem prefabs now use Humanoid combat controllers. " +
                    $"Parameters: {MoveSpeedParameter} (0 idle, 0.5 walk, 1 run), {AttackParameter} (Trigger).");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError($"{LogPrefix} Setup failed. See the exception above.");
            }
        }

        private static void ConfigureClipLooping(string assetPath, bool shouldLoop)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"ModelImporter not found for {assetPath}");
            }

            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importer.defaultClipAnimations;
            }

            bool changed = false;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                if (clip.loopTime == shouldLoop)
                {
                    continue;
                }

                clip.loopTime = shouldLoop;
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static AnimationClip LoadClip(string assetPath)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate =>
                    !candidate.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase));

            if (clip == null)
            {
                throw new InvalidOperationException($"Animation clip not found in {assetPath}");
            }

            return clip;
        }

        private static AnimatorController CreateController(
            string controllerPath,
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run,
            AnimationClip attack,
            float locomotionSpeed,
            float attackSpeed,
            bool forceRebuild)
        {
            AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (existing != null && !forceRebuild)
            {
                return existing;
            }

            if (existing != null && !AssetDatabase.DeleteAsset(controllerPath))
            {
                throw new InvalidOperationException($"Could not replace controller: {controllerPath}");
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter(MoveSpeedParameter, AnimatorControllerParameterType.Float);
            controller.AddParameter(AttackParameter, AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            BlendTree locomotionTree = new BlendTree()
            {
                name = "Locomotion Blend Tree",
                blendType = BlendTreeType.Simple1D,
                blendParameter = MoveSpeedParameter,
                useAutomaticThresholds = false
            };
            AssetDatabase.AddObjectToAsset(locomotionTree, controller);
            locomotionTree.AddChild(idle, 0f);
            locomotionTree.AddChild(walk, 0.5f);
            locomotionTree.AddChild(run, 1f);

            AnimatorState locomotionState = stateMachine.AddState("Locomotion", new Vector3(300f, 100f));
            locomotionState.motion = locomotionTree;
            locomotionState.speed = locomotionSpeed;
            locomotionState.writeDefaultValues = false;
            stateMachine.defaultState = locomotionState;

            AnimatorState attackState = stateMachine.AddState("Attack Combo", new Vector3(560f, 100f));
            attackState.motion = attack;
            attackState.speed = attackSpeed;
            attackState.writeDefaultValues = false;

            AnimatorStateTransition attackTransition = stateMachine.AddAnyStateTransition(attackState);
            attackTransition.hasExitTime = false;
            attackTransition.duration = 0.05f;
            attackTransition.canTransitionToSelf = false;
            attackTransition.interruptionSource = TransitionInterruptionSource.None;
            attackTransition.AddCondition(AnimatorConditionMode.If, 0f, AttackParameter);

            AnimatorStateTransition returnTransition = attackState.AddTransition(locomotionState);
            returnTransition.hasExitTime = true;
            returnTransition.exitTime = 0.95f;
            returnTransition.duration = 0.08f;
            returnTransition.interruptionSource = TransitionInterruptionSource.Source;

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(locomotionTree);
            return controller;
        }

        private static void AssignControllerToPrefab(
            string prefabPath,
            RuntimeAnimatorController controller,
            string label)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Animator animator = root.GetComponentInChildren<Animator>(includeInactive: true);
                if (animator == null)
                {
                    throw new InvalidOperationException($"Animator not found in {prefabPath}");
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

                string avatarSummary = animator.avatar == null
                    ? "no Avatar"
                    : $"Avatar valid={animator.avatar.isValid}, humanoid={animator.avatar.isHuman}";
                Debug.Log($"{LogPrefix} Assigned {label}: {controller.name} ({avatarSummary}).");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidatePrefab(
            string prefabPath,
            RuntimeAnimatorController expectedController,
            string label)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Animator animator = prefab == null
                ? null
                : prefab.GetComponentInChildren<Animator>(includeInactive: true);

            if (animator == null || animator.runtimeAnimatorController != expectedController)
            {
                throw new InvalidOperationException($"{label} prefab controller validation failed.");
            }

            if (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
            {
                throw new InvalidOperationException($"{label} prefab does not have a valid Humanoid Avatar.");
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            string[] segments = folderPath.Split('/');
            string current = segments[0];

            for (int index = 1; index < segments.Length; index++)
            {
                string next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }
    }
}
