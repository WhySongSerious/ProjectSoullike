# 스크립트 작성 이력과 현재 담당

기준: 2026-10-07, `integration/s1-combat-arena`의 `Assets/Soullike/Scripts` 아래 C# 파일 19개.

**작성 이력**은 이 저장소의 Git 커밋 작성자다. 파일 원저작자나 각 줄의 실제 작성자를 단정하지 않는다. 특히 초기 보스 상태 머신은 다른 팀 프로젝트에서 가져와 재혁이 이 저장소에 맞게 이식한 코드다. **현재 담당**은 이번 전투 데모에서 변경 조율·검증을 이끌 사람으로, 과거 커밋 작성자와 다를 수 있다. 아래 담당은 [팀 역할·스프린트 계획](SPRINT_PLAN.md)에 따른 작업 기준이다.

표의 경로는 모두 `Assets/Soullike/Scripts/`를 기준으로 한다.

| 스크립트 | 저장소 도입 / 후속 수정 커밋 | 현재 담당 | 검토·협의 |
| --- | --- | --- | --- |
| `Combat/IDamageable.cs` | 재혁 `d8d4a43` | 재혁 | 피해 계약 변경 시 지민과 합의 |
| `Player/KeyboardPlayerController.cs` | 재혁 `d8d4a43` | 재혁 | 지민 코드 검토 |
| `Player/PlayerHealth.cs` | 재혁 `d8d4a43` | 재혁 | 보스 피해 연동 시 지민 검토 |
| `Enemies/Golem/GolemHealth.cs` | 재혁 `d8d4a43` | 지민 | 재혁 통합 검토 |
| `Enemies/Golem/Boss/Core/BossController.cs` | 재혁 `d8d4a43` 이식 → 지민 `0867115`, `1a1c01d` | 지민 | 재혁 통합 검토 |
| `Enemies/Golem/Boss/State/BossAttackState.cs` | 재혁 `d8d4a43` 이식 → 지민 `0867115` | 지민 | 재혁 통합 검토 |
| `Enemies/Golem/Boss/State/BossChaseState.cs` | 재혁 `d8d4a43` 이식 → 지민 `0867115` | 지민 | 재혁 통합 검토 |
| `Enemies/Golem/Boss/State/BossDeadState.cs` | 재혁 `d8d4a43` 이식 | 지민 | 재혁 통합 검토 |
| `Enemies/Golem/Boss/State/BossIdleState.cs` | 재혁 `d8d4a43` 이식 | 지민 | 재혁 통합 검토 |
| `Enemies/Golem/Boss/State/BossState.cs` | 재혁 `d8d4a43` 이식 | 지민 | 재혁 통합 검토 |
| `Enemies/Golem/Boss/State/BossStateMachine.cs` | 재혁 `d8d4a43` 이식 | 지민 | 재혁 통합 검토 |
| `Enemies/Golem/Boss/State/BossStateType.cs` | 재혁 `d8d4a43` 이식 | 지민 | 재혁 통합 검토 |
| `Editor/Setup/SoullikeGolemCombatSetup.cs` | 재혁 `d8d4a43` | 지민 | 공용 프리팹·씬 영향은 재혁과 협의 |
| `Editor/Setup/SoullikeCombatAnimationSetup.cs` | 재혁 `e771606` → `d8d4a43` | 재혁 | 보스 Animator 영향은 지민 검토 |
| `Editor/Setup/SoullikeKeyboardControllerSetup.cs` | 재혁 `e771606` → `d8d4a43` | 재혁 | 지민 코드 검토 |
| `Editor/Setup/SoullikePlayerComboSetup.cs` | 재혁 `e771606` → `d8d4a43` | 재혁 | 지민 코드 검토 |
| `Editor/Setup/SoullikePlayerWeaponSetup.cs` | 재혁 `8115507` → `d8d4a43` | 재혁 | 지민 코드 검토 |
| `Editor/Setup/DungeonEnvironmentSetup.cs` | 다윤 `49b0ca8` | 다윤: 배치·아트 기준 / 재혁: 기술 검증 | Unity 임포트·컴파일은 재혁 확인 |
| `Editor/Setup/BossArenaBlockoutBuilder.cs` | 다윤 `49b0ca8` | 다윤: 레벨 배치 / 재혁: 기술 검증 | 지민: 보스 동선·판정 확인 |

## 변경할 때

1. 담당자는 기획·동작 기준과 PR 검증 결과를 확인한다. 실제 구현자는 이슈·PR에 별도로 적는다. 담당자와 구현자가 같을 필요는 없다.
2. 다른 영역의 파일을 바꿀 때는 해당 담당자를 PR에 적고 검토를 요청한다. 공용 피해 계약, 전투 씬, Animator, 보스·플레이어 연결부는 두 개발자가 함께 확인한다.
3. 다윤이 만든 에디터 스크립트의 레벨·시각적 의도는 다윤이 결정하고, Unity API·컴파일·프로젝트 통합은 재혁이 확인한다. 이것이 다윤에게 모든 C# 유지보수를 맡긴다는 뜻은 아니다.
4. 새 스크립트를 추가하거나 담당을 바꾸면 같은 PR에서 이 표를 갱신한다. 정확한 줄 단위 수정 이력은 `git blame -- <파일경로>`, 파일별 커밋은 `git log --follow -- <파일경로>`로 확인한다. 병합 커밋의 작성자는 실제 파일 변경자를 대신하지 않는다.

이 표는 코드 리뷰 연락처와 책임 범위를 보이기 위한 기록이다. GitHub의 자동 리뷰 요청이나 브랜치 보호 설정을 의미하지 않는다.
