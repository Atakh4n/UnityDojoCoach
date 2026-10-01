namespace Dojo.Tests;

[TestClass, TestCategory("US1")]
public sealed class NextCliTests
{
    [TestMethod]
    public void FirstNextShowsChallengeAndTask()
    {
        using var workspace = new DojoWorkspace();
        var result = workspace.Run("next");
        Assert.AreEqual(0, result.ExitCode);
        StringAssert.Contains(result.Output, "Challenge: csharp-001 — Exercise 1");
        StringAssert.Contains(result.Output, "Investigate a small C# behavior");
        Assert.AreEqual("", result.Error);
    }

    [TestMethod]
    public void EmptyAndMissingCurriculumDoNotCreateProgress()
    {
        using var workspace = new DojoWorkspace(0);
        foreach (var missing in new[] { false, true })
        {
            if (missing) Directory.Delete(workspace.Curriculum);
            var result = workspace.Run("next");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains(result.Output, "No challenges available.");
            Assert.IsFalse(File.Exists(workspace.ProgressPath));
        }
    }

    [TestMethod]
    public void LastChallengeCompletesThenRepeatedNextDoesNotSave()
    {
        using var workspace = new DojoWorkspace(1);
        Assert.AreEqual(0, workspace.Run("next").ExitCode);
        StringAssert.Contains(workspace.Run("next").Output, "Curriculum complete.");
        var before = File.ReadAllBytes(workspace.ProgressPath);
        Assert.AreEqual(0, workspace.Run("next").ExitCode);
        CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
    }
}
