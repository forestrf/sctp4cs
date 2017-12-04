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
using pe.pi.sctp4j.sctp.messages;
/**
 *
 * @author tim
 * what DCEPS do 
 */
namespace pe.pi.sctp4j.sctp.behave {
	internal class DCEPStreamBehaviour : SCTPStreamBehaviour {

		public Chunk[] respond(SCTPStream a) {
			Logger.Debug("in respond() for a opened stream " + a.getLabel());
			return null;
		}

		public void deliver(SCTPStream s, SortedArray<DataChunk> a, SCTPStreamListener l) {
			Logger.Debug("in deliver() for stream " + s.getLabel() + " with " + a.Count + " chunks. ");
			// strictly this should be looking at flags etc, and bundling the result into a message
			foreach (DataChunk dc in a) {
				if (dc.getDCEP() != null) {
					Logger.Debug("in deliver() for a DCEP message " + dc.getDataAsString());
				} else {
					Logger.Debug("inbound data chunk is " + dc.ToString());
					l.onMessage(s, dc.getDataAsString());
				}
			}
			a.Clear();
		}
	}
}
