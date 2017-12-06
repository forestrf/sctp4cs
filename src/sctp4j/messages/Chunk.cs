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
using System.Collections.Generic;
using System;

/**
 *
 * @author Westhawk Ltd<thp@westhawk.co.uk>
 */
namespace pe.pi.sctp4j.sctp.messages {
	internal abstract class Chunk {
		/*
		 0                   1                   2                   3
		 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 |   Chunk Type  | Chunk  Flags  |        Chunk Length           |
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 \                                                               \
		 /                          Chunk Value                          /
		 \                                                               \
		 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
		 */

		public enum CType : byte {
			DATA = 0,
			INIT = 1,
			INITACK = 2,
			SACK = 3,
			HEARTBEAT = 4,
			HEARTBEAT_ACK = 5,
			ABORT = 6,
			SHUTDOWN = 7,
			SHUTDOWN_ACK = 8,
			ERROR = 9,
			COOKIE_ECHO = 10,
			COOKIE_ACK = 11,
			ECNE = 12,
			CWR = 13,
			SHUTDOWN_COMPLETE = 14,
			AUTH = 15,
			PKTDROP = 129,
			RE_CONFIG = 130,
			FORWARDTSN = 192,
			ASCONF = 193,
			ASCONF_ACK = 128,
		}



		public const byte TBIT = 1;

		/*
		   Chunk Length: 16 bits (unsigned integer)

		  This value represents the size of the chunk in bytes, including
		  the Chunk Type, Chunk Flags, Chunk Length, and Chunk Value fields.
		  Therefore, if the Chunk Value field is zero-length, the Length
		  field will be set to 4.  The Chunk Length field does not count any
		  chunk padding.
		*/
		public static Chunk mkChunk(ref ByteBuffer pkt) {
			Chunk ret = null;
			if (pkt.remaining() >= 4) {
				CType type = (CType) pkt.GetByte();
				byte flags = pkt.GetByte();
				int length = pkt.GetUShort();
				switch (type) {
					case CType.DATA:
						ret = new DataChunk(type, flags, length, ref pkt);
						break;
					case CType.INIT:
						ret = new InitChunk(type, flags, length, ref pkt);
						break;
					case CType.SACK:
						ret = new SackChunk(type, flags, length, ref pkt);
						break;
					case CType.INITACK:
						ret = new InitAckChunk(type, flags, length, ref pkt);
						break;
					case CType.COOKIE_ECHO:
						ret = new CookieEchoChunk(type, flags, length, ref pkt);
						break;
					case CType.COOKIE_ACK:
						ret = new CookieAckChunk(type, flags, length, ref pkt);
						break;
					case CType.ABORT:
						ret = new AbortChunk(type, flags, length, ref pkt);
						break;
					case CType.HEARTBEAT:
						ret = new HeartBeatChunk(type, flags, length, ref pkt);
						break;
					case CType.RE_CONFIG:
						ret = new ReConfigChunk(type, flags, length, ref pkt);
						break;
					default:
						Logger.Warn("Default chunk type " + type + " read in ");
						ret = new FailChunk(type, flags, length, ref pkt);
						break;
				}
				if (ret != null) {
					if (pkt.hasRemaining()) {
						int mod = ret.getLength() % 4;
						if (mod != 0) {
							for (int pad = mod; pad < 4; pad++) {
								pkt.GetByte();
							}
						}
					}
				}
			}
			return ret;
		}
		/*
		 0          - Payload Data (DATA)
		 1          - Initiation (INIT)
		 2          - Initiation Acknowledgement (INIT ACK)
		 3          - Selective Acknowledgement (SACK)
		 4          - Heartbeat Request (HEARTBEAT)
		 5          - Heartbeat Acknowledgement (HEARTBEAT ACK)
		 6          - Abort (ABORT)
		 7          - Shutdown (SHUTDOWN)
		 8          - Shutdown Acknowledgement (SHUTDOWN ACK)
		 9          - Operation Error (ERROR)
		 10         - State Cookie (COOKIE ECHO)
		 11         - Cookie Acknowledgement (COOKIE ACK)




		 Stewart                     Standards Track                    [Page 17]

		 RFC 4960          Stream Control Transmission Protocol    September 2007


		 12         - Reserved for Explicit Congestion Notification Echo
		 (ECNE)
		 13         - Reserved for Congestion Window Reduced (CWR)
		 14         - Shutdown Complete (SHUTDOWN COMPLETE)
		 */
		/*
    
		 Chunk Type  Chunk Name
		 --------------------------------------------------------------
		 0xC1    Address Configuration Change Chunk        (ASCONF)
		 0x80    Address Configuration Acknowledgment      (ASCONF-ACK)
    
		 +------------+------------------------------------+
		 | Chunk Type | Chunk Name                         |
		 +------------+------------------------------------+
		 | 130        | Re-configuration Chunk (RE-CONFIG) |
		 +------------+------------------------------------+
    
		 The following new chunk type is defined:

		 Chunk Type    Chunk Name
		 ------------------------------------------------------
		 192 (0xC0)    Forward Cumulative TSN (FORWARD TSN)
     
    
		 Chunk Type  Chunk Name
		 --------------------------------------------------------------
		 0x81    Packet Drop Chunk        (PKTDROP)
		 */

		public CType _type;
		public byte _flags;
		int _length;
		protected ByteBuffer _body;
		internal List<Param> _varList = new List<Param>();


		protected Chunk(CType type) {
			_type = type;
		}

		protected Chunk(CType type, byte flags, int length, ref ByteBuffer pkt) {
			_type = type;
			_flags = flags;
			_length = length;
			/* Copy version 
			byte[] bb = new byte[length -4]; 
			pkt[bb];
			_body = MemoryStream.wrap(bb);
			*/
			// or use same data but different MemoryStreams wrapping it
			_body = pkt.slice();
			_body.Length = length - 4;
			pkt.Position += (length - 4);
		}
		// sad ommission in MemoryStream 

