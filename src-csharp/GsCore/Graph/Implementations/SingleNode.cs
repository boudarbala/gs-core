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
/// Nodes used with {@link SingleGraph}
/// </summary>

public class SingleNode : AdjacencyListNode {
	class TwoEdges {
		protected AbstractEdge inEdge, outEdge;
	}

	protected Dictionary<AbstractNode, TwoEdges> neighborMap;

	// *** Constructor ***

	protected SingleNode(AbstractGraph graph, string id) : base(graph, id) {
		neighborMap = new Dictionary<AbstractNode, TwoEdges>(4 * INITIAL_EDGE_CAPACITY / 3 + 1);
	}

	// *** Helpers ***

	
	
	protected T locateEdge<T>(INode opposite, char type) where T : IEdge {
		TwoEdges ee = neighborMap[opposite];

		if (ee == null)
			return null;

		if (type == IO_EDGE)
			return (T) (ee.inEdge == null ? ee.outEdge : ee.inEdge);

		return (T) (type == I_EDGE ? ee.inEdge : ee.outEdge);
	}

	
	protected void removeEdge(int i) {
		AbstractNode opposite = (AbstractNode) edges[i].getOpposite(this);
		TwoEdges ee = neighborMap[opposite];
		char type = edgeType(edges[i]);
		if (type != O_EDGE)
			ee.inEdge = null;
		if (type != I_EDGE)
			ee.outEdge = null;
		if (ee.inEdge == null && ee.outEdge == null)
			neighborMap.Remove(opposite);
		base.removeEdge(i);
	}

	// *** Callbacks ***

	
	protected bool addEdgeCallback(AbstractEdge edge) {
		AbstractNode opposite = (AbstractNode) edge.getOpposite(this);
		TwoEdges ee = neighborMap[opposite];
		if (ee == null)
			ee = new TwoEdges();
		char type = edgeType(edge);
		if (type != O_EDGE) {
			if (ee.inEdge != null)
				return false;
			ee.inEdge = edge;
		}
		if (type != I_EDGE) {
			if (ee.outEdge != null)
				return false;
			ee.outEdge = edge;
		}
		neighborMap[opposite] = ee;
		return base.addEdgeCallback(edge);
	}

	
	protected void clearCallback() {
		neighborMap.Clear();
		base.clearCallback();
	}
}

}
