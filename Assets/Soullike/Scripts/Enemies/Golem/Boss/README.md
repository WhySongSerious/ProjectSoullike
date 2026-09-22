# Teammate Boss Prototype Integration

This folder contains the reusable state-machine portion extracted from the sibling
`Project` Unity project and adapted to the active `ProjectSoullike` prototype.

## Included

- Idle, chase, attack, and dead states
- Target acquisition and facing
- NavMeshAgent chase with a direct-movement fallback when no NavMesh is baked
- Timed melee damage using the active `ProjectSoullike.DamageInfo`
- Existing `GolemHealth` death integration

## Deliberately excluded

- The source project's duplicate `IDamageable` and `DamageInfo`
- The source project's duplicate `BossHealth`
- `BossDamageDebug` and the sample scene
- Render-pipeline settings, generated Library files, and project files

The source project used Unity `6000.5.2f1`; only C# logic was ported so the active
Unity `6000.3.21f1` project does not need to import newer scene or settings data.
