#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitlink"

var sln = new FilePath("Chafu.sln");
var project = new FilePath("Chafu/Chafu.csproj");
var sample = new FilePath("Sample/Sample.csproj");
var binDir = new DirectoryPath("Chafu/bin/Release");
var nuspec = new FilePath("chafu.nuspec");
var outputDir = new DirectoryPath("artifacts");
var target = Argument("target", "Default");

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

var isRunningOnBitrise = Bitrise.IsRunningOnBitrise;
var bitrisePullRequest = Bitrise.Environment.Repository.PullRequest;

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
	
	DotNetBuild(sln, 
		settings => settings.SetConfiguration("Release")
							.WithProperty("DebugSymbols", "true")
            				.WithProperty("DebugType", "Full")
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

	var dllDir = binDir + "/Chafu.*";

	Information("Dll Dir: {0}", dllDir);

	var nugetContent = new List<NuSpecContent>();
	foreach(var dll in GetFiles(dllDir)){
	 	Information("File: {0}", dll.ToString());
		nugetContent.Add(new NuSpecContent {
			Target = "lib/Xamarin.iOS10",
			Source = dll.ToString()
		});
	}

	Information("File Count {0}", nugetContent.Count);

	NuGetPack(nuspec, new NuGetPackSettings {
		Authors = new [] { "Tomasz Cielecki" },
		Owners = new [] { "Brüel & Kjær" },
		IconUrl = new Uri("http://i.imgur.com/V3983YY.png"),
		ProjectUrl = new Uri("https://github.com/BruelAndKjaer/Chafu"),
		LicenseUrl = new Uri("https://github.com/BruelAndKjaer/Chafu/blob/master/LICENSE"),
		Copyright = "Copyright (c) Brüel & Kjær",
		RequireLicenseAcceptance = false,
		ReleaseNotes = ParseReleaseNotes("./releasenotes.md").Notes.ToArray(),
		Tags = new [] {"fusuma", "photo", "media", "video", "picker", "browser", "mobile",
			"xamarin", "ios"},
		Version = versionInfo.NuGetVersion,
		Symbols = false,
		NoPackageAnalysis = true,
		OutputDirectory = outputDir,
		Verbosity = NuGetVerbosity.Detailed,
		Files = nugetContent,
		BasePath = "/."
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

Task("UploadBitriseArtifact")
	.IsDependentOn("Package")
	.WithCriteria(() => isRunningOnBitrise)
	.Does(() => {

	Information("Artifacts Dir: {0}", outputDir.FullPath);

	var deployDir = Bitrise.Environment.Directory.DeployDirectory;

	foreach(var file in GetFiles(outputDir.FullPath + "/*")) {
		Information("Moving file {0} to deloy dir", file.FullPath);

		MoveFileToDirectory(file.FullPath, deployDir);
	}
});

Task("Default")
	.IsDependentOn("UploadAppVeyorArtifact")
	.IsDependentOn("UploadBitriseArtifact")
	.Does(() => {
	
	Information("Bitrise: {0}", isRunningOnBitrise);
	Information("AppVeyor: {0}", isRunningOnAppVeyor);

	});

RunTarget(target);
