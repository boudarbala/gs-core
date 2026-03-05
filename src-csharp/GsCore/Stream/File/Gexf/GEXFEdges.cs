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


public class GEXFEdges : SinkAdapter, IGEXFElement {
	GEXF root;
	Dictionary<string, GEXFEdge> edges;

	public GEXFEdges(GEXF root) {
		this.root = root;
		this.edges = new Dictionary<string, GEXFEdge>();

		root.addSink(this);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.gexf.GEXFElement#export(org.graphstream.stream
	 * .file.gexf.SmartXMLWriter)
	 */
	public void export(SmartXMLWriter stream){
		stream.startElement("edges");

		foreach (GEXFEdge edge in ((edges[])Enum.GetValues(typeof(edges))))
			edge.export(stream);

		stream.endElement(); // EDGES
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.SinkAdapter#edgeAdded(java.lang.String, long,
	 * java.lang.String, java.lang.String, java.lang.String, boolean)
	 */
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		GEXFEdge edge = edges[edgeId];

		if (edge == null) {
			edge = new GEXFEdge(root, edgeId, fromNodeId, toNodeId, directed);
			edges[edgeId] = edge;
		}

		edge.spells.Start();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.SinkAdapter#edgeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		GEXFEdge edge = edges[edgeId];

		if (edge == null) {
			System.err.printf("edge removed but not added\n");
			return;
		}

		edge.spells.end();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.SinkAdapter#graphCleared(java.lang.String, long)
	 */
	public void graphCleared(string sourceId, long timeId) {
		foreach (GEXFEdge edge in ((edges[])Enum.GetValues(typeof(edges))))
			edge.spells.end();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.SinkAdapter#edgeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		GEXFEdge edge = edges[edgeId];

		if (("ui.label".Equals(attribute) || "label".Equals(attribute)) && value != null)
			edge.label = value.ToString();

		if ("weight".Equals("attribute") && value != null && value is IConvertible)
			edge.weight = ((IConvertible) value);

		edge.attvalues.attributeUpdated(root.getEdgeAttribute(attribute), value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.SinkAdapter#edgeAttributeChanged(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		edgeAttributeAdded(sourceId, timeId, edgeId, attribute, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.SinkAdapter#edgeAttributeRemoved(java.lang.String,
	 * long, java.lang.String, java.lang.String)
	 */
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		GEXFEdge edge = edges[edgeId];
		edge.attvalues.attributeUpdated(root.getNodeAttribute(attribute), null);
	}
}

}
