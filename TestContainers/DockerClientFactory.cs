using System;
using System.IO;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace TestContainers
{
    public sealed class DockerClientFactory
    {
        private static volatile DockerClientFactory _instance;

        private static readonly object SyncRoot = new object();

        private readonly DockerClientProviderStrategy _strategy  = DockerClientProviderStrategy.GetFirstValidStrategy();

        public static DockerClientFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new DockerClientFactory();
                    }
                }
                return _instance;
            }
        }

        public DockerClient Client() => _strategy.GetClient();

        public string GetDockerHostIpAddress(ContainerInspectResponse containerInfo)
        {
            var dockerHostUri = Client().Configuration.EndpointBaseUri;

            switch (dockerHostUri.Scheme)
            {
                case "http":
                case "https":
                case "tcp":
                    return dockerHostUri.Host;
                case "npipe": //will have to revisit this for LCOW/WCOW
                case "unix":
                    return File.Exists("/.dockerenv")
                        ? containerInfo.NetworkSettings.Gateway
                        : "localhost";
                default:
                    return null;
            }
        }
    }
}