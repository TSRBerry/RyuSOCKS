# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Logo made by [Justin de Haas](https://onemuri.nl/).
- Basic SOCKS5 proxy client (not compliant yet).
- New generated `IProxyAuth.GetAuth()` extension method
  to get the `AuthMethod` value from an auth implementation.
- `Packet` constructor which takes a byte array.
- New method `Packet.AsSpan()` to get the underlying byte array of a packet
  as a `Span<byte>`.
- Constructors taking a `ProxyEndpoint` for `EndpointPackets`.
- `Equals()` and `GetHashCode()` implementation for `ProxyEndpoint`.
- New interface `IWrapper` which contains `Wrap()` and `Unwrap()`.
- New properties for `Command` implementations to control communication
  with the proxy server.
- Username and password authentication method
  according to [RFC 1929](https://datatracker.ietf.org/doc/html/rfc1929) ([#16](https://github.com/TSRBerry/RyuSOCKS/pull/16))

### Changed

- Improved introduction in `README.md`.
- Renamed `Destination` class to `ProxyEndpoint`.
- `SocksSession.Process*()` methods were marked as virtual.
- Improved exception messages.
- Throw `ArgumentOutOfRangeException` instead of `ArgumentException`
  if the length of the domain name is invalid for `EndpointPackets`.

### Fixed

- Misbehaving UdpAssociate command.

## [0.1.0-alpha] - 2024-04-21

### Added

- Basic SOCKS5 proxy server (not compliant yet).
- Connect command.
- Bind command.
- UdpAssociate command without fragmentation.
- Configurable authentication methods.
- Configurable SOCKS commands.
- Configurable allow and block lists.

[Unreleased]: https://github.com/TSRBerry/RyuSOCKS/compare/v0.1.0-alpha...HEAD
[0.1.0-alpha]: https://github.com/TSRBerry/RyuSOCKS/releases/tag/v0.1.0-alpha
