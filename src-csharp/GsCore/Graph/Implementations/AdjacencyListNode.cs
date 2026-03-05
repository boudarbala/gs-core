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
/// Nodes used with {@link AdjacencyListGraph}
/// </summary>
public class AdjacencyListNode : AbstractNode {
	protected static readonly int INITIAL_EDGE_CAPACITY;
	protected static readonly double GROWTH_FACTOR = 1.1;

	static AdjacencyListNode() {
		string p = "org.graphstream.graph.node.initialEdgeCapacity";
		int initialEdgeCapacity = 16;
		try {
			initialEdgeCapacity = int.Parse(((System.Environment.GetEnvironmentVariable(p) ?? "16"));
		} catch (Exception e) {
		}
		INITIAL_EDGE_CAPACITY = initialEdgeCapacity;
	}

	protected static readonly char I_EDGE = 0;
	protected static readonly char IO_EDGE = 1;
	protected static readonly char O_EDGE = 2;

	protected AbstractEdge[] edges;
	protected int ioStart, oStart, degree;

	// *** Constructor ***

	protected AdjacencyListNode(AbstractGraph graph, string id) : base(graph, id) {
		edges = new AbstractEdge[INITIAL_EDGE_CAPACITY];
		ioStart = oStart = degree = 0;
	}

	// *** Helpers ***

	protected char edgeType(AbstractEdge e) {
		if (!e.directed || e.source == e.target)
			return IO_EDGE;
		return e.source == this ? O_EDGE : I_EDGE;
	}

	
	protected T locateEdge<T>(INode opposite, char type) { where T : IEdge
		// where to search ?
		int start = 0;
		int end = degree;
		if (type == I_EDGE)
			end = oStart;
		else if (type == O_EDGE)
			start = ioStart;

		for (int i = start; i < end; i++)
			if (edges[i].getOpposite(this) == opposite)
				return (T) edges[i];
		return null;
	}

	protected void removeEdge(int i) {
		if (i >= oStart) {
			edges[i] = edges[--degree];
			edges[degree] = null;
			return;
		}

		if (i >= ioStart) {
			edges[i] = edges[--oStart];
			edges[oStart] = edges[--degree];
			edges[degree] = null;
			return;
		}

		edges[i] = edges[--ioStart];
		edges[ioStart] = edges[--oStart];
		edges[oStart] = edges[--degree];
		edges[degree] = null;

	}

	// *** Callbacks ***

	
	protected bool addEdgeCallback(AbstractEdge edge) {
		// resize edges if necessary
		if (edges.Length == degree) {
			AbstractEdge[] tmp = new AbstractEdge[(int) (GROWTH_FACTOR * edges.Length) + 1];
			Array.Copy(edges, 0, tmp, 0, edges.Length);
			Array.Clear(edges, 0, edges.Length);
			edges = tmp;
		}

		char type = edgeType(edge);

		if (type == O_EDGE) {
			edges[degree++] = edge;
			return true;
		}

		if (type == IO_EDGE) {
			edges[degree++] = edges[oStart];
			edges[oStart++] = edge;
			return true;
		}

		edges[degree++] = edges[oStart];
		edges[oStart++] = edges[ioStart];
		edges[ioStart++] = edge;
		return true;
	}

	
	protected void removeEdgeCallback(AbstractEdge edge) {
		// locate the edge first
		char type = edgeType(edge);
		int i = 0;
		if (type == IO_EDGE)
			i = ioStart;
		else if (type == O_EDGE)
			i = oStart;
		while (edges[i] != edge)
			i++;

		removeEdge(i);
	}

	
	protected void clearCallback() {
		Array.Clear(edges, 0, degree);
		ioStart = oStart = degree = 0;
	}

	// *** Access methods ***

	
	public int getDegree() {
		return degree;
	}

	
	public int getInDegree() {
		return oStart;
	}

	
	public int getOutDegree() {
		return degree - ioStart;
	}

	
	public IEdge getEdge(int i) {
		if (i < 0 || i >= degree)
			throw new IndexOutOfRangeException("INode \"" + this + "\"" + " has no edge " + i);
		return edges[i];
	}

	
	public IEdge getEnteringEdge(int i) {
		if (i < 0 || i >= getInDegree())
			throw new IndexOutOfRangeException("INode \"" + this + "\"" + " has no entering edge " + i);
		return edges[i];
	}

	
	public IEdge getLeavingEdge(int i) {
		if (i < 0 || i >= getOutDegree())
			throw new IndexOutOfRangeException("INode \"" + this + "\"" + " has no edge " + i);
		return edges[ioStart + i];
	}

	
	public IEdge getEdgeBetween(INode node) {
		return locateEdge(node, IO_EDGE);
	}

	
	public IEdge getEdgeFrom(INode node) {
		return locateEdge(node, I_EDGE);
	}

	
	public IEdge getEdgeToward(INode node) {
		return locateEdge(node, O_EDGE);
	}

	// *** Iterators ***

	
	public IEnumerable<IEdge> edges() {
		return new ArraySegment<object>(edges, 0, degree - 0);
	}

	
	public IEnumerable<IEdge> enteringEdges() {
		return new ArraySegment<object>(edges, 0, oStart - 0);
	}

	
	public IEnumerable<IEdge> leavingEdges() {
		return new ArraySegment<object>(edges, ioStart, degree - ioStart);
	}

	protected class EdgeIterator<T> : IEnumerator<T> where T : IEdge {
		protected int iPrev, iNext, iEnd;

		protected EdgeIterator(char type) {
			iPrev = -1;
			iNext = 0;
			iEnd = degree;
			if (type == I_EDGE)
				iEnd = oStart;
			else if (type == O_EDGE)
				iNext = ioStart;
		}

		public bool hasNext() {
			return iNext < iEnd;
		}

		
		public T next() {
			if (iNext >= iEnd)
				throw new InvalidOperationException();
			iPrev = iNext++;
			return (T) edges[iPrev];
		}

		public void remove() {
			if (iPrev == -1)
				throw new InvalidOperationException();
			AbstractEdge e = edges[iPrev];
			// do not call the callback because we already know the index
			graph.removeEdge(e, true, e.source != this, e.target != this);
			removeEdge(iPrev);
			iNext = iPrev;
			iPrev = -1;
			iEnd--;
		}
	}
}

}
