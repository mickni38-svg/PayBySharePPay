# Workflow — Code Review Only

Do not modify code.

Review in this order:

1. correctness
2. security
3. business rules
4. architecture
5. data integrity
6. payment/idempotency/state transitions
7. error handling
8. compatibility
9. performance
10. maintainability
11. tests
12. documentation

For each finding include:

- severity
- file/location
- problem
- impact
- recommendation
- confidence

Do not turn stylistic preferences into defects.

Stop after findings and summary.
