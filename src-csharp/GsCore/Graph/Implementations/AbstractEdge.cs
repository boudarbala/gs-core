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
/// <p> This class provides a basic implementation of {@code Edge} interface, to minimize the effort required to implement this interface. </p> <p> Although this class is abstract it : all the methods of {@link org.graphstream.graph.Edge} and {@link org.graphstream.graph.implementations.AbstractElement}. It has a low memory overhead (3 references and a boolean as fields). All {@code Edge} methods are executed in O(1) time. </p>
/// </summary>
public class AbstractEdge : AbstractElement, IEdge {

	// *** Fields ***

	/// <summary>
/// The source node
/// </summary>
	protected AbstractNode source;

	/// <summary>
/// The target node
/// </summary>
	protected AbstractNode target;

	/// <summary>
/// Is this edge directed ?
/// </summary>
	protected bool directed;

	/// <summary>
/// The graph to which this edge belongs
/// </summary>
	protected AbstractGraph graph;

	// *** Constructors ***

	/// <summary>
/// Constructs a new edge. This constructor copies the parameters into the corresponding fields.
/// </summary>
/// <param name="id"> Unique identifier of this edge.</param>
/// <param name="source"> Source node.</param>
/// <param name="target"> Target node.</param>
/// <param name="directed"> Indicates if the edge is directed.</param>
	protected AbstractEdge(string id, AbstractNode source, AbstractNode target, bool directed) : base(id) {
		System.Diagnostics.Debug.Assert(source != null && target != null, "An edge cannot have null endpoints");
		this.source = source;
		this.target = target;
		this.directed = directed;
		this.graph = (AbstractGraph) source.getGraph();
	}

	// *** Inherited from AbstractElement ***

	
	protected void attributeChanged(AttributeChangeEvent evt, string attribute, object oldValue, object newValue) {
		graph.listeners.sendAttributeChangedEvent(id, ElementType.EDGE, attribute, evt, oldValue, newValue);
	}

	
	public string toString() {
		return string.Format("{0}[{1}-{2}{3}]", getId(), source, directed ? ">" : "-", target);
	}

	// *** Inherited from Edge ***

	
	public INode getNode0() {
		return source;
	}

	
	public INode getNode1() {
		return target;
	}

	
	public INode getOpposite(INode node) {
		if (node == source)
			return target;
		if (node == target)
			return source;
		return null;
	}

	
	public INode getSourceNode() {
		return source;
	}

	
	public INode getTargetNode() {
		return target;
	}

	public bool isDirected() {
		return directed;
	}

	public bool isLoop() {
		return source == target;
	}
}

}
