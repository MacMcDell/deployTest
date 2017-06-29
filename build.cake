#tool nuget:?package=vswhere

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
const string solutionFile = "./DeployTest.sln";

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./*/bin");
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionFile);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(solutionFile, settings => settings.SetConfiguration(configuration));
});

const string testDllsPattern = "./*.Tests/bin/**/*.Tests.dll";

// Specify MSTest path until Cake has built-in support for VS 2017
// See: http://cakebuild.net/blog/2017/03/vswhere-and-visual-studio-2017-support
//      https://github.com/cake-build/cake/issues/1522
var vsLatestDirectoryPath = VSWhereLatest();
var msTestFilePath = vsLatestDirectoryPath == null
    ? null
    : vsLatestDirectoryPath.CombineWithFilePath(@"Common7\IDE\MSTest.exe");

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    MSTest(testDllsPattern, new MSTestSettings() {
        NoIsolation = false,
        Category = "!Integration", // Don't include integration tests
        ToolPath = msTestFilePath
    });
});

Task("Run-Integration-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    MSTest(testDllsPattern, new MSTestSettings() {
        NoIsolation = false,
        Category = "Integration",
        ToolPath = msTestFilePath
    });
});

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

RunTarget(target);
