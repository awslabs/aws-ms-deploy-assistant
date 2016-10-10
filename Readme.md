# AWS EC2 Deployment Assistant for Microsoft Developer Platforms

Deployment automation from Microsoft Developer platforms (Microsoft Visual Studio and/or Microsoft Team Foundation Server) to AWS EC2 instances is a common customer requirement. AWS Developer Tools can support the continuos delivery/continuous deployment requirements for AWS customers leveraging these development platforms at a very low cost while monitoring an maintaining application availability. This capability comes through the AWS CodePipline and CodeDeploy services which can consume the precomipled output of the Microsoft Build System. To begin this pipeline, build output must be packaged and uploaded to an AWS S3 staging location where it is pickedup by the AWS CodePipeline service and can then flow through a customer defined continous delivery workflow.

### CodePipeline [Learn more >>](https://aws.amazon.com/codepipeline/)
AWS CodePipeline is a continuous delivery service for fast and reliable application updates and was used to deploy this website.

### CodeDeploy [Learn more >>](https://aws.amazon.com/codedeploy/)
AWS CodeDeploy is a service that automates code deployments to any instance, including Amazon EC2 instances and instances running on-premises.

## The Tool 
The AWS EC2 Deployment Assistant for Microsoft Developer Platforms is a utility application that enables integration of the AWS CodePipeline and CodeDeploy Services into the Visual Studio or Team Foundation Server build process. It handles the packaging and upload of successful builds to the AWS S3 service staging location where it is pickedup by the AWS CodePipeline service and can then flow through a customer defined continous delivery workflow.

## How It Works
Using Visual Studio Post Build Events, MSBuild Events or a customized Team Foundation Server (TFS) Build Template, the AWS Deployment Assistant is triggered and runs against a sucessful build's output.

* Build Output Preparation  
  * Automates the packaging and preparation of build output  
  * Extensible plugin framework enables custom task insertion  
* Create Deployment Package  
  * Creates zip format archive of build output  
  * Filtered by include and exclude expression  
* Upload Package to S3 Drop Location  
  * Package pushed to S3 drop location where CodePipeline can pickup and execution workflow  
  * Uses stored credential profile  
* Stored Credential Profile Management  
  * Enables command line management of stored credential profiles  

![Process Worflow](https://github.com/awslabs/aws-ms-deploy-assistant/blob/master/DemoSite/Images/process.png "Process Workflow")

The AWS EC2 Deployment Assistant will automatically generate an AWS CodeDeploy service deployment definitions, an AppSpec.yml file as well as PowerShell scripts to execute a simple deployment to an IIS root site. For more complex deployment requirements, we recommend that you include an AppSpec file and any required deployment scripts in your build output. If an AppSpec is already present, the AWS EC2 Deployment Assistant will skip auto generation. 

----

Copyright 2016 - 2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.

Licensed under the Apache License Version 2.0 (the "License"). You may not use this file except in compliance with the License. A copy of the License is located at [https://github.com/awslabs/aws-ms-deploy-assistant/blob/master/LICENSE](https://github.com/awslabs/aws-ms-deploy-assistant/blob/master/LICENSE).
