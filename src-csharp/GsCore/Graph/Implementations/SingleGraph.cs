using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Graph.Implementations
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
/// An implementation of graph that supports only one edge between two nodes.
/// </summary>
public class SingleGraph : AdjacencyListGraph {

	/// <summary>
/// Creates an empty graph.
/// </summary>
/// <param name="id"> Unique identifier of the graph.</param>
/// <param name="strictChecking"> If true any non-fatal error throws an exception.</param>
/// <param name="autoCreate"> If true (and strict checking is false), nodes are automatically created when referenced when creating a edge, even if not yet inserted in the graph.</param>
/// <param name="initialNodeCapacity"> Initial capacity of the node storage data structures. Use this if you know the approximate maximum number of nodes of the graph. The graph can grow beyond this limit, but storage reallocation is expensive operation.</param>
/// <param name="initialEdgeCapacity"> Initial capacity of the edge storage data structures. Use this if you know the approximate maximum number of edges of the graph. The graph can grow beyond this limit, but storage reallocation is expensive operation.</param>
	public SingleGraph(string id, bool strictChecking, bool autoCreate, int initialNodeCapacity,
			int initialEdgeCapacity) {
		base(id, strictChecking, autoCreate, initialNodeCapacity, initialEdgeCapacity);
		// All we need to do is to change the node factory
		setNodeFactory(null /* TODO: implement INodeFactory<SingleNode> */);
	}

	/// <summary>
/// Creates an empty graph with default edge and node capacity.
/// </summary>
/// <param name="id"> Unique identifier of the graph.</param>
/// <param name="strictChecking"> If true any non-fatal error throws an exception.</param>
/// <param name="autoCreate"> If true (and strict checking is false), nodes are automatically created when referenced when creating a edge, even if not yet inserted in the graph.</param>
	public SingleGraph(string id, bool strictChecking, bool autoCreate) : this(id, strictChecking, autoCreate, DEFAULT_NODE_CAPACITY, DEFAULT_EDGE_CAPACITY) {
	}

	/// <summary>
/// Creates an empty graph with strict checking and without auto-creation.
/// </summary>
/// <param name="id"> Unique identifier of the graph.</param>
	public SingleGraph(string id) : this(id, true, false) {
	}

}

}
