**BSc (Hons) in Information Technology – Software Engineering** sees SLUT **SE3090 : Software Engineering Frameworks** Corer **Year 3 Semester 1 – 2026** 

**A S S I G N M E NT 2** a 

|AssignmentTitle|Software Testing and Quality Evaluation of the SE3090 Integrated System|
|---|---|
|Learning outcomes covered|LO2: Apply suitable frameworks and tools to build web, mobile, and full-stack software<br>applications efficiently and effectively.<br>LO3: Use best practices for integrating frameworks, managing collaborative<br>development, applying CI/CD, ensuring code quality, and deploying software solutions.|
|AssignmentMode|Group assignment with individual viva|
|MaximumMarks|100 Marks|
|Contribution to the Final<br>Grade|15%|
|Date published|19<sup>th</sup>September 2026|
|Deadline for submissions|5<sup>th</sup>October 2026|
|Mode of Submission|Submit through the official Learning Management System (CourseWeb).|



# **Assignment Description** 

This assignment is directly connected to the SE3090 Main Assignment. You must test the same integrated system developed by your group. You are not required to build a separate application for this assessment. 

The purpose is to show that your system has been tested in a planned and professional way. You must select suitable testing areas, use appropriate testing tools or frameworks, execute the tests, record the results, identify defects, and provide clear evidence of your testing work. 

## **1. Relationship to the SE3090 Main Assignment** 

- Use the same ASP.NET Core Web API, PostgreSQL database, React web application, Flutter mobile application and Agentic AI subsystem developed for the SE3090 Main Assignment. 

- Test the actual features and workflows implemented by your group. 

- At least one test must cover a complete integrated workflow across the relevant components of the system. 

- The testing evidence produced here can also support the testing-related documentation required for the SE3090 Main Assignment. 

## **2. Testing Scope** 

Your group must perform suitable testing from the areas below. For each selected technical testing area, you must use an appropriate testing tool or framework. **Manual observation alone is not sufficient.** 

|Testing Area|What to Test|Suggested Tools / Frameworks|
|---|---|---|
|Backend / API Testing|Unit testing; service/business-logic<br>testing; validation testing; controller<br>testing; authentication and<br>authorization testing; API integration<br>testing.|xUnit, NUnit, MSTest, Moq,<br>WebApplicationFactory,<br>Postman/Newman|
|Database Testing|Database integration testing; constraint<br>testing; relationship and data-integrity<br>testing; migration testing; transaction<br>testing.|xUnit + PostgreSQL, Testcontainers for<br>.NET, Entity Framework Core|
|React Web Application Testing|Component testing; form-validation<br>testing; protected-route testing; API-<br>integration testing; UI-state and error-<br>state testing.|Vitest/Jest, React Testing Library,<br>MSW, Playwright|



sees SLUT Cereey 

|Flutter Mobile Application Testing|Unit testing; widget testing; form-<br>validation testing; navigation testing;<br>API-integration testing.|flutter_test, integration_test,<br>mocktail/Mockito|
|---|---|---|
|Integration / End-to-End Testing|API integration testing; cross-<br>component integration testing;<br>complete business-workflow testing;<br>cross-platform workflow testing.|Playwright, Postman/Newman, Flutter<br>integration_test, or another justified<br>E2E tool|
|Non-Functional Testing|Performance testing; load testing;<br>stress testing; security testing; usability<br>testing; accessibility testing;<br>compatibility testing; reliability testing;<br>recovery testing.|k6, Apache JMeter, OWASP ZAP,<br>Lighthouse, axe DevTools, Playwright,<br>and other appropriate<br>monitoring/testing tools|
|Agentic AI Testing & Evaluation|Task-completion testing; agent-<br>selection testing; tool-selection testing;<br>structured-output validation; business-<br>rule compliance testing; prompt-<br>injection testing; approval-enforcement<br>testing; failure-recovery testing; safe-<br>failure testing.|xUnit/pytest, promptfoo, DeepEval,<br>schema validation, deterministic test<br>cases|



**Non-functional testing requirement:** Performance and security testing are required. Select additional nonfunctional testing types where relevant to your system and justify your selection. 

## **3. What You Need to Do** 

**1.** Identify the important features, workflows and quality risks in your SE3090 system. 

