**AS S I G N M E N T 1** EE: 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence. SE3090 – Software Engineering Frameworks** 

**Year 3 | Semester 1 | 2026** es, 

# **INTEGRATED FULL-STACK AND AGENTIC AI** 

# **APPLICATION DEVELOPMENT** 

**ASSIGNMENT 1  •  GROUP ASSIGNMENT SPECIFICATION** 

|**Programme**|BSc (Hons) in Information Technology, specializing in SE/AI|
|---|---|
|**Academic Level**|Year 3, Semester 1|
|**Assignment Type**|Group Full-Stack and Agentic AI Application Development Project|
||**25% of the final module mark**(marked out of 100 and scaled). Together with|
|**Weighting**|the Mini Hackathon (15%), this forms the 40% Assignments component of<br>SE3090.|
|**Group Size**|Normally 4 students; variations require written approval from the lecturer-in-<br>charge.|
|**Duration**|9 weeks (31 July – 30 September 2026)|
|**Mandatory Technologies**|ASP.NET Core Web API, PostgreSQL, React, Flutter and Agentic AI|
||**Level 4 — Full AI.**Development AI use is allowed with disclosure. No external|
|**AI Use Level**|AI tools may be used during the final demonstration or viva; the submitted<br>application’s Agentic AI subsystem must be run. See Section 18.|
|**Evaluation**|One final evaluation: demonstration + viva, 100 marks (30 group / 70<br>individual)|
|**Release Date**|**Friday, 31 July 2026**— published through the official Learning Management<br>System (Course Web)|
|**Due Date**|**Wednesday, 30 September 2026 at 11:50 PM —**one submission by the<br>nominated group leader through Course Web|



**Important:** Students must read the complete specification before selecting a domain or beginning implementation. Working software, traceable individual contribution, tests, documentation, deployment evidence and viva understanding are all required. 

Assignment 1 Specification 2026 | Page 1 of 17 <u>stcage? FACULTY OF COMPUTING</u> 

**BSc (Hons) in Information Technology – Specializing in Software Engineering** Beuiee Facultya. **BSc (Hons) in Information Technology – Specializing in Artificial Intelligence.** Sit **SE3090 – Software Engineering Frameworks AS S I G N M E N T 1** — **Year 3 | Semester 1 | 2026** El -. es, 

## **1.  Assignment Overview** 

Each group must design, implement, integrate, test and deploy one coherent software system that combines a web application, mobile application, RESTful backend, relational database and a meaningful Agentic AI workflow. The system must solve a credible real-world problem and demonstrate how modern software engineering frameworks work together. 

**Integrated-system rule:** The React and Flutter applications must use the same ASP.NET Core Web API, PostgreSQL database, user identity, permissions and business rules. Disconnected prototypes will not satisfy the assignment. 

### **1.1 Assignment Objectives** 

- Apply ASP.NET Core, PostgreSQL, React and Flutter to a realistic full-stack problem. 

- Design secure REST APIs, relational data models, web interfaces and mobile workflows. 

- Implement a controlled Agentic AI workflow that plans, delegates, uses tools, validates results and requests human approval when required. 

- Use Git, GitHub, automated testing, code review, CI/CD, documentation and deployment as part of professional software development. 

- Justify framework and architecture decisions in writing through an Architecture Decision Record (ADR). 

- Demonstrate individual technical ownership and the ability to explain, modify and debug submitted work. 

### **1.2 Learning Outcome Alignment** 

This assignment assesses all four learning outcomes of SE3090, as stated in the approved module outline: 

|**Learning**<br>**Outcome**|**Official Module Outline Wording**|
|---|---|
|**LO1**|Evaluate different types of software engineering frameworks used in web, mobile, cloud, and AI-<br>assisted software development.|
|**LO2**|Apply suitable frameworks and tools to build web, mobile, and full-stack software applications<br>efficiently and effectively.|
|**LO3**|Use best practices for integrating frameworks, managing collaborative development, applying<br>CI/CD, ensuring code quality, and deploying software solutions.|
|**LO4**|Select appropriate frameworks, tools, and agentic AI-assisted approaches to meet specific<br>project and industry requirements.|



## **2.  Required Technology Stack** 

|**Area**|**Requirement**|
|---|---|
|**Backend**|C# and ASP.NET Core Web API. This is the mandatory public backend.|



Assignment 1 Specification 2026 | Page 2 of 17 <u>stcage? FACULTY OF COMPUTING</u> 

**AS S I G N M E N T 1** <u>El</u> -. 

**Year 3 | Semester 1 | 2026** <u>es,</u> 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence.** 

**SE3090 – Software Engineering Frameworks** 

|**Area**|**Requirement**|
|---|---|
|**Data Access**|Entity Framework Core with the PostgreSQL provider.|
|**Database**|PostgreSQL.|
|**Web Application**|React using functional components, hooks, routing and a justified state-management<br>approach.|
|**Mobile Application**|Flutter and Dart using a justified state-management approach.|
|**Agentic AI**|Any suitable and justified framework, such as LangGraph (the stack used in labs),<br>Microsoft Agent Framework, LlamaIndex agents, Google ADK, or a custom<br>orchestration approach.|
|**Version Control**|Git and GitHub from the beginning of the project, including a GitHub Actions CI<br>workflow (see Section 13).|
|**Testing**|Suitable tools for backend, React, Flutter, integration, performance and Agentic AI<br>evaluation.|



**Mandatory backend rule:** React and Flutter must communicate only with the ASP.NET Core Web API. Where a Python Agentic AI service is used, it must operate as an internal service called by ASP.NET Core and must not be called directly by either client application. 

## **3.  Group Structure and Individual Contribution** 

