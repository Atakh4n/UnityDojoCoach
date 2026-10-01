using System.Text.Json;

namespace Dojo.Tests;

[TestClass]
public sealed class ErrorTests
{
    [TestMethod]
    public void ArgumentErrorsHaveUsageExitTwoAndNeverSave()
    {
        using var workspace = new DojoWorkspace();
        foreach (var args in new[] { Array.Empty<string>(), new[] { "unknown" }, new[] { "next", "extra" } })
        {
            var result = workspace.Run(args);
            Assert.AreEqual(2, result.ExitCode);
            Assert.AreEqual("", result.Output);
            StringAssert.Contains(result.Error, "Usage: dojo <status|next|hint>");
            Assert.IsFalse(File.Exists(workspace.ProgressPath));
        }
    }

    [TestMethod]
    public void MalformedAndInvalidProgressNeverGetsOverwritten()
    {
        using var workspace = new DojoWorkspace();
        Directory.CreateDirectory(Path.GetDirectoryName(workspace.ProgressPath)!);
        var invalidStates = new[]
        {
            "{", "null", "{}",
            "{\"activeChallengeId\":null,\"completedChallengeIds\":[\"x\",\"x\"],\"hintCounts\":{}}",
            "{\"activeChallengeId\":\"x\",\"completedChallengeIds\":[\"x\"],\"hintCounts\":{}}",
            "{\"activeChallengeId\":null,\"completedChallengeIds\":[],\"hintCounts\":{\"x\":5}}",
            "{\"activeChallengeId\":null,\"completedChallengeIds\":[],\"hintCounts\":{\"x\":-1}}",
            "{\"activeChallengeId\":null,\"completedChallengeIds\":[\"\"],\"hintCounts\":{}}"
        };
        foreach (var json in invalidStates)
        {
            File.WriteAllText(workspace.ProgressPath, json);
            var before = File.ReadAllBytes(workspace.ProgressPath);
            foreach (var command in new[] { "status", "next", "hint" })
            {
                var result = workspace.Run(command);
                Assert.AreEqual(1, result.ExitCode, json);
                Assert.AreEqual("", result.Output);
                StringAssert.Contains(result.Error, "Error:");
                CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
            }
        }
    }

    [TestMethod]
    public void UnreadableProgressFailsWithoutReset()
    {
        using var workspace = new DojoWorkspace();
        workspace.Store.Save(Progress.Fresh());
        var before = File.ReadAllBytes(workspace.ProgressPath);
        using (var locked = new FileStream(workspace.ProgressPath, FileMode.Open, FileAccess.Read, FileShare.None))
            foreach (var command in new[] { "status", "next", "hint" })
                Assert.AreEqual(1, workspace.Run(command).ExitCode);
        CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
    }

    [TestMethod]
    public void InvalidCurriculumFailsAndKeepsValidProgress()
    {
        using var workspace = new DojoWorkspace(1);
        workspace.Store.Save(Progress.Fresh());
        var before = File.ReadAllBytes(workspace.ProgressPath);
        var valid = File.ReadAllText(Path.Combine(workspace.Curriculum, "001.json"));
        var invalidFiles = new[] { "{", "{}", "null", valid.Replace("\"order\": 1", "\"order\": 0"),
            valid.Replace("\"title\": \"Exercise 1\"", "\"title\": \"\""),
            valid.Replace("What do you expect?", "") };
        foreach (var json in invalidFiles)
        {
            File.WriteAllText(Path.Combine(workspace.Curriculum, "001.json"), json);
            foreach (var command in new[] { "status", "next", "hint" })
                Assert.AreEqual(1, workspace.Run(command).ExitCode, json);
            CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
        }
        File.WriteAllText(Path.Combine(workspace.Curriculum, "001.json"), valid);
        File.WriteAllText(Path.Combine(workspace.Curriculum, "duplicate.json"), valid);
        Assert.AreEqual(1, workspace.Run("next").ExitCode);
        var duplicateOrder = JsonSerializer.Deserialize<Challenge>(valid, ProgressStore.JsonOptions)!;
        duplicateOrder.Id = "another";
        File.WriteAllText(Path.Combine(workspace.Curriculum, "duplicate.json"), JsonSerializer.Serialize(duplicateOrder, ProgressStore.JsonOptions));
        Assert.AreEqual(1, workspace.Run("next").ExitCode);
        CollectionAssert.AreEqual(before, File.ReadAllBytes(workspace.ProgressPath));
    }
}
