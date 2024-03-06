# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

[PR #2](https://github.com/TSRBerry/RyuSOCKS/pull/2): implemented Username and Password authentication according to [RFC 1929](https://datatracker.ietf.org/doc/html/rfc1929)\
Added UsernameAndPasswordRequest and UsernameAndPasswordResponse, aswell as the actual Authentication in UsernameAndPassword.cs

## [Unreleased]

### Added

- Username and password authentication according to RFC1929 ([#2](https://github.com/TSRBerry/RyuSOCKS/pull/2))

### Added

- A logo made by [Justin de Haas](https://onemuri.nl/)

## [0.1.0-alpha] - 2024-04-21

### Added

- Basic SOCKS5 proxy server and client (not compliant yet)
- Connect command
- Bind command
- UdpAssociate command without fragmentation
- Configurable authentication methods
- Configurable SOCKS commands
- Configurable allow and block lists
- Small unit test suite
- Minimal integration test suite
