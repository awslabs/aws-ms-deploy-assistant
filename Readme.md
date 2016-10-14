# AWS EC2 Deployment Assistant for Microsoft Developer Platforms
 
Deployment automation from Microsoft Developer platforms (Microsoft Visual Studio and/or Microsoft Team Foundation Server) to AWS EC2 instances is a common customer requirement. AWS Developer Tools can support the continuos delivery/continuous deployment requirements for AWS customers leveraging these development platforms at a very low cost while monitoring and maintaining application availability. This capability comes through the AWS CodePipline and CodeDeploy services that can consume the precomipled output of the Microsoft Build System. To begin this pipeline, build output must be packaged and uploaded to an AWS S3 staging location where it is pickedup by the AWS CodePipeline service and can then flow through a customer defined continous delivery workflow.
 
### CodePipeline [Learn more >>](https://aws.amazon.com/codepipeline/)
AWS CodePipeline is a continuous delivery service for fast and reliable application updates.
 
### CodeDeploy [Learn more >>](https://aws.amazon.com/codedeploy/)
AWS CodeDeploy is a service that automates code deployments to any instance, including Amazon EC2 instances and instances running on-premises.
 
## The Tool 
The AWS EC2 Deployment Assistant for Microsoft Developer Platforms is a utility application that enables integration of the AWS CodePipeline and CodeDeploy Services into the Visual Studio or Team Foundation Server build process. It handles the packaging and upload of successful builds to the AWS S3 service staging location where it is pickedup by the AWS CodePipeline service and can then flow through a customer defined continous delivery workflow.
 
#### Features
 
* Build Output Preparation  
  * Automates the packaging and preparation of build output  
  * Extensible plugin framework enables custom task insertion (more info below) 
* Create Deployment Package  
  * Creates zip format archive of build output  
  * Filtered by include and exclude expression  
* Upload Package to S3 Drop Location  
  * Package pushed to S3 drop location where CodePipeline can pickup and execution workflow  
  * Uses stored credential profile  
* Stored Credential Profile Management  
  * Enables command line management of stored credential profiles   
   
## How It Works
Using Visual Studio Post Build Events, MSBuild Events or a customized Team Foundation Server (TFS) Build Template, the AWS Deployment Assistant runs against a sucessful builds output.
 
