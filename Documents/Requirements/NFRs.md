## Non-Functional Requirements / Architectural Constraints

| ID     | Category        | Requirement                                      | Applies To |
|--------|-----------------|--------------------------------------------------|------------|
| NFR-01 | Architecture    | Data access via ADO.NET only  | All stories |
| NFR-02 | CI/CD           | All pipelines via GitHub Actions                 | All stories |
| NFR-03 | Infrastructure  | AWS-only deployment with Terraform IaC           | All stories |
| NFR-04 | Infrastructure  | All cloud resources tracked in version control   | All stories |
| NFR-05 | Quality | Unit test should cover 90% of components UI, Business Logic, and Data Access layers, and run on CI | All stories |
| NFR-06 | Quality | Integration test should cover 100% of API services and run on CI | All stories |
| NFR-07 | Quality | End to End test should cover 100% of critical functionality and run on demand. | All stories |
| NFR-08 | Security | All secrets managed via AWS Secrets Manager | All stories |
| NFR-09 | Observability | Logging implemented as cross-cutting concern via middleware and decorators | All stories |
| NFR-10 | Observability | Structured logging with correlation IDs and contextual metadata using Serilog or Application Insights | All stories |
| NFR-11 | Observability | Centralized exception handling and error logging via middleware | All stories |