The standard group size is four students. Each student must take primary ownership of one business component, so a four-student group must implement four primary components. If the lecturer-in-charge approves a different group size, the number of primary components must equal the number of students (for example, five students = five components). The lecturer-in-charge will confirm any proportional changes to the Agentic AI contributions and overall functional scope in writing. 

|**Student**|**Primary**<br>**Component**|**Required Individual Evidence**|
|---|---|---|
|**Student 1**|Component A|Backend, database, React, Flutter, tests, Git evidence, documentation<br>and a distinct Agentic AI contribution.|
|**Student 2**|Component B|Backend, database, React, Flutter, tests, Git evidence, documentation<br>and a distinct Agentic AI contribution.|
|**Student 3**|Component C|Backend, database, React, Flutter, tests, Git evidence, documentation<br>and a distinct Agentic AI contribution.|
|**Student 4**|Component D|Backend, database, React, Flutter, tests, Git evidence, documentation<br>and a distinct Agentic AI contribution.|



- There must be no project-manager-only, testing-only or documentation-only roles. 

Assignment 1 Specification 2026 | Page 3 of 17 <u>stcage? FACULTY OF COMPUTING</u> 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence.** Sir er eoonae) **SE3090 – Software Engineering Frameworks AS S I G N M E N T 1** inal EE: **Year 3 | Semester 1 | 2026** ee 

- Every student must contribute technically across the required stack and must have an identifiable Agentic AI contribution. 

- Individual marks may be adjusted using Git history, pull requests, issue ownership, test evidence, code ownership, and responses to embedded viva and technical questions. 

- Code or features that a student cannot explain, modify or debug may receive reduced or zero individual marks. 

## **4.  Domain and Functional Scope** 

Each group must select a unique real-world domain. Suggested domains include healthcare appointments, event management, travel planning, inventory, vehicle services, education, property rental, food delivery, recruitment, agriculture, tourism, help-desk systems and community services. 

### **4.1 Minimum Domain Complexity** 

- At least three user roles with different responsibilities and permissions. 

- For the standard four-student group, include at least four major business components with relational data and business-specific operations. An approved group-size variation must follow the one-component-per-student rule in Section 3. 

- CRUD operations plus status workflows, search, filtering, sorting, pagination and reporting or analytics. 

- Meaningful and different purposes for the React and Flutter applications. 

- At least one third-party service integration. 

- At least one complete cross-platform workflow involving React, Flutter, ASP.NET Core, PostgreSQL and Agentic AI. 

## **5.  Part 1 – Secure ASP.NET Core RESTful API Backend** 

The ASP.NET Core backend is the authoritative application layer for public REST APIs, authentication, authorization, validation, business rules, persistence, Agentic AI workflow initiation, approval and audit logging. 

|**Area**|**Minimum Requirement**|
|---|---|
|**Architecture**|Controllers, DTOs, service/application layer, suitable data-access abstraction and<br>dependency injection.|
|**REST API**|Correct routes, HTTP methods, status codes, request/response models and<br>asynchronous operations.|
|**Security**|JWT authentication, role-based authorization, protected endpoints, password hashing<br>and secure configuration.|
|**Data Operations**|CRUD, search, filtering, sorting, pagination, history and business-specific operations.|
|**Quality**|Server-side validation, global error handling, structured logging, CORS and<br>Swagger/OpenAPI.|
|**Agent Integration**|Endpoints for starting workflows, reviewing status, human approval and viewing<br>execution summaries.|



Assignment 1 Specification 2026 | Page 4 of 17 <u>stBe” FACULTY OF COMPUTING</u> 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence.** 

**AS S I G N M E N T 1** EE: 

**Year 3 | Semester 1 | 2026** <u>ee</u> 

**SE3090 – Software Engineering Frameworks** 

**Individual component minimum:** Each student-owned component must include at least four meaningful API endpoints and at least one business-specific operation beyond basic CRUD. 

## **6.  Part 2 – PostgreSQL Database** 

- Design a normalized relational database with an ER diagram and clear relational schema. 

- Use primary keys, foreign keys, appropriate relationships, constraints, indexes and suitable PostgreSQL data types. 

- Use Entity Framework Core migrations and suitable seed data. 

- Apply transactions where required and maintain audit fields such as CreatedAt and UpdatedAt. 

- Persist only the Agentic AI workflow state and execution summaries required by the design; do not store hidden reasoning, passwords, tokens or unnecessary sensitive data. 

## **7.  Part 3 – React Web Application** 

The React application should primarily support administrative, staff, dashboard, reporting, business-data management, Agentic AI monitoring and approval functions. 

- Functional components, React Hooks, React Router and reusable component design. 

- A suitable state-management approach such as Context API, Redux Toolkit, Zustand or another justified option. 

- Complete ASP.NET Core API integration with protected routes and role-based navigation. 

- CRUD interfaces, validation, search, filters, sorting, pagination and dashboard views. 

- Responsive and accessible UI with loading, empty, success and error states. 

- Agent workflow monitoring, execution summaries and approve/reject/revise controls where relevant. 

## **8.  Part 4 – Flutter Mobile Application** 

The Flutter application should primarily support user-facing or operational workflows. It must be a genuine mobile application that consumes the shared ASP.NET Core API. 

- Reusable widgets, navigation/routing and a suitable state-management approach. 

- Registration, login, logout, secure token storage and protected screens. 

- Forms, validation, search, filtering, main business transactions, status tracking and history. 

- Responsive layouts with loading, empty and error states. 

- Agentic task submission, recommendation display and workflow status where suitable. 

- At least one meaningful device feature, such as camera/image picker, GPS/map, QR scanning, file upload, notifications or date/time selection. 

## **9.  Part 5 – Agentic AI Subsystem** 

The Agentic AI feature must solve a meaningful, domain-relevant, multi-step problem. It must not be limited to a generic chatbot, FAQ interface, single-prompt workflow or simple text generator. 

