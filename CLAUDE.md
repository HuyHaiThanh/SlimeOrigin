# SlimeOrigin — CLAUDE.md

Đây là file context cho Claude Code agent. Đọc toàn bộ trước khi viết bất kỳ dòng code nào.

---

## 0. Claude Code — Cách làm việc với project này

**Unity MCP đang kết nối** — dùng MCP tools để tạo/sửa script trực tiếp trong Editor, không cần write file thủ công.

### Workflow bắt buộc sau mỗi lần tạo/sửa script
```
create_script / script_apply_edits
  → poll editor/state cho đến khi is_compiling = false
  → read_console types=["error"] — nếu có lỗi thì fix trước khi tiếp tục
  → chỉ sau khi compile xanh mới attach component hoặc làm bước kế
```

### Slash commands có sẵn
| Command | Dùng khi |
|---------|---------|
| `/project:check` | Muốn biết Unity đang OK không, có error không |
| `/project:screenshot` | Muốn verify visual sau khi sửa scene/UI |
| `/project:play` | Toggle Play Mode để test nhanh |
| `/project:progress` | Xem đã xong bao nhiêu script trong tuần hiện tại |

### Quy tắc tạo script
- Đặt đúng folder theo kiến trúc mục 3.1
- Dùng `create_script` (MCP) thay vì Write tool
- Namespace: không dùng namespace (Unity 2D mobile project nhỏ)
- Không thêm `using` thừa

---

## 1. Project overview

**Tên game:** SlimeOrigin  
**Platform:** Mobile (Android/iOS), portrait orientation  
**Engine:** Unity 6.3 LTS (6000.3.14f1), template Universal 2D (URP)  
**Developer:** 1 người, người mới Unity  
**Mục tiêu:** Portfolio showcase — hoàn chỉnh Chapter 1 trong 4 tuần  

**Concept:** Match-3 RPG với narrative. Người chơi thức tỉnh thành một Slime trắng không có element, phải chiến đấu và absorb quái để học skill và đổi màu — lấy cảm hứng từ "Tensei Shitara Slime Datta Ken" (Chuyển Sinh Thành Slime) và hệ nguyên tố Genshin Impact.

---

## 2. Game design

### 2.1 Core fantasy
> "Bạn là Slime yếu nhất. Board phản ánh những gì bạn đã ăn."

- Bắt đầu: board toàn Slime trắng, không skill, không element
- Khi đánh bại và absorb quái → tile trên board đổi sang element của quái đó
- Skill không có sẵn — học được bằng cách absorb
- Build của người chơi hình thành tự nhiên qua lịch sử chiến đấu

### 2.2 Core game loop (mỗi màn)
```
Intro text (gặp quái)
  → Combat: match-3 theo lượt
    → Match đủ tile → skill kích hoạt
    → Quái tấn công cuối mỗi lượt
  → Quái chết → chọn Absorb hoặc bỏ qua
    → Absorb → tile mới spawn trên board + skill mới mở khóa
    → Claude API generate 1–2 câu dialogue của Slime
  → Chuyển màn tiếp
```

### 2.3 Board
- Kích thước: **7 × 7**
- Mỗi ô là `SlimeTile` — có `ElementType`
- Swap 2 tile liền kề (ngang/dọc) để tạo match
- Match 3+ cùng loại theo hàng hoặc cột → xóa → tile mới rơi từ trên xuống
- Match 4+: tạo special tile (implement sau)

### 2.4 Hệ thống nguyên tố (7 elements)

| Element | Màu | Board role | Genshin ref |
|---------|-----|-----------|-------------|
| Pyro | Đỏ cam | Damage chính | Fire |
| Hydro | Xanh dương | Heal + setup | Water |
| Cryo | Trắng xanh | Freeze / slow | Ice |
| Electro | Tím vàng | Chain damage | Thunder |
| Dendro | Xanh lá | Trap / grow | Nature |
| Anemo | Xám bạc | Spread element | Wind |
| Geo | Nâu vàng | Shield / block | Earth |

**MVP Chapter 1:** chỉ dùng Pyro, Hydro, Cryo trước. Thêm dần sau.

### 2.5 Skill system
- Không cooldown — kích hoạt khi đạt ngưỡng tile đã match trong 1 trận
- Counter hiển thị rõ trên màn hình
- Ví dụ:
  - Match tích lũy 10 Pyro tile → **Flare** (damage lớn tất cả)
  - Match tích lũy 8 Hydro tile → **Regenerate** (hồi HP)
  - Match tích lũy 5 Pyro + 5 Electro → **Overloaded** (reaction AoE)
- Mỗi skill học được từ 1 loại quái cụ thể

### 2.6 Elemental reactions (6 cái, implement dần)

| Reaction | Công thức | Effect trên board |
|----------|-----------|------------------|
| Vaporize | Pyro + Hydro | Damage ×2, tile xung quanh bị Wet |
| Frozen | Hydro + Cryo | Quái freeze 2 lượt, ô thành Ice tile |
| Overloaded | Pyro + Electro | AoE xóa vùng 3×3 ngẫu nhiên |
| Bloom | Hydro + Dendro | Tạo Dendro Core, nổ sau 2 lượt |
| Swirl | Anemo + any | Lan element sang 2–3 tile xung quanh |
| Crystallize | Geo + any | Tạo Crystal shard → nhặt để có shield |

