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
4. `Assets/Scenes/SampleScene.unity`를 실행합니다.

```powershell
git lfs install
git clone https://github.com/WhySongSerious/ProjectSoullike.git
cd ProjectSoullike
git lfs pull
```

## 현재 상태

- Mechanic Girl 플레이어 후보 캐릭터 배치
- Stone Golem 적 캐릭터 배치
- URP 테스트 씬 구성
- 기본 Input System 액션 에셋 적용

현재는 에셋 검증 단계이며 플레이어 조작, 카메라, 락온, 전투, 스태미나, 적 AI는 아직 구현되지 않았습니다.

## 개발 로드맵

1. DualSense 중심 입력 액션 재설계
2. 3인칭 이동과 카메라 구현
3. 락온, 스태미나, 회피 구현
4. 공격, 피격, 가드, 패링 구현
5. Stone Golem 전투 AI 구현
6. 전투 데이터 수집과 적응형 난이도 실험

## 저장소 정책

- `main`은 실행 가능한 상태를 유지합니다.
- 기능 개발은 `feature/*`, 에셋 작업은 `asset/*`, 실험은 `experiment/*` 브랜치에서 진행합니다.
- Unity 에셋과 대응하는 `.meta` 파일은 항상 함께 커밋합니다.
- `Library`, `Temp`, `Logs`, `UserSettings`, 빌드 결과물과 원본 에셋 설치 패키지는 커밋하지 않습니다.
- 대용량 바이너리 파일은 Git LFS로 관리합니다.

서드파티 에셋 정보는 [THIRD_PARTY_ASSETS.md](THIRD_PARTY_ASSETS.md)를 참고하세요.
