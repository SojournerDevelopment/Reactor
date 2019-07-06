![](https://raw.githubusercontent.com/SojournerDevelopment/Reactor/master/Doc/reactor.svg?sanitize=true)

### Secure Server/Client Framework

> **Warning:** Although this version of Reactor is based on modern infrastructure and processes data in real time, it is not designed for ultra high performance applications with thousands of clients.

> **Reactor High Performance Edition**: A Version of the Reactor Framework, which allows you to create asynchronous socket event based high performance server/clients. Handle up to thousands of clients. [Sojourner Development](https://sojourner.dev)


Reactor is a framework designed to enable the development of server and client applications that rely on reliability, performance and security.

Reactor can also be used for IPC.

**Attention:** Reactor is a framework that is at a very early stage of development. In the Development Roadmap you can find out more about future and planned features.

The latest implementation allows TCP Server/Clients to be developed using the JSON Mime Format. For example, a Python script can be used as a client while Reactor runs as a .net server on the other side.

Check out the SimpleJsonServer/Client Projects in the Example folder.

Important to note are the specifications for packet exchange and encoding, since UFT-16 Little Endian is transmitted in the encoding.

Reactor is used for Eos: https://github.com/sojournercntl/Eos

**PLEASE VIEW THE EXPERIMENTAL BRANCH FOR COMPATIBILITY SUPPORT**
