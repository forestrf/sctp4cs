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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using SCTP4CS.Utils;
using pe.pi.sctp4j.sctp;
using pe.pi.sctp4j.sctp.messages;
using System;
using System.Collections.Generic;

/**
 *
 * @author Westhawk Ltd<thp@westhawk.co.uk>
 */
namespace com.ipseorama.sctp.messages {
	[TestClass]
	public class TestPacket {
		byte[] sampleInit = {
			(byte) 0x13, (byte) 0x88, (byte) 0x13, (byte) 0x88, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x68,
			(byte) 0x1c, (byte) 0xb9, (byte) 0xa0, (byte) 0x01, (byte) 0x00, (byte) 0x00, (byte) 0x56, (byte) 0x95,
			(byte) 0xaa, (byte) 0x39, (byte) 0xc0, (byte) 0x00, (byte) 0x02, (byte) 0x00, (byte) 0x00, (byte) 0x03,
			(byte) 0xff, (byte) 0x08, (byte) 0x00, (byte) 0x83, (byte) 0x14, (byte) 0xf5, (byte) 0xb3, (byte) 0xc0,
			(byte) 0x00, (byte) 0x00, (byte) 0x04, (byte) 0x80, (byte) 0x08, (byte) 0x00, (byte) 0x0a, (byte) 0xc1,
			(byte) 0x80, (byte) 0xc0, (byte) 0x81, (byte) 0x82, (byte) 0x0f, (byte) 0x00, (byte) 0x00, (byte) 0x80,
			(byte) 0x02, (byte) 0x00, (byte) 0x24, (byte) 0x8e, (byte) 0xb2, (byte) 0x16, (byte) 0x46, (byte) 0x28,
			(byte) 0xe1, (byte) 0xaf, (byte) 0x0d, (byte) 0xf7, (byte) 0x19, (byte) 0xef, (byte) 0x53, (byte) 0xa7,
			(byte) 0xa7, (byte) 0x7c, (byte) 0x6c, (byte) 0x0e, (byte) 0x93, (byte) 0x73, (byte) 0x60, (byte) 0x54,
			(byte) 0x73, (byte) 0xee, (byte) 0x2c, (byte) 0x6f, (byte) 0x8c, (byte) 0x23, (byte) 0x6c, (byte) 0x51,
			(byte) 0xe1, (byte) 0xbe, (byte) 0x5f, (byte) 0x80, (byte) 0x04, (byte) 0x00, (byte) 0x06, (byte) 0x00,
			(byte) 0x01, (byte) 0x00, (byte) 0x00, (byte) 0x80, (byte) 0x03, (byte) 0x00, (byte) 0x06, (byte) 0x80,
			(byte) 0xc1, (byte) 0x00, (byte) 0x00 };
    
		byte [] sampleAbort  = { (byte)0x13, (byte)0x88, (byte)0x13, (byte)0x88, (byte)0x27, (byte)0x44, (byte)0xfa, (byte)0x52, (byte)0xb0, (byte)0x85
			, (byte)0xdd, (byte)0x7b, (byte)0x06, (byte)0x01, (byte)0x00, (byte)0x0c, (byte)0x00, (byte)0x0d, (byte)0x00, (byte)0x08, (byte)0x40, (byte)0x00, (byte)0x00
			, (byte)0x01 };
		byte [] sampleAbort2 = { (byte)0x13,(byte)0x88,(byte)0x13,(byte)0x88,(byte)0x53,(byte)0x05,(byte)0x6d,(byte)0xb5,(byte)0x97,(byte)0xe0,(byte)0xd0
			,(byte)0x20,(byte)0x06,(byte)0x00,(byte)0x00,(byte)0x14,(byte)0x00,(byte)0x0d,(byte)0x00,(byte)0x10,(byte)0x30,(byte)0x00,(byte)0x00,(byte)0x02
			,(byte)0x44,(byte)0x15,(byte)0xd2,(byte)0x71,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x00 };
    
		byte [] sampleData = { (byte)0x13, (byte)0x88, (byte)0x13, (byte)0x88, (byte)0x13, (byte)0xfc, (byte)0x3a, (byte)0x88, (byte)0x87, (byte)0xa9
			, (byte)0x5b, (byte)0xfc, (byte)0x00, (byte)0x03, (byte)0x00, (byte)0x20, (byte)0xf8, (byte)0x13, (byte)0x18, (byte)0x5b, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x32, (byte)0x03, (byte)0x00
			, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x63, (byte)0x68, (byte)0x61, (byte)0x74 };
    
		byte [] sampleHeartBeat = { (byte)0x13, (byte)0x88, (byte)0x13, (byte)0x88, (byte)0x00, (byte)0x6f, (byte)0x0e, (byte)0x57, (byte)0x9a, (byte)0x69, (byte)0x21, (byte)0x26
			, (byte)0x04, (byte)0x00, (byte)0x00, (byte)0x2c, (byte)0x00, (byte)0x01, (byte)0x00, (byte)0x28, (byte)0xd7, (byte)0x47, (byte)0x62, (byte)0x53, (byte)0x88, (byte)0xc4, (byte)0x0d, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x7b, (byte)0x08, (byte)0x00, (byte)0x00, (byte)0xa0, (byte)0x82
			, (byte)0x07, (byte)0x7c, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00 };
    
