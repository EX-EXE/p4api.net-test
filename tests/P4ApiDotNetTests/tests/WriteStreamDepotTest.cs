using Microsoft.Extensions.Logging;
using Perforce.P4;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace P4ApiDotNetTests.tests;

public class WriteStreamDepotTest
    : TestBase
{
    private readonly Perforce.P4.Depot depot;

    public WriteStreamDepotTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        using var repository = CreateAndConnectByEnvironment();
        this.depot = CreateStreamDepot(repository, nameof(WriteStreamDepotTest), 1);
    }


    [Fact]
    public void SubmitFiles()
    {
        using var repository = CreateAndConnectByEnvironment();
        var stream = CreateStream(repository, $"//{depot.Id}/{nameof(SubmitFiles)}-{DateTimeOffset.Now:yyyy_MM_dd_HH_mm_ss_ffffff}");

        // Workspace
        var client = CreateWorkspace(repository, stream.Id);
        repository.Connection.Client = client;

        // Write Files
        var changeList = repository.GetChangelist(-1, new ChangeCmdOptions(ChangeCmdFlags.Output, ChangeListType.Restricted));
        changeList.Description = $"Create.[{DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss")}]";
        changeList = repository.CreateChangelist(changeList);
        foreach (var num in Enumerable.Range(0, 100))
        {
            if (!System.IO.Directory.Exists(client.Root))
            {
                System.IO.Directory.CreateDirectory(client.Root);
            }
            var path = System.IO.Path.Combine(client.Root, $"{num}.data");
            FileUtility.GenerateRandomBinaryFile(path, 4 * 1024 * 1024);
            var hash = FileUtility.ComputeMd5Hash(path);
            GetLogger().LogInformation($"{path} - {hash}");
            repository.Connection.Client.AddFiles(new AddFilesCmdOptions(AddFilesCmdFlags.None, changeList.Id, null), new LocalPath(path));
        }

        // Submit
        var clientOptions = new ClientSubmitOptions(false, SubmitType.SubmitUnchanged);
        var submitOptions = new SubmitCmdOptions(
            SubmitFilesCmdFlags.None,
            changeList.Id,
            null,
            "",
            clientOptions);
        var submit = repository.Connection.Client.SubmitFiles(submitOptions, null);
        foreach (var submitFile in submit.Files)
        {
            GetLogger().LogInformation($"{submitFile.File} - {submitFile.Action}");
        }
    }

    [Fact]
    public async Task SubmitFilesAsync()
    {
        using var repository = CreateAndConnectByEnvironment();
        var stream = CreateStream(repository, $"//{depot.Id}/{nameof(SubmitFilesAsync)}-{DateTimeOffset.Now:yyyy_MM_dd_HH_mm_ss_ffffff}");

        // Workspace
        var client = CreateWorkspace(repository, stream.Id);
        repository.Connection.Client = client;

        // Write Files
        var changeList = repository.GetChangelist(-1, new ChangeCmdOptions(ChangeCmdFlags.Output, ChangeListType.Restricted));
        changeList.Description = $"Create.[{DateTimeOffset.Now.ToString("yyyy/MM/dd HH:mm:ss")}]";
        changeList = repository.CreateChangelist(changeList);
        foreach (var num in Enumerable.Range(0, 100))
        {
            if (!System.IO.Directory.Exists(client.Root))
            {
                System.IO.Directory.CreateDirectory(client.Root);
            }
            var path = System.IO.Path.Combine(client.Root, $"{num}.data");
            FileUtility.GenerateRandomBinaryFile(path, 4 * 1024 * 1024);
            var hash = FileUtility.ComputeMd5Hash(path);
            GetLogger().LogInformation($"{path} - {hash}");
            repository.Connection.Client.AddFiles(new AddFilesCmdOptions(AddFilesCmdFlags.None, changeList.Id, null), new LocalPath(path));
            await Task.Yield();
        }

        // Submit
        await Task.Delay(1000);
        var clientOptions = new ClientSubmitOptions(false, SubmitType.SubmitUnchanged);
        var submitOptions = new SubmitCmdOptions(
            SubmitFilesCmdFlags.None,
            changeList.Id,
            null,
            "",
            clientOptions);
        var submit = repository.Connection.Client.SubmitFiles(submitOptions, null);
        foreach (var submitFile in submit.Files)
        {
            GetLogger().LogInformation($"{submitFile.File} - {submitFile.Action}");
        }
    }
}