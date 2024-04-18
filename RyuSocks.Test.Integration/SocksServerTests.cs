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

using FluentFTP;
using FluentFTP.Proxy.SyncProxy;
using System;
using System.IO;
using System.Net;
using Xunit;

namespace RyuSocks.Test.Integration
{
    public class SocksServerTests : IClassFixture<FTPServerEnvironment>, IDisposable
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly FTPServerEnvironment _fixture;
        private readonly FtpClientSocks5Proxy _ftp;

        public SocksServerTests(FTPServerEnvironment fixture)
        {
            _fixture = fixture;
            Console.WriteLine($"Using ProxyHost: {_fixture.RyuSocksIPAddress}");
            _ftp = new FtpClientSocks5Proxy(new FtpProxyProfile
            {
                FtpCredentials = new NetworkCredential(FTPServerEnvironment.FTPUsername, FTPServerEnvironment.FTPPassword),
                FtpHost = _fixture.FTPServerIPAddress,
                ProxyHost = _fixture.RyuSocksIPAddress,
                ProxyPort = 1080,
            });
            _ftp.Config.LogToConsole = true;
            _ftp.Config.ConnectTimeout = 20 * 1000;
            _ftp.Config.ReadTimeout = 20 * 1000;
        }

        [Fact]
        public void FTPClient_CanConnect()
        {
            _ftp.Connect();
        }

        [Fact]
        public void FTPClient_CanListFiles()
        {
            _ftp.Connect();

            var items = _ftp.GetListing("assets");

            Assert.Equal(items.Length, FTPServerEnvironment.FTPServerFiles.GetFiles().Length);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(1, false)]
        public void FTPClient_CanDownloadFile(int fileIndex, bool isPassiveMode)
        {
            var file = FTPServerEnvironment.FTPServerFiles.GetFiles()[fileIndex];
            var downloadDirectory = Directory.CreateTempSubdirectory("Ryujinx.Test.Integration.FTPClient");
            var downloadPath = Path.Combine(downloadDirectory.FullName, file.Name);

            _ftp.Config.DataConnectionType = isPassiveMode ? FtpDataConnectionType.AutoPassive : FtpDataConnectionType.AutoActive;
            _ftp.Connect();

            var result = _ftp.DownloadFile(downloadPath, $"assets/{file.Name}");

            Assert.Equal(FtpStatus.Success, result);
            Assert.Equal(File.ReadAllBytes(downloadPath), File.ReadAllBytes(file.FullName));

            downloadDirectory.Delete(true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FTPClient_CanUploadFile(bool isPassiveMode)
        {
            string fileName = isPassiveMode ? "upload_passive.bin" : "upload_active.bin";
            byte[] fileBytes = new byte[1024];
            Random.Shared.NextBytes(fileBytes);

            _ftp.Config.DataConnectionType = isPassiveMode ? FtpDataConnectionType.AutoPassive : FtpDataConnectionType.AutoActive;
            _ftp.Connect();

            var result = _ftp.UploadBytes(fileBytes, $"upload/{fileName}");

            Assert.Equal(FtpStatus.Success, result);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ftp?.Dispose();
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}
