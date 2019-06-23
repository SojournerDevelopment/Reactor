# Packet Specification

### Registration

Sender: **Server**

This packet contains the client identification assigned by the server, the server identification and the server public RSA key.

```json
{
  "reactor": {
    "type": "REG",
    "sender": "SENDER-ID",
    "receiver": "RECEIVER-ID",
    "key": "RSA PUBLIC KEY BASE 64"
  }
}
```

### Authentication

