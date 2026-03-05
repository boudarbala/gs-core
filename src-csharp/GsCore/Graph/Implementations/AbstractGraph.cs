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
/// <p> This class provides a basic implementation of {@link org.graphstream.graph.Graph} interface, to minimize the effort required to implement this interface. It provides event management implementing all the methods of {@link org.graphstream.stream.Pipe}. It also manages strict checking and auto-creation policies, as well as other services as displaying, reading and writing. </p> <p> <p> Subclasses have to maintain data structures allowing to efficiently access graph elements by their id or index and iterating on them. They also have to maintain coherent indices of the graph elements. When AbstractGraph decides to add or remove elements, it calls one of the "callbacks" {@link #addNodeCallback(AbstractNode)} {@link #addEdgeCallback(AbstractEdge)} {@link #removeNodeCallback(AbstractNode)} {@link #removeEdgeCallback(AbstractEdge)} {@link #clearCallback()}. The role of these callbacks is to update the data structures and to re-index elements if necessary. </p>
/// </summary>
class AbstractGraph : AbstractElement, IGraph, IReplayable {
	// *** Fields ***

	GraphListeners listeners;
	private bool strictChecking;
	private bool autoCreate;
	private INodeFactory<AbstractNode> nodeFactory;
	private IEdgeFactory<AbstractEdge> edgeFactory;

	private double step = 0;

	private long replayId = 0;

	// *** Constructors ***

	/// <summary>
/// The same as {@code AbstractGraph(id, true, false)}
/// </summary>
/// <param name="id"> Identifier of the graph</param>
	public AbstractGraph(string id) : this(id, true, false) {
	}

	/// <summary>
/// Creates a new graph. Subclasses must create their node and edge factories and initialize their data structures in their constructors.
/// </summary>
/// <param name="id"></param>
/// <param name="strictChecking"></param>
/// <param name="autoCreate"></param>
	public AbstractGraph(string id, bool strictChecking, bool autoCreate) : base(id) {

		this.strictChecking = strictChecking;
		this.autoCreate = autoCreate;
		this.listeners = new GraphListeners(this);
	}

	// *** Inherited from abstract element

	
	protected void attributeChanged(AttributeChangeEvent evt, string attribute, object oldValue, object newValue) {
		listeners.sendAttributeChangedEvent(id, SourceBase.ElementType.GRAPH, attribute, evt, oldValue, newValue);
	}

	// *** Inherited from graph ***

	/// <summary>
/// This implementation returns an iterator over nodes.
/// </summary>
	
	public IEnumerator<INode> iterator() {
		return nodes().GetEnumerator();
	}

	// Factories

	
	public INodeFactory<INode> nodeFactory() {
		return nodeFactory;
	}

	
	public IEdgeFactory<IEdge> edgeFactory() {
		return edgeFactory;
	}

	
	
	public void setNodeFactory(INodeFactory<INode> nf) {
		nodeFactory = (INodeFactory<AbstractNode>) nf;
	}

	
	
	public void setEdgeFactory(IEdgeFactory<IEdge> ef) {
		edgeFactory = (IEdgeFactory<AbstractEdge>) ef;
	}

	// strict checking, autocreation, etc

	
	public bool isStrict() {
		return strictChecking;
	}

	
	public void setStrict(bool on) {
		strictChecking = on;
	}

	
	public bool isAutoCreationEnabled() {
		return autoCreate;
	}

	
	public double getStep() {
		return step;
	}

	
	public void setAutoCreate(bool on) {
		autoCreate = on;
	}

	
	public void stepBegins(double time) {
		listeners.sendStepBegins(time);
		this.step = time;
	}

	// display, read, write

	public IViewer display() {
		return display(true);
	}

