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

/**
 *
 * @author tim
 */
namespace pe.pi.sctp4j.sctp.messages {
	internal class InitChunk : Chunk {
		/*
		 0                   1                   2                   3
		 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |   Type = 1    |  Chunk Flags  |      Chunk Length             |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |                         Initiate Tag                          |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |           Advertised Receiver Window Credit (a_rwnd)          |
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

		public int initiateTag;
		public uint adRecWinCredit;
		public int numOutStreams;
		public int numInStreams;
		public uint initialTSN;
		public readonly ByteBuffer farSupportedExtensions;
		ByteBuffer _farRandom;
		bool _farForwardTSNsupported;
		ByteBuffer _farHmacs;
		ByteBuffer _farChunks;

		public InitChunk() : base(CType.INIT) { }

		public InitChunk(CType type, byte flags, int length, ref ByteBuffer pkt)
			: base(type, flags, length, ref pkt) {
			if (_body.remaining() >= 16) {
				initiateTag = _body.GetInt();
				adRecWinCredit = _body.GetUInt();
				numOutStreams = _body.GetUShort();
				numInStreams = _body.GetUShort();
				initialTSN = _body.GetUInt();
				Logger.Trace("Init " + this.ToString());
				while (_body.hasRemaining()) {
					_varList.Add(readVariable());
				}
				foreach (Param v in _varList) {
					// now look for variables we are expecting...
					Logger.Trace("variable of type: " + v.type + " " + v.ToString());
					if (v.type == VariableParamType.SupportedExtensions) {
						farSupportedExtensions = v.data;
					} else if (v.type == VariableParamType.Random) {
						_farRandom = v.data;
					} else if (v.type == VariableParamType.ForwardTSNsupported) {
						_farForwardTSNsupported = true;
					} else if (v.type == VariableParamType.RequestedHMACAlgorithmParameter) {
						_farHmacs = v.data;
					} else if (v.type == VariableParamType.ChunkList) {
						_farChunks = v.data;
					} else {
						Logger.Trace("unexpected variable of type: " + v.type);
					}
				}
			}
		}

		protected override void putFixedParams(ref ByteBuffer ret) {
			ret.Put((int) initiateTag);
			ret.Put(adRecWinCredit);
			ret.Put((ushort) numOutStreams);
			ret.Put((ushort) numInStreams);
			ret.Put(initialTSN);
		}
	}
}
