# TarDistillation — install

One package for players and for dedicated servers. Requires **BepInEx 5.4.x** and
**Jotunn 2.30.2**; neither is bundled.

Everyone on a server needs **the same mod at the same version** — the mod adds items, so
it is marked `EveryoneMustHaveMod` and a client without it is refused at the door rather
than left holding prefabs it cannot resolve.

## Players — Thunderstore Mod Manager / r2modman

1. Select the Valheim profile you use for that server.
2. Install from Thunderstore, or for a zip you were handed: **Settings → Import local
   mod** (r2modman: *Import local mod* in the left menu), pick the zip.
3. Start the game modded, as usual.

## Players — by hand

Unpack the zip and copy `BepInEx/plugins/TarDistillation/` into your game's `BepInEx/
plugins/` — the whole folder, so that the `assets` folder lands beside the DLL:

```
BepInEx/plugins/TarDistillation/TarDistillation.dll
BepInEx/plugins/TarDistillation/assets/
```

The DLL on its own is a working mod; without `assets` the two tyrispik items just look
like the vanilla log and bark they are cloned from.

## Dedicated server

Unpack the zip and copy its `BepInEx` folder into the folder that holds
`valheim_server.exe` (Windows) or `valheim_server.x86_64` (Linux), so the DLL lands at:

```
BepInEx/plugins/TarDistillation/TarDistillation.dll
```

Anywhere under `BepInEx/plugins/` works. Then restart the server. There is no config file
to place — BepInEx writes `BepInEx/config/hakon.valheim.tardistillation.cfg` with the
defaults on the first run.

The `assets` folder that comes with it holds the item models, which a headless server
never draws. Copying it anyway is simplest and harmless; leaving it out changes nothing a
player sees, because each client draws from its own copy.

## Configuration

`BepInEx/config/hakon.valheim.tardistillation.cfg`, written with the defaults the first
time the mod runs. Edit it in a text editor with the game or server stopped, from your mod
manager's *Config editor*, or in game with
[Configuration Manager](https://thunderstore.io/c/valheim/p/Azumatt/Official_BepInEx_ConfigurationManager/)
on `F1`.

Only the server's copy matters for the chain. Those settings are admin only: Jotunn locks
the entries clientside and sends the server's values during the connect handshake, so a
player's local file is ignored.

`4 - Appearance` is the exception. Those two settings only change what one machine draws,
so they are not synced and each player keeps their own — there is nothing for a server
admin to set there.

| Section | Setting | Default | Range | Applies |
| --- | --- | --- | --- | --- |
| 1 - Splitting | Splitting station | `ArtisanTable` | any crafting station | Restart |
| 1 - Splitting | Core wood per batch | 10 | 1-100 | Immediately |
| 1 - Splitting | Resin per batch | 5 | 0-100 | Immediately (`0` needs a restart) |
| 2 - Drying | Drying station | `BlastFurnace` | any smelter-type station | Restart |
| 2 - Drying | Drying seconds | 0 | 0-86400 | Immediately |
| 3 - Burning | Tar per batch | 25 | 1-50 | Immediately* |
| 3 - Burning | Coal per batch | 10 | 0-50 | Immediately* |
| 3 - Burning | Burning seconds | 0 | 0-86400 | Immediately |
| 4 - Appearance | Custom models | `true` | on/off | Restart, local |
| 4 - Appearance | Model scale | 0.5 | 0.1-3.0 | Immediately, local |

\* Fermenters already placed and loaded keep the yield they were built with until their
zone reloads.

### The two timers

`Drying seconds` and `Burning seconds` are real seconds for one item. `0` does not mean
instant — it means the station keeps its own speed, which is the default: the furnace
dries a spik as fast as it smelts metal, and the fermenter runs its full vanilla cycle.

```ini
[2 - Drying]
Drying seconds = 120     # 2 minutes per spik

[3 - Burning]
Burning seconds = 600    # 10 minutes per batch
```

So `60` is a minute, `600` ten minutes, `3600` an hour, and `86400` — the ceiling — a day.

Both timers touch tyrispik only. Ore in the same blast furnace smelts at its normal speed
and mead ferments at its normal speed, so you can shorten the tar chain without making the
rest of the game faster. Both apply live, including to a fermenter that is already
burning: lower `Burning seconds` far enough and a batch under way can finish at once.

The two **station** settings are read once at startup. Change those only while the server
is down, never mid-session with players connected, and remember that clients need the
restart too.

Full detail, and example setups, are in `README.md`.

## Checking it loaded

`BepInEx/LogOutput.log` should contain:

```
[Info   :  TarDistillation] Tar chain: 10 core wood + 5 resin to one spik, dried in blastfurnace, fermented into 25 tar and 10 coal
```

That line is the mod confirming its own recipe and both conversions. If it is missing,
check that Jotunn loaded earlier in the same log.

## What it adds

Tar you can make instead of hauling it out of the plains, following the old Norwegian
tar-burning process: split pitch-soaked pine heartwood into **tyrispik** at an artisan
table, dry it in a **blast furnace**, then burn it slowly in a **fermenter** — which
stands in for the covered *mile* — for **25 tar and 10 coal** a batch. The plains tar pits
still work exactly as before.

See `README.md` for the full chain, the amounts, and where each step comes from.

## Removing it

Delete `BepInEx/plugins/TarDistillation/`. Any tyrispik still sitting in chests or
inventories, and anything mid-cycle in a fermenter, belongs to prefabs the game no longer
knows and will be dropped on load. Burn off the stock first.
