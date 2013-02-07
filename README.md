AvisNet
=======

.Net (C#) Port of the Java Avis high-performance event router (see http://avis.sourceforge.net/index.html, description taken from there)


About
=====
Avis is a high-performance event router. It provides a fast publish/subscribe event routing service compatible with the commercial Elvin implementation developed by Mantara Software.

Features
========
* Fast broadcast message delivery. No requirement to support transactions or persistence, plus concise message selection allows near real-time delivery.
* Flexible message format. Messages are just name-value pairs.
* Content-based subscription. Select messages using subscription expressions like From == 'logger' && Severity > 3 or (string (Message) && Timeout > 0) || regex (Message, 'News:.*').
* Federation of multiple message routers. Replicate selected messages between any configuration of routers to form local and wide-area message networks.
* Security. Supports per-subscription access control and SSL/TLS client/server authentication and encryption.

Currently only the client has been ported to .Net 4.5. The router implementation will follow.
SSL/TLS and secured transmission are supported.
