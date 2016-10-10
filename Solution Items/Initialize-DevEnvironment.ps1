function Initialize-DevEnvironment{
	param(
		[Parameter(Mandatory=$true)][string]$username
	)
	New-Item -ItemType Directory -Force -Path "C:\Program Files (x86)\AWS Tools\EC2 Deployment Assistant";
	New-Item -ItemType Directory -Force -Path "C:\temp\AWS Development Tools\EC2 Deployment Assistant\logs";
	New-Item -ItemType Directory -Force -Path "C:\temp\AWS Development Tools\EC2 Deployment Assistant\temp";

	$acl = Get-Acl "C:\Program Files (x86)\AWS Tools\EC2 Deployment Assistant";
	$rule = New-Object  system.security.accesscontrol.filesystemaccessrule($username,"FullControl","Allow");
	$acl.SetAccessRule($rule);
	Set-Acl "C:\Program Files (x86)\AWS Tools\EC2 Deployment Assistant" $acl;
}

Initialize-DevEnvironment -username "<Domain\Account>"