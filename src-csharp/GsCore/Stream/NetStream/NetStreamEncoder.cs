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



public class NetStreamEncoder : ByteEncoder {
	private static readonly object /* Logger */ LOGGER = null /* Logger */;

	protected List<Transport> transportList;
	protected string sourceId;
	protected byte[] sourceIdBuff;
	protected byte[] streamBuffer;

	public NetStreamEncoder(params Transport[] transports) : this("default", transports) {
	}

	public NetStreamEncoder(string stream, params Transport[] transports) {
		streamBuffer = encodeString(stream);
		transportList = new List<object>();

		if (transports != null) {
			foreach (Transport transport in transports)
				transportList.Add(transport);
		}
	}

	
	public void addTransport(Transport transport) {
		transportList.Add(transport);
	}

	
	public void removeTransport(Transport transport) {
		transportList.Remove(transport);
	}

	protected byte[] getEncodedValue(object input, int valueType) {
		byte[] value = encodeValue(input, valueType);

		if (value == null) {
			Console.Error.WriteLine(string.Format("unknown value type {0}\n", valueType));
		}

		return value;
	}

	protected void doSend(byte[] evt) {
		foreach (Transport transport in transportList) {
			/* evt.rewind() */;
			transport.send(evt);
		}
	}

	protected byte[] getAndPrepareBuffer(string sourceId, long timeId, int eventType, int messageSize) {
		if (!sourceId.Equals(this.sourceId)) {
			this.sourceId = sourceId;
			sourceIdBuff = encodeString(sourceId);
		}

		/* streamBuffer.rewind() */;
		/* sourceIdBuff.rewind() */;

		int size = 4 + +streamBuffer.Length // stream
				+ 1 // CMD
				+ sourceIdBuff.Length // source id
				+ getVarintSize(timeId) // timeId
				+ messageSize;

		byte[] bb = new byte[size];
		// Legacy Java code removed

		return bb;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang.String ,
	 * long, java.lang.String, java.lang.object)
	 */
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		byte[] attrBuff = encodeString(attribute);
		int valueType = getType(value);
		byte[] valueBuff = getEncodedValue(value, valueType);

