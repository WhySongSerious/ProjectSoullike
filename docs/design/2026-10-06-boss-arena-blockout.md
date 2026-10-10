# 보스 아레나 블록아웃 (#29 확인용)

작성일: 2026-10-06 · 담당: 이다윤 · 관련: [#29 레벨·맵 디자인](https://github.com/WhySongSerious/ProjectSoullike/issues/29), [9/28 확정 서사·S1](2026-09-28-story-and-s1-sprint.md)

공간 크기와 동선, 분위기를 팀이 같이 보기 위한 **확인용 블록아웃**이다. 최종 맵이나 최종 에셋 결정이 아니다.

## 열어 보는 방법

에셋 원본(약 3.2GB)과 씬 파일은 저장소에 올리지 않는다. 배치 데이터를 담은 에디터 스크립트로 각자의 Unity에서 같은 맵을 생성한다.

1. Fab에서 [Dungeon Environment / 135+ Assets](https://www.fab.com/listings/bb39bae4-7f7a-4127-b07e-151cf52db0f6)(무료)의 **Unity** 형식 패키지를 받는다. 확인에 쓴 파일은 `DungeonEnvironment_91Assets_202.unitypackage`(약 3.2GB)다.
2. Unity에서 `Assets > Import Package > Custom Package...`로 가져온다. 서명 없음 경고가 뜨면 `Import Anyway`를 누른다.
3. `Tools > Project Soullike > Dungeon Environment > 1. Set Up Imported Package`를 실행한다.
4. `Tools > Project Soullike > Dungeon Environment > 2. Build Boss Arena Blockout`를 실행한다. `Assets/Scenes/BossArena_Blockout.unity`가 생성되고 열린다.

4번만 실행해도 3번이 필요하면 자동으로 먼저 실행된다. 두 메뉴는 여러 번 실행해도 결과가 같다. 처음 실행할 때는 텍스처 변환 때문에 몇 분 걸린다.

## 설정 스크립트가 하는 일

이 패키지는 언리얼에서 변환된 에셋이라 URP 프로젝트에서 그대로 쓰면 다음 문제가 생긴다.

| 문제 | 증상 | 처리 |
| --- | --- | --- |
| 언리얼 변환 셰이더(`Unreal/*`)와 Built-in `Standard` 머티리얼 | 분홍색으로 렌더링 | URP Lit / Particles Unlit으로 변환. 언리얼 ORM 텍스처(R=AO, G=Roughness, B=Metallic)를 URP 마스크(R=Metallic, G=AO, A=Smoothness)로 재생성 |
| 메시 UV의 V축 반전 | 텍스처가 뒤집힘 | Base Map 타일링 (1, -1), 오프셋 (0, 1) |
| 프리팹 MeshCollider의 메시가 비어 있음 (83개) | 캐릭터가 바닥과 벽을 통과 | 자식의 렌더 메시를 콜라이더에 연결 |
| 거미줄 텍스처에 알파 채널 없음 | 검은 사각형 | 회색값을 알파로 사용, 알파 클립 |

패키지에 함께 들어 있는 `Scripts/Utilities.cs`(UTU 변환 도구)는 프로젝트 안의 **모든 `.obj` 모델 임포트**를 가로채 메시를 다시 만든다. 현재 저장소에는 `.obj`가 없어 영향이 없지만, 나중에 `.obj`를 쓰게 되면 이 파일을 지우거나 FBX로 가져와야 한다.

패키지는 저장소 규칙에 맞춰 `Assets/ThirdParty/Dungeon_Environment`로 옮긴다. GUID는 유지된다.

## 배치 근거

| 결정 | 근거 |
| --- | --- |
| 단일 전투장 24×24m와 입구 복도 4×14m | 9/28 문서 "맵은 보스 전조와 락온을 가리지 않는 단일 공간". 넓은 탐험 맵은 데모 범위 밖. |
| 중앙 약 16m를 비우고 소품은 벽 쪽에만 배치 | 회피 방향과 보스 모션 시야 확보. 기둥 없음. |
| 보스 시작 위치를 입구에서 약 20m 거리에 배치 | 현재 `BossController.detectionRange`가 14m라 플레이어가 몇 걸음 들어와야 보스가 반응한다. 락온 범위는 18m. |
| 천장은 벽 쪽 4m만 무너진 지붕, 중앙은 개방 | 플레이어 카메라(거리 4.6m, 피벗 1.45m)가 천장과 충돌하지 않게 함. "유적에서 깨어난다"는 서사와 연결. |
| 수련장 소품(무기 거치대, 대검, 창, 벤치) | 보스가 주인공의 스승이었던 수호자라는 확정 서사. 감옥·고문 기구는 쓰지 않음. |
| 청회색 안개와 달빛, 따뜻한 횃불, 보스 위치의 약한 붉은빛 | 확정 색 규칙: 차가운 석재·안개 / 기억의 따뜻한 빛 / 절제된 타락의 붉은빛. |
| 보스 뒤 북쪽 벽의 쇠창살 아치 + 비활성 `VictoryReveal` 조명 | "승리 후 달라지는 장면" 연출용 자리. 승리 시 창살을 치우고 조명을 켜는 식으로 쓸 수 있다. |

`Markers` 아래 `PlayerSpawn`(복도 끝)과 `BossSpawn`(아레나 북쪽, 입구 방향)은 캐릭터를 배치할 기준점이다.

## 확인한 것과 남은 것

- 확인함: Unity 6000.6에서 원본 패키지를 새로 가져온 상태부터 1→2번 메뉴를 실행했다. 원래 블록아웃과 프리팹 329개의 위치·크기가 모두 같았다. 바닥 전체 충돌, 입구 통과 가능, 봉인문 통과 불가를 확인했다.
- 확인함: 에셋을 가져온 상태에서 플레이어(Windows) 스크립트 컴파일이 통과한다. 패키지의 `TreeInstanceComponent.cs`에 `using UnityEditor;`가 있지만, URP 런타임이 `UnityEditor.Rendering.Universal` 네임스페이스를 포함하므로 URP 프로젝트에서는 빌드 오류가 나지 않는다.
- 미확인: 팀 기준 버전 6000.3.21에서의 실행. 사용한 API는 6000.3에도 있는 것만 썼다.
- 미정: 실제 플레이어·골렘을 배치했을 때의 체감 크기, 최종 맵 에셋 선택(#30).
