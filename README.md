SCTP4CS
====
Ported by ASH KATCHAP from [pipe/sctp4j](https://github.com/pipe/sctp4j)

This is a pure C# implementation of the SCTP protocol.
The target usecase is small devices that want to run the webRTC datachannel.

This implementation does not include all the necessary parts for a full 
webRTC stack. You'll need DTLS (For example, [Bouncy Castle](https://github.com/bcgit/bc-csharp)) and ICE/STUN/TURN (For example, [SIPSorcery](https://github.com/sipsorcery/sipsorcery)).

This implementation assumes that datagrams will arrive from an DTLS/ICE stack
which implements a Datagram Transport. It also assumes a consumer of open SCTP Streams (or datachannels) - it is pure middleware.

The stack tries to keep the details of concurrency in a single package, so that the current pure thread model could be replaced with Akka actors or NIO-like async mechanisms.

This port was made to implement WebRTC with datachannels in another library that will link this one.

Some tests files in the tests project are under GPLv2.
