# HERETICIDE — Session Recap

A running log of what's been built, how to run it, and what's next. Read this first next time.

_Last updated: 2026-06-08_

---

## TL;DR

**HERETICIDE: Imperium Survivors** — a Warhammer 40K–themed, *Vampire Survivors*–style horde
survival action roguelike in **C# / MonoGame (DesktopGL, net8.0)**. Runs on **Windows desktop now**;
built to port to **Android** later (touch joystick + tap menus already wired in). All art is
**procedural pixel art generated in code** — no asset files, no Content Pipeline.

**To play right now:** run `E:\Game\dist\Hereticide.exe` (standalone, needs nothing installed).

---

## ⚠️ Important environment note (don't lose this)

My build session and the user's interactive PowerShell **share only the `E:` drive, not `C:`**.
- The .NET 8 SDK was installed to the agent's profile at `C:\Users\fabde\AppData\Local\Microsoft\dotnet`
  — **invisible to the user's shell**.
- Therefore the **deliverable is a self-contained publish on E:**: `E:\Game\dist\` (≈75 MB, bundles
  the runtime + SDL2). The user runs `E:\Game\dist\Hereticide.exe` directly.
- **After any code change, re-publish** so `dist` is current:
  ```powershell
  $dn='C:\Users\fabde\AppData\Local\Microsoft\dotnet'; $env:Path="$dn;$env:Path"; $env:DOTNET_ROOT=$dn
  dotnet publish E:\Game\Hereticide.csproj -c Release -r win-x64 --self-contained true -o E:\Game\dist
  ```
- Don't tell the user to double-click `bin\...\Hereticide.exe` (framework-dependent apphost can't
  find the user-local runtime). Use `dist\Hereticide.exe` or `dotnet run`.

---

## How to run / build

| Goal | Command |
|---|---|
| **Play (user)** | `E:\Game\dist\Hereticide.exe` |
| Build from source | `dotnet build E:\Game\Hereticide.csproj -c Release` |
| Run from source | `./run.ps1`  (or `./run.ps1 -Demo` for autoplay) |
| Re-publish standalone | see the publish command above |

`run.ps1` auto-locates the user-local `dotnet`, so it works even from an odd shell.

## Controls

- **Move:** drag the **left half** of the screen, or `WASD` / arrow keys
- **Weapons:** fire automatically
- **Level-up:** click a card or press `1` / `2` / `3`
- **Start / restart:** tap / `Enter` / `Space` · **Quit:** `Esc`

---

## What's implemented

### Core vertical slice
- Game-state loop: **Title → Playing → Level-Up → Game-Over**, HUD, menus.
- **1 Space Marine** (Ultramarine), 8-way movement, follow camera w/ screen shake.
- **Weapons (auto-fire, level-able):** Bolter (ranged, multi-shot/pierce), Chainsword (melee AoE),
  **Plasma Gun** + **Frag Launcher** (unlockable via level-up).
- **Hordes:** Cultist, Hormagaunt (fast), Ork (tanky), Chaos Marine elite — density/toughness ramp
  per minute, periodic swarm waves.
- **Progression:** XP gems → level up → pick 1 of 3 blessings (new weapons / upgrades / passives:
  +HP, +damage, cooldown, proj speed, pickup range, armour, regen, area).
- Particles, blood, muzzle flashes, AoE blasts; procedural 5×7 bitmap font; tiled battlefield floor.

### Companion + boss (added this session)
- **HABIBTI NOURHAN MY LOVE** companion — unlocks at **player level 5**. Follows the marine, fires a **weak**
  bolt pistol, and **slowly heals the player over time** (green aura).
- At **player level 10** she **falls to Chaos → becomes the Fallen Sister boss**: large corrupted
  winged sprite, **boss HP bar**, fires **hostile warp-bolts** at the player.
- **Purge the boss** → big XP + heal burst, and the **redeemed** Sister rejoins as companion.
- Event banners announce each beat.

---

## Project layout

```
Hereticide.csproj      MonoGame DesktopGL project (net8.0, WinExe)
Program.cs             Entry point
run.ps1                Build & run helper (-Demo for autoplay)
dist/                  >>> standalone playable build (run Hereticide.exe) <<<
src/
  Game1.cs             Main loop, states, HUD, menus, boss bar, banner, autoplay
  World.cs             Simulation + rendering of all entities; weapon API; companion/boss logic
  Core/
    Art.cs             Procedural sprites (incl. Sister + SisterBoss), ground, fx textures
    PixelFont.cs       5×7 bitmap font (no font files)
    Camera2D.cs        Follow camera, zoom, shake
    Input.cs           Unified keyboard / mouse / touch pointers
    VirtualJoystick.cs Floating thumbstick (touch + mouse) — Android-ready
  Entities/
    Player.cs  Enemy.cs  Projectile.cs  XpGem.cs  Particle.cs  Companion.cs
  Weapons/
    Weapon.cs          Base + Bolter / Chainsword / Plasma / Frag
  Systems/
    Spawner.cs         Horde spawning, difficulty ramp, swarms, elites
    Upgrades.cs        Level-up card pool
```

---

## Key decisions

- **Engine: MonoGame** (over Godot/Unity) so the whole game is code-driven and fully scaffoldable.
- **Procedural art** (sprites + font generated in `Art.cs` / `PixelFont.cs`) — no asset files.
- **Content boundary:** user asked to turn a real person's photo into a "sexy" character. Declined
  sexualizing a real individual; instead built a themed **armored Battle Sister** with the same
  mechanics. Keep future characters non-sexualized and not derived from real photos.

## Debug / tuning env vars (all default off)

- `HERETICIDE_AUTOPLAY=1` — self-driving attract/test mode (kites horde, auto-picks blessings).
- `HERETICIDE_XPSCALE=<n>` — multiply XP gain (e.g. `60` to reach the lvl-10 boss fast).
- `HERETICIDE_BOSSHP=<n>` — scale boss HP (e.g. `0.03` for a quick boss kill in tests).
- `HERETICIDE_EVENTLOG=<path>` — append companion/boss milestone events to a file.

## Verification done

Build clean (0 warnings / 0 errors). Autoplay smoke tests confirmed, with **zero exceptions**:
companion joins @ lvl 5 → boss spawns @ lvl 10 + shoots back (45s stable) → boss purged → redeemed
Sister returns. Standalone `dist\Hereticide.exe` launches and runs.

---

## TODO / next steps (pick up here)

- [ ] **Android APK** — shared library + `mgandroid` head project (steps in `README.md`); needs
      `dotnet workload install android` + Android SDK/JDK.
- [ ] **Audio** — SFX/music (no audio yet; would add DynamicSoundEffect or content audio).
- [ ] Boss **phases / special attacks**; more bosses.
- [ ] More **weapons / enemy types / passives**; weapon evolutions.
- [ ] Meta-progression (persistent unlocks between runs), save file.
- [ ] Pause menu; settings (volume, resolution).
- [ ] Polish companion combat AI / make her targetable & revivable (currently invulnerable support).

## Related memory (agent)
`~/.claude/projects/E--Game/memory/` → `project-hereticide.md`, `dotnet-userlocal.md`.
