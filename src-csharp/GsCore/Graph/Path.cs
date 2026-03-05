using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Graph
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
/// Path description. <p> A path is a class that stores ordered lists of nodes and links that are adjacent. Such a path may be manipulated with nodes and/or edges added or removed. This class is designed as a dynamic structure that is, to add edges during the construction of the path. Only edges need to be added, the nodes list is maintained automatically. </p> <p> The two lists (one for nodes, one for edges) may be acceded at any moment in constant time. </p> <p> The constraint of this class is that it needs to know the first node of the path (the root). This root can be set with the {@link #setRoot(Node)} method or by using the {@link #add(Node, Edge)} method. </p> <p> The normal use with this class is to first use the {@link #setRoot(Node)} method to initialize the path; then to use the {@link #add(Edge)} method to grow it and the {@link #popEdge()} or {@link #popNode()}.
/// </summary>
public class Path : IStructure {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	// ------------- ATTRIBUTES ------------

	/// <summary>
/// The root of the path;
/// </summary>
	private INode root = null;

	/// <summary>
/// The list of edges that represents the path.
/// </summary>
	Stack<IEdge> edgePath;

	/// <summary>
/// The list of nodes representing the path.
/// </summary>
	Stack<INode> nodePath;

	// ------------- CONSTRUCTORS ------------

	/// <summary>
/// New empty path.
/// </summary>
	public Path() {
		edgePath = new Stack<IEdge>();
		nodePath = new Stack<INode>();
	}

	/// <summary>
/// Get the root (the first node) of the path.
/// </summary>
/// <returns>the root of the path.</returns>
	public INode getRoot() {
		return this.root;
	}

	/// <summary>
/// Set the root (first node) of the path.
/// </summary>
/// <param name="root"> The root of the path.</param>
	public void setRoot(INode root) {
		if (this.root == null) {
			this.root = root;
			nodePath.Push(root);
		} else {
			Console.Error.WriteLine("Root node is not null - first use the clear method.");
		}
	}

	/// <summary>
/// Says whether the path contains this node or not.
/// </summary>
/// <param name="node"> The node tested for existence in the path.</param>
/// <returns><code>true</code> if the path contains the node.</returns>
	public bool contains(INode node) {
		return nodePath.Contains(node);
	}

	/// <summary>
/// Says whether the path contains this edge or not.
/// </summary>
/// <param name="edge"> The edge tested for existence in the path.</param>
/// <returns><code>true</code> if the path contains the edge.</returns>
	public bool contains(IEdge edge) {
		return edgePath.Contains(edge);
	}

	/// <summary>
/// Returns true if the path is empty.
/// </summary>
/// <returns><code>true</code> if the path is empty.</returns>
	public bool empty() {
		return nodePath.empty();
	}

	/// <summary>
/// Returns the size of the path
/// </summary>
	public int size() {
		return nodePath.Count;
	}

	/// <summary>
/// It returns the sum of the <code>characteristic</code> given value in the Edges of the path.
/// </summary>
/// <param name="characteristic"> The characteristic.</param>
/// <returns>Sum of the characteristics.</returns>
	public double getPathWeight(string characteristic) {
		double d = 0;
		foreach (IEdge l in edgePath) {
			d += (double) l.getAttribute(characteristic, typeof(IConvertible));
		}
		return d;
	}

	/// <summary>
/// Returns the list of edges representing the path.
/// </summary>
/// <returns>The list of edges representing the path.</returns>
	public List<IEdge> getEdgePath() {
		return edgePath;
	}

	/// <summary>
/// Construct an return a list of nodes that represents the path.
/// </summary>
/// <returns>A list of nodes representing the path.</returns>
	public List<INode> getNodePath() {
		return nodePath;
	}


	/// <summary>
/// Adds a node and an edge to the path. If root is not set, the node will be set as root. Otherwise from node must be the same as the head node of the path.
/// </summary>
/// <param name="from"> The start node.</param>
/// <param name="edge"> The edge used.</param>
	public void add(INode from, IEdge edge) {
		if (root == null) {
			if (from == null) {
				throw new ArgumentException("From node cannot be null.");
			} else {
				setRoot(from);
			}
		}

		if (from == null) {
			from = nodePath.Peek();
		}

		if (!nodePath.Peek().Equals(from)) {
			throw new ArgumentException("From node must be at the head of the path");
		}

		if (!edge.getSourceNode().Equals(from) && !edge.getTargetNode().Equals(from)) {
			throw new ArgumentException("From node must be part of the edge");
		}

		nodePath.Push(edge.getOpposite(from));
		edgePath.Push(edge);
	}