**Minimum acceptance rule:** The group must demonstrate at least one complete assessed workflow that satisfies every step in the “Minimum assessed workflow” row below. 

Assignment 1 Specification 2026 | Page 5 of 17 iit <u>FACULTY OF COMPUTING</u> 

**AS S I G N M E N T 1** EE: 

**Year 3 | Semester 1 | 2026** ee 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence. SE3090 – Software Engineering Frameworks** 

### **9.1 Minimum Agentic AI Requirements** 

|**Requirement**|**Expected Behaviour**|
|---|---|
|**Minimum assessed**<br>**workflow**|At least one assessed workflow must receive a domain objective; create a structured<br>multi-step plan; delegate steps to distinct agent roles; call allow-listed tools using<br>validated inputs and structured outputs; persist workflow state; apply deterministic<br>checks such as schema or business-rule validation; pause a defined high-impact<br>action for approval by an authorized user; and produce either an auditable result or<br>a safe, clearly recorded failure.|
||An agent counts as distinct only when it has an identifiable responsibility, a defined|
|**What counts as a**<br>**distinct agent?**|input and output contract, controlled tool permissions and visible participation in the<br>workflow. Renaming the same prompt or copying identical behaviour does not count<br>as a separate agent.|
||For the standard group, implement at least**four distinct agents**with clearly different|
|**Specialized agents**|responsibilities, such as planning or coordination, domain analysis, action or tool use,<br>and validation or safety. Any approved adjustment must be confirmed in writing by<br>the lecturer-in-charge.|
|**Planning and delegation**|The system analyses a user objective, creates a structured multi-step plan and<br>delegates each step to an appropriate agent.|
|**Controlled tools**|Agents may use only allow-listed tools. Validate every tool input, return structured<br>outputs, handle errors and apply least-privilege access.|
|**Shared state**|Persist the workflow ID, objective, plan, completed steps, tool results, validation<br>results, errors, approval status and final outcome in structured, durable storage.|
|**Validation**|Apply deterministic validation, such as schema checks and business rules, before<br>accepting outputs or allowing high-impact actions. Unsupported or unsafe actions<br>must be rejected or returned for revision.|
|**Human approval**|At least one clearly defined high-impact action must pause until an authorized user<br>approves, rejects or requests revision.|
|**Observability**|Store or display auditable execution summaries, tool calls, timings, validation results,<br>errors, retries, approval decisions and the final result or safe-failure outcome.|
|**Security**|Apply role-based access, prompt and tool-input validation, output validation, secret<br>protection, timeouts, retry limits and safe failure behaviour.|



**Implementation flexibility:** Students may use any suitable Agentic AI framework, model and orchestration method. The selected approach must meet the minimum acceptance scenario, be justified in the ADR, run reliably during evaluation, be secured and be integrated through ASP.NET Core. 

Assignment 1 Specification 2026 | Page 6 of 17 Supe Mi <u>racucry OF COMPUTING</u> 

**AS S I G N M E N T 1** 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence. SE3090 – Software Engineering Frameworks** 

**Year 3 | Semester 1 | 2026** ——ee 

## **10.  Required Integrated Architecture** 



<!-- Start of picture text -->
SE3090 Reference Integration Architecture<br>PostgreSQL<br>React Web Application Entity Framework Core<br>Administration, statt Migrations and constraints<br>agent monitoring and approval ASP.NET Core Web API Agent execution summaries<br>HTP REST / JON EF Cor<br>Mandatory public backend<br>Controllers and DTOs<br>Application/service layer<br>Audit logging and approval<br>Agent workflow endpoints<br>Controlled Agentic Al<br>‘Action / tool agent<br>Flutter Mobile Application interm Validation / safety agent<br>Userfacing and HTTpS/ REST JON Structured shared state<br>operational workflows, Ollama local model<br>eee Allowlsted tools and traces<br>Controlled aPt<br>Third-Party Service<br>Meaningful AP or service integrated<br>through controlled ASPNET Core calls<br><!-- End of picture text -->

_Figure 1. Reference integration architecture. Groups must adapt and justify the architecture for their selected domain._ 



<!-- Start of picture text -->
Required Cross-Platform Workflow Pattern<br>1. Flutter 2. ASP.NET Core 3. PostgreSQL 4. Agentic Al 5. React 6. Shared Status<br>trancaction validates and applies and audit elds creates a validated fend approves, rejects record; mobile user<br>Evidence must show consistent identities, permissions, business rules, data and status across both applications.<br><!-- End of picture text -->

_Figure 2. Required cross-platform workflow pattern._ 

**End-to-end evidence:** At least one demonstrated workflow must begin in one client, pass through ASP.NET Core, PostgreSQL and Agentic AI, require review or approval in the other client, and return an updated status to the initiating user. 

SLUT] FACULTY Assignment 1 Specification 2026 | Page 7 of 17 OF COMPUTING 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence.** 

**AS S I G N M E N T 1** EE: 

**SE3090 – Software Engineering Frameworks** 

**Year 3 | Semester 1 | 2026** ee 

## **11.  Third-Party Integration** 

Each system must integrate at least one meaningful third-party API or service, such as maps, weather, currency, email/SMS, payment sandbox, calendar, cloud storage, QR service or notifications. 

- Explain the business purpose and user benefit. 

- Route external-service access through the ASP.NET Core backend where appropriate. 

- Protect credentials and environment variables. 

- Handle timeouts, invalid responses, service failures and rate limits. 

- Validate and minimize any personal or sensitive data shared with the service. 

## **12.  Testing Requirements** 