**2.** Prepare a test plan showing what will be tested, the testing type, expected result, tool/framework and responsible member. 

**3.** Design meaningful test cases, including normal, invalid, boundary/edge and failure cases where relevant. 

**4.** Use suitable testing tools/frameworks and execute the tests. 

**5.** Record actual results and clearly mark each test as Passed or Failed. 

**6.** Record defects found, correct important defects and perform retesting. 

**7.** Collect evidence such as automated test output, screenshots, logs, coverage, performance results, security scan results or AI evaluation results. 

**8.** Prepare the required testing documents and submit them with the supporting evidence. 

**9.** Prepare to explain and demonstrate your own testing contribution during the viva. 

## **4. Testing Documents to Prepare** 

|Document|Minimum Content|
|---|---|
|Test Plan|Scope, objectives, testing areas, tools/frameworks, test<br>environment,responsibilities and schedule.|
|Test Case Document|Test case ID, feature, preconditions, steps/input,<br>expectedresult, actual result andPass/Failstatus.|
|Defect / Bug Report|Defect ID, description, severity/priority, steps to<br>reproduce, evidence, status andretestresult.|
|Test Execution Summary|Tests executed, passed, failed, defects identified/fixed<br>and a short conclusion.|
|Tool-Generated Evidence|Relevant automated test reports, coverage, performance<br>results, security scans,logs or AIevaluationoutputs.|



## **5. Deliverables** 

- Software Testing Report (PDF) containing the test plan, testing scope, test execution summary, defect summary, and conclusion. 

- Completed Test Case Document with expected result, actual result, and Pass/Fail status. 

- Defect / Bug Report with retesting evidence. 

- Testing tool/framework evidence, including relevant screenshots, generated reports, logs or exported results. 

Eee SHIT Corer 

- Automated test source code/scripts used for the applicable testing areas. 

- GitHub repository link and contribution/commit evidence related to testing. 

- Any additional configuration or files required to reproduce or rerun the tests. 

**Important:** Evidence must come from your own SE3090 system. A submission containing only theoretical descriptions of testing will not receive full marks. 

## **6. Viva** 

A viva will be conducted as part of this assignment. Each student must be able to explain the tests they contributed to, the selected tool/framework, how the test was executed, what the result means, defects found, and how the system was improved. Students may also be asked to run, modify or explain a test during the viva. Individual viva performance may affect the individual mark. 

## **7. Usage of AI** 

AI tools may be used to support learning, brainstorming, test-case ideas, debugging, test-script generation, documentation and code review. Students are responsible for checking, adapting and understanding all AIassisted work. AI-generated test cases or scripts must be verified against the actual system before submission. Students must not submit testing work they cannot explain or reproduce during the viva. AI assistance must be declared according to the module requirements and the CLEAR framework. 

## **8. Marking Scheme** 

Total: 100 marks | Group contribution: 40 marks | Individual contribution: 60 marks | Contribution to the final module grade: 15% 

Important: This assessment is demonstration-based. Submitted documents and screenshots alone are not sufficient. Each student must personally demonstrate and explain at least one meaningful tool/framework-based testing contribution using the group’s SE3090 system. 

## **Detailed Marking Rubric** 

