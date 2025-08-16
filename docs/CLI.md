# Command-Line Interface

KRouter provides a minimal cross-platform CLI for routing KiCad boards.

## Build

```bash
./build-cli.sh
```

This script restores dependencies, builds the project and publishes standalone executables for Linux, Windows and macOS.

## Usage

```bash
krouter route --in board.dsn --out board.ses [--profile Fast|Balanced|Quality]
```

### Options

- `-i, --in` — Input DSN file (required)
- `-o, --out` — Output SES file (required)
- `-p, --profile` — Routing profile (default: `Balanced`)

### Example

```bash
krouter route --in samples/boards/example.dsn --out example.ses --profile Quality
```

## KiCad Integration

1. Export DSN from KiCad: **File → Export → Specctra DSN**
2. Run KRouter: `krouter route --in board.dsn --out board.ses`
3. Import SES into KiCad: **File → Import → Specctra Session**
