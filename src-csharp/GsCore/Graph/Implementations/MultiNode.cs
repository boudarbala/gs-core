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
/// Nodes used with {@link MultiGraph}
/// </summary>
public class MultiNode : AdjacencyListNode {
	protected Dictionary<AbstractNode, List<AbstractEdge>> neighborMap;

	// *** Constructor ***

	public MultiNode(AbstractGraph graph, string id) : base(graph, id) {
		neighborMap = new Dictionary<AbstractNode, List<AbstractEdge>>(4 * INITIAL_EDGE_CAPACITY / 3 + 1);
	}

	// *** Helpers ***

	
	
	protected T locateEdge<T>(INode opposite, char type) { where T : IEdge
		List<AbstractEdge> l = neighborMap[opposite];
		if (l == null)
			return null;

		foreach (AbstractEdge e in l) {
			char etype = edgeType(e);
			if ((type != I_EDGE || etype != O_EDGE) && (type != O_EDGE || etype != I_EDGE))
				return (T) e;
		}
		return null;
	}

	
	protected void removeEdge(int i) {
		AbstractNode opposite = (AbstractNode) edges[i].getOpposite(this);
		List<AbstractEdge> l = neighborMap[opposite];
		l.Remove(edges[i]);
		if (l.Length == 0)
			neighborMap.Remove(opposite);
		base.removeEdge(i);
	}

	// *** Callbacks ***

	
	protected bool addEdgeCallback(AbstractEdge edge) {
		AbstractNode opposite = (AbstractNode) edge.getOpposite(this);
		List<AbstractEdge> l = neighborMap[opposite];
		if (l == null) {
			l = new List<AbstractEdge>();
			neighborMap[opposite] = l;
		}
		l.Add(edge);
		return base.addEdgeCallback(edge);
	}

	
	protected void clearCallback() {
		neighborMap.Clear();
		base.clearCallback();
	}

	// *** Others ***

	
	public ICollection<T> getEdgeSetBetween<T>(INode node) { where T : IEdge
		List<AbstractEdge> l = neighborMap[node];
		if (l == null)
			return new List<object>();
		return (ICollection<T>) new List<object>(l);
	}

	public ICollection<T> getEdgeSetBetween<T>(string id) { where T : IEdge
		return getEdgeSetBetween(graph.getNode(id));
	}

	public ICollection<T> getEdgeSetBetween<T>(int index) { where T : IEdge
		return getEdgeSetBetween(graph.getNode(index));
	}
}

}
