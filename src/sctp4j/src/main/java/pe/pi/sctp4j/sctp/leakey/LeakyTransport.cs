/*
 * Copyright 2017 pi.pe gmbh .
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
// Modified by Andrés Leone Gámez

using SCTP4CS;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Net;
using System.Net.Sockets;

/**
 *
 * @author Westhawk Ltd<thp@westhawk.co.uk>
 *
 * class that taps off the DTLS decoded SCTP traffic and echoes it to a pair of
 * UDP ports so they can be pcaped by wireshark Do not use this class in
 * production code. test/debug only!
 *
 */
namespace pe.pi.sctp4j.sctp.leakey {
	class LeakyTransport : DatagramTransport {
		DatagramTransport _dtls;
		Socket _logrec;
		Socket _logsend;
		static short SCTP = 9899;
		IPEndPoint remoteEndPoint_logrec;
		IPEndPoint remoteEndPoint_logsend;

		public LeakyTransport(DatagramTransport transport) {
			try {
				_dtls = transport;
				IPAddress me = IPAddress.Parse("127.0.0.1");
				_logrec = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				remoteEndPoint_logrec = new IPEndPoint(me, SCTP + 1);
				_logsend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				remoteEndPoint_logsend = new IPEndPoint(me, SCTP);
				/*
				InetSocketAddress s = (InetSocketAddress) _logsend.getLocalSocketAddress();
				Log.logger.warn("Leaking to send address " + s.getHoststring() + ":" + s.getPort());
				InetSocketAddress r = (InetSocketAddress) _logrec.getLocalSocketAddress();
				Log.logger.warn("Leaking to recv address " + r.getHoststring() + ":" + r.getPort());
				*/
			} catch (Exception ex) {
				Logger.logger.Error("exception in making Leaky socket");
			}
		}
		
		public int GetReceiveLimit() {
			return _dtls.GetReceiveLimit();
		}
		
		public int GetSendLimit() {
			return _dtls.GetSendLimit();
		}
		
		public int Receive(byte[] bytes, int offs, int len, int sleep) {
			int sz = _dtls.Receive(bytes, offs, len, sleep);
			if (sz > 0) {
				_logrec.SendTo(bytes, offs, sz, SocketFlags.None, remoteEndPoint_logrec);
			}
			return sz;
		}
		
		public void Send(byte[] bytes, int offs, int len) {
			if ((bytes == null) || (bytes.Length < offs + len) || (bytes.Length < 1)) {
				Logger.logger.Error("Implausible packet for encryption ");
				if (bytes == null) {
					Logger.logger.Error("null buffer");
				} else {
					Logger.logger.Error("Length =" + bytes.Length + " len =" + len + " offs=" + offs);
				}
				return;
			}
			try {
				_logsend.SendTo(bytes, offs, len, SocketFlags.None, remoteEndPoint_logsend);
			} catch (Exception x) {
				Logger.logger.Error("can't leak to " + remoteEndPoint_logsend);
				Logger.logger.Error(x.StackTrace);
			}
			_dtls.Send(bytes, offs, len);
		}
		
		public void Close() {
			_dtls.Close();
			_logrec.Close();
			_logsend.Close();
		}
	}
}