|**Area**|**Required Evidence**|
|---|---|
|**Backend**|Unit, service-layer, validation, authentication/authorization, controller and API<br>integration tests.|
|**Database**|PostgreSQL integration tests, constraints, migrations and transaction behaviour.|
|**React**|Component, form-validation, protected-route, API-integration and error-state tests.|
|**Flutter**|Unit, widget, form-validation, navigation and API-integration tests.|
|**End to End**|At least one complete Flutter/React – ASP.NET Core – PostgreSQL – Agentic AI workflow.|
|**Performance**|Concurrent requests, response time, success/failure rate, database response and Agentic<br>AI latency.|
|**Agent Evaluation**|Evidence that at least one complete minimum acceptance workflow passes a suitable<br>golden case, including correct planning and delegation, agent and tool selection,<br>structured outputs, deterministic validation, business-rule compliance, approval<br>enforcement, prompt-injection resistance, failure recovery and safe failure.|



**Agent evaluation rule:** LLM-as-a-judge may be used as supporting evidence, but it must not be the only evaluation method. Use rule-based assertions, schema validation, golden cases, deterministic validators and human review where appropriate. 

## **13.  Git, CI/CD and Collaborative Development** 

- Create the GitHub repository at the beginning of the project. 

- Use meaningful commits, feature branches, issues, pull requests, reviews and a project board. 

- Configure at least one **GitHub Actions CI workflow** that restores, builds and runs the automated backend tests on every push and pull request to the main branch. Additional pipelines (frontend build, lint, Flutter analyze, deployment) are encouraged. 

- Maintain clear evidence of task allocation, merge management and conflict resolution. 

- Each student must show regular technical contribution across the project lifecycle. 

- Artificial commit activity, final-day bulk uploads or unexplained copied code will not be accepted as evidence of contribution. 

Assignment 1 Specification 2026 | Page 8 of 17 <u>stBe” FACULTY OF COMPUTING</u> 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence.** 

**AS S I G N M E N T 1** EE: 

**SE3090 – Software Engineering Frameworks** 

**Year 3 | Semester 1 | 2026** ee 

## **14.  Deployment and Documentation** 

|**Component**|**Deployment Requirement**|
|---|---|
|**ASP.NET Core API**|Deploy to a suitable cloud platform and provide a working health URL and Swagger URL.|
|**PostgreSQL**|Deploy securely with migrations, restricted credentials and initialization instructions.|
|**React**|Deploy and provide a working live URL configured to use the deployed API.|
|**Flutter**|Submit complete source code and a runnable Android APK or approved equivalent.|
|**Agentic AI**|Deploy or run locally as appropriate; provide complete setup, model/framework<br>requirements and startup order.|



**<mark>Service cost and availability:</mark>** <mark>You must be able to complete this assignment using institution-provided or no-cost services. Paid subscriptions are not required. Keep clear local setup instructions. If a required external service has a confirmed outage near submission or evaluation, inform that to evaluator and provide evidence of the outage. When you doing the evolution.</mark> 

### **14.1 README and Technical Documentation** 

- Project overview, business problem, user roles, features and technology justification. 

- System architecture, Agentic AI architecture, database design and repository structure. 

- Installation, environment variables, database setup and startup instructions for all components. 

- API documentation, test instructions, deployment instructions, live URLs and test accounts. 

- Individual contributions, challenges, security considerations and AI usage declaration. 

### **14.2 Architecture Decision Record (ADR)** 

As introduced in Lecture 01, each group must submit an **Architecture Decision Record** — a short document (one page per decision) capturing the context, options considered, decision and consequences for the group’s key technical choices. The ADR is the primary written evidence for LO4 and will be referenced during the viva. 

At minimum, record decisions for: the state-management approach in React and in Flutter, the Agentic AI framework and orchestration method, the database schema strategy for agent workflow state, and the cloud deployment platform. Three to six decisions is a typical, healthy range. 

Assignment 1 Specification 2026 | Page 9 of 17 Supe Mi <u>racucry OF COMPUTING</u> 

**AS S I G N M E N T 1** EE: 

**Year 3 | Semester 1 | 2026** ee 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence. SE3090 – Software Engineering Frameworks** 

## **15.  Submission Guidelines** 

|**Submission Item**|**What You Must Submit**|
|---|---|
|**Group leader and**<br>**deadline**|Group leader must make the only group submission through Course Web by 11:50<br>PM on Wednesday, 30 September 2026.|
|**One consolidated**<br>**report (PDF)**|Combine all written work into one clearly organized PDF. Do not upload the group<br>and individual written reports as separate files.|
|**Group Report section**|Include the project overview and scope; requirements and user roles; full-stack and<br>Agentic AI architecture; database and ER diagram; API, React and Flutter design;<br>technical report; software testing report; Agentic AI evaluation report; performance<br>report; deployment report; ADRs; security considerations; diagrams; references; and<br>the consolidated group AI usage declaration.|
|**Individual Report**<br>**sections**|Include one clearly labelled section for each student containing the contribution<br>statement; owned component and technical work; key commit, pull-request and test<br>evidence; challenges and learning; individual AI usage log; approximately one-page<br>AI reflection; and signed declaration.|
|**Repository and**<br>**deployed system**|Include the repository URL, React URL, ASP.NET Core API or health URL, Swagger URL,<br>PostgreSQL deployment evidence, Agentic AI setup or access information, required<br>environment-variable names and startup instructions.|
|**Flutter APK**|Submit a runnable Android APK, or another format approved in writing, together<br>with installation instructions.|
|**Demonstration video**|Provide a working demonstration-video (10 min) link. Sharing must be set so that<br>anyone with the link can view it without requesting access.|
|**Required access period**|Keep the repository, demonstration video and all deployed services accessible to<br>evaluators until at least Wednesday, 21 October 2026 (three weeks after the<br>submission deadline).|



**Before submitting:** The group leader must open every submitted link in a private or incognito browser and confirm that evaluators can access it. Only one submission is required per group. 

