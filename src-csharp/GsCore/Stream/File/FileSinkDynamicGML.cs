using System.Collections.Generic;
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
/// Transform the input events into a GML graph. <p> THIS CLASS IS REALLY NOT APPROPRIATE FOR GENERAL USE. Indeed the GML format is not dynamic and it is very difficult to export the correct attributes of nodes if the declaration of the attribute is far from the declaration of the node. The only way would be to store the graph in a buffer and output it at once when the file is closed. </p> <p> Therefore this class outputs attributes of nodes and edges only if their addition directly follows the corresponding node or edge. </p>
/// </summary>
public class FileSinkDynamicGML : FileSinkGML {
	// Construction

	public FileSinkDynamicGML() {
		// NOP
	}

	// Attribute events

	
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		ensureToFinish();

		string val = valueToString(value);

		if (val != null) {
			output.printf("\t{0} {1}\n", attribute, val);
		}
	}

	
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		ensureToFinish();
		graphAttributeAdded(sourceId, timeId, attribute, newValue);
	}

	
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		ensureToFinish();
		output.printf("\t-{0}\n", attribute);
	}

	
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		nodeAttributeChanged(sourceId, timeId, nodeId, attribute, null, value);
	}

	
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {

		if (nodeToFinish == null || (!nodeToFinish.Equals(nodeId))) {
			ensureToFinish();
			output.printf("\t+node [%n");
			output.printf("\t\tid \"{0}\"%n", nodeId);
			nodeToFinish = nodeId;
		}

		if (newValue != null) {
			string val = valueToString(newValue);

			if (val != null) {
				output.printf("\t\t{0} {1}\n", attribute, val);
			}
		} else {
			output.printf("\t\t-{0}\n", attribute);
		}
	}

	
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		nodeAttributeChanged(sourceId, timeId, nodeId, attribute, null, null);
	}

	
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		edgeAttributeChanged(sourceId, timeId, edgeId, attribute, null, value);
	}

	
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {

		if (edgeToFinish == null || (!edgeToFinish.Equals(edgeId))) {
			ensureToFinish();
			output.printf("\t+edge [%n");
			output.printf("\t\tid \"{0}\"%n", edgeId);
			edgeToFinish = edgeId;
		}

		if (newValue != null) {
			string val = valueToString(newValue);

			if (val != null) {
				output.printf("\t\t{0} {1}\n", attribute, val);
			}
		} else {
			output.printf("\t\t-{0}\n", attribute);
		}
	}

	
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		edgeAttributeChanged(sourceId, timeId, edgeId, attribute, null, null);
	}

	// Element events

	
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		ensureToFinish();
		output.printf("\tnode [%n");
		output.printf("\t\tid \"{0}\"%n", nodeId);
		nodeToFinish = nodeId;
	}

	
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		ensureToFinish();
		output.printf("\t-node \"{0}\"%n", nodeId);
	}

	
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		ensureToFinish();
		output.printf("\tedge [%n");
		output.printf("\t\tid \"{0}\"%n", edgeId);
		output.printf("\t\tsource \"{0}\"%n", fromNodeId);
		output.printf("\t\ttarget \"{0}\"%n", toNodeId);
		output.printf("\t\tdirected {0}\n", directed ? "1" : "0");
		edgeToFinish = edgeId;
	}

	
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		ensureToFinish();
		output.printf("\t-edge \"{0}\"%n", edgeId);
	}

	
	public void graphCleared(string sourceId, long timeId) {
		// Ah ah ah !!
	}

	
	public void stepBegins(string sourceId, long timeId, double step) {
		ensureToFinish();
		if ((step - ((int) step)) == 0)
			output.printf("\tstep %d%n", (int) step);
		else
			output.printf("\tstep %f%n", step);
	}
}
}
