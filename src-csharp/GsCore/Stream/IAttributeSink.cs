using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream
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
/// Interface to listen at changes on attributes of a graph. <p> The graph attributes listener is called each time an attribute is added, or removed, and each time its value is changed. </p>
/// </summary>
public interface IAttributeSink {
	/// <summary>
/// A graph attribute was added.
/// </summary>
/// <param name="sourceId"> Identifier of the graph where the attribute changed.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="value"> The attribute new value.</param>
	void graphAttributeAdded(string sourceId, long timeId, string attribute, object value);

	/// <summary>
/// A graph attribute was changed.
/// </summary>
/// <param name="sourceId"> Identifier of the graph where the attribute changed.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="oldValue"> The attribute old value.</param>
/// <param name="newValue"> The attribute new value.</param>
	void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue, object newValue);

	/// <summary>
/// A graph attribute was removed.
/// </summary>
/// <param name="sourceId"> Identifier of the graph where the attribute was removed.</param>
/// <param name="attribute"> The removed attribute name.</param>
	void graphAttributeRemoved(string sourceId, long timeId, string attribute);

	/// <summary>
/// A node attribute was added.
/// </summary>
/// <param name="sourceId"> Identifier of the graph where the change occurred.</param>
/// <param name="nodeId"> Identifier of the node whose attribute changed.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="value"> The attribute new value.</param>
	void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value);

	/// <summary>
/// A node attribute was changed.
/// </summary>
/// <param name="sourceId"> Identifier of the graph where the change occurred.</param>
/// <param name="nodeId"> Identifier of the node whose attribute changed.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="oldValue"> The attribute old value.</param>
/// <param name="newValue"> The attribute new value.</param>
	void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue);

	/// <summary>
/// A node attribute was removed.
/// </summary>
/// <param name="sourceId"> Identifier of the graph where the attribute was removed.</param>
/// <param name="nodeId"> Identifier of the node whose attribute was removed.</param>
/// <param name="attribute"> The removed attribute name.</param>
	void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute);

	/// <summary>
/// A edge attribute was added.
/// </summary>
/// <param name="sourceId"> Identifier of the graph where the change occurred.</param>
/// <param name="edgeId"> Identifier of the edge whose attribute changed.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="value"> The attribute new value.</param>
	void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value);

	/// <summary>
/// A edge attribute was changed.
/// </summary>
/// <param name="sourceId"> Identifier of the graph where the change occurred.</param>
/// <param name="edgeId"> Identifier of the edge whose attribute changed.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="oldValue"> The attribute old value.</param>
/// <param name="newValue"> The attribute new value.</param>
	void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue);

	/// <summary>
/// A edge attribute was removed.
/// </summary>
/// <param name="sourceId"> Identifier of the graph where the attribute was removed.</param>
/// <param name="edgeId"> Identifier of the edge whose attribute was removed.</param>
/// <param name="attribute"> The removed attribute name.</param>
	void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute);
}
}
