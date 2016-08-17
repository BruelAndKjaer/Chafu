#addin "Cake.Xamarin"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"

var sln = "./Chafu.sln";
var nuspec = "./chafu.nuspec";
var outputDir = "./artifacts/";
var target = Argument("target", "Default");

var isRunningOnAppVeyor = AppVeyor.isRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

Task("Clean").Does(() =>
{
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
});

GitVersion versionInfo = null;
Task("Version").Does(() => {
	GitVersion(new GitVersionSettings {
		UpdateAssemblyInfo = true,
		OutputType = GitVersionOutput.BuildServer
	});

	versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
	Information("VI:\t{0}", versionInfo.FullSemVer);
});

Task("Restore").Does(() => {
	NuGetRestore(sln);
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.IsDependentOn("Restore")
	.Does(() =>  {
	
	MDToolBuild("./Chafu/Chafu.csproj", 
		s => s.Configuration = "Release");
});

Task("Package")
	.IsDependentOn("Build")
	.Does(() => {
	//GitLink("./");

	NuGetPack(nuspec, new NuGetPackSettings{
		Version = versionInfo.NuGetVersion,
		Symbols = false,
		NoPackageAnalysis = true,
		OutputDirectory = outputDir
	});
});

Task("UploadAppVeyorArtifact")
	.IsDependentOn("Package")
	.WithCriteria(() => !isPullRequest)
	.WithCriteria(() => isRunningOnAppVeyor)
	.Does(() => {

	foreach(var file in GetFiles(outputDir)) {
		Information("Uploading {0}", file.FullPath);
		AppVeyor.UploadAppVeyorArtifact(file.FullPath);
	}
});

Task("Default").IsDependentOn("UploadAppVeyorArtifact").Does(() => {});

RunTarget(target);