	/// <summary>
/// Adds an edge to the path.
/// </summary>
/// <param name="edge"> The edge to add to the path.</param>
	public void add(IEdge edge) {
		if (nodePath.Length == 0) {
			add(null, edge);
		} else {
			add(nodePath.Peek(), edge);
		}
	}

	/// <summary>
/// A synonym for {@link #add(Edge)}.
/// </summary>
	public void push(INode from, IEdge edge) {
		add(from, edge);
	}

	/// <summary>
/// A synonym for {@link #add(Edge)}.
/// </summary>
	public void push(IEdge edge) {
		add(edge);
	}

	/// <summary>
/// This methods pops the 2 stacks (<code>edgePath</code> and <code>nodePath</code>) and returns the removed edge.
/// </summary>
/// <returns>The edge that have just been removed.</returns>
	public IEdge popEdge() {
		nodePath.Pop();
		return edgePath.Pop();
	}

	/// <summary>
/// This methods pops the 2 stacks (<code>edgePath</code> and <code>nodePath</code>) and returns the removed node.
/// </summary>
/// <returns>The node that have just been removed.</returns>
	public INode popNode() {
		edgePath.Pop();
		return nodePath.Pop();
	}

	/// <summary>
/// Looks at the node at the top of the stack without removing it from the stack.
/// </summary>
/// <returns>The node at the top of the stack.</returns>
	public INode peekNode() {
		return nodePath.Peek();
	}

	/// <summary>
/// Looks at the edge at the top of the stack without removing it from the stack.
/// </summary>
/// <returns>The edge at the top of the stack.</returns>

	public IEdge peekEdge() {
		return edgePath.Peek();
	}

	/// <summary>
/// Clears the path;
/// </summary>
	public void clear() {
		nodePath.Clear();
		edgePath.Clear();
		// Runtime.getRuntime().gc();
		root = null;
	}

	/// <summary>
/// Get a copy of this path
/// </summary>
/// <returns>A copy of this path.</returns>
	
	public Path getACopy() {
		Path newPath = new Path();
		newPath.root = this.root;
		newPath.edgePath = (Stack<IEdge>) edgePath.clone();
		newPath.nodePath = (Stack<INode>) nodePath.clone();

		return newPath;
	}

	/// <summary>
/// Remove all parts of the path that start at a given node and pass a new at this node.
/// </summary>
	public void removeLoops() {
		int n = nodePath.Count;
		// For each node-edge pair
		for (int i = 0; i < n; i++) {
			// Lookup each other following node. We start
			// at the end to find the largest loop possible.
			for (int j = n - 1; j > i; j--) {
				// If another node match, this is a loop.
				if (nodePath[i] == nodePath[j]) {
					// We found a loop between i and j.
					// Remove ]i,j].
					for (int k = i + 1; k <= j; k++) {
						nodePath.Remove(i + 1);
						edgePath.Remove(i);
					}
					n -= (j - i);
					j = i; // To stop the search.
				}
			}
		}
	}

	/// <summary>
/// Compare the content of the current path and the specified path to decide weather they are equal or not.
/// </summary>
/// <param name="p"> A path to compare to the curent one.</param>
/// <returns>True if both paths are equal.</returns>
	public bool equals(Path p) {
		if (nodePath.Count != p.nodePath.Count) {
			return false;
		} else {
			for (int i = 0; i < nodePath.Count; i++) {
				if (nodePath[i] != p.nodePath[i]) {
					return false;
				}
			}
		}
		return true;
	}

	// ------------ UTILITY METHODS ------------

	/// <summary>
/// Returns a String description of the path.
/// </summary>
/// <returns>A String representation of the path.</returns>
	
	public string toString() {
		return nodePath.ToString();
	}

	/// <summary>
/// Returns the size of the path. Identical to {@link #size()}.
/// </summary>
/// <returns>The size of the path.</returns>
	
	public int getNodeCount() {
		return nodePath.Count;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.graph.Structure#getEdgeCount()
	 */
	
	public int getEdgeCount() {
		return edgePath.Count;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Structure#nodes()
	 */
	
	public IEnumerable<INode> nodes() {
		return nodePath;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Structure#edges()
	 */
	
	public IEnumerable<IEdge> edges() {
		return edgePath;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Structure#getNodeSet()
	 */
	
	public ICollection<T> getNodeSet<T>() where T : INode {
		return (ICollection<T>) nodePath;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.graph.Structure#getEdgeSet()
	 */
	
	public ICollection<T> getEdgeSet<T>() where T : IEdge {
		return (ICollection<T>) edgePath;
	}
}
}