	public IViewer display(bool autoLayout) {
		try {
			IDisplay display = Display.getDefault();
			return display.display(this, autoLayout);
		} catch (MissingDisplayException e) {
			throw new Exception("Cannot launch viewer.", e);
		}
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Graph#clear()
	 */
	
	public void clear() {
		listeners.sendGraphCleared();

		nodes().ToList().ForEach(n => ((AbstractNode) n).clearCallback());

		clearCallback();
		clearAttributesWithNoEvent();
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Graph#addNode(java.lang.String)
	 */
	
	public INode addNode(string id) {
		AbstractNode node = (AbstractNode) getNode(id);

		if (node != null) {
			if (strictChecking)
				throw new IdAlreadyInUseException("id \"" + id + "\" already in use. Cannot create a node.");
			return node;
		}

		node = nodeFactory.newInstance(id, this);
		addNodeCallback(node);

		listeners.sendNodeAdded(id);

		return node;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Graph#addEdge(java.lang.String,
	 * org.graphstream.graph.Node, org.graphstream.graph.Node, boolean)
	 */
	
	public IEdge addEdge(string id, INode from, INode to, bool directed) {
		return addEdge(id, (AbstractNode) from, from.getId(), (AbstractNode) to, to.getId(), directed);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Graph#removeNode(org.graphstream.graph.Node)
	 */
	
	public INode removeNode(INode node) {
		if (node == null)
			return null;

		removeNode((AbstractNode) node, true);
		return node;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Graph#removeEdge(org.graphstream.graph.Edge)
	 */
	
	public IEdge removeEdge(IEdge edge) {
		if (edge == null)
			return null;

		removeEdge((AbstractEdge) edge, true, true, true);
		return edge;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Graph#removeEdge(org.graphstream.graph.Node,
	 * org.graphstream.graph.Node)
	 */
	
	public IEdge removeEdge(INode node1, INode node2) {
		IEdge edge = node1.getEdgeToward(node2);

		if (edge == null) {
			if (strictChecking)
				throw new ElementNotFoundException("There is no edge from \"%s\" to \"%s\". Cannot remove it.",
						node1.getId(), node2.getId());
			return null;
		}

		return removeEdge(edge);
	}

	// *** Sinks, sources etc. ***

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.graph.Graph#attributeSinks()
	 */
	
	public IEnumerable<IAttributeSink> attributeSinks() {
		return listeners.attributeSinks();
	}

	/*
	 * *(non-Javadoc)
	 *
	 * @see org.graphstream.graph.Graph#elementSinks()
	 */
	
	public IEnumerable<IElementSink> elementSinks() {
		return listeners.elementSinks();
	}

	/*
	 * *(non-Javadoc)
	 *
	 * @see org.graphstream.stream.Source#addAttributeSink(org.graphstream.stream
	 * .AttributeSink)
	 */
	
	public void addAttributeSink(IAttributeSink sink) {
		listeners.addAttributeSink(sink);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.Source#addElementSink(org.graphstream.stream.
	 * ElementSink)
	 */
	
	public void addElementSink(IElementSink sink) {
		listeners.addElementSink(sink);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.Source#addSink(org.graphstream.stream.Sink)
	 */
	
	public void addSink(ISink sink) {
		listeners.addSink(sink);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.Source#clearAttributeSinks()
	 */
	
	public void clearAttributeSinks() {
		listeners.clearAttributeSinks();
	}

	/*
	 * *(non-Javadoc)
	 *
	 * @see org.graphstream.stream.Source#clearElementSinks()
	 */
	
	public void clearElementSinks() {
		listeners.clearElementSinks();
	}

	/*
	 * *(non-Javadoc)
	 *
	 * @see org.graphstream.stream.Source#clearSinks()
	 */
	
	public void clearSinks() {
		listeners.clearSinks();
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.Source#removeAttributeSink(org.graphstream.stream
	 * .AttributeSink)
	 */
	
	public void removeAttributeSink(IAttributeSink sink) {
		listeners.removeAttributeSink(sink);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.Source#removeElementSink(org.graphstream.stream
	 * .ElementSink)
	 */
	
	public void removeElementSink(IElementSink sink) {
		listeners.removeElementSink(sink);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.Source#removeSink(org.graphstream.stream.Sink)
	 */
	
	public void removeSink(ISink sink) {
		listeners.removeSink(sink);
	}

	
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		listeners.edgeAttributeAdded(sourceId, timeId, edgeId, attribute, value);
	}

	
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		listeners.edgeAttributeChanged(sourceId, timeId, edgeId, attribute, oldValue, newValue);
	}

	
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		listeners.edgeAttributeRemoved(sourceId, timeId, edgeId, attribute);
	}

	
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		listeners.graphAttributeAdded(sourceId, timeId, attribute, value);
	}

	
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		listeners.graphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
	}

	
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		listeners.graphAttributeRemoved(sourceId, timeId, attribute);
	}

	
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		listeners.nodeAttributeAdded(sourceId, timeId, nodeId, attribute, value);
	}

	
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		listeners.nodeAttributeChanged(sourceId, timeId, nodeId, attribute, oldValue, newValue);
	}

	
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		listeners.nodeAttributeRemoved(sourceId, timeId, nodeId, attribute);
	}

	
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		listeners.edgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);
	}

	
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		listeners.edgeRemoved(sourceId, timeId, edgeId);
	}

	
	public void graphCleared(string sourceId, long timeId) {
		listeners.graphCleared(sourceId, timeId);
	}

	
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		listeners.nodeAdded(sourceId, timeId, nodeId);
	}

	
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		listeners.nodeRemoved(sourceId, timeId, nodeId);
	}

	
	public void stepBegins(string sourceId, long timeId, double step) {
		listeners.stepBegins(sourceId, timeId, step);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.Replayable#getReplayController()
	 */
	
	public Replayable.Controller getReplayController() {
		return new GraphReplayController();
	}

	// *** callbacks maintaining user's data structure

	/// <summary>
/// This method is automatically called when a new node is created. Subclasses must add the new node to their data structure and to set its index correctly.
/// </summary>
/// <param name="node"> the node to be added</param>
	protected abstract void addNodeCallback(AbstractNode node);

	/// <summary>
/// This method is automatically called when a new edge is created. Subclasses must add the new edge to their data structure and to set its index correctly.
/// </summary>
/// <param name="edge"> the edge to be added</param>
	protected abstract void addEdgeCallback(AbstractEdge edge);

	/// <summary>
/// This method is automatically called when a node is removed. Subclasses must remove the node from their data structures and to re-index other node(s) so that node indices remain coherent.
/// </summary>
/// <param name="node"> the node to be removed</param>
	protected abstract void removeNodeCallback(AbstractNode node);

	/// <summary>
/// This method is automatically called when an edge is removed. Subclasses must remove the edge from their data structures and re-index other edge(s) so that edge indices remain coherent.
/// </summary>
/// <param name="edge"> the edge to be removed</param>
	protected abstract void removeEdgeCallback(AbstractEdge edge);

	/// <summary>
/// This method is automatically called when the graph is cleared. Subclasses must remove all the nodes and all the edges from their data structures.
/// </summary>
	protected abstract void clearCallback();

	// *** _ methods ***

	// Why do we pass both the ids and the references of the endpoints here?
	// When the caller knows the references it's stupid to call getNode(id)
	// here. If the node does not exist the reference will be null.
	// And if autoCreate is on, we need also the id. Sad but true!
	protected IEdge addEdge(string edgeId, AbstractNode src, string srcId, AbstractNode dst, string dstId,
			bool directed) {
		AbstractEdge edge = (AbstractEdge) getEdge(edgeId);

		if (edge != null) {
			if (strictChecking)
				throw new IdAlreadyInUseException("id \"" + edgeId + "\" already in use. Cannot create an edge.");
			if ((edge.getSourceNode() == src && edge.getTargetNode() == dst) || (!directed
					&& edge.getTargetNode() == src && edge.getSourceNode() == dst))
				return edge;
			return null;
		}

		if (src == null || dst == null) {
			if (strictChecking)
				throw new ElementNotFoundException(
						string.Format("Cannot create edge {0}[{1}-{2}{3}]. INode '{4}' does not exist.", edgeId, srcId,
								directed ? ">" : "-", dstId, src == null ? srcId : dstId));
			if (!autoCreate)
				return null;

			if (src == null)
				src = (AbstractNode) addNode(srcId);
			if (dst == null)
				dst = (AbstractNode) addNode(dstId);
		}
		// at this point edgeId is not in use and both src and dst are not null

		if(src.getGraph() != this || dst.getGraph() != this) {
			throw new ElementNotFoundException("At least one of two nodes does not belong to the graph.");
		}
		edge = edgeFactory.newInstance(edgeId, src, dst, directed);
		// see if the endpoints accept the edge
		if (!src.addEdgeCallback(edge)) {
			if (strictChecking)
				throw new EdgeRejectedException("IEdge " + edge + " was rejected by node " + src);
			return null;
		}
		// note that for loop edges the callback is called only once
		if (src != dst && !dst.addEdgeCallback(edge)) {
			// the edge is accepted by src but rejected by dst
			// so we have to remove it from src
			src.removeEdgeCallback(edge);
			if (strictChecking)
				throw new EdgeRejectedException("IEdge " + edge + " was rejected by node " + dst);
			return null;
		}

		// now we can finally add it
		addEdgeCallback(edge);

		listeners.sendEdgeAdded(edgeId, srcId, dstId, directed);

		return edge;
	}

	// helper for removeNode_
	private void removeAllEdges(AbstractNode node) {
		ICollection<IEdge> toRemove = node.edges().ToList();
		toRemove.ToList().ForEach(this::removeEdge);
	}

	// *** Methods for iterators ***

	/// <summary>
/// This method is similar to {@link #removeNode(Node)} but allows to control if {@link #removeNodeCallback(AbstractNode)} is called or not. It is useful for iterators supporting {@link java.util.Iterator#remove()} who want to update the data structures by their owns.
/// </summary>
/// <param name="node"> the node to be removed</param>
/// <param name="graphCallback"> if {@code false} {@code removeNodeCallback(node)} is not called</param>
	protected void removeNode(AbstractNode node, bool graphCallback) {
		if (node == null) {
			throw new NullReferenceException("node reference is null");
		}
		if (node.getGraph() != this){
			throw new ElementNotFoundException( "INode \""+node.getId()+"\" does not belong to this graph");
		}
		

		removeAllEdges(node);
		listeners.sendNodeRemoved(node.getId());

		if (graphCallback)
			removeNodeCallback(node);
	}

	/// <summary>
/// This method is similar to {@link #removeEdge(Edge)} but allows to control if different callbacks are called or not. It is useful for iterators supporting {@link java.util.Iterator#remove()} who want to update the data structures by their owns.
/// </summary>
/// <param name="edge"> the edge to be removed</param>
/// <param name="graphCallback"> if {@code false} {@link #removeEdgeCallback(AbstractEdge)} of the graph is not called</param>
/// <param name="sourceCallback"> if {@code false} {@link AbstractNode#removeEdgeCallback(AbstractEdge)} is not called for the source node of the edge</param>
/// <param name="targetCallback"> if {@code false} {@link AbstractNode#removeEdgeCallback(AbstractEdge)} is not called for the target node of the edge</param>
	protected void removeEdge(AbstractEdge edge, bool graphCallback, bool sourceCallback,
			bool targetCallback) {
		if (edge == null) {
			throw new NullReferenceException("edge reference is null");
		}

		AbstractNode src = (AbstractNode) edge.getSourceNode();
		AbstractNode dst = (AbstractNode) edge.getTargetNode();
		
		if (src.getGraph() != this || dst.getGraph() != this){
			throw new ElementNotFoundException( "IEdge \""+edge.getId()+"\" does not belong to this graph");
		}

		listeners.sendEdgeRemoved(edge.getId());

		if (sourceCallback)
			src.removeEdgeCallback(edge);

		if (src != dst && targetCallback)
			dst.removeEdgeCallback(edge);

		if (graphCallback)
			removeEdgeCallback(edge);
	}

	class GraphReplayController : SourceBase, Replayable.Controller {
		GraphReplayController() {
			base(this.id + "replay");
		}

		/*
		 * (non-Javadoc)
		 *
		 * @see org.graphstream.stream.Replayable.Controller#replay()
		 */
		
		public void replay() {
			string sourceId = string.Format("{0}-replay-{1}", id, replayId++);
			replay(sourceId);
		}

		/*
		 * (non-Javadoc)
		 *
		 * @see org.graphstream.stream.Replayable.Controller#replay(java.lang.String)
		 */
		
		public void replay(string sourceId) {
			attributeKeys().ToList().ForEach(key => sendGraphAttributeAdded(sourceId, key, getAttribute(key)));

			for (int i = 0; i < getNodeCount(); i++) {
				INode node = getNode(i);
				string nodeId = node.getId();

				sendNodeAdded(sourceId, nodeId);

				node.attributeKeys()
						.ToList().ForEach(key => sendNodeAttributeAdded(sourceId, nodeId, key, node.getAttribute(key)));
			}

			for (int i = 0; i < getEdgeCount(); i++) {
				IEdge edge = getEdge(i);
				string edgeId = edge.getId();

				sendEdgeAdded(sourceId, edgeId, edge.getNode0().getId(), edge.getNode1().getId(), edge.isDirected());

				edge.attributeKeys()
						.ToList().ForEach(key => sendEdgeAttributeAdded(sourceId, edgeId, key, edge.getAttribute(key)));
			}
		}
	}
}

}
