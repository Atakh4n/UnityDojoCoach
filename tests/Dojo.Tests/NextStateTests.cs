namespace Dojo.Tests;

[TestClass, TestCategory("US1")]
public sealed class NextStateTests
{
    [TestMethod]
    public void EachProcessAdvancesOnceAndPersistsDistinctCompletions()
    {
        using var workspace = new DojoWorkspace();
        // Filenames do not determine authored order.
        File.Move(Path.Combine(workspace.Curriculum, "001.json"), Path.Combine(workspace.Curriculum, "z.json"));
        Assert.AreEqual(0, workspace.Run("next").ExitCode);
        Assert.AreEqual("csharp-001", workspace.Store.Load().ActiveChallengeId);
        Assert.IsEmpty(workspace.Store.Load().CompletedChallengeIds);
        Assert.AreEqual(0, workspace.Run("next").ExitCode);
        Assert.AreEqual("csharp-002", workspace.Store.Load().ActiveChallengeId);
        CollectionAssert.AreEqual(new[] { "csharp-001" }, workspace.Store.Load().CompletedChallengeIds);
        Assert.AreEqual(0, workspace.Run("next").ExitCode);
        Assert.IsNull(workspace.Store.Load().ActiveChallengeId);
        Assert.HasCount(2, workspace.Store.Load().CompletedChallengeIds);
    }

    [TestMethod]
    public void UnavailableActiveFailsWithoutCompletingOrRewriting()
    {
        using var workspace = new DojoWorkspace(0);
        var progress = Progress.Fresh();
        progress.ActiveChallengeId = "missing";
        workspace.Store.Save(progress);
        var before = File.ReadAllBytes(workspace.ProgressPath);
        var result = workspace.Run("next");
        Assert.AreEqual(1, result.ExitCode);
        Assert.AreEqual("", result.Output);
        StringAssert.Contains(result.Error, "Error: Challenge missing is unavailable.");
        CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
    }

    [TestMethod]
    public void HistoricalDataSurvivesEmptyAndRestoredCurriculum()
    {
        using var workspace = new DojoWorkspace(0);
        var progress = Progress.Fresh();
        progress.CompletedChallengeIds.Add("old");
        progress.HintCounts["old"] = 3;
        workspace.Store.Save(progress);
        var before = File.ReadAllBytes(workspace.ProgressPath);
        Assert.AreEqual(0, workspace.Run("next").ExitCode);
        CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
        workspace.AddChallenge(1);
        Assert.AreEqual(0, workspace.Run("next").ExitCode);
        var saved = workspace.Store.Load();
        CollectionAssert.AreEqual(new[] { "old" }, saved.CompletedChallengeIds);
        Assert.AreEqual(3, saved.HintCounts["old"]);
    }
}
