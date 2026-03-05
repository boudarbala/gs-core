using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File.Gexf
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


public class GEXFNodes : SinkAdapter, IGEXFElement {
	GEXF root;
	Dictionary<string, GEXFNode> nodes;

	public GEXFNodes(GEXF root) {
		this.root = root;
		this.nodes = new Dictionary<string, GEXFNode>();

		root.addSink(this);
	}

	private float[] convertToXYZ(object value) {
		if (value == null || !value.GetType().IsArray)
			return null;

		float[] xyz = new float[Array.getLength(value)];

		for (int i = 0; i < xyz.Length; i++) {
			object o = Array[value, i];

			if (o is IConvertible)
				xyz[i] = ((IConvertible) o);
			else
				return null;
		}

		return xyz;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.gexf.GEXFElement#export(org.graphstream.stream
	 * .file.gexf.SmartXMLWriter)
	 */
	public void export(SmartXMLWriter stream){
		stream.startElement("nodes");

		foreach (GEXFNode node in ((nodes[])Enum.GetValues(typeof(nodes))))
			node.export(stream);

		stream.endElement(); // NODES
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.SinkAdapter#nodeAdded(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		GEXFNode node = nodes[nodeId];

		if (node == null) {
			node = new GEXFNode(root, nodeId);
			nodes[nodeId] = node;
		}

		node.spells.Start();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.SinkAdapter#nodeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		GEXFNode node = nodes[nodeId];

		if (node == null) {
			System.err.printf("node removed but not added\n");
			return;
		}

		node.spells.end();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.SinkAdapter#nodeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		GEXFNode node = nodes[nodeId];

		if (("ui.label".Equals(attribute) || "label".Equals(attribute)) && value != null)
			node.label = value.ToString();

		if ("xyz".Equals(attribute)) {
			float[] xyz = convertToXYZ(value);

			switch (xyz.Length) {
			default:
				node.z = xyz[2];
			case 2:
				node.y = xyz[1];
			case 1:
				node.x = xyz[0];
			case 0:
				break;
			}

			node.position = true;
		}

		node.attvalues.attributeUpdated(root.getNodeAttribute(attribute), value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.SinkAdapter#nodeAttributeChanged(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		nodeAttributeAdded(sourceId, timeId, nodeId, attribute, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.SinkAdapter#nodeAttributeRemoved(java.lang.String,
	 * long, java.lang.String, java.lang.String)
	 */
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		GEXFNode node = nodes[nodeId];
		node.attvalues.attributeUpdated(root.getNodeAttribute(attribute), null);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.SinkAdapter#graphCleared(java.lang.String, long)
	 */
	public void graphCleared(string sourceId, long timeId) {
		foreach (GEXFNode node in ((nodes[])Enum.GetValues(typeof(nodes))))
			node.spells.end();
	}
}

}
