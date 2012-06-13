param($installPath, $toolsPath, $package, $project)

$path = [System.IO.Path]
$packages = $path::Combine($path::GetDirectoryName($project.FileName), "packages.config")

$installedPackages = ""

function IsPackageInstalled($packageName){
	if (!$installedPackages){
		[xml]$installedPackages = Get-Content $packages
	}

	foreach( $package in $installedPackages.packages.package) 
	{
		if ($package.id -eq $packageName){
			return 1
		}
	}
}

if (IsPackageInstalled("Phone.Identity.Membership")){
	Throw [system.Exception]("This package is not compatible with the 'Phone.Identity.Membership' NuGet package.");
}