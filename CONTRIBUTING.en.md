# Contributing to KRouter

Thank you for your interest. This document is normative: by opening a Pull Request you agree to follow these rules.

## 1. Code of Conduct
The [Code of Conduct](CODE_OF_CONDUCT.md) applies. Violations will be moderated (warning → temporary suspension → ban).

## 2. Supported Targets
- .NET: Current LTS + Current STS (e.g. net8.0). Older frameworks are not supported.
- Platforms: Windows x64, Linux x64, macOS arm64/x64. PRs must not break builds on any target.

## 3. Issue Lifecycle
1. Create (label: `needs-triage`).
2. Triage (≤72h): labels (`bug`, `enhancement`, `question`, `design`, `good-first-issue`).
3. Discussion / Spec → `ready`.
4. Implementation → PR → Review → Merge.
5. Optional backport (`backport`).

Inactive (>30 days no activity) or unclear issues → `stale` then closed.

## 4. Branch & Git
- Default: `main` (always releasable).
- Features: `feature/<issue>-short`.
- Bug fixes: `fix/<issue>-short`.
- Hotfix: `hotfix/<version>-short`.
- No direct commits to `main` (except release bumps by maintainers).

### Commit Messages (Conventional Commits)
```
<type>(<scope>): <imperative, lower case>

<body optional>

<footer optional>
```
Types: feat, fix, perf, refactor, test, docs, build, ci, chore, revert. Reference issues (`Fixes #123`).

## 5. Pull Requests Checklist (required)
- [ ] Linked issue or justified scope.
- [ ] Tests green (`dotnet test`).
- [ ] No new analyzer warnings.
- [ ] Public API changes documented in `CHANGELOG.md` (Unreleased).
- [ ] New / changed logic covered by tests (≥1 happy, ≥1 edge).
- [ ] CLI behavior changes documented (`docs/CLI.md`).
- [ ] No dead/unreferenced files.
- [ ] Logging levels appropriate (no debug spam).

Large performance impacts must be reasoned and measured.

## 6. Code Quality
- C# official conventions, nullable enabled.
- Analyzers + xUnit analyzers: warnings must be fixed (CI treats as errors).
- No `#pragma warning disable` without comment + issue reference.
- Prefer ≤80 lines per method (justify exceptions).
- Avoid global mutable static state; ensure thread-safety.
- Public APIs fully XML documented.

## 7. Tests
- Goal: ≥70% statement coverage for core libs (informational; do not regress without justification).
- Types: Unit (pure, deterministic), Integration (DSN → SES), CLI (command handlers, exit codes, JSON), Performance (benchmarks if needed).
- No `Thread.Sleep` (use async coordination or deterministic control).
- Flaky tests: mark `[Trait("flaky","true")]` max 14 days before fix or removal.

## 8. Performance
- Avoid >O(n^2) unless justified and documented.
- Add micro-benchmarks for algorithmic hotspots (BenchmarkDotNet recommended).
- Minimize allocations in inner loops (consider Span, pooling).

## 9. Dependencies
Only add NuGet packages if:
1. Maintained.
2. License: MIT/BSD/Apache2 compatible.
3. Clear net value (>50 LoC saved).

Minor/Patch updates auto; Major requires issue + migration notes.

## 10. Security
- Validate file paths (no path traversal).
- Safe deserialization only (restricted types / options).
- Report vulnerabilities privately (see `SECURITY.md`), not via public issue.

## 11. Release Process
1. Aggregate changes under Unreleased in CHANGELOG.
2. Bump SemVer (fix=patch, feature=minor, breaking=major).
3. Tag: `vX.Y.Z`.
4. Generate GitHub Release notes.
5. Build artifacts (single-file binaries) & sign if configured.

## 12. Documentation
- Every CLI option documented (`docs/CLI.md`).
- Architecture changes → `ARCHITECTURE.md`.
- DRC rule format changes → `docs/DRC_RULES.md`.

## 13. Deprecation
- Mark with `[Obsolete("Reason; removed >= X.Y")]` + CHANGELOG note.
- Remove ≥2 minor releases later.

## 14. CI / Lint
CI must run: build, tests, analyzers, format (if configured), license header check. PR merges require green CI.

## 15. Communication
Discussions via GitHub Issues/Discussions only (no private email support). Architectural decisions: short note or ADR for larger changes.

## 16. Contribution Rights
By contributing you grant (to the extent legally possible) a non-exclusive license under MIT to the project.

---
Thank you for contributing to KRouter!

