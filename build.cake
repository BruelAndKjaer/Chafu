#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"

var sln = new FilePath("Chafu.sln");
var project = new FilePath("Chafu/Chafu.csproj");
var nuspec = new FilePath("chafu.nuspec");
var outputDir = new DirectoryPath("artifacts");
var target = Argument("target", "Default");

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

Task("Clean").Does(() =>
{
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
	CleanDirectories(outputDir.FullPath);
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
	
	DotNetBuild(project, 
		settings => settings.SetConfiguration("Release")
							.WithTarget("Build")
	);
});

Task("Package")
	.IsDependentOn("Build")
	.Does(() => {
	if (IsRunningOnWindows()) //pdbstr.exe and costura are not xplat currently
		GitLink(sln.GetDirectory(), new GitLinkSettings {
			ArgumentCustomization = args => args.Append("-ignore Sample")
		});

	EnsureDirectoryExists(outputDir);

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

	Information("Artifacts Dir: {0}", outputDir.FullPath);

	foreach(var file in GetFiles(outputDir.FullPath + "/*")) {
		Information("Uploading {0}", file.FullPath);
		AppVeyor.UploadArtifact(file.FullPath);
	}
});

Task("Default").IsDependentOn("UploadAppVeyorArtifact").Does(() => {});

RunTarget(target);