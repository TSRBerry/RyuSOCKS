// Copyright (C) RyuSOCKS
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2,
// as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RyuSocks.Test.Integration
{
    public class ServerEnvironment : IAsyncDisposable, IDisposable
    {
        private const string FTPServerImage = "delfer/alpine-ftp-server";
        private const string FTPServerTag = "latest";
        private const string FTPMountPath = "/test";
        private const string NetworkName = "ryusocks-test-network";
        private const string TestServerImage = "ryusocks-test-server";

        public const string FTPUsername = "foo";
        public const string FTPPassword = "bar";

        public static readonly string ProjectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
        private static readonly string _assetsPath = Path.Combine(ProjectPath, "assets");
        public static readonly DirectoryInfo FTPServerFiles = new(_assetsPath);

        private static readonly Dictionary<string, string> _buildContext = new()
        {
            { "RyuSocks.Test.Integration/docker/Dockerfile", "Dockerfile" },
            { "RyuSocks.Generator", "RyuSocks.Generator" },
            { "RyuSocks", "RyuSocks" },
            { "RyuSocks.Test.Integration/docker/TestServer.cs", "RyuSocks/Program.cs" },
        };

        private DockerClient _docker;

        private string _networkId;
        private string _ftpContainerId;
        private string _ryuSocksContainerId;

        public string RyuSocksIPAddress { get; private set; }
        public string FTPServerIPAddress { get; private set; }

        public ServerEnvironment()
        {
            CreateEnvironment().Wait();
        }

        private async Task CreateEnvironment()
        {
            _docker = new DockerClientConfiguration().CreateClient();

            try
            {
                await _docker.Images.CreateImageAsync(
                    new ImagesCreateParameters { FromImage = FTPServerImage, Tag = FTPServerTag, },
                    null,
                    new Progress<JSONMessage>()
                );

                await BuildTestServer();

                _networkId = (await _docker.Networks.CreateNetworkAsync(
                    new NetworksCreateParameters
                    {
                        Attachable = true,
                        Driver = "bridge",
                        Internal = true,
                        Name = NetworkName,
                    }
                )).ID;

                NetworkingConfig containerNetwork = new()
                {
                    EndpointsConfig = new Dictionary<string, EndpointSettings>
                    {
                        { NetworkName, new EndpointSettings { NetworkID = _networkId } },
                    },
                };

                _ftpContainerId = (await _docker.Containers.CreateContainerAsync(
                    new CreateContainerParameters
                    {
                        Image = FTPServerImage,
                        Env =
                        [
                            $"USERS=\"{FTPUsername}|{FTPPassword}|{FTPMountPath}|1000\"",
                        ],
                        HostConfig = new HostConfig
                        {
                            AutoRemove = true,
                            Mounts =
                            [
                                new Mount { Type = "bind", Source = _assetsPath, Target = FTPMountPath },
                            ],
                        },
                        Name = "ryusocks-ftpserver",
                        Hostname = "ftpserver",
                        NetworkingConfig = containerNetwork,
                    }
                )).ID;

                _ryuSocksContainerId = (await _docker.Containers.CreateContainerAsync(
                    new CreateContainerParameters
                    {
                        Image = TestServerImage,
                        HostConfig = new HostConfig
                        {
                            AutoRemove = true,
                        },
                        Name = "ryusocks-server",
                        Hostname = "ryusocks-server",
                        NetworkingConfig = containerNetwork,
                    }
                )).ID;

                await _docker.Containers.StartContainerAsync(_ftpContainerId, new ContainerStartParameters());
                await _docker.Containers.StartContainerAsync(_ryuSocksContainerId, new ContainerStartParameters());

                FTPServerIPAddress = (await _docker.Containers.InspectContainerAsync(_ftpContainerId)).NetworkSettings
                    .Networks[NetworkName].IPAddress;
                RyuSocksIPAddress = (await _docker.Containers.InspectContainerAsync(_ryuSocksContainerId)).NetworkSettings
                    .Networks[NetworkName].IPAddress;
            }
            catch (DockerApiException)
            {
                await DestroyEnvironment();
                throw;
            }
        }

        private async Task BuildTestServer()
        {
            MemoryStream tarMemory = new();
            TarWriter tarWriter = new(tarMemory, true);

            string solutionPath = Path.GetFullPath(Path.Combine(ProjectPath, ".."));

            foreach ((string hostPath, string tarPath) in _buildContext)
            {
                string fullHostPath = Path.GetFullPath(Path.Combine(solutionPath, hostPath));

                // Check if the current path is a directory
                if (Directory.Exists(fullHostPath))
                {
                    foreach (var hostFilePath in Directory.EnumerateFiles(fullHostPath, "*", SearchOption.AllDirectories))
                    {
                        string relativeFilePath = Path.GetRelativePath(fullHostPath, hostFilePath);
                        string[] fileParts = relativeFilePath.Split(Path.DirectorySeparatorChar);
                        // Ignore files in bin/ and obj/
                        if (fileParts.Contains("obj") || fileParts.Contains("bin"))
                        {
                            continue;
                        }

                        // Add file to tar archive
                        await tarWriter.WriteEntryAsync(hostFilePath, Path.Combine(tarPath, relativeFilePath));
                    }

                    continue;
                }

                // Add file to tar archive
                await tarWriter.WriteEntryAsync(fullHostPath, tarPath);
            }

            await tarWriter.DisposeAsync();

            tarMemory.Position = 0;

            await _docker.Images.BuildImageFromDockerfileAsync(
                new ImageBuildParameters
                {
                    Tags = [$"{TestServerImage}:latest"],
                    Remove = true,
                    ForceRemove = true,
                    Pull = "always",
                },
                tarMemory,
                null,
                null,
                new Progress<JSONMessage>()
            );

            await tarMemory.DisposeAsync();
        }

        private async Task DestroyEnvironment()
        {
            if (_ftpContainerId != null)
            {
                try
                {
                    await _docker.Containers.RemoveContainerAsync(_ftpContainerId, new ContainerRemoveParameters
                    {
                        Force = true,
                    });
                }
                catch (DockerContainerNotFoundException)
                {
                    // Ignored.
                }

                _ftpContainerId = null;
            }

            if (_ryuSocksContainerId != null)
            {
                try
                {
                    await _docker.Containers.RemoveContainerAsync(_ryuSocksContainerId, new ContainerRemoveParameters
                    {
                        Force = true,
                    });
                }
                catch (DockerContainerNotFoundException)
                {
                    // Ignored.
                }
                _ryuSocksContainerId = null;
            }

            if (_networkId != null)
            {
                await _docker.Networks.DeleteNetworkAsync(_networkId);
                _networkId = null;
            }

            try
            {
                await _docker.Images.DeleteImageAsync(TestServerImage, new ImageDeleteParameters { Force = true });
            }
            catch (DockerImageNotFoundException)
            {
                // Ignored.
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_docker != null)
            {
                await DestroyEnvironment().ConfigureAwait(false);

                _docker.Dispose();
            }

            _docker = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_docker != null)
                {
                    DestroyEnvironment().Wait();
                    _docker.Dispose();
                }

                _docker = null;
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            // Perform async cleanup.
            await DisposeAsyncCore().ConfigureAwait(false);

            // Dispose of unmanaged resources.
            Dispose(false);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}
