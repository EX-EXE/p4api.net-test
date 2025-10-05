using Microsoft.Extensions.Logging;
using Perforce.P4;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace P4ApiDotNetTests.tests;

public class ReadStreamDepotTest
    : TestBase
{
    private readonly Perforce.P4.Depot depot;
    private readonly Perforce.P4.Stream stream;
    private readonly Perforce.P4.FileSubmitRecord[] submitFiles;
    private readonly HashSet<string> submitFileHashes = new();

    public ReadStreamDepotTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        using var repository = CreateAndConnectByEnvironment();
        this.depot = CreateStreamDepot(repository, nameof(ReadStreamDepotTest), 1);
        this.stream = CreateStream(repository, $"//{depot.Id}/{DateTimeOffset.Now:yyyy_MM_dd_HH_mm_ss_ffffff}");

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
            submitFileHashes.Add(hash);

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
        submitFiles = submit.Files.ToArray();
        foreach (var submitFile in submit.Files)
        {
            GetLogger().LogInformation($"{submitFile.File} - {submitFile.Action}");
        }
    }


    [Fact]
    public void GetFileContents()
    {
        using var repository = CreateAndConnectByEnvironment();

        foreach (var submitFile in submitFiles)
        {
            var temp = System.IO.Path.GetTempFileName();
            var fileContent = repository.GetFileContents(new GetFileContentsCmdOptions(GetFileContentsCmdFlags.None, temp), submitFile.File);
            var hash = FileUtility.ComputeMd5Hash(temp);
            Assert.Contains(hash, submitFileHashes);
        }
    }
    [Fact]
    public void GetFileContentsParallel()
    {
        Parallel.ForEach(submitFiles, (submitFile) =>
        {
            using var repository = CreateAndConnectByEnvironment();
            var temp = System.IO.Path.GetTempFileName();
            var fileContent = repository.GetFileContents(new GetFileContentsCmdOptions(GetFileContentsCmdFlags.None, temp), submitFile.File);
            var hash = FileUtility.ComputeMd5Hash(temp);
            Assert.Contains(hash, submitFileHashes);
        });
    }

    //[Fact]
    //public void GetFileContentsParallel()
    //{
    //    using var repository = CreateAndConnectByEnvironment();

    //    Parallel.ForEach(submitFiles, (submitFile) =>
    //    {
    //        var temp = System.IO.Path.GetTempFileName();
    //        var fileContent = repository.GetFileContents(new GetFileContentsCmdOptions(GetFileContentsCmdFlags.None, temp), submitFile.File);
    //        var hash = FileUtility.ComputeMd5Hash(temp);
    //        Assert.Contains(hash, submitFileHashes);
    //    });
    //}
}