![Tool Execution Worflow](https://github.com/awslabs/aws-ms-deploy-assistant/blob/master/DemoSite/Images/process.png "Process Workflow")
 
## Tool Setup
Today, the AWS EC2 Deployment Assistant requires manually installation for use as part of a DevOps process. You do not need to download or build the projects code if you only wish to use the tool. To setup/install the tool please follow the steps below:
 
1. Download the [Latest Release](https://github.com/awslabs/aws-ms-deploy-assistant/releases/latest) distribution package.
2. Create the following folder path "C:\Program Files (x86)\AWS Tools\EC2 Deployment Assistant".
3. Unzip the distribution package directly to "C:\Program Files (x86)\AWS Tools\EC2 Deployment Assistant". Please make sure that distribution files are directly at the root of that path.
4. Create the following folder paths "C:\temp\AWS Development Tools\EC2 Deployment Assistant\logs" and "C:\temp\AWS Development Tools\EC2 Deployment Assistant\temp".
5. Configure your project(s) to invoke the continuous delivery pipeline after a successful build. A successful builds output can be hooked in several ways and you should pick the option that best matches your DevOps needs.
    * Visual Studio Project Post-Build Event Setup  
      * This option triggers a pipeline on a successful build within Visual Studio. In this example, the build must be run with a build configuration called "Deploy". 
      * To configure this option, edit the project properties for the project you wish to deploy. On the Build Events table paste the below snipet into the Post Build field. Be sure to update the bucket, profile and zipname attributes for your use case.
      
      > "if $(ConfigurationName) == Deploy (  
      >    "C:\Program Files (x86)\AWS SDK for .NET\bin\Developer Tools\AWSDeploymentAssistant.exe" build --source "$(ProjectDir)." --bucket "useast-my-bucket" --profile “default" --zipname "MyCodePackage.zip"
      > )  
      
      ![Post Build Event](https://github.com/awslabs/aws-ms-deploy-assistant/blob/master/DemoSite/Images/vspostbuild.png "Post Build Event")
    * TFS Build Template Setup  
      * This option triggers a pipeline on a successful build within Team Foundation Server (TFS). Instructions Coming Soon!
    
    * Project File MSBuild Event Setup  
      * This option triggers a pipeline on a successful build within Visual Studio or Team Foundation Server (TFS). In this example, the build must be run with a build configuration called "Deploy".
      * To configure this option, edit the project file for the project you wish to deploy. Find the commented line \<Target Name="AfterBuild"\>\</Target>, generally at the end of a project file, and uncomment. Between the opening and closing tags of this element add the below snippet. Be sure to update the bucket, profile and zipname attributes for your use case.  
 
      > \<Exec Command="&quot;C:\Program Files (x86)\AWS Tools\EC2 Deployment Assistant\AWSDeploymentAssistant.exe&quot; build --source &quot;$(MSBuildProjectDirectory)&quot; --bucket &quot;useast-my-bucket&quot; --profile &quot;default&quot; --zipname &quot;MyCodePackage.zip&quot;" Condition="'$(Configuration)'=='Deploy'" /\>
 
      ![MSBuild Event](https://github.com/awslabs/aws-ms-deploy-assistant/blob/master/DemoSite/Images/msbuild-target.png "MSBuild Event")
6. Configure deployment content filter. When creating a deployment package, the tool filters the file contents that it packages using a configurable “include” and “exclude pattern”.
   * See "PublishFileTypesIncludePattern" for files/paths you wish to include in the AWSDeploymentAssistant.exe.config file.
   * See "PublishFileTypesExcludePattern" for files/paths you wish to exclude in the AWSDeploymentAssistant.exe.config file.  
 
## Configuring CodePipeline and CodeDeploy
To configure CodePipeline and CodeDeploy for this tool follow the following steps:
 
1. Walk through the [Simple Pipeline Walkthrough (Amazon S3 Bucket) >>](https://docs.aws.amazon.com/codepipeline/latest/userguide/getting-started-w.html) documentation Steps one thru four.
 
## Build Output Preparation Plugins
The AWS EC2 Deployment Assistant supports extensibility for the preperation of build output. At runtime, the tool loads plugins from the "C:\Program Files (x86)\AWS Tools\EC2 Deployment Assistant\plugins" directory. Plugins can do any number of things to prepare build output for deployment and run with a priority weighting. Plugins can accept input through options json files that include key value pair string values.  Each plugin includes a name attribute set at implementation. Plugins are simply classes within an assembly placed in the plugin folder that Implement the IDeploymentTask interface. Options files are read from the root path of the build content being processed and file names must follow the format “\<PluginName\>.options.json”. The sample CodeDeploy plugin generates a CodeDeploy AppSpec file.
 
### CodeDeploy Plugin
The CodeDeploy Plugin will automatically generate an AWS CodeDeploy service deployment definition, an AppSpec.yml file as well as PowerShell scripts to execute a simple deployment to an IIS root site. Options may be specified in a plugin options file "AWSCodeDeployPlugin.options.json" to customize deployment behavior.  For more complex deployment requirements, we recommend that you include an AppSpec file and any required deployment scripts in your build output. If an AppSpec is already present, the AWS EC2 Deployment Assistant will skip auto generation. 
 
## Development Environment Setup
If you would like to develop additional functionality, such as a new plugin, you will need to follow the below setps to get up and running:
 
1. Clone the "aws-ms-deploy-assistant" repository [Clone >>](https://github.com/awslabs/aws-ms-deploy-assistant.git).
2. Either manually configure "Tool Setup" as described above, note that you must manually add file system ACLs to allow the copy of build output to the tool runtime path, or run the "Initialize-Environment.ps1" script included in the repository (Local Admin Rights Required). Be sure to replace the account name "\<Domain\\Account\>" in the script with your account details before you run the script. 
3. Build your environment. 
 
### Debugging
To debug the AWS EC2 Deployment Assitant or any of its plugins, set the "AWSDeploymentAssistant" project as the startup project. Set the debug command line arguments for your environment and then run the solution in debug mode. Be sure to include a leading space in your command argument.  
 
> build --source "\<source path for deploy content\>"" --bucket "useast-my-bucket" --profile "default" --zipname "MyCodePackage.zip"  
 
![Command Line Args](https://github.com/awslabs/aws-ms-deploy-assistant/blob/master/DemoSite/Images/command-line-args.png "Command Line Args")
 
### Demo Website
The demo website is preconfigured using the "Project File MSBuild Event" method of invoking a deployment pipeline. Be sure to update the bucket, profile and zipname attributes for your use case.
 
----
 
Copyright 2016 - 2016 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 
Licensed under the Apache License Version 2.0 (the "License"). You may not use this file except in compliance with the License. A copy of the License is located at [https://github.com/awslabs/aws-ms-deploy-assistant/blob/master/LICENSE](https://github.com/awslabs/aws-ms-deploy-assistant/blob/master/LICENSE).