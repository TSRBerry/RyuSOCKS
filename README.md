![RyuSOCKS](assets/logo.png)

RyuSOCKS is a *work-in-progress* extensible SOCKS5 library, providing server and client implementations which are **almost** compliant with [RFC1928](https://datatracker.ietf.org/doc/html/rfc1928).

The extensibility of RyuSOCKS mainly focuses on two parts of the protocol: Authentication and commands.

This means in addition to the standard SOCKS5 commands (`CONNECT`, `BIND` and `UDP ASSOCIATE`), custom commands can also be added.
Similarly, custom implementations for authentication methods are supported as well.

## LICENSE

This software library is licensed under the terms of the GPLv2, with exemptions for specific projects noted below.

You can find a copy of the license in the [LICENSE file](LICENSE).

Exemptions:

- The [Ryujinx Team and Contributors](https://github.com/orgs/Ryujinx) are exempt from GPLv2 licensing.
  They are permitted, at their discretion, to instead license any source code authored for the RyuSOCKS project as either GPLv2 or later or the [MIT license](docs/licensing/MIT-LICENSE).
  In doing so, they may alter, supplement, or entirely remove the copyright notice for each file they choose to relicense.
  Neither the RyuSOCKS project nor its individual contributors shall assert their moral rights against the aforementioned project.

## Credits

I want to thank the following people for their help and/or other involvement with this project:

- [Justin de Haas](https://onemuri.nl/) for creating the RyuSOCKS logo.
