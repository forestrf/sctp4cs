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
using SCTP4CS.Utils;
using pe.pi.sctp4j.sctp.messages.Params;
using System;

/**
 *
 * @author Westhawk Ltd<thp@westhawk.co.uk>
 */

/*

 The format of the INIT ACK chunk is shown below:

 0                   1                   2                   3
 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |   Type = 2    |  Chunk Flags  |      Chunk Length             |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |                         Initiate Tag                          |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |              Advertised Receiver Window Credit                |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |  Number of Outbound Streams   |  Number of Inbound Streams    |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |                          Initial TSN                          |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 \                                                               \
 /              Optional/Variable-Length Parameters              /
 \                                                               \
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 */
namespace pe.pi.sctp4j.sctp.messages {
	internal class InitAckChunk : Chunk {
		int _initiateTag;
		uint _adRecWinCredit;
		int _numOutStreams;
		int _numInStreams;
		uint _initialTSN;
		public ByteBuffer cookie;
		public ByteBuffer supportedExtensions;

		public InitAckChunk() : base(CType.INITACK) { }

		public int getInitiateTag() {
			return _initiateTag;
		}

		public void setInitiateTag(int v) {
			_initiateTag = v;
		}

		public uint getAdRecWinCredit() {
			return _adRecWinCredit;
		}

		public void setAdRecWinCredit(uint v) {
			_adRecWinCredit = v;
		}

		public int getNumOutStreams() {
			return _numOutStreams;
		}

		public void setNumOutStreams(int v) {
			_numOutStreams = v;
		}

		public int getNumInStreams() {
			return _numInStreams;
		}

		public void setNumInStreams(int v) {
			_numInStreams = v;
		}

		public uint getInitialTSN() {
			return _initialTSN;
		}

		public void setInitialTSN(uint v) {
			_initialTSN = v;
		}
		
		public InitAckChunk(CType type, byte flags, int length, ref ByteBuffer pkt)
			: base(type, flags, length, ref pkt) {
			if (_body.remaining() >= 16) {
				_initiateTag = _body.GetInt();
				_adRecWinCredit = _body.GetUInt(); ;
				_numOutStreams = _body.GetUShort();
				_numInStreams = _body.GetUShort();
				_initialTSN = _body.GetUInt();
				Logger.Trace("Init Ack" + this.ToString());
				while (_body.hasRemaining()) {
					_varList.Add(readVariable());
				}

				foreach (Param v in _varList) {
					// now look for variables we are expecting...
					Logger.Trace("variable of type: " + Enum.GetName(typeof(VariableParamType), v.type) + " " + v.ToString());
					if (v.type == VariableParamType.StateCookie) {
						cookie = v.data;
					} else {
						Logger.Trace("ignored variable of type: " + Enum.GetName(typeof(VariableParamType), v.type));
					}
				}

			}
		}

		protected override void putFixedParams(ref ByteBuffer ret) {
			ret.Put(_initiateTag);
			ret.Put(_adRecWinCredit);
			ret.Put((ushort) _numOutStreams);
			ret.Put((ushort) _numInStreams);
			ret.Put(_initialTSN);
			if (cookie != null) {
				Param sc = new Param(VariableParamType.StateCookie);
				sc.data = cookie;
				_varList.Add(sc);
			}
			if (supportedExtensions != null) {
				Param se = new Param(VariableParamType.SupportedExtensions);
				se.data = supportedExtensions;
				_varList.Add(se);
			}
		}
	}
}
