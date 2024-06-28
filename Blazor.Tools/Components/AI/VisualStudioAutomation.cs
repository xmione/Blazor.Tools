using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
namespace Blazor.Tools.Components.AI
{
    public class VisualStudioAutomation
    {
        public void CompileAndRunProject(string projectFilePath)
        {
            var projectCollection = new ProjectCollection();
            var buildParameters = new BuildParameters(projectCollection);
            var buildRequestData = new BuildRequestData(projectFilePath, new Dictionary<string, string>(), null, new[] { "Build" }, null);
            var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequestData);

            if (buildResult.OverallResult == BuildResultCode.Success)
            {
                var process = System.Diagnostics.Process.Start(projectFilePath);
                process.WaitForExit();
            }
            else
            {
                Console.WriteLine("Build failed.");
            }
        }
    }
}
