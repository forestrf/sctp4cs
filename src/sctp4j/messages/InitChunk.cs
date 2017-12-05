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
		public readonly byte[] farSupportedExtensions;
		byte[] _farRandom;
		bool _farForwardTSNsupported;
		byte[] _farHmacs;
		byte[] _farChunks;

		public InitChunk() : base(CType.INIT) { }

		public InitChunk(CType type, byte flags, int length, ByteBuffer pkt)
			: base(type, flags, length, pkt) {
			if (_body.remaining() >= 16) {
				initiateTag = _body.GetInt();
				adRecWinCredit = _body.GetUInt();
				numOutStreams = _body.GetUShort();
				numInStreams = _body.GetUShort();
				initialTSN = _body.GetUInt();
				Logger.Trace("Init " + this.ToString());
				while (_body.hasRemaining()) {
					VariableParam v = readVariable();
					_varList.Add(v);
				}
				foreach (VariableParam v in _varList) {
					// now look for variables we are expecting...
					Logger.Trace("variable of type: " + v.name + " " + v.ToString());
					if (typeof(SupportedExtensions).IsAssignableFrom(v.GetType())) {
						farSupportedExtensions = ((SupportedExtensions) v).getData();
					} else if (typeof(RandomParam).IsAssignableFrom(v.GetType())) {
						_farRandom = ((RandomParam) v).getData();
					} else if (typeof(ForwardTSNsupported).IsAssignableFrom(v.GetType())) {
						_farForwardTSNsupported = true;
					} else if (typeof(RequestedHMACAlgorithmParameter).IsAssignableFrom(v.GetType())) {
						_farHmacs = ((RequestedHMACAlgorithmParameter) v).getData();
					} else if (typeof(ChunkListParam).IsAssignableFrom(v.GetType())) {
						_farChunks = ((ChunkListParam) v).getData();
					} else {
						Logger.Trace("unexpected variable of type: " + v.name);
					}
				}
			}
		}

		protected override void putFixedParams(ByteBuffer ret) {
			ret.Put((int) initiateTag);
			ret.Put(adRecWinCredit);
			ret.Put((ushort) numOutStreams);
			ret.Put((ushort) numInStreams);
			ret.Put(initialTSN);
		}
	}
}
