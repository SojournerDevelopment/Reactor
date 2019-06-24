![](https://raw.githubusercontent.com/SojournerDevelopment/Reactor/master/Doc/reactor.svg?sanitize=true)

### Secure Server/Client Framework

Reactor is a framework designed to enable the development of server and client applications that rely on reliability, performance and security.

Reactor does not require any third party libraries and can easily be integrated in your projects. ReactorSecure provides you with bleeding edge security features.

All your communication will be encrypted and use the ReactorProtocol (You can read more about it in the specifications.md).

Reactor can also be used for IPC.

The latest implementation allows TCP Server/Clients to be developed using the JSON Mime Format. For example, a Python script can be used as a client while Reactor runs as a .net server on the other side.
Reactor *next-gen* Version 1.0 is completely platform independent meaning you can develop a server / client for it in nearly any programming language.

Check out the many different examples included within the repository, starting with a simple console based server, over to a easy to implement Remote Desktop application and more. It has never been easier to create an secure tcp/ip socket server and client.

Important to note are the specifications for packet exchange and encoding, since UFT-16 Little Endian is transmitted in the encoding.

**Reactor 0.1** is used for **Eos:** https://github.com/sojournercntl/Eos
