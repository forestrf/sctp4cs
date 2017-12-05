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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto.Tls;
using pe.pi.sctp4j.sctp.messages;
using pe.pi.sctp4j.sctp.small;
using SCTP4CS.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

/**
*
* @author tim
*/
namespace pe.pi.sctp4j.sctp {
	[TestClass]
	public class SCTPMessageTest {

		private Random _rand = new Random(1); // deterministic non crypto quality random for repeatable tests
		private SCTPStream _fakeStream;

		public SCTPMessageTest() {
		}

		Association _fakeAssociation;

		class DatagramTransportImpl : DatagramTransport {
			public int GetReceiveLimit() {
				return 1200;
			}

			public int GetSendLimit() {
				return 1200;
			}

			public int Receive(byte[] bytes, int i, int i1, int waitMs) {
				try {
					Thread.Sleep(waitMs);
				}
				catch (Exception x) {
				}
				throw new Exception("[InterruptedIOException] empty");
			}

			public void Send(byte[] bytes, int i, int i1) { }
			public void Close() { }
		}
		class AssociationImpl : Association {
			public AssociationImpl(DatagramTransport transport, AssociationListener al) : base(transport, al) {
			}

			public AssociationImpl(DatagramTransport transport, AssociationListener al, bool client) : base(transport, al, client) {
			}

			public override void associate() {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}

			internal override void enqueue(DataChunk d) {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}

			public override SCTPStream mkStream(int id) {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}

			internal override void sendAndBlock(SCTPMessage m) {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}

			internal override SCTPMessage makeMessage(byte[] bytes, int offset, int length, BlockingSCTPStream aThis) {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}

			internal override SCTPMessage makeMessage(string s, BlockingSCTPStream aThis) {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}

			internal override Chunk[] sackDeal(SackChunk sackChunk) {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}
		}
		class SCTPStreamImpl : SCTPStream {
			public SCTPStreamImpl(Association a, int id) : base(a, id) {
			}

			public override void send(string message) {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}

			internal override void deliverMessage(SCTPMessage message) {
				message.run();
			}

			public override void send(byte[] message, int offset, int length) {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}

			internal override void delivered(DataChunk d) {
				throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
			}
		}

		[TestInitialize]
		public void setUp() {
			DatagramTransport _fakedt = new DatagramTransportImpl();
			_fakeAssociation = new AssociationImpl(_fakedt, null);
			_fakeStream = new SCTPStreamImpl(_fakeAssociation, 22);
		}

		/**
		 * Test of setCompleteHandler method, of class SCTPMessage.
		 */
		/*
		 @Test
		 public void testSetCompleteHandler() {
		 Console.WriteLine("setCompleteHandler");
		 MessageCompleteHandler mch = null;
		 SCTPMessage instance = null;
		 instance.setCompleteHandler(mch);
		 // TODO review the generated test code and remove the default call to fail.
		 fail("The test case is a prototype.");
		 }
		 */
		/**
		 * Test of hasMoreData method, of class SCTPMessage.
		 */
		/*
		 @Test
		 public void testHasMoreData() {

		 }
		 */
		/**
		 * Test of fill method, of class SCTPMessage.
		 */
		[TestMethod]
		public void testFillShortstring() {
			Console.WriteLine("--> fill short string ");
			string teststring = "This is a short test";
			SCTPMessage instance = new SCTPMessage(teststring, _fakeStream);
			SortedArray<DataChunk> chunks = new SortedArray<DataChunk>();
			while (instance.hasMoreData()) {
				DataChunk dc = new DataChunk();
				instance.fill(dc);
				chunks.Add(dc);
			}
			Assert.AreEqual(chunks.Count, 1);
			var ppid = ((DataChunk) chunks.First).ppid;
			Assert.AreEqual(ppid, SCTP_PPID.WEBRTCstring);
		}
		[TestMethod]
		public void testFillShortBlob() {
			Console.WriteLine("--> fill short blob ");
			byte[] testBlob = new byte[21];
			_rand.NextBytes(testBlob);
			SCTPMessage instance = new SCTPMessage(testBlob, 0, testBlob.Length, _fakeStream);
			SortedArray<DataChunk> chunks = new SortedArray<DataChunk>();
			while (instance.hasMoreData()) {
				DataChunk dc = new DataChunk();
				instance.fill(dc);
				chunks.Add(dc);
			}
			Assert.AreEqual(chunks.Count, 1);
			var ppid = ((DataChunk) chunks.First).ppid;
			Assert.AreEqual(ppid, SCTP_PPID.WEBRTCBINARY);
		}
		[TestMethod]
		public void testFillLongstring() {
			Console.WriteLine("--> fill long");
			StringBuilder sb = new StringBuilder("This is a");
			for (int i = 0; i < 1030; i++) {
				sb.Append(" long");
			}
			sb.Append(" test.");
			string teststring = sb.ToString();
			SCTPMessage instance = new SCTPMessage(teststring, _fakeStream);
			SortedArray<DataChunk> chunks = new SortedArray<DataChunk>();
			long tsn = 111;

			while (instance.hasMoreData()) {
				DataChunk dc = new DataChunk();
				dc.tsn = (uint) tsn++;
				instance.fill(dc);
				chunks.Add(dc);
			}
			double pktsz = chunks.First.getDataSize();
			int estimate = (int) Math.Ceiling(teststring.Length / pktsz);
			Assert.AreEqual(chunks.Count, estimate);
		}

		[TestMethod]
		public void testEmptystring() {
			Console.WriteLine("--> fill empty string");
			StringBuilder sb = new StringBuilder("");
			string teststring = sb.ToString();
			SCTPMessage instance = new SCTPMessage(teststring, _fakeStream);
			SortedArray<DataChunk> chunks = new SortedArray<DataChunk>();
			long tsn = 111;

			while (instance.hasMoreData()) {
				DataChunk dc = new DataChunk();
				dc.tsn = (uint) tsn++;
				instance.fill(dc);
				chunks.Add(dc);
			}
			int pktsz = chunks.First.getDataSize();
			Assert.AreEqual(chunks.Count, 1);
			Assert.AreEqual(pktsz, 1);
			var ppid = ((DataChunk) chunks.First).ppid;
			Assert.AreEqual(ppid, SCTP_PPID.WEBRTCstringEMPTY);
		}

		[TestMethod]
		public void testEmptyBlob() {
			Console.WriteLine("--> fill empty blob");
			byte[] testBlob = new byte[0];
			SCTPMessage instance = new SCTPMessage(testBlob, 0, testBlob.Length, _fakeStream);
			SortedArray<DataChunk> chunks = new SortedArray<DataChunk>();
			long tsn = 111;

			while (instance.hasMoreData()) {
				DataChunk dc = new DataChunk();
				dc.tsn = (uint) tsn++;
				instance.fill(dc);
				chunks.Add(dc);
			}
			Assert.AreEqual(chunks.Count, 1);
			int pktsz = chunks.First.getDataSize();
			Assert.AreEqual(pktsz, 1);
			var ppid = ((DataChunk) chunks.First).ppid;
			Assert.AreEqual(ppid, SCTP_PPID.WEBRTCBINARYEMPTY);
		}

		/**
		 * Test of getData method, of class SCTPMessage.
		 */
		/*
		 @Test
		 public void testGetData() {
		 Console.WriteLine("getData");
		 SCTPMessage instance = null;
		 byte[] expResult = null;
		 byte[] result = instance.getData();
		 assertArrayEquals(expResult, result);
		 // TODO review the generated test code and remove the default call to fail.
		 fail("The test case is a prototype.");
		 }*/
	}
}
