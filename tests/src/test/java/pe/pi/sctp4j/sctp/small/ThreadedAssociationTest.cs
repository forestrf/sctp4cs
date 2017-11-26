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

using LiteNetLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto.Tls;
using pe.pi.sctp4j.sctp.behave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

/**
*
* @author tim
*/
namespace pe.pi.sctp4j.sctp.small {
	[TestClass]
	public class ThreadedAssociationTest {
		private List<DatagramTransport> __transList;

		[TestInitialize]
		public void setUpClass() {
			__transList = new List<DatagramTransport>();
		}

		[TestCleanup]
		public void tearDown() {
			foreach (DatagramTransport t in __transList) {
				try {
					t.Close();
				} catch (IOException ex) {
					Console.WriteLine(ex.ToString());
				}
			}
			__transList.Clear();
		}

		private DatagramTransport[] mkMockTransports() {
			BlockingQueue<byte[]> left = new BlockingQueue<byte[]>();
			BlockingQueue<byte[]> right = new BlockingQueue<byte[]>();
			DatagramTransport[] ret = new DatagramTransport[2];
			ret[0] = new MockTransport(left, right);
			ret[1] = new MockTransport(right, left);
			__transList.Add(ret[0]);
			__transList.Add(ret[1]);
			return ret;
		}

		class MockAssociationListener : AssociationListener {
			public bool associated = false;
			SCTPStream stream = null;

			public void onAssociated(Association a) {
				lock (this) {
					Console.WriteLine("associated");
					associated = true;
					Monitor.PulseAll(this);
				}
			}

			public void onDisAssociated(Association a) {
				lock (this) {
					Console.WriteLine("dis associated");
					associated = false;
					Monitor.PulseAll(this);
				}
			}

			public virtual void onDCEPStream(SCTPStream s, string label, int type) {
				Console.WriteLine("dcep stream");
			}

			public virtual void onRawStream(SCTPStream s) {
				stream = s;
			}
		}

		class MockTransport : DatagramTransport {
			private BlockingQueue<byte[]> _packetQueueIn;
			private bool _isShutdown;
			private BlockingQueue<byte[]> _packetQueueOut;

			public MockTransport(BlockingQueue<byte[]> _in, BlockingQueue<byte[]> _out) {
				_packetQueueIn = _in;
				_packetQueueOut = _out;
			}

			public int GetReceiveLimit() {
				return 1200;
			}

			public int GetSendLimit() {
				return 1200;
			}

			public int Receive(byte[] buf, int off, int len, int waitMillis) {
				int ret = -1;
				byte[] pkt = null;
				if (!_isShutdown && _packetQueueIn.TryDequeue(out pkt, waitMillis)) {
					Console.WriteLine("Mock recv ");
					Console.WriteLine("Mock recv pkt length =" + pkt.Length);
					Console.WriteLine("Mock recv buff length =" + len);
					if (pkt.Length > len) {
						throw new Exception("[IllegalArgumentException] We should not be here");
					}
					ret = Math.Min(len, pkt.Length);
					Array.Copy(pkt, 0, buf, off, ret);
				} else if (_isShutdown) {
					Console.WriteLine("Transport  shutdown - throw exception.");
					throw new EndOfStreamException("[EOFException] Transport was shutdown.");
				}
				Console.WriteLine("Mock receive returning " + ret);
				return ret;
			}

			public void Send(byte[] bytes, int off, int len) {
				if (len > 0) {
					byte[] stack = new byte[len];
					Array.Copy(bytes, 0, stack, 0, len);
					bool res = _packetQueueOut.Enqueue(stack);
					if (!res) {
						Console.WriteLine("overflowed stack");
					}
				}
			}

			public void Close() {
				if (_isShutdown) {
					Console.WriteLine("Transport  already shutdown - throw exception.");
					throw new EndOfStreamException("[EOFException] transport shut.");
				}
				Console.WriteLine("Transport shutdown.");
				_isShutdown = true;
			}
		}