_Within the single consolidated report, the following section lengths are suggested for guidance only: technical report 10– 15 pages; testing report 6–10 pages; Agentic AI evaluation report 5–8 pages; performance report 3–5 pages; deployment report 3–5 pages; and ADRs 3–6 pages. These are not graded limits; the quality and relevance of the evidence matter more than page count._ 

**Naming convention:** Use **SE3090_GroupNumber** for all submitted items. (e.g. `SE3090_G07` ) 

Assignment 1 Specification 2026 | Page 10 of 17 <u>stBe” FACULTY OF COMPUTING</u> 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence. SE3090 – Software Engineering Frameworks** 

**AS S I G N M E N T 1** EE: 

**Year 3 | Semester 1 | 2026** ee 

## **16.  Final Evaluation – Complete Integrated System** 

**Total: 100 marks** |   Group contribution: 30 marks   |   Individual contribution: 70 marks. The mark out of 100 is scaled to **25% of the final module mark** . 

The assignment will be assessed through one final evaluation only. The group must deliver a 10-minute demonstration of the complete integrated system, followed by a 20-minute viva and technical question session. Every student must be present and may be asked to explain, modify, test or debug their individual contribution. 

|**Contribution**|**Criterion**|**Marks**|
|---|---|---|
|**Group**|Component Design and Business Logic|**10**|
|**Group**|Integrated Architecture, Agent Orchestration and State Management|**10**|
|**Group**|Documentation and Deployment|**10**|
|**Individual**|ASP.NET Core RESTful API Development|**10**|
|**Individual**|PostgreSQL Integration and Data Modelling|**10**|
|**Individual**|React Web Application|**10**|
|**Individual**|Flutter Mobile Application|**10**|
|**Individual**|Individual Agentic AI Contribution|**12**|
|**Individual**|API Integration, Security and Cross-Platform Functionality|**10**|
|**Individual**|Testing, CI and Git Workflow|**8**|



**AI use during the evaluation:** During the final demonstration and viva, students may not use external AI assistants, chatbots, IDE copilots or agentic coding tools to answer questions, generate explanations or modify the submitted work. The Agentic AI subsystem implemented as part of the submitted application must be executed during the demonstration. 

Assignment 1 Specification 2026 | Page 11 of 17 Supe Mi <u>racucry OF COMPUTING</u> 

SLIIT  •  Faculty of Computing  •  Department of Software Engineering 

### **16.1 Marking Rubric – Final Evaluation** 

#### **Complete Integrated Full-Stack and Agentic AI System | Total: 100 Marks** 

### **Group Contribution (30 Marks)** 

|**Criterion**|**Excellent**|**Good**|**Satisfactory**|**Poor**|**Very Poor**|
|---|---|---|---|---|---|
|**Component Design and**<br>**Business Logic (10)**|All major components are clearly<br>defined and fully functional. Business<br>rules are correctly implemented<br>through suitable services and support<br>a complete end-to-end workflow. —<br>10 marks|Main components and business<br>rules work with only minor<br>functional<br>or<br>architectural<br>issues. — 8 marks|Core components and main<br>business rules function, and a<br>basic end-to-end workflow is<br>demonstrated; some secondary<br>rules, edge cases or integration<br>remain incomplete. — 6 marks|Some components or business<br>rules are implemented, but<br>workflows are fragmented,<br>unreliable<br>or<br>substantially<br>incomplete. — 4 marks|Components<br>are<br>poorly<br>structured, mostly incomplete<br>or fail to implement the stated<br>business requirements. — 2<br>marks|
|**Integrated Architecture,**<br>**Agent Orchestration and**<br>**State Management (10)**|Complete full-stack integration and<br>one complete minimum Agentic AI<br>acceptance<br>workflow<br>are<br>demonstrated.<br>Distinct<br>agents,<br>persisted state, allow-listed tools,<br>deterministic validation, auditable<br>logs, safe failure and authorized<br>human approval all work correctly. —<br>10 marks|The integrated workflow meets<br>the<br>minimum<br>acceptance<br>scenario, with only minor issues<br>in orchestration, state, logging,<br>tool<br>controls,<br>validation,<br>approval or recovery. — 8<br>marks|The core integrated workflow<br>works and most acceptance<br>elements are present, but one or<br>more elements are only partly<br>effective or supported by limited<br>evidence. — 6 marks|Only a partial integrated<br>workflow<br>works;<br>several<br>mandatory<br>acceptance<br>elements<br>are<br>missing,<br>unreliable<br>or<br>weakly<br>integrated. — 4 marks|No<br>complete<br>assessed<br>workflow is demonstrated;<br>agents are not distinct, state,<br>tools, validation or approval<br>are absent, or the feature is<br>only a chatbot or disconnected<br>prototype. — 2 marks|
|**Documentation and**<br>**Deployment (10)**|The consolidated report is complete,<br>well organized and contains all Group<br>Report<br>and<br>Individual<br>Report<br>sections, ADRs, AI usage documents,<br>evidence and working links. Required<br>systems are deployed, the APK works,<br>evaluator access is clear and setup is<br>fully reproducible. — 10 marks|The consolidated report, access<br>information and deployment<br>are mostly complete, with only<br>minor missing evidence, link or<br>setup issues. — 8 marks|The main Group Report and<br>Individual Report sections and<br>core deployment instructions are<br>provided, but several evidence<br>items, links, AI documents or<br>setup details are incomplete. — 6<br>marks|The consolidated report is<br>limited or poorly organized,<br>and deployment or evaluator<br>access is only partly working<br>or difficult to reproduce. — 4<br>marks|The<br>consolidated<br>report,<br>required group or individual<br>sections, or access details are<br>missing,<br>and<br>major<br>components<br>cannot<br>be<br>deployed or executed. — 2<br>marks|



### **Individual Contribution (70 Marks)** 

