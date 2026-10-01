namespace Dojo.Tests;

[TestClass]
public sealed class ProgressSaveTests
{
    [TestMethod]
    public void FailedReplacementLeavesPreviousBytesAndNoCliSuccess()
    {
        using var workspace = new DojoWorkspace();
        var progress = Progress.Fresh();
        progress.ActiveChallengeId = "csharp-001";
        workspace.Store.Save(progress);
        var before = File.ReadAllBytes(workspace.ProgressPath);
        // Reading is allowed, but Windows denies replacement while this handle is open.
        using (var locked = new FileStream(workspace.ProgressPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            foreach (var command in new[] { "next", "hint" })
            {
                var result = workspace.Run(command);
                Assert.AreEqual(1, result.ExitCode);
                Assert.AreEqual("", result.Output);
                StringAssert.Contains(result.Error, "Error:");
            }
        }
        CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
        Assert.AreEqual("csharp-001", workspace.Store.Load().ActiveChallengeId);
        Assert.IsEmpty(Directory.GetFiles(Path.GetDirectoryName(workspace.ProgressPath)!, "*.tmp"));
    }

    [TestMethod]
    public void FailedTemporaryWriteLeavesPreviousBytesAndNoResult()
    {
        using var workspace = new DojoWorkspace();
        var progress = Progress.Fresh();
        progress.ActiveChallengeId = "csharp-001";
        workspace.Store.Save(progress);
        var before = File.ReadAllBytes(workspace.ProgressPath);
        var store = new ProgressStore(workspace.ProgressPath, (temporary, contents) =>
        {
            File.WriteAllText(temporary, "partial");
            throw new IOException("Simulated interrupted temporary write.");
        });
        var app = new DojoApp(ChallengeCatalog.Load(workspace.Curriculum), store);
        Assert.Throws<IOException>(() => app.Next());
        Assert.Throws<IOException>(() => app.Hint());
        CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
        Assert.AreEqual("csharp-001", workspace.Store.Load().ActiveChallengeId);
        Assert.IsEmpty(Directory.GetFiles(Path.GetDirectoryName(workspace.ProgressPath)!, "*.tmp"));
    }
}
