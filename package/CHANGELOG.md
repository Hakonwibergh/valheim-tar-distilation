# Changelog

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
