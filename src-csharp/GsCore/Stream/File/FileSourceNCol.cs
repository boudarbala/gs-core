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
/// Reader for the "ncol" graph format. <p> The ncol graph format is a simple format where each line describes an edge by giving two node names and an optional third parameters giving the edge weight. The nodes are created implicitly. </p> <p> Also, the format does not specify any direction for edges. By default all edges are undirected. It is specified in the format that you will never have directed edges and that the lines: <pre> node1Name node2Name </pre> and <pre> node2Name node1Name </pre> Cannot both appear at the same time in a file. </p> <p> This format only contains edges. To ensure the "add node" events are sent before an edge referencing two nodes is created via an "add edge" event, this reader has a hash set of already encountered nodes. The hash set allows to issue "add node" events only when a node is encountered for the first time. </p> </p> This hash set consumes memory, but is the only way to ensure "add node" events are correctly issued. If this input is directly connected to a graph, as graphs can create non-existing nodes automatically, you can disable the hash set of nodes using the constructor {@link #FileSourceNCol(boolean)}, and giving "false" for the first argument. </p> The usual file name extension for this format is ".ncol".
/// </summary>
public class FileSourceNCol : FileSourceBase {
	// Attribute

	/// <summary>
/// Allocator for edge identifiers.
/// </summary>
	protected int edgeid = 0;

	/// <summary>
/// Set of existing nodes (if nodes are declared).
/// </summary>
	protected HashSet<string> nodes;

	protected string graphName = "NCOL_";

	// Construction

	/// <summary>
/// New reader for the "ncol" format.
/// </summary>
	public FileSourceNCol() : this(false) {
	}

	/// <summary>
/// New reader for the "ncol" format.
/// </summary>
/// <param name="declareNodes"> If true (default=true) this reader outputs nodeAdded events.</param>
	public FileSourceNCol(bool declareNodes) {
		nodes = declareNodes ? new HashSet<string>() : null;
	}

	// Commands

	
	protected void continueParsingInInclude(){
		// Should not happen, NCol files cannot be nested.
	}

	
	public bool nextEvents(){
		string id1 = getWordOrNumberOrStringOrEolOrEof();

		if (id1.Equals("EOL")) {
			// Empty line. Skip it.
		} else if (id1.Equals("EOF")) {
			return false;
		} else {
			declareNode(id1);

			string id2 = getWordOrNumberOrStringOrEolOrEof();

			if (!id2.Equals("EOL") && !id2.Equals("EOF")) {
				// Loops are not accepted by the format.
				if (!id1.Equals(id2)) {
					// There may be a weight.
					string weight = getWordOrNumberOrStringOrEolOrEof();
					double w = 0.0;

					if (weight.Equals("EOL") || weight.Equals("EOF")) {
						weight = null;
						pushBack();
					} else {
						try {
							w = double.Parse(weight);
						} catch (Exception e) {
							throw new System.IO.IOException(string.Format("cannot transform weight {0} into a number", weight));
						}
					}

					string edgeId = int.toString(edgeid++);

					declareNode(id2);
					sendEdgeAdded(graphName, edgeId, id1, id2, false);

					if (weight != null)
						sendEdgeAttributeAdded(graphName, edgeId, "weight", (double) w);
				}
			} else {
				throw new System.IO.IOException("unexpected EOL or EOF");
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
