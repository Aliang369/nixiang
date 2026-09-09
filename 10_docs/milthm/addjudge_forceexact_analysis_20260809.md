# milthm 6.0.5 — Why pre-writing `HitObject.JudgeLevel@0x8C` before `AddJudge` can fail

Date: 2026-08-09  
APK: `02_apks/milthm/milthm_6.0.5_orig_vc40060005.apk`  
lib: `03_decompiled/milthm/lib_arm64_6.0.5/libil2cpp.so` (RX LOAD: **VA = file_offset + 0x4000**)  
Dumper: Cpp2IL MethodMap + LibCpp2IL field offsets + DummyDll `06_dumped/milthm/il2cpp/Milthm.dll`

## Addresses

| Method | FileOffset (MethodMap Address) | VA |
|--------|--------------------------------|-----|
| `JudgeResult.AddJudge` | `0x42E2474` | `0x42E6474` |
| next: `GetPredictedPerfectComboCounter` | `0x42E29A4` | `0x42E69A4` → length **0x530** |
| `PlayJudgements.AddJudgeStatus` | `0x42E4EBC` | `0x42E8EBC` |
| `PlayJudgements.ProcessJudgeStatus` | `0x42E5794` | `0x42E9794` |
| `HitObject.ToStatisticJudgeLevel` | `0x4182484` | `0x4186484` |

Sigs (MethodMap):

- `void JudgeResult.AddJudge(HitObject obj)`
- `void PlayJudgements.AddJudgeStatus(JudgeStatus status, HitObject obj, JudgeInputContext ctx, MappedInputData input)`
- `void PlayJudgements.ProcessJudgeStatus(JudgeInputContext ctx, JudgeStatus status, MappedInputData input, HitObject obj, bool autoplay=false)`

## Enums / fields (DummyDll + `_field_offsets_libcpp2il.txt`)

### `JudgeLevel` (int @ HitObject+0x8C)

| Name | Value |
|------|------:|
| Miss | 0 |
| Bad | 1 |
| Good | 2 |
| Perfect | 3 |
| **Exact** | **4** |

### `StatisticJudgeLevel` (byte — scoring token)

| Name | Value |
|------|------:|
| Miss | 255 (`0xFF`) |
| Bad | 155 (`0x9B`) |
| Good | 140 (`0x8C`) |
| Great | 105 (`0x69`) |
| Perfect | 70 (`0x46`) |
| **Exact** | **35 (`0x23`)** |

### `JudgeStatus` (8-byte struct — **no Level**)

| Off | Field |
|----:|-------|
| 0 | `bool OccupyInput` |
| 1 | `bool Disconnect` |
| 2 | `bool CanJudge` |
| 3 | `bool PendingInput` |
| 4 | `HitStatus NextStatus` |

`HitStatus`: UnHit=0, AutoHit=1, Wait=2, Completed=3, **Missed=4**, Keep=5.

### HitObject (relevant)

| Off | Field |
|----:|-------|
| 0x89 | `bool IsAlwaysPerfect` |
| **0x8C** | **`JudgeLevel JudgeLevel`** |
| 0x90 | `float JudgeBias` |
| 0x94 | `HitStatus Status` |
| 0xB0 | `HitType Type` |
| 0xDC | `JudgeStatus PendingStatus` |

---

## 1) Does `AddJudge` load from hitObj at 0x89 / 0x8C / 0x90 / 0x94 / 0xDC?

`x1` → saved as `x20` (hitObj). Direct mem ops on hitObj in `AddJudge`:

| Offset | In AddJudge? | Notes |
|-------:|:------------:|-------|
| 0x89 | **No** | |
| **0x8C** | **Yes** | `ldr w9,[x20,#0x8c]` @ VA `0x42E6548`; also read inside callee `ToStatisticJudgeLevel` |
| 0x90 | **Indirect** | Only via `ToStatisticJudgeLevel` when level==Good(2) |
| 0x94 | **No** | |
| 0xDC | **No** | |
| 0xB0 | Yes | `HitType` |
| 0x98 | Yes | `JudgeSerialStart` |

