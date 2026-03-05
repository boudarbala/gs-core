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
/// <p> A lightweight graph class intended to allow the construction of big graphs (millions of elements). </p> <p> The main purpose here is to minimize memory consumption even if the management of such a graph implies more CPU consuming. See the <code>complexity</code> tags on each method so as to figure out the impact on the CPU. </p>
/// </summary>
public class AdjacencyListGraph : AbstractGraph {

	public static readonly double GROW_FACTOR = 1.1;
	public static readonly int DEFAULT_NODE_CAPACITY = 128;
	public static readonly int DEFAULT_EDGE_CAPACITY = 1024;

	protected Dictionary<string, AbstractNode> nodeMap;
	protected Dictionary<string, AbstractEdge> edgeMap;

	protected AbstractNode[] nodeArray;
	protected AbstractEdge[] edgeArray;

	protected int nodeCount;
	protected int edgeCount;

	// *** Constructors ***

	/// <summary>
/// Creates an empty graph.
/// </summary>
/// <param name="id"> Unique identifier of the graph.</param>
/// <param name="strictChecking"> If true any non-fatal error throws an exception.</param>
/// <param name="autoCreate"> If true (and strict checking is false), nodes are automatically created when referenced when creating a edge, even if not yet inserted in the graph.</param>
/// <param name="initialNodeCapacity"> Initial capacity of the node storage data structures. Use this if you know the approximate maximum number of nodes of the graph. The graph can grow beyond this limit, but storage reallocation is expensive operation.</param>
/// <param name="initialEdgeCapacity"> Initial capacity of the edge storage data structures. Use this if you know the approximate maximum number of edges of the graph. The graph can grow beyond this limit, but storage reallocation is expensive operation.</param>
	public AdjacencyListGraph(string id, bool strictChecking, bool autoCreate, int initialNodeCapacity,
			int initialEdgeCapacity) {
		base(id, strictChecking, autoCreate);

		setNodeFactory(null /* TODO: implement INodeFactory<AdjacencyListNode> */);

		setEdgeFactory(null /* TODO: implement IEdgeFactory<AbstractEdge> */);

		if (initialNodeCapacity < DEFAULT_NODE_CAPACITY)
			initialNodeCapacity = DEFAULT_NODE_CAPACITY;
		if (initialEdgeCapacity < DEFAULT_EDGE_CAPACITY)
			initialEdgeCapacity = DEFAULT_EDGE_CAPACITY;

		nodeMap = new Dictionary<string, AbstractNode>(4 * initialNodeCapacity / 3 + 1);
		edgeMap = new Dictionary<string, AbstractEdge>(4 * initialEdgeCapacity / 3 + 1);
		nodeArray = new AbstractNode[initialNodeCapacity];
		edgeArray = new AbstractEdge[initialEdgeCapacity];
		nodeCount = edgeCount = 0;
	}

	/// <summary>
/// Creates an empty graph with default edge and node capacity.
/// </summary>
/// <param name="id"> Unique identifier of the graph.</param>
/// <param name="strictChecking"> If true any non-fatal error throws an exception.</param>
/// <param name="autoCreate"> If true (and strict checking is false), nodes are automatically created when referenced when creating a edge, even if not yet inserted in the graph.</param>
	public AdjacencyListGraph(string id, bool strictChecking, bool autoCreate) : this(id, strictChecking, autoCreate, DEFAULT_NODE_CAPACITY, DEFAULT_EDGE_CAPACITY) {
	}

	/// <summary>
/// Creates an empty graph with strict checking and without auto-creation.
/// </summary>
/// <param name="id"> Unique identifier of the graph.</param>
	public AdjacencyListGraph(string id) : this(id, true, false) {
	}

