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



using Org.BouncyCastle.Crypto.Tls;
using pe.pi.sctp4j.sctp.messages;
using pe.pi.sctp4j.sctp.small;
using System;
/**
*
* @author Westhawk Ltd<thp@westhawk.co.uk>
*/
namespace pe.pi.sctp4j.sctp {
	public class MockAssociation : Association {
		public MockAssociation(DatagramTransport transport, AssociationListener al) : base(transport, al) { }



		internal override void enqueue(DataChunk d) {
			throw new Exception("[UnsupportedOperationException] Not supported yet. (enqueue)"); //To change body of generated methods, choose Tools | Templates.
		}

		class SCTPStreamImpl : SCTPStream {
			public SCTPStreamImpl(Association a, int id) : base(a, id) {
			}

			internal override void delivered(DataChunk d) { }

			internal override void deliverMessage(SCTPMessage message) { }

			public override void send(string message) {
				SCTPMessage m = new SCTPMessage(message, this);
				_ass.sendAndBlock(m);
			}

			public override void send(byte[] message, int offset, int length) { }
		}


		public override SCTPStream mkStream(int id) {
			return new SCTPStreamImpl(this, id);
		}

		internal override void sendAndBlock(SCTPMessage m) {
			Chunk[] dar = new Chunk[1];

			DataChunk dc = new DataChunk();
			m.fill(dc);
			dc.setTsn(_nearTSN++);
			// check rollover - will break at maxint.
			dar[0] = dc;
			send(dar);

		}

		internal override SCTPMessage makeMessage(byte[] bytes, int offset, int length, BlockingSCTPStream aThis) {
			throw new Exception("[UnsupportedOperationException] Not supported yet. (makeMessage)"); //To change body of generated methods, choose Tools | Templates.
		}

		internal override Chunk[] inboundInit(InitChunk i) {
			return base.inboundInit(i);
		}

		public void setMyVerTag(int v) {
			base._myVerTag = v;
		}

		internal override SCTPMessage makeMessage(string s, BlockingSCTPStream aThis) {
			throw new Exception("[UnsupportedOperationException] Not supported yet.(Make Message - string"); //To change body of generated methods, choose Tools | Templates.
		}

		internal override Chunk[] sackDeal(SackChunk sackChunk) {
			throw new Exception("[UnsupportedOperationException] Not supported yet."); //To change body of generated methods, choose Tools | Templates.
		}

		public override void associate() { }
	}
}
