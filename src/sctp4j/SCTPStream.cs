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
using pe.pi.sctp4j.sctp.behave;
using pe.pi.sctp4j.sctp.messages;
using System;

/**
 *
 * @author Westhawk Ltd<thp@westhawk.co.uk>
 */
namespace pe.pi.sctp4j.sctp {
	public abstract class SCTPStream {
		/* unfortunately a webRTC SCTP stream can change it's reliability rules etc post creation
		 so we can't encapsulate the streams into multiple implementations of the same interface/abstract
		 So what we do is put the bulk of the stream code here, then delegate the variant rules off to the
		 behave class - which has to be stateless since it can be swapped out - it is ugly 
		 - and I wonder if closures would do it better.
		 */
		private SCTPStreamBehaviour _behave;
		protected Association _ass;
		private int _sno;
		private string _label;
		private SortedArray<DataChunk> _stash;
		private SCTPStreamListener _sl;
		private int _nextMessageSeqIn;
		private int _nextMessageSeqOut;
		private bool closing;
		private State state = State.OPEN;

		public bool InboundIsOpen() {
			return ((state == State.OPEN) || (state == State.INBOUNDONLY));
		}

		public bool OutboundIsOpen() {
			return ((state == State.OPEN) || (state == State.OUTBOUNDONLY));
		}

		internal Chunk immediateClose() {
			Chunk ret = null;
			try {
				ret = _ass.addToCloseList(this);
			}
			catch (Exception ex) {
				Logger.Error("Can't make immediate close for " + this._sno + " because " + ex.ToString());
			}
			return ret;
		}

		abstract internal void delivered(DataChunk d);

		enum State {
			CLOSED, INBOUNDONLY, OUTBOUNDONLY, OPEN
		}

		public SCTPStream(Association a, int id) {
			_ass = a;
			_sno = id;
			_stash = new SortedArray<DataChunk>(); // sort bt tsn
			_behave = new OrderedStreamBehaviour(); // default 'till we know different
		}

		public void setLabel(string l) {
			_label = l;
		}

		public int getNum() {
			return _sno;
		}

		public override string ToString() {
			return "Stream (" + _sno + ") label:" + _label + " state:" + state + " behave:" + _behave.GetType().Name;
		}

		internal Chunk[] append(DataChunk dc) {
			Logger.Debug("adding data to stash on stream " + _label + "(" + dc + ")");
			_stash.Add(dc);
			return _behave.respond(this);
		}

		/**
		 * note that behaviours must be stateless - since they can be swapped out
		 * when we finally get the 'open'
		 *
		 * @param behave
		 */
		internal void setBehave(SCTPStreamBehaviour behave) {
			_behave = behave;
		}

		// seqno management.
		/**
		 * annotate the outgoing chunk with stuff this stream knows.
		 *
		 * @param chunk
		 */
		internal void outbound(DataChunk chunk) {
			chunk.setStreamId(_sno);
			// roll seqno here.... hopefully....
		}

		internal void inbound(DataChunk dc) {
			if (_behave != null) {
				_behave.deliver(this, _stash, _sl);
			} else {
				Logger.Warn("No behaviour set");
			}
		}

		public string getLabel() {
			return _label;
		}

		public int stashCap() {
			int ret = 0;
			foreach (DataChunk d in _stash) {
				ret += d.getData().Length;
			}
			return ret;
		}

		internal void setSCTPStreamListener(SCTPStreamListener sl) {
			_sl = sl;
			Logger.Debug("action a delayed delivery now we have a listener.");
			//todo think about what reliablility looks like here.
			_behave.deliver(this, _stash, _sl);
		}

		abstract public void send(string message);

		abstract public void send(byte[] message, int offset, int length);

		public Association getAssociation() {
			return _ass;
		}

		public void close() {
			Logger.Debug("closing stream " + this._label + " " + this._sno);
			_ass.closeStream(this);
		}

		internal void setNextMessageSeqIn(int expectedSeq) {
			_nextMessageSeqIn = (expectedSeq == 1 + ushort.MaxValue) ? 0 : expectedSeq;
		}

		internal int getNextMessageSeqIn() {
			return _nextMessageSeqIn;
		}

		internal void setNextMessageSeqOut(int expectedSeq) {
			_nextMessageSeqOut = (expectedSeq == 1 + ushort.MaxValue) ? 0 : expectedSeq;
		}

		internal int getNextMessageSeqOut() {
			return _nextMessageSeqOut;
		}

		abstract internal void deliverMessage(SCTPMessage message);

		public void setDeferred(bool b) {
			bool deferred = true;
		}

		public void reset() {
			Logger.Debug("Resetting stream " + this._sno);
			if (this._sl != null) {
				_sl.close(this);
			}
		}

		public void setClosing(bool b) {
			closing = b;
		}

		bool isClosing() {
			return closing;
		}

		void setOutboundClosed() {
			switch (state) {
				case State.OPEN:
					state = State.INBOUNDONLY;
					break;
				case State.OUTBOUNDONLY:
					state = State.CLOSED;
					break;
				case State.CLOSED:
				case State.INBOUNDONLY:
					break;
			}
			Logger.Debug("Stream State for " + _sno + " is now " + state);
		}

		void setInboundClosed() {
			switch (state) {
				case State.OPEN:
					state = State.OUTBOUNDONLY;
					break;
				case State.INBOUNDONLY:
					state = State.CLOSED;
					break;
				case State.CLOSED:
				case State.OUTBOUNDONLY:
					break;
			}
			Logger.Debug("Stream State for " + _sno + " is now " + state);
		}

		State getState() {
			Logger.Debug("Stream State for " + _sno + " is currently " + state);
			return state;
		}

		public virtual bool idle() {
			return true;
		}
	}
}
