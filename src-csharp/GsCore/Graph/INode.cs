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
/// An Interface that advises general purpose methods for handling nodes as elements of a graph. <h3>Important</h3> <p> Implementing classes should indicate the complexity of their implementation for each method. </p>
/// </summary>
public interface INode : IElement, IEnumerable<IEdge> {
	/// <summary>
/// Parent graph. Some elements are not able to give their parent graph.
/// </summary>
/// <returns>The graph containing this node or null if unknown.</returns>
	IGraph getGraph();

	/// <summary>
/// Total number of relations with other nodes or this node.
/// </summary>
/// <returns>The number of edges/relations/links.</returns>
	int getDegree();

	/// <summary>
/// Number of leaving edges.
/// </summary>
/// <returns>the count of edges that only leave this node plus all undirected edges.</returns>
	int getOutDegree();

	/// <summary>
/// Number of entering edges.
/// </summary>
/// <returns>the count of edges that only enter this node plus all undirected edges.</returns>
	int getInDegree();

	/// <summary>
/// Retrieve an edge that leaves this node toward 'id'. <p> This method selects only edges leaving this node an pointing at node 'id' (this also selects undirected edges). </p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdgeToward(&quot;...&quot;); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p>
/// </summary>
/// <param name="id"> Identifier of the target node.</param>
/// <returns>Directed edge going from this node to 'id', or undirected edge if it exists, else null.</returns>
	IEdge getEdgeToward(string id);

	/// <summary>
/// Retrieve an edge that leaves node 'id' toward this node. <p> This method selects only edges leaving node 'id' an pointing at this node (this also selects undirected edges). </p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdgeFrom(&quot;...&quot;); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p>
/// </summary>
/// <param name="id"> Identifier of the source node.</param>
/// <returns>Directed edge going from node 'id' to this node, or undirected edge if it exists, else null.</returns>
	IEdge getEdgeFrom(string id);

	/// <summary>
/// Retrieve an edge between this node and the node 'id', if it exits. <p> This method selects directed or undirected edges. If the edge is directed, its direction is not important and leaving or entering edges will be selected. </p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdgeBetween(&quot;...&quot;); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p>
/// </summary>
/// <param name="id"> Identifier of the opposite node.</param>
/// <returns>Edge between node 'id' and this node if it exists, else null.</returns>
	IEdge getEdgeBetween(string id);

	/// <summary>
/// Stream over neighbor nodes connected to this node via one or more edges. This iterator iterates across any leaving, entering and non directed edges (nodes are neighbors even if they only have a directed edge from them toward this node). If there are multiple edges connecting the same node, it might be iterated several times.
/// </summary>
/// <returns>The stream, neighbors are streamed in arbitrary order.</returns>
	IEnumerable<INode> neighborNodes();

	/// <summary>
/// I-th edge. Edges are stored in no given order. <p> However this method allows to iterate very quickly on all edges, or to choose a given edge with direct access. </p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdge(i); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p> if <code>i</code> is negative or greater than or equal to the degree
/// </summary>
/// <param name="i"> Index of the edge.</param>
/// <returns>The i-th edge.</returns>
	IEdge getEdge(int i);

	/// <summary>
/// I-th entering edge. Edges are stored in no given order. <p> However this method allows to iterate very quickly on all entering edges, or to choose a given entering edge with direct access. </p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEnteringEdge(i); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p> if <code>i</code> is negative or greater than or equal to the in-degree
/// </summary>
/// <param name="i"> Index of the edge.</param>
/// <returns>The i-th entering edge.</returns>
	IEdge getEnteringEdge(int i);

	/// <summary>
/// I-th leaving edge. Edges are stored in no given order. <p> However this method allows to iterate very quickly on all leaving edges, or to choose a given leaving edge with direct access. </p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getLeavingEdge(i); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p> if <code>i</code> is negative or greater than or equal to the out-degree
/// </summary>
/// <param name="i"> Index of the edge.</param>
/// <returns>The i-th leaving edge.</returns>
	IEdge getLeavingEdge(int i);

	/// <summary>
/// Iterator for breadth first exploration of the graph, starting at this node. <p> If the graph is not connected, only a part of it will be explored. By default, this iterator will respect edge orientation. </p> <p> This method is implicitly generic and return an Iterator over something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <pre> Iterator&lt;ExtendedNode&gt; ite = node.getBreadthFirstIterator(); </pre> the method will return an Iterator&lt;ExtendedNode&gt;. If no left part exists, method will just return an Iterator&lt;Node&gt;. </p>
/// </summary>
/// <returns>An iterator able to explore the graph in a breadth first way starting at this node.</returns>
	IEnumerator<INode> getBreadthFirstIterator();

