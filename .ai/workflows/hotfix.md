# Workflow — Hotfix

Use only for production-blocking or severe defects.

Rules:

- minimal changed files
- no refactor
- no dependency change unless unavoidable and approved
- no schema change unless essential
- preserve rollback simplicity
- add focused regression coverage when practical
- record deferred cleanup separately

Steps:

1. confirm impact
2. identify minimal root cause
3. state risk and rollback
4. implement
5. build and test
6. review diff
7. update incident-relevant documentation
8. stop
