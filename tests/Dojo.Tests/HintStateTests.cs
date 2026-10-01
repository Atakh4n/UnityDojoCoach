namespace Dojo.Tests;

[TestClass, TestCategory("US3")]
public sealed class HintStateTests
{
    [TestMethod]
    public void UnavailableActiveFailsEvenIfHintsExhausted()
    {
        using var workspace = new DojoWorkspace(0);
        foreach (var count in new[] { 0, 4 })
        {
            var progress = Progress.Fresh();
            progress.ActiveChallengeId = "missing";
            progress.HintCounts["missing"] = count;
            workspace.Store.Save(progress);
            var before = File.ReadAllBytes(workspace.ProgressPath);
            var result = workspace.Run("hint");
            Assert.AreEqual(1, result.ExitCode);
            Assert.AreEqual("", result.Output);
            StringAssert.Contains(result.Error, "Challenge missing is unavailable.");
            CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
        }
    }

    [TestMethod]
    public void HintRetainsHistoryAndExhaustionDoesNotRewrite()
    {
        using var workspace = new DojoWorkspace(1);
        var progress = Progress.Fresh();
        progress.ActiveChallengeId = "csharp-001";
        progress.CompletedChallengeIds.Add("old");
        progress.HintCounts["old"] = 2;
        progress.HintCounts["csharp-001"] = 3;
        workspace.Store.Save(progress);
        Assert.AreEqual(0, workspace.Run("hint").ExitCode);
        Assert.AreEqual(4, workspace.Store.Load().HintCounts["csharp-001"]);
        Assert.AreEqual(2, workspace.Store.Load().HintCounts["old"]);
        CollectionAssert.AreEqual(new[] { "old" }, workspace.Store.Load().CompletedChallengeIds);
        var before = File.ReadAllBytes(workspace.ProgressPath);
        Assert.AreEqual(0, workspace.Run("hint").ExitCode);
        CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
    }
}
