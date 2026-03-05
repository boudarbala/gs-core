using System.Collections.Generic;
using System.IO;
using System.Linq;
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
/// <p> File output for the DGS (Dynamic Graph Stream) file format. It includes also the possibility to filter dynamic events such as : <ul> <li>Addition or deletion of nodes or edges.</li> <li>Addition/deletion/modification of attributes of nodes, edges or the graph itself.</li> <li>A step evt.</li> </ul> </p> <p> For instance : </p> <pre> Graph graph = new SingleGraph("My_Graph"); FileSinkDGSFiltered fileSink = new FileSinkDGSFiltered(); graph.addSink(fileSink); // No need to save the attribute "attr1" of any edges fileSink.addEdgeAttributeFiltered("attr1"); // No need to save graph attributes fileSink.setNoFilterGraphAttributeAdded(false); // Start to listen event fileSink.begin("./my_graph.dgs"); // Make some modifications on the graph and generate events graph.stepBegins(0); // this event will be saved graph.setAttribute("attr2", 2); // this event will not be saved Node a = graph.addNode("A"); // this event will be saved a.setAttribute("attr3", 3); // this event will be saved // and now, no more need to save modification on nodes attributes fileSink.setNoFilterNodeAttributeChanged(false); Node b = graph.addNode("B"); // this event will be saved b.setAttribute("attr4", 4); // this event will not be saved Edge ab = graph.addEdge("AB", a, b); // this event will be saved ab.setAttribute("attr1", 1); // this event will not be saved ab.setAttribute("attr5", 5); // this event will be saved fileSink.end(); </pre>
/// </summary>
public class FileSinkDGSFiltered : FileSinkBaseFiltered {

	// Attribute

	/// <summary>
/// A shortcut to the output.
/// </summary>
	protected System.IO.StreamWriter out;
	protected string graphName = "";

	// Command

	
	protected void outputHeader(){
		out = (System.IO.StreamWriter) output;
		output.printf("DGS004%n");

		if (graphName.Length <= 0)
			output.printf("null 0 0%n");
		else
			output.printf("\"{0}\" 0 0%n", FileSinkDGSUtility.formatStringForQuoting(graphName));
	}

	
	protected void outputEndOfFile(){
		// NOP
	}

	public void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value) {
		if (noFilterEdgeAttributeAdded && !edgeAttributesFiltered.Contains(attribute))
			edgeAttributeChanged(graphId, timeId, edgeId, attribute, null, value);
	}

	public void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		if (noFilterEdgeAttributeChanged && !edgeAttributesFiltered.Contains(attribute))
			output.printf("ce \"{0}\" %s%n", FileSinkDGSUtility.formatStringForQuoting(edgeId),
					FileSinkDGSUtility.attributeString(attribute, newValue, false));
	}

	public void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute) {
		if (noFilterEdgeAttributeRemoved && !edgeAttributesFiltered.Contains(attribute))
			output.printf("ce \"{0}\" %s%n", FileSinkDGSUtility.formatStringForQuoting(edgeId),
					FileSinkDGSUtility.attributeString(attribute, null, true));
	}

	public void graphAttributeAdded(string graphId, long timeId, string attribute, object value) {
		if (noFilterGraphAttributeAdded && !graphAttributesFiltered.Contains(attribute))
			graphAttributeChanged(graphId, timeId, attribute, null, value);
	}

	public void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue) {
		if (noFilterGraphAttributeChanged && !graphAttributesFiltered.Contains(attribute))
			output.printf("cg {0}\n", FileSinkDGSUtility.attributeString(attribute, newValue, false));
	}

	public void graphAttributeRemoved(string graphId, long timeId, string attribute) {
		if (noFilterGraphAttributeRemoved && !graphAttributesFiltered.Contains(attribute))
			output.printf("cg {0}\n", FileSinkDGSUtility.attributeString(attribute, null, true));
	}

	public void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value) {
		if (noFilterNodeAttributeAdded && !nodeAttributesFiltered.Contains(attribute))
			nodeAttributeChanged(graphId, timeId, nodeId, attribute, null, value);
	}

	public void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		if (noFilterNodeAttributeChanged && !nodeAttributesFiltered.Contains(attribute))
			output.printf("cn \"{0}\" %s%n", FileSinkDGSUtility.formatStringForQuoting(nodeId),
					FileSinkDGSUtility.attributeString(attribute, newValue, false));
	}

	public void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute) {
		if (noFilterNodeAttributeRemoved && !nodeAttributesFiltered.Contains(attribute))
			output.printf("cn \"{0}\" %s%n", FileSinkDGSUtility.formatStringForQuoting(nodeId),
					FileSinkDGSUtility.attributeString(attribute, null, true));
	}

	public void edgeAdded(string graphId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		if (noFilterEdgeAdded) {
			edgeId = FileSinkDGSUtility.formatStringForQuoting(edgeId);
			fromNodeId = FileSinkDGSUtility.formatStringForQuoting(fromNodeId);
			toNodeId = FileSinkDGSUtility.formatStringForQuoting(toNodeId);
			output.printf("ae \"{0}\" \"{0}\" %s \"{0}\"%n", edgeId, fromNodeId, directed ? ">" : "", toNodeId);
		}
	}

	public void edgeRemoved(string graphId, long timeId, string edgeId) {
		if (noFilterEdgeRemoved)
			output.printf("de \"{0}\"%n", FileSinkDGSUtility.formatStringForQuoting(edgeId));
	}

	public void graphCleared(string graphId, long timeId) {
		if (noFilterGraphCleared)
			output.printf("cl%n");
	}

	public void nodeAdded(string graphId, long timeId, string nodeId) {
		if (noFilterNodeAdded)
			output.printf("an \"{0}\"%n", FileSinkDGSUtility.formatStringForQuoting(nodeId));
	}

	public void nodeRemoved(string graphId, long timeId, string nodeId) {
		if (noFilterNodeRemoved)
			output.printf("dn \"{0}\"%n", FileSinkDGSUtility.formatStringForQuoting(nodeId));
	}

	public void stepBegins(string graphId, long timeId, double step) {
		if (noFilterStepBegins)
			output.printf(System.Globalization.CultureInfo.InvariantCulture, "st %f%n", step);
	}
}

}
