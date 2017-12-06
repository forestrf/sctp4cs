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

/**
 *
 * @author tim
 */
namespace pe.pi.sctp4j.sctp.messages.Params {
	public struct ReconfigurationResponseParameter {
		public enum STATUS : uint {
			SUCCESS_NOTHING_TO_DO = 0,
			SUCCESS_PERFORMED = 1,
			DENIED = 2,
			ERROR_WRONG_SSN = 3,
			ERROR_REQUEST_ALREADY_IN_PROGESS = 4,
			ERROR_BAD_SEQUENCE_NUMBER = 5,
			IN_PROGRESS = 6
		}

		/*
		 0                   1                   2                   3
		 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |     Parameter Type = 16       |      Parameter Length         |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |         Re-configuration Response Sequence Number             |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |                            Result                             |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |                   Sender's Next TSN (optional)                |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |                  Receiver's Next TSN (optional)               |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 */
		public readonly uint seqNo;
		public readonly STATUS result;
		public readonly uint senderNextTSN;
		public readonly uint receiverNextTSN;
		public readonly bool hasTSNs;

		public ReconfigurationResponseParameter(ref ByteBuffer body, int blen) {
			seqNo = body.GetUInt();
			result = (STATUS) body.GetUInt();
			if (blen == 16) {
				senderNextTSN = body.GetUInt();
				receiverNextTSN = body.GetUInt();
				hasTSNs = true;
			} else {
				senderNextTSN = 0;
				receiverNextTSN = 0;
				hasTSNs = false;
			}
		}
		public ReconfigurationResponseParameter(uint seqNo, STATUS result, bool hasTSNs, uint senderNextTSN, uint receiverNextTSN) {
			this.seqNo = seqNo;
			this.result = result;
			this.hasTSNs = hasTSNs;
			this.senderNextTSN = senderNextTSN;
			this.receiverNextTSN = receiverNextTSN;
		}

		public void writeBody(ref ByteBuffer body) {
			body.Put(seqNo);
			body.Put((uint) result);
			if (hasTSNs) {
				body.Put(senderNextTSN);
				body.Put(receiverNextTSN);
			}
		}
	}
}
