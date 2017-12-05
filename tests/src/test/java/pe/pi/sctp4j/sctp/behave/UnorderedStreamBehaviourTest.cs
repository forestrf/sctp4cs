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
using pe.pi.sctp4j.sctp.messages;
using SCTP4CS.Utils;
using System;
using System.Collections.Generic;
using System.Text;

/**
*
* @author tim
*/
namespace pe.pi.sctp4j.sctp.behave {
	[TestClass]
	public class UnorderedStreamBehaviourTest {

		private long _tsn = 111;

		public UnorderedStreamBehaviourTest() {
		}

		class MockStreamImpl : SCTPStream {
			public MockStreamImpl(Association a, int id) : base(a, id) {
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
		SCTPStream mockStream() {
			Association a = null;
			int n = 10;
			return new MockStreamImpl(a, n);
		}
		/**
		 * Test of respond method, of class UnreliableStreamBehaviour.
		 */
		/*[TestMethod]
		 public void testRespond() {
		 Console.WriteLine("respond");
		 SCTPStream a = null;
		 UnreliableStreamBehaviour instance = new UnreliableStreamBehaviour();
		 Chunk[] expResult = null;
		 Chunk[] result = instance.respond(a);
		 assertArrayEquals(expResult, result);
		 // TODO review the generated test code and remove the default call to fail.
		 fail("The test case is a prototype.");
		 }*/
		/**
		 * Test of deliver method, of class UnreliableStreamBehaviour.
		 */
		class CheckingStreamListener : SCTPStreamListener {
			List<string> _results;

			public CheckingStreamListener(List<string> results) {
				_results = results;
			}

			public void onMessage(SCTPStream s, string message) {
				Assert.IsTrue(_results.Contains(message));
				_results.Remove(message);
			}

			public void close(SCTPStream aThis) {
			}
		};

		[TestMethod]
		public void testDeliverSingle() {
			Console.WriteLine("--> deliver single");
			SCTPStream s = mockStream();
			SortedArray<DataChunk> stash = new SortedArray<DataChunk>();
			DataChunk single = new DataChunk();
			string teststring = "Test string";
			single.setData(teststring.getBytes());
			single.ppid = SCTP_PPID.WEBRTCstring;
			single.setFlags(DataChunk.SINGLEFLAG);
			single.tsn = (uint) _tsn++;
			single.sSeqNo = 0;
			stash.Add(single);
			List<string> result = new List<string>();
			result.Add(teststring);
			SCTPStreamListener l = new CheckingStreamListener(result);
			UnorderedStreamBehaviour instance = new UnorderedStreamBehaviour();
			instance.deliver(s, stash, l);
			int remain = result.Count;
			Assert.AreEqual(remain, 0);
		}

		[TestMethod]
		public void testDontDeliverBegin() {
			Console.WriteLine("--> dont deliver Lone Begin");

			dontDeliverOnePart(DataChunk.BEGINFLAG);
		}

		[TestMethod]
		public void testDontDeliverMiddle() {
			Console.WriteLine("--> dont deliver Lone Middle");

			dontDeliverOnePart(0);
		}

		[TestMethod]
		public void testDontDeliverEnd() {
			Console.WriteLine("--> dont deliver Lone End");

			dontDeliverOnePart(DataChunk.ENDFLAG);
		}

		void dontDeliverOnePart(int flag) {
			SCTPStream s = mockStream();
			SortedArray<DataChunk> stash = new SortedArray<DataChunk>();
			DataChunk single = new DataChunk();
			string teststring = "Test string";
			single.setData(teststring.getBytes());
			single.ppid = SCTP_PPID.WEBRTCstring;
			single.setFlags(flag);
			single.tsn = (uint) _tsn++;
			single.sSeqNo = 0;
			stash.Add(single);
			List<string> result = new List<string>();
			result.Add(teststring);
			SCTPStreamListener l = new CheckingStreamListener(result);
			UnorderedStreamBehaviour instance = new UnorderedStreamBehaviour();
			instance.deliver(s, stash, l);
			int remain = result.Count;
			Assert.AreEqual(1, remain);
		}

		[TestMethod]
		public void testDeliverTwo() {
			Console.WriteLine("--> deliver two");
			SCTPStream s = mockStream();
			SortedArray<DataChunk> stash = new SortedArray<DataChunk>();
			string[] teststrings = { "Test string A", "Test string B" };
			List<string> result = new List<string>();
			int n = 0;
			foreach (string ts in teststrings) {
				DataChunk single = new DataChunk();
				single.tsn = (uint) _tsn++;
				single.sSeqNo = 0;
				single.setData(ts.getBytes());
				single.ppid = SCTP_PPID.WEBRTCstring;
				single.setFlags(DataChunk.SINGLEFLAG);
				stash.Add(single);
				result.Add(ts);
			}
			SCTPStreamListener l = new CheckingStreamListener(result);
			UnorderedStreamBehaviour instance = new UnorderedStreamBehaviour();
			instance.deliver(s, stash, l);

			int remain = result.Count;
			Assert.AreEqual(remain, 0);
		}

		[TestMethod]
		public void testDeliverTwoPartMessage() {
			string[] teststrings = { "Test string A, ", "Test string B." };
			Console.WriteLine("--> deliver two part message");
			multiPartMessage(teststrings);
		}

		[TestMethod]
		public void testDeliverThreePartMessage() {
			string[] teststrings = { "Test string A, ", "Test string B ", "and Test string C" };
			Console.WriteLine("--> deliver three part message");
			multiPartMessage(teststrings);
		}

		[TestMethod]
		public void testDeliverLongMessage() {
			string[] teststrings = new string[333];
			Console.WriteLine("--> deliver many part message");
			for (int i = 0; i < teststrings.Length; i++) {
				teststrings[i] = " Test string " + i;
			}
			multiPartMessage(teststrings);
		}

		void multiPartMessage(string[] teststrings) {
			SCTPStream s = mockStream();
			SortedArray<DataChunk> stash = new SortedArray<DataChunk>();
			List<string> result = new List<string>();
			int n = 0;
			StringBuilder bs = new StringBuilder();
			foreach (string ts in teststrings) {
				DataChunk single = new DataChunk();
				single.tsn = (uint) _tsn++;
				single.sSeqNo = 0;
				single.setData(ts.getBytes());
				single.ppid = SCTP_PPID.WEBRTCstring;
				if (n == 0) {
					single.setFlags(DataChunk.BEGINFLAG);
				} else if (n == teststrings.Length - 1) {
					single.setFlags(DataChunk.ENDFLAG);
				}
				n++;
				bs.Append(ts);
				stash.Add(single);
			}
			result.Add(bs.ToString());
			SCTPStreamListener l = new CheckingStreamListener(result);
			UnorderedStreamBehaviour instance = new UnorderedStreamBehaviour();
			instance.deliver(s, stash, l);

			int remain = result.Count;
			Assert.AreEqual(remain, 0);
		}

		void oneMissingPartMessages(string[] teststrings, string es, int ec) {
			SCTPStream s = mockStream();
			SortedArray<DataChunk> stash = new SortedArray<DataChunk>();
			List<string> result = new List<string>();
			int n = 0;
			int expectedToRemain = 0;
			foreach (string ts in teststrings) {
				for (int i = 0; i < ts.Length; i++) {
					DataChunk single = new DataChunk();
					single.sSeqNo = n;
					single.tsn = (uint) _tsn++;
					string letter = ts.Substring(i, 1);
					single.setData(letter.getBytes());
					single.ppid = SCTP_PPID.WEBRTCstring;
					if (i == 0) {
						single.setFlags(DataChunk.BEGINFLAG);
					} else if (i == ts.Length - 1) {
						single.setFlags(DataChunk.ENDFLAG);
					}
					if ((ec != i) || !ts.Equals(es)) {
						stash.Add(single);
					}
				}
				if (ts.Equals(es)) {
					expectedToRemain += 1;
				}
				result.Add(ts);
				n++;

			}

			SCTPStreamListener l = new CheckingStreamListener(result);
			UnorderedStreamBehaviour instance = new UnorderedStreamBehaviour();

			instance.deliver(s, stash, l);

			int remain = result.Count;
			//Console.WriteLine("expected:" + expectedToRemain + " remain:" + remain);

			Assert.AreEqual(remain, expectedToRemain);
		}

		[TestMethod]
		public void testDeliverNoMissingPartMessage() {
			Console.WriteLine("--> deliver no missing part message");
			string[] teststrings = { "Test string A, ", "Test string B ", "and Test string C" };

			oneMissingPartMessages(teststrings, "", -1);
		}
		/*
			// todo - fix this - 
			[TestMethod]
			public void testDeliverOneMissingPartMessage() {
				Console.WriteLine("--> deliver one missing part message");
				string teststrings[] = {"Test string A, ", "Test string B ", "and Test string C"};

				foreach (string es in teststrings) {
					for (int ec = 0; ec < es.length(); ec++) {
						oneMissingPartMessages(teststrings, es, ec);
					}
				}
			}
		*/
		[TestMethod]
		public void testDeliverUnorderedPackets() {
			Console.WriteLine("--> deliver messages with random packet arrival");
			for (int i = 0; i < 100; i++) {
				deliverUnorderedPackets(i);
			}
		}

		public void deliverUnorderedPackets(int seed) {
			Random rand = new Random(seed); // deliberately not crypto random so test is repeatable 
											// Console.WriteLine("seed = "+seed);
			string[] teststrings = { "Test string A, ", "Test string B ", "and Test string C" };
			SCTPStream s = mockStream();
			List<string> result = new List<string>();
			int n = 0;
			List<DataChunk> all = new List<DataChunk>();
			foreach (string ts in teststrings) {
				for (int i = 0; i < ts.Length; i++) {
					DataChunk single = new DataChunk();
					single.sSeqNo = n;
					single.tsn = (uint) _tsn++;
					string letter = ts.Substring(i, 1);
					single.setData(letter.getBytes());
					single.ppid = SCTP_PPID.WEBRTCstring;
					if (i == 0) {
						single.setFlags(DataChunk.BEGINFLAG);
					} else if (i == ts.Length - 1) {
						single.setFlags(DataChunk.ENDFLAG);
					}
					all.Add(single);
				}
				result.Add(ts);
				n++;

			}

			SCTPStreamListener l = new CheckingStreamListener(result);
			UnorderedStreamBehaviour instance = new UnorderedStreamBehaviour();
			SortedArray<DataChunk> stash = new SortedArray<DataChunk>();
			while (all.Count > 0) {
				int v = rand.Next(all.Count);
				stash.Add(all[v]);
				all.RemoveAt(v);
				instance.deliver(s, stash, l);
			}

			int remain = result.Count;
			Assert.AreEqual(remain, 0);
		}
	}
}
