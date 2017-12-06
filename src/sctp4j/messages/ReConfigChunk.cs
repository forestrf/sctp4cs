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
 * @author thp
 */
namespace pe.pi.sctp4j.sctp.messages {
	internal class ReConfigChunk : Chunk {

		private long sentAt;
		private int retries;

		public ReConfigChunk(CType type, byte flags, int length, ref ByteBuffer pkt)
			: base(type, flags, length, ref pkt) {
			Logger.Debug("ReConfig chunk" + this.ToString());
			if (_body.remaining() >= 4) {
				while (_body.hasRemaining()) {
					Param v = readVariable();
					_varList.Add(v);
					Logger.Debug("\tParam :" + v.ToString());
				}
			}
		}

		public ReConfigChunk() : base(CType.RE_CONFIG) { }

		protected override void putFixedParams(ref ByteBuffer ret) {
			//throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
		}

		public bool hasIncomingReset() {
			foreach (var v in _varList)
				if (v.type == VariableParamType.IncomingSSNResetRequestParameter)
					return true;
			return false;
		}

		public bool GetVar(VariableParamType type, out Param p) {
			foreach (var v in _varList) {
				if (v.type == type) {
					p = v;
					return true;
				}
			}
			p = default(Param);
			return false;
		}

		private bool hasResponse() {
			foreach (var v in _varList)
				if (typeof(ReconfigurationResponseParameter).IsAssignableFrom(v.GetType()))
					return true;
			return false;
		}

		public bool hasParam() {
			return _varList.Count > 0;
		}

		/*
		   1.   Outgoing SSN Reset Request Parameter.

	   2.   Incoming SSN Reset Request Parameter.

	   3.   Outgoing SSN Reset Request Parameter, Incoming SSN Reset Request
			Parameter.

	   4.   SSN/TSN Reset Request Parameter.

	   5.   Add Outgoing Streams Request Parameter.

	   6.   Add Incoming Streams Request Parameter.

	   7.   Add Outgoing Streams Request Parameter, Add Incoming Streams
			Request Parameter.

	   8.   Re-configuration Response Parameter.

	   9.   Re-configuration Response Parameter, Outgoing SSN Reset Request
			Parameter.

	   10.  Re-configuration Response Parameter, Re-configuration Response
			Parameter.
		 */
		public override void validate() {
			Param ignore;

			if (_varList.Count < 1) {
				throw new Exception("[IllegalArgumentException] Too few params " + _varList.Count);
			}
			if (_varList.Count > 2) {
				throw new Exception("[IllegalArgumentException] Too many params " + _varList.Count);
			}
			// now check for invalid combos
			if ((_varList.Count == 2)) {
				if (this.GetVar(VariableParamType.OutgoingSSNResetRequestParameter, out ignore)) {
					bool failed = true;

					foreach (var v in _varList) {
						if (v.type != VariableParamType.OutgoingSSNResetRequestParameter) {
							if (v.type != VariableParamType.IncomingSSNResetRequestParameter //3
								&& v.type != VariableParamType.ReconfigurationResponseParameter) { //9
								throw new Exception("[IllegalArgumentException] OutgoingSSNResetRequestParameter and " + v.type + " in same Chunk not allowed ");
							}
							failed = false;
							break;
						}
					}
					if (failed) {
						throw new Exception("[IllegalArgumentException] 2 OutgoingSSNResetRequestParameter in one Chunk not allowed ");
					}
				} else if (GetVar(VariableParamType.AddOutgoingStreamsRequestParameter, out ignore)) {
					bool failed = true;

					foreach (var v in _varList) {
						if (v.type != VariableParamType.AddOutgoingStreamsRequestParameter) {
							if (v.type != VariableParamType.AddIncomingStreamsRequestParameter) { //7
								throw new Exception("[IllegalArgumentException] OutgoingSSNResetRequestParameter and " + v.type + " in same Chunk not allowed ");
							}
							failed = false;
							break;
						}
					}
					if (failed) {
						throw new Exception("[IllegalArgumentException] 2 AddOutgoingStreamsRequestParameter in one Chunk not allowed ");
					}
				} else if (this.hasResponse()) {
					foreach (var v in _varList) {
						if (v.type != VariableParamType.ReconfigurationResponseParameter) { // 10
							throw new Exception("[IllegalArgumentException] ReconfigurationResponseParameter and " + v.type + " in same Chunk not allowed ");
							break;
						}
					}

				}
			} // implicitly just one - which is ok 1,2,4,5,6,8
		}

		public void addParam(ref Param rep) {
			Logger.Debug("adding " + rep + " to " + this);
			_varList.Add(rep);
			validate();
		}



		public bool sameAs(ReConfigChunk other) {
			// we ignore other var types for now....
			bool ret = false; // assume the negative.
			if (other != null) {
				// if there are 2 params and both match, or if there is only one (of these) params and matches
				Param thisInc, otherInc;
				bool incomings = false;
				if (GetVar(VariableParamType.IncomingSSNResetRequestParameter, out thisInc)
					&& other.GetVar(VariableParamType.IncomingSSNResetRequestParameter, out otherInc)) {
					incomings = true;
					ret = IncomingSSNResetRequestParameter.Compare(ref thisInc, ref otherInc);
				}

				Param thisOut, otherOut;
				if (GetVar(VariableParamType.OutgoingSSNResetRequestParameter, out thisOut)
					&& other.GetVar(VariableParamType.OutgoingSSNResetRequestParameter, out otherOut)) {
					ret = (incomings ? ret : true) && OutgoingSSNResetRequestParameter.Compare(ref thisOut, ref otherOut);
				}
			}
			return ret;
		}
		// stuff to manage outbound retries
		public long getSentTime() {
			return sentAt;
		}
		public void setSentTime(long now) {
			sentAt = now;
		}
		public int getAndIncrementRetryCount() {
			return retries++;
		}
	}
}
