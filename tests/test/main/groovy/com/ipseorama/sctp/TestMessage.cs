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


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pe.pi.sctp4j.sctp;
using pe.pi.sctp4j.sctp.messages;
using System.Collections.Generic;

/**
 *
 * @author Westhawk Ltd<thp@westhawk.co.uk>
 */
namespace com.ipseorama.sctp {
	[TestClass]
	public class TestMessage {
		string data = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		Association a;
		SCTPStream stream;
		int id;

		class SCTPStreamImpl : SCTPStream {
			public SCTPStreamImpl(Association a, int id) : base(a, id) {
			}

			public override void delivered(DataChunk d) {
				throw new NotImplementedException();
			}

			public override void deliverMessage(SCTPMessage message) {
				throw new NotImplementedException();
			}

			public override void send(byte[] message) {
				throw new NotImplementedException();
			}

			public override void send(string message) {
			}
		}

		[TestInitialize]
		public void setUp() {
			a = null;
			id = 1000;
			stream = new SCTPStreamImpl(a, id);
		}

		[TestMethod]
		public void testMessFillSingle() {
			// assume capacity > 52        
			SCTPMessage m = new SCTPMessage(data, stream);
			List<DataChunk> chunks = new List<DataChunk>();
			while (m.hasMoreData()) {
				DataChunk dc = new DataChunk();
				m.fill(dc);
				chunks.Add(dc);
			}
			Console.WriteLine("chunks " + chunks.Count);
			Assert.AreEqual(chunks.Count, 1, "Wrong number of chunks");

			Assert.AreEqual(chunks[0].getFlags(), DataChunk.SINGLEFLAG, "First (and only) chunk should have single flag set");

		}

		class DataChunkImpl : DataChunk {
			public override int getCapacity() {
				return 1; // shrug
			}
		}

		[TestMethod]
		public void testMessFill1() {
			SCTPMessage m = new SCTPMessage(data, stream);
			List<DataChunk> chunks = new List<DataChunk>();
			while (m.hasMoreData()) {
				DataChunk dc = new DataChunkImpl();
				m.fill(dc);
				chunks.Add(dc);
			}
			Console.WriteLine("chunks " + chunks.Count);
			Assert.AreEqual(chunks.Count, data.Length, "Wrong number of chunks");

			Assert.AreEqual(chunks[0].getFlags(), DataChunk.BEGINFLAG, "Start chunk should have start flag set");
			Assert.AreEqual(chunks[data.Length - 1].getFlags(), DataChunk.ENDFLAG, "End chunk should have end flag set");
			for (int i = 1; i < data.Length - 1; i++) {
				Assert.AreEqual(chunks[i].getFlags(), 0, "middle chunk should have no flag set");
				Assert.AreEqual(chunks[i].getDataAsString(), data[i].ToString(), "middle data should match input");
			}
		}
	}
}
