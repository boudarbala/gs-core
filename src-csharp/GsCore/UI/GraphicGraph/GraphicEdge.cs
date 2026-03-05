using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.GraphicGraph
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
/// Graphical edge. <p> The graphic edge defines its source and target node as well as a direction, a string label and a style from the style sheet. </p>
/// </summary>
public class GraphicEdge : GraphicElement, IEdge {
	// Attributes

	/// <summary>
/// The first node.
/// </summary>
	public GraphicNode from;

	/// <summary>
/// The second node.
/// </summary>
	public GraphicNode to;

	/// <summary>
/// Is the edge directed ?.
/// </summary>
	public bool directed;

	/// <summary>
/// In case of a multi-graph this is the index of the edge between to and from.
/// </summary>
	public int multi;

	/// <summary>
/// If non null, this gives the number of edges between the two same nodes.
/// </summary>
	public EdgeGroup group;

	/// <summary>
/// Control points for curved edges or polylines. This contains the control points of an edge. If the edge is in 2D each sequence of two cells gives the x and y coordinates of a control point. Else each sequence of three cells gives the x, y and z coordinates. Therefore the number of control points can be obtained by dividing by 2 or 3 the length of this array. For example for cubic Bezier curves in 2D this array contains four cells. The control points are ordered from node0 to node1.
/// </summary>
	public double[] ctrl;

	// Constructors

	/// <summary>
/// New graphic edge.
/// </summary>
/// <param name="id"> The edge unique identifier.</param>
/// <param name="from"> The source node.</param>
/// <param name="to"> The target node.</param>
/// <param name="dir"> True if the edge is directed in the direction from-to.</param>
/// <param name="attributes"> A set of initial attributes.</param>
	public GraphicEdge(string id, GraphicNode from, GraphicNode to, bool dir, Dictionary<string, object> attributes) : base(id, from.mygraph) {

		this.from = from;
		this.to = to;
		this.directed = dir;

		if (this.attributes == null)
			this.attributes = new Dictionary<string, object>();

		if (attributes != null)
			setAttributes(attributes);
	}

	
	public Selector.Type getSelectorType() {
		return Selector.Type.EDGE;
	}

	/// <summary>
/// Obtain the node that is not "n" attached to this edge.
/// </summary>
/// <param name="n"> One of the node of this edge.</param>
/// <returns>The other node of this edge.</returns>
	public GraphicNode otherNode(GraphicNode n) {
		return (GraphicNode) getOpposite(n);
	}

	
	public double getX() {
		return from.x + ((to.x - from.x) / 2);
	}

	
	public double getY() {
		return from.y + ((to.y - from.y) / 2);
	}

	
	public double getZ() {
		return from.z + ((to.z - from.z) / 2);
	}

	/// <summary>
/// Control points for curved edges or polylines. This contains the control points of an edge. If the edge is in 2D each sequence of two cells gives the x and y coordinates of a control point. Else each sequence of three cells gives the x, y and z coordinates. Therefore the number of control points can be obtained by dividing by 2 or 3 the length of this array. For example for cubic Bezier curves in 2D this array contains four cells. The control points are ordered from node0 to node1. The units are "graph units".
/// </summary>
/// <returns>The control points coordinates or null if this edge is a straight line.</returns>
	public double[] getControlPoints() {
		return ctrl;
	}

	/// <summary>
/// True if the the edge defines control points to draw a curve or polyline. This does not mean the edge style asks to paint the edge as a curve, only that control points are defined.
/// </summary>
/// <returns>True if control points are available.</returns>
	public bool isCurve() {
		return ctrl != null;
	}

	/// <summary>
/// Change the control points array for this edge.
/// </summary>
/// <param name="points"> The new set of points. See the {@link #getControlPoints()} method for an explanation on the organisation of this array.</param>
	public void setControlPoints(double points[]) {
		ctrl = points;
	}

