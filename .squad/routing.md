# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Architecture, scope, and technical decisions | Morgana | system boundaries, trade-offs, design reviews |
| Planning, backlog, and delivery management | Circe | issue triage, sequencing, milestones, acceptance criteria |
| Backend and data services | Hecate + Fern | APIs, auth flow wiring, persistence, integrations |
| Frontend and UX implementation | Selene | UI components, client state, UX polish |
| UX design, review, and user experience | Calista | information hierarchy, message tone, edge case UX, accessibility |
| Motion design, animations, and visual experience | Lyra | micro-interactions, transitions, loading states, error toast animation |
| Performance profiling and optimization | Ember | startup time, API pipeline parallelism, memory usage, 60fps animations, Android perf |
| Testing and quality verification | Nyx | test plans, unit/integration tests, regression checks |
| DevOps and platform operations | Rowan | CI/CD, containers, infra config, deployment workflows |
| Documentation and technical writing | Freya | developer docs, API docs, runbooks, release notes |
| Security architecture and threat modeling (pre-build) | Vespera | secure design guidance, trust boundaries, abuse-case prevention |
| Security review and hardening verification (post-build) | Vespera | security regression checks, vuln review, remediation guidance |
| Code review and quality gate | River | clean code, efficiency, no duplication, professional comments, no spaghetti |
| Architecture decisions and PR sign-off | Morgana | system boundaries, trade-offs, design reviews |
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
8. **Vespera consultation is MANDATORY — before AND after every new build.** She may pair with Morgana during design; she SHALL always pair with Freya so security is both implemented and documented.
9. **Hecate and Fern always run in parallel** for backend work — never serialize them.
10. **River's lockout rule is advisory, not automatic.** Lockouts are reserved for serious rejections where a full rework is needed. For minor alterations and small fixes, the original author may address River's feedback directly. River guides and improves — she does not block the coven unnecessarily.
11. **Always address the project owner as Sigge** — never Maja or any other name. This applies to all agents and the coordinator in every session.
