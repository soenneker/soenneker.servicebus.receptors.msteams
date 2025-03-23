using Soenneker.ServiceBus.Receptors.MsTeams.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.ServiceBus.Receptors.MsTeams.Tests;

[Collection("Collection")]
public class MsTeamsReceptorTests : FixturedUnitTest
{
    private readonly IMsTeamsReceptor _util;

    public MsTeamsReceptorTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IMsTeamsReceptor>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
