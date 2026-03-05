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
/// Graphical node. <p> A graphic node defines a position (x,y,z), a string label, and a style from the style sheet. </p>
/// </summary>
public class GraphicNode : GraphicElement, INode {
	/// <summary>
/// The position of the node. In graph units.
/// </summary>
	public double x, y, z;

	public bool positionned = false;

	/// <summary>
/// New graphic node.
/// </summary>
/// <param name="id"> The node identifier.</param>
/// <param name="attributes"> The node attribute set (can be null).</param>
	public GraphicNode(GraphicGraph graph, string id, Dictionary<string, object> attributes) : base(id, graph) {

		if (attributes != null)
			setAttributes(attributes);
	}

	
	public Selector.Type getSelectorType() {
		return Selector.Type.NODE;
	}

	
	public double getX() {
		return x;
	}

	
	public double getY() {
		return y;
	}

	
	public double getZ() {
		return z;
	}

	protected Point3 getPosition() {
		return new Point3(x, y, z);
	}

	protected void moveFromEvent(double x, double y, double z) {
		this.x = x;
		this.y = y;
		this.z = z;

		if (!positionned) {
			positionned = true;
		}

		mygraph.graphChanged = true;
		mygraph.boundsChanged = true;
	}

	
	public void move(double x, double y, double z) {
		moveFromEvent(x, y, z);

		if (mygraph.feedbackXYZ)
			setAttribute("xyz", x, y, z);
	}

	
	protected void attributeChanged(AttributeChangeEvent evt, string attribute, object oldValue, object newValue) {
		base.attributeChanged(evt, attribute, oldValue, newValue);
		char c = attribute[0];

		if (attribute.Length > 2 && c == 'u' && attribute[1] == 'i' && attribute.StartsWith("ui.sprite.")) {
			mygraph.spriteAttribute(evt, this, attribute, newValue);
		} else if ((evt == AttributeChangeEvent.ADD || evt == AttributeChangeEvent.CHANGE)) {
			if (attribute.Length == 1) {
				switch (c) {
				case 'x':
					moveFromEvent(numberAttribute(newValue), y, z);
					break;
				case 'y':
					moveFromEvent(x, numberAttribute(newValue), z);
					break;
				case 'z':
					moveFromEvent(x, y, numberAttribute(newValue));
					break;
				default:
					break;
				}
			} else if (c == 'x' && attribute.Length > 1 && attribute[1] == 'y'
					&& (attribute.Length == 2 || (attribute.Length == 3 && attribute[2] == 'z'))) {

				double pos[] = nodePosition(this);
				moveFromEvent(pos[0], pos[1], pos[2]);
			}
		}

		mygraph.listeners.sendAttributeChangedEvent(getId(), ElementType.NODE, attribute, evt, oldValue, newValue);
	}

	/// <summary>
/// Try to convert the object to a double.
/// </summary>
/// <param name="value"> The object to convert.</param>
/// <returns>The value.</returns>
	protected double numberAttribute(object value) {
		if (value is IConvertible) {
			return ((IConvertible) value);
		} else if (value is string) {
			try {
				return double.Parse((string) value);
			} catch (FormatException e) {
			}
		} else if (value is string) {
			try {
				return double.Parse(((string) value).ToString());
			} catch (FormatException e) {
			}
		}

		return 0;
	}

	
	protected void removed() {
		// NOP
	}

	// Node interface.

	/// <summary>
/// Not implemented.
/// </summary>
	
	public IEnumerator<INode> getBreadthFirstIterator() {
		throw new Exception("not implemented !");
	}

	/// <summary>
/// Not implemented.
/// </summary>
	
	public IEnumerator<INode> getBreadthFirstIterator(bool directed) {
		throw new Exception("not implemented !");
	}

	/// <summary>
/// Not implemented.
/// </summary>
	
	public IEnumerator<INode> getDepthFirstIterator() {
		throw new Exception("not implemented !");
	}

	/// <summary>
/// Not implemented.
/// </summary>
	
	public IEnumerator<INode> getDepthFirstIterator(bool directed) {
		throw new Exception("not implemented !");
	}

	
	public int getDegree() {
		List<GraphicEdge> edges = mygraph.connectivity[this];

		if (edges != null)
			return edges.Count;

		return 0;
	}

	
	public IEdge getEdge(int i) {
		List<GraphicEdge> edges = mygraph.connectivity[this];

		if (edges != null && i >= 0 && i < edges.Count)
			return edges[i];

		return null;
	}

	
	public IEdge getEdgeBetween(string id) {
		if (hasEdgeToward(id))
			return getEdgeToward(id);
		else
			return getEdgeFrom(id);
	}

	
	public IEdge getEdgeFrom(string id) {
		return null;
	}

	
	public IEnumerable<IEdge> edges() {
		return mygraph.connectivity[this].map(ge => (IEdge) ge);
	}

	
	public IEnumerator<IEdge> iterator() {
		return edges().GetEnumerator();
	}

	
	public IEdge getEdgeToward(string id) {
		List<IEdge> edges = mygraph.connectivity[this];

		foreach (IEdge edge in edges) {
			if (edge.getOpposite(this).getId().Equals(id))
				return edge;
		}

		return null;
	}

	
	public IGraph getGraph() {
		return mygraph;
	}

	public string getGraphName() {
		throw new Exception("impossible with GraphicGraph");
	}

	public string getHost() {
		throw new Exception("impossible with GraphicGraph");
	}

	
	public int getInDegree() {
		return getDegree();
	}

	
	public int getOutDegree() {
		return getDegree();
	}

	
	public bool hasEdgeBetween(string id) {
		return (hasEdgeToward(id) || hasEdgeFrom(id));
	}

	
	public bool hasEdgeFrom(string id) {
		return false;
	}

	
	public bool hasEdgeToward(string id) {
		return false;
	}

	public bool isDistributed() {
		return false;
	}

	public void setGraph(IGraph graph) {
		throw new Exception("impossible with GraphicGraph");
	}

	public void setGraphName(string newHost) {
		throw new Exception("impossible with GraphicGraph");
	}

	public void setHost(string newHost) {
		throw new Exception("impossible with GraphicGraph");
	}

	// XXX stubs for the new methods

	
	public IEdge getEdgeBetween(INode INode) {
		// TODO Auto-generated method stub
		return null;
	}

	
	public IEdge getEdgeBetween(int index) {
		// TODO Auto-generated method stub
		return null;
	}

	
	public IEdge getEdgeFrom(INode INode) {
		// TODO Auto-generated method stub
		return null;
	}

	
	public IEdge getEdgeFrom(int index) {
		// TODO Auto-generated method stub
		return null;
	}

	
	public IEdge getEdgeToward(INode INode) {
		// TODO Auto-generated method stub
		return null;
	}

	
	public IEdge getEdgeToward(int index) {
		// TODO Auto-generated method stub
		return null;
	}

	
	public IEdge getEnteringEdge(int i) {
		// TODO Auto-generated method stub
		return null;
	}

	
	public IEdge getLeavingEdge(int i) {
		// TODO Auto-generated method stub
		return null;
	}
}
}