|**Criterion**|**Excellent**|**Good**|**Satisfactory**|**Poor**|**Very Poor**|
|---|---|---|---|---|---|
|GROUP: Testing<br>Strategy & Coverage<br>(10)|Clearly explains a well-<br>planned testing strategy.<br>Testing scope is relevant<br>and covers the important<br>system components,<br>workflows, and risks with<br>suitable tools/frameworks.|Good strategy and<br>coverage of most<br>important areas<br>with only minor<br>gaps.|Basic strategy covers<br>the main areas, but<br>some important<br>components, risks or<br>test types are missing.|Limited testing<br>strategy; coverage is<br>narrow or poorly<br>justified.|No meaningful testing<br>strategy or coverage is<br>demonstrated.|
|GROUP: Integrated<br>& Non-Functional<br>Testing<br>Demonstration (15)|Successfully demonstrates<br>meaningful integration/E2E<br>testing and relevant non-<br>functional testing using<br>suitable tools. Results are<br>clearly interpreted and<br>connected to the actual<br>SE3090 system.|Demonstrates<br>integration and<br>non-functional<br>testing effectively<br>with minor gaps in<br>coverage,<br>execution or<br>explanation.|Basic demonstrations<br>are completed, but<br>coverage, tool use or<br>interpretation is<br>limited.|Demonstration is<br>incomplete, mostly<br>manual, or provides<br>weak evidence of<br>integrated/non-<br>functional testing.|No meaningful<br>integrated or non-<br>functional testing<br>demonstration.|
|GROUP: Overall<br>Test Results,<br>Defects &<br>Documentation (15)|Test results are complete<br>and traceable. Defects are<br>clearly recorded, important<br>fixes are shown with<br>retesting, and required<br>testing documents/tool<br>evidence are complete and<br>consistent.|Good results,<br>defect handling<br>and documentation<br>with minor<br>omissions.|Main documents and<br>results are present,<br>but defect/retest<br>evidence or<br>consistency is basic.|Documents/results are<br>incomplete or poorly<br>supported by actual<br>testing evidence.|Major testing<br>documents, results or<br>defect evidence are<br>missing.|
|INDIVIDUAL: Testing<br>Tool / Framework<br>Demonstration (15)|Personally and confidently<br>demonstrates appropriate<br>testing tool(s)/framework(s),<br>explains why they were<br>selected,<br>configuration/setup, and<br>how they are used on the<br>actual system.|Demonstrates<br>suitable<br>tool/framework use<br>with good<br>understanding and<br>only minor gaps.|Can run and explain<br>basic tool/framework<br>usage but shows<br>limited understanding<br>of configuration or<br>purpose.|Limited demonstration;<br>relies heavily on<br>others or cannot<br>clearly explain how the<br>tool/framework is<br>used.|Cannot demonstrate<br>meaningful personal<br>use of a testing<br>tool/framework.|



SLU 

|INDIVIDUAL: Test<br>Implementation &<br>Execution (15)|Shows meaningful<br>personally implemented<br>tests and executes them<br>successfully. Test design<br>includes appropriate<br>normal, invalid,<br>boundary/edge and/or<br>failure cases and<br>meaningful<br>assertions/checks.|Good<br>implementation<br>and execution with<br>minor gaps in test<br>variety, assertions<br>or coverage.|Basic tests are<br>implemented and run,<br>but scenarios or<br>assertions are limited.|Few/trivial tests, weak<br>implementation, or<br>difficulty executing<br>own tests.|Cannot show or<br>execute meaningful<br>personally<br>implemented tests.|
|---|---|---|---|---|---|
|INDIVIDUAL:<br>Results, Defects &<br>Retesting (10)|Clearly interprets own test<br>results, identifies<br>meaningful defects/issues,<br>explains their cause/fix<br>where applicable, and<br>demonstrates retesting or<br>verification.|Good<br>understanding of<br>results and defect<br>handling with minor<br>gaps.|Can explain basic<br>results and some<br>defect/retest evidence,<br>but analysis is limited.|Results are shown<br>with little interpretation<br>or weak defect/retest<br>evidence.|Cannot explain test<br>results or provide<br>meaningful<br>defect/retesting<br>evidence.|
|INDIVIDUAL:<br>Technical<br>Contribution (5)|Clear individual ownership<br>is visible through test<br>code/scripts, Git history and<br>related fixes/evidence;<br>contribution is consistent<br>and traceable.|Good identifiable<br>contribution with<br>minor gaps in<br>traceability.|Some identifiable<br>contribution exists, but<br>ownership/evidence is<br>limited.|Very limited or unclear<br>individual contribution.|No identifiable<br>individual testing<br>contribution.|
|INDIVIDUAL: Viva &<br>Technical<br>Understanding (15)|Demonstrates strong<br>understanding of own<br>testing approach,<br>code/scripts, tools, results<br>and related system<br>behaviour; confidently<br>answers questions and can<br>run, explain, modify or<br>troubleshoot a test when<br>requested.|Good<br>understanding and<br>demonstration with<br>minor difficulty on<br>advanced<br>questions or<br>modifications.|Basic understanding,<br>but has difficulty<br>explaining some<br>technical decisions,<br>results or test<br>changes.|Weak understanding<br>and significant<br>difficulty explaining or<br>modifying submitted<br>testing work.|Cannot explain,<br>reproduce, modify or<br>demonstrate submitted<br>testing work.|



