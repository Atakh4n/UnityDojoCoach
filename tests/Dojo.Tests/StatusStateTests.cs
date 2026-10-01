namespace Dojo.Tests;

[TestClass, TestCategory("US2")]
public sealed class StatusStateTests
{
    [TestMethod]
    public void EmptyAndMissingCatalogPreserveUnavailableHistoryAndTotals()
    {
        using var workspace = new DojoWorkspace(0);
        var progress = Progress.Fresh();
        progress.ActiveChallengeId = "z-active";
        progress.CompletedChallengeIds.Add("a-completed");
        progress.HintCounts["a-completed"] = 1;
        progress.HintCounts["z-active"] = 2;
        progress.HintCounts["m-history"] = 4;
        workspace.Store.Save(progress);
        var before = File.ReadAllBytes(workspace.ProgressPath);
        foreach (var missing in new[] { false, true })
        {
            if (missing) Directory.Delete(workspace.Curriculum);
            var result = workspace.Run("status");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains(result.Output, "Current challenge: z-active (unavailable)");
            StringAssert.Contains(result.Output, "Unavailable challenges: a-completed, m-history, z-active");
            StringAssert.Contains(result.Output, "Completed challenges: 1");
            StringAssert.Contains(result.Output, "Hints used: 7");
            CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
        }
    }

    [TestMethod]
    public void MissingHistoricalDefinitionsWithoutActiveKeepTotals()
    {
        using var workspace = new DojoWorkspace();
        var progress = Progress.Fresh();
        progress.CompletedChallengeIds.Add("old");
        progress.HintCounts["old"] = 4;
        workspace.Store.Save(progress);
        var before = File.ReadAllBytes(workspace.ProgressPath);
        var result = workspace.Run("status");
        Assert.AreEqual(0, result.ExitCode);
        StringAssert.Contains(result.Output, "Current challenge: none");
        StringAssert.Contains(result.Output, "Hints used: 4");
        CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
    }
}
