# Tar Distillation

[github.com/Hakonwibergh/valheim-tar-distilation](https://github.com/Hakonwibergh/valheim-tar-distilation)

A Valheim mod that adds a second way to get tar: burn it out of resinous pine wood,
the way Norwegian tar burners did it. The plains tar pits are untouched — you can
still go and farm them, this is just the other option.

The chain is deliberately gated behind the **blast furnace**, which needs an artisan
table and therefore Moder. You cannot reach tar this way any sooner than you could
have walked into the plains and taken it.

## The chain

| Step | Station | In | Out |
| --- | --- | --- | --- |
| 1. Split | Artisan table | 10 core wood + 5 resin | 1 Tyrispik |
| 2. Dry | Blast furnace (burns coal as usual) | 1 Tyrispik | 1 Dried Tyrispik |
| 3. Burn | Fermenter | 1 Dried Tyrispik | 25 Tar + 10 Coal |

One full run takes a whole fermenter cycle — the same wait as a batch of mead — so you
scale it by building more fermenters, the way you scale smelters. If that is too slow for
your server, `Burning seconds` shortens it without touching mead; see
[The two timers](#the-two-timers).

Everything in that table is configurable; see [Configuration](#configuration).

## Why these steps

From [Tjærebrenning](https://no.wikipedia.org/wiki/Tj%C3%A6rebrenning) on Norwegian
Wikipedia, the real process was:

- **Tyri** — the resin-soaked *kjerneved* (heartwood) and roots of pine. The best came
  from stumps that had stood 10–15 years or more, until the soft outer wood had rotted
  off and only the pitch-soaked core was left. In game that is **core wood**, which is
  literally the heartwood of pine, plus resin.
- **Spik** — the stump wood was cleaned, coarse-split, then fine-split into *spik*,
  sorted and **stacked to dry**. The article notes this preparation alone could take up
  to a man-year of work. That is step 1 and step 2 here.
- **The mile** — the dried spik was stacked in a funnel-shaped *tjæredal* lined with
  birch bark, covered with turf, and lit so it smouldered with as little air as
  possible. Tar melted out and ran down a hollowed log to be collected. The fermenter
  stands in for the mile: a covered vessel that works slowly and is tapped at the
  bottom. The small version, the *minimile*, burned for about half an hour — close
  enough to a fermenter cycle that the timing needs no apology.
- **Charcoal** — "store mengder trekull blir igjen etter brenningen og er verdsatt."
  A burn leaves a lot of valuable charcoal behind, so tapping the fermenter gives coal
  along with the tar. It roughly pays back what the blast furnace burned to dry the
  wood, which is the same loop the tar burners were in.
- **The yield** — here the mod stops being faithful on purpose. Real tar came to
  20–25 % of the dry weight of the wood: about 1 m³ of pine (~280 kg) gave some 60 kg,
  around 50 litres. Scaled honestly onto vanilla item weights that is about 6 tar for ten
  core wood, which is a miserable return for three stations and a fermenter cycle. So the
  default is **25 tar, half a stack**, as if you could squeeze the wood dry. Set
  `Tar per batch = 6` if you want the honest trickle back.

What is deliberately **not** modelled: the fractions. Real tar was tapped in layers —
thin watery turpentine first, then acidic *tjærelóg*, then clean tar at the bottom —
and the burner read the smoke colour to steer the temperature. That is a mini-game, not
a recipe, so the fermenter just hands over finished tar.

## What it looks like

Both items carry their own model: a bundle of five split sticks, raw in a resinous orange
and dried in a darker, cracked brown, with a matching inventory icon each. Underneath they
are still clones of core wood and elder bark — that is what gives them the right weight,
stack size and physics — but nothing of the log or the bark is left on screen.

The parts live in `assets/`, beside the plugin:

```
BepInEx/plugins/TarDistillation/
  TarDistillation.dll
  assets/
    Tyrispik.obj  Tyrispik_albedo.png  Tyrispik_icon.png
    TyrispikDried.obj  TyrispikDried_albedo.png  TyrispikDried_icon.png
```

They are read when the game loads. **If the folder is missing the mod still works** — the
items fall back to looking like a log and a piece of bark, and the log in
`BepInEx/LogOutput.log` names the file it could not read. Nothing about the chain changes.

There is no asset bundle, which is the usual way a Valheim mod ships a model and the one
part of this that needs the Unity editor. A mesh is only numbers and a PNG only pixels, so
both are read straight off disk at runtime; the one thing that genuinely cannot be built
that way is a **shader**, so the material is cloned from the vanilla wood the item was
cloned from and only its albedo is swapped. That is what keeps the bundle lit, wetted and
fogged like the wood lying beside it instead of like a bright sticker.

Two settings under [4 - Appearance](#4---appearance) cover the size and an off switch.
They are local to each machine, not synced from the server.

## Install

Needs [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)
and [Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).

One package covers players and dedicated servers: install it with a mod manager, or
unpack the zip and copy its `BepInEx` folder over the game or server root. Steps for both
are in `package/INSTALL.md`.

## Multiplayer

The mod adds items, so it is marked `EveryoneMustHaveMod` with `VersionStrictness.Minor`:

- **Install it on the server and on every client**, at the same version. A client
  without it will be refused, which is the point — the alternative is a client that
  cannot resolve the item prefabs.
- **The server's config wins**, for everything that touches the chain. Those settings are
  admin-only, so Jotunn locks the entries on clients and pushes the server's values down as
  part of the connect handshake. You only have to keep the server's config file correct.
- **Except `4 - Appearance`.** Model scale and the off switch for the shipped models change
  what one machine draws, nothing more, so they are not synced and every player keeps their
  own. A server does not need the `assets` folder at all - it draws nothing - though there
  is no harm in the whole package being unpacked there, which is what the install steps do.
- Amounts (`Core wood per batch`, `Resin per batch`, `Tar per batch`, `Coal per batch`)
  are applied to the live recipe and the fermenter when they change, so a synced value
  takes effect without a restart. Fermenters that are already placed and loaded keep the
  yield they were built with until their zone reloads.
- The two **station** settings are read once at startup and need a restart on every
  machine. Do not change those mid-session.
- Tapping is handled by the fermenter's owner only — vanilla checks that before it
  schedules the tap — so the charcoal cannot be duplicated by a second player standing
  at the same fermenter.
- On a **dedicated server** the items are built from an `ObjectDB.Awake` prefix as well as
  from Jotunn's usual vanilla-prefabs event. That event hangs off `ObjectDB.CopyOtherDB`,
  which in the client build only the main menu calls, and a headless server is a different
  build of the game — so the content is registered either way rather than on an assumption.

## Configuration

Everything in the chain is tunable: which stations are used, how much goes in, how much
comes out, and how long each step takes.

### Where the settings live

`BepInEx/config/hakon.valheim.tardistillation.cfg`, written with the defaults the first
time the mod runs. Three ways to edit it:

- **In game**, if you have [Configuration Manager](https://thunderstore.io/c/valheim/p/Azumatt/Official_BepInEx_ConfigurationManager/)
  installed: press `F1`, find **TarDistillation**. Values apply as you change them, which
  makes it the quickest way to feel out the timers.
- **From a mod manager**: in Thunderstore Mod Manager / r2modman, select the mod and use
  *Config editor*.
- **In a text editor**, with the game or server shut down.

On a server, the server's file is the only one that matters for the chain itself; the two
appearance settings stay local to each machine — see [Multiplayer](#multiplayer).

### The settings

#### 1 - Splitting

| Setting | Default | Range | Applies |
| --- | --- | --- | --- |
| Splitting station | `ArtisanTable` | any crafting station | Restart |
| Core wood per batch | 10 | 1–100 | Immediately |
| Resin per batch | 5 | 0–100 | Immediately (`0` needs a restart) |

**Splitting station** is what gates the chain. `ArtisanTable` needs Moder, so tar by
distillation is never cheaper than a trip to the plains; `Workbench` or `Forge` hand it to
you in the meadows instead. **Resin per batch** set to `0` drops resin from the recipe
altogether, and since that adds or removes an ingredient rather than retuning one, it is
the single amount that waits for a restart.

#### 2 - Drying

| Setting | Default | Range | Applies |
| --- | --- | --- | --- |
| Drying station | `BlastFurnace` | any smelter-type station | Restart |
| Drying seconds | 0 | 0–86400 | Immediately |

**Drying station** burns fuel exactly as it normally does. `Smelter` or `CharcoalKiln`
move drying into the black forest, which pairs with moving the splitting station earlier —
leaving one of the two at its default is what keeps the gate closed.

#### 3 - Burning

| Setting | Default | Range | Applies |
| --- | --- | --- | --- |
| Tar per batch | 25 | 1–50 | Immediately\* |
| Coal per batch | 10 | 0–50 | Immediately\* |
| Burning seconds | 0 | 0–86400 | Immediately |

\* A fermenter that is already placed and loaded keeps the yield it was built with until
its zone reloads; new ones use the new numbers at once.

**Tar per batch** is the dial to turn if the chain feels too generous: `25` is half a
stack and assumes you squeeze the wood dry, while `6` is roughly what the historical
20–25 % yield works out to. **Coal per batch** set to `0` turns the by-product off, if
you would rather the chain cost fuel outright instead of paying back what the furnace
burned — at `10` a run keeps itself in fuel.

#### 4 - Appearance

| Setting | Default | Range | Applies |
| --- | --- | --- | --- |
| Custom models | `true` | on/off | Restart |
| Model scale | 0.5 | 0.1–3.0 | Immediately\* |

\* Tyrispik already lying on the ground keeps the size it was dropped at.

The one section that is **not** admin-only and not synced: both settings change what a
single machine draws and nothing else, so a server has no opinion and players on the same
server can disagree about them freely.

**Custom models** off falls back to the vanilla log and bark, which is also what happens
by itself when the `assets` folder is missing. **Model scale** multiplies the size the
bundle was modelled at: `1` is about a metre and a fifth across, the default `0.5` an
armful of kindling, `0.25` a handful. Weight, stack size and the collider you kick along
the ground are untouched — it is the mesh and nothing else. Turning it in the in-game
config menu and dropping a fresh bundle is the quickest way to settle on a size.

### The two timers

`Drying seconds` and `Burning seconds` are the two settings worth spelling out, because
`0` does not mean "instant" — it means **"leave this step alone and let the station time
it"**, which is the default for both. The blast furnace then dries a spik in the time it
smelts a bar of metal, and the fermenter runs its full vanilla mead cycle, which is far
and away the longest step.

Any other value is **real seconds for one item**:

| Value | One batch takes |
| --- | --- |
| `0` | whatever the station does (the default) |
| `60` | 1 minute |
| `300` | 5 minutes |
| `600` | 10 minutes |
| `1800` | 30 minutes |
| `3600` | 1 hour |
| `86400` | 24 hours (the ceiling) |

Two things to know about them:

- **Only tyrispik is retimed.** Ore in the same blast furnace still smelts at the vanilla
  speed, and mead in the same fermenter still takes the vanilla cycle. Shortening the tar
  chain does not speed up the rest of the game.
- **They apply live, to stations already running.** Drop `Burning seconds` while a
  fermenter is burning and it recalculates against the time already elapsed, so a batch
  that has been going a while can finish the moment you set it. Hover text stays honest
  either way — the progress you see is read from the same number the mod adjusts.

Vanilla keeps one timer per *station*, not per recipe, so this works by patching the clock
for our item only: `Smelter.m_secPerProduct` is swapped in for the single update call
working on a tyrispik, and the fermenter's elapsed time is scaled so it crosses the
station's finish line at the configured moment.

### Example setups

**Shorter chain for a small server** — a batch every ten minutes instead of a mead cycle:

```ini
[2 - Drying]
Drying seconds = 120

[3 - Burning]
Burning seconds = 600
```

**Tar in the black forest**, long before Moder — both stations have to move, and both need
a restart:

```ini
[1 - Splitting]
Splitting station = Workbench

[2 - Drying]
Drying station = CharcoalKiln
```

**The realistic trade** — the historical yield, more wood in, no charcoal back:

```ini
[1 - Splitting]
Core wood per batch = 20

[3 - Burning]
Tar per batch = 6
Coal per batch = 0
```

Station names are Jotunn's, and the config file lists every valid value above each entry,
so you never have to guess at the spelling.

## Building

Needs the .NET SDK. Valheim, BepInEx and Jotunn supply the references, so the paths in
`TarDistillation.csproj` must point at a real install:

```sh
dotnet build
```

`ProfileDir` has to point at a profile that actually holds BepInEx and Jotunn, because the
build references their DLLs. It defaults to the Steam install under
`C:\Program Files (x86)` and the profile named `Default`, which is the one a fresh
Thunderstore Mod Manager install creates — if yours is named anything else, set it.

The tidiest way is a **`Local.props`** next to the csproj. It is gitignored, so your paths
stay on your machine:

```xml
<Project>
  <PropertyGroup>
    <ProfileDir>$(APPDATA)\Thunderstore Mod Manager\DataFolder\Valheim\profiles\MyProfile</ProfileDir>
    <!-- <ValheimDir>D:\SteamLibrary\steamapps\common\Valheim</ValheimDir> -->
  </PropertyGroup>
</Project>
```

The command line works too, and wins over both:

```sh
dotnet build -p:ValheimDir="D:\SteamLibrary\steamapps\common\Valheim" -p:ProfileDir="..."
```

To drop the built DLL straight into that profile while developing, add `-p:Deploy=true`.
It copies `assets/` along with it, since the models are read from beside the DLL. It is
off by default so a build cannot plant a second copy of the plugin beside one installed
from Thunderstore — two copies means BepInEx loads it twice.

### Packaging

```powershell
.\pack.ps1
```

Builds Release and writes one archive to `dist/`: `TarDistillation-<version>.zip`, with
`manifest.json`, `icon.png`, `README.md` and `INSTALL.md` at the root, and the plugin plus
its `assets/` folder at `BepInEx/plugins/TarDistillation/`. That is the layout Thunderstore
takes on upload, the layout mod managers import, and the layout a server root already has,
so the same zip serves players and the server.

The models go in the package, not in the DLL. Keeping them loose is what lets a server
admin swap a texture or drop the folder entirely without a rebuild.

Entry paths are written with forward slashes, so `unzip` on a Linux server builds the
folder tree instead of one oddly named file.

BepInEx and Jotunn are dependencies and are not bundled.

## Status

Compiles clean against Valheim build 25909357, BepInEx 5.4.2351 and Jotunn 2.30.2, and
every game and Jotunn API it touches was checked against those assemblies. It has not
been run in-game yet — that is the next step, ideally on a second profile before it goes
near a live world.

The models were checked as far as they can be without starting the game: both meshes read
back as 450 triangles with every face pointing outwards, the sides smoothing along each
stick while the sawn ends stay sharp, UVs inside 0–1 and the whole bundle centred on the
item's pivot. What that cannot tell you is how it sits in Valheim's own light, so the
first run is worth doing with a bundle on the ground in front of you: check the size
against `Model scale`, check it is not half-sunk in the ground, and check the texture
reads as wood rather than as orange plastic.