		byte [] dcepack = { (byte) 0x13, (byte) 0x88, (byte) 0x13, (byte) 0x88, (byte) 0x96, (byte) 0x83, (byte) 0x0e, (byte) 0xe2, (byte) 0xa4, (byte) 0xed, (byte) 0x62, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x11, (byte) 0x57, (byte) 0xd3, (byte) 0x59, (byte) 0x0a, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x32, (byte) 0x02 };
		byte [] cookieEcho = { (byte)0x13,(byte)0x88,(byte)0x13,(byte)0x88,(byte)0xf5,(byte)0x36,(byte)0xd8,(byte)0x68,(byte)0x23,(byte)0xf0,(byte)0x16,(byte)0x11,(byte)0x0a,(byte)0x00
			,(byte)0x00,(byte)0x24,(byte)0x59,(byte)0xfe,(byte)0xc8,(byte)0x5b,(byte)0xf0,(byte)0x2c,(byte)0x25,(byte)0xe6,(byte)0x97,(byte)0x23,(byte)0x33,(byte)0xa7,(byte)0x71,(byte)0x5e,(byte)0xb0,(byte)0x42,(byte)0x16,(byte)0x5f
			,(byte)0xdd,(byte)0xa9,(byte)0x0a,(byte)0x5a,(byte)0xfa,(byte)0xa1,(byte)0x90,(byte)0xfe,(byte)0x0f,(byte)0x2b,(byte)0xd0,(byte)0x08,(byte)0x56,(byte)0xd4,(byte)0x00,(byte)0x03,(byte)0x00,(byte)0x20,(byte)0xad
			,(byte)0xac,(byte)0x5b,(byte)0xe2,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x32,(byte)0x03,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x00,(byte)0x00
			,(byte)0x00,(byte)0x04,(byte)0x00,(byte)0x00,(byte)0x63,(byte)0x68,(byte)0x61,(byte)0x74 };
		
		class NonvalidatingPacket : Packet {
			public NonvalidatingPacket(ByteBuffer pkt) : base(pkt) { }
			void setChecksum(ByteBuffer pkt){}
			/*
			protected override void checkChecksum(ByteBuffer pkt){
				// some of the tests would produce invalid checksums - we don't care - for the moment.
			}
			*/
		}
		
		[TestMethod]
		public void testValidHeader() {
			ByteBuffer b = ByteBuffer.wrap(sampleInit);
			Packet p = new NonvalidatingPacket(b);
			Assert.AreEqual(5000, p.getDestPort(), "DestPort should be 5000");
			Assert.AreEqual(5000, p.getSrcPort(), "SrcPort should be 5000");
			Assert.AreEqual(0, p.getVerTag(), "verTag should be 0");
		}

		[TestMethod]
		public void testShortHeader(){
			ByteBuffer b = new ByteBuffer(new byte[1]);
			Exception x = null;
			try {
				Packet p = new NonvalidatingPacket(b);
			} catch (Exception e){
				x = e;
			}
			Assert.AreEqual(false,(x==null), "Exception should be thrown ");
			Assert.AreEqual(x.GetType(),typeof(SctpPacketFormatException), "Expecting exception SctpPacketException ");
		}

		[TestMethod]
		public void testRecycleHeader() {
			//ByteBuffer b = ByteBuffer.wrap(sampleInit, 0, 12);
			ByteBuffer b = ByteBuffer.wrap(sampleInit);
			Packet p = new NonvalidatingPacket(b);
			ByteBuffer bb = p.getByteBuffer();
			Assert.AreEqual(Packet.getHex(b), Packet.getHex(bb), "Expecting same content in packets ");
		}

		[TestMethod]
		public void testChunks(){
			ByteBuffer b = new ByteBuffer(sampleInit);
			Packet p = new Packet(b);
			Assert.AreEqual(1, p.getChunkList().Count, "Expecting 1 chunk ");
		}

		[TestMethod]
		public void testInitWrite(){
			ByteBuffer b = ByteBuffer.wrap(sampleInit);
			Packet p = new Packet(b);
			List<Chunk> chunks = p.getChunkList();
			Assert.AreEqual(1,chunks.Count, "Expecting 1 chunk ");
			Chunk init = chunks[0];
			Assert.AreEqual(init.getType(),Chunk.CType.INIT, "Expecting an init chunk");
			ByteBuffer bout = ByteBuffer.allocate(sampleInit.Length-12);
			init.write(bout);
			byte[]sin = new byte[sampleInit.Length-12];
			Array.Copy(sampleInit,12,sin,0, sin.Length);
			Assert.AreEqual(Packet.getHex(sin),Packet.getHex(bout.Data), "Expected to re-make same packet");
		}

