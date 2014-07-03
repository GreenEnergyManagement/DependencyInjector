using System.ServiceModel;

namespace Gem.DependencyInjector.Wcf.Test
{
    [Configuration]
    public class Configuration : IConfigureDependencies
    {
        [Definition]
        public ITestService GetSomeService()
        {
            return new TestService();
        }

        [Definition]
        public HostWrappingServiceFactory<ITestService> GetWcfServiceFactory(ITestService service)
        {
            return new HostWrappingServiceFactory<ITestService>(service, new BasicHttpBinding(), "localhost", 0);
        }
    }
}