|**Criterion**|**Excellent**|**Good**|**Satisfactory**|**Poor**|**Very Poor**|
|---|---|---|---|---|---|
|**ASP.NET Core RESTful API**<br>**Development (10)**|API component is complete and<br>follows REST conventions with DTOs,<br>validation, security, async operations,<br>suitable<br>architecture,<br>exception<br>handling and correct status codes.<br>The student accurately answers|Main API functionality works<br>with minor REST, validation,<br>security or architecture issues.<br>The student answers most<br>related questions and can|Core CRUD and API operations<br>work, but notable gaps remain.<br>The student answers routine<br>questions or makes a simple<br>change but has technical gaps. —<br>6 marks|Only limited API functionality<br>works;<br>major<br>areas<br>are<br>incomplete or unreliable. The<br>student struggles to answer<br>questions or modify and<br>debug the work. — 4 marks|The API contribution is missing<br>or non-functional, or the<br>student cannot explain or<br>modify it. — 2 marks|



SE3090 – Software Engineering Frameworks  |   Assignment 1 Specification 2026  |  Page 12 of 17 

SLIIT  •  Faculty of Computing  •  Department of Software Engineering 

|**Criterion**|**Excellent**|**Good**|**Satisfactory**|**Poor**|**Very Poor**|
|---|---|---|---|---|---|
||related viva questions and can<br>explain, test, modify or debug the<br>contribution. — 10 marks|explain a suitable change. — 8<br>marks||||
|**PostgreSQL Integration and**<br>**Data Modelling (10)**|The<br>contribution<br>demonstrates<br>suitable<br>entities,<br>relationships,<br>constraints,<br>normalization,<br>migrations,<br>indexing<br>and<br>data<br>integrity. The student accurately<br>answers related viva questions and<br>can explain, trace, modify or debug<br>the database contribution. — 10<br>marks|The database is functional and<br>suitably modelled, with minor<br>design or integration issues.<br>The student answers most<br>questions and can explain or<br>make<br>a<br>suitable<br>database<br>change. — 8 marks|Basic<br>database<br>integration<br>works, but notable gaps remain.<br>The student answers routine<br>questions<br>but<br>has<br>difficulty<br>explaining some relationships,<br>constraints or migrations. — 6<br>marks|Database<br>integration<br>is<br>limited,<br>incomplete<br>or<br>inconsistent.<br>The<br>student<br>provides weak answers and<br>struggles to modify or debug<br>it. — 4 marks|The database contribution is<br>missing or non-functional, or<br>the student cannot explain the<br>schema and integration. — 2<br>marks|
|**React Web Application (10)**|Uses reusable components, routing,<br>suitable<br>state<br>management,<br>protected<br>routes,<br>validation,<br>responsive UI, loading and error<br>states, and complete API integration.<br>The student accurately answers<br>related viva questions and can<br>explain, modify or debug the React<br>contribution. — 10 marks|Main React functionality works<br>with minor issues in structure,<br>state, UI, validation or error<br>handling. The student answers<br>most<br>questions<br>and<br>can<br>complete and explain a suitable<br>change. — 8 marks|Core React screens and API<br>operations work, but notable<br>gaps<br>remain.<br>The<br>student<br>answers routine questions or<br>completes a simple change with<br>some difficulty. — 6 marks|Limited React functionality is<br>demonstrated. The student<br>struggles to answer questions<br>or modify and debug the<br>application. — 4 marks|The React contribution is<br>missing or non-functional, or<br>the student cannot explain or<br>modify it. — 2 marks|
|**Flutter Mobile Application**<br>**(10)**|Contains reusable widgets, routing,<br>state<br>management,<br>secure<br>API<br>integration, validation, responsive<br>screens, loading and error states, and<br>a meaningful device feature. The<br>student accurately answers related<br>viva questions and can explain,<br>modify<br>or<br>debug<br>the<br>Flutter<br>contribution. — 10 marks|Main<br>Flutter<br>functionality<br>works with minor issues in<br>architecture, state, UI or API<br>handling. The student answers<br>most<br>questions<br>and<br>can<br>complete and explain a suitable<br>change. — 8 marks|Core Flutter screens and API<br>communication<br>work,<br>but<br>notable<br>gaps<br>remain.<br>The<br>student<br>answers<br>routine<br>questions or completes a simple<br>change with some difficulty. — 6<br>marks|Limited Flutter functionality is<br>demonstrated. The student<br>struggles to answer questions<br>or modify and debug the<br>application. — 4 marks|The Flutter contribution is<br>missing or non-functional, or<br>the student cannot explain or<br>modify it. — 2 marks|
|**Individual Agentic AI**<br>**Contribution (12)**|A distinct, domain-relevant Agentic AI<br>contribution<br>is<br>functional<br>and<br>integrated,<br>with<br>an<br>identifiable<br>responsibility, defined input and<br>output contract, controlled tool<br>permissions,<br>validation,<br>error<br>handling, security, documentation<br>and tests. The student accurately<br>explains the agent, tools, state,|The<br>distinct<br>Agentic<br>AI<br>contribution is functional and<br>relevant, with only minor issues<br>in its contract, permissions,<br>validation,<br>security,<br>testing,<br>observability or integration.<br>The student answers most<br>questions and can explain a<br>suitable change. — 10 marks|A basic but identifiable Agentic AI<br>contribution participates in the<br>workflow,<br>but<br>its<br>contract,<br>controls, tests or integration are<br>incomplete. The student answers<br>routine<br>questions<br>but<br>demonstrates notable gaps. — 7<br>marks|A limited Agentic AI prototype<br>is shown, but its responsibility<br>or participation is unclear. The<br>student struggles to explain<br>the agent's behaviour, tools,<br>state or controls. — 5 marks|The contribution is missing,<br>non-functional, disconnected<br>or duplicates another agent,<br>and<br>the<br>student<br>cannot<br>explain or modify it. — 2 marks|



