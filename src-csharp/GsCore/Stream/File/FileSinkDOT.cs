using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.Stream.File
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


/// <summary>
/// Graph writer for the GraphViz DOT format.
/// </summary>
public class FileSinkDOT : FileSinkBase {
	// Attribute

	/// <summary>
/// The output.
/// </summary>
	protected System.IO.StreamWriter out;

	/// <summary>
/// The graph name (set as soon as known).
/// </summary>
	protected string graphName = "";

	/// <summary>
/// Is the graph directed ?
/// </summary>
	protected bool digraph;

	/// <summary>
/// What element ?.
/// </summary>
	protected enum What {
		NODE, EDGE, OTHER
	}

	/// <summary>
/// Build a new DOT sink to export undirected graph.
/// </summary>
	public FileSinkDOT() : this(false) {
	}

	/// <summary>
/// Build a new DOT sink specifying if the graph is directed or not.
/// </summary>
/// <param name="digraph"> true if the graph is directed</param>
	public FileSinkDOT {
		this.digraph = digraph;
	}

	// Command

	/// <summary>
/// Set flag indicating if exported graph is directed or not.
/// </summary>
/// <param name="digraph"> true is exported graph is directed</param>
	public void setDirected(bool digraph) {
		this.digraph = digraph;
	}

	/// <summary>
/// Get the flag indicating if exported graph is directed or not.
/// </summary>
/// <returns>true if exported graph is directed</returns>
	public bool isDirected() {
		return digraph;
	}

	
	protected void exportGraph(IGraph graph) {
		string graphId = graph.getId();
		AtomicLong timeId = new AtomicLong(0);

		graph.attributeKeys()
				.ToList().ForEach(key => graphAttributeAdded(graphId, timeId.getAndIncrement(), key, graph.getAttribute(key)));

		foreach (INode node in graph) {
			string nodeId = node.getId();
			output.printf("\t\"{0}\" %s;%n", nodeId, outputAttributes(node));
		}

		graph.edges().ToList().ForEach(edge => {
			string fromNodeId = edge.getNode0().getId();
			string toNodeId = edge.getNode1().getId();
			string attr = outputAttributes(edge);

			if (digraph) {
				output.printf("\t\"{0}\" -> \"{0}\"", fromNodeId, toNodeId);

				if (!edge.isDirected())
					output.printf(" -> \"{0}\"", fromNodeId);
			} else
				output.printf("\t\"{0}\" -- \"{0}\"", fromNodeId, toNodeId);

			output.printf(" {0};\n", attr);
		});
	}

	
	protected void outputHeader(){
		out = (System.IO.StreamWriter) output;
		output.printf("{0} {\n", digraph ? "digraph" : "graph");

		if (graphName.Length > 0)
			output.printf("\tgraph [label={0}];\n", graphName);
	}

	
	protected void outputEndOfFile(){
		output.printf("}%n");
	}

	public void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value) {
		// NOP
	}

	public void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		// NOP
	}

	public void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute) {
		// NOP
	}

	public void graphAttributeAdded(string graphId, long timeId, string attribute, object value) {
		output.printf("\tgraph [ {0} ];\n", outputAttribute(attribute, value, true));
	}

	public void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue) {
		output.printf("\tgraph [ {0} ];\n", outputAttribute(attribute, newValue, true));
	}

	public void graphAttributeRemoved(string graphId, long timeId, string attribute) {
		// NOP
	}

	public void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value) {
		output.printf("\t\"{0}\" [ %s ];%n", nodeId, outputAttribute(attribute, value, true));
	}

	public void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		output.printf("\t\"{0}\" [ %s ];%n", nodeId, outputAttribute(attribute, newValue, true));
	}

	public void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute) {
		// NOP
	}

	public void edgeAdded(string graphId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		if (digraph) {
			output.printf("\t\"{0}\" -> \"{0}\"", fromNodeId, toNodeId);

			if (!directed)
				output.printf(" -> \"{0}\"", fromNodeId);

			output.printf(";%n");
		} else
			output.printf("\t\"{0}\" -- \"{0}\";%n", fromNodeId, toNodeId);
	}

	public void edgeRemoved(string graphId, long timeId, string edgeId) {
		// NOP
	}

	public void graphCleared(string graphId, long timeId) {
		// NOP
	}

	public void nodeAdded(string graphId, long timeId, string nodeId) {
		output.printf("\t\"{0}\";%n", nodeId);
	}

	public void nodeRemoved(string graphId, long timeId, string nodeId) {
		// NOP
	}

	public void stepBegins(string graphId, long timeId, double step) {
		// NOP
	}

	// Utility
	/*
	 * protected void outputAttributes(Map<String, object> attributes, What what)
	 * throws System.IO.IOException { output.printf(" [");
	 * 
	 * boolean first = true;
	 * 
	 * for (String key : attributes.keySet()) { object value = attributes.get(key);
	 * 
	 * if (what == What.NODE) { // if( ! nodeForbiddenAttrs.contains( key ) ) {
	 * first = outputAttribute(key, value, first); } } else if (what == What.EDGE) {
	 * // if( ! edgeForbiddenAttrs.contains( key ) ) { first = outputAttribute(key,
	 * value, first); } } else { first = outputAttribute(key, value, first); }
	 * 
	 * }
	 * 
	 * output.printf("]"); }
	 */
	protected string outputAttribute(string key, object value, bool first) {
		bool quote = true;

		if (value is IConvertible)
			quote = false;

		return string.Format("{0}\"{0}\"=%s%s%s", first ? "" : ",", key, quote ? "\"" : "", value, quote ? "\"" : "");
	}

	protected string outputAttributes(IElement e) {
		if (e.getAttributeCount() == 0)
			return "";

		System.Text.StringBuilder buffer = new System.Text.StringBuilder("[");
		AtomicBoolean first = new AtomicBoolean(true);

		e.attributeKeys().ToList().ForEach(key => {
			bool quote = true;
			object value = e.getAttribute(key);

			if (value is IConvertible)
				quote = false;

			buffer.Append(string.Format("{0}\"{0}\"=%s%s%s", first[] ? "" : ",", key, quote ? "\"" : "", value,
					quote ? "\"" : ""));

			first.set(false);
		});

		return buffer.Append(']').ToString();
	}
}
}
