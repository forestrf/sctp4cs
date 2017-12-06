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
using pe.pi.sctp4j.sctp.messages;
using pe.pi.sctp4j.sctp.messages.Params;
using System.Collections.Generic;

/**
 *
 * @author thp
 */
namespace pe.pi.sctp4j.sctp {
	class ReconfigState {
		ReConfigChunk recentInbound = null;
		ReConfigChunk recentOutboundRequest = null;
		ReConfigChunk sentReply = null;
		bool timerRunning = false;
		uint nearSeqno = 0;
		uint farSeqno = 0;
		Association assoc;
		Queue<SCTPStream> listOfStreamsToReset;

		public ReconfigState(Association a, uint farTSN) {
			nearSeqno = a.getNearTSN();
			farSeqno = farTSN;
			assoc = a;
			listOfStreamsToReset = new Queue<SCTPStream>();
		}

		private bool haveSeen(ReConfigChunk rconf) {
			return rconf.sameAs(recentInbound);
		}

		private ReConfigChunk getPrevious(ReConfigChunk rconf) {
			return rconf.sameAs(recentInbound) ? sentReply : null;
		}

		private bool timerIsRunning() {
			return timerRunning;
		}

		private void markAsAcked(ReConfigChunk rconf) {
			// ooh, what does this button do ??? To Do
		}

		private uint nextNearNo() {
			return (uint) nearSeqno++;
		}

		private uint nextFarNo() {
			return (uint) farSeqno++;
		}

		public uint nextDue() {
			return 1000;
		}

		/*
		 * https://tools.ietf.org/html/rfc6525
		 */
		public Chunk[] deal(ReConfigChunk rconf) {
			Chunk[] ret = new Chunk[1];
			ReConfigChunk reply = null;
			Logger.Debug("Got a reconfig message to deal with");
			if (haveSeen(rconf)) {
				// if not - is this a repeat
				reply = getPrevious(rconf); // then send the same reply
			}
			if (reply == null) {
				// not a repeat then
				reply = new ReConfigChunk(); // create a new thing
				Param oresetParam;
				if (rconf.GetVar(VariableParamType.OutgoingSSNResetRequestParameter, out oresetParam)) {
					var oreset = new OutgoingSSNResetRequestParameter(ref oresetParam);
					int[] streams = oreset.streams;
					if (streams.Length == 0) {
						streams = assoc.allStreams();
					}
					if (timerIsRunning()) {
						markAsAcked(rconf);
					}
					// if we are behind, we are supposed to wait untill we catch up.
					Param p = new Param(VariableParamType.ReconfigurationResponseParameter);
					if (oreset.lastTsn > assoc.getCumAckPt()) {
						Logger.Debug("Last assigned > farTSN " + oreset.lastTsn + " v " + assoc.getCumAckPt());
						ReconfigurationResponseParameter rep = new ReconfigurationResponseParameter(
							oreset.reqSeqNo, ReconfigurationResponseParameter.STATUS.IN_PROGRESS,
							false, 0, 0);
						rep.writeBody(ref p.data);
						reply.addParam(ref p);
					} else {
						// somehow invoke this when TSN catches up ?!?! ToDo
						Logger.Debug("we are up-to-date ");
						ReconfigurationResponseParameter.STATUS result = streams.Length > 0 ? ReconfigurationResponseParameter.STATUS.SUCCESS_PERFORMED : ReconfigurationResponseParameter.STATUS.SUCCESS_NOTHING_TO_DO;
						foreach (int s in streams) {
							SCTPStream cstrm = assoc.delStream(s);
							if (cstrm == null) {
								Logger.Error("Close a non existant stream");
								result = ReconfigurationResponseParameter.STATUS.ERROR_WRONG_SSN;
								break;
								// bidriectional might be a problem here...
							} else {
								cstrm.reset();
							}
						}

						ReconfigurationResponseParameter rep = new ReconfigurationResponseParameter(
							oreset.reqSeqNo, result, false, 0, 0);
						rep.writeBody(ref p.data);
						reply.addParam(ref p);
					}
				}
				// ponder putting this in a second chunk ?
				Param iresetParam;
				if (rconf.GetVar(VariableParamType.IncomingSSNResetRequestParameter, out iresetParam)) {
					var ireset = new IncomingSSNResetRequestParameter(ref iresetParam);
					/*The Re-configuration
					Response Sequence Number of the Outgoing SSN Reset Request
					Parameter MUST be the Re-configuration Request Sequence Number
					of the Incoming SSN Reset Request Parameter. */
					int[] streams = ireset.streams;
					OutgoingSSNResetRequestParameter rep = new OutgoingSSNResetRequestParameter(nextNearNo(), ireset.reqSeqNo, assoc.getNearTSN(), streams);
					if (streams.Length == 0) {
						streams = assoc.allStreams();
					}
					foreach (int s in streams) {
						SCTPStream st = assoc.getStream(s);
						if (st != null) {
							st.setClosing(true);
						}
					}
					Param param = rep.ToParam();
					reply.addParam(ref param);
					// set outbound timer running here ???
					Logger.Debug("Ireset " + ireset);
				}
			}
			if (reply.hasParam()) {
				ret[0] = reply;
				// todo should add sack here
				Logger.Debug("about to reply with " + reply.ToString());
			} else {
				ret = null;
			}
			return ret;
		}

		/* we can only demand they close their outbound streams */
		/* we can request they start to close inbound (ie ask us to shut our outbound */
		/* DCEP treats streams as bi-directional - so this is somewhat of an inpedance mis-match */
		/* resulting in a temporary 'half closed' state */
		/* mull this over.... */
		public ReConfigChunk makeClose(SCTPStream st) {
			ReConfigChunk ret = null;
			Logger.Debug("building reconfig so close stream " + st);
			st.setClosing(true);
			lock (listOfStreamsToReset) {
				listOfStreamsToReset.Enqueue(st);
			}
			if (!timerIsRunning()) {
				ret = makeSSNResets();
			}
			return ret;
		}

		private ReConfigChunk makeSSNResets() {
			ReConfigChunk reply = new ReConfigChunk(); // create a new thing
			Logger.Debug("closing streams n=" + listOfStreamsToReset.Count);
			List<int> streamsL = new List<int>();
			lock (listOfStreamsToReset) {
				foreach (var s in listOfStreamsToReset) if (s.InboundIsOpen()) streamsL.Add(s.getNum());
			}
			int[] streams = streamsL.ToArray();
			if (streams.Length > 0) {
				OutgoingSSNResetRequestParameter rep = new OutgoingSSNResetRequestParameter(nextNearNo(), farSeqno - 1, assoc.getNearTSN(), streams);
				Param param = rep.ToParam();
				reply.addParam(ref param);
			}
			streamsL.Clear();
			lock (listOfStreamsToReset) {
				foreach (var s in listOfStreamsToReset) if (s.OutboundIsOpen()) streamsL.Add(s.getNum());
			}
			streams = streamsL.ToArray();
			if (streams.Length > 0) {
				IncomingSSNResetRequestParameter rep = new IncomingSSNResetRequestParameter(nextNearNo(), streams);
				var param = rep.ToParam();
				reply.addParam(ref param);
			}
			Logger.Debug("reconfig chunk is " + reply.ToString());
			return reply;
		}
	}
}
