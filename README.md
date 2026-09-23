# ProjectSoullike · 팀 개발 허브

Unity 6 기반 3D 소울라이크 전투 프로토타입입니다. **송재혁·천지민·이다윤 3명이 11월 23일까지 머신러닝 기반 동적 난이도를 포함한 전투 데모를 완성**합니다.

> 개발 코드 기준: **`codex/initial-content`** · 코드 확인 기준 커밋: `d8d4a43`
> 현재 `main`은 팀 안내 진입점입니다. 게임 실행은 아래 명령으로 개발 브랜치를 받으세요.
> 목표일 **2026-11-23** · 머신러닝 기반 동적 난이도 **필수** · 휴대폰 웹 컨트롤러 **시간 여유 시 선택**.

## 처음 오셨나요?

1. [팀 시작 안내](docs/team/START_HERE.md)를 읽고 프로젝트를 실행합니다.
2. [역할 분담·스크럼·주간 일정](docs/team/SPRINT_PLAN.md)을 확인합니다.
3. [팀 작업 허브 #2](https://github.com/WhySongSerious/ProjectSoullike/issues/2)에서 자기 이름의 이슈를 선택합니다.
4. [전체 작업 목록](docs/team/BACKLOG.md)의 완료 기준에 맞춰 작업하고 PR과 검증 결과를 연결합니다.

**관리 화면:** [팀 허브](https://github.com/WhySongSerious/ProjectSoullike/issues/2) · [열린 작업](https://github.com/WhySongSerious/ProjectSoullike/issues?q=is%3Aissue+is%3Aopen) · [GitHub 관리 방법](docs/team/GITHUB_WORKFLOW.md)

## 업무 분담

| 팀원 | 주 담당 | 주간 결과물 |
| --- | --- | --- |
| 송재혁 · 팀장 | 우선순위·통합·플레이어 전투·VFX·애니메이션·ML 총괄 | 데이터 수집 구조, 학습 모델, Unity 추론·통합 |
| 천지민 | 보스 AI·판정·난이도 조절 연동·RC 검증 | 골렘 전투, 조절 API, 회귀 검증 |
| 이다윤 | 기획 및 아트·UI·수집/평가 운영 | 라벨 기준, 플레이 데이터 수집, 난이도 평가 |

별도 아트 전담자는 없습니다. 다윤은 기획과 아트 방향·리소스를 맡고, 재혁은 VFX와 애니메이션 적용을 맡습니다. 세부 배정은 팀의 가용시간에 맞춰 주간 계획에서 조정합니다.

## 이번 목표와 선택 기능

- **필수:** 이동·카메라·락온·공격에 회피/스태미나/피격을 연결하고, 보스 전투·사망·재시작·승리까지 완성합니다.
- **필수:** 키보드·마우스와 DualSense 조작, 핵심 VFX/애니메이션, UI/안내, 최종 시연 검증.
- **선택:** [QR 모바일 웹 컨트롤러 #5](https://github.com/WhySongSerious/ProjectSoullike/issues/5)는 **일정 여유가 있을 때 진행**합니다. 현재 구현 기능이나 필수 납품 항목이 아닙니다.
- **필수:** [ML 동적 난이도 작업 #21~#27](docs/team/BACKLOG.md). 송재혁이 데이터 수집 파이프라인·학습·평가·Unity 적용을 주도합니다.
- **후순위:** 가드·패링은 필수 품질을 확보한 뒤 선택합니다.

## 개발 환경과 실행

Unity `6000.3.21f1` / URP `17.3.0` / Input System `1.20.0` / AI Navigation `2.0.14`

```powershell
git lfs install
git clone --branch codex/initial-content https://github.com/WhySongSerious/ProjectSoullike.git
cd ProjectSoullike
git lfs pull
```

Unity Hub에서 저장소 루트를 연 뒤 `Assets/Scenes/Combat.unity`를 실행합니다. 기존 clone 사용자는 미커밋 작업을 보존한 뒤 [시작 안내](docs/team/START_HERE.md)를 따르세요.

## 현재 구현 근거

| 항목 | 코드·문서에서 확인한 상태 |
| --- | --- |
| 캐릭터 | Mechanic Girl·검·Stone Golem 배치 |
| 플레이어 | 3인칭 이동·달리기·카메라 충돌·3단 콤보·락온 |
| 입력 | 키보드/마우스·DualSense Input System 액션 |
| 전투 | 공격별 피해·플레이어/골렘 임시 체력 UI·사망 처리 |
| 보스 | Idle/Chase/Attack/Dead 상태 머신·추적·근접 피해 |
| 남은 필수 | 회피·스태미나·피격 연출·보스 전용 모션·재시작/승리 흐름·ML 동적 난이도·검수 |

이는 기준 브랜치의 소스/README 확인 결과입니다. 이번 정리 작업에서 Unity 플레이·빌드를 새로 검증한 것은 아닙니다. 별도 패링 실험 브랜치는 기본 구현 완료로 집계하지 않습니다.

## 기본 조작

| 동작 | 키보드·마우스 | DualSense |
| --- | --- | --- |
| 이동 | WASD | 왼쪽 스틱 |
| 카메라 | 마우스 | 오른쪽 스틱 |
| 달리기 | Shift | L3 |
| 공격 | 마우스 왼쪽 | R1 |
| 락온 | Q / 휠 클릭 | R3 |
| 카메라 초기화 | R | — |

회피 등 신규 입력은 구현 후 이 표를 갱신합니다.

## 이번 주 서사·디자인 협의

주인공·보스·이야기·공간 연출은 [S0 비동기 협의안](docs/design/2026-09-24-direction-workshop.md)에 따라 이번 주 팀이 확정합니다. [마지막 수문](docs/design/2026-09-23-narrative-visual-direction.md)은 현재 캐릭터를 활용하는 미확정 후보이며, [기획 기준안](docs/design/2026-09-23-game-direction.md)은 전투 범위와 일정상의 경계를 정리합니다.

## 개발 규칙

- 코드: `Assets/Soullike/Scripts`의 Combat·Player·Enemies·Editor/Setup 구조를 유지합니다.
- 에셋과 `.meta`를 함께 커밋합니다. 외부 에셋 원본은 직접 수정하지 않습니다.
- 씬·Animator·프리팹은 작업 전 이슈에서 편집자를 지정합니다.
- `Library/Temp/Logs/UserSettings`·빌드 산출물·원본 설치 패키지는 커밋하지 않습니다.
- 대용량 바이너리는 Git LFS를 사용합니다.
- [개발 브랜치 AGENTS.md](https://github.com/WhySongSerious/ProjectSoullike/blob/codex/initial-content/AGENTS.md)와 [외부 에셋 목록](https://github.com/WhySongSerious/ProjectSoullike/blob/codex/initial-content/THIRD_PARTY_ASSETS.md)을 따릅니다.

[현재 코드](https://github.com/WhySongSerious/ProjectSoullike/tree/codex/initial-content) · [초기 코드 통합 PR #1](https://github.com/WhySongSerious/ProjectSoullike/pull/1) · [팀장 전달문](docs/team/TEAM_BRIEF.md)
