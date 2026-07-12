# Build and Verification Rules

Backend change:
- build affected .NET projects
- run relevant tests

Frontend change:
- use existing lockfile
- install nothing new
- build Angular
- verify template/type compilation

Schema change:
- build
- generate/review migration
- review snapshot

Workflow change:
- validate YAML and paths
- run underlying build commands when possible

Always inspect final diff and report failures honestly.
