#addin "Cake.Xamarin"
#tool "nuget:?package=GitVersion.CommandLine"

var sln = "Chafu.sln";
var nuspec = "nuspec/chafu.nuspec";
var target = Argument("target", "Default");


Task("Clean").Does(() =>
{
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
});

Task("Version").Does(() => {
	
});

Task("Restore").Does(() => {
	NuGetRestore(sln);
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.IsDependentOn("Restore")
	.Does(() => 
	{
	    iOSBuild("./Chafu/Chafu.csproj", 
	        new MDToolSettings { Configuration = "Release" });
	});

Task("Default").IsDependentOn("clean").IsDependentOn("lib").Does(() => {});

RunTarget(target);