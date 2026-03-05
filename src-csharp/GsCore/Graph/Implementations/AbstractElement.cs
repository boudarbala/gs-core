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
/// A base implementation of an element. <p> <p> This class is the Base class for {@link org.graphstream.graph.Node} {@link org.graphstream.graph.Edge} and {@link org.graphstream.graph.Graph}. An element is made of an unique and arbitrary identifier that identifies it, and a set of attributes. </p>
/// </summary>
abstract class AbstractElement : IElement {
	public enum AttributeChangeEvent {
		ADD, CHANGE, REMOVE
	}

	// Attribute

	/// <summary>
/// Tag of this element.
/// </summary>
	protected string id;

	/// <summary>
/// The index of this element.
/// </summary>
	private int index;

	/// <summary>
/// Attributes map. This map is created only when needed. It contains pairs where the key is the attribute name and the value an object.
/// </summary>
	protected Dictionary<string, object> attributes = null;

	/// <summary>
/// Vector used when removing attributes to avoid recursive removing.
/// </summary>
	protected List<string> attributesBeingRemoved = null;

	// Construction

	/// <summary>
/// New element.
/// </summary>
/// <param name="id"> The unique identifier of this element.</param>
	public AbstractElement(string id) {
		System.Diagnostics.Debug.Assert(id != null, "IGraph elements cannot have a null identifier");
		this.id = id;
	}

	// Access

	public string getId() {
		return id;
	}

	public int getIndex() {
		return index;
	}

	/// <summary>
/// Used by subclasses to change the index of an element
/// </summary>
/// <param name="index"> the new index</param>
	protected void setIndex(int index) {
		this.index = index;
	}

	// XXX UGLY. how to create events in the abstract element ?
	// XXX The various methods that add and remove attributes will propagate an
	// event
	// XXX sometimes this is in response to another event and the
	// sourceId/timeId is given
	// XXX sometimes this comes from a direct call to
	// add/change/removeAttribute() methods
	// XXX in which case we need to generate a new event (sourceId/timeId) using
	// the graph
	// XXX id and a new time. These methods allow access to this.
	// protected abstract String myGraphId(); // XXX

	// protected abstract long newEvent(); // XXX

	/// <summary>
/// Called for each change in the attribute set. This method must be implemented by sub-elements in order to send events to the graph listeners.
/// </summary>
/// <param name="attribute"> The attribute name that changed.</param>
/// <param name="event"> The type of event among ADD, CHANGE and REMOVE.</param>
/// <param name="oldValue"> The old value of the attribute, null if the attribute was added.</param>
/// <param name="newValue"> The new value of the attribute, null if the attribute is about to be removed.</param>
	protected abstract void attributeChanged(AttributeChangeEvent evt, string attribute, object oldValue,
			object newValue);

	
	
	public object getAttribute(string key) {
		if (attributes != null) {
			object value = attributes[key];

			if (value != null)
				return value;
		}

		return null;
	}

	/// <summary>
/// and m the number of keys given.
/// </summary>
	
	public object getFirstAttributeOf(params string[] keys) {
		object o = null;

		if (attributes != null) {
			foreach (string key in keys) {
				o = attributes[key];

				if (o != null)
					return o;
			}
		}

		return o;
	}

	
	
	public T getAttribute<T>(string key, Type clazz) {
		if (attributes != null) {
			object o = attributes[key];

			if (o != null && clazz.IsInstanceOfType(o))
				return clazz.cast(o);
		}

		return null;
	}

	/// <summary>
/// and m the number of keys given.
/// </summary>
	
	public T getFirstAttributeOf<T>(Type clazz, params string[] keys) {
		object o = null;

		if (attributes == null)
			return null;

		foreach (string key in keys) {
			o = attributes[key];

			if (o != null && clazz.IsInstanceOfType(o))
				return clazz.cast(o);
		}

		return null;
	}

	
	
	public bool hasAttribute(string key) {
		return attributes != null && attributes.ContainsKey(key);
	}

	
	
	public bool hasAttribute(string key, Type clazz) {
		if (attributes != null) {
			object o = attributes[key];

			if (o != null)
				return (clazz.IsInstanceOfType(o));
		}

		return false;
	}

	
	public IEnumerable<string> attributeKeys() {
		if (attributes == null)
			return Stream.empty();

		return attributes.Keys;
	}

	/// <summary>
/// Override the object method
/// </summary>
	
	public string toString() {
		return id;
	}

	
	public int getAttributeCount() {
		if (attributes != null)
			return attributes.Count;

		return 0;
	}

	// Command

	
	public void clearAttributes() {
		if (attributes != null) {
			foreach (Map.Entry<string, object> entry in attributes)
				attributeChanged(AttributeChangeEvent.REMOVE, entry.Key, entry.Value, null);

			attributes.Clear();
		}
	}

	protected void clearAttributesWithNoEvent() {
		if (attributes != null)
			attributes.Clear();
	}

	
	
	public void setAttribute(string attribute, params object[] values) {
		if (attributes == null)
			attributes = new Dictionary<object, object>(1);

		object oldValue;
		object value;

		if (values == null)
			value = null;
		else if (values.Length == 0)
			value = true;
		else if (values.Length == 1)
			value = values[0];
		else
			value = values;

		AttributeChangeEvent evt = AttributeChangeEvent.ADD;

		if (attributes.ContainsKey(attribute)) // In case the value is null,
			evt = AttributeChangeEvent.CHANGE; // but the attribute exists.

		oldValue = attributes[attribute] = value;
		attributeChanged(evt, attribute, oldValue, value);
	}

	
	
	public void removeAttribute(string attribute) {
		if (attributes != null) {
			//
			// 'attributesBeingRemoved' is created only if this is required.
			//
			if (attributesBeingRemoved == null)
				attributesBeingRemoved = new List<object>();

			//
			// Avoid recursive calls when synchronizing graphs.
			//
			if (attributes.ContainsKey(attribute) && !attributesBeingRemoved.Contains(attribute)) {
				attributesBeingRemoved.Add(attribute);

				attributeChanged(AttributeChangeEvent.REMOVE, attribute, attributes[attribute], null);

				attributesBeingRemoved.Remove(attributesBeingRemoved.Count - 1);
				attributes.Remove(attribute);
			}
		}
	}
}
}
