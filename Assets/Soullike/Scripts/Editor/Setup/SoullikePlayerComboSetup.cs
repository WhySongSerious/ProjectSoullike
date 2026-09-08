using System;
using System.Linq;
using ProjectSoullike;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ProjectSoullike.Editor
{
    public static class SoullikePlayerComboSetup
    {
        private const string LogPrefix = "[Soullike Combo Setup]";
        private const string AttackFbxPath =
            "Assets/ThirdParty/TC_Sword/Animation/KBS_Sword_ATK_Combo_01_001_IP.fbx";
        private const string IdlePath = "Assets/ThirdParty/TC_Sword/Animation/KBS_Ready_Idle_001.fbx";
        private const string WalkPath = "Assets/ThirdParty/TC_Sword/Animation/KBS_Walk_F_001_IP.fbx";
        private const string RunPath = "Assets/ThirdParty/TC_Sword/Animation/KBS_Run_F_001_IP.fbx";
        private const string PlayerControllerPath = "Assets/Soullike/Animations/PlayerCombat.controller";
        private const string PlayerPrefabPath = "Assets/Prefabs/Mechanic_Girl.prefab";
        private const string MoveSpeedParameter = "MoveSpeed";
        private const string AttackOneClipName = "Attack_01";
        private const string AttackTwoClipName = "Attack_02";
        private const string AttackThreeClipName = "Attack_03";

        [MenuItem("Tools/Project Soullike/Set Up Player Combo and Camera")]
        public static void SetupFromMenu()
        {
            try
            {
                ConfigureAttackClips();

                AnimationClip idle = LoadFirstClip(IdlePath);
                AnimationClip walk = LoadFirstClip(WalkPath);
                AnimationClip run = LoadFirstClip(RunPath);
                AnimationClip attackOne = LoadNamedClip(AttackFbxPath, AttackOneClipName);
                AnimationClip attackTwo = LoadNamedClip(AttackFbxPath, AttackTwoClipName);
                AnimationClip attackThree = LoadNamedClip(AttackFbxPath, AttackThreeClipName);

                AnimatorController controller = RebuildPlayerController(
                    idle,
                    walk,
                    run,
                    attackOne,
                    attackTwo,
                    attackThree);

                AssignControllerAndRefreshPrefab(controller);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ValidateSetup(controller);
            }
            catch (Exception exception)
            {
                Debug.LogError($"{LogPrefix} Setup failed: {exception}");
                throw;
            }
        }

        private static void ConfigureAttackClips()
        {
            ModelImporter importer = AssetImporter.GetAtPath(AttackFbxPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"ModelImporter not found for {AttackFbxPath}.");
            }

            ModelImporterClipAnimation[] currentClips = importer.clipAnimations;
            if (currentClips == null || currentClips.Length == 0)
            {
                currentClips = importer.defaultClipAnimations;
            }

            if (currentClips == null || currentClips.Length == 0)
            {
                throw new InvalidOperationException("Attack FBX does not contain a source animation take.");
            }

            ModelImporterClipAnimation template = currentClips[0];
            string fullClipName = currentClips
                .FirstOrDefault(clip => clip.name != AttackOneClipName &&
                                        clip.name != AttackTwoClipName &&
                                        clip.name != AttackThreeClipName)?.name
                ?? template.takeName;

            importer.clipAnimations = new[]
            {
                CreateClip(template, fullClipName, 0f, 164f),
                CreateClip(template, AttackOneClipName, 0f, 40f),
                CreateClip(template, AttackTwoClipName, 40f, 80f),
                CreateClip(template, AttackThreeClipName, 80f, 164f)
            };
            importer.SaveAndReimport();
        }

        private static ModelImporterClipAnimation CreateClip(
            ModelImporterClipAnimation template,
            string name,
            float firstFrame,
            float lastFrame)
        {
            return new ModelImporterClipAnimation
            {
                name = name,
                takeName = template.takeName,
                firstFrame = firstFrame,
                lastFrame = lastFrame,
                wrapMode = WrapMode.Once,
                loop = false,
                loopTime = false,
                loopPose = false,
                cycleOffset = 0f,
                mirror = template.mirror,
                keepOriginalOrientation = template.keepOriginalOrientation,
                keepOriginalPositionY = template.keepOriginalPositionY,
                keepOriginalPositionXZ = template.keepOriginalPositionXZ,
                heightFromFeet = template.heightFromFeet,
                lockRootRotation = template.lockRootRotation,
                lockRootHeightY = template.lockRootHeightY,
                lockRootPositionXZ = template.lockRootPositionXZ,
                rotationOffset = template.rotationOffset,
                heightOffset = template.heightOffset
            };
        }

        private static AnimationClip LoadFirstClip(string assetPath)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate =>
                    !candidate.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase));

            return clip ?? throw new InvalidOperationException($"Animation clip not found in {assetPath}.");
        }

        private static AnimationClip LoadNamedClip(string assetPath, string clipName)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate => candidate.name == clipName);

            return clip ?? throw new InvalidOperationException(
                $"Animation clip {clipName} not found in {assetPath}.");
        }

        private static AnimatorController RebuildPlayerController(
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run,
            AnimationClip attackOne,
            AnimationClip attackTwo,
            AnimationClip attackThree)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(PlayerControllerPath);
            }

            controller.layers = Array.Empty<AnimatorControllerLayer>();
            controller.parameters = Array.Empty<AnimatorControllerParameter>();

            UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(PlayerControllerPath)
                .Where(asset => asset != null && asset != controller)
                .ToArray();

            foreach (UnityEngine.Object subAsset in subAssets)
            {
                UnityEngine.Object.DestroyImmediate(subAsset, allowDestroyingAssets: true);
            }

            AnimatorStateMachine stateMachine = new AnimatorStateMachine
            {
                name = "Base Layer",
                hideFlags = HideFlags.HideInHierarchy
            };
            AssetDatabase.AddObjectToAsset(stateMachine, controller);

            controller.layers = new[]
            {
                new AnimatorControllerLayer
                {
                    name = "Base Layer",
                    defaultWeight = 1f,
                    stateMachine = stateMachine
                }
            };
            controller.AddParameter(MoveSpeedParameter, AnimatorControllerParameterType.Float);

            BlendTree locomotionTree = new BlendTree
            {
                name = "Locomotion Blend Tree",
                blendType = BlendTreeType.Simple1D,
                blendParameter = MoveSpeedParameter,
                useAutomaticThresholds = false,
                hideFlags = HideFlags.HideInHierarchy
            };
            AssetDatabase.AddObjectToAsset(locomotionTree, controller);
            locomotionTree.AddChild(idle, 0f);
            locomotionTree.AddChild(walk, 0.5f);
            locomotionTree.AddChild(run, 1f);

            AnimatorState locomotion = stateMachine.AddState("Locomotion", new Vector3(250f, 100f));
            locomotion.motion = locomotionTree;
            locomotion.writeDefaultValues = false;
            stateMachine.defaultState = locomotion;

            AnimatorState attackOneState = CreateAttackState(
                stateMachine, "Attack 1", attackOne, new Vector3(500f, 20f));
            AnimatorState attackTwoState = CreateAttackState(
                stateMachine, "Attack 2", attackTwo, new Vector3(700f, 100f));
            AnimatorState attackThreeState = CreateAttackState(
                stateMachine, "Attack 3", attackThree, new Vector3(500f, 180f));
            AddReturnTransition(attackOneState, locomotion);
            AddReturnTransition(attackTwoState, locomotion);
            AddReturnTransition(attackThreeState, locomotion);

            EditorUtility.SetDirty(locomotionTree);
            EditorUtility.SetDirty(stateMachine);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorState CreateAttackState(
            AnimatorStateMachine stateMachine,
            string name,
            AnimationClip clip,
            Vector3 position)
        {
            AnimatorState state = stateMachine.AddState(name, position);
            state.motion = clip;
            state.speed = 1.2f;
            state.writeDefaultValues = false;
            return state;
        }

        private static void AddReturnTransition(AnimatorState attackState, AnimatorState locomotion)
        {
            AnimatorStateTransition transition = attackState.AddTransition(locomotion);
            transition.hasExitTime = true;
            transition.exitTime = 0.98f;
            transition.duration = 0.08f;
            transition.hasFixedDuration = true;
            transition.interruptionSource = TransitionInterruptionSource.Source;
            transition.orderedInterruption = true;
            transition.canTransitionToSelf = false;
        }

        private static void AssignControllerAndRefreshPrefab(RuntimeAnimatorController controller)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (root == null)
            {
                throw new InvalidOperationException($"Could not load prefab at {PlayerPrefabPath}.");
            }

            try
            {
                Animator animator = root.GetComponentInChildren<Animator>(true);
                KeyboardPlayerController keyboardController = root.GetComponent<KeyboardPlayerController>();
                CharacterController characterController = root.GetComponent<CharacterController>();

                if (animator == null || keyboardController == null || characterController == null)
                {
                    throw new InvalidOperationException(
                        "Player prefab needs an Animator, KeyboardPlayerController, and CharacterController.");
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                EditorUtility.SetDirty(animator);
                EditorUtility.SetDirty(keyboardController);
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

        private static void ValidateSetup(AnimatorController expectedController)
        {
            AnimationClip attackOne = LoadNamedClip(AttackFbxPath, AttackOneClipName);
            AnimationClip attackTwo = LoadNamedClip(AttackFbxPath, AttackTwoClipName);
            AnimationClip attackThree = LoadNamedClip(AttackFbxPath, AttackThreeClipName);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            Animator animator = prefab == null ? null : prefab.GetComponentInChildren<Animator>(true);

            if (animator == null || animator.runtimeAnimatorController != expectedController ||
                animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
            {
                throw new InvalidOperationException("Player Animator validation failed.");
            }

            string[] stateNames = expectedController.layers[0].stateMachine.states
                .Select(child => child.state.name)
                .ToArray();
            string[] requiredStates =
            {
                "Locomotion",
                "Attack 1",
                "Attack 2",
                "Attack 3"
            };
            if (requiredStates.Any(required => !stateNames.Contains(required)))
            {
                throw new InvalidOperationException("Player controller is missing one or more combo states.");
            }

            Debug.Log(
                $"{LogPrefix} COMPLETE States={string.Join(", ", requiredStates)}, " +
                $"Clips={attackOne.length:F2}s/{attackTwo.length:F2}s/{attackThree.length:F2}s, " +
                $"AvatarValid={animator.avatar.isValid}, Humanoid={animator.avatar.isHuman}");
        }
    }
}