		int innerSize = attrBuff.Length // attribute id
				+ 1 // attr type
				+ valueBuff.Length;

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_ADD_GRAPH_ATTR, innerSize);

		buff.Add(attrBuff).Add((byte) valueType).Add(valueBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang.
	 * String, long, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		byte[] attrBuff = encodeString(attribute);
		int oldValueType = getType(oldValue);
		int newValueType = getType(newValue);

		byte[] oldValueBuff = getEncodedValue(oldValue, oldValueType);
		byte[] newValueBuff = getEncodedValue(newValue, newValueType);

		int innerSize = attrBuff.Length + // attribute id
				1 + // attr type
				oldValueBuff.Length + // attr value
				1 + // attr type
				newValueBuff.Length; // attr value

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_CHG_GRAPH_ATTR, innerSize);

		buff.Add(attrBuff).Add((byte) oldValueType).Add(oldValueBuff).Add((byte) newValueType).Add(newValueBuff);

		doSend(buff);

	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang.
	 * String, long, java.lang.String)
	 */
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		byte[] attrBuff = encodeString(attribute);

		int innerSize = attrBuff.Length;

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_DEL_GRAPH_ATTR, innerSize);
		buff.Add(attrBuff);

		doSend(buff);

	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		byte[] nodeBuff = encodeString(nodeId);
		byte[] attrBuff = encodeString(attribute);
		int valueType = getType(value);
		byte[] valueBuff = getEncodedValue(value, valueType);

		int innerSize = nodeBuff.Length + // nodeId
				attrBuff.Length + // attribute
				1 + // value type
				valueBuff.Length;

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_ADD_NODE_ATTR, innerSize);

		buff.Add(nodeBuff).Add(attrBuff).Add((byte) valueType).Add(valueBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		byte[] nodeBuff = encodeString(nodeId);
		byte[] attrBuff = encodeString(attribute);

		int oldValueType = getType(oldValue);
		int newValueType = getType(newValue);

		byte[] oldValueBuff = getEncodedValue(oldValue, oldValueType);
		byte[] newValueBuff = getEncodedValue(newValue, newValueType);

		int innerSize = nodeBuff.Length + // nodeId
				attrBuff.Length + // attribute
				1 + // value type
				oldValueBuff.Length + // value
				1 + // value type
				newValueBuff.Length;

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_CHG_NODE_ATTR, innerSize);

		buff.Add(nodeBuff).Add(attrBuff).Add((byte) oldValueType).Add(oldValueBuff).Add((byte) newValueType)
				.put(newValueBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		byte[] nodeBuff = encodeString(nodeId);
		byte[] attrBuff = encodeString(attribute);

		int innerSize = nodeBuff.Length + // nodeId
				attrBuff.Length; // attribute

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_DEL_NODE_ATTR, innerSize);

		buff.Add(nodeBuff).Add(attrBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		byte[] edgeBuff = encodeString(edgeId);
		byte[] attrBuff = encodeString(attribute);

		int valueType = getType(value);

		byte[] valueBuff = getEncodedValue(value, valueType);

		int innerSize = edgeBuff.Length + // nodeId
				attrBuff.Length + // attribute
				1 + // value type
				valueBuff.Length; // value

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_ADD_EDGE_ATTR, innerSize);

		buff.Add(edgeBuff).Add(attrBuff).Add((byte) valueType) // value type
				.put(valueBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		byte[] edgeBuff = encodeString(edgeId);
		byte[] attrBuff = encodeString(attribute);
		int oldValueType = getType(oldValue);
		int newValueType = getType(newValue);

		byte[] oldValueBuff = getEncodedValue(oldValue, oldValueType);
		byte[] newValueBuff = getEncodedValue(newValue, newValueType);

		int innerSize = edgeBuff.Length + // nodeId
				attrBuff.Length + // attribute
				1 + // value type
				oldValueBuff.Length + // value
				1 + // value type
				newValueBuff.Length; // value

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_CHG_EDGE_ATTR, innerSize);

		buff.Add(edgeBuff).Add(attrBuff).Add((byte) oldValueType).Add(oldValueBuff).Add((byte) newValueType)
				.put(newValueBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		byte[] edgeBuff = encodeString(edgeId);
		byte[] attrBuff = encodeString(attribute);

		int innerSize = edgeBuff.Length + // nodeId
				attrBuff.Length; // attribute

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_DEL_EDGE_ATTR, innerSize);

		buff.Add(edgeBuff).Add(attrBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.ElementSink#nodeAdded(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		byte[] nodeBuff = encodeString(nodeId);

		int innerSize = nodeBuff.Length;

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_ADD_NODE, innerSize);
		buff.Add(nodeBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.ElementSink#nodeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		byte[] nodeBuff = encodeString(nodeId);

		int innerSize = nodeBuff.Length;

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_DEL_NODE, innerSize);
		buff.Add(nodeBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.ElementSink#edgeAdded(java.lang.String, long,
	 * java.lang.String, java.lang.String, java.lang.String, boolean)
	 */
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		byte[] edgeBuff = encodeString(edgeId);
		byte[] fromNodeBuff = encodeString(fromNodeId);
		byte[] toNodeBuff = encodeString(toNodeId);

		int innerSize = edgeBuff.Length + // edge
				fromNodeBuff.Length + // from nodeId
				toNodeBuff.Length + // to nodeId
				1; // direction

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_ADD_EDGE, innerSize);

		buff.Add(edgeBuff).Add(fromNodeBuff).Add(toNodeBuff).Add((byte) (!directed ? 0 : 1));

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.ElementSink#edgeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		byte[] edgeBuff = encodeString(edgeId);

		int innerSize = edgeBuff.Length;

		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_DEL_EDGE, innerSize);
		buff.Add(edgeBuff);

		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.ElementSink#graphCleared(java.lang.String, long)
	 */
	public void graphCleared(string sourceId, long timeId) {
		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_CLEARED, 0);
		doSend(buff);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.ElementSink#stepBegins(java.lang.String, long,
	 * double)
	 */
	public void stepBegins(string sourceId, long timeId, double step) {
		byte[] buff = getAndPrepareBuffer(sourceId, timeId, NetStreamConstants.EVENT_STEP, 8);
		buff.Write(BitConverter.GetBytes(step);

		doSend(buff);
	}
}

}
