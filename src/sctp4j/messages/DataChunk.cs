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
using pe.pi.sctp4j.sctp.dataChannel.DECP;
using System;
using System.Collections.Generic;
using System.Text;

/**
 *
 * @author Westhawk Ltd<thp@westhawk.co.uk>
 */
namespace pe.pi.sctp4j.sctp.messages {
	/*
   +-------------------------------+----------+-----------+------------+
   | Value                         | SCTP     | Reference | Date       |
   |                               | PPID     |           |            |
   +-------------------------------+----------+-----------+------------+
   | WebRTC string                 | 51       | [RFCXXXX] | 2013-09-20 |
   | WebRTC Binary Partial         | 52       | [RFCXXXX] | 2013-09-20 |
   | (Deprecated)                  |          |           |            |
   | WebRTC Binary                 | 53       | [RFCXXXX] | 2013-09-20 |
   | WebRTC string Partial         | 54       | [RFCXXXX] | 2013-09-20 |
   | (Deprecated)                  |          |           |            |
   | WebRTC string Empty           | 56       | [RFCXXXX] | 2014-08-22 |
   | WebRTC Binary Empty           | 57       | [RFCXXXX] | 2014-08-22 |
   +-------------------------------+----------+-----------+------------+

	 */
	public enum SCTP_PPID {
		WEBRTCCONTROL = 50,
		WEBRTCstring = 51,
		WEBRTCBINARY = 53,
		WEBRTCstringEMPTY = 56,
		WEBRTCBINARYEMPTY = 57
	}

	internal class DataChunk : Chunk, IComparer<DataChunk>, IComparable<DataChunk> {
		public const int BEGINFLAG = 2;
		public const int ENDFLAG = 1;
		public const int SINGLEFLAG = 3;
		public const int UNORDERED = 4;

		public uint tsn;
		private int _streamId;
		public int sSeqNo;
		public SCTP_PPID ppid;
		private byte[] _data;
		private int _dataOffset;
		private int _dataLength;

		private DCOpen _open;
		private InvalidDataChunkException _invalid;
		private bool _gapAck;
		private long _retryTime;
		private int _retryCount;
		private long _sentTime;

		public DataChunk(CType type, byte flags, int length, ref ByteBuffer pkt) : base(type, flags, length, ref pkt) {
			Logger.Debug("read in chunk header " + length);
			Logger.Debug("body remaining " + _body.remaining());

			if (_body.remaining() >= 12) {
				tsn = _body.GetUInt();
				_streamId = _body.GetUShort();
				sSeqNo = _body.GetUShort();
				ppid = (SCTP_PPID) _body.GetInt();

				Logger.Debug(" _tsn : " + tsn
						+ " _streamId : " + _streamId
						+ " _sSeqNo : " + sSeqNo
						+ " _ppid : " + ppid);
				Logger.Debug("data size remaining " + _body.remaining());

				switch (ppid) {
					case SCTP_PPID.WEBRTCCONTROL:
						ByteBuffer bb = _body.slice();
						try {
							_open = new DCOpen(bb);
						}
						catch (InvalidDataChunkException ex) {
							_invalid = ex;
						}
						Logger.Trace("Got an DCEP " + _open);
						break;
					case SCTP_PPID.WEBRTCstring:
						// what format is a string ?
						_data = new byte[_body.remaining()];
						_body.GetBytes(_data, _data.Length);
						_dataOffset = 0;
						_dataLength = _data.Length;
						Logger.Trace("string data is " + Encoding.ASCII.GetString(_data));
						break;
					case SCTP_PPID.WEBRTCBINARY:
						_data = new byte[_body.remaining()];
						_body.GetBytes(_data, _data.Length);
						_dataOffset = 0;
						_dataLength = _data.Length;
						Logger.Trace("data is " + _data.GetHex());
						break;

					default:
						_invalid = new InvalidDataChunkException("Invalid Protocol Id in data Chunk " + ppid);
						break;
				}
			}
		}

		public string getDataAsString() {
			string ret;
			switch (ppid) {
				case SCTP_PPID.WEBRTCCONTROL:
					ret = "Got an DCEP " + _open;
					break;
				case SCTP_PPID.WEBRTCstring:
					ret = Encoding.ASCII.GetString(_data, _dataOffset, _dataLength);
					break;
				case SCTP_PPID.WEBRTCstringEMPTY:
					ret = "Empty string message";
					break;
				case SCTP_PPID.WEBRTCBINARY:
					byte[] p = new byte[_dataLength];
					Array.Copy(_data, _dataOffset, p, 0, _dataLength);
					ret = _data.GetHex();
					break;
				case SCTP_PPID.WEBRTCBINARYEMPTY:
					ret = "Empty binay message";
					break;
				default:
					ret = "Invalid Protocol Id in data Chunk " + ppid;
					break;
			}
			return ret;
		}

