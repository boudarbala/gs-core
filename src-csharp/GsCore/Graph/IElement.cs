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
/// An element is a part of a graph (node, edge, the graph itself). <p> <p> An interface that defines common method to manipulate identifiers, attributes and indices of the elements (graph, nodes and edges) of a graph. </p> * <p> Attributes can be any object and are identified by arbitrary strings. Some attributes are stored as numbers or strings and are in this case named number, label or vector. There are utility methods to handle these attributes ({@link #getNumber(String)} {@link #getLabel(String)}) or {@link #getVector(String)}, however they are also accessible through the more general method {@link #getAttribute(String)}. </p> <p> <h3>Important</h3> <p> Implementing classes should indicate the complexity of their implementation for each method. </p>
/// </summary>
public interface IElement {
	/// <summary>
/// Unique identifier of this element.
/// </summary>
/// <returns>The identifier value.</returns>
	string getId();

	/// <summary>
/// The current index of this element
/// </summary>
/// <returns>The index value</returns>
	int getIndex();

	/// <summary>
/// Get the attribute object bound to the given key. The returned value may be null to indicate the attribute does not exists or is not supported.
/// </summary>
/// <param name="key"> Name of the attribute to search.</param>
/// <returns>The object bound to the given key or null if no object match this attribute name.</returns>
	object getAttribute(string key);

	/// <summary>
/// Like {@link #getAttribute(String)}, but returns the first existing attribute in a list of keys, instead of only one key. The key list order matters.
/// </summary>
/// <param name="keys"> Several strings naming attributes.</param>
/// <returns>The first attribute that exists.</returns>
	object getFirstAttributeOf(params string[] keys);

	/// <summary>
/// Get the attribute object bound to the given key if it is an instance of the given class. Some The returned value maybe null to indicate the attribute does not exists or is not an instance of the given class.
/// </summary>
/// <param name="key"> The attribute name to search.</param>
/// <param name="clazz"> The expected attribute class.</param>
/// <returns>The object bound to the given key or null if no object match this attribute.</returns>
	// object getAttribute( String key, Type clazz );
	T getAttribute<T>(string key, Type clazz);

	/// <summary>
/// Like {@link #getAttribute(String, Type)}, but returns the first existing attribute in a list of keys, instead of only one key. The key list order matters.
/// </summary>
/// <param name="clazz"> The class the attribute must be instance of.</param>
/// <param name="keys"> Several string naming attributes.</param>
/// <returns>The first attribute that exists.</returns>
	// object getFirstAttributeOf( Type clazz, params String[] keys );
	T getFirstAttributeOf<T>(Type clazz, params string[] keys);

	/// <summary>
/// Get the label string bound to the given key key. Labels are special attributes whose value is a character sequence. If an attribute with the same name exists but is not a character sequence, null is returned.
/// </summary>
/// <param name="key"> The label to search.</param>
/// <returns>The label string value or null if not found.</returns>
	string getLabel(string key);

	/// <summary>
/// Get the number bound to key. Numbers are special attributes whose value is an instance of Number. If an attribute with the same name exists but is not a Number, NaN is returned.
/// </summary>
/// <param name="key"> The name of the number to search.</param>
/// <returns>The number value or NaN if not found.</returns>
	double getNumber(string key);

	/// <summary>
/// Get the vector of number bound to key. Vectors of numbers are special attributes whose value is a sequence of numbers. If an attribute with the same name exists but is not a vector of number, null is returned. A vector of number is a non-empty {@link java.util.List} of {@link java.lang.Number} objects.
/// </summary>
/// <param name="key"> The name of the number to search.</param>
/// <returns>The vector of numbers or null if not found.</returns>
	
	List<IConvertible> getVector(string key);

	/// <summary>
/// Get the array of objects bound to key. Arrays of objects are special attributes whose value is a sequence of objects. If an attribute with the same name exists but is not an array, null is returned.
/// </summary>
/// <param name="key"> The name of the array to search.</param>
/// <returns>The array of objects or null if not found.</returns>
	object[] getArray(string key);