		[TestMethod]
		public void testAbort(){
			ByteBuffer b = ByteBuffer.wrap(sampleAbort);
			Console.WriteLine("Sample Abort is " + sampleAbort.Length);

			Packet p = new Packet(b);
			List<Chunk> chunks = p.getChunkList();
			Assert.AreEqual(1,chunks.Count, "Expecting 1 chunk ");
			Chunk abort = chunks[0];
			Assert.AreEqual(abort.getType(),Chunk.CType.ABORT, "Expecting an abort chunk");
		}

		[TestMethod]
		public void testAbort2(){
			ByteBuffer b = ByteBuffer.wrap(sampleAbort2);
			Console.WriteLine("Sample Abort is " + sampleAbort2.Length);

			Packet p = new Packet(b);
			List<Chunk> chunks = p.getChunkList();
			Assert.AreEqual(1,chunks.Count, "Expecting 1 chunk ");
			Chunk abort = chunks[0];
			Assert.AreEqual(abort.getType(),Chunk.CType.ABORT, "Expecting an abort chunk");
		}

		[TestMethod]
		public void testCookieEcho(){
			ByteBuffer b = ByteBuffer.wrap(cookieEcho);
			Packet p = new Packet(b);
			List<Chunk> chunks = p.getChunkList();
			Assert.AreEqual(2,chunks.Count, "Expecting 2 chunks ");
			Chunk ic = chunks[0];
			Assert.AreEqual(ic.getType(),Chunk.CType.COOKIE_ECHO, "Expecting a cookie echo chunk");
			Chunk dopen = chunks[1];
			Assert.AreEqual(dopen.getType(),Chunk.CType.DATA, "Expecting a data chunk");

		}

		[TestMethod]
		public void testInitAck(){
			ByteBuffer b = ByteBuffer.wrap(sampleInit);
			Packet p = new Packet(b);
			List<Chunk> chunks = p.getChunkList();
			Assert.AreEqual(1,chunks.Count, "Expecting 1 chunk ");
			Chunk ic = chunks[0];
			Assert.AreEqual(ic.getType(),Chunk.CType.INIT, "Expecting an init chunk");

			Association ass = new MockAssociation(null,null);
			Chunk [] ca = ass.inboundInit(ic as InitChunk);
			Assert.AreEqual(1,ca.Length, "Expecting a single reply chunk");
			ByteBuffer iacbb = ass.mkPkt(ca);
			Packet iac = new Packet(iacbb);
			chunks = iac.getChunkList();
			Assert.AreEqual(1,chunks.Count, "Expecting 1 chunk ");
			ic  = chunks[0];
			Assert.AreEqual(ic.getType(),Chunk.CType.INITACK, "Expecting an InitAck chunk");

		}

		[TestMethod]
		public void testOpenDataChunk(){
			ByteBuffer b = ByteBuffer.wrap(sampleData);
			Packet p = new Packet(b);
			List<Chunk> chunks = p.getChunkList();
			Assert.AreEqual(1,chunks.Count, "Expecting 1 chunk ");
			DataChunk dat = chunks[0] as DataChunk;
			Assert.AreEqual(dat.getType(),Chunk.CType.DATA, "Expecting a Data chunk");
			Console.WriteLine("got "+dat.GetType().Name+ " chunk" + dat.ToString());
			Assert.AreEqual(dat.getSSeqNo(),0, "Expecting seqno of zero");
			Assert.AreEqual(dat.getStreamId(),0, "Expecting stream of zero");
			Assert.AreEqual(dat.getPpid(),50, "Expecting an DCEP");
			Assert.AreEqual(dat.getData(),null, "Data should be zero");
			Assert.AreEqual(dat.getDCEP()!=null,true, "Expected  to parse a DCEP packet");
			Assert.AreEqual(dat.getDCEP().isAck(),false, "Expected an open DCEP packet ");


		}

		[TestMethod]
		public void testHeartBeatChunk(){
			ByteBuffer b = ByteBuffer.wrap(sampleHeartBeat);
			Packet p = new Packet(b);
			List<Chunk> chunks = p.getChunkList();
			Assert.AreEqual(1,chunks.Count, "Expecting 1 chunk ");
			HeartBeatChunk dat = chunks[0] as HeartBeatChunk;
			Assert.AreEqual(dat.getType(),Chunk.CType.HEARTBEAT, "Expecting a HeartBeatChunk");
			Chunk[] r = dat.mkReply();
			Assert.AreEqual(1,r.Length, "Expecting 1 chunk ");
			Assert.AreEqual(r[0].getType(),Chunk.CType.HEARTBEAT_ACK, "Expecting a HeartBeatAckChunk");
		}

		[TestMethod]
		public void testDCEPAckChunk(){
			ByteBuffer b = ByteBuffer.wrap(dcepack);
			Packet p = new Packet(b);
			List<Chunk> chunks = p.getChunkList();
			Assert.AreEqual(1,chunks.Count, "Expecting 1 chunk ");
			DataChunk dat = chunks[0] as DataChunk;
			Assert.AreEqual(dat.getType(),Chunk.CType.DATA, "Expecting a DataChunk");
		}
	}
}