		abstract class ASCTPStreamListener : SCTPStreamListener {
			public void close(SCTPStream aThis) {
				Console.WriteLine("closed");
			}
			public virtual void onMessage(SCTPStream s, string message) {
				//throw new NotImplementedException();
			}
		}

		[TestMethod]
		public void testTransportLeft() {
			testTransport(0, 1);
		}

		[TestMethod]
		public void testTransportRight() {
			testTransport(1, 0);
		}

		private void testTransport(int l, int r) {
			DatagramTransport[] trans = mkMockTransports();
			string mess = "Test Message";
			trans[l].Send(mess.getBytes(), 0, mess.Length);
			int rl = trans[r].GetReceiveLimit();
			byte[] bytes = new byte[rl];
			int sz = trans[r].Receive(bytes, 0, rl, 100);
			byte[] sb = new byte[sz];
			Array.Copy(bytes, 0, sb, 0, sz);
			string got = sb.getString();
			Assert.AreEqual(mess, got);
		}

		/**
		 * Test of associate method, of class ThreadedAssociation.
		 */
		[TestMethod]
		public void testAssociate() {
			Console.WriteLine("--> associate");

			DatagramTransport[] trans = mkMockTransports();
			MockAssociationListener listenLeft = new MockAssociationListener();
			MockAssociationListener listenRight = new MockAssociationListener();
			ThreadedAssociation instanceLeft = new ThreadedAssociation(trans[0], listenLeft);
			ThreadedAssociation instanceRight = new ThreadedAssociation(trans[1], listenRight);
			instanceLeft.associate();
			lock (listenLeft) {
				Monitor.Wait(listenLeft, 1000);
				Assert.IsTrue(listenLeft.associated);
				Assert.IsTrue(listenRight.associated);
			}
		}

		/**
		 * Test of mkStream method, of class ThreadedAssociation.
		 */
		[TestMethod]
		public void testMkStream() {
			Console.WriteLine("--> mkStream");

			DatagramTransport[] trans = mkMockTransports();
			MockAssociationListener listenLeft = new MockAssociationListener();
			MockAssociationListener listenRight = new MockAssociationListener();
			ThreadedAssociation instanceLeft = new ThreadedAssociation(trans[0], listenLeft);
			ThreadedAssociation instanceRight = new ThreadedAssociation(trans[1], listenRight);
			instanceLeft.associate();
			lock (listenLeft) {
				Monitor.Wait(listenLeft, 1000);
				Assert.IsTrue(listenLeft.associated);
				Assert.IsTrue(listenRight.associated);
			}
			int id = 10;
			SCTPStream result = instanceLeft.mkStream(id);
			Assert.IsTrue(typeof(BlockingSCTPStream).IsAssignableFrom(result.GetType()));
		}

		/**
		 * Test of sendAndBlock method, of class ThreadedAssociation.
		 */
		class ASCTPStreamListenerImpl : ASCTPStreamListener {
			StringBuilder rightout;
			public ASCTPStreamListenerImpl(StringBuilder rightout) {
				this.rightout = rightout;
			}

			public override void onMessage(SCTPStream s, string message) {
				lock (this) {
					Console.WriteLine("onmessage : " + message);
					rightout.Append(message);
					Monitor.Pulse(this);
				}
			}
		}

		class MockAssociationListenerImpl : MockAssociationListener {
			SCTPStreamListener rsl;
			public MockAssociationListenerImpl(SCTPStreamListener rsl) {
				this.rsl = rsl;
			}

			public override void onRawStream(SCTPStream s) {
				base.onRawStream(s);
				s.setBehave(new OrderedStreamBehaviour());
				s.setSCTPStreamListener(rsl);
			}
		}

