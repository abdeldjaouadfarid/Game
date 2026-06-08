# HERETICIDE: Imperium Survivors

A Warhammer 40,000–flavoured, *Vampire Survivors*–style horde survival action roguelike,
written in **C# / MonoGame**. One lone Space Marine, infinite heretics, auto-firing **melee +
ranged** weapons, XP gems, and level-up blessings. Designed to run on **Windows desktop now**
and port to **Android** (touch joystick already built in).

> 100% procedural pixel art — every sprite and the font are generated in code. No asset files,
> no MonoGame Content Pipeline, nothing to import.

---

## Controls

| Action | Desktop | Android (touch) |
|---|---|---|
| Move | `WASD` / arrow keys, **or** click-drag the left half of the screen | Drag anywhere on the left half (floating joystick) |
| Fire | Automatic — all weapons fire on their own | Automatic |
| Pick upgrade | Click a card, or press `1` / `2` / `3` | Tap a card |
| Start / restart | Click / `Enter` / `Space` | Tap |
| Quit (abandon run → title; title → exit) | `Esc` | — |

## Gameplay

- **Weapons** (auto-fire, level them up on the level-up screen):
  - **Bolter** — ranged, auto-targets nearest foes; gains extra bolts & piercing.
  - **Chainsword** — melee, sweeps everything around you; gains damage & reach.
  - **Plasma Gun** *(unlockable)* — slow, hard-hitting piercing shots.
  - **Frag Launcher** *(unlockable)* — lobs grenades that explode in an area.
- **Hordes**: Chaos Cultists, Tyranid Hormagaunts (fast), Orks (tanky), and **Chaos Marine elites**.
  Density and toughness ramp up every minute, with periodic **swarm waves**.
- **Progression**: enemies drop XP gems → fill the bar → **level up** → pick 1 of 3 blessings
  (new weapons, weapon upgrades, or passives like +HP, +damage, +speed, armour, regen…).
- **HABIBTI NOURHAN MY LOVE (companion)** — joins you at **level 5**: a Battle Sister who fires a weak bolt
  pistol and **slowly heals you over time**. At **level 10** she **falls to Chaos and becomes a
  boss** (shoots warp bolts, big HP bar). Purge her and she returns *redeemed* as your companion.
- Survive as long as you can. Score = time survived, level reached, skulls taken.

---

## Build & Run (Windows)

The .NET 8 SDK was installed to your user profile at `%LOCALAPPDATA%\Microsoft\dotnet`.

```powershell
# from E:\Game
./run.ps1
```

or manually:

```powershell
$env:Path = "$env:LOCALAPPDATA\Microsoft\dotnet;$env:Path"
dotnet run -c Release
```

First build restores the `MonoGame.Framework.DesktopGL` NuGet package (needs internet once).

> **Note:** launch with `dotnet run` (or `./run.ps1`), not by double-clicking `bin\...\Hereticide.exe`.
> The SDK is installed under your user profile rather than registered machine-wide, so the bare
> `.exe` apphost can't find the runtime — the `dotnet` host resolves it correctly. (To make a
> double-clickable build, publish self-contained: `dotnet publish -c Release -r win-x64 --self-contained`.)

### Demo / attract mode
`./run.ps1 -Demo` (or set `HERETICIDE_AUTOPLAY=1`) makes the marine deploy and fight on its own —
handy for watching the game or for a hands-off smoke test.

### Debug / tuning env vars (all default off)
- `HERETICIDE_AUTOPLAY=1` — self-driving attract mode (kites the horde, auto-picks blessings).
- `HERETICIDE_XPSCALE=<n>` — multiply XP gain (e.g. `60` to reach the level-10 boss fast).
- `HERETICIDE_BOSSHP=<n>` — scale the Fallen Sister's HP (e.g. `0.03` for a quick boss kill).
- `HERETICIDE_EVENTLOG=<path>` — append milestone events (companion/boss) to a file.

---

## Project layout

```
Hereticide.csproj          MonoGame DesktopGL project (net8.0)
Program.cs                 Entry point
src/
  Game1.cs                 Main loop, states (Title/Playing/LevelUp/GameOver), HUD & menus
  World.cs                 The run: simulation + rendering of all entities; weapon API
  Core/
    Art.cs                 Procedural pixel-art sprite/texture generation
    PixelFont.cs           5x7 procedural bitmap font (no font files)
    Camera2D.cs            Follow camera, zoom, screen shake
    Input.cs               Unified keyboard / mouse / touch pointers
    VirtualJoystick.cs     Floating thumbstick (touch + mouse) — Android-ready
  Entities/
    Player.cs  Enemy.cs  Projectile.cs  XpGem.cs  Particle.cs
  Weapons/
    Weapon.cs              Weapon base + Bolter / Chainsword / Plasma / Frag
  Systems/
    Spawner.cs             Horde spawning, difficulty ramp, swarm waves, elites
    Upgrades.cs            Level-up card pool
```

---

## Porting to Android (next step)

The gameplay is already mobile-friendly (virtual joystick, tap menus, auto-fire). To ship the
APK you add a second "head" project that shares this code:

1. Install the Android workload: `dotnet workload install android`
   (plus Android SDK / JDK; the .NET MAUI / Xamarin Android tooling provides these).
2. Move the game code into a shared library and add a `MonoGame.Framework.Android` head
   project (`dotnet new mgandroid`) referencing it. The `Game1`, `World`, etc. stay unchanged.
3. In the Android `Activity`, MonoGame runs the same `Game1`. Touch + `TouchPanel` already work;
   the keyboard/mouse paths simply go unused.
4. `dotnet build -f net8.0-android -c Release` produces the APK.

The only desktop-specific bits are the `Esc`-to-quit and mouse fallbacks — harmless on Android.
