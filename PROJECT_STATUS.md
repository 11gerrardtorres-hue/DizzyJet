# Dizzy Jet — 프로젝트 현황 (핸드오프 문서)

> 새 채팅에서 이 파일을 붙여넣으면 맥락을 이어받을 수 있습니다.
> 작성 시점 기준: 코어 게임플레이 + 모바일조작 + 시작화면 + 3D비행기 + 드리프트 정체성(니어미스/코인) 완성. MVP 마무리(사운드 채우기 + iOS 실기기 빌드)만 남음.

---

## 0. 한 줄 요약
Unity로 만드는 **모바일(세로) 캐주얼 회피 게임**. 자동 전진하는 전투기로 유도미사일을 **드리프트**로 따돌리며 오래 버티고, 위험한 곳의 코인을 줍는다. **드리프트가 게임의 정체성**.

## 1. 개발 환경
- **엔진**: Unity 6000.3.16f1 (Unity 6.3 LTS), **URP**
- **타깃**: iOS (아이폰), **세로(Portrait) 390x844 기준**
- **프로젝트 경로**: `/Users/danielkim/DizzyJet`
- **Git**: GitHub `11gerrardtorres-hue/DizzyJet` 에 연결됨 (`git add . && git commit && git push`)
- **입력**: 새 Input System 사용 (Keyboard.current 등). EventSystem은 **InputSystemUIInputModule** 사용.
- 개발자는 **유니티/코딩 입문자** — 단계별로 "에디터에서 뭘 클릭하는지"까지 안내 필요. 코드 생성 후 2~3줄 설명 + Git 커밋 시점 알려주기.

## 2. 게임플레이 개요
- **조작 (모바일)**: 화면 **왼쪽 절반 터치 = 좌선회**, **오른쪽 절반 = 우선회**, **양쪽 동시 누름 = 드리프트**(먼저 누른 방향으로). 에디터에선 키보드 ←/→, ←+→ 동시 = 드리프트.
- **자동 전진** + 좌우 선회 + 3D 모델 **뱅킹(롤)**.
- **드리프트**: 넓은 U턴(급선회+살짝 감속). **충전식 부스터** — 드리프트하며 선회를 0.45초 이상 유지 후 떼면 부스터 발동(쿨다운 1초). 연타로는 발동 안 됨.
- **유도미사일**: 적기가 발사(한 번에 1발). 스폰 시 정조준 → 직진하다 유도. 선회율 제한 → 크게 돌거나 드리프트하면 **락 해제(lockBreakAngle 60°)** 되어 직진으로 빠짐. 장애물에 막히면 터짐(엄폐). 명중 시 게임오버.
- **적기**: 고무줄 추격(일정 거리 유지). 시간 지나면 최대 3기로 증원(화면 아래에서 등장, 좌우 대형). 너무 가까우면 충돌=게임오버.
- **장애물**: 회색 큐브, 절차적 생성(카메라 시야에 자동 맞춤). 닿으면 게임오버. 미사일 차폐물로도 사용.
- **무한 맵**: InfiniteGround가 바닥을 플레이어 따라 이동 + 텍스처 오프셋으로 무한 착시.
- **난이도 램프**: 25초마다 미사일 속도 ×1.04, 발사 간격 ×0.9, 적기 +1(최대 3).

## 3. 드리프트 정체성 (핵심 차별점)
- **니어미스**: 드리프트 중 **나를 추적 중(락온)인 미사일**이 가까이(NearMissDetector.nearMissRadius, 현재 2~3) 스치면 "NEAR MISS! +50" 점수 팝업. 미사일당 1회만.
- **코인**: **두 장애물이 빽빽한 틈(사이)**에만 소수(2개) 생성. 점수엔 영향 X, **영구 지갑(Wallet, PlayerPrefs)**에 적립.
- **코인 콤보**: 2초 안에 연속 수집하면 1→2→3… 배수로 더 적립(최대 5). "COIN COMBO xN" 팝업.
- 점수 = 생존시간 + 니어미스 보너스. 코인 = 상점용 재화(분리).

