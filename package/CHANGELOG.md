# Changelog

## 0.0.6

- **`Model scale` defaults to 0.5 again**, back up from the 0.25 that 0.0.5 shipped. A
  dropped bundle is roughly **0.6 m** across, a generous armful rather than the 0.3 m the
  concept art marks. Set it to 0.25 if you preferred the smaller one — the setting is local
  to your own machine, and the range is unchanged.
- **Softer inventory icons.** Same two bundles of split logs, repainted with the edges
  and the grain eased off. The 0.0.5 pair were cut hard enough that at 32 px the detail
  turned to noise; these hold their shape in the slot and still read apart at a glance,
  raw against dried.
- Nothing else changed. The mod checks versions at `Minor` strictness, so **0.0.5 and
  0.0.6 still accept each other**, and the settings that moved are local anyway: a
  player who stays on 0.0.5 simply keeps the smaller bundle and the older icons.

## 0.0.5

- **New inventory icons.** A stack of split logs seen end-on, bark and annual rings and
  all, in place of the flat orange silhouette 0.0.4 shipped with. The visible log ends
  give the silhouette something to hold onto, so it still reads at 32 px, and the raw and
  dried bundles stay apart at a glance. They are a good deal prettier than the model they
  label, and will stay ahead of it until the mesh is redone to match.
- **`Model scale` now defaults to 0.25**, down from 0.5, which puts a dropped bundle at
  roughly 0.3 m — the size the concept art marks it at. If you preferred the larger
  bundle, set it back to 0.5; the setting is local to your own machine either way.
- Nothing else changed: same chain, same recipe, same amounts, same timers. Only the patch
  number moved, and the mod checks versions at `Minor` strictness, so **a 0.0.4 client can
  still join a 0.0.5 server** and the other way round — nobody is locked out while the
  server updates. The two settings that did change are local anyway, so a player still on
  0.0.4 simply keeps the old icons.

## 0.0.4

- **Tyrispik and dried tyrispik have their own look.** A bundle of split pine with its
  own wood texture and its own inventory icon, instead of the round log and the piece of
  elder bark they are cloned from. The dried bundle is the darker, cracked one.
- The models are plain OBJ and PNG files in `assets/`, beside the plugin, read when the
  game loads — no asset bundle, so the material is borrowed from the vanilla wood and the
  items still darken in rain and sit in the fog like everything around them.
- New section `4 - Appearance` with two settings, `Custom models` and `Model scale`. They
  are the first settings in the mod that are **not** server-synced: they change nothing
  but what one machine draws, so each player keeps their own.
- Copy `assets/` along with the DLL. Without it the items quietly fall back to the vanilla
  look and the log says which file it could not read.

## 0.0.3

- Source code published at
  [github.com/Hakonwibergh/valheim-tar-distilation](https://github.com/Hakonwibergh/valheim-tar-distilation),
  now linked from the package page.
- Build: the Thunderstore profile path is a placeholder instead of a personal one, and a
  gitignored `Local.props` can hold your own paths. No change to the mod itself.

## 0.0.2

- **A batch now gives 25 tar and 10 coal**, up from 6 and 2. Half a stack of tar per run:
  less faithful to the real process, which gave tar worth about a fifth of the weight of
  the wood, but worth the three stations and the fermenter cycle it costs. Set
  `Tar per batch = 6` and `Coal per batch = 0` for the historical trickle.
- Every setting now explains itself in the config file and in the in-game config menu:
  units, what `0` means, and whether it takes effect at once or needs a restart.
- README documents configuration properly — where the file is, three ways to edit it,
  per-setting tables with ranges, the two timers in detail, and example setups.

## 0.0.1

- First release. Tar from resinous pine in three steps: split core wood and resin into
  tyrispik at an artisan table, dry it in a blast furnace, burn it in a fermenter.
