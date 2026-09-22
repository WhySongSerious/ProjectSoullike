# 초기 백로그

2026-09-22 계획 초안. 일정·SP는 첫 주 가용시간에 맞춰 조정합니다. 실제 진행 상태는 GitHub 이슈를 확인합니다.

| ID | 목표 | 우선순위 | 담당 | SP 초안 | 작업 | 선행 |
| --- | --- | --- | --- | --- | --- | --- |
| SETUP | S0 | P0 | 송재혁 | 2 | [#3 개발 기준 브랜치와 팀원 실행 환경 확정](https://github.com/WhySongSerious/ProjectSoullike/issues/3) | — |
| SPEC | S0 | P0 | 이다윤 | 2 | [#4 데모 범위·보스 패턴·테스트 기준 확정](https://github.com/WhySongSerious/ProjectSoullike/issues/4) | — |
| SPIKE | BACKLOG | P2 | 미정 | 미산정 | [#5 여유 시 진행: QR 모바일 웹 컨트롤러](https://github.com/WhySongSerious/ProjectSoullike/issues/5) | #15 |
| DODGE | S1 | P0 | 송재혁 | 5 | [#6 회피·스태미나 최소 전투 루프 구현](https://github.com/WhySongSerious/ProjectSoullike/issues/6) | #3, #4 |
| BOSS | S1 | P0 | 천지민 | 5 | [#7 골렘 공격 예고·판정·후딜과 모션 연결](https://github.com/WhySongSerious/ProjectSoullike/issues/7) | #4 |
| ART | S1 | P0 | 이다윤 | 3 | [#8 전투 가독성·UI·아트 리소스 명세](https://github.com/WhySongSerious/ProjectSoullike/issues/8) | #4 |
| LOOP | S2 | P0 | 송재혁 | 5 | [#9 피격·사망·재시작·승리 흐름 완성](https://github.com/WhySongSerious/ProjectSoullike/issues/9) | #6, #7 |
| BOSSLOOP | S2 | P0 | 천지민 | 3 | [#10 보스 전투 리셋·패턴 예외 처리 통합](https://github.com/WhySongSerious/ProjectSoullike/issues/10) | #7 |
| PLAYTEST | S2 | P0 | 이다윤 | 3 | [#11 첫 통합 플레이테스트·밸런스 조정안](https://github.com/WhySongSerious/ProjectSoullike/issues/11) | #9, #7 |
| VFX | S3 | P0 | 송재혁 | 5 | [#12 핵심 타격 VFX·애니메이션 전환 완성](https://github.com/WhySongSerious/ProjectSoullike/issues/12) | #9, #8, #11 |
| TUNE | S3 | P0 | 천지민 | 3 | [#13 보스 난이도·전투 오류 개선](https://github.com/WhySongSerious/ProjectSoullike/issues/13) | #11 |
| HUD | S3 | P0 | 이다윤 | 3 | [#14 HUD·조작 안내·시연 동선 최종 검수](https://github.com/WhySongSerious/ProjectSoullike/issues/14) | #8, #11 |
| RC | S4 | P0 | 송재혁 | 3 | [#15 릴리스 후보 빌드·성능·회귀 검증](https://github.com/WhySongSerious/ProjectSoullike/issues/15) | #12, #13, #14 |
| POLISH | S5 | P0 | 송재혁 | 3 | [#16 최종 전투 연출·빌드 안정화](https://github.com/WhySongSerious/ProjectSoullike/issues/16) | #15 |
| REGRESS | S5 | P0 | 천지민 | 3 | [#17 전투·시연 차단 오류 수정 및 재검증](https://github.com/WhySongSerious/ProjectSoullike/issues/17) | #15 |
| DEMO | S5 | P0 | 이다윤 | 3 | [#18 시연 대본·최종 QA·알려진 문제 정리](https://github.com/WhySongSerious/ProjectSoullike/issues/18) | #15 |
| RELEASE | S6 | P0 | 송재혁 | 2 | [#19 11월 초 최종 납품·공유 체크포인트](https://github.com/WhySongSerious/ProjectSoullike/issues/19) | #17, #18 |
| OPTION | BACKLOG | P2 | 천지민 | 3 | [#20 가드·패링·적응형 난이도 후속 범위 검토](https://github.com/WhySongSerious/ProjectSoullike/issues/20) | #15 |

각 이슈에 구체적인 완료 체크리스트가 있습니다. 필수 16개, 선택 2개이며 허브 이슈는 작업 수에서 제외합니다.

QR 모바일 컨트롤러는 핵심 전투 검증 후 여유가 있으면 진행합니다. 착수 전 담당·추정·실기기 검증 기준을 정하며 기본 필수 작업량에 포함하지 않습니다. 패링 실험 브랜치는 병합 전 확인 대상이고 완료된 기능으로 집계하지 않습니다.