## 4. 스크립트 구조 (`Assets/Scripts/`)
- **PlayerController.cs** — 이동/선회/뱅킹/드리프트/충전식 부스터, 입력 통합(키보드+HoldButton), 드리프트 트레일(부스터 때만)·파티클·사운드 토글. `CurrentSpeed`, `IsDrifting` 노출.
- **CameraFollow.cs** — 탑뷰 추적(height), 부스터 시 FOV 킥, 화면 흔들림(Shake, 정적 Instance). 부스터/속도감.
- **InfiniteGround.cs** — 무한 바닥 착시(따라 이동 + 텍스처 오프셋, 체커보드 절차 생성).
- **Missile.cs** — 유도(제한 선회율), 락 해제(각도), 장애물 충돌, 명중→게임오버. `IsLockedOn`, `nearMissCounted`.
- **Enemy.cs** — 고무줄 추격(preferredDistance), 대형 오프셋, 미사일 발사(1발씩), 난이도 램프(속도/간격).
- **EnemyManager.cs** — 시간에 따라 적기 증원(최대 maxEnemies=3), 화면 아래에서 등장(자동맞춤), 좌우 대형(formationSpacing).
- **ObstacleSpawner.cs** — 절차적 장애물(카메라 자동맞춤 autoFitToCamera), 충돌→게임오버. "Obstacle" 태그 사용.
- **CoinSpawner.cs** — 빽빽한 장애물 사이에만 코인 생성(clusterRadius), 소수 유지(targetCount).
- **Coin.cs** — 회전 연출, 근접 시 `GameManager.CollectCoin()` 호출 후 소멸. (value 필드는 현재 미사용)
- **NearMissDetector.cs** — Player에 부착. 드리프트 중 락온 미사일 근접 시 `RegisterNearMiss()` (미사일당 1회).
- **GameManager.cs** — 상태(시작/플레이/게임오버), 점수, 니어미스/코인콤보, 최고기록(PlayerPrefs "BestScore"), 시작화면/게임오버 UI, timeScale 제어. 정적 Instance + skipIntro(재시작 시 타이틀 스킵).
- **Wallet.cs** — 정적 클래스. 영구 코인 지갑(PlayerPrefs "Coins"). `Wallet.Coins`, `Wallet.Add()`, `Wallet.TrySpend()` (상점용).
- **AudioManager.cs** — 정적 Instance. sfx/bgm/drift 소스. 클립: explosion/boost/warning/bgm/drift. PlayExplosion/PlayBoost/PlayWarning/SetDrift(bool).
- **HoldButton.cs** — UI 버튼 누름 감지(IsHeld). SteerLeft/SteerRight(화면 좌/우 절반 투명 Image)에 부착.
- **MissileSpawner.cs** — (구) 사방 미사일 스폰. **현재 비활성/미사용** (적기가 발사). 정리 대상.

## 5. 씬 오브젝트 (SampleScene)
- Main Camera (CameraFollow, height≈36, FOV 60, rot X90 탑뷰)
- Player (PlayerController, BoxCollider; 자식: JetModel=3D F-15, DriftTrailR/L=트레일, DriftFx 좌/우=파티클; NearMissDetector)
- ground (InfiniteGround), Enemy(프리팹화됨), EnemyManager, ObstacleSpawner, CoinSpawner, GameManager, AudioManager(+Sfx/Bgm/DriftSource)
- Canvas: ScoreText, ComboText(니어미스/코인콤보 팝업), SteerLeft, SteerRight, GameOverPanel(+Restart 버튼), StartPanel(타이틀+조작안내+탭시작)
- 프리팹(`Assets/Prefabs`): Missile, Enemy, Obstacle, Coin