	/// <summary>
/// Get the map bound to key. Maps are special attributes whose value is a set of pairs (name,object). Instances of object implementing the {@link CompoundAttribute} interface are considered like maps since they can be transformed to a map. If an attribute with the same name exists but is not a map, null is returned. We cannot enforce the type of the key. It is considered a string and you should use "object.toString()" to get it.
/// </summary>
/// <param name="key"> The name of the map to search.</param>
/// <returns>The map or null if not found.</returns>
	Dictionary<object, object> getMap(string key);

	/// <summary>
/// Does this element store a value for the given attribute key? Note that returning true here does not mean that calling getAttribute with the same key will not return null since attribute values can be null. This method just checks if the key is present, with no test on the value.
/// </summary>
/// <param name="key"> The name of the attribute to search.</param>
/// <returns>True if a value is present for this attribute.</returns>
	bool hasAttribute(string key);

	/// <summary>
/// Does this element store a value for the given attribute key and this value is an instance of the given class?
/// </summary>
/// <param name="key"> The name of the attribute to search.</param>
/// <param name="clazz"> The expected class of the attribute value.</param>
/// <returns>True if a value is present for this attribute.</returns>
	bool hasAttribute(string key, Type clazz);

	/// <summary>
/// Does this element store a label value for the given key? A label is an attribute whose value is a char sequence.
/// </summary>
/// <param name="key"> The name of the label.</param>
/// <returns>True if a value is present for this attribute and : CharSequence.</returns>
	bool hasLabel(string key);

	/// <summary>
/// Does this element store a number for the given key? A number is an attribute whose value is an instance of Number.
/// </summary>
/// <param name="key"> The name of the number.</param>
/// <returns>True if a value is present for this attribute and can contain a double (inherits from Number).</returns>
	bool hasNumber(string key);

	/// <summary>
/// Does this element store a vector value for the given key? A vector is an attribute whose value is a sequence of numbers.
/// </summary>
/// <param name="key"> The name of the vector.</param>
/// <returns>True if a value is present for this attribute and can contain a sequence of numbers.</returns>
	bool hasVector(string key);

	/// <summary>
/// Does this element store an array value for the given key? Only object arrays (instance of object[]) are considered as array here.
/// </summary>
/// <param name="key"> The name of the array.</param>
/// <returns>True if a value is present for this attribute and can contain an array object.</returns>
	bool hasArray(string key);

	/// <summary>
/// Does this element store a map value for the given key? A map is a set of pairs (key,value) ({@link java.util.Map}) or objects that implement the {@link org.graphstream.graph.CompoundAttribute} class.
/// </summary>
/// <param name="key"> The name of the hash.</param>
/// <returns>True if a value is present for this attribute and can contain a hash.</returns>
	bool hasMap(string key);

	/// <summary>
/// Stream over the attribute keys of the element. If no attribute exist, method will return empty stream.
/// </summary>
/// <returns>a String stream corresponding to the keys of the attributes.</returns>
	IEnumerable<string> attributeKeys();

	/// <summary>
/// Remove all registered attributes. This includes numbers, labels and vectors.
/// </summary>
	void clearAttributes();

	/// <summary>
/// Add or replace the value of an attribute. Existing attributes are overwritten silently. All classes inheriting from Number can be considered as numbers. All classes inheriting from CharSequence can be considered as labels. You can pass zero, one or more arguments for the attribute values. If no value is given, a boolean with value "true" is added. If there is more than one value, an array is stored. If there is only one value, the value is stored (but not in an array).
/// </summary>
/// <param name="attribute"> The attribute name.</param>
/// <param name="values"> The attribute value or set of values.</param>
	void setAttribute(string attribute, params object[] values);

	/// <summary>
/// Add or replace each attribute found in attributes. Existing attributes are overwritten silently. All classes inheriting from Number can be considered as numbers. All classes inheriting from CharSequence can be considered as labels.
/// </summary>
/// <param name="attributes"> A set of (key,value) pairs.</param>
	void setAttributes(Dictionary<string, object> attributes);

	/// <summary>
/// Remove an attribute. Non-existent attributes errors are ignored silently.
/// </summary>
/// <param name="attribute"> Name of the attribute to remove.</param>
	void removeAttribute(string attribute);

	/// <summary>
/// Number of attributes stored in this element.
/// </summary>
/// <returns>the number of attributes.</returns>
	int getAttributeCount();
}
}
