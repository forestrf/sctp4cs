/*
 * Copyright (C) 2014 Westhawk Ltd<thp@westhawk.co.uk>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */
// Modified by Andrés Leone Gámez


using SCTP4CS.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto.Tls;
using pe.pi.sctp4j.sctp;
using pe.pi.sctp4j.sctp.messages;
using System;
using System.Collections.Generic;
using System.Threading;

/**
 *
 * @author Westhawk Ltd<thp@westhawk.co.uk>
 */
namespace com.ipseorama.sctp {
	[TestClass]
	public class TestAssociation {
		byte[] sampleDataOpen = { (byte)0x13, (byte)0x88, (byte)0x13, (byte)0x88, (byte)0x13, (byte)0xfc, (byte)0x3a, (byte)0x88, (byte)0x8d, (byte)0xa9
			, (byte)0xa7, (byte)0x20, (byte)0x00, (byte)0x03, (byte)0x00, (byte)0x20, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x32, (byte)0x03, (byte)0x00
			, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x63, (byte)0x68, (byte)0x61, (byte)0x74 };

		class DatagramTransportImpl : DatagramTransport {
			public ByteBuffer sent;
			public DatagramTransportImpl(ByteBuffer sent) {
				this.sent = sent;
			}
			public int GetReceiveLimit() {
				return 1200;
			}

			public int GetSendLimit() {
				return 1200;
			}

			public int Receive(byte[] bytes, int i, int i1, int i2) {
				Thread.Sleep(i2);
				return 0;
			}

			public void Send(byte[] ou, int off, int pos) {
				byte[] bsent = new byte[pos - off];
				Array.Copy(ou, off, bsent, (int) 0, bsent.Length);
				sent = ByteBuffer.wrap(bsent);
			}

			public void Close() { }
		}

		class AssociationListenerImpl : AssociationListener {
			public SCTPStream stream;
			public AssociationListenerImpl(SCTPStream stream) {
				this.stream = stream;
			}

			public void onAssociated(Association a) {
				Console.WriteLine("Associated");

			}

			public void onDCEPStream(SCTPStream s, string label, int type) {
			}

			public void onDisAssociated(Association a) {
				Console.WriteLine("Disssociated");

			}

			public void onRawStream(SCTPStream s) {
			}

			public void onStream(SCTPStream s) {
				Console.WriteLine("Association opened stream ");
				stream = s;
			}
		}

		[TestMethod]
		public void testAss() {
			DatagramTransportImpl mock = new DatagramTransportImpl(null);
			AssociationListenerImpl al = new AssociationListenerImpl(null);
			MockAssociation ass = new MockAssociation(mock, al);
			ass.setMyVerTag(335297160);
			ByteBuffer b = ByteBuffer.wrap(sampleDataOpen);
			Packet p = new Packet(b);
			ass.deal(p);

			Packet ack = new Packet(mock.sent);
			List<Chunk> chunks = ack.getChunkList();
			Assert.AreEqual(1, chunks.Count, "Expecting 1 chunk ");
			DataChunk dat = chunks[0] as DataChunk;
			Assert.AreEqual(dat.getType(), DataChunk.CType.DATA, "Expecting a Data chunk");
			Console.WriteLine("got " + dat.GetType().Name + " chunk" + dat.ToString());
			Assert.AreEqual(dat.getSSeqNo(), 0, "Expecting seqno of zero");
			Assert.AreEqual(dat.getStreamId(), 0, "Expecting stream of zero");
			Assert.AreEqual(dat.getPpid(), 50, "Expecting an DCEP");
			Assert.AreEqual(dat.getData(), null, "Data should be zero");
			Assert.AreEqual(dat.getDCEP() != null, true, "Expected  to parse a DCEP packet");
			Assert.AreEqual(dat.getDCEP().isAck(), true, "Expected an ack DCEP packet ");

			Assert.AreEqual((al.stream == null), false, "expecting a stream");
			al.stream.send("hello");
			// ugh - uses a side effect on the sent buffer, which we capture.
			Packet pack = new Packet(mock.sent);
			chunks = pack.getChunkList();
			Assert.AreEqual(1, chunks.Count, "Expecting 1 chunk ");
			dat = chunks[0] as DataChunk;
			Assert.AreEqual(dat.getType(), Chunk.CType.DATA, "Expecting a Data chunk");
			Console.WriteLine("got " + dat.GetType().Name + " chunk" + dat.ToString());
			Assert.AreEqual(dat.getSSeqNo(), 1, "Expecting seqno of one"); // we've done a DCEP ack by now.
			Assert.AreEqual(dat.getStreamId(), 0, "Expecting stream of zero");
			Assert.AreEqual(dat.getDataAsString(), "hello", "Expecting hello in the data");
		}
	}
}
