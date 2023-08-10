>\Packages.props <-- Containing packages with versions in format `<PackageReference Update="PackageA" Version="default">`

>\Directory.Build.targets <-- Applying Microsoft.Build.CentralPackageVersions packages for every csproj. This lets us use the whole mechanics of managing nuggets from Packages.props

>*.csproj (one of the project files) <-- Should have *PackageReference* as well, but without specifying *Version* property. If version has to be overriden for specific project it cna be done with syntax: `<PackageReference Includ="PackageA" OverrideVersion="override">`

[Documentation for approach](https://github.com/microsoft/MSBuildSdks/tree/main/src/CentralPackageVersions)