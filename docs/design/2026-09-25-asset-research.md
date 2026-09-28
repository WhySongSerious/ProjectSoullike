# S0 에셋 리서치 — 현재 프로젝트와 서사 후보 기준

조사일: 2026-09-25. 상품 설명·공식 판매 페이지와 저장소를 확인한 결과이며, 다운로드·구매·Unity 임포트 검증은 하지 않았다. 가격은 USD 표시가이며 할인·세금·팀 사용 라이선스에 따라 최종 금액이 달라진다. 사용자는 소액 유료 구매를 허용했으나 정확한 예산 상한은 미정이다.

## 1. 추천 방향

**기존 플레이어와 검 콤보를 개발용으로 유지하고, 부족한 회피·피격·사망 모션 및 보스전 공간부터 보충한다.** 최종 캐릭터는 이번 주 서사·미술 합의로 교체할 수 있다.

- 이다윤의 기억 회복·유적 수호자 안을 기획 검토의 중심에 두고, 천지민의 몰락한 왕국·타락한 수호자 안과 연결 가능한 외형을 비교한다. 통합 서사는 아직 확정되지 않았다.
- 한 개 전투장으로 범위를 제한한다. 청회색 석재, 따뜻한 기억 파편, 필요할 때 붉은 타락 표현을 사용하는 연출안을 추천한다.
- 기억 회복을 성장 시스템으로 확장하지 않는다. 패턴 학습, 전투 전후 짧은 문장, 승리 후 파편 연출로 먼저 표현한다.
- 큰 성 전체, 추가 무기, 다수 보스보다 읽기 쉬운 공격과 빠른 재도전에 예산을 배정한다. ML 동적 난이도 구현 일정도 확보해야 한다.

## 2. 실제 프로젝트에서 재사용할 것

| 항목 | 확인한 상태 | 선택에 주는 영향 |
| --- | --- | --- |
| 개발 환경 | Unity 6000.3.21f1 / URP 17.3.0 | Built-in 전용 재질은 변환 작업이 필요하며, URP 표기도 실제 프로젝트 검증을 대신하지 못함 |
| 플레이어 | Mechanic Girl, 검, 이동·달리기·3단 콤보·락온 | 새 이동/기본 검 콤보 팩은 우선순위 낮음 |
| TC Sword | idle/walk/run/검 콤보, in-place 변형을 기존 설정 코드에서 사용 | 신규 모션도 기존 이동 제어와 맞는지 확인 |
| 부족한 동작 | 회피·피격 연출·보스 전용 모션, 재시작/승리 흐름 | 모션 보충과 전투 흐름 구현을 우선 |
| 보스 | Stone Golem, 추적·공격 상태, 시간/거리 중심 임시 피해 판정 | 모델을 바꿔도 공격 전조·실제 타격 구간·판정 작업은 필요 |
| 리그 | Golem 및 TC Sword FBX 메타에 animationType: 3 | Humanoid 설정은 확인했으나 Avatar 유효성·리타기팅 품질은 플레이 검증 필요 |

근거: `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`, `Assets/ThirdParty`, `SoullikePlayerComboSetup.cs`, `BossAttackState.cs`, README. 보스 교체 시 Golem 전용 체력 참조와 락온 연결도 점검한다.

## 3. 우선 검토 후보

