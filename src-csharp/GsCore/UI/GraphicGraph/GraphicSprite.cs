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
/// A small gentle sprite.
/// </summary>
public class GraphicSprite : GraphicElement {
	// Attributes

	/// <summary>
/// The node this sprite is attached to.
/// </summary>
	protected GraphicNode node;

	/// <summary>
/// The edge this sprite is attached to.
/// </summary>
	protected GraphicEdge edge;

	/// <summary>
/// Sprite position.
/// </summary>
	public Values position = new Values(StyleConstants.Units.GU, 0, 0, 0);

	// Constructors

	/// <summary>
/// New sprite.
/// </summary>
/// <param name="id"> The sprite unique identifier.</param>
/// <param name="graph"> The graph containing this sprite.</param>
	public GraphicSprite(string id, GraphicGraph graph) : base(id, graph) {

		// Get the position of a random node.

		if (graph.getNodeCount() > 0) {
			GraphicNode node = (GraphicNode) graph.nodes().FirstOrDefault()[];

			position.setValue(0, node.x);
			position.setValue(1, node.y);
			position.setValue(2, node.z);
		}

		string myPrefix = string.Format("ui.sprite.{0}", id);

		if (mygraph.getAttribute(myPrefix) == null)
			mygraph.setAttribute(myPrefix, position);
	}

	// Access

	/// <summary>
/// The node this sprite is attached to or null if not attached to an edge.
/// </summary>
/// <returns>A graphic node.</returns>
	public GraphicNode getNodeAttachment() {
		return node;
	}

	/// <summary>
/// The edge this sprite is attached to or null if not attached to an edge.
/// </summary>
/// <returns>A graphic edge.</returns>
	public GraphicEdge getEdgeAttachment() {
		return edge;
	}

	/// <summary>
/// Return the graphic object this sprite is attached to or null if not attached.
/// </summary>
/// <returns>A graphic object or null if no attachment.</returns>
	public GraphicElement getAttachment() {
		GraphicNode n = getNodeAttachment();

		if (n != null)
			return n;

		return getEdgeAttachment();
	}

	/// <summary>
/// True if the sprite is attached to a node or edge.
/// </summary>
	public bool isAttached() {
		return (edge != null || node != null);
	}

	/// <summary>
/// True if the sprite is attached to a node.
/// </summary>
	public bool isAttachedToNode() {
		return node != null;
	}

	/// <summary>
/// True if the node is attached to an edge.
/// </summary>
	public bool isAttachedToEdge() {
		return edge != null;
	}

	
	public Selector.Type getSelectorType() {
		return Selector.Type.SPRITE;
	}

	
	public double getX() {
		return position[0];
	}

	
	public double getY() {
		return position[1];
	}

	
	public double getZ() {
		return position[2];
	}

	public Style.Units getUnits() {
		return position.getUnits();
	}

	// Commands

	
	public void move(double x, double y, double z) {

		if (isAttachedToNode()) {
			GraphicNode n = getNodeAttachment();
			x -= n.x;
			y -= n.y;
			z -= n.z;
			setPosition(x, y, z, Style.Units.GU);

		} else if (isAttachedToEdge()) {
			GraphicEdge e = getEdgeAttachment();
			double len = e.to.x - e.from.x;
			double diff = x - e.from.x;
			x = diff / len;
			setPosition(x);

		} else {
			setPosition(x, y, z, Style.Units.GU);

		}
	}

	/// <summary>
/// Attach this sprite to the given node.
/// </summary>
/// <param name="node"> A graphic node.</param>
	public void attachToNode(GraphicNode node) {
		this.edge = null;
		this.node = node;

		string prefix = string.Format("ui.sprite.{0}", getId());

		if (this.node.getAttribute(prefix) == null)
			this.node.setAttribute(prefix);

		mygraph.graphChanged = true;
	}

	/// <summary>
/// Attach this sprite to the given edge.
/// </summary>
/// <param name="edge"> A graphic edge.</param>
	public void attachToEdge(GraphicEdge edge) {
		this.node = null;
		this.edge = edge;

		string prefix = string.Format("ui.sprite.{0}", getId());

		if (this.edge.getAttribute(prefix) == null)
			this.edge.setAttribute(prefix);

		mygraph.graphChanged = true;
	}

	/// <summary>
/// Detach this sprite from the edge or node it was attached to.
/// </summary>
	public void detach() {
		string prefix = string.Format("ui.sprite.{0}", getId());

		if (this.node != null)
			this.node.removeAttribute(prefix);
		else if (this.edge != null)
			this.edge.removeAttribute(prefix);

		this.edge = null;
		this.node = null;
		mygraph.graphChanged = true;
	}

	/// <summary>
/// Reposition this sprite.
/// </summary>
/// <param name="value"> The coordinate.</param>
	public void setPosition(double value) {
		setPosition(value, 0, 0, getUnits());
	}

	/// <summary>
/// Reposition this sprite.
/// </summary>
/// <param name="x"> First coordinate.</param>
/// <param name="y"> Second coordinate.</param>
/// <param name="z"> Third coordinate.</param>
/// <param name="units"> The units to use for lengths and radii, null means "unchanged".</param>
	public void setPosition(double x, double y, double z, Style.Units units) {
		/*
		 * if( node != null ) { y = checkAngle( y ); z = checkAngle( z ); } else
		 */if (edge != null) {
			if (x < 0)
				x = 0;
			else if (x > 1)
				x = 1;
		}

		bool changed = false;

		if (getX() != x) {
			changed = true;
			position.setValue(0, x);
		}
		if (getY() != y) {
			changed = true;
			position.setValue(1, y);
		}
		if (getZ() != z) {
			changed = true;
			position.setValue(2, z);
		}
		if (getUnits() != units) {
			changed = true;
			position.setUnits(units);
		}

		if (changed) {
			mygraph.graphChanged = true;
			mygraph.boundsChanged = true;

			string prefix = string.Format("ui.sprite.{0}", getId());

			mygraph.setAttribute(prefix, position);
		}
	}

	public void setPosition(Values values) {
		double x = 0;
		double y = 0;
		double z = 0;

		if (values.getValueCount() > 0)
			x = values[0];
		if (values.getValueCount() > 1)
			y = values[1];
		if (values.getValueCount() > 2)
			z = values[2];

		if (x == 1 && y == 1 && z == 1)
			throw new Exception("WTF !!!");
		setPosition(x, y, z, values.units);
	}

	protected double checkAngle(double angle) {
		if (angle > Math.PI * 2)
			angle = angle % (Math.PI * 2);
		else if (angle < 0)
			angle = (Math.PI * 2) - (angle % (Math.PI * 2));

		return angle;
	}

	
	protected void attributeChanged(AttributeChangeEvent evt, string attribute, object oldValue, object newValue) {
		base.attributeChanged(evt, attribute, oldValue, newValue);

		// if( attribute.equals( "ui.clicked" ) ) // Filter the clicks to avoid
		// loops XXX BAD !!! XXX
		// return;

		string completeAttr = string.Format("ui.sprite.{0}.{1}", getId(), attribute);

		mygraph.listeners.sendAttributeChangedEvent(mygraph.getId(), ElementType.GRAPH, completeAttr, evt, oldValue,
				newValue);
	}

	
	protected void removed() {
	}
}
}
