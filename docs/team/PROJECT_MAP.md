# 프로젝트 파일 지도

기준: 2026-10-07, `integration/s1-combat-arena`. **Git 도입·수정자**는 커밋 기록이며 원저작자를 단정하지 않는다. **현재 담당**은 팀의 수정·검토 연락처다. 스크립트 19개의 개별 이력은 [스크립트 담당표](SCRIPT_OWNERSHIP.md)를 본다.

저장소 최상위에서는 `Assets/`가 게임 내용, `Packages/`가 Unity 패키지 목록, `ProjectSettings/`가 프로젝트 설정이다. `docs/`는 기획·팀 운영 기록이고 `.github/`는 이슈·PR 양식이다. `Library/`, `Temp/`, `Logs/`는 Unity가 생성하므로 Git에서 제외한다.

## Unity에서 먼저 찾을 것

| 찾는 것 | 위치 | 용도 | Git 이력 / 현재 담당 |
| --- | --- | --- | --- |
| 기본 전투 씬 | `Assets/Scenes/Combat.unity` | 플레이어·골렘이 들어 있는 기존 전투 프로토타입 | 재혁 도입 / 재혁 통합 |
| 지민 보스 실험 씬 | `Assets/Scenes/JIMIN.unity` | 수호자 공격 전조·연격 시험 | 지민 수정 / 지민 보스, 재혁 통합 |
| 다윤 맵 블록아웃 | `Assets/Scenes/BossArena_Blockout.unity` | 유적 아레나 배치 확인. 플레이어·보스 전투 연결 전 | 다윤의 배치 스크립트로 생성 / 다윤 레벨, 재혁 기술 검증 |
| 플레이어 프리팹 | `Assets/Prefabs/Mechanic_Girl.prefab` | 외부 모델을 프로젝트 전투 코드와 연결 | 재혁 도입 / 재혁 |
| 골렘 프리팹 | `Assets/Prefabs/Golem.prefab` | 기존 공용 보스 프로토타입. 지민 전용 설정은 `JIMIN.unity` 인스턴스에 있음 | 재혁 도입 / 재혁 공용 프리팹, 지민 보스 동작 협의 |
| 플레이어 전투 애니메이션 | `Assets/Soullike/Animations/` | 프로젝트에서 만든 Animator·클립 연결 | 재혁 도입 / 재혁 |
| 맵 밤하늘 머티리얼 | `Assets/Soullike/Materials/BossArena_NightSky.mat` | 맵 생성기가 만든 프로젝트 소유 머티리얼 | 다윤 배치 스크립트에서 생성 / 다윤 시각 기준, 재혁 기술 검증 |
| 프로젝트 코드 | `Assets/Soullike/Scripts/` | `Player`, `Combat`, `Enemies`, `Editor/Setup`으로 분리 | [파일별 작성 이력·담당](SCRIPT_OWNERSHIP.md) |
| 입력·렌더링 설정 | `Assets/InputSystem_Actions.inputactions`, `Assets/Settings/` | Input System 액션과 URP 설정 | 재혁 초기 도입 / 재혁 |

`Assets/Readme.asset`은 Unity 프로젝트 템플릿 안내 파일이며 게임 내용이 아니다.

`Assets/Soullike/Scripts/Editor/Setup/BossArenaBlockoutBuilder.cs`가 다윤의 배치 수치를 담고 있다. `DungeonEnvironmentSetup.cs`는 임포트한 외부 에셋을 URP에 맞게 설정한다. 맵 씬은 Git에 있어도 **외부 Dungeon Environment 에셋을 임포트하고 설정해야 정상으로 보인다.** [맵 설치·재생성 절차](../design/2026-10-06-boss-arena-blockout.md)를 따른다.

## 외부 에셋과 문서

| 위치 | 무엇인가 | 관리 원칙 |
| --- | --- | --- |
| `Assets/ThirdParty/IdaFaber/` | Fab Mechanic Girl 원본 | 벤더 파일 구조 유지. 재혁이 저장소에 도입 |
| `Assets/ThirdParty/Stone_Golem/` | Stone Golem 원본 | 벤더 파일 구조 유지. 재혁이 저장소에 도입 |
| `Assets/ThirdParty/TC_Sword/` | 검 모델·애니메이션 원본 | 벤더 파일 구조 유지. 재혁이 저장소에 도입 |
| `Assets/ThirdParty/Dungeon_Environment/` | Fab 던전 맵 원본, 약 4GB | 각자 임포트. Git 제외. 다윤의 맵 배치가 이 프리팹들을 참조 |
| `docs/design/` | 서사·레벨·전투 기획 및 결정 기록 | 해당 기획 담당이 내용 검수 |
| `docs/team/` | 일정·역할·작업 안내 | 재혁 통합 관리, 각 담당이 자기 항목 확인 |
| `THIRD_PARTY_ASSETS.md` | 외부 에셋 출처·라이선스 상태 | 새 에셋을 들일 때 함께 갱신 |

다운로드한 `.unitypackage`와 작업 원본은 Unity 프로젝트 밖 `../SourceAssets`에 둔다. `Assets/Soullike/Gun`은 출처가 확정되기 전까지 임의로 분류하지 않는다.

## 새 파일을 넣을 때

- 팀이 만든 씬은 `Assets/Scenes`, 공용 프리팹은 `Assets/Prefabs`, 코드·애니메이션·머티리얼은 `Assets/Soullike`에 둔다.
- 구매·다운로드한 Unity 에셋은 `Assets/ThirdParty/<에셋명>`에 묶고 벤더 내부 폴더명은 유지한다.
- 씬·프리팹의 주 편집자와 검토자를 이슈/PR에 적는다. 폴더 위치만으로 만든 사람을 추측하지 않는다.
- 에셋을 옮길 때 `.meta`를 함께 옮겨 GUID를 보존한다. 경로를 문자열로 사용하는 에디터 코드와 빌드 설정도 확인한다.
