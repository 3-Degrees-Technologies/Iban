# Parsing & Normalization Logic

**Objective**: Implements canonical normalization (strip separators, uppercase, trim) with parsing pipeline and invalid character handling.

**Task**: Implements canonical normalization (strip separators, uppercase, trim) with parsing pipeline and invalid character handling.

**Implementation Approach**: Use test-driven development approach following existing Centro patterns.

**Definition of Done**:

* Implementation completed using test-driven development
* Core functionality implemented
* Code review completed and approved

**Testing Approach**:

* Use Test-Driven Development (TDD) - unit tests are written AS PART of implementation
* DO NOT create separate integration test suites or comprehensive integration testing
* Integration tests are ONLY for Bruno API test tickets or specific test harness creation
* All testing should be integrated into the implementation process using TDD methodology

**Important Note for Implementation**
If any requirements are unclear or you need additional context, please ask clarifying questions rather than making assumptions. It's better to get confirmation on approach, scope, or technical details before implementation.

**Infrastructure Notice**
DO NOT attempt to access or modify databases directly. DO NOT attempt to change LocalStack configuration (centro-localstack:4566). For all database and infrastructure questions, contact Agent Black.

**Labels**
backend