	/// <summary>
/// Iterator for breadth first exploration of the graph, starting at this node. <p> If the graph is not connected, only a part of it will be explored. </p> <p> This method is implicitly generic and return an Iterator over something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <pre> Iterator&lt;ExtendedNode&gt; ite = node.getBreadthFirstIterator(true); </pre> the method will return an Iterator&lt;ExtendedNode&gt;. If no left part exists, method will just return an Iterator&lt;Node&gt;. </p>
/// </summary>
/// <param name="directed"> If false, the iterator will ignore edge orientation (the is "True").</param>
/// <returns>An iterator able to explore the graph in a breadth first way starting at this node.</returns>
	IEnumerator<INode> getBreadthFirstIterator(bool directed);

	/// <summary>
/// Iterator for depth first exploration of the graph, starting at this node. <p> If the graph is not connected, only a part of it will be explored. By default, this iterator will respect edge orientation. </p> <p> This method is implicitly generic and return an Iterator over something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <pre> Iterator&lt;ExtendedNode&gt; ite = node.getDepthFirstIterator(); </pre> the method will return an Iterator&lt;ExtendedNode&gt;. If no left part exists, method will just return an Iterator&lt;Node&gt;. </p> m the number of edges.
/// </summary>
/// <returns>An iterator able to explore the graph in a depth first way starting at this node.</returns>
	IEnumerator<INode> getDepthFirstIterator();

	/// <summary>
/// Iterator for depth first exploration of the graph, starting at this node. <p> If the graph is not connected, only a part of it will be explored. </p> <p> This method is implicitly generic and return an Iterator over something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <pre> Iterator&lt;ExtendedNode&gt; ite = node.getDepthFirstIterator(true); </pre> the method will return an Iterator&lt;ExtendedNode&gt;. If no left part exists, method will just return an Iterator&lt;Node&gt;. </p>
/// </summary>
/// <param name="directed"> If false, the iterator will ignore edge orientation (the is "True").</param>
/// <returns>An iterator able to explore the graph in a depth first way starting at this node.</returns>
	IEnumerator<INode> getDepthFirstIterator(bool directed);

	/// <summary>
/// Stream over all entering and leaving edges.
/// </summary>
/// <returns>A stream over all directed and undirected edges, leaving or entering.</returns>
	IEnumerable<IEdge> edges();

	/// <summary>
/// Stream over all leaving edges.
/// </summary>
/// <returns>A stream over only edges that leave this node plus all undirected edges.</returns>
	IEnumerable<IEdge> leavingEdges();

	/// <summary>
/// Stream over all entering edges.
/// </summary>
/// <returns>A stream over only edges that enter this node plus all undirected edges.</returns>
	IEnumerable<IEdge> enteringEdges();

	
	IEnumerator<IEdge> iterator();

	/// <summary>
/// Override the object.toString() method.
/// </summary>
	string toString();

	// New methods

	/// <summary>
/// True if an edge leaves this node toward node 'id'.
/// </summary>
/// <param name="id"> Identifier of the target node.</param>
/// <returns>True if a directed edge goes from this node to 'id' or if an undirected edge exists.</returns>
	bool hasEdgeToward(string id);

	/// <summary>
/// True if an edge leaves this node toward a given node.
/// </summary>
/// <param name="node"> The target node.</param>
/// <returns>True if a directed edge goes from this node to the other node or if an undirected edge exists.</returns>
	bool hasEdgeToward(INode node);

	/// <summary>
/// True if an edge leaves this node toward a node with given index. if the index is negative or greater than {@code getNodeCount() - 1}.
/// </summary>
/// <param name="index"> Index of the target node.</param>
/// <returns>True if a directed edge goes from this node to the other node or if an undirected edge exists.</returns>
	bool hasEdgeToward(int index);

	/// <summary>
/// True if an edge enters this node from node 'id'.
/// </summary>
/// <param name="id"> Identifier of the source node.</param>
/// <returns>True if a directed edge goes from this node to 'id' or if an undirected edge exists.</returns>
	bool hasEdgeFrom(string id);