## 6. 주요 밸런스 값 (Inspector에서 조절)
- Player: cruiseSpeed 15, turnSpeed 85, bankAngle(낮게), driftSpeed 18, driftTurnSpeed 150, driftBankAngle 45~60, boostSpeed 26, minChargeTime 0.45, boostCooldown 1
- Missile(프리팹): speed 22~25, turnSpeed 95~110, lifetime 6, hitRadius 1, lockBreakAngle 50~60
- Enemy: preferredDistance 13, baseSpeed 15, minSpeed 8, maxSpeed 21, fireInterval 3, hitDistance 2.5
- **연결 관계 주의**: cruiseSpeed는 미사일 speed보다 낮아야(미사일이 따라잡음) + 적기 속도와 비슷해야 함. 속도 하나 바꾸면 적기/미사일도 같이 조정.

## 7. 남은 할 일
### MVP 마무리 (필수)
1. **사운드 채우기** — AudioManager에 explosion/warning/bgm 클립 연결 (드리프트/부스터는 됨). mixkit/pixabay에서 받기.
2. **iOS 실기기 빌드** — Xcode 설치됨(최신은 macOS 26.2+ 필요). **iOS Build Support 모듈** 설치 후 Unity 재시작 → Build Profiles → iOS → Switch Platform → Player Settings(번들ID) → Build → Xcode에서 Apple ID 서명 → 아이폰 케이블 연결 → 설치 → 폰에서 개발자 신뢰.

### Phase 2 (MVP·재미 검증 후)
- **상점** — Wallet 코인으로 코스메틱(비행기색/트레일색) 구매. `Wallet.TrySpend()` 사용. 상점 UI + 구매 적용 로직.
- **아트 패스** — 적기(빨간 큐브→적 전투기 모델), 장애물(회색 큐브→바위/구조물), 맵/배경 테마. ※디자인은 MVP 다음 단계로 합의됨.
- **시작화면 정체성** — "DRIFT TO SURVIVE" 문구.
- **코인 카운터 UI** — 지갑 코인 화면 표시.
- 폭발 이펙트(큐브 파편), MissileSpawner 오브젝트 삭제, 밸런스 튜닝.

## 8. 중요한 학습/함정 (반복하지 말 것)
- **직렬화 함정**: 스크립트의 public 필드 **기본값을 바꿔도, 이미 씬/프리팹에 붙은 컴포넌트엔 적용 안 됨**(저장된 값이 우선). → Inspector에서 직접 바꾸거나, (저장된) 씬/프리팹 파일을 직접 수정 후 Unity에서 Reload. **새로 추가한 필드만** 기본값을 받음.
- **씬 저장 필수**: 에디터에서 오브젝트/컴포넌트/Inspector 값 바꾸면 **Cmd+S로 씬 저장**해야 디스크에 남음(메모리에만 있으면 날아감).
- **Play 중 수정 금지**: 재생 중에 값 바꾸면 멈출 때 원복됨.
- **2D 스프라이트 뱅킹은 포기, 3D 모델 채택**: 평평한 스프라이트는 롤(기울이면) 납작하게 찌그러짐. 큐브식 입체 뱅킹은 3D 모델이라야 됨. (PlayerController의 스프라이트 코드는 정리/제거됨)
- **세로 화면 좌우 시야 좁음**: 적기/장애물/미사일을 화면 **위/아래(긴 면)에서만** 등장시키고 카메라를 줌아웃(height↑)해서 해결. 스포너들은 카메라에 auto-fit.
- **모바일 UI Raycast**: 화면 위 글자(ScoreText/ComboText 등)는 **Raycast Target 끄기**(터치가 선회 영역을 막지 않게). 버튼은 켜기.

## 9. 코드 상태
- 최근 점검 완료: **큰 버그 없음**. 죽은 코드(2D 스프라이트, AddScore) 정리함.
- 성능: 매 프레임 OverlapSphere 여러 곳 — 현재 개수면 폰에서 OK. 오브젝트 대폭 증가 시 throttle 고려.

---

## 새 채팅에서 이어가기
1. 위 내용을 붙여넣고 "이 프로젝트 이어서 작업한다"고 알리기.
2. 현재 우선순위: **iOS Build Support 모듈 설치 → Unity 재시작 → iOS Switch Platform → 빌드/서명/설치**.
3. 그 전/병행으로 사운드 클립 3개 채우기.
4. 개발자는 입문자이므로 에디터 클릭 단계까지 안내.
