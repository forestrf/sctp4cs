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

using SCTP4CS.Utils;
using System.Text;

/**
*
* @author tim
*/
namespace pe.pi.sctp4j.sctp.messages.Params {
	public struct OutgoingSSNResetRequestParameter {
		/*
		 0                   1                   2                   3
		 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |     Parameter Type = 13       | Parameter Length = 16 + 2 * N |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |           Re-configuration Request Sequence Number            |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |           Re-configuration Response Sequence Number           |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |                Sender's Last Assigned TSN                     |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |  Stream Number 1 (optional)   |    Stream Number 2 (optional) |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 /                            ......                             /
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |  Stream Number N-1 (optional) |    Stream Number N (optional) |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 */
		public readonly uint reqSeqNo;
		public readonly uint respSeqNo;
		public readonly uint lastTsn;
		public readonly int[] streams;

		private ByteBuffer b;

		public OutgoingSSNResetRequestParameter(ref Param param) {
			b = param.data;
			param.data.Position = 0;
			reqSeqNo = param.data.GetUInt();
			respSeqNo = param.data.GetUInt();
			lastTsn = param.data.GetUInt();
			streams = new int[(param.data.Length - 12) / 2];
			for (int i = 0; i < streams.Length; i++) {
				streams[i] = param.data.GetUShort();
			}
		}
		
		public OutgoingSSNResetRequestParameter(uint reqNo, uint respNo, uint lastTsn, int[] streams) {
			this.respSeqNo = respNo;
			this.lastTsn = lastTsn;
			this.reqSeqNo = reqNo;
			this.streams = streams;
			b = new ByteBuffer(new byte[1500]);
			writeBody(ref b);
		}

		public void writeBody(ref ByteBuffer body) {
			body.Put(reqSeqNo);
			body.Put(respSeqNo);
			body.Put(lastTsn);
			if (streams != null) {
				for (int i = 0; i < streams.Length; i++) {
					body.Put((ushort) streams[i]);
				}
			}
		}

		public override string ToString() {
			StringBuilder ret = new StringBuilder();
			ret.Append(this.GetType().Name).Append(" ");
			ret.Append("reqseq:").Append(this.reqSeqNo).Append(" ");
			ret.Append("respseq:").Append(this.respSeqNo).Append(" ");
			ret.Append("latsTSN:").Append(this.lastTsn).Append(" ");

			if (streams != null) {
				ret.Append("streams {");
				foreach (int s in streams) {
					ret.Append("" + s);
				}
				ret.Append("}");
			} else {
				ret.Append("no streams");
			}
			return ret.ToString();
		}



		public static bool Compare(ref Param a, ref Param b) {
			return a.type == VariableParamType.IncomingSSNResetRequestParameter
				&& b.type == VariableParamType.IncomingSSNResetRequestParameter
				&& a.data.rewind().GetUInt() == b.data.rewind().GetUInt();
		}

		public Param ToParam() {
			Param p = new Param(VariableParamType.OutgoingSSNResetRequestParameter);
			p.data = b;
			return p;
		}
	}
}
