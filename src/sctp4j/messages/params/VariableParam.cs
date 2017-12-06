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
	public struct Param {
		public ByteBuffer data;
		public VariableParamType type;

		public Param(VariableParamType t) {
			type = t;
			data = null;
		}

		public void readBody(ref ByteBuffer b, int len) {
			data = b.slice();
			b.Length = len;
		}

		public void writeBody(ref ByteBuffer b) {
			b.Put(data.Data, data.offset, data.Length);
		}
	}

	public enum VariableParamType {
		/*
		1	Heartbeat Info	[RFC4960]
		5	IPv4 Address	[RFC4960]
		6	IPv6 Address	[RFC4960]
		7	State Cookie	[RFC4960]
		8	Unrecognized Parameters	[RFC4960]
		9	Cookie Preservative	[RFC4960]
		11	Host Name Address	[RFC4960]
		12	Supported Address Types	[RFC4960]
		13	Outgoing SSN Reset Request Parameter	[RFC6525]
		14	Incoming SSN Reset Request Parameter	[RFC6525]
		15	SSN/TSN Reset Request Parameter	[RFC6525]
		16	Re-configuration Response Parameter	[RFC6525]
		17	Add Outgoing Streams Request Parameter	[RFC6525]
		18	Add Incoming Streams Request Parameter	[RFC6525]
		32768	Reserved for ECN Capable (0x8000)	
		32770	Random (0x8002)	[RFC4805]
		32771	Chunk List (0x8003)	[RFC4895]
		32772	Requested HMAC Algorithm Parameter (0x8004)	[RFC4895]
		32773	Padding (0x8005)	
		32776	Supported Extensions (0x8008)	[RFC5061]
		49152	Forward TSN supported (0xC000)	[RFC3758]
		49153	Add IP Address (0xC001)	[RFC5061]
		49154	Delete IP Address (0xC002)	[RFC5061]
		49155	Error Cause Indication (0xC003)	[RFC5061]
		49156	Set Primary Address (0xC004)	[RFC5061]
		49157	Success Indication (0xC005)	[RFC5061]
		49158	Adaptation Layer Indication (0xC006)	[RFC5061]
		*/
		Unknown = -1,
		HeartbeatInfo = 1,
		IPv4Address = 5,
		IPv6Address = 6,
		StateCookie = 7,
		UnrecognizedParameters = 8,
		CookiePreservative = 9,
		HostNameAddress = 11,
		SupportedAddressTypes = 12,
		OutgoingSSNResetRequestParameter = 13,
		IncomingSSNResetRequestParameter = 14,
		SSNTSNResetRequestParameter = 15,
		ReconfigurationResponseParameter = 16,
		AddOutgoingStreamsRequestParameter = 17,
		AddIncomingStreamsRequestParameter = 18,
		ReservedforECNCapable = 32768,
		Random = 32770,
		ChunkList = 32771,
		RequestedHMACAlgorithmParameter = 32772,
		Padding = 32773,
		SupportedExtensions = 32776,
		ForwardTSNsupported = 49152,
		AddIPAddress = 49153,
		DeleteIPAddress = 49154,
		ErrorCauseIndication = 49155,
		SetPrimaryAddress = 49156,
		SuccessIndication = 49157,
		AdaptationLayerIndication = 49158,



		InvalidStreamIdentifier = 1, //[RFC4960]
		MissingMandatoryParameter = 2, //[RFC4960]
		StaleCookieError = 3, //[RFC4960]
		OutofResource = 4, //[RFC4960]
		UnresolvableAddress = 5, //[RFC4960]
		UnrecognizedChunkType = 6, //[RFC4960]
		InvalidMandatoryParameter = 7, //[RFC4960]
		//UnrecognizedParameters = 8, //[RFC4960]
		NoUserData = 9, //[RFC4960]
		CookieReceivedWhileShuttingDown = 10, //[RFC4960]
		RestartofanAssociationwithNewAddresses = 11, //[RFC4960]
		UserInitiatedAbort = 12, //[RFC4460]
		ProtocolViolation = 13, //[RFC4460]
		RequesttoDeleteLastRemainingIPAddress = 160, //[RFC5061]
		OperationRefusedDuetoResourceShortage = 161, //[RFC5061]
		RequesttoDeleteSourceIPAddress = 162, //[RFC5061]
		AssociationAbortedduetoillegalASCONF_ACK = 163, //[RFC5061]
		Requestrefused_noauthorization = 164, //[RFC5061]
		UnsupportedHMACIdentifier = 261, //[RFC4895]
	}
}
