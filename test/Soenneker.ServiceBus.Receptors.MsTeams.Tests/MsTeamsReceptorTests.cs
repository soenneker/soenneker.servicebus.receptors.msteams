using Soenneker.ServiceBus.Receptors.MsTeams.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.ServiceBus.Receptors.MsTeams.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class MsTeamsReceptorTests : HostedUnitTest
{
    private readonly IMsTeamsReceptor _util;

    public MsTeamsReceptorTests(Host host) : base(host)
    {
        _util = Resolve<IMsTeamsReceptor>(true);
    }

    [Test]
    public void Default()
    {

    }
}
