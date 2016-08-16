#addin "Cake.Xamarin"
#tool "nuget:?package=GitVersion.CommandLine"

var sln = "Fusuma.sln";
var nuspec = "nuspec/fusuma.nuspec";
var target = Argument("target", "Default");

Task("clean").Does(() =>
{
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
});

Task("lib").Does(() => 
{
    NuGetRestore(sln);

    iOSBuild("./Fusuma/Fusuma.csproj", 
        new MDToolSettings { Configuration = "Release" });
});

Task("Default").IsDependentOn("clean").IsDependentOn("lib").Does(() => {});

RunTarget(target);