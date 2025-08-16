[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com)
[![Build Status](https://github.com/arnisz/krouter/workflows/CI/badge.svg)](https://github.com/arnisz/krouter/actions)

KRouter ist ein moderner, DRC-konformer PCB-Autorouter für KiCad, entwickelt in C#/.NET 8. Er bietet deterministisches Routing mit vollständiger Design-Rule-Compliance und eine benutzerfreundliche GUI.

[English version below](#english)

## 🚀 Features

- ✅ **100% DRC-Konform**: Keine Regelverletzungen während des Routings
- 🎯 **Deterministisch**: Reproduzierbare Ergebnisse mit Seeds
- ⚡ **Performant**: Multithreading-Support für große Boards
- 🔧 **Flexibel**: Anpassbare Profile (Fast/Balanced/Quality)
- 📊 **Transparent**: Detaillierte Reports und Visualisierungen
- 🖥️ **Cross-Platform**: Windows, Linux, macOS

## 📦 Installation

### Voraussetzungen
- .NET 8 SDK oder Runtime
- KiCad 6.0 oder höher

### Download
Laden Sie die neueste Version von der [Releases-Seite](https://github.com/arnisz/krouter/releases) herunter.

### Build from Source
```bash
git clone https://github.com/arnisz/krouter.git
cd krouter
dotnet build -c Release
```

## 🛠️ CLI

Ein minimales CLI steht für Headless-Workflows zur Verfügung.

```bash
./build-cli.sh
./dist/linux/krouter route --in board.dsn --out board.ses
```

Weitere Details finden sich in [docs/CLI.md](docs/CLI.md).

## <a id="english"></a> English

 - 🚀 Features
 - ✅ 100% DRC-Compliant: No rule violations during routing
 - 🎯 Deterministic: Reproducible results with seeds
 - ⚡ Performant: Multithreading support for large boards
 - 🔧 Flexible: Customizable profiles (Fast/Balanced/Quality)
 - 📊 Transparent: Detailed reports and visualizations
 - 🖥️ Cross-Platform: Windows, Linux, macOS

## 📦 Installation

### Prerequisites
 - .NET 8 SDK or Runtime
 - KiCad 6.0 or higher

### Download
Download the latest version from the Releases page.

### Build from Source
```bash

git clone https://github.com/arnisz/krouter.git
cd krouter
dotnet build -c Release
```

## 🛠️ CLI

A minimal CLI is available for headless workflows.

```bash
./build-cli.sh
./dist/linux/krouter route --in board.dsn --out board.ses
```

See [docs/CLI.md](docs/CLI.md) for details.
