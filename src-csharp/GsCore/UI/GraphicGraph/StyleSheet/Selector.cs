using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.GraphicGraph.StyleSheet
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
/// A selector is the part of a CSS rule that defines to which element a style applies in the graph.
/// </summary>
public class Selector {
	/// <summary>
/// Types of elements.
/// </summary>
	enum Type {
		ANY, GRAPH, NODE, EDGE, SPRITE
	}

	/// <summary>
/// The kind of element this matcher applies to.
/// </summary>
	public Type type;

	/// <summary>
/// If the selector specify an identifier.
/// </summary>
	public string id;

	/// <summary>
/// If the selector specify a class.
/// </summary>
	public string clazz;

	/// <summary>
/// If the selector also specify a pseudo class.
/// </summary>
	public string pseudoClass;

	/// <summary>
/// New selector for a given type of element.
/// </summary>
/// <param name="type"> The element type of this selector.</param>
	public Selector(Type type) {
		this.type = type;
	}

	/// <summary>
/// New selector for a given type of element. This constructor allows to specify either an identifier or a class to restrict this selector. If the identifier is given, the class will never be used (as identifiers are finer than classes). If the identifier is null the class will be used. The identifier allow to select only one element by its name. The class allows to select several elements.
/// </summary>
/// <param name="type"> The element type of this selector.</param>
/// <param name="identifier"> The element name.</param>
/// <param name="clazz"> The element class.</param>
	public Selector(Type type, string identifier, string clazz) {
		this.type = type;
		setId(identifier);
		setClass(clazz);
	}

	/// <summary>
/// Utility constructor that assign the correct type to the selector from a string. The type must be "node", "edge", "graph", or "sprite".
/// </summary>
/// <param name="type"> Either "node", "edge", "graph" or "sprite".</param>
	public Selector(string type) {
		if (type.Equals("node"))
			this.type = Type.NODE;
		else if (type.Equals("edge"))
			this.type = Type.EDGE;
		else if (type.Equals("graph"))
			this.type = Type.GRAPH;
		else if (type.Equals("sprite"))
			this.type = Type.SPRITE;
		else
			throw new Exception("invalid matcher type '" + type + "'");
	}

	/// <summary>
/// New selector, copy of another.
/// </summary>
/// <param name="other"> The other selector.</param>
	public Selector(Selector other) {
		this.type = other.type;
		setId(other.id);
		setClass(other.clazz);
	}

	/// <summary>
/// Specify the identifier of the unique element this selector applies to.
/// </summary>
/// <param name="id"> A string that identifies an element of the graph.</param>
	public void setId(string id) {
		this.id = id;
	}

	/// <summary>
/// Specify the class of the elements this selector applies to.
/// </summary>
/// <param name="clazz"> A string that matches all elements of a given class.</param>
	public void setClass(string clazz) {
		this.clazz = clazz;
	}

	/// <summary>
/// Specify the pseudo-class of the elements this selector applies to.
/// </summary>
/// <param name="pseudoClass"> A string that matches all elements of a given pseudo-class.</param>
	public void setPseudoClass(string pseudoClass) {
		this.pseudoClass = pseudoClass;
	}

	/// <summary>
/// The kind of elements this selector applies to.
/// </summary>
/// <returns>An element type.</returns>
	public Type getType() {
		return type;
	}

	/// <summary>
/// The identifier of the element this selector uniquely applies to. This can be null if this selector is general.
/// </summary>
/// <returns>The identifier or null if the selector is general.</returns>
	public string getId() {
		return id;
	}

	/// <summary>
/// The class of elements this selector applies to. This can be null if this selector is general.
/// </summary>
/// <returns>A class name or null if the selector is general.</returns>
	public string getClazz() {
		return clazz;
	}

	/// <summary>
/// The pseudo-class of elements this selector applies to. This can be null.
/// </summary>
/// <returns>A pseudo-class name or null.</returns>
	public string getPseudoClass() {
		return pseudoClass;
	}

	
	public string toString() {
		return string.Format("{0}{1}{2}{3}", type.ToString(), id != null ? string.Format("#{0}", id) : "",
				clazz != null ? string.Format(".{0}", clazz) : "",
				pseudoClass != null ? string.Format(" {0}", pseudoClass) : "");
	}
}
}