SE3090 – Software Engineering Frameworks  |   Assignment 1 Specification 2026  |  Page 13 of 17 

SLIIT  •  Faculty of Computing  •  Department of Software Engineering 

|**Criterion**|**Excellent**|**Good**|**Satisfactory**|**Poor**|**Very Poor**|
|---|---|---|---|---|---|
||validation and approval flow and can<br>modify or debug the contribution. —<br>12 marks|||||
|**API Integration, Security**<br>**and Cross-Platform**<br>**Functionality (10)**|React and Flutter use the same API.<br>Authentication, authorization, token<br>handling, validation, shared data,<br>Agentic AI approvals and security<br>controls work correctly. The student<br>accurately answers<br>related<br>viva<br>questions and can trace, modify or<br>debug the complete workflow. — 10<br>marks|Most integration and security<br>functions work with minor<br>inconsistencies or incomplete<br>edge<br>cases.<br>The<br>student<br>answers most questions and<br>can explain the main cross-<br>platform workflow. — 8 marks|The shared API and core cross-<br>platform workflow function, but<br>notable<br>gaps<br>remain.<br>The<br>student<br>answers<br>routine<br>questions<br>but<br>has<br>difficulty<br>explaining some security or<br>integration decisions. — 6 marks|Only limited integration is<br>demonstrated. The student<br>provides weak answers and<br>struggles to trace or debug the<br>workflow. — 4 marks|Integration is absent or non-<br>functional, or the student<br>cannot explain the shared<br>workflow<br>and<br>security<br>controls. — 2 marks|
|**Testing, CI and Git**<br>**Workflow (8)**|Comprehensive<br>tests<br>cover<br>the<br>required layers and Agentic AI. CI<br>passes, and Git history shows regular<br>reviewed contributions with clear<br>ownership. The student accurately<br>answers<br>related<br>viva<br>questions,<br>explains the tests, CI workflow and<br>Git evidence, and can diagnose a<br>relevant failure. — 8 marks|Suitable testing, CI and Git<br>practices<br>are demonstrated<br>with<br>only<br>minor<br>missing<br>evidence. The student answers<br>most<br>related<br>questions<br>correctly. — 6 marks|Some relevant tests and basic Git<br>and CI evidence are provided.<br>The student answers routine<br>questions<br>but<br>demonstrates<br>limited<br>understanding<br>of<br>coverage or workflow decisions.<br>— 4 marks|Few meaningful tests are<br>provided, CI is absent or<br>unreliable, and Git evidence is<br>weak. The student struggles<br>to answer related questions.<br>— 2 marks|Almost no meaningful testing,<br>CI or Git evidence is provided,<br>and<br>the<br>student<br>cannot<br>explain the available evidence.<br>— 1 mark|



#### **Final Evaluation Total = 30 Group + 70 Individual = 100 Marks   →   scaled to 25% of the module mark.** 

_Rubric application and viva note: The five listed performance levels represent performance-band anchors, and evaluators may award intermediate marks. To receive full marks for an individual criterion, the student must demonstrate the required work, correctly answer the related viva questions and, where requested, explain, test, modify or debug the work. If the student cannot demonstrate understanding or ownership, the criterion mark will be reduced; where no relevant evidence is provided, zero marks may be awarded._ 

SE3090 – Software Engineering Frameworks  |   Assignment 1 Specification 2026  |  Page 14 of 17 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence. SE3090 – Software Engineering Frameworks Year 3 | Semester 1 | 2026** 

**AS S I G N M E N T 1** FN 

## **17.  Demonstration and Viva Requirements** 

### **17.1 Demonstration Checklist** 

- ☐ Login using different roles and demonstrate protected operations. 

- ☐ Demonstrate CRUD and a business-specific workflow and show PostgreSQL data changes and Swagger documentation. 

- ☐ Demonstrate React and Flutter using the same ASP.NET Core API. 

- ☐ Run the application’s Agentic AI subsystem and demonstrate the complete minimum acceptance workflow: domain objective, structured plan, distinct agent roles, allow-listed tool use, persisted state, deterministic validation, authorized approval, and an auditable result or safe failure. 

- ☐ Demonstrate human approval and execution-history summaries. 

- ☐ Show error handling, tests, the passing CI workflow, deployed applications and GitHub contribution history. 

### **17.2 Viva Scope** 

- Explain a controller, service, DTO, database relationship, constraint, migration or index. 

- Explain authentication, authorization, state management, secure storage and API integration. 

- Explain an agent role, tool, orchestration decision, shared state, validation, security control and human approval. 

- Explain a test, Git contribution, CI workflow step, deployment decision, third-party integration or a decision recorded in the ADR. 

- Modify a small feature, validation rule or business rule, or debug a failed workflow. 

## **18.  Usage of AI** 

This assessment is designed using the CLEAR Framework and the AI Assessment Scale (Perkins, Furze, Roe & MacVaugh, 2024). The permitted level of AI use, the tasks it applies to, the disclosure required and the marks awarded for process are set out below. 

**AI Use Level — Level 4 (Full AI):** AI tools may be used extensively during the permitted development tasks in Section 18.1 when all use is disclosed, verified and understood. During the final demonstration and viva, no external AI assistant, chatbot, IDE copilot or agentic coding tool may be used to answer questions, generate explanations or modify the submitted work (Level 1 — No AI). 

### **18.1 Where AI Tools May Be Used** 

AI tools — chat assistants, IDE copilots and agentic coding tools — may be used for the tasks below. In every case the group remains fully responsible for the correctness, security, originality and understanding of what is submitted. 

