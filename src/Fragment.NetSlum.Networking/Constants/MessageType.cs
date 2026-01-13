namespace Fragment.NetSlum.Networking.Constants;

public enum MessageType
{
    None = 0x00,
    PingRequest = 0x02,

    Data = 0x30,
    KeyExchangeRequest = 0x34,
    KeyExchangeResponse = 0x35,
    KeyExchangeAcknowledgmentRequest = 0x36,
    
    // Enterprise / Extended MPS message types (reserved/added for Enterprise features)
    EnterpriseData = 0x40,
    RelayForward = 0x41,
    HostRegistration = 0x42,
    HostAssignment = 0x43,
    LobbyLoginRequest = 0x44,
    LobbyLoginResponse = 0x45,
    ServerListUpdate = 0x46,
    SystemControllerCommand = 0x47,
    GameServerTransfer = 0x48,
    EnterprisePing = 0x49,

    // Reserved range for future enterprise messages
    EnterpriseReservedStart = 0x50,
    EnterpriseReservedEnd = 0x5F,
}
