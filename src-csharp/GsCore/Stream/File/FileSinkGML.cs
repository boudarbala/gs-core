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
/// Transform the input events into a GML graph. <p> THIS CLASS IS REALLY NOT APPROPRIATE FOR GENERAL USE. Indeed the GML format is not dynamic and it is very difficult to export the correct attributes of nodes if the declaration of the attribute is far from the declaration of the node. The only way would be to store the graph in a buffer and output it at once when the file is closed. </p> <p> Therefore this class outputs attributes of nodes and edges only if their addition directly follows the corresponding node or edge. </p>
/// </summary>
public class FileSinkGML : FileSinkBase {
	// Attributes

	/// <summary>
/// Alias on the output OutputStream.
/// </summary>
	protected System.IO.StreamWriter out;

	protected string nodeToFinish = null;

	protected string edgeToFinish = null;

	// Construction

	public FileSinkGML() {
		// NOP
	}

	// File format events

	
	protected void outputHeader(){
		out = (System.IO.StreamWriter) output;

		output.printf("graph [%n");
	}

	
	protected void outputEndOfFile(){
		ensureToFinish();
		output.printf("]%n");
	}

	// Attribute events

	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		ensureToFinish();

		string val = valueToString(value);
		attribute = keyToString(attribute);

		if (val != null) {
			output.printf("\t%s %s%n", attribute, val);
		}
	}

	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		ensureToFinish();
		// GML is not a dynamic file format ?
	}

	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		ensureToFinish();
		// GML is not a dynamic file format ?
	}

	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		if (nodeToFinish != null && nodeToFinish.Equals(nodeId)) {
			string val = valueToString(value);
			attribute = keyToString(attribute);

			if (val != null) {
				output.printf("\t\t%s %s%n", attribute, val);
			}
		} else {
			ensureToFinish();
		}
	}

	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		if (edgeToFinish != null)
			ensureToFinish();
		// GML is not a dynamic file format ?
	}

	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		if (edgeToFinish != null)
			ensureToFinish();
		// GML is not a dynamic file format ?
	}

	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		if (edgeToFinish != null && edgeToFinish.Equals(edgeId)) {
			string val = valueToString(value);
			attribute = keyToString(attribute);

			if (val != null) {
				output.printf("\t\t%s %s%n", attribute, val);
			}
		} else {
			ensureToFinish();
		}
	}

	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		if (nodeToFinish != null)
			ensureToFinish();
		// GML is not a dynamic file format ?
	}

	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		if (nodeToFinish != null)
			ensureToFinish();
		// GML is not a dynamic file format ?
	}

	// Element events

	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		ensureToFinish();
		output.printf("\tnode [%n");
		output.printf("\t\tid \"%s\"%n", nodeId);
		nodeToFinish = nodeId;
	}

	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		ensureToFinish();
	}

	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		ensureToFinish();
		output.printf("\tedge [%n");
		output.printf("\t\tid \"%s\"%n", edgeId);
		output.printf("\t\tsource \"%s\"%n", fromNodeId);
		output.printf("\t\ttarget \"%s\"%n", toNodeId);
		edgeToFinish = edgeId;
	}

	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		ensureToFinish();
	}

	public void graphCleared(string sourceId, long timeId) {
		// Ah ah ah !!
	}

	public void stepBegins(string sourceId, long timeId, double step) {
		// NOP
	}

	// Commands

	Pattern forbiddenKeyChars = Pattern.compile(".*[^a-zA-Z0-9-_.].*");

	protected string keyToString(string key) {
		if (forbiddenKeyChars.matcher(key).matches())
			return "\"" + key.Replace("\"", "\\\"") + "\"";

		return key;
	}

	protected string valueToString(object value) {
		if (value == null)
			return null;

		if (value is IConvertible) {
			double val = ((IConvertible) value);
			if ((val - ((int) val)) == 0)
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", (int) val);
			else
				return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", val);
		}

		return string.Format("\"%s\"", value.ToString().Replace("\n|\r|\"", " "));
	}

	protected void ensureToFinish() {
		assert ((nodeToFinish != null && edgeToFinish == null) || (nodeToFinish == null && edgeToFinish != null)
				|| (nodeToFinish == null && edgeToFinish == null));

		if (nodeToFinish != null || edgeToFinish != null) {
			output.printf("\t]%n");
			nodeToFinish = null;
			edgeToFinish = null;
		}
	}
}
}
