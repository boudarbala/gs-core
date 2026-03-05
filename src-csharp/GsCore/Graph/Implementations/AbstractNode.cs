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
/// <p> This class provides a basic implementation of {@code Node} interface, to minimize the effort required to implement this interface. </p> <p> This class : all the methods of {@link org.graphstream.graph.implementations.AbstractElement} and most of the methods of {@link org.graphstream.graph.Node} (there are "only" ten methods). In addition to these, subclasses must provide implementations for {@link #addEdgeCallback(AbstractEdge)} and {@link #removeEdgeCallback(AbstractEdge)} which are called by the parent graph when an edge incident to this node is added to or removed from the graph. This class has a low memory overhead (one reference as field). </p>
/// </summary>
abstract class AbstractNode : AbstractElement, INode {

	// *** Fields ***

	/// <summary>
/// The graph to which this node belongs
/// </summary>
	protected AbstractGraph graph;

	// *** Constructors

	/// <summary>
/// Constructs a new node. This constructor copies the parameters into the corresponding fields
/// </summary>
/// <param name="graph"> The graph to which this node belongs.</param>
/// <param name="id"> Unique identifier of this node.</param>
	protected AbstractNode(AbstractGraph graph, string id) : base(id) {
		this.graph = graph;
	}

	// *** Inherited from abstract element ***

	
	protected void attributeChanged(AttributeChangeEvent evt, string attribute, object oldValue, object newValue) {
		graph.listeners.sendAttributeChangedEvent(id, SourceBase.ElementType.NODE, attribute, evt, oldValue,
				newValue);
	}

	// *** Inherited from Node ***

	/// <summary>
/// This implementation returns {@link #graph}.
/// </summary>
	public IGraph getGraph() {
		return graph;
	}

	// [has|get]Edge[Toward|From|Between](Node|int|String) -> 2 * 3 * 3 = 18
	// methods

	/// <summary>
/// This implementation uses {@link #getEdgeToward(Node)}
/// </summary>
	
	public IEdge getEdgeToward(int index) {
		return getEdgeToward(graph.getNode(index));
	}

	/// <summary>
/// This implementation uses {@link #getEdgeToward(Node)}
/// </summary>
	
	public IEdge getEdgeToward(string id) {
		return getEdgeToward(graph.getNode(id));
	}

	/// <summary>
/// This implementation uses {@link #getEdgeFrom(Node)}
/// </summary>
	
	public IEdge getEdgeFrom(int index) {
		return getEdgeFrom(graph.getNode(index));
	}

	/// <summary>
/// This implementation uses {@link #getEdgeFrom(Node)}
/// </summary>
	
	public IEdge getEdgeFrom(string id) {
		return getEdgeFrom(graph.getNode(id));
	}

	/// <summary>
/// This implementation uses {@link #getEdgeBetween(Node)}
/// </summary>
	
	public IEdge getEdgeBetween(int index) {
		return getEdgeBetween(graph.getNode(index));
	}

	/// <summary>
/// This implementation uses {@link #getEdgeBetween(Node)}
/// </summary>
	
	public IEdge getEdgeBetween(string id) {
		return getEdgeBetween(graph.getNode(id));
	}

	/// <summary>
/// This implementation creates an instance of {@link org.graphstream.graph.BreadthFirstIterator} and returns it.
/// </summary>
	
	public IEnumerator<INode> getBreadthFirstIterator() {
		// XXX change it when the old iterator disappears
		// XXX change the return type to have access to the other methods
		return new BreadthFirstIterator(this);
	}

	/// <summary>
/// This implementation creates an instance of {@link org.graphstream.graph.BreadthFirstIterator} and returns it.
/// </summary>
	
	public IEnumerator<INode> getBreadthFirstIterator(bool directed) {
		// XXX change it when the old iterator disappears
		// XXX change the return type to have access to the other methods
		return new BreadthFirstIterator(this, directed);
	}

	/// <summary>
/// This implementation creates an instance of {@link org.graphstream.graph.DepthFirstIterator} and returns it.
/// </summary>
	
	public IEnumerator<INode> getDepthFirstIterator() {
		// XXX change it when the old iterator disappears
		// XXX change the return type to have access to the other methods
		return new DepthFirstIterator(this);
	}

	/// <summary>
/// This implementation creates an instance of {@link org.graphstream.graph.DepthFirstIterator} and returns it.
/// </summary>
	
	public IEnumerator<INode> getDepthFirstIterator(bool directed) {
		// XXX change it when the old iterator disappears
		// XXX change the return type to have access to the other methods
		return new DepthFirstIterator(this, directed);
	}

	// *** Other methods ***

	/// <summary>
/// This method is called automatically when an edge incident to this node is created. Subclasses use it to add the edge to their data structure.
/// </summary>
/// <param name="edge"> a new edge incident to this node</param>
	protected abstract bool addEdgeCallback(AbstractEdge edge);

	/// <summary>
/// This method is called automatically before removing an edge incident to this node. Subclasses use it to remove the edge from their data structure.
/// </summary>
/// <param name="edge"> an edge incident to this node that will be removed</param>
	protected abstract void removeEdgeCallback(AbstractEdge edge);

	/// <summary>
/// This method is called for each node when the graph is cleared. Subclasses may use it to clear their data structures in order to facilitate the garbage collection.
/// </summary>
	protected abstract void clearCallback();

	/// <summary>
/// Checks if an edge enters this node. Utility method that can be useful in subclasses.
/// </summary>
/// <param name="e"> an edge</param>
/// <returns>{@code true} if {@code e} is entering edge for this node.</returns>
	public bool isEnteringEdge(IEdge e) {
		return e.getTargetNode() == this || (!e.isDirected() && e.getSourceNode() == this);
	}

	/// <summary>
/// Checks if an edge leaves this node. Utility method that can be useful in subclasses.
/// </summary>
/// <param name="e"> an edge</param>
/// <returns>{@code true} if {@code e} is leaving edge for this node.</returns>
	public bool isLeavingEdge(IEdge e) {
		return e.getSourceNode() == this || (!e.isDirected() && e.getTargetNode() == this);
	}

	/// <summary>
/// Checks if an edge is incident to this node. Utility method that can be useful in subclasses.
/// </summary>
/// <param name="e"> an edge</param>
/// <returns>{@code true} if {@code e} is incident edge for this node.</returns>
	public bool isIncidentEdge(IEdge e) {
		return e.getSourceNode() == this || e.getTargetNode() == this;
	}
}

}
