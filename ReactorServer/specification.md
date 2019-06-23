# Packet Specification

The following specifications apply to the reactor framework SecureServer and SecureClient.

### Registration

Sender: **Server**

This packet contains the client identification assigned by the server, the server identification and the server public RSA key.

```json
{
  "reactor": {
    "type": "REG",
    "sender": "SERVER-ID",
    "receiver": "CLIENT-ID",
    "key": "RSA PUBLIC KEY BASE 64"
  }
}
```

### Authentication

Sender: **Client**

The client generates a session-key and encrypts its `base64()` content with the public key of the server.

```json
{
  "reactor": {
    "type": "AUTH",
    "sender": "CLIENT-ID",
    "receiver": "SERVER-ID",
    "session":"base64(EncryptedKey)"
  }
} 
```

### Fusion

Sender: **Server**

If the session key is successfully decrypted, the server sends the fusion packet. This packet is now encrypted symmetrically.

```json
{
  "reactor": {
    "type": "FUSION",
    "sender": "SERVER-ID",
    "receiver": "CLIENT-ID",
    "core": "base64(end-key)",
    "core-fin": "base64(end-key-client)"
  }
}
```

### Request Melt

Sender: **Client**

The request melt packet is sent by the client and contains the shutdown key the server gave the client in the Fusion packet.

```json
{
  "reactor": {
    "type": "MELT",
    "sender": "CLIENT-ID",
    "receiver": "SERVER-ID",
    "control": "base64(end-key)"
  }
}
```


### Melt

Sender: **Server**

The request tells the client to release all required ressources, as the server now closes the connection and the client will be disposed.

```json
{
  "reactor": {
    "type": "MELT",
    "sender": "SERVER-ID",
    "receiver": "CLIENT-ID",
    "control": "base64(end-key-client)"
  }
}
```

## Data Flow

The normal data transfer after the fusion request is made through the following packets, which provide a base for send more complex data (eg. compressed images)

### Data

Sender:  **Server/Client**

Default data packet that should only be received, after the fusion packet was sent (server) and received (client).

```json
{
  "reactor": {
    "type": "DATA"
    "sender": "ID",
    "receiver": "ID",
    "packet": "base64(ENCRYPTED(DATA))"
  }
}
```

*Attention:* The data in the `packet` attribute must be converted to byte and the decrypted, to get the real content.