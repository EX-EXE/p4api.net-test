using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace P4ApiDotNetTests.tests;

public class ConnectionTest(ITestOutputHelper testOutputHelper)
    : TestBase(testOutputHelper)
{
    [Fact]
    public void GetActiveTicket()
    {
        var repository = CreateAndConnectByEnvironment();
        var activeTicket = repository.Connection.GetActiveTicket();
        Assert.NotNull(activeTicket);
        GetLogger().LogInformation($"ActiveTicket : {activeTicket}");
    }
}