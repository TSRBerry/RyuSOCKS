#pragma endian big
#include <type/ip.pat>

enum Method : u8 {
  NoAuthenticationRequired,
  GSSAPI,
  UsernameAndPassword,
  NoAcceptableMethod = 0xFF,
};

enum Command : u8 {
    Connect = 0x01,
    Bind,
    UdpAssociate,
};

enum AddressType : u8 {
    Ipv4Address = 0x01,
    DomainName = 0x03,
    Ipv6Address,
};

enum ReplyField : u8 {
    Succeeded,
    GeneralSOCKSServerFailure,
    ConnectionNotAllowed,
    NetworkUnreachable,
    HostUnreachable,
    ConnectionRefused,
    TTLExpired,
    CommandNotSupported,
    AddressTypeNotSupported,
};

struct DomainName {
    u8 length;
    char domainName[length];
};

struct MethodSelectionRequest {
    u8 version;
    u8 nmethods;
    Method methods[nmethods];
};

struct MethodSelectionResponse {
    u8 version;
    Method method;
};


struct CommandRequest {
    u8 version;
    Command cmd;
    padding[1];
    AddressType addressType;
    match (addressType) {
        (AddressType::Ipv4Address): type::IPv4Address destinationAddress;
        (AddressType::DomainName): DomainName destinationDomainName;
        (AddressType::Ipv6Address): type::IPv6Address destinationAddress;
    }
    u16 destinationPort;
};

struct CommandResponse {
    u8 version;
    ReplyField reply;
    padding[1];
    AddressType addressType;
    match (addressType) {
        (AddressType::Ipv4Address): type::IPv4Address boundAddress;
        (AddressType::DomainName): DomainName boundDomainName;
        (AddressType::Ipv6Address): type::IPv6Address boundAddress;
    }
    u16 boundPort;
};

struct UdpRequestHeader {
    padding[2];
    u8 fragment;
    AddressType addressType;
    match (addressType) {
        (AddressType::Ipv4Address): type::IPv4Address destinationAddress;
        (AddressType::DomainName): DomainName destinationDomainName;
        (AddressType::Ipv6Address): type::IPv6Address destinationAddress;
    }
    u16 destinationPort;
    u8 data[];
};