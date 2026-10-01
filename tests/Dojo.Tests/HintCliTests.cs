namespace Dojo.Tests;

[TestClass, TestCategory("US3")]
public sealed class HintCliTests
{
    [TestMethod]
    public void HintRevealsExactlyOneAuthoredPromptPerInvocation()
    {
        using var workspace = new DojoWorkspace(1);
        var progress = Progress.Fresh();
        progress.ActiveChallengeId = "csharp-001";
        workspace.Store.Save(progress);
        var hints = ChallengeCatalog.Load(workspace.Curriculum)[0].Hints;
        for (var i = 0; i < 4; i++)
        {
            var result = workspace.Run("hint");
            Assert.AreEqual(0, result.ExitCode);
            Assert.AreEqual($"Hint {i + 1}/4: {hints[i]}{Environment.NewLine}", result.Output);
            Assert.AreEqual(i + 1, workspace.Store.Load().HintCounts["csharp-001"]);
        }
        Assert.AreEqual($"No more hints available.{Environment.NewLine}", workspace.Run("hint").Output);
    }

    [TestMethod]
    public void NoActiveHintExplainsWithoutCreatingProgress()
    {
        using var workspace = new DojoWorkspace(0);
        var result = workspace.Run("hint");
        Assert.AreEqual(0, result.ExitCode);
        StringAssert.Contains(result.Output, "No active challenge. Run dojo next.");
        Assert.IsFalse(File.Exists(workspace.ProgressPath));
    }
}
