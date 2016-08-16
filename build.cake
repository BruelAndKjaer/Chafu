#addin "Cake.Xamarin"

var sln = "Fusuma.sln";
var nuspec = "nuspec/fusuma.nuspec";
var target = Argument("target", "Default");

Task("clean").Does(() =>
{
    CleanDirectories("./**/bin");
    CleanDirectories("./**/obj");
});

Task("lib").IsDependentOn("clean").Does(() => 
{
    NuGetRestore(sln);

    iOSBuild("./Fusuma/Fusuma.csproj", 
        new MDToolSettings { Configuration = "Release" });
});

Task("Default").IsDependentOn("lib").Does(() => {});

RunTarget(target);