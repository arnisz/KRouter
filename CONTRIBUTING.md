# Beitragen zu KRouter

Danke für dein Interesse an KRouter. Dieses Dokument ist verbindlich und beschreibt, wie Beiträge einzureichen sind. Mit einem Pull Request erklärst du dich mit diesen Regeln einverstanden.

## 1. Verhaltenskodex
Es gilt der [Code of Conduct](CODE_OF_CONDUCT.md). Verstöße werden konsequent moderiert (Verwarnung, temporärer Ausschluss, Bann).

## 2. Unterstützte Zielumgebungen
- .NET: Aktuelle LTS + Aktuelle STS (z.B. net8.0). Ältere Frameworks werden nicht unterstützt.
- Plattformen: Windows x64, Linux x64, macOS arm64/x64. PRs dürfen Builds auf diesen Plattformen nicht brechen.

## 3. Issue Lifecycle
1. Neu anlegen (Label: `needs-triage`).
2. Triage innerhalb von 72h (Zuweisung Labels: `bug`, `enhancement`, `question`, `design`, `good-first-issue`).
3. Diskussion / Spezifikation → Status `ready`.
4. Implementierung in Branch → PR → Review → Merge.
5. Optionales Backporting (Label: `backport`).

Unklare oder inaktive Issues (>30 Tage ohne Reaktion) werden geschlossen (`stale`).

## 4. Branch & Git-Konventionen
- Default Branch: `main` (immer releasable).
- Feature Branches: `feature/<issue-id>-kurz-beschreibung` (z.B. `feature/123-fast-ripup`).
- Bugfix Branches: `fix/<issue-id>-problem`.
- Hotfix (Release-Korrektur): `hotfix/<version>-kurz`.
- Keine direkten Commits auf `main` außer Release-Versionsbump durch Maintainer.

### Commit Messages
Format (Conventional Commits):
```
<type>(<scope>): <imperativ, klein geschrieben>

<body optional>

<footer optional>
```
Types: `feat`, `fix`, `perf`, `refactor`, `test`, `docs`, `build`, `ci`, `chore`, `revert`.
Pflicht bei Bugfixes: Issue Referenz (`Fixes #123`).

## 5. Pull Requests (PR)
PR Checkliste (PR wird sonst abgelehnt):
- [ ] Issue existiert oder klarer Scope im PR-Text begründet.
- [ ] Alle Tests grün (`dotnet test`).
- [ ] Keine neuen Analyzer-Warnungen.
- [ ] Öffentliche API Änderungen dokumentiert (`CHANGELOG.md` unter "Unreleased").
- [ ] Neue/angepasste Funktionen haben Tests (min. 1 Happy Path + 1 Edge Case).
- [ ] Falls CLI Verhalten beeinflusst → `docs/CLI.md` aktualisiert.
- [ ] Keine toten/useless Dateien.
- [ ] Kein Debug-/Console-Spam (Logging Levels korrekt).

Reviewer prüfen zusätzlich Performance-/Speicher-Auswirkungen größerer Features.

## 6. Code Style & Qualität
- C# Style: Offizielle .NET Konventionen + Nullable aktiv.
- Analyzer: Standard + xUnit Analyzer (Tests). Warnungen = Buildfehler bei CI.
- Keine `#pragma warning disable` ohne Kommentar + Issue Referenz.
- Max Method Length: ~80 Zeilen (Ausnahmen dokumentieren).
- Keine statischen globalen Zustände außer klar begründet (Thread-Sicherheit sicherstellen).
- Public APIs mit XML-Dokumentation.

## 7. Tests
- Mindestanforderung: Statements Coverage Ziel >= 70% für Core Bibliotheken (informativ, aber PR darf Ziel nicht reduzieren ohne Begründung).
- Testarten:
  - Unit: Core.* Klassen (Determinismus, keine IO/Netzwerk Side Effects).
  - Integration: End-to-End Routing Beispiel DSN → SES.
  - CLI: Command Handler (Return Codes, JSON Output, Fehlerfälle).
- Keine Thread.Sleep in Tests (verwende Task/Wait Handles oder deterministische Strategien).
- Flaky Tests werden priorisiert gefixt oder temporär mit `[Trait("flaky","true")]` markiert – max 14 Tage.

## 8. Performance
- Algorithmen mit O(n^2)+ Komplexität über kleine Testdaten hinaus nur mit Begründung.
- Neue kostenintensive Pfade: Ergänze Benchmark (`BenchmarkDotNet` Projekt wenn vorhanden) oder micro-benchmark Beispiel.
- Keine unnötigen Allokationen in Hot Paths (Routing-Schleifen). `Span<>`, `ArrayPool` wo sinnvoll.

## 9. Abhängigkeiten
- Externe NuGet-Pakete nur wenn: (a) aktiv gepflegt, (b) Lizenz kompatibel (MIT/BSD/Apache2), (c) Mehrwert > 50 LoC eigener Code.
- Aktualisierung via Dependabot/Manuell: Minor+Patch automatisch, Major per Issue + Migrationsplan.

## 10. Sicherheit
- Keine Ausführung unvalidierter Eingaben (Pfad Traversal prüfen).
- Deserialisierung nur mit erlaubten Typen / sicheren Optionen.
- Sicherheitsrelevante Bugs NICHT als öffentliches Issue, sondern per Security Policy (`SECURITY.md`).

## 11. Release Prozess
1. Sammeln Änderungen in `CHANGELOG.md` unter "Unreleased".
2. Versionserhöhung (SemVer) gemäß: fix = patch, neue Funktion = minor, Breaking = major.
3. Tag: `vX.Y.Z`.
4. GitHub Release Notes generieren.
5. Artefakt Build (Single File Binaries) & Signieren falls konfiguriert.

## 12. Dokumentation
- Jede neue CLI Option dokumentieren (`docs/CLI.md`).
- Architekturänderungen: `ARCHITECTURE.md` ergänzen.
- DRC Regeländerungen: `docs/DRC_RULES.md`.

## 13. Deprecation Policy
- Deprecation mit `[Obsolete("Begründung; Entfernt in >= X.Y")]` + CHANGELOG Hinweis.
- Entfernung frühestens nach 2 Minor Releases.

## 14. Linters / CI
- CI muss mindestens prüfen: Build, Tests, Analyzer, Format (falls dotnet format eingebunden), Lizenz-Header.
- PR ohne grünes CI wird nicht gemergt.

## 15. Kommunikation
- Diskussionen: GitHub Issues / Discussions. Kein privates Support-Mail.
- Entscheidungen werden festgehalten (Kurzer Abschnitt im Issue/PR oder ADR bei größerem Impact).

## 16. Eigentumsübertragung
Mit Submission überträgst du (soweit rechtlich möglich) ein einfaches Nutzungsrecht an das Projekt unter MIT.

---
Vielen Dank für deine Beiträge!

Eine englische Version findest du in `CONTRIBUTING.en.md`.
