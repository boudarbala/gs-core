using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.Layout.SpringBox
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
/// Edge representation. <p> This is mainly used to store data about an edge, all the computation is done in the node particle. </p>
/// </summary>
public class EdgeSpring {
	/// <summary>
/// The edge identifier.
/// </summary>
	public string id;

	/// <summary>
/// Source node.
/// </summary>
	public NodeParticle node0;

	/// <summary>
/// Target node.
/// </summary>
	public NodeParticle node1;

	/// <summary>
/// Edge weight.
/// </summary>
	public double weight = 1f;

	/// <summary>
/// The attraction force on this edge.
/// </summary>
	public Point3 spring = new Point3();

	/// <summary>
/// Make this edge ignored by the layout algorithm ?.
/// </summary>
	public bool ignored = false;

	/// <summary>
/// The edge attraction energy.
/// </summary>
	public double attE;

	/// <summary>
/// New edge between two given nodes.
/// </summary>
/// <param name="id"> The edge identifier.</param>
/// <param name="n0"> The first node.</param>
/// <param name="n1"> The second node.</param>
	public EdgeSpring(string id, NodeParticle n0, NodeParticle n1) {
		this.id = id;
		this.node0 = n0;
		this.node1 = n1;
	}

	/// <summary>
/// Considering the two nodes of the edge, return the one that was not given as argument.
/// </summary>
/// <param name="node"> One of the nodes of the edge.</param>
/// <returns>The other node.</returns>
	public NodeParticle getOpposite(NodeParticle node) {
		return node0 == node ? node1 : node0;
	}
}
}
