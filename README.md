# ProjectSoullike

Unity 6 기반의 3D 소울라이크 액션 게임 프로토타입입니다. 콘솔 플레이와 DualSense 입력을 우선 고려하고, 전투 시스템이 안정된 뒤 플레이 데이터 기반 적응형 난이도 조절을 실험하는 것을 목표로 합니다.

## 개발 환경

- Unity `6000.3.21f1`
- Universal Render Pipeline `17.3.0`
- Input System `1.20.0`
- AI Navigation `2.0.14`

## 프로젝트 실행

1. Git LFS를 설치하고 초기화합니다.
2. 저장소를 clone한 뒤 LFS 에셋을 내려받습니다.
3. Unity Hub에서 저장소 루트를 프로젝트로 엽니다.
4. `Assets/Scenes/Combat.unity`를 실행합니다.

```powershell
git lfs install
git clone https://github.com/WhySongSerious/ProjectSoullike.git
cd ProjectSoullike
git lfs pull
```

## 현재 상태

- Mechanic Girl 플레이어 캐릭터와 Tony Sword 배치
- Stone Golem 적 캐릭터 배치
- 3인칭 이동, 카메라 충돌, 달리기 구현
- 마우스 및 DualSense R1 기반 3단 공격 콤보 구현
- 키보드·마우스와 DualSense를 지원하는 Input System 액션 적용
- 3단 공격 판정과 공격별 피해량 구현
- Stone Golem 임시 체력, 머리 위 체력바, 사망 처리 구현
- Stone Golem 상태 머신 기반 추적·공격·사망 로직 병합
- 플레이어 임시 체력과 체력바 구현
- 화면 중심 우선 대상 선택과 자동 해제를 포함한 락온 구현

현재는 전투 프로토타입 단계이며 피격 애니메이션, 회피, 스태미나와 보스 전용 모션 연결은 아직 구현되지 않았습니다.

## 기본 조작

| 동작 | 키보드·마우스 | DualSense |
| --- | --- | --- |
| 이동 | `WASD` | 왼쪽 스틱 |
| 카메라 | 마우스 | 오른쪽 스틱 |
| 달리기 | `Shift` | `L3` |
| 공격 | 마우스 왼쪽 버튼 | `R1` |
| 락온 전환 | `Q` 또는 마우스 휠 클릭 | `R3` |
| 카메라 초기화 | `R` | - |

## 개발 로드맵

1. DualSense 중심 입력 액션 재설계
2. 3인칭 이동과 카메라 구현
3. 락온, 스태미나, 회피 구현
4. 공격, 피격, 가드, 패링 구현
5. Stone Golem 전투 AI 구현
6. 전투 데이터 수집과 적응형 난이도 실험

## 코드 폴더 규칙

- 프로젝트 C# 코드는 `Assets/Soullike/Scripts` 아래에서 관리합니다.
- 공용 전투 규약은 `Combat`, 플레이어 코드는 `Player`, 적 코드는 `Enemies/<적 이름>`으로 구분합니다.
- Unity 에디터에서만 실행되는 설정 도구는 `Scripts/Editor/Setup`에 둡니다.
- Unity에서 사용하는 외부 에셋은 `Assets/ThirdParty/<에셋 이름>` 아래에서 원본 구조를 유지합니다.
- 원본 다운로드와 설치 패키지는 Unity 프로젝트 밖의 `../SourceAssets`에서 관리합니다.
- 파일을 이동할 때 Unity 참조가 유지되도록 대응하는 `.meta` 파일도 함께 이동합니다.

## 저장소 정책

- `main`은 실행 가능한 상태를 유지합니다.
- 기능 개발은 `feature/*`, 에셋 작업은 `asset/*`, 실험은 `experiment/*` 브랜치에서 진행합니다.
- Unity 에셋과 대응하는 `.meta` 파일은 항상 함께 커밋합니다.
- `Library`, `Temp`, `Logs`, `UserSettings`, 빌드 결과물과 원본 에셋 설치 패키지는 커밋하지 않습니다.
- 대용량 바이너리 파일은 Git LFS로 관리합니다.

서드파티 에셋 정보는 [THIRD_PARTY_ASSETS.md](THIRD_PARTY_ASSETS.md)를 참고하세요.