**Verdict: AddJudge does read `0x8C`.** Pre-write is not ignored because the field is unused; failures come from **timing / wrong constant / conversion / upstream overwrite**.

---

## 2) Early control flow — JudgeLevel is not the primary score register

Prologue (VA):

1. cctor / static init guards  
2. `cbz x20` → null throw  
3. **`bl ToStatisticJudgeLevel(hitObj)` @ `0x42E64D0`** → result in `w0`, saved **`w21`**  
4. Serial / type bookkeeping using `Type@0xB0`, `JudgeSerialStart@0x98`  
5. **`ldr JudgeLevel@0x8C`; `cmp #4`** — only special-cases **Exact** for HP (`set_HP`)  
6. Score / combo increments switch on **`w21` (StatisticJudgeLevel byte)**, not re-reading level for each bucket  

So score path is:

```
HitObject.JudgeLevel (+ JudgeBias if Good)
        │
        ▼
ToStatisticJudgeLevel()  →  StatisticJudgeLevel in w21
        │
        ▼
AddJudge switch(w21): 0xFF/0x9B/0x8C/0x69/0x46/0x23
```

`ToStatisticJudgeLevel` map (VA `0x41864B4`+):

| JudgeLevel | Statistic |
|-----------:|-----------|
| 0 Miss | `0xFF` |
| 1 Bad | `0x9B` |
| 2 Good | `abs(JudgeBias) <= thresh ? 0x69 Great : 0x8C Good` |
| 3 Perfect | `0x46` |
| **4 Exact** | **`0x23`** |

Common ineffective writes:

- Write **`0` thinking “best”** → **Miss**  
- Write **`35` / `0x23` (Statistic Exact)** into `JudgeLevel` → falls off switch → throw path  
- Write **Perfect(3)** → Perfect stats, not Exact  
- Write **Good(2)** without zeroing **`JudgeBias@0x90`** → may become Great vs Good, never Exact  

---

## 3) Calls from `AddJudge`

Meaningful game calls (ignoring il2cpp helpers `0x23af*`):

| VA bl | Target |
|------:|--------|
| `0x42E64D0` | **`HitObject.ToStatisticJudgeLevel`** |
| `0x42E6584` | `JudgeResult.set_HP` (Exact HP adjust) |
| `0x42E65C0` / `0x42E65D8` | statistic dictionary get/set helpers @ `0x42E621C` / `0x42E68C8` |
| `0x42E6630` / `0x42E66E4` | score-related `0x4502D04` |
| `0x42E6684` | `0x42EF440` (Miss/bomb-ish path when stat==0xFF on Type==3) |
| epilogue | tail to `0x42EF5CC` |

---

## 4) `AddJudgeStatus` — Level from HitObject; can **wipe** it from `JudgeStatus.NextStatus`

Args (ARM64): `x0=status` (8B in register), `x1=obj`, `x2=ctx`, `x3=input`.

Early (VA `0x42E8F18`):

```text
lsr  x8, x21, #32          ; NextStatus = status high dword
cmp  x8, #4                ; HitStatus.Missed
b.ne skip
str  wzr, [x19, #0x8c]     ; JudgeLevel = Miss (0)  ← CLEARS Exact
bl   JudgeResult.AddJudge  ; @ 0x42E8FA0
```

- **Does not read Level from `JudgeStatus`** (struct has none).  
- Uses **`HitObject.JudgeLevel`**, but if **`NextStatus == Missed(4)`** it **forces Miss before `AddJudge`**.  
- Also reads `Type@0xB0`, and later paths in sibling methods use `0x8C` / `0x89` / `0x90` for SweetDream HP / perfectionism.

**Implication:** writing Exact on the object, then entering `AddJudgeStatus` with Missed next-status, **zeros `0x8C` before `AddJudge`**. A hook that only patches memory “sometime before AddJudge” loses to this store.

---

## 5) `ProcessJudgeStatus` — computes Level onto HitObject, then calls `AddJudgeStatus`