| 후보 / 공식 출처 | 확인 가격 | 프로젝트 적합성과 남은 확인 |
| --- | --- | --- |
| [Human Melee Animations · Kevin Iglesias](https://assetstore.unity.com/packages/3d/animations/human-melee-animations-151650) | 할인 $6.90 / 정가 $23 | **유료 1순위 후보.** Unity 6000.0.59f2 URP 호환 표기. 제작자 목록에 dodge·피격·사망이 있어 현재 빈 부분 보충 가능. dodge가 원하는 구르기인지는 영상 확인 필요 |
| [Dungeon Environment / 135+ Assets · PackDev](https://www.fab.com/listings/bb39bae4-7f7a-4127-b07e-151cf52db0f6) | 무료 | **배경 1차 검증 후보.** PBR 던전, Unity 형식 제공. 봉인된 유적 공간에 검토할 만함. 무료 Fab 패키지의 URP 구성은 미확인 |
| [Simple Modular Dungeon · DanProps](https://assetstore.unity.com/packages/3d/environments/dungeons/simple-modular-dungeon-259641) | $14.99 | **배경 유료 대안.** Unity 2022.3.49f1 URP 호환 표기. 무료 후보 재질 변환에 시간이 많이 들면 비교. Unity 6 실제 적용은 미검증 |
| [Crimson Knight Warrior 무료판 · PolyReady](https://www.fab.com/listings/1fc0baa6-fc4f-487b-92e2-0d369709d819) | 무료 샘플 / 완전판 가격 미확인 | **타락한 기사형 수호자 검토.** 무료판은 idle만 제공. 리그·재질·실루엣 테스트용이며 전투 완성품 아님. Unity 호환 안내는 있으나 URP 재질 미확인 |
| [Dark Knight 02 · Maksim Bugrimov](https://marketplace.unity.com/packages/3d/characters/humanoids/fantasy/dark-knight-02-282418) | 검색 색인 $24.99, 결제 전 재확인 | **기사형 주인공 또는 보스의 외형 대안.** 공식 검색 결과에 Unity 2021.3.15 URP 호환 표기. 상세 페이지 재접속 실패; Humanoid 및 포함 전투 모션은 확인 전 구매 보류 |
| [Ultimate Modular Ruins · Quaternius](https://quaternius.com/packs/ultimatemodularruins.html) | 무료 / CC0 | 90개 모델, FBX 등 제공. 비용 없는 유적 구성 대안. 단순화된 스타일로 갈 때 검토; 사실적인 기사와 혼합 시 통일감 확인 |
| [Universal Animation Library · Quaternius](https://quaternius.itch.io/universal-animation-library) | Standard 무료 / Pro $9.99 / Source $14.99 | CC0, Unity용 Humanoid 리타기팅 및 root-motion/in-place 제공 안내. 무료는 일부 구성으로 전체 120+ 동작이 무료라는 뜻은 아님. 필요한 회피·피격의 등급별 포함 여부 확인 |

### 모션·보스 후보 상세

[Human Melee 제작자 설명](https://kevdev.itch.io/human-melee-animations)에는 피격 2종, 사망 6종, 회피와 여러 무기 공격이 기재되어 있다. 이동 동작 중복이 있지만 부족한 반응 동작을 함께 확보하는 점에서 검토 가치가 있다. [무료판](https://kevdev.itch.io/human-melee-animatons-free)으로 먼저 기존 캐릭터의 리타기팅을 확인한다. 무료판이 유료판의 회피까지 제공한다고 가정하지 않는다.

[Crimson Knight 완전판](https://www.fab.com/listings/5ea4c4eb-3c00-43e7-bfca-a70845d9a137)은 idle 포함 9개 동작(이동, 공격, 스킬 2개, 피격 2개, 사망)을 안내한다. 스킬 모션이 학습 가능한 기본·딜레이·연계 패턴에 적합한지는 미리보기에서 판단해야 한다. 제작자는 텍스처 세부 개선에 AI를 사용했다고 밝힌다.

PackDev의 [Unity URP 전용 상품](https://marketplace.unity.com/packages/3d/props/dungeon-environment-91-assets-urp-292320)은 $49.99이며 Fab 무료 상품과 이름·구성 수가 다르다. 무료판을 유료 URP판과 동일한 패키지로 취급하지 않는다. 현재는 저렴한 대안을 먼저 검토한다.

## 4. 조합별 예산

| 조합 | 표시 가격 합계 | 권장 상황 |
| --- | --- | --- |
| 기존 캐릭터·검 + 무료 배경 + 무료 모션 샘플 | $0 | 이번 주 화면/리그 적합성 확인. 필요한 모든 전투 동작을 확보한 상태는 아님 |
| 기존 캐릭터·검 + Human Melee + 무료 배경 | 할인 $6.90 / 정가 $23 | **현재 우선 추천.** 무료 배경의 URP 적용과 모션 품질이 통과할 때 |
| 기존 캐릭터·검 + Human Melee + Simple Modular Dungeon | 할인 $21.89 / 정가 $37.99 | 무료 배경 적용 비용이 클 때 대안 |
| Dark Knight 02 + Human Melee + 무료 배경 | 할인 기준 $31.89 / 정가 기준 $47.99 | 기사 외형 확정 이후. 캐릭터 가격·리그·모션 확인 전 예산 참고용 |

환율·세금·실제 팀 사용에 필요한 라이선스 비용은 합계에서 제외했다. 할인 종료일은 확인하지 못했으므로 정가도 함께 판단한다. 예산 절감을 위해 새 캐릭터와 새 환경을 동시에 구매할 필요는 없다.

## 5. 이번 주 의사결정과 다음 주 적용

1. **9/25 금 — 이다윤 중심 비동기 선택:** 주인공 외형 유지/기사 교체, 수호자 외형, 공간 후보 각 1순위와 이유를 모은다. 에셋이 세계관을 자동 확정하는 것으로 취급하지 않는다.
2. **9/26 토 — 송재혁 적용 검증:** 무료 모션 샘플 1개와 무료 배경 1개를 별도 시험 공간에서 확인한다. 기존 검 쥠새·발 미끄러짐·모션 전환, URP 재질과 카메라 시야를 본다. 이는 예정 작업이며 이번 조사에서 수행하지 않았다.
3. **9/26 토 — 천지민 패턴 판단:** 보스 후보의 기본 공격·늦게 내려치는 공격·연계 동작을 영상/클립으로 검토하고 채택 가능한 패턴 2개를 정한다. 공격 모션·전조·판정은 난이도별로 유지하고 간격·빈도를 조절한다.
4. **9/27 일 — 방향 확정:** 다윤의 서사/미술안, 지민의 패턴안, 재혁의 적용 결과로 조합과 구매 필요성을 기록한다. 모션이 맞지 않는 후보는 모델 외형이 좋아도 탈락시킨다.
5. **9/28 월 — 정기회의 후 개발:** 확정 후보로 회피·피격·보스 공격 1개·재도전 흐름부터 연결한다. 기억 파편과 색 연출은 이 전투장 안에서 구현한다.

화면 확인 기준: 청회색 배경에서도 공격 전조가 분명하고, 보스 실루엣이 플레이어와 구별되며, 따뜻한 기억 파편이 배경 소품보다 먼저 읽히는지 본다. 안개가 전조를 가리면 밀도를 낮춘다.

## 6. 도입 시 저장 방식

기존 `THIRD_PARTY_ASSETS.md`에서 Stone Golem·TC Sword의 라이선스 출처는 아직 확인 필요로 남아 있다. 해당 문서의 비공개 저장소 전제와 실제 저장소 공개 상태도 대조해야 한다. 제한된 원본 에셋을 공개 Git에 추가하기 전에 팀 공유 방식을 확정한다.

[Fab Standard License](https://www.fab.com/eula)는 협업자와의 비공개 공유를 허용하지만 에셋 자체 재배포를 허용하지 않는다. [Unity EULA](https://unity.com/legal/as-terms)도 구매 유형에 맞춰 적용한다. 무료라는 표시가 원본 공개 재배포 허용을 의미하지는 않는다. CC0 후보는 이 부담이 적다.

도입이 결정되면 출처·구매/취득 계정·라이선스·버전·프로젝트 경로를 외부 에셋 목록에 기록한다. 원본 패키지는 프로젝트 밖 `SourceAssets`, 사용할 파일은 `Assets/ThirdParty/<이름>` 원칙을 따른다.
