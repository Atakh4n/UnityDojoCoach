namespace Dojo.Tests;

[TestClass, TestCategory("US2")]
public sealed class StatusCliTests
{
    [TestMethod]
    public void FreshStatusShowsZeroCountsWithoutSaving()
    {
        using var workspace = new DojoWorkspace(0);
        var result = workspace.Run("status");
        Assert.AreEqual(0, result.ExitCode);
        StringAssert.Contains(result.Output, "Current challenge: none");
        StringAssert.Contains(result.Output, "Completed challenges: 0");
        StringAssert.Contains(result.Output, "Hints used: 0");
        Assert.IsFalse(File.Exists(workspace.ProgressPath));
    }

    [TestMethod]
    public void SeededStatusShowsTitleAndSavedTotals()
    {
        using var workspace = new DojoWorkspace();
        var progress = Progress.Fresh();
        progress.ActiveChallengeId = "csharp-002";
        progress.CompletedChallengeIds.Add("csharp-001");
        progress.HintCounts["csharp-001"] = 1;
        progress.HintCounts["csharp-002"] = 2;
        workspace.Store.Save(progress);
        var result = workspace.Run("status");
        Assert.AreEqual(0, result.ExitCode);
        StringAssert.Contains(result.Output, "Current challenge: csharp-002 — Exercise 2");
        StringAssert.Contains(result.Output, "Completed challenges: 1");
        StringAssert.Contains(result.Output, "Hints used: 3");
    }
}