|**Task / Section**|**Permitted use of AI tools**|
|---|---|
|**Domain**<br>**&**<br>**requirements**|Brainstorm domains, roles, features and user stories; research the domain.|
|(Section 4)|The final scope must be your own and defensible at the viva.|
|**Architecture & ADR**(Sections|Explore options, compare frameworks, draft the ADR. The decision taken and|
|10, 14.2)|its justification must be the group’s own reasoning.|



Assignment 1 Specification 2026 | Page 15 of 17 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence. SE3090 – Software Engineering Frameworks** 

**AS S I G N M E N T 1** ypN 

**Year 3 | Semester 1 | 2026** 

|**Task / Section**|**Permitted use of AI tools**|
|---|---|
|**Backend & database**(Sections 5,<br>6)|Generate, refactor and debug ASP.NET Core code; design schema,<br>migrations, seed data, indexes and the ER diagram. All owner-reviewed and<br>tested.|
|**React & Flutter**(Sections 7, 8)|Scaffold components and widgets, state management, routing, validation,<br>styling, secure storage and device features.|
|**Agentic AI subsystem**(Section 9)|Agent design, prompt engineering, tool definitions, orchestration, validation<br>and safety controls. Prompt engineering and human-in-the-loop validation<br>are examinable at the viva.|
|**Testing, CI/CD & deployment**<br>(Sections 12–14)|Generate and run tests and test data; write CI workflows; configure and<br>troubleshoot deployment. Tests must be executed and understood, not only<br>generated.|
|**Reports, README & diagrams**|Draft, structure and proofread. All facts, figures, screenshots, test results and|
|(Sections 14.1, 15)|evaluation findings must be your own.|



### **18.2 Where AI Tools May Not Be Used** 

- **Final demonstration and viva.** This evaluation is conducted under Level 1 (No AI). You may not use external AI assistants or agentic coding tools to answer questions, generate explanations or modify the submitted work. 

- **Work you cannot explain.** Any code, test, diagram or documentation you cannot explain, test or modify may receive reduced or zero individual marks (Section 3). 

- **Fabricated evidence.** Back-filled commit history, invented AI-log entries, or test/evaluation results that were not actually produced. 

- **Confidential data and others’ work.** Never share credentials, API keys, private or institutional data with an AI tool, or commit them to GitHub. Presenting another party’s work as your own remains plagiarism, and the individual reflection must be your own writing. 

### **18.3 Disclosure, Process Marks and Reflection** 

**Disclosure.** Each student must keep an individual AI usage log showing the date, tool and model, task or section, what the tool produced, what was changed or rejected, and how the result was verified. Include this log in that student’s Individual Report section of the consolidated report. Include one consolidated group AI usage declaration in the Group Report section, confirming that all AI use has been disclosed and that every member can explain, test and modify the work submitted under their name. 

**Process marks.** As AI use is permitted at Level 4, 30 marks assess the development process: 15 marks for technical understanding and ownership assessed through the viva and embedded across the individual criteria, 5 core marks for Testing, CI and Git Workflow, and 10 marks for Documentation and Deployment. These marks are assessed through the rubric in Section 16.1. **This meets the CLEAR minimum of thirty percent process marks for a Level 4 assessment.** 

**Reflection (marked).** Each student must include an approximately one-page individual reflection in their Individual Report section of the consolidated report. It is marked under Documentation and Deployment and may be discussed during the viva. Address the following: 

Assignment 1 Specification 2026 | Page 16 of 17 

**BSc (Hons) in Information Technology – Specializing in Software Engineering BSc (Hons) in Information Technology – Specializing in Artificial Intelligence. SE3090 – Software Engineering Frameworks Year 3 | Semester 1 | 2026** 

**AS S I G N M E N T 1** a 

- Which AI tools, if any, were used, and at which stages? 

- What did the AI tools do well, and what did they get wrong? 

- What did you change, add or reject from the AI output, and why? 

- What did you learn about your own skills and understanding? 

_The AI rules above were agreed with the SE3090 cohort and are published on Course Web (SE3090 → Assignments → Assignment 1). A reflection that is AI-generated, or that does not match the student’s Git history and AI usage log, will not receive credit._ 

## **19.  Academic Integrity** 

AI use that complies with Section 18 is permitted and expected. The following requirements apply to all submitted work, whether or not AI tools were used. 

- Do not submit code or features that you cannot explain, test or modify. 

- Acknowledge external libraries, APIs, tutorials, sample code and AI assistance. 

- Maintain the AI usage log and submit the AI usage declaration. 

- Do not expose credentials, private data or protected institutional information. 

- Copying another group’s code, agent design or reports, or submitting commissioned work, is a serious academic offence. 

- Plagiarism in any form is not permitted, and standard SLIIT academic-integrity and plagiarism procedures apply to all submitted work. 

## **20.  Final Student Checklist** 

|☐Required number of primary business components<br>completed (one per student)|☐ASP.NET Core API and PostgreSQL working|
|---|---|
|☐JWT authentication and role-based authorization<br>completed|☐React and Flutter applications working through the<br>shared API|
|☐At least four specialized agents with controlled tools<br>and structured state|☐Validation, observability and human approval<br>implemented|
|☐Meaningful third-party integration completed|☐Traditional testing, Agentic AI evaluation and<br>performance testing completed|
|☐GitHub Actions CI workflow building and running<br>tests.|☐ADR completed with justified framework and<br>architecture decisions|
|☐React, ASP.NET Core and PostgreSQL deployed;<br>Flutter APK generated|☐One consolidated report containing the Group<br>Report, all Individual Reports, diagrams and required<br>links completed|
|☐Git contribution visible for every member|☐AI usage declared and no secrets committed to<br>GitHub|
|☐Demonstration and viva prepared with no external<br>AI use|☐Contribution statements, AI logs, group declaration<br>and individual reflections included in the consolidated<br>report|



Assignment 1 Specification 2026 | Page 17 of 17 

