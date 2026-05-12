# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**수박 게임(Suika Game) 클론** — Unity AI 도구를 활용해 당일 완성하는 것이 목표인 2D 과일 병합 퍼즐 게임.

- **Unity 버전**: `6000.3.15f1` (Unity 6.3) — 반드시 이 버전으로 열 것
- **렌더러**: URP 2D (3D 파이프라인 아님) — `Assets/Settings/Renderer2D.asset` 기준
- **게임 설계 1차 명세**: `docs/GDD_수박게임_HanuYat.docx`. 게임플레이 규칙, 점수 체계, 과일 진화 체인(11단계), 스크립트 구조 등 모든 설계 결정은 이 문서를 1차 출처로 삼을 것

## Commands

**Unity Editor 실행**
Unity Hub에서 버전 `6000.3.15f1`로 프로젝트를 열어야 함. CLI 빌드 스크립트는 아직 없음.

**GDD 재생성**
```
py docs/create_gdd.py
```
> ⚠ Windows에서 `python` 명령은 Microsoft Store alias에 인터셉트됨 — 반드시 `py` 런처를 사용할 것.

**테스트**
`com.unity.test-framework 1.6.0`이 포함되어 있으나 테스트 코드는 아직 없음. 추가 시 Unity Editor의 Test Runner (Window > General > Test Runner) 또는 `mcp__unity-mcp__Unity_RunCommand` 도구로 실행.

## Architecture

### Asset Layout

| 경로 | 내용 |
|------|------|
| `Assets/Scenes/Suika.unity` | 메인 게임 씬 (유일한 씬) |
| `Assets/Scripts/` | 게임 로직 C# 스크립트 (현재 비어있음) |
| `Assets/Prefabs/` | 과일 프리팹 11종 (체리~수박) |
| `Assets/Sprites/` | Unity AI Generator 출력 스프라이트 |
| `Assets/Settings/` | URP Renderer/Pipeline/Volume 설정 에셋 |
| `Assets/InputSystem_Actions.inputactions` | 신규 Input System 1.19.0 액션 맵 |
| `Packages/com.unity.ai.assistant/` | **임베디드 AI Assistant 패키지** (삭제·수정 금지 — 아래 참고) |

### 스크립트 구조 (GDD §7 확정)

GDD에서 확정된 스크립트 역할 분담 (아직 미구현):

- `GameManager` — 게임 상태 (시작/종료/재시작)
- `FruitSpawner` — 과일 생성·투하, 미리보기 이동
- `Fruit` — 개별 과일 데이터 + 충돌 감지 (병합 중 플래그 `isMerging` 필수)
- `FruitMerger` — 병합 로직 + 새 과일 Instantiate
- `ScoreManager` — 점수 계산 + PlayerPrefs 저장
- `UIManager` — HUD 실시간 갱신 + 게임 오버 패널
- `FruitData` — ScriptableObject (과일별 레벨·점수·프리팹·반지름 정의)

### 게임플레이 핵심 결정 (재논의 불필요)

- **물리**: `Rigidbody2D` + `CircleCollider2D`, 각 과일 크기에 맞는 반지름
- **진화 체인 (11단계)**: 체리→딸기→포도→데코폰→감→사과→배→복숭아→파인애플→멜론→수박
- **투하 제한**: 1~5단계만 투하 가능, 다음 과일 1턴 예고 표시
- **병합**: 동일 과일 접촉 시 중심점에 다음 단계 생성 — 중복 충돌 방지 플래그(`isMerging`) 필수
- **게임 오버**: 과일이 위험선을 3초 이상 초과 유지 시

## Unity AI / MCP Workflow

### 임베디드 AI Assistant 패키지

`Packages/com.unity.ai.assistant/` (버전 2.7.0-pre.3)는 레지스트리에서 받은 패키지가 아니라 **프로젝트에 직접 포함된 소스**다. 이 패키지가 Sprite/Texture/Animation/Sound Generator와 Unity MCP 서버를 모두 제공하므로 절대 삭제하거나 임의로 수정하지 말 것.

### Claude Code → Unity Editor 직접 통신

Unity Editor가 실행 중이고 MCP 서버가 연결된 상태에서 아래 도구들로 Editor를 직접 조작할 수 있음:

| 작업 | MCP 도구 |
|------|----------|
| 씬·오브젝트 생성/수정 | `Unity_ManageScene`, `Unity_ManageGameObject` |
| C# 스크립트 생성·수정 | `Unity_CreateScript`, `Unity_ManageScript`, `Unity_ScriptApplyEdits` |
| AI 스프라이트·텍스처 생성 | `Unity_AssetGeneration_GenerateAsset` |
| 콘솔 로그 확인 | `Unity_GetConsoleLogs` |
| 씬 뷰 캡처·검증 | `Unity_SceneView_Capture2DScene` |

MCP 연결 실패 시 → Unity Editor 재시작이 1차 대응.

### Unity AI Generators (아트 파이프라인)

과일 스프라이트 11종은 Unity AI Sprite Generator로 생성하여 `Assets/Sprites/`에 저장.  
프롬프트 예시: `"cute cartoon cherry fruit, 2D game sprite, isolated white background, vibrant colors, round shape"`

## Repository Conventions

- **`.csproj` / `.slnx` / `.sln` 커밋 금지** — Unity가 자동 생성하며 `.gitignore`로 이미 제외됨 (루트의 `Unity.AI.*.csproj`들도 마찬가지)
- **`Assets/**/*.meta` 반드시 커밋** — `.gitignore`의 `!/[Aa]ssets/**/*.meta` 규칙으로 보존. 메타 파일 누락 시 Unity에서 GUID 재생성으로 참조 깨짐
- `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/` — 모두 무시 대상, 커밋하지 말 것
- `.claude/settings.local.json` — `py`, `dir`, `where` 명령 권한이 이미 등록되어 있음

## Notes

- OS: Windows 11 + PowerShell. 절대 경로 (`C:\...`) 권장
- 사용자 프롬프트는 한국어 기반이나 영어 혼용 OK
