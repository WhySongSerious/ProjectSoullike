# GitHub 작업 관리

## 지금 사용 가능한 인터페이스

[팀 허브 #2](https://github.com/WhySongSerious/ProjectSoullike/issues/2)에서 스프린트별 이슈를 열고 진행 상황을 갱신합니다. 실시간 완료 상태는 각 이슈의 Open/Closed를 기준으로 하며 문서 표는 계획입니다.

| 화면 | 용도 |
| --- | --- |
| [전체 작업](https://github.com/WhySongSerious/ProjectSoullike/issues?q=is%3Aissue+is%3Aopen) | 열려 있는 작업 확인 |
| [송재혁](https://github.com/WhySongSerious/ProjectSoullike/issues?q=is%3Aissue%20is%3Aopen%20%22%EC%86%A1%EC%9E%AC%ED%98%81%22%20in%3Atitle) | 팀장·플레이어·VFX·애니메이션 |
| [천지민](https://github.com/WhySongSerious/ProjectSoullike/issues?q=is%3Aissue%20is%3Aopen%20%22%EC%B2%9C%EC%A7%80%EB%AF%BC%22%20in%3Atitle) | 보스·전투 안정화 |
| [이다윤](https://github.com/WhySongSerious/ProjectSoullike/issues?q=is%3Aissue%20is%3Aopen%20%22%EC%9D%B4%EB%8B%A4%EC%9C%A4%22%20in%3Atitle) | 기획·아트·QA |
| [선택 백로그](https://github.com/WhySongSerious/ProjectSoullike/issues?q=is%3Aissue%20is%3Aopen%20%22%5BBACKLOG%5D%22%20in%3Atitle) | QR 컨트롤러 등 여유 시 검토 |

이슈 제목은 [S번호][우선순위][담당자]로 시작합니다. 담당 계정이 초대되면 Assignee를 연결합니다. 재혁 계정은 WhySongSerious로 연결했고 지민·다윤은 이름만 기록한 상태입니다.

## 이슈 사용 규칙

1. 주간 계획에서 이슈 본문 상태를 Ready로 변경.
2. 착수 시 In progress와 시작일 기록.
3. 검토 요청 시 Review와 PR/영상/문서 링크 기록.
4. 완료 기준·검증·리뷰를 만족하면 이슈 닫기.
5. 미완료 이슈는 그대로 다음 스프린트로 옮기고 제목의 S번호·선행 링크를 갱신.

이슈의 본문 상태와 Open/Closed가 다르면 정리합니다. 팀 허브의 체크박스는 주간 리뷰 때 실제 종료 상태에 맞춰 갱신합니다. 자동 동기화 보드로 표시하지 않습니다.

## 드래그형 GitHub Projects 보드 설정

GitHub Projects는 이슈와 PR을 표·보드·로드맵으로 관리합니다. [공식 설명](https://docs.github.com/en/issues/planning-and-tracking-with-projects/learning-about-projects/about-projects)

**현재 생성되지 않았습니다.** 연결된 도구에는 Projects 생성 기능이 없고, 브라우저는 GitHub 로그아웃 상태입니다. 지금 생성한 이슈는 로그인 후 아래 보드에 그대로 추가할 수 있습니다.

1. 저장소 Projects → 새 프로젝트 생성 또는 계정의 새 Project 생성 후 저장소 연결.
2. 이름: ProjectSoullike · 11월 데모. 팀 접근 범위에 맞게 권한 설정.
3. 기존 이슈 #3~#27을 추가. #2는 안내 허브이므로 작업량 집계에서는 제외.
4. Status: Backlog / Ready / In progress / Review / Done.
5. 필드: Assignees, Priority(P0/P1/P2), Sprint(S0~S8/마감/Backlog), Estimate(숫자), Blocked(선택).
6. 보기: 전체 백로그(표), 이번 주(보드·Sprint 필터), 담당자별(Assignees 그룹), 일정(로드맵).
7. 이슈 닫힘→Done 자동화를 켜고, 새 이슈 자동 추가 범위를 이 저장소로 제한.
8. 보드 URL을 README와 #2에 추가.

11/23 마감이며 ML 동적 난이도 #21~#27은 필수, 휴대폰 컨트롤러 #5는 여유 시 선택입니다. 11/16 새 기능 동결을 기준으로 운영합니다.

P0는 이번 데모 필수, P1은 개선, P2는 선택 기능입니다. 기능 우선순위와 버그 심각도를 혼동하지 않습니다. 크래시·진행 불가는 우선 수정하고, 시각적 미세 조정은 마감 버퍼를 해치지 않을 때 처리합니다.

## PR와 공유

- 현재 기능 PR base: codex/initial-content. PR #1은 아직 Draft이며 이번 정리로 병합하지 않았습니다.
- 1개 PR에 1개 논리적 변경. 이슈 번호, 변경 이유, 검증, 씬/프리팹 영향 기록.
- 프로젝트 규칙·에셋 출처는 개발 브랜치 문서 참고.
- 팀장은 저장소 Settings → Collaborators에서 팀원을 초대하고 수락 후 Assignee 연결.
- GitHub Projects는 저장소 collaborator와 별도 접근 설정을 확인해야 할 수 있습니다.
- 외부 에셋 문서의 “비공개 유지” 문구와 현재 저장소 공개 설정이 일치하지 않습니다. #3에서 기존 공개 설정과 에셋 제공 범위를 확인합니다. 이번 문서 작업은 공개 범위를 변경하지 않았습니다.
