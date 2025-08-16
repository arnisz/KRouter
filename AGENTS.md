## agents.md

```markdown
# Agent Guidelines for KRouter

This document provides guidelines for AI agents and automated tools working with the KRouter codebase.

## Core Principles

### Invariants to Preserve
1. **DRC Compliance**: Every routing operation MUST maintain 100% design rule compliance
2. **Determinism**: Given identical inputs and seed, output MUST be identical
3. **Coordinate Precision**: All coordinates use nanometer integers, never floating point
4. **Test Coverage**: Maintain >80% test coverage for all new code

## Architecture Constraints

### Do NOT:
- Introduce circular dependencies between layers
- Add external network calls or telemetry
- Use floating-point for geometric calculations
- Copy code from incompatibly licensed projects
- Create God objects or mega-classes
- Add dependencies without careful consideration

### Always:
- Keep Core layer framework-agnostic
- Maintain clean separation between I/O and logic
- Use dependency injection for testability
- Document public APIs with XML comments
- Add unit tests for new functionality
- Update relevant documentation

## Code Modification Process

### Before Making Changes
1. Understand the existing architecture
2. Check for related tests
3. Review DRC implications
4. Consider performance impact

### Definition of Ready (DoR)
- [ ] Issue/requirement clearly defined
- [ ] Acceptance criteria specified
- [ ] Dependencies identified
- [ ] Test strategy defined

### Definition of Done (DoD)
- [ ] Code compiles without warnings
- [ ] All tests pass
- [ ] New tests added for new functionality
- [ ] Documentation updated
- [ ] DRC validation passes on test boards
- [ ] Performance benchmarks acceptable
- [ ] Code reviewed (if applicable)

## Common Tasks

### Adding a New Cost Function Parameter
1. Update `CostFunction` class
2. Add weight property to `RoutingProfile`
3. Update profile presets
4. Add tests for new parameter
5. Document in ALGORITHMS.md
6. Update GUI/CLI if user-configurable

### Implementing a New Routing Algorithm
```csharp
public class NewRouter : IRoutingAlgorithm
{
    public RoutingPath? FindPath(
        RoutingNode start, 
        RoutingNode end, 
        RoutingGraph graph, 
        CostFunction costFunction)
    {
        // Implementation
        // MUST respect all DRC rules
        // MUST be deterministic
        // SHOULD be efficient
    }
}
