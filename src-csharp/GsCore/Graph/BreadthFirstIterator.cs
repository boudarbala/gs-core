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


public class BreadthFirstIterator : IEnumerator<INode> {
	protected bool directed;
	protected IGraph graph;
	protected INode[] queue;
	protected int[] depth;
	protected int qHead, qTail;

	public BreadthFirstIterator(INode startNode, bool directed) {
		this.directed = directed;
		graph = startNode.getGraph();
		int n = graph.getNodeCount();
		queue = new INode[n];
		depth = new int[n];

		int s = startNode.getIndex();
		for (int i = 0; i < n; i++)
			depth[i] = i == s ? 0 : -1;
		queue[0] = startNode;
		qHead = 0;
		qTail = 1;
	}

	public BreadthFirstIterator(INode startNode) : this(startNode, true) {
	}

	public bool hasNext() {
		return qHead < qTail;
	}

	public INode next() {
		if (qHead >= qTail)
			throw new InvalidOperationException();
		INode current = queue[qHead++];
		int level = depth[current.getIndex()] + 1;
		IEnumerable<IEdge> edges = directed ? current.leavingEdges() : current.edges();

		edges.ToList().ForEach(e => {
			INode node = e.getOpposite(current);
			int j = node.getIndex();

			if (depth[j] == -1) {
				queue[qTail++] = node;
				depth[j] = level;
			}
		});

		return current;
	}

	public void remove() {
		throw new NotSupportedException("This iterator does not support remove");
	}

	public int getDepthOf(INode node) {
		return depth[node.getIndex()];
	}

	public int getDepthMax() {
		return depth[queue[qTail - 1].getIndex()];
	}

	public bool tabu(INode node) {
		return depth[node.getIndex()] != -1;
	}

	public bool isDirected() {
		return directed;
	}
}

}
