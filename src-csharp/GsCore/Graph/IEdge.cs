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
/// A general purpose class that provides methods for the management of edges in a graph. <p> <h3>Important</h3> Implementing classes may indicate the complexity of their implementation of the methods with the <code>complexity</code> tag.
/// </summary>
public interface IEdge : IElement {
	/// <summary>
/// Is the edge directed ?.
/// </summary>
/// <returns>True if the edge is directed.</returns>
	bool isDirected();

	/// <summary>
/// Does the source and target of this edge identify the same node ?.
/// </summary>
/// <returns>True if this edge is a loop.</returns>
	bool isLoop();

	/// <summary>
/// First node of the edge. <p> This is equivalent to the {@link #getSourceNode()} method, but may be clearer in the source code if the graph you are using is not directed. </p>
/// </summary>
/// <returns>The first node of the edge.</returns>
	INode getNode0();

	/// <summary>
/// Second node of the edge. <p> This is equivalent to the {@link #getTargetNode()} method, but may be clearer in the source code if the graph you are using is not directed. </p>
/// </summary>
/// <returns>The second node of the edge.</returns>
	INode getNode1();

	/// <summary>
/// Start node. <p> When the edge is directed this is the source node, in this case you can get the opposite node using {@link #getTargetNode()}. This is equivalent to the {@link #getNode0()} method but may be clearer in the source code if the graph you are using is directed. </p>
/// </summary>
/// <returns>The origin node of the edge.</returns>
	INode getSourceNode();

	/// <summary>
/// End node. <p> When the edge is directed this is the target node, in this case you can get the opposite node using {@link #getSourceNode()}. This is equivalent to the {@link #getNode1()} method but may be clearer in the source code if the graph you are using is directed. </p>
/// </summary>
/// <returns>The destination node of the edge.</returns>
	INode getTargetNode();

	/// <summary>
/// When knowing one node and one edge of this node, this method return the node at the other end of the edge. <p> Return null if the given node is not at any end of the edge. </p>
/// </summary>
/// <param name="node"> The node we search the opposite of.</param>
/// <returns>the opposite node of the given node.</returns>
	INode getOpposite(INode node);
}
}
