[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com)
[![Build Status](https://github.com/yourusername/krouter/workflows/CI/badge.svg)](https://github.com/yourusername/krouter/actions)

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
Laden Sie die neueste Version von der [Releases-Seite](https://github.com/yourusername/krouter/releases) herunter.

### Build from Source
```bash
git clone https://github.com/yourusername/krouter.git
cd krouter
dotnet build -c Release
