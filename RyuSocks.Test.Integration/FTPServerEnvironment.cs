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
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RyuSocks.Test.Integration
{
    public class FTPServerEnvironment : IAsyncDisposable, IDisposable
    {
        private const string FTPBaseServerImage = "delfer/alpine-ftp-server";
        private const string FTPServerImage = "ryusocks-test-ftpserver";
        private const string FTPServerTag = "latest";
        private const string FTPMountPath = "/test";
        private const string NetworkName = "ryusocks-test-network";
        private const string TestServerImage = "ryusocks-test-server";

        public const string FTPUsername = "foo";
        public const string FTPPassword = "bar";

        public static readonly string ProjectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
        private static readonly string _assetsPath = Path.Combine(ProjectPath, "assets");
        public static readonly DirectoryInfo FTPServerFiles = new(_assetsPath);

        private static readonly Dictionary<string, string> _ryuSocksBuildContext = new()
        {
            { "RyuSocks.Test.Integration/docker/server.Dockerfile", "Dockerfile" },
            { "RyuSocks.Generator", "RyuSocks.Generator" },
            { "RyuSocks", "RyuSocks" },
            { "RyuSocks.Test.Integration/docker/TestProject.csproj", "TestProject/TestProject.csproj" },
            { "RyuSocks.Test.Integration/docker/TestServer.cs", "TestProject/Program.cs" },
        };

        private static readonly Dictionary<string, string> _ftpBuildContext = new()
        {
            { "RyuSocks.Test.Integration/docker/ftp.Dockerfile", "Dockerfile" },
            { "RyuSocks.Test.Integration/docker/vsftpd.conf", "vsftpd.conf" },
            { "RyuSocks.Test.Integration/assets", "assets" },
        };

        private static readonly DockerClientConfiguration _dockerConfig;
        private readonly List<Thread> _threads = [];
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private DockerClient _docker;

        private string _networkId;
        private string _ftpContainerId;
        private string _ryuSocksContainerId;

        public string RyuSocksIPAddress { get; private set; }
        public string FTPServerIPAddress { get; private set; }

        static FTPServerEnvironment()
        {
            var dockerEndpoint = Environment.GetEnvironmentVariable("DOCKER_HOST");
            _dockerConfig = dockerEndpoint != null ? new DockerClientConfiguration(new Uri(dockerEndpoint)) : new DockerClientConfiguration();
        }

        public FTPServerEnvironment()
        {
            CreateEnvironment().Wait();
        }

        private async Task CreateEnvironment()
        {
            _docker = _dockerConfig.CreateClient();

            try
            {
                await _docker.Images.CreateImageAsync(
                    new ImagesCreateParameters { FromImage = FTPBaseServerImage, Tag = FTPServerTag, },
                    null,
                    new Progress<JSONMessage>()
                );

                await BuildDockerImage(_ftpBuildContext, FTPServerImage);
                await BuildDockerImage(_ryuSocksBuildContext, TestServerImage);

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
                            $"USERS={FTPUsername}|{FTPPassword}|{FTPMountPath}|1000",
                        ],
                        Name = "ryusocks-ftpserver",
                        Hostname = "ftpserver",
                        NetworkingConfig = containerNetwork,
                    }
                )).ID;

                _ryuSocksContainerId = (await _docker.Containers.CreateContainerAsync(
                    new CreateContainerParameters
                    {
                        Image = TestServerImage,
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

                _threads.Add(new Thread(() => _ = PrintContainerLogs(_ryuSocksContainerId, "RyuSocks", _cancellationTokenSource.Token)));
                _threads.Add(new Thread(() => _ = PrintContainerLogs(_ftpContainerId, "FTPServer", _cancellationTokenSource.Token)));

                foreach (var thread in _threads)
                {
                    thread.Start();
                }
            }
            catch (DockerApiException)
            {
                await DestroyEnvironment();
                throw;
            }
        }

        private async Task PrintContainerLogs(string containerId, string name, CancellationToken token)
        {
            var logStream = await _docker.Containers.GetContainerLogsAsync(
                containerId,
                false,
                new ContainerLogsParameters
                {
                    ShowStderr = true,
                    ShowStdout = true,
                    Follow = true,
                },
                token
            );

            var stdout = Console.OpenStandardOutput();
            var stderr = Console.OpenStandardError();

            var buffer = new byte[1024 * 1024];
            MultiplexedStream.ReadResult readResult = await logStream.ReadOutputAsync(buffer, 0, buffer.Length, token);

            while (!readResult.EOF && !token.IsCancellationRequested)
            {
                var stream = readResult.Target == MultiplexedStream.TargetStream.StandardError
                    ? stderr
                    : stdout;

                await stream.WriteAsync(Encoding.Default.GetBytes($"<{name}>: "), token);
                await stream.WriteAsync(buffer.AsMemory(0, readResult.Count), token);

                if (readResult.Count < buffer.Length)
                {
                    await stream.FlushAsync(token);
                }

                Array.Clear(buffer);
                readResult = await logStream.ReadOutputAsync(buffer, 0, buffer.Length, token);
            }

            stderr.Close();
            stdout.Close();
        }

        private async Task BuildDockerImage(Dictionary<string, string> buildContext, string imageName)
        {
            MemoryStream tarMemory = new();
            TarWriter tarWriter = new(tarMemory, true);

            string solutionPath = Path.GetFullPath(Path.Combine(ProjectPath, ".."));

            foreach ((string hostPath, string tarPath) in buildContext)
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
                    Tags = [$"{imageName}:latest"],
                    Remove = true,
                    ForceRemove = true,
                    Pull = "always",
                },
                tarMemory,
                null,
                null,
                new Progress<JSONMessage>(message => Console.WriteLine($"BuildImage: {JsonSerializer.Serialize(message)}"))
            );

            await tarMemory.DisposeAsync();
        }

        private async Task DestroyEnvironment()
        {
            await _cancellationTokenSource.CancelAsync();
            foreach (var thread in _threads)
            {
                thread.Join();
            }

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

            try
            {
                await _docker.Images.DeleteImageAsync(FTPServerImage, new ImageDeleteParameters { Force = true });
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