		[TestMethod]
		public void testSendAndBlock() {
			Console.WriteLine("--> sendAndBlock");
			StringBuilder rightout = new StringBuilder();
			SCTPStreamListener rsl = new ASCTPStreamListenerImpl(rightout);
			DatagramTransport[] trans = mkMockTransports();
			MockAssociationListener listenLeft = new MockAssociationListener();
			MockAssociationListener listenRight = new MockAssociationListenerImpl(rsl);
			ThreadedAssociation instanceLeft = new ThreadedAssociation(trans[0], listenLeft);
			ThreadedAssociation instanceRight = new ThreadedAssociation(trans[1], listenRight);
			instanceLeft.associate();
			lock (listenLeft) {
				Monitor.Wait(listenLeft, 1000);
				Assert.IsTrue(listenLeft.associated);
				Assert.IsTrue(listenRight.associated);
			}
			int id = 10;
			SCTPStream result = instanceLeft.mkStream(id);
			Assert.IsTrue(typeof(BlockingSCTPStream).IsAssignableFrom(result.GetType()));
			string test = "Test message";
			SCTPMessage m = new SCTPMessage(test, result);
			instanceLeft.sendAndBlock(m);
			lock (rightout) {
				Monitor.Wait(rightout, 1000);
				Assert.AreEqual(rightout.ToString(), test);
			}
		}

		/**
		 * Test of makeMessage method, of class ThreadedAssociation.
		 */
		class SCTPByteStreamListenerImpl : SCTPByteStreamListener {
			StringBuilder empty;
			ByteBuffer rightout;
			public SCTPByteStreamListenerImpl(StringBuilder empty, ByteBuffer rightout) {
				this.empty = empty;
				this.rightout = rightout;
			}

			public void onMessage(SCTPStream s, string message) {
				empty.Append(message);
				Console.WriteLine("string onmessage : " + message);
				lock (rightout) {
					Monitor.Pulse(rightout);
				}
			}

			public void onMessage(SCTPStream s, byte[] message) {
				rightout.Put(message);
				Console.WriteLine("Byte onmessage : " + message.Length);
				lock (rightout) {
					Monitor.Pulse(rightout);
				}
			}

			public void close(SCTPStream aThis) {
				Console.WriteLine("closed");
			}
		}
		[TestMethod]
		public void testMakeMessage_byteArr_BlockingSCTPStream() {
			Console.WriteLine("---->makeMessage bytes");

			ByteBuffer rightout = new ByteBuffer(new byte[10000]);
			StringBuilder empty = new StringBuilder();
			SCTPByteStreamListener rsl = new SCTPByteStreamListenerImpl(empty, rightout);
			DatagramTransport[] trans = mkMockTransports();
			MockAssociationListener listenLeft = new MockAssociationListener();
			MockAssociationListener listenRight = new MockAssociationListenerImpl(rsl);
			ThreadedAssociation instanceLeft = new ThreadedAssociation(trans[0], listenLeft);
			ThreadedAssociation instanceRight = new ThreadedAssociation(trans[1], listenRight);
			instanceLeft.associate();
			lock (listenLeft) {
				Monitor.Wait(listenLeft, 2000);
				Assert.IsTrue(listenLeft.associated);
				Assert.IsTrue(listenRight.associated);
			}
			int id = 10;
			SCTPStream s = instanceLeft.mkStream(id);
			Assert.IsTrue(typeof(BlockingSCTPStream).IsAssignableFrom(s.GetType()));
			string test = "Test message";
			SCTPMessage m = instanceLeft.makeMessage(test.getBytes(), (BlockingSCTPStream) s);
			instanceLeft.sendAndBlock(m);
			lock (rightout) {
				Monitor.Wait(rightout, 2000);
				int l = rightout.Position;
				string res = rightout.Data.getString(0, l);
				Assert.AreEqual(res, test);
				Assert.AreEqual(empty.Length, 0);
			}
		}

