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
/// File output for the DGS (Dynamic Graph Stream) file format.
/// </summary>
public class FileSinkDGS : FileSinkBase {
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
			output.printf("\"%s\" 0 0%n", FileSinkDGSUtility.formatStringForQuoting(graphName));
	}

	
	protected void outputEndOfFile(){
		// NOP
	}

	public void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value) {
		edgeAttributeChanged(graphId, timeId, edgeId, attribute, null, value);
	}

	public void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		output.printf("ce \"%s\" %s%n", FileSinkDGSUtility.formatStringForQuoting(edgeId),
				FileSinkDGSUtility.attributeString(attribute, newValue, false));
	}

	public void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute) {
		output.printf("ce \"%s\" %s%n", FileSinkDGSUtility.formatStringForQuoting(edgeId),
				FileSinkDGSUtility.attributeString(attribute, null, true));
	}

	public void graphAttributeAdded(string graphId, long timeId, string attribute, object value) {
		graphAttributeChanged(graphId, timeId, attribute, null, value);
	}

	public void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue) {
		output.printf("cg %s%n", FileSinkDGSUtility.attributeString(attribute, newValue, false));
	}

	public void graphAttributeRemoved(string graphId, long timeId, string attribute) {
		output.printf("cg %s%n", FileSinkDGSUtility.attributeString(attribute, null, true));
	}

	public void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value) {
		nodeAttributeChanged(graphId, timeId, nodeId, attribute, null, value);
	}

	public void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		output.printf("cn \"%s\" %s%n", FileSinkDGSUtility.formatStringForQuoting(nodeId),
				FileSinkDGSUtility.attributeString(attribute, newValue, false));
	}

	public void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute) {
		output.printf("cn \"%s\" %s%n", FileSinkDGSUtility.formatStringForQuoting(nodeId),
				FileSinkDGSUtility.attributeString(attribute, null, true));
	}

	public void edgeAdded(string graphId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		edgeId = FileSinkDGSUtility.formatStringForQuoting(edgeId);
		fromNodeId = FileSinkDGSUtility.formatStringForQuoting(fromNodeId);
		toNodeId = FileSinkDGSUtility.formatStringForQuoting(toNodeId);

		output.printf("ae \"%s\" \"%s\" %s \"%s\"%n", edgeId, fromNodeId, directed ? ">" : "", toNodeId);
	}

	public void edgeRemoved(string graphId, long timeId, string edgeId) {
		output.printf("de \"%s\"%n", FileSinkDGSUtility.formatStringForQuoting(edgeId));
	}

	public void graphCleared(string graphId, long timeId) {
		output.printf("cl%n");
	}

	public void nodeAdded(string graphId, long timeId, string nodeId) {
		output.printf("an \"%s\"%n", FileSinkDGSUtility.formatStringForQuoting(nodeId));
	}

	public void nodeRemoved(string graphId, long timeId, string nodeId) {
		output.printf("dn \"%s\"%n", FileSinkDGSUtility.formatStringForQuoting(nodeId));
	}

	public void stepBegins(string graphId, long timeId, double step) {
		output.printf(System.Globalization.CultureInfo.InvariantCulture, "st %f%n", step);
	}
}
}