- `x0=ctx`, `x1=status`, `x2=input`, `x3=obj`, `w4=autoplay`.  
- Timing windows → `w23` level; respects **`IsAlwaysPerfect@0x89`**.  
- **`str w23,[x21,#0x8c]`** / Exact special **`str #4`** then **`str s9,[x21,#0x90]`** (bias).  
- Calls **`AddJudgeStatus`** (`bl 0x42E8EBC`) in several branches.  
- Hold/`Type==1` path: **save level → temporarily `str #4` (Exact) → `AddJudgeStatus` → restore previous level** (`0x42E9D44`–`0x42E9D90`). Scoring inside that call sees the temporary value; object field may not stay Exact afterward.  
- Another path saves/restores `0x8C` around `AddJudgeStatus` (`0x42E9DEC` / `0x42E9E30`).

**Level source:** computed in `ProcessJudgeStatus` from timing + flags, stored on **HitObject**; `JudgeStatus` only contributes flags / `NextStatus`, not a Level field.

---

## Why “write `0x8C` before `AddJudge`” feels ineffective

1. **Must write Exact = 4**, not 0 / 35.  
2. Score uses **`ToStatisticJudgeLevel` at the very start** of `AddJudge`; `0x8C` must be correct **before that `bl`**.  
3. **`AddJudgeStatus` can `str 0` to `0x8C`** when `NextStatus==Missed` immediately before calling `AddJudge`.  
4. **`ProcessJudgeStatus` overwrites `0x8C`/`0x90`** after any earlier poke.  
5. Good(2) also needs **`JudgeBias`**; Exact does not go through that branch.  
6. Direct `0x8C` load later in `AddJudge` is mostly the **Exact HP** branch (`cmp #4`), not the combo/acc switch.

---

## Recommended hook points (force Exact)

### A. Preferred — `JudgeResult.AddJudge` onEnter (file `0x42E2474` / VA `0x42E6474`)

Before native body:

- `obj+0x8C = 4` (Exact)  
- optional `obj+0x90 = 0.0` (bias)  

Ensures `ToStatisticJudgeLevel` returns `0x23` even if `AddJudgeStatus` cleared the field earlier **only if your onEnter runs after that clear** (it does: clear is in caller, then `bl AddJudge` → your onEnter).

### B. Stronger / simpler — replace `ToStatisticJudgeLevel` return

Always return `0x23` (Statistic Exact). Bypasses enum mistakes and bias split.

### C. Native patch inside `AddJudge`

After `bl 0x4186484`, force `mov w21, #0x23` (or NOP HP-only path as needed). Survives any prior field churn.

### D. Upstream — `ProcessJudgeStatus` / `AddJudgeStatus`

- Force computed level `w23=4` before stores / before `AddJudgeStatus`.  
- If Missed path must still “look Exact”, **NOP or skip** `str wzr,[obj,#0x8c]` @ VA `0x42E8F28`, **or** re-write `0x8C=4` in `AddJudgeStatus` after that store and before `bl AddJudge`.

### Not sufficient alone

- Poking `0x8C` once on note spawn / before `ProcessJudgeStatus` without A–C.  
- Writing Statistic enum values into `JudgeLevel`.  
- Hooking only graphics / `Status@0x94` / `PendingStatus@0xDC`.

---

## Short answers

| Question | Answer |
|----------|--------|
| Does `AddJudge` read `0x8C`? | **Yes** (directly + via `ToStatisticJudgeLevel`). |
| Why pre-write may still fail? | Wrong constant; upstream clear on Missed; score latch is statistic in `w21` from early convert; Good needs bias. |
| What to hook? | **`AddJudge` onEnter → set level=4**, or **force `ToStatisticJudgeLevel`→0x23**, or patch `w21` after the convert `bl`. |

## Sources

- Capstone / `llvm-objdump` on `libil2cpp.so`  
- `06_dumped/milthm/il2cpp_6.0.5/script.json` MethodMap  
- `06_dumped/milthm/il2cpp_6.0.5/hitobject_fields_probe.txt`  
- `06_dumped/milthm/il2cpp_6.0.5/_field_offsets_libcpp2il.txt`  
- ilspycmd on DummyDll `Milthm.dll` (`JudgeLevel`, `JudgeStatus`, `StatisticJudgeLevel`, `HitStatus`)

FindCrypt: not applicable to this judgment-pipeline disasm (no cipher hunt this pass).
