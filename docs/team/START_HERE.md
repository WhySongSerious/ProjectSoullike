# 팀원 시작 안내

## 15분 안에 확인할 것

1. README에서 역할과 이번 주 스프린트 확인.
2. [팀 허브 #2](https://github.com/WhySongSerious/ProjectSoullike/issues/2)에서 자기 이름의 작업 확인.
3. Unity `6000.3.21f1`, Git, Git LFS 설치 여부 확인.
4. 개발 브랜치 clone, LFS 다운로드, Combat 씬 실행.
5. 실행 화면·오류 유무·주간 가용시간을 [환경 확인 #3](https://github.com/WhySongSerious/ProjectSoullike/issues/3)에 기록.

```powershell
git lfs install
git clone --branch codex/initial-content https://github.com/WhySongSerious/ProjectSoullike.git
cd ProjectSoullike
git lfs pull
```

기존 clone에서는 먼저 `git status`로 작업을 확인합니다. 미커밋 변경이 있으면 담당 작업으로 커밋하거나 별도로 보존한 후 전환합니다. 변경을 버리는 reset/clean 명령은 사용하지 않습니다.

```powershell
git fetch origin
git switch codex/initial-content
git pull --ff-only
git lfs pull
```

Unity Hub에 저장소 루트를 등록하고 `Assets/Scenes/Combat.unity`를 실행합니다. 누락 에셋이면 LFS 다운로드를 확인하고, 컴파일 오류는 Console의 첫 오류와 재현 절차를 이슈에 기록합니다.

## 첫 담당 작업

- **송재혁:** #3 환경·통합 기준 확정 → #6 회피/스태미나.
- **천지민:** 기존 BossController/BossAttackState 검토 → #7 패턴·모션 연결. S0에는 #4 패턴 정의 검토와 #3 실행 확인에 참여.
- **이다윤:** #4 필수 범위/패턴 표 → #8 UI·아트·리소스 명세.

지민·다윤의 GitHub 아이디는 아직 받지 않았으므로 이름은 이슈 제목·본문에 기록했습니다. 팀장이 collaborator로 초대하고 수락한 뒤 해당 이슈 Assignee에 각 계정을 연결합니다. 초대 자체는 이번 정리에서 수행하지 않았습니다.

## 작업 시작부터 완료까지

1. 이슈 완료 기준과 선행 작업을 확인하고 상태를 Ready → In progress로 변경.
2. 기준 브랜치에서 `feature/<이슈번호>-<주제>` 또는 `asset/<이슈번호>-<주제>` 생성.
3. 변경 파일과 검증 결과를 커밋·push하고 PR 생성.
4. **PR base는 현재 `codex/initial-content`**. PR #1이 병합된 뒤에만 팀 공지로 main 기준 전환.
5. 담당자 외 1명 검토 → 병합 → 플레이/산출물 확인 → 이슈 닫기.

기본 브랜치가 아닌 개발 브랜치에 병합하면 `Closes #번호`가 즉시 이슈를 닫지 않을 수 있습니다. 검증 후 실제 이슈 상태를 확인하고 수동으로 완료 처리합니다.

## 충돌 줄이기

- 플레이어 코드·VFX·Animator: 재혁 주 편집, 지민 리뷰.
- 보스 코드·패턴: 지민 주 편집, 재혁 리뷰.
- 기획·UI 리소스·테스트 문서: 다윤 주 편집, 적용 담당자 리뷰.
- 공용 Combat 인터페이스와 InputActions: 변경 전 두 개발자가 이슈에서 합의.
- Combat 씬은 하루 편집 담당자 1명. 다른 사람은 독립 프리팹·복제 테스트 씬을 사용하고 통합을 요청.
- 에셋 이동 시 .meta 동반. 외부 원본 대신 프로젝트 소유 복제본/변형으로 작업.