	/// <summary>
/// This edge is the i-th between the two same nodes.
/// </summary>
/// <returns>The edge index between the two nodes if there are several such edges.</returns>
	public int getMultiIndex() {
		return multi;
	}

	
	public void move(double x, double y, double z) {
		// NOP on edges !!!
	}

	
	protected void attributeChanged(AttributeChangeEvent evt, string attribute, object oldValue, object newValue) {
		base.attributeChanged(evt, attribute, oldValue, newValue);

		if (attribute.StartsWith("ui.sprite.")) {
			mygraph.spriteAttribute(evt, this, attribute, newValue);
		}

		mygraph.listeners.sendAttributeChangedEvent(getId(), ElementType.EDGE, attribute, evt, oldValue, newValue);
	}

	/// <summary>
/// Count the number of identical edges between the two nodes of this edge and create or update the edge group. The edge group contains all the edges between two same nodes and allows to render faster multiple edges in a multigraph.
/// </summary>
/// <param name="edgeList"> The actual set of edges between two nodes (see the connectivity in the graphic graph).</param>
	protected void countSameEdges(IEnumerable<GraphicEdge> edgeList) {
		foreach (GraphicEdge other in edgeList) {
			if (other != this) {
				if ((other.from == from && other.to == to) || (other.to == from && other.from == to)) {
					group = other.group;

					if (group == null)
						group = new EdgeGroup(other, this);
					else
						group.increment(this);

					break;
				}
			}
		}
	}

	
	public void removed() {
		if (group != null) {
			group.decrement(this);

			if (group.getCount() == 1)
				group = null;
		}
	}

	// Edge interface

	
	public INode getNode0() {
		return from;
	}

	
	public INode getNode1() {
		return to;
	}

	/// <summary>
/// If there are several edges between two nodes, this edge pertains to a group. Else this method returns null.
/// </summary>
/// <returns>The group of edges between two same nodes, null if the edge is alone between the two nodes.</returns>
	public EdgeGroup getGroup() {
		return group;
	}

	
	public INode getOpposite(INode node) {
		if (node == from)
			return to;

		return from;
	}

	
	public INode getSourceNode() {
		return from;
	}

	
	public INode getTargetNode() {
		return to;
	}

	public bool isDirected() {
		return directed;
	}

	public bool isLoop() {
		return (from == to);
	}

	public void setDirected(bool on) {
		directed = on; // / XXX
	}

	public void switchDirection() {
		GraphicNode tmp; // XXX
		tmp = from;
		from = to;
		to = tmp;
	}

	// Nested classes

	/// <summary>
/// An edge group contains the set of edges between two given nodes. This allows to quickly know how many 'multi' edges there is between two nodes in a multigraph and to associate invariant indices to edges (the {@link GraphicEdge#multi} attribute) inside the multi-representation.
/// </summary>
	public class EdgeGroup {
		/// <summary>
/// The set of multiple edges.
/// </summary>
		public List<GraphicEdge> edges;

		/// <summary>
/// Create a new edge group, starting with two edges.
/// </summary>
/// <param name="first"> The initial edge.</param>
/// <param name="second"> The second edge.</param>
		public EdgeGroup(GraphicEdge first, GraphicEdge second) {
			edges = new List<GraphicEdge>();
			first.group = this;
			second.group = this;
			edges.Add(first);
			edges.Add(second);
			first.multi = 0;
			second.multi = 1;
		}

		/// <summary>
/// I-th edge of the group.
/// </summary>
/// <param name="i"> The edge index.</param>
/// <returns>The i-th edge.</returns>
		public GraphicEdge getEdge(int i) {
			return edges[i];
		}

		/// <summary>
/// Number of edges in this group.
/// </summary>
/// <returns>The edge count.</returns>
		public int getCount() {
			return edges.Count;
		}

		/// <summary>
/// Add an edge in the group.
/// </summary>
/// <param name="edge"> The edge to add.</param>
		public void increment(GraphicEdge edge) {
			edge.multi = getCount();
			edges.Add(edge);
		}

		/// <summary>
/// Remove an edge from the group.
/// </summary>
/// <param name="edge"> The edge to remove.</param>
		public void decrement(GraphicEdge edge) {
			edges.Remove(edges.IndexOf(edge));

			for (int i = 0; i < edges.Count; i++)
				edges[i].multi = i;
		}
	}

}
}