**MVP:** chỉ implement Vaporize và Frozen trước.

### 2.7 Absorb system
- Quái chết → hiện popup "Absorb?" (Yes/No)
- Absorb Yes:
  - Chọn 3 ô **trống ngẫu nhiên** trên board → spawn tile element của quái
  - Nếu board đầy: xóa 3 tile White (Slime trắng) ngẫu nhiên trước rồi spawn
  - Nếu lần đầu absorb element này → skill mới mở khóa
  - Claude API gọi để generate dialogue (xem mục 5)
- Absorb No: không có gì thay đổi

### 2.8 Enemy design — Chapter 1 (5 màn)

| Màn | Quái | Element | HP | Attack/lượt | Skill dạy |
|-----|------|---------|-----|------------|-----------|
| 1 | Goblin | Pyro | 30 | 8 | Flare |
| 2 | Forest Wolf | Dendro | 40 | 10 | Thorn Trap |
| 3 | Cave Bat | Cryo | 35 | 9 | Frost Nova |
| 4 | River Fish | Hydro | 45 | 11 | Regenerate |
| 5 (boss) | Baby Dragon | Pyro + Hydro | 80 | 15 | — (dùng skill đã học) |

### 2.9 Player stats & combat numbers

| Stat | Giá trị | Ghi chú |
|------|---------|---------|
| Player HP | 100 | Không hồi giữa màn (trừ skill Regenerate) |
| Damage mỗi match-3 | 5 | Base damage |
| Bonus mỗi tile thêm | +2 | Match-5 = 5 + 2×2 = 9 damage |
| Skill: Flare | 20 damage tất cả | Kích hoạt khi tích lũy 10 Pyro tile |
| Skill: Regenerate | Hồi 15 HP | Kích hoạt khi tích lũy 8 Hydro tile |
| Skill: Frost Nova | Freeze 2 lượt | Kích hoạt khi tích lũy 8 Cryo tile |

**Board khởi tạo:** Random đều từ 3 elements MVP (Pyro / Hydro / Cryo). White tile = 0 lúc đầu — chỉ xuất hiện sau khi absorb thất bại hoặc board reset. Đảm bảo không có match-3 sẵn khi spawn (shuffle lại nếu có).

---

## 3. Kiến trúc code

### 3.1 Folder structure
```
Assets/
  Scripts/
    Board/
      BoardManager.cs       ← quản lý grid 7×7
      SlimeTile.cs          ← mỗi ô trên board
      MatchDetector.cs      ← tìm match 3+
      GravitySystem.cs      ← tile rơi sau khi xóa
      SwapHandler.cs        ← xử lý input swap
    Combat/
      CombatManager.cs      ← điều phối lượt đánh
      EnemyController.cs    ← HP, attack, AI đơn giản
      SkillSystem.cs        ← đếm tile, kích hoạt skill
      ReactionSystem.cs     ← detect và trigger reaction
    Absorb/
      AbsorbSystem.cs       ← logic absorb sau khi quái chết
      DialogueManager.cs    ← gọi Claude API, hiện text
    Data/
      ElementType.cs        ← enum 7 element
      SkillData.cs          ← ScriptableObject cho skill
      EnemyData.cs          ← ScriptableObject cho enemy
    UI/
      UIManager.cs          ← HP bar, skill counter, score
    Core/
      GameManager.cs        ← game state machine
      LevelManager.cs       ← load màn từ JSON
  Prefabs/
  Sprites/
  ScriptableObjects/
  Scenes/
  Resources/
    Levels/                 ← JSON data cho từng màn
```

### 3.2 Scene & Camera setup (reference cho tuần 1)

**Chỉ 1 scene duy nhất:** `GameScene.unity` — tất cả state chuyển qua GameManager, không dùng Scene load giữa màn.

**Layout portrait 1080×1920:**
```
┌─────────────────┐
│  Enemy area     │  ~320px  (top)
│  HP bar, sprite │
├─────────────────┤
│                 │
│   Board 7×7     │  ~1080px (middle) → tile size ≈ 154px
│                 │
├─────────────────┤
│  Player HUD     │  ~520px  (bottom)
│  HP, skills,    │
│  element counter│
└─────────────────┘
```

**Camera settings:**
- Projection: Orthographic
- Size: **9.6** (= 1920/2 / 100 — giả sử 100 px/unit)
- Tile size trong Unity: **1.5 units** (≈ 150px)
- Board tổng = 7 × 1.5 = **10.5 units** → cần scale hoặc dùng 1.3 units/tile nếu chật

**Placeholder sprites:** Dùng `Sprites/Square` built-in (Project > Create > Sprites > Square), tint màu theo element. Đủ để chạy board logic trước khi có art thật.

| Element | Hex color |
|---------|-----------|
| Pyro | `#FF4400` |
| Hydro | `#0088FF` |
| Cryo | `#88DDFF` |
| White (default) | `#DDDDDD` |