		/**
		 * Test of makeMessage method, of class ThreadedAssociation.
		 */
		[TestMethod]
		public void testMakeMessage_string_BlockingSCTPStream() {
			Console.WriteLine("---->makeMessage string");

			StringBuilder rightout = new StringBuilder();
			SCTPStreamListener rsl = new ASCTPStreamListenerImpl(rightout);
			DatagramTransport[] trans = mkMockTransports();
			MockAssociationListener listenLeft = new MockAssociationListener();
			MockAssociationListener listenRight = new MockAssociationListenerImpl(rsl);
			ThreadedAssociation instanceLeft = new ThreadedAssociation(trans[0], listenLeft);
			ThreadedAssociation instanceRight = new ThreadedAssociation(trans[1], listenRight);
			instanceLeft.associate();
			lock (listenLeft) {
				Monitor.Wait(listenLeft, 2000);
				Assert.IsTrue(listenLeft.associated);
				Assert.IsTrue(listenRight.associated);
			}
			int id = 10;
			SCTPStream s = instanceLeft.mkStream(id);
			Assert.IsTrue(typeof(BlockingSCTPStream).IsAssignableFrom(s.GetType()));
			string test = "Test message";
			SCTPMessage m = instanceLeft.makeMessage(test, (BlockingSCTPStream) s);
			instanceLeft.sendAndBlock(m);
			lock (rightout) {
				Monitor.Wait(rightout, 2000);
				Assert.AreEqual(rightout.ToString(), test);
			}
		}

		/**
		 * Test of makeMessage method, of class ThreadedAssociation.
		 */
		class MockAssociationListenerImpl2 : MockAssociationListener {
			public override void onDCEPStream(SCTPStream s, string label, int type) {
			}
		}
		class MockAssociationListenerImpl3 : MockAssociationListener {
			StringBuilder rightLabel;
			SCTPStreamListener rsl;

			public MockAssociationListenerImpl3(StringBuilder rightLabel, SCTPStreamListener rsl) {
				this.rightLabel = rightLabel;
				this.rsl = rsl;
			}

			public override void onDCEPStream(SCTPStream s, string label, int type) {
				s.setSCTPStreamListener(rsl);
				rightLabel.Append(label);
			}
		}
		[TestMethod]
		public void testDCEPStreamSend() {
			Console.WriteLine("---->testDCEPStreamSend string");

			StringBuilder rightout = new StringBuilder();
			StringBuilder rightLabel = new StringBuilder();
			SCTPStreamListener rsl = new ASCTPStreamListenerImpl(rightout);
			DatagramTransport[] trans = mkMockTransports();
			MockAssociationListener listenLeft = new MockAssociationListenerImpl2();
			MockAssociationListener listenRight = new MockAssociationListenerImpl3(rightLabel, rsl);
			ThreadedAssociation instanceLeft = new ThreadedAssociation(trans[0], listenLeft);
			ThreadedAssociation instanceRight = new ThreadedAssociation(trans[1], listenRight);
			instanceLeft.associate();
			lock (listenLeft) {
				Monitor.Wait(listenLeft, 2000);
				Assert.IsTrue(listenLeft.associated);
				Assert.IsTrue(listenRight.associated);
			}
			int id = 10;
			string label = "test Stream";
			SCTPStream s = instanceLeft.mkStream(id, label);
			lock (rightLabel) {
				Monitor.Wait(rightLabel, 2000);
				Assert.AreEqual(rightLabel.ToString(), label);
			}
			Assert.IsTrue(typeof(BlockingSCTPStream).IsAssignableFrom(s.GetType()));
			BlockingSCTPStream bs = (BlockingSCTPStream) s;
			StringBuilder longTestMessage = new StringBuilder();
			for (int i = 0; i < 10000; i++) {
				longTestMessage.Append(" " + i);
			}
			string teststring = longTestMessage.ToString();
			Console.WriteLine("-------> string length = " + teststring.Length);
			Thread st = new Thread(() => {
				try {
					s.send(teststring);
				} catch (Exception ex) {
					Console.WriteLine(ex.ToString());
				}
			});
			st.Name = "sender ";
			st.Start();

			lock (rightout) {
				Monitor.Wait(rightout, 10000);
				Assert.AreEqual(teststring, rightout.ToString());
			}
		}
	}
}