		public override void validate() {
			if (_invalid != null) {
				throw _invalid;
			}
		}

		public DataChunk() : base(Chunk.CType.DATA) {
			setFlags(0); // default assumption.
		}

		/*
   
		 0                   1                   2                   3
		 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |   Type = 0    | Reserved|U|B|E|    Length                     |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |                              TSN                              |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |      Stream Identifier S      |   Stream Sequence Number n    |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |                  Payload Protocol Identifier                  |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 \                                                               \
		 /                 User Data (seq n of Stream S)                 /
		 \                                                               \
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

		 */

		public int getStreamId() {
			return this._streamId;
		}

		public byte[] getData() {
			return this._data;
		}

		public DCOpen getDCEP() {
			return this._open;
		}
		
		public int getDataSize() {
			return _dataLength;
		}

		public int getChunkLength() {
			int len = base.getLength();
			if (len == 0) {
				// ie outbound chunk.
				len = _dataLength + 12 + 4;
			}
			return len;
		}

		protected override void putFixedParams(ref ByteBuffer ret) {
			ret.Put(tsn);// = _body.getInt();
			ret.Put((ushort) _streamId);// = _body.getushort();
			ret.Put((ushort) sSeqNo);// = _body.getushort();
			ret.Put((int) ppid);// = _body.getInt();
			ret.Put(_data, _dataOffset, _dataLength);
		}

		/**
		 * @param _streamId the _streamId to set
		 */
		public void setStreamId(int streamId) {
			_streamId = streamId;
		}

		public DataChunk mkAck(DCOpen dcep) {
			DataChunk ack = new DataChunk();
			ack.setData(dcep.mkAck());
			ack.ppid = SCTP_PPID.WEBRTCCONTROL;
			ack.setFlags(DataChunk.SINGLEFLAG);

			return ack;
		}

		public static DataChunk mkDCOpen(string label) {
			DataChunk open = new DataChunk();
			DCOpen dope = new DCOpen(label);
			open.setData(dope.getBytes());
			open.ppid = SCTP_PPID.WEBRTCCONTROL;
			open.setFlags(DataChunk.SINGLEFLAG);
			return open;
		}

		/*
		public DataChunk(string s) {
			this();
			_data = s.getBytes();
			_ppid = WEBRTCstring;
		}
		 */
		public override string ToString() {
			string ret = base.ToString();
			ret += " ppid = " + ppid + "seqn " + sSeqNo + " streamId " + _streamId + " tsn " + tsn
					+ " retry " + _retryTime + " gap acked " + _gapAck;
			return ret;
		}

		public void setFlags(int flag) {
			_flags = (byte) flag;
		}

		public int getFlags() {
			return _flags;
		}

		public virtual int getCapacity() {
			return 1024; // shrug - needs to be less than the theoretical MTU or slow start fails.
		}
		public static int GetCapacity() {
			return 1024; // shrug - needs to be less than the theoretical MTU or slow start fails.
		}

		public void setData(byte[] data) {
			_data = data;
			_dataLength = data.Length;
			_dataOffset = 0;
		}

		/**
		 * Only use this method if you are certain that data won't be reused until
		 * this chunk is sent and ack'd ie after MessageCompleteHandler has been
		 * called for the surrounding message
		 */
		public void setData(byte[] data, int offs, int len) {
			_data = data;
			_dataLength = len;
			_dataOffset = offs;
		}

		public void setGapAck(bool b) {
			_gapAck = b;
		}

		public bool getGapAck() {
			return _gapAck;
		}

		public void setRetryTime(long l) {
			_retryTime = l;
			_retryCount++;
		}

		public long getRetryTime() {
			return _retryTime;
		}

		public int CompareTo(DataChunk o) {
			return Compare(this, o);
		}

		public int Compare(DataChunk o1, DataChunk o2) {
			return (int) (o1.tsn - o2.tsn);
		}

		public long getSentTime() {
			return _sentTime;
		}

		public void setSentTime(long now) {
			_sentTime = now;
		}
	}
}
