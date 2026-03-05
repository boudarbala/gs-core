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
/// An implementation of an {@link org.graphstream.graph.Element}. <p> It allows only one attribute and has no internal map structure. <b>It is not used and may be removed.</b> </p>
/// </summary>
abstract class OneAttributeElement : IElement {
	// Constants

	// Attributes

	/// <summary>
/// Tag of this element.
/// </summary>
	protected string id;

	/// <summary>
/// The only one attribute
/// </summary>
	object attribute = null;

	// Constructors

	/// <summary>
/// New element.
/// </summary>
/// <param name="id"> The unique identifier of this element.</param>
	public OneAttributeElement(string id) {
		System.Diagnostics.Debug.Assert(id != null, "IGraph elements cannot have a null identifier");
		this.id = id;
	}

	// Accessors

	public string getId() {
		return id;
	}

	public object getAttribute(string key) {
		return attribute;
	}

	public object getFirstAttributeOf(params string[] keys) {
		return attribute;
	}

	
	public T getAttribute<T>(string key, Type clazz) {
		return (T) attribute;
	}

	
	public T getFirstAttributeOf<T>(Type clazz, params string[] keys) {
		return (T) attribute;
	}

	public string getLabel(string key) {
		if (attribute != null && attribute is string)
			return (string) attribute;
		return null;
	}

	public double getNumber(string key) {
		if (attribute != null && attribute is IConvertible)
			return ((IConvertible) attribute);

		return double.NaN;
	}

	
	public List<IConvertible> getVector(string key) {
		if (attribute != null && attribute is List)
			return ((List<IConvertible>) attribute);

		return null;
	}

	public bool hasAttribute(string key) {

		return true;
	}

	public bool hasAttribute(string key, Type clazz) {
		if (attribute != null)
			return (clazz.IsInstanceOfType(attribute));
		return false;
	}

	public bool hasLabel(string key) {
		if (attribute != null)
			return (attribute is string);

		return false;
	}

	public bool hasNumber(string key) {
		if (attribute != null)
			return (attribute is IConvertible);

		return false;
	}

	public bool hasVector(string key) {
		if (attribute != null && attribute is List<object>)
			return true;

		return false;
	}

	public IEnumerator<string> getAttributeKeyIterator() {
		return null;
	}

	public Dictionary<string, object> getAttributeMap() {
		return null;
	}

	/// <summary>
/// Override the object method
/// </summary>
	
	public string toString() {
		return id;
	}

	// Commands

	public void clearAttributes() {
		attribute = null;
	}

	public void addAttribute(string attribute, object value) {
		this.attribute = value;
	}

	public void changeAttribute(string attribute, object value) {
		addAttribute(attribute, value);
	}

	public void setAttributes(Dictionary<string, object> attributes) {
		if (attributes.Count >= 1)
			addAttribute("", attributes[(attributes.Keys.ToArray()[0])]);
	}

	public void removeAttribute(string attribute) {
		this.attribute = null;
	}

	enum AttributeChangeEvent {
		ADD, CHANGE, REMOVE
	}

	/// <summary>
/// Called for each change in the attribute set. This method must be implemented by sub-elements in order to send events to the graph listeners.
/// </summary>
/// <param name="sourceId"> The source of the change.</param>
/// <param name="timeId"> The source time of the change, for synchronization.</param>
/// <param name="attribute"> The attribute name that changed.</param>
/// <param name="event"> The type of event among ADD, CHANGE and REMOVE.</param>
/// <param name="oldValue"> The old value of the attribute, null if the attribute was added.</param>
/// <param name="newValue"> The new value of the attribute, null if the attribute is about to be removed.</param>
	protected abstract void attributeChanged(string sourceId, long timeId, string attribute, AttributeChangeEvent evt,
			object oldValue, object newValue);
}
}
