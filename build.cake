#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=ReportUnit"
///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// PREPARATION
///////////////////////////////////////////////////////////////////////////////

var projectName = "Enrichable";

// Get whether or not this is a local build.
var local = BuildSystem.IsLocalBuild;
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

// Parse release notes.
var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");

// Get version.
var semanticVersion = releaseNotes.Version.ToString();

// Define directories.
var sourceDirectory = Directory("./src");

var outputDirectory = Directory("./output");
var testResultsDirectory = outputDirectory + Directory("tests");
var artifactsDirectory = outputDirectory + Directory("artifacts");
var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());

// Define files.
var nugetExecutable = "./Tools/nuget.exe"; 

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
  // Executed BEFORE the first task.
  Information("Target: " + target);
  Information("Configuration: " + configuration);
  Information("Is local build: " + local.ToString());
  Information("Is running on AppVeyor: " + isRunningOnAppVeyor.ToString());
  Information("Semantic Version: " + semanticVersion);
  Information("NuGet Api Key: " + EnvironmentVariable("NuGetApiKey"));
});

Teardown(() =>
{
  // Executed AFTER the last task.
  Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
{
  // Clean solution directories.
  foreach(var path in solutionPaths)
  {
    Information("Cleaning {0}", path);
    CleanDirectories(artifactsDirectory.Path + "/");
    CleanDirectories(path + "/**/bin/" + configuration);
    CleanDirectories(path + "/**/obj/" + configuration);
  }

  CleanDirectories(outputDirectory);
});

Task("Create-Directories")
  .IsDependentOn("Clean")
  .Does(() =>
{
  var directories = new List<DirectoryPath>{ outputDirectory, testResultsDirectory, artifactsDirectory };
  directories.ForEach(directory => 
  {
    if (!DirectoryExists(directory))
    {
      CreateDirectory(directory);
    }
  });
});

Task("Restore-NuGet-Packages")
  .IsDependentOn("Create-Directories")
  .Does(() =>
{
  // Restore all NuGet packages.
  foreach(var solution in solutions)
  {
    Information("Restoring {0}...", solution);
    NuGetRestore(solution);
  }
});

Task("Patch-Assembly-Info")
  .IsDependentOn("Restore-NuGet-Packages")
  .WithCriteria(() => !local)
  .Does(() =>
{
  var assemblyInfoFiles = GetFiles("./**/AssemblyInfo.cs");
  foreach(var assemblyInfoFile in assemblyInfoFiles)
  {
    CreateAssemblyInfo(assemblyInfoFile, new AssemblyInfoSettings {
      Product = projectName,
      Version = semanticVersion,
      FileVersion = semanticVersion,
      InformationalVersion = semanticVersion,
      Copyright = "Copyright (c) Anders Ljusberg"
    });
  }
});

Task("Build")
  .IsDependentOn("Patch-Assembly-Info")
  .Does(() =>
{
  // Build all solutions.
  foreach(var solution in solutions)
  {
    Information("Building {0}", solution);
    MSBuild(solution, settings => 
      settings
        .WithTarget("Build")
        .SetConfiguration(configuration));
  }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
  var testAssemblies = GetFiles(sourceDirectory.Path + "/**/bin/" + configuration + "/*.Tests.dll");
  if(testAssemblies.Count() > 0)
  {
    XUnit2(sourceDirectory.Path + "/**/bin/" + configuration + "/*.Tests.dll", 
      new XUnit2Settings
      { 
        OutputDirectory = testResultsDirectory.Path,
        XmlReport = true
      }
    );
  }
}).
Finally(() => {
  ReportUnit(testResultsDirectory.Path);
});

Task("Create-NuGet-Packages")
  .IsDependentOn("Run-Unit-Tests")
  .Does(() =>
{
  var nuspecFiles = GetFiles(sourceDirectory.Path + "/**/*.nuspec");
  foreach(var nuspecFile in nuspecFiles)
  {
    NuGetPack(nuspecFile,
      new NuGetPackSettings
      {
        OutputDirectory = artifactsDirectory.Path,
        Version = semanticVersion
      }
    );
  }
});

Task("Update-AppVeyor-Build-Number")
  .WithCriteria(() => isRunningOnAppVeyor)
  .Does(() =>
{
  AppVeyor.UpdateBuildVersion(semanticVersion);
});

Task("Upload-AppVeyor-Artifacts")
  .IsDependentOn("Package")
  .WithCriteria(() => isRunningOnAppVeyor)
  .Does(() =>
{
  var artifacts = GetFiles(artifactsDirectory.Path + "/**/*.nupkg");
  foreach(var artifact in artifacts)
  {
    AppVeyor.UploadArtifact(artifact);
  }
});

Task("Publish-NuGet-Packages")
  .IsDependentOn("Upload-AppVeyor-Artifacts")
  .WithCriteria(() => !local)
  .WithCriteria(() => !isPullRequest)
  .Does(() =>
{
  // Resolve the API key.
  var apiKey = EnvironmentVariable("NuGetApiKey");
  if(string.IsNullOrEmpty(apiKey)) {
    throw new InvalidOperationException("Could not resolve NuGet API key.");
  }

  var nugetPackages = GetFiles(artifactsDirectory.Path + "/**/*.nupkg");
  foreach(var nugetPackage in nugetPackages)
  {
    NuGetPush(nugetPackage, new NuGetPushSettings {
      ApiKey = apiKey
    });
  }
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
  .IsDependentOn("Run-Unit-Tests");

Task("Package")
  .IsDependentOn("Create-NuGet-Packages");

Task("Publish")
  .IsDependentOn("Update-AppVeyor-Build-Number")
  .IsDependentOn("Upload-AppVeyor-Artifacts")
  .IsDependentOn("Publish-NuGet-Packages");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);