	/// <summary>
/// True if an edge enters this node from a given node.
/// </summary>
/// <param name="node"> The source node.</param>
/// <returns>True if a directed edge goes from the other node to this node or if an undirected edge exists.</returns>
	bool hasEdgeFrom(INode node);

	/// <summary>
/// True if an edge enters this node from a node with given index. if the index is negative or greater than {@code getNodeCount() - 1}.
/// </summary>
/// <param name="index"> Index of the source node.</param>
/// <returns>True if a directed edge goes from the other node to this node or if an undirected edge exists.</returns>
	bool hasEdgeFrom(int index);

	/// <summary>
/// True if an edge exists between this node and node 'id'.
/// </summary>
/// <param name="id"> Identifier of another node.</param>
/// <returns>True if a edge exists between this node and node 'id'.</returns>
	bool hasEdgeBetween(string id);

	/// <summary>
/// True if an edge exists between this node and another node.
/// </summary>
/// <param name="node"> Another node.</param>
/// <returns>True if an edge exists between this node and the other node.</returns>
	bool hasEdgeBetween(INode node);

	/// <summary>
/// True if an edge exists between this node and a node with given index. if the index is negative or greater than {@code getNodeCount() - 1}.
/// </summary>
/// <param name="index"> Index of another node.</param>
/// <returns>True if an edge exists between this node and the other node.</returns>
	bool hasEdgeBetween(int index);

	/// <summary>
/// Retrieves an edge that leaves this node toward another node. <p> This method selects only edges leaving this node an pointing at the parameter node (this also selects undirected edges). </p> <p> This method is implicitly generic and returns something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdgeToward(...); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p>
/// </summary>
/// <param name="node"> The target node.</param>
/// <returns>Directed edge going from this node to the parameter node, or undirected edge if it exists, else null.</returns>
	IEdge getEdgeToward(INode node);

	/// <summary>
/// Retrieves an edge that leaves this node toward the node with given index. <p> This method selects only edges leaving this node an pointing at the parameter node (this also selects undirected edges). </p> <p> This method is implicitly generic and returns something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdgeToward(...); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p> if the index is negative or greater than {@code getNodeCount() - 1}.
/// </summary>
/// <param name="index"> Index of the target node.</param>
/// <returns>Directed edge going from this node to the parameter node, or undirected edge if it exists, else null.</returns>
	IEdge getEdgeToward(int index);

	/// <summary>
/// Retrieves an edge that leaves given node toward this node. <p> This method selects only edges leaving the other node an pointing at this node (this also selects undirected edges). </p> <p> This method is implicitly generic and returns something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdgeFrom(...); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p>
/// </summary>
/// <param name="node"> The source node.</param>
/// <returns>Directed edge going from the parameter node to this node, or undirected edge if it exists, else null.</returns>
	IEdge getEdgeFrom(INode node);

	/// <summary>
/// Retrieves an edge that leaves node with given index toward this node. <p> This method selects only edges leaving the other node an pointing at this node (this also selects undirected edges). </p> <p> This method is implicitly generic and returns something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdgeFrom(&quot;...&quot;); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p> if the index is negative or greater than {@code getNodeCount() - 1}.
/// </summary>
/// <param name="index"> Index of the source node.</param>
/// <returns>Directed edge going from the parameter node to this node, or undirected edge if it exists, else null.</returns>
	IEdge getEdgeFrom(int index);

	/// <summary>
/// Retrieves an edge between this node and and another node if one exists. <p> This method selects directed or undirected edges. If the edge is directed, its direction is not important and leaving or entering edges will be selected. </p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdgeBetween(...); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p>
/// </summary>
/// <param name="node"> The opposite node.</param>
/// <returns>Edge between this node and the parameter node if it exists, else null.</returns>
	IEdge getEdgeBetween(INode node);

	/// <summary>
/// Retrieves an edge between this node and the node with index i if one exists. <p> This method selects directed or undirected edges. If the edge is directed, its direction is not important and leaving or entering edges will be selected. </p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <pre> ExtendedEdge e = node.getEdgeBetween(...); </pre> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p> if the index is negative or greater than {@code getNodeCount() - 1}.
/// </summary>
/// <param name="index"> The index of the opposite node.</param>
/// <returns>Edge between node with index i and this node if it exists, else null.</returns>
	IEdge getEdgeBetween(int index);

}
}