	// *** Callbacks ***

	
	protected void addEdgeCallback(AbstractEdge edge) {
		edgeMap[edge.getId()] = edge;
		if (edgeCount == edgeArray.Length) {
			AbstractEdge[] tmp = new AbstractEdge[(int) (edgeArray.Length * GROW_FACTOR) + 1];
			Array.Copy(edgeArray, 0, tmp, 0, edgeArray.Length);
			Array.Clear(edgeArray, 0, edgeArray.Length);
			edgeArray = tmp;
		}
		edgeArray[edgeCount] = edge;
		edge.setIndex(edgeCount++);
	}

	
	protected void addNodeCallback(AbstractNode node) {
		nodeMap[node.getId()] = node;
		if (nodeCount == nodeArray.Length) {
			AbstractNode[] tmp = new AbstractNode[(int) (nodeArray.Length * GROW_FACTOR) + 1];
			Array.Copy(nodeArray, 0, tmp, 0, nodeArray.Length);
			Array.Clear(nodeArray, 0, nodeArray.Length);
			nodeArray = tmp;
		}
		nodeArray[nodeCount] = node;
		node.setIndex(nodeCount++);
	}

	
	protected void removeEdgeCallback(AbstractEdge edge) {
		edgeMap.Remove(edge.getId());
		int i = edge.getIndex();
		edgeArray[i] = edgeArray[--edgeCount];
		edgeArray[i].setIndex(i);
		edgeArray[edgeCount] = null;
	
	}

	
	protected void removeNodeCallback(AbstractNode node) {
		nodeMap.Remove(node.getId());
		int i = node.getIndex();
		nodeArray[i] = nodeArray[--nodeCount];
		nodeArray[i].setIndex(i);
		nodeArray[nodeCount] = null;
	
	}

	
	protected void clearCallback() {
		nodeMap.Clear();
		edgeMap.Clear();
		Array.Clear(nodeArray, 0, nodeCount);
		Array.Clear(edgeArray, 0, edgeCount);
		nodeCount = edgeCount = 0;
	}

	
	public IEnumerable<INode> nodes() {
		return new ArraySegment<object>(nodeArray, 0, nodeCount - 0);
	}

	
	public IEnumerable<IEdge> edges() {
		return new ArraySegment<object>(edgeArray, 0, edgeCount - 0);
	}

	
	public IEdge getEdge(string id) {
		return edgeMap[id];
	}

	
	public IEdge getEdge(int index) {
		if (index < 0 || index >= edgeCount)
			throw new IndexOutOfRangeException("IEdge " + index + " does not exist");
		return edgeArray[index];
	}

	
	public int getEdgeCount() {
		return edgeCount;
	}

	
	public INode getNode(string id) {
		return nodeMap[id];
	}

	
	public INode getNode(int index) {
		if (index < 0 || index > nodeCount)
			throw new IndexOutOfRangeException("INode " + index + " does not exist");
		return nodeArray[index];
	}

	
	public int getNodeCount() {
		return nodeCount;
	}

	// *** Iterators ***

	protected class EdgeIterator<T> : IEnumerator<T> where T : IEdge {
		int iNext = 0;
		int iPrev = -1;

		public bool hasNext() {
			return iNext < edgeCount;
		}

		
		public T next() {
			if (iNext >= edgeCount)
				throw new InvalidOperationException();
			iPrev = iNext++;
			return (T) edgeArray[iPrev];
		}

		public void remove() {
			if (iPrev == -1)
				throw new InvalidOperationException();
			removeEdge(edgeArray[iPrev], true, true, true);
			iNext = iPrev;
			iPrev = -1;
		}
	}

	protected class NodeIterator<T> : IEnumerator<T> where T : INode {
		int iNext = 0;
		int iPrev = -1;

		public bool hasNext() {
			return iNext < nodeCount;
		}

		
		public T next() {
			if (iNext >= nodeCount)
				throw new InvalidOperationException();
			iPrev = iNext++;
			return (T) nodeArray[iPrev];
		}

		public void remove() {
			if (iPrev == -1)
				throw new InvalidOperationException();
			removeNode(nodeArray[iPrev], true);
			iNext = iPrev;
			iPrev = -1;
		}
	}

	/*
	 * For performance tuning
	 * 
	 * @return the number of allocated but unused array elements public int
	 * getUnusedArrayElements() { int count = 0; count += edgeArray.length -
	 * edgeCount; count += nodeArray.length - nodeCount; for (ALNode n :
	 * this.<ALNode> getEachNode()) count += n.edges.length - n.degree; return
	 * count; }
	 */
}

}
