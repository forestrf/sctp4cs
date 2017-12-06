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
using System.Text;

/**
*
* @author tim
*/
namespace pe.pi.sctp4j.sctp.messages.Params {
	public struct IncomingSSNResetRequestParameter {
		/*
		 0                   1                   2                   3
		 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |     Parameter Type = 14       |  Parameter Length = 8 + 2 * N |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |          Re-configuration Request Sequence Number             |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |  Stream Number 1 (optional)   |    Stream Number 2 (optional) |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 /                            ......                             /
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |  Stream Number N-1 (optional) |    Stream Number N (optional) |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 */
		public readonly uint reqSeqNo;
		public readonly int[] streams;

		private ByteBuffer b;

		public IncomingSSNResetRequestParameter(ref Param param) {
			b = param.data;
			reqSeqNo = param.data.GetUInt();
			if (param.data.Length > 4) {
				this.streams = new int[(param.data.Length - 4) / 2];
				for (int i = 0; i < streams.Length; i++) {
					streams[i] = param.data.GetUShort();
				}
			} else {
				this.streams = new int[0];
				Logger.Warn("No inbound stream mentioned");
			}
		}
		
		public IncomingSSNResetRequestParameter(uint reqNo, int[] streams) {
			this.reqSeqNo = reqNo;
			this.streams = streams;
			b = new ByteBuffer(new byte[1500]);
			writeBody(ref b);
		}
		

		public void writeBody(ref ByteBuffer body) {
			body.Put(reqSeqNo);
			if (streams != null) {
				for (int i = 0; i < streams.Length; i++) {
					body.Put((ushort) streams[i]);
				}
			}
		}

		public override string ToString() {
			StringBuilder ret = new StringBuilder();
			ret.Append(this.GetType().Name).Append(" ");
			ret.Append("seq:" + this.reqSeqNo);
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
			Param p = new Param(VariableParamType.IncomingSSNResetRequestParameter);
			p.data = b;
			return p;
		}
	}
}
