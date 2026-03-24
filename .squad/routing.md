# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Architecture, scope, and technical decisions | Morgana | system boundaries, trade-offs, design reviews |
| Planning, backlog, and delivery management | Circe | issue triage, sequencing, milestones, acceptance criteria |
| Backend and data services | Hecate | APIs, auth flow wiring, persistence, integrations |
| Frontend and UX implementation | Selene | UI components, client state, UX polish |
| UX design, review, and user experience | Calista | information hierarchy, message tone, edge case UX, accessibility |
| Testing and quality verification | Nyx | test plans, unit/integration tests, regression checks |
| DevOps and platform operations | Rowan | CI/CD, containers, infra config, deployment workflows |
| Documentation and technical writing | Freya | developer docs, API docs, runbooks, release notes |
| Security architecture and threat modeling (pre-build) | Vespera | secure design guidance, trust boundaries, abuse-case prevention |
| Security review and hardening verification (post-build) | Vespera | security regression checks, vuln review, remediation guidance |
| Code review | Morgana | PR review, quality gate, cross-domain sanity checks |
| Async issue work (bugs, tests, small features) | @copilot 🤖 | well-defined isolated tasks matching capability profile |
| Session logging | Scribe | automatic — never needs routing |

## Rules

1. Eager by default — spawn all agents who can usefully start now.
2. Scribe always runs after substantial work in background mode.
3. Quick factual questions can be answered directly by coordinator.
4. "Team" requests fan out to all relevant members in parallel.
5. Pair build + test by default (implementation plus Nyx in parallel).
6. Route planning/management-first work to Circe.
7. Route final architecture or quality gates through Morgana.
8. For security-impacting work, involve Vespera before implementation and again before merge.
9. For documentation deliverables, route Freya and Vespera together for security-accurate docs/runbooks.
