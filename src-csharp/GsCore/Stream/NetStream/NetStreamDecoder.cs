using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.NetStream
{
/*
 * This file is part of GraphStream <http://graphstream-project.org>.
 * 
 * GraphStream is a library whose purpose is to handle static or dynamic
 * graph, create them from scratch, file or any source and display them.
 * 
 * This program is free software distributed under the terms of two licenses, the
 * CeCILL-C license that fits European law, and the GNU Lesser General Public
 * License. You can  use, modify and/ or redistribute the software under the terms
 * of the CeCILL-C license as circulated by CEA, CNRS and INRIA at the following
 * URL <http://www.cecill.info> or under the terms of the GNU LGPL as published by
 * the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * The fact that you are presently reading this means that you have had
 * knowledge of the CeCILL-C and LGPL licenses and that you accept their terms.
 */



public class NetStreamDecoder : SourceBase, ByteDecoder {
	private static readonly object /* Logger */ LOGGER = null /* Logger */;

	
	public bool validate(byte[] buffer) {
		if (buffer.position() >= 4) {
			int size = buffer.getInt(0);
			return buffer.position() >= size;
		}

		return false;
	}

	
	public void decode(byte[] bb) {
		try {
			int size = bb.ReadByte() /* getInt */;
			string streamId = NetStreamUtils.decodeString(bb);
			int cmd = bb[];

			if (cmd == NetStreamConstants.EVENT_ADD_NODE) {
				serve_EVENT_ADD_NODE(bb);
			} else if ((cmd & 0xFF) == (NetStreamConstants.EVENT_DEL_NODE & 0xFF)) {
				serve_DEL_NODE(bb);
			} else if (cmd == NetStreamConstants.EVENT_ADD_EDGE) {
				serve_EVENT_ADD_EDGE(bb);
			} else if (cmd == NetStreamConstants.EVENT_DEL_EDGE) {
				serve_EVENT_DEL_EDGE(bb);
			} else if (cmd == NetStreamConstants.EVENT_STEP) {
				serve_EVENT_STEP(bb);
			} else if (cmd == NetStreamConstants.EVENT_CLEARED) {
				serve_EVENT_CLEARED(bb);
			} else if (cmd == NetStreamConstants.EVENT_ADD_GRAPH_ATTR) {
				serve_EVENT_ADD_GRAPH_ATTR(bb);
			} else if (cmd == NetStreamConstants.EVENT_CHG_GRAPH_ATTR) {
				serve_EVENT_CHG_GRAPH_ATTR(bb);
			} else if (cmd == NetStreamConstants.EVENT_DEL_GRAPH_ATTR) {
				serve_EVENT_DEL_GRAPH_ATTR(bb);
			} else if (cmd == NetStreamConstants.EVENT_ADD_NODE_ATTR) {
				serve_EVENT_ADD_NODE_ATTR(bb);
			} else if (cmd == NetStreamConstants.EVENT_CHG_NODE_ATTR) {
				serve_EVENT_CHG_NODE_ATTR(bb);
			} else if (cmd == NetStreamConstants.EVENT_DEL_NODE_ATTR) {
				serve_EVENT_DEL_NODE_ATTR(bb);
			} else if (cmd == NetStreamConstants.EVENT_ADD_EDGE_ATTR) {
				serve_EVENT_ADD_EDGE_ATTR(bb);
			} else if (cmd == NetStreamConstants.EVENT_CHG_EDGE_ATTR) {
				serve_EVENT_CHG_EDGE_ATTR(bb);
			} else if (cmd == NetStreamConstants.EVENT_DEL_EDGE_ATTR) {
				serve_EVENT_DEL_EDGE_ATTR(bb);
			} else if (cmd == NetStreamConstants.EVENT_END) {
				Console.WriteLine("NetStreamReceiver : Client properly ended the connection.");
			} else {
				Console.Error.WriteLine("NetStreamReceiver: Don't know this command: " + cmd);
			}
		} catch (BufferUnderflowException e) {
			Console.Error.WriteLine("bad buffer");
		}
	}

	/// <param name="bb"></param>
	protected void serve_EVENT_DEL_EDGE_ATTR(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received DEL_EDGE_ATTR command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string edgeId = decodeString(bb);
		string attrId = decodeString(bb);

		sendEdgeAttributeRemoved(sourceId, timeId, edgeId, attrId);
	}

	
	protected void serve_EVENT_CHG_EDGE_ATTR(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received CHG_EDGE_ATTR command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string edgeId = decodeString(bb);
		string attrId = decodeString(bb);
		int oldValueType = decodeType(bb);
		object oldValue = decodeValue(bb, oldValueType);
		int newValueType = decodeType(bb);
		object newValue = decodeValue(bb, newValueType);

		sendEdgeAttributeChanged(sourceId, timeId, edgeId, attrId, oldValue, newValue);

	}

	
	protected void serve_EVENT_ADD_EDGE_ATTR(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received ADD_EDGE_ATTR command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string edgeId = decodeString(bb);
		string attrId = decodeString(bb);
		object value = decodeValue(bb, decodeType(bb));

		sendEdgeAttributeAdded(sourceId, timeId, edgeId, attrId, value);

	}

	
	protected void serve_EVENT_DEL_NODE_ATTR(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received DEL_NODE_ATTR command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string nodeId = decodeString(bb);
		string attrId = decodeString(bb);

		sendNodeAttributeRemoved(sourceId, timeId, nodeId, attrId);

	}

	
	protected void serve_EVENT_CHG_NODE_ATTR(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received EVENT_CHG_NODE_ATTR command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string nodeId = decodeString(bb);
		string attrId = decodeString(bb);
		int oldValueType = decodeType(bb);
		object oldValue = decodeValue(bb, oldValueType);
		int newValueType = decodeType(bb);
		object newValue = decodeValue(bb, newValueType);

		sendNodeAttributeChanged(sourceId, timeId, nodeId, attrId, oldValue, newValue);
	}

	
	protected void serve_EVENT_ADD_NODE_ATTR(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received EVENT_ADD_NODE_ATTR command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string nodeId = decodeString(bb);
		string attrId = decodeString(bb);
		object value = decodeValue(bb, decodeType(bb));

		sendNodeAttributeAdded(sourceId, timeId, nodeId, attrId, value);
	}

	
	protected void serve_EVENT_DEL_GRAPH_ATTR(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received EVENT_DEL_GRAPH_ATTR command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string attrId = decodeString(bb);

		sendGraphAttributeRemoved(sourceId, timeId, attrId);
	}

	
	protected void serve_EVENT_CHG_GRAPH_ATTR(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received EVENT_CHG_GRAPH_ATTR command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string attrId = decodeString(bb);
		int oldValueType = decodeType(bb);
		object oldValue = decodeValue(bb, oldValueType);
		int newValueType = decodeType(bb);
		object newValue = decodeValue(bb, newValueType);

		sendGraphAttributeChanged(sourceId, timeId, attrId, oldValue, newValue);
	}

	
	protected void serve_EVENT_ADD_GRAPH_ATTR(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received EVENT_ADD_GRAPH_ATTR command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string attrId = decodeString(bb);
		object value = decodeValue(bb, decodeType(bb));

		LOGGER.finest(string.Format("NetStreamServer | EVENT_ADD_GRAPH_ATTR | {0}={1}", attrId, value.ToString()));

		sendGraphAttributeAdded(sourceId, timeId, attrId, value);
	}

	
	protected void serve_EVENT_CLEARED(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received EVENT_CLEARED command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);

		sendGraphCleared(sourceId, timeId);
	}

	
	protected void serve_EVENT_STEP(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received EVENT_STEP command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		double time = decodeDouble(bb);

		sendStepBegins(sourceId, timeId, time);
	}

	
	protected void serve_EVENT_DEL_EDGE(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received EVENT_DEL_EDGE command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string edgeId = decodeString(bb);

		sendEdgeRemoved(sourceId, timeId, edgeId);
	}

	
	protected void serve_EVENT_ADD_EDGE(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received ADD_EDGE command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string edgeId = decodeString(bb);
		string from = decodeString(bb);
		string to = decodeString(bb);
		bool directed = decodeBoolean(bb);

		sendEdgeAdded(sourceId, timeId, edgeId, from, to, directed);
	}

	
	protected void serve_DEL_NODE(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received DEL_NODE command.");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string nodeId = decodeString(bb);

		sendNodeRemoved(sourceId, timeId, nodeId);
	}

	
	protected void serve_EVENT_ADD_NODE(byte[] bb) {
		LOGGER.finest("NetStreamServer: Received EVENT_ADD_NODE command");

		string sourceId = decodeString(bb);
		long timeId = decodeUnsignedVarint(bb);
		string nodeId = decodeString(bb);

		sendNodeAdded(sourceId, timeId, nodeId);
	}
}

}