### 3.3 Conventions
- Unity 6.3 LTS, Universal 2D (URP), C#
- Mỗi script 1 responsibility — không viết god class
- Dùng `ScriptableObject` cho data (skill, enemy) — không hardcode số trong script
- Dùng `UnityEvent` hoặc `Action` để decouple component
- Không dùng `FindObjectOfType` trong Update — cache reference trong Awake/Start
- Tên biến tiếng Anh, comment tiếng Việt nếu cần giải thích logic phức tạp

### 3.5 Game state machine (GameManager)
```
States: MainMenu → LevelIntro → Combat → AbsorbChoice → LevelComplete → GameOver
```

### 3.6 Turn structure (CombatManager)
```
PlayerTurn:
  1. Nhận input swap
  2. Detect match → xóa tile → gravity
  3. Tích lũy element counter
  4. Kiểm tra skill threshold → kích hoạt nếu đủ
  5. Kiểm tra reaction (2 element kề nhau khi match)
  6. Gây damage cho enemy

EnemyTurn:
  1. Enemy attack (damage cố định hoặc theo pattern)
  2. Kiểm tra HP player → GameOver nếu = 0
  3. Kiểm tra HP enemy → chuyển sang AbsorbChoice nếu = 0
```

---

## 4. Lộ trình 4 tuần

### Tuần 1 — Board hoạt động
- [ ] `ElementType.cs` enum
- [ ] `SlimeTile.cs` MonoBehaviour
- [ ] `BoardManager.cs` — spawn grid 7×7 ngẫu nhiên
- [ ] `SwapHandler.cs` — tap/swap input (mobile touch)
- [ ] `MatchDetector.cs` — tìm match 3+ hàng và cột
- [ ] `GravitySystem.cs` — tile rơi sau khi xóa
- **Verify:** board chạy được, match và refill đúng

### Tuần 2 — Combat loop
- [ ] `EnemyData.cs` ScriptableObject
- [ ] `EnemyController.cs` — HP, attack theo lượt
- [ ] `SkillData.cs` ScriptableObject
- [ ] `SkillSystem.cs` — đếm counter, kích hoạt skill
- [ ] `CombatManager.cs` — PlayerTurn / EnemyTurn
- [ ] `UIManager.cs` — HP bar, element counter
- **Verify:** đánh được quái, skill kích hoạt đúng

### Tuần 3 — Absorb + Reactions + 5 màn
- [ ] `AbsorbSystem.cs`
- [ ] `DialogueManager.cs` + Claude API call
- [ ] `ReactionSystem.cs` — Vaporize và Frozen trước
- [ ] `LevelManager.cs` + JSON cho 5 màn
- [ ] `GameManager.cs` state machine hoàn chỉnh
- **Verify:** chơi được Chapter 1 từ đầu đến cuối

### Tuần 4 — Polish + publish
- [ ] Sprite thật cho 7 slime (AI-generated hoặc free asset)
- [ ] Particle effect khi reaction
- [ ] Sound effect cơ bản
- [ ] Build APK
- [ ] Publish itch.io
- [ ] Viết `AI_WORKFLOW.md` — case study cho portfolio

---

## 5. AI integration (Claude API)

### 5.1 Dialogue generation khi absorb
Gọi sau khi absorb xong, hiện 1–2 câu slime độc thoại.

**Endpoint:** `POST https://api.anthropic.com/v1/messages`  
**Model:** `claude-sonnet-4-6`

**System prompt:**
```
Bạn là một Slime mới thức tỉnh trong thế giới giả tưởng. 
Bạn vừa absorb một sinh vật và học được element mới.
Hãy nói 1–2 câu ngắn, giọng tò mò và hơi ngây thơ, 
phản ánh đúng element vừa học (Pyro = nóng bỏng/tự tin, 
Hydro = bình tĩnh/linh hoạt, Cryo = lạnh lùng/chậm rãi...).
Không giải thích — chỉ nói như nhân vật đang cảm nhận.
Trả lời bằng tiếng Việt, tối đa 2 câu.
```

**User prompt (dynamic):**
```
Tôi vừa absorb [TÊN QUÁI] và học được element [ELEMENT].
```

### 5.2 Cách gọi từ Unity (C#)
Dùng `UnityWebRequest` với coroutine. Key lưu trong `Assets/Resources/config.json` (gitignore file này).

### 5.3 Các chỗ khác có thể dùng AI sau này
- Generate level JSON từ prompt (tuần 3+)
- Generate enemy description cho intro text
- Balance check: paste số liệu, hỏi Claude có cân bằng không

---

## 6. Nguyên tắc khi agent làm việc

1. **Scope nhỏ mỗi lần** — 1 class, 1 feature, không viết toàn bộ game 1 lúc
2. **Hỏi trước khi giả định** — nếu không rõ behavior mong muốn, hỏi
3. **Không thêm feature ngoài yêu cầu** — không viết code "phòng xa"
4. **Verify rõ ràng** — sau mỗi task nói rõ "test bằng cách nào"
5. **Dùng ScriptableObject cho data** — không hardcode số vào script
6. **Comment logic phức tạp** — đặc biệt reaction system và state machine