		public void write(ref ByteBuffer ret) {
			ret.Put((byte) _type);
			ret.Put((byte) _flags);
			ret.Put((ushort) 4); // marker for length;
			putFixedParams(ref ret);
			int pad = 0;
			if (_varList != null) {
				foreach (Param v in this._varList) {
					Logger.Debug("var " + Enum.GetName(typeof(VariableParamType), v.type) + " at " + ret.Position);

					ByteBuffer var = ret.slice();
					var.Put((ushort) v.type);
					var.Put((ushort) 4); // length holder.
					v.writeBody(ref var);
					var.Put(2, (ushort) var.Position);
					Logger.Trace("setting var length to " + var.Position);
					pad = var.Position % 4;
					pad = (pad != 0) ? 4 - pad : 0;
					Logger.Trace("padding by " + pad);
					ret.Position += var.Position + pad;
				}
			}
			//Console.WriteLine("un padding by " + pad);
			ret.Position -= pad;
			// and push the new length into place.
			ret.Put(2, (ushort) ret.Position);
			//Console.WriteLine("setting chunk length to " + ret.position());
		}

		public CType getType() {
			return _type;
		}

		protected int getLength() {
			return _length;
		}

		protected Param readVariable() {
			VariableParamType type = (VariableParamType) _body.GetUShort();
			int len = _body.GetUShort();
			int blen = len - 4;
			Param var;
			switch (type) {
				case VariableParamType.RequestedHMACAlgorithmParameter:
				case VariableParamType.ReconfigurationResponseParameter:
				case VariableParamType.SupportedAddressTypes:
				case VariableParamType.CookiePreservative:
				case VariableParamType.UnrecognizedParameters:
				case VariableParamType.ChunkList:
				case VariableParamType.SupportedExtensions:
				case VariableParamType.IncomingSSNResetRequestParameter:
				case VariableParamType.IPv4Address:
				case VariableParamType.IPv6Address:
				case VariableParamType.StateCookie:
				case VariableParamType.HostNameAddress:
				case VariableParamType.SSNTSNResetRequestParameter:
				case VariableParamType.OutgoingSSNResetRequestParameter:
				case VariableParamType.Random:
				case VariableParamType.HeartbeatInfo:
				case VariableParamType.ForwardTSNsupported:
				case VariableParamType.AddOutgoingStreamsRequestParameter:
				case VariableParamType.AddIncomingStreamsRequestParameter:
				case VariableParamType.ReservedforECNCapable:
				case VariableParamType.Padding:
				case VariableParamType.AddIPAddress:
				case VariableParamType.DeleteIPAddress:
				case VariableParamType.ErrorCauseIndication:
				case VariableParamType.SetPrimaryAddress:
				case VariableParamType.SuccessIndication:
				case VariableParamType.AdaptationLayerIndication:
					var = new Param(type);
					break;
				default:
					var = new Param(VariableParamType.Unknown);
					break;
			}
			try {
				var.readBody(ref _body, blen);
				Logger.Debug("variable type " + var.type + " name " + Enum.GetName(typeof(VariableParamType), var.type));
			}
			catch (SctpPacketFormatException ex) {
				Logger.Error(ex.ToString());
			}
			if (_body.hasRemaining()) {
				int mod = blen % 4;
				if (mod != 0) {
					for (int pad = mod; pad < 4; pad++) {
						_body.GetByte();
					}
				}
			}
			return var;
		}

		protected Param readErrorParam() {
			VariableParamType type = (VariableParamType) _body.GetUShort();
			int len = _body.GetUShort();
			int blen = len - 4;
			Param var = new Param();
			switch (type) {
				case VariableParamType.InvalidStreamIdentifier:
				case VariableParamType.MissingMandatoryParameter:
				case VariableParamType.OutofResource:
				case VariableParamType.UnresolvableAddress:
				case VariableParamType.UnrecognizedChunkType:
				case VariableParamType.InvalidMandatoryParameter:
				case VariableParamType.UnrecognizedParameters:
				case VariableParamType.NoUserData:
				case VariableParamType.CookieReceivedWhileShuttingDown:
				case VariableParamType.RestartofanAssociationwithNewAddresses:
				case VariableParamType.UserInitiatedAbort:
				case VariableParamType.ProtocolViolation:
				case VariableParamType.RequesttoDeleteLastRemainingIPAddress:
				case VariableParamType.OperationRefusedDuetoResourceShortage:
				case VariableParamType.RequesttoDeleteSourceIPAddress:
				case VariableParamType.AssociationAbortedduetoillegalASCONF_ACK:
				case VariableParamType.Requestrefused_noauthorization:
				case VariableParamType.UnsupportedHMACIdentifier:
				case VariableParamType.StaleCookieError:
					var = new Param(type);
					break;
			}
			try {
				var.readBody(ref _body, blen);
				Logger.Trace("variable type " + var.type + " name " + Enum.GetName(typeof(VariableParamType), var.type));
				Logger.Trace("additional info " + var.ToString());
			}
			catch (SctpPacketFormatException ex) {
				Logger.Error(ex.ToString());
			}
			if (_body.hasRemaining()) {
				int mod = blen % 4;
				if (mod != 0) {
					for (int pad = mod; pad < 4; pad++) {
						_body.GetByte();
					}
				}
			}
			return var;
		}

		protected abstract void putFixedParams(ref ByteBuffer ret);

		public virtual void validate() { // todo be more specific in the Exception tree
										 // throw new Exception("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
		}
	}
}
