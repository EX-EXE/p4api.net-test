using Microsoft.Extensions.Logging;
using Perforce.P4;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace P4ApiDotNetTests.tests;

public class StreamDepotTest
    : TestBase
{
    private readonly Perforce.P4.Depot depot;
    private readonly Perforce.P4.Stream stream;

    public StreamDepotTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        using var repository = CreateAndConnectByEnvironment();
        this.depot = CreateStreamDepot(repository, nameof(StreamDepotTest), 1);
        this.stream = CreateStream(repository, $"//{depot.Id}/test");
    }

    [Fact]
    public void GetActiveTicket()
    {
        using var repository = CreateAndConnectByEnvironment();
        var client = CreateWorkspace(repository, stream.Id);
        repository.Connection.Client = client;

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
            FileGenerator.GenerateRandomBinaryFile(path, 4 * 1024 * 1024);
            repository.Connection.Client.AddFiles(new AddFilesCmdOptions(AddFilesCmdFlags.None, changeList.Id, null), new LocalPath(path));
        }

        var clientOptions = new ClientSubmitOptions(false, SubmitType.SubmitUnchanged);
        var submitOptions = new SubmitCmdOptions(
            SubmitFilesCmdFlags.None,
            changeList.Id, 
            null,
            "",
            clientOptions);
        var submit = repository.Connection.Client.SubmitFiles(submitOptions, null);
    }
}