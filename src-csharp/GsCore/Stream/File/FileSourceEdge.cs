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
/// Reader for the "edge" graph format. <p> The edge graph format is a very simple and lightweight format where each line describes an edge by giving two node names. The nodes are created implicitly. </p> <p> This reader also understands the derivative format where a line contains a first node name, followed by several node names separated by spaces. In this case it links the first node with all other node name following on the line. </p> <p> Also, the format does not specify any direction for edges. By default all edges are undirected. You can choose to make all edges directed by passing "true" as the first arguments to constructors {@link #FileSourceEdge(boolean)} or {@link #FileSourceEdge(boolean, boolean)} . The direction of edges goes from the first node name on each line toward the second (or more) node names on each line. </p> <p> This format only contains edges. To ensure the "add node" events are sent before an edge referencing two nodes is created via an "add edge" event, this reader has a hash set of already encountered nodes. The hash set allows to issue "add node" events only when a node is encountered for the first time. </p> </p> This hash set consumes memory, but is the only way to ensure "add node" events are correctly issued. If this input is directly connected to a graph, as graphs can create non-existing nodes automatically, you can disable the hash set of nodes using the constructor {@link #FileSourceEdge(boolean, boolean)}, and giving "false" for the second argument. </p> The usual file name extension for this format is ".edge".
/// </summary>
public class FileSourceEdge : FileSourceBase {
	// Attribute

	/// <summary>
/// Allocator for edge identifiers.
/// </summary>
	protected int edgeid = 0;

	/// <summary>
/// By default, consider edges as undirected.
/// </summary>
	protected bool directed = false;

	/// <summary>
/// Set of existing nodes (if nodes are declared).
/// </summary>
	protected HashSet<string> nodes;

	protected string graphName = "EDGE_";

	// Construction

	/// <summary>
/// New reader for the "edge" format.
/// </summary>
	public FileSourceEdge() : this(false) {
	}

	/// <summary>
/// New reader for the "edge" format.
/// </summary>
/// <param name="edgesAreDirected"> If true (default=false) edges are considered directed.</param>
	public FileSourceEdge(bool edgesAreDirected) : this(edgesAreDirected, true) {
	}

	/// <summary>
/// New reader for the "edge" format.
/// </summary>
/// <param name="edgesAreDirected"> If true (default=false) edges are considered directed.</param>
/// <param name="declareNodes"> If true (default=true) this reader outputs nodeAdded events.</param>
	public FileSourceEdge(bool edgesAreDirected, bool declareNodes) {
		directed = edgesAreDirected;
		nodes = declareNodes ? new HashSet<string>() : null;
	}

	// Commands

	
	protected void continueParsingInInclude(){
		// Should not happen, EDGE files cannot be nested.
	}

	
	public bool nextEvents(){
		string id1 = getWordOrNumberOrStringOrEolOrEof();

		if (id1.Equals("EOL")) {
			// Empty line.
		} else if (id1.Equals("EOF")) {
			return false;
		} else {
			declareNode(id1);

			string id2 = getWordOrNumberOrStringOrEolOrEof();

			while (!id2.Equals("EOL")) {
				if (!id1.Equals(id2)) {
					string edgeId = int.toString(edgeid++);

					declareNode(id2);
					sendEdgeAdded(graphName, edgeId, id1, id2, directed);
				}

				id2 = getWordOrNumberOrStringOrEolOrEof();
			}
		}

		return true;
	}

	protected void declareNode(string id) {
		if (nodes != null) {
			if (!nodes.Contains(id)) {
				sendNodeAdded(graphName, id);
				nodes.Add(id);
			}
		}
	}

	
	public void begin(string filename){
		base.begin(filename);
		init();
	}

	
	public void begin(System.Uri url){
		base.begin(url);
		init();
	}

	
	public void begin(System.IO.Stream stream){
		base.begin(stream);
		init();
	}

	
	public void begin(System.IO.TextReader reader){
		base.begin(reader);
		init();
	}

	protected void init(){
		st.eolIsSignificant(true);
		st.commentChar('#');

		graphName = string.Format("{0}_{1}", graphName, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ((long) new Random().NextDouble() * 10));
	}

	public bool nextStep(){
		return nextEvents();
	}

	
	public void end(){
		base.end();
	}
}
}
