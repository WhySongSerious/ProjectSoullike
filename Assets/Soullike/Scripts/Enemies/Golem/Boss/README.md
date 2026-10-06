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

## JIMIN 수호자 공격

`Assets/Scenes/JIMIN.unity`의 `SKM_Golem` 인스턴스에서만
`Use Guardian Patterns`, `Show Prototype Weapon`을 활성화한다.
공용 `Assets/Prefabs/Golem.prefab`, Animator Controller, `Combat.unity`는 수정하지 않는다.
새 패턴을 끄면 기존 단일 거리 공격을 사용한다.

### 기존 코드와 연결

- `BossStateMachine`, `BossState`, `BossStateType`: 기존 Idle → Chase → Attack → Dead 구조 유지.
- `BossIdleState`: 감지 범위에 들어온 대상을 추적한다.
- `BossChaseState`: 공격 범위 진입 시 Attack 전환. 새 패턴에서는 죽은 대상 또는 감지 범위 이탈 시 Idle 복귀.
- `BossAttackState`: 기존 공격 경로 보존. 새 패턴은 Telegraph → Active → Recovery로 진행한다.
- `BossController`: 패턴 수치, 방향 고정, 물리 판정, 임시 시각 표시, 선택적 Animator 상태 연결 담당.
- `BossDeadState`: 이동 정지. Attack.Exit에서 판정/예고를 해제한다.
- `GolemHealth`: 기존 사망 이벤트, 충돌체 해제, Animator 정지, 지연 비활성화 재사용.
- `PlayerHealth`, `DamageInfo`, `IDamageable`: 기존 피해 전달 방식 재사용. 플레이어의 피해 처리 코드는 변경하지 않았다.
- 기존 Editor/Setup 코드는 공용 프리팹과 애니메이션을 설정하므로 이번 작업에서는 변경하지 않았다.

### 기본 패턴 수치

| 공격 | 예고(초) | 판정(초) | 후딜(초) | 피해 |
|---|---:|---:|---:|---:|
| 내려베기 / Overhead | 0.9 | 0.22 | 1.4 | 28 |
| 3연격 1타 / First Slash | 0.4 | 0.16 | 0.12 | 12 |
| 3연격 2타 / Second Slash | 0.12 | 0.16 | 0.15 | 12 |
| 3연격 3타 / Thrust | 0.75 | 0.2 | 1.2 | 32 |

- `Combo Probability`: 패턴 시작 시 3연격 선택 확률. 기본 0.5. 0이면 내려베기만, 1이면 연격만 실행.
- `Third Strike Probability`: 연격의 3타 발생 확률. 기본 1. 0이면 2타로 끝난다.
- 3타를 생략하면 `Two Slash Recovery`(기본 0.9초)를 사용해 반격 시간을 보장한다.
- `Thrust / Telegraph`로 3타 직전 지연 시간을 조정한다.
- 기존 `Attack Damage/Delay/Cooldown`은 기존 공격 전용이다. 새 패턴은 각 타격의 설정을 사용한다.
- 각 타격 예고 시작에 방향을 고정한다. 판정과 후딜 중에는 플레이어를 따라 회전하지 않는다.
- 사거리 이탈만으로 후딜을 취소하지 않는다. 대상 비활성화·교체·사망 시에는 공격을 취소한다.
- 한 타격의 Active 동안 동일 `PlayerHealth`에는 한 번만 피해를 준다. 다음 타격에서 기록을 초기화하며 `AttackIndex`는 1/2/3이다.
- `Hit Center/Hit Size`는 보스 전방 기준 미터 단위 박스다. 모델 스케일과 별개이며 Inspector 선택 시 Gizmos로 확인할 수 있다.
- 판정은 `Attack Hit Mask`에 포함된 비 Trigger 충돌체에 적용한다. 플레이어의 CapsuleCollider/CharacterController를 유지한다.

### Unity에서 확인 및 설정

1. JIMIN을 열고 Play. 두 활성화 옵션은 씬에 설정되어 있어 임시 패턴 확인에 추가 설정이 필요 없다.
2. `SKM_Golem > Boss Controller`에서 확률과 시간, 피해, 판정 박스를 조정한다.
3. NavMesh가 없으면 기존 직선 추적을 사용한다. 장애물 회피가 필요하면 씬에 NavMesh를 Bake한다.
   직선 추적에는 장애물 경로 탐색이나 낙하 방지가 없다.
4. 임시 검은 코드로 움직이는 단순 도형이다. 내려베기 예고 때 검이 붉어지고, 3타 예고 때 머리 앞 붉은 표시가 켜진다.
   골렘 팔·손의 본 애니메이션, 실제 가면 눈 위치, 실제 검의 접지는 최종 모델 작업이 필요하다.
5. 최종 수호자 모델을 넣으면 `Show Prototype Weapon`을 끄고, JIMIN 전용 Animator Controller를 연결한다.
   각 타격의 `Telegraph State / Active State / Recovery State`에 `Base Layer.OverheadTell` 같은 전체 상태 경로를 입력한다.
   빈 값이나 존재하지 않는 상태는 재생하지 않는다. `MoveSpeed` float 파라미터는 기존 이동 연동에 사용한다.
6. 클립을 각 단계의 시간에 맞추고, 후딜 이후 이동/대기로 복귀하는 Animator 전환도 구성한다.
   `Apply Root Motion`은 끈다. 피해 판정은 코드 타이머가 담당하므로 Animation Event로 피해를 추가하지 않는다.
7. `Blade Telegraph`, `Eyes Telegraph`에는 전용 발광 이펙트 자식 오브젝트를 연결한다.
   이 오브젝트들은 예고에만 활성화되므로 보스 본체나 실제 검 전체를 넣지 않는다.
8. 최종 모델의 검 길이·키에 맞게 Hit Center/Size와 Attack Range를 다시 맞춘다.
   프리팹에 Apply하면 다른 씬에도 영향을 주므로 JIMIN 인스턴스 설정으로 유지한다.

### 검증

- `dotnet build Assembly-CSharp.csproj` 통과. 최종 증분 빌드 경고/오류 0개.
- 별도 임시 Unity 프로젝트에서 실제 보스/체력/상태 코드와 Unity Physics, NavMesh를 사용해
  예고 중 무피해, 다중 충돌체/다중 프레임 중복 방지, 옆 회피, 방향 고정,
  사거리 이탈 후 후딜 유지, 3타 지연·피해·인덱스, 3타 생략 후딜,
  예고/타격 중 보스 사망, 대상 비활성화·사망, 재활성화,
  NavMesh 없는 경로와 Bake된 NavMesh의 이동 정지·타격, 임시 검 생성/해제를 포함한 30개 검증을 통과했다.
- JIMIN 외부 GUID 참조가 프로젝트/URP 패키지 에셋으로 해결되는 것을 확인했다.
- 검증 중 발견한 NavMeshAgent의 velocity 설정 후 정지 해제 문제는
  ResetPath → velocity 초기화 → isStopped 설정 순서로 수정했다.
- JIMIN의 실제 카메라에서 보이는 연출과 최종 수호자 리깅/애니메이션은 직접 Play 확인이 필요하다.
