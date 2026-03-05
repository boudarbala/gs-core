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
/// Super class of all graphic node, edge, and sprite elements. <p> Each graphic element references a style, a graphic graph and has a label. </p> <p> The element also defines the basic behaviour to reload the style when needed, defines abstract methods to set and get the position and bounds in spaces of the element, and to do appropriate actions when specific predefined attributes change (most of them starting with the prefix "ui."). </p> <p> The graphic element has the ability to store attributes like any other graph element, however the attributes stored by the graphic element are restricted. There is a filter on the attribute adding methods that let pass only : <ul> <li>All attributes starting with "ui.".</li> <li>The "x", "y", "z", "xy" and "xyz" attributes.</li> <li>The "stylesheet" attribute.</li> <li>The "label" attribute.</li> </ul> All other attributes are filtered and not stored. The result is that if the graphic graph is used as an input (a source of graph events) some attributes will not pass through the filter. </p>
/// </summary>
abstract class GraphicElement : AbstractElement {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	/// <summary>
/// Interface for renderers registered in each style group.
/// </summary>
	public interface SwingElementRenderer {
	}

	/// <summary>
/// Graph containing this element.
/// </summary>
	protected GraphicGraph mygraph;

	/// <summary>
/// The label or null if not specified.
/// </summary>
	public string label;

	/// <summary>
/// The node style.
/// </summary>
	public StyleGroup style;

	/// <summary>
/// Associated GUI component.
/// </summary>
	public object component;

	/// <summary>
/// Do not show.
/// </summary>
	public bool hidden = false;

	/// <summary>
/// New element.
/// </summary>
	public GraphicElement(string id, GraphicGraph graph) : base(id) {
		this.mygraph = graph;
	}

	public GraphicGraph myGraph() {
		return mygraph;
	}

	/// <summary>
/// Type of selector for the graphic element (Node, Edge, Sprite ?).
/// </summary>
	public Selector.Type getSelectorType();

	/// <summary>
/// Style group. An style group may reference several elements.
/// </summary>
/// <returns>The style group corresponding to this element.</returns>
	public StyleGroup getStyle() {
		return style;
	}

	/// <summary>
/// Label or null if not set.
/// </summary>
	public string getLabel() {
		return label;
	}

	/// <summary>
/// Abscissa of the element, always in GU (graph units). For edges this is the X of the "from" node.
/// </summary>
	public abstract double getX();

	/// <summary>
/// Ordinate of the element, always in GU (graph units). For edges this is the Y of the "from" node.
/// </summary>
	public abstract double getY();

	/// <summary>
/// Depth of the element, always in GU (graph units). For edges this is the Z of the "from" node.
/// </summary>
	public abstract double getZ();

	/// <summary>
/// The associated GUI component.
/// </summary>
/// <returns>An object.</returns>
	public object getComponent() {
		return component;
	}

	// Commands

	/// <summary>
/// The graphic element was removed from the graphic graph, clean up.
/// </summary>
	protected abstract void removed();

	/// <summary>
/// Try to force the element to move at the give location in graph units (GU). For edges, this may move the two attached nodes.
/// </summary>
/// <param name="x"> The new X.</param>
/// <param name="y"> The new Y.</param>
/// <param name="z"> the new Z.</param>
	public abstract void move(double x, double y, double z);

	/// <summary>
/// Set the GUI component of this element.
/// </summary>
/// <param name="component"> The component.</param>
	public void setComponent(object component) {
		this.component = component;
	}

	/// <summary>
/// Handle the "ui.class", "label", "ui.style", etc. attributes.
/// </summary>
	
	protected void attributeChanged(AttributeChangeEvent evt, string attribute, object oldValue, object newValue) {
		if (evt == AttributeChangeEvent.ADD || evt == AttributeChangeEvent.CHANGE) {
			if (attribute[0] == 'u' && attribute[1] == 'i') {
				if (attribute.Equals("typeof(ui)")) {
					mygraph.styleGroups.checkElementStyleGroup(this);
					// mygraph.styleGroups.removeElement( tis );
					// mygraph.styleGroups.addElement( this );
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.label")) {
					label = StyleConstants.convertLabel(newValue);
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.style")) {
					// Cascade the new style in the style sheet.

					if (newValue is string) {
						try {
							mygraph.styleSheet.parseStyleFromString(new Selector(getSelectorType(), getId(), null),
									(string) newValue);
						} catch (Exception e) {
							Console.Error.WriteLine(string.Format("Error while parsing style for %S '{0}' :",
									getSelectorType(), getId()), e);
						}
						mygraph.graphChanged = true;
					} else {
						Console.Error.WriteLine("Unknown value for style [" + newValue + "].");
					}
				} else if (attribute.Equals("ui.hide")) {
					hidden = true;
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.clicked")) {
					style.pushEventFor(this, "clicked");
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.selected")) {
					style.pushEventFor(this, "selected");
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.color")) {
					style.pushElementAsDynamic(this);
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.size")) {
					style.pushElementAsDynamic(this);
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.icon")) {
					mygraph.graphChanged = true;
				}
				// else if( attribute.equals( "ui.state" ) )
				// {
				// if( newValue == null )
				// state = null;
				// else if( newValue instanceof String )
				// state = (String) newValue;
				// }
			} else if (attribute.Equals("label")) {
				label = StyleConstants.convertLabel(newValue);
				mygraph.graphChanged = true;
			}
		} else // REMOVE
		{
			if (attribute[0] == 'u' && attribute[1] == 'i') {
				if (attribute.Equals("typeof(ui)")) {
					object o = attributes.Remove("typeof(ui)"); // Not yet removed
																// at
																// this point !
					mygraph.styleGroups.checkElementStyleGroup(this);
					attributes["typeof(ui)"] = o;
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.label")) {
					label = "";
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.hide")) {
					hidden = false;
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.clicked")) {
					style.popEventFor(this, "clicked");
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.selected")) {
					style.popEventFor(this, "selected");
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.color")) {
					style.popElementAsDynamic(this);
					mygraph.graphChanged = true;
				} else if (attribute.Equals("ui.size")) {
					style.popElementAsDynamic(this);
					mygraph.graphChanged = true;
				}
			} else if (attribute.Equals("label")) {
				label = "";
				mygraph.graphChanged = true;
			}
		}
	}

	// Overriding of standard attribute changing to filter them.

	protected static System.Text.RegularExpressions.Regex acceptedAttribute;

	static GraphicElement() {
		acceptedAttribute = new System.Text.RegularExpressions.Regex("(ui[.].*)|(layout[.].*)|x|y|z|xy|xyz|label|stylesheet");
	}

	
	public void setAttribute(string attribute, params object[] values) {
		System.Text.RegularExpressions.Match matcher = acceptedAttribute.Match(attribute);

		if (matcher.Success)
			base.setAttribute(attribute, values);
	}
}
}
