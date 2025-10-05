using Microsoft.Extensions.Logging;
using Perforce.P4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace P4ApiDotNetTests;

public class TestBase
{
    private readonly ITestOutputHelper testOutputHelper;
    private readonly ILogger logger;
    protected ILogger GetLogger() => logger;

    public TestBase(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        this.logger = new TestLogger(testOutputHelper);
    }

    public Perforce.P4.Repository CreateAndConnect(
        string address,
        string user,
        string password)
    {
        var serverAddress = new ServerAddress(address);
        var serverData = new Perforce.P4.Server(serverAddress);
        var repository = new Perforce.P4.Repository(serverData);
        var connection = repository.Connection;

        var connectResult = connection.TrustAndConnect(new TrustCmdOptions(TrustCmdFlags.AutoAccept | TrustCmdFlags.ForceReplacement), null, null);
        if (!connectResult)
        {
            throw new InvalidOperationException($"Failed connect.");
        }

        connection.UserName = user;
        var loginResult = connection.Login(password, false);
        if (loginResult == null || string.IsNullOrEmpty(loginResult.Ticket))
        {
            throw new InvalidOperationException($"Failed login.");
        }
        return repository;
    }

    public Perforce.P4.Repository CreateAndConnectByEnvironment()
    {
        string GetEnvironmentVariable(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Failed get environment. {key}");
            }
            return value;
        }
        var p4Port = GetEnvironmentVariable("P4_PORT");
        var p4User = GetEnvironmentVariable("P4_USER");
        var p4Pass = GetEnvironmentVariable("P4_PASS");
        GetLogger().LogInformation($"Environment. P4PORT:{p4Port} P4USER:{p4User}");
        return CreateAndConnect(p4Port, p4User, p4Pass);
    }
}
