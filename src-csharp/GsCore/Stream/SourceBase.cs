using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
/// Base implementation of an input that provide basic sink handling. <p> This implementation can register a set of graph sinks (or separate sets of attributes or elements sinks) and provides protected methods to easily broadcast events to all the sinks (beginning with "send"). </p> <p> Each time you want to produce an event toward all registered sinks, you call one of the "send*" methods with correct parameters. The parameters of the "send*" methods maps to the usual GraphStream events. </p> <p> This class is "reentrant". This means that if a send*() method is called during the execution of another or the same send*() method, the event is deferred until the first send*() method is finished. This avoid recursive loops if a sink modifies the input during event handling. </p>
/// </summary>
abstract class SourceBase : ISource {
	// Attribute

	public enum ElementType {
		NODE, EDGE, GRAPH
	}

	/// <summary>
/// Set of graph attributes sinks.
/// </summary>
	protected List<IAttributeSink> attrSinks = new List<IAttributeSink>();

	/// <summary>
/// Set of graph elements sinks.
/// </summary>
	protected List<IElementSink> eltsSinks = new List<IElementSink>();

	/// <summary>
/// A queue that allow the management of events in the right order.
/// </summary>
	protected List<GraphEvent> eventQueue = new List<GraphEvent>();

	/// <summary>
/// A boolean that indicates whether or not an Sink event is being sent during another one.
/// </summary>
	protected bool eventProcessing = false;

	/// <summary>
/// Id of this source.
/// </summary>
	protected string sourceId;

	/// <summary>
/// Time of this source.
/// </summary>
	protected SourceTime sourceTime;

	// Construction

	protected SourceBase() {
		this(string.Format("sourceOnThread#{0}_{1}", System.Threading.Thread.CurrentThread.getId(),
				DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ((int) (new Random().NextDouble() * 1000))));
	}

	protected SourceBase(string sourceId) {
		this.sourceId = sourceId;
		this.sourceTime = new SourceTime(sourceId);
	}

	// Access

	public IEnumerable<IAttributeSink> attributeSinks() {
		return attrSinks;
	}

	public IEnumerable<IElementSink> elementSinks() {
		return eltsSinks;
	}

	// Command

	public void addSink(ISink sink) {
		addAttributeSink(sink);
		addElementSink(sink);
	}

	public void addAttributeSink(IAttributeSink sink) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			attrSinks.Add(sink);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new AddToListEvent<IAttributeSink>(attrSinks, sink));
		}
	}

	public void addElementSink(IElementSink sink) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			eltsSinks.Add(sink);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new AddToListEvent<IElementSink>(eltsSinks, sink));
		}
	}

	public void clearSinks() {
		clearElementSinks();
		clearAttributeSinks();
	}

	public void clearElementSinks() {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			eltsSinks.Clear();

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new ClearListEvent<IElementSink>(eltsSinks));
		}
	}

	public void clearAttributeSinks() {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			attrSinks.Clear();

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new ClearListEvent<IAttributeSink>(attrSinks));
		}
	}

	public void removeSink(ISink sink) {
		removeAttributeSink(sink);
		removeElementSink(sink);
	}

	public void removeAttributeSink(IAttributeSink sink) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			attrSinks.Remove(sink);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new RemoveFromListEvent<IAttributeSink>(attrSinks, sink));
		}
	}

	public void removeElementSink(IElementSink sink) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			eltsSinks.Remove(sink);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new RemoveFromListEvent<IElementSink>(eltsSinks, sink));
		}
	}

	/// <summary>
/// Send a "graph cleared" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
	public void sendGraphCleared(string sourceId) {
		sendGraphCleared(sourceId, sourceTime.newEvent());
	}

	/// <summary>
/// Send a "graph cleared" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
	public void sendGraphCleared(string sourceId, long timeId) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].graphCleared(sourceId, timeId);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new BeforeGraphClearEvent(sourceId, timeId));
		}
	}

	/// <summary>
/// Send a "step begins" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The graph identifier.</param>
/// <param name="step"> The step time stamp.</param>
	public void sendStepBegins(string sourceId, double step) {
		sendStepBegins(sourceId, sourceTime.newEvent(), step);
	}

	/// <summary>
/// Send a "step begins" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The graph identifier.</param>
/// <param name="timeId"></param>
/// <param name="step"> The step time stamp.</param>
	public void sendStepBegins(string sourceId, long timeId, double step) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].stepBegins(sourceId, timeId, step);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new StepBeginsEvent(sourceId, timeId, step));
		}
	}

	/// <summary>
/// Send a "node added" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="nodeId"> The node identifier.</param>
	public void sendNodeAdded(string sourceId, string nodeId) {
		sendNodeAdded(sourceId, sourceTime.newEvent(), nodeId);
	}

	/// <summary>
/// Send a "node added" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="nodeId"> The node identifier.</param>
	public void sendNodeAdded(string sourceId, long timeId, string nodeId) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].nodeAdded(sourceId, timeId, nodeId);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new AfterNodeAddEvent(sourceId, timeId, nodeId));
		}
	}

	/// <summary>
/// Send a "node removed" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The graph identifier.</param>
/// <param name="nodeId"> The node identifier.</param>
	public void sendNodeRemoved(string sourceId, string nodeId) {
		sendNodeRemoved(sourceId, sourceTime.newEvent(), nodeId);
	}

	/// <summary>
/// Send a "node removed" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The graph identifier.</param>
/// <param name="timeId"></param>
/// <param name="nodeId"> The node identifier.</param>
	public void sendNodeRemoved(string sourceId, long timeId, string nodeId) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].nodeRemoved(sourceId, timeId, nodeId);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new BeforeNodeRemoveEvent(sourceId, timeId, nodeId));
		}
	}

	/// <summary>
/// Send an "edge added" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="edgeId"> The edge identifier.</param>
/// <param name="fromNodeId"> The edge start node.</param>
/// <param name="toNodeId"> The edge end node.</param>
/// <param name="directed"> Is the edge directed?.</param>
	public void sendEdgeAdded(string sourceId, string edgeId, string fromNodeId, string toNodeId, bool directed) {
		sendEdgeAdded(sourceId, sourceTime.newEvent(), edgeId, fromNodeId, toNodeId, directed);
	}

	/// <summary>
/// Send an "edge added" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="edgeId"> The edge identifier.</param>
/// <param name="fromNodeId"> The edge start node.</param>
/// <param name="toNodeId"> The edge end node.</param>
/// <param name="directed"> Is the edge directed?.</param>
	public void sendEdgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].edgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new AfterEdgeAddEvent(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed));
		}
	}

	/// <summary>
/// Send a "edge removed" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="edgeId"> The edge identifier.</param>
	public void sendEdgeRemoved(string sourceId, string edgeId) {
		sendEdgeRemoved(sourceId, sourceTime.newEvent(), edgeId);
	}

	/// <summary>
/// Send a "edge removed" event to all element sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="edgeId"> The edge identifier.</param>
	public void sendEdgeRemoved(string sourceId, long timeId, string edgeId) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].edgeRemoved(sourceId, timeId, edgeId);

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(new BeforeEdgeRemoveEvent(sourceId, timeId, edgeId));
		}
	}

	/// <summary>
/// Send a "edge attribute added" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="edgeId"> The edge identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="value"> The attribute value.</param>
	public void sendEdgeAttributeAdded(string sourceId, string edgeId, string attribute, object value) {
		sendAttributeChangedEvent(sourceId, edgeId, ElementType.EDGE, attribute, AttributeChangeEvent.ADD, null, value);
	}

	/// <summary>
/// Send a "edge attribute added" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="edgeId"> The edge identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="value"> The attribute value.</param>
	public void sendEdgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		sendAttributeChangedEvent(sourceId, timeId, edgeId, ElementType.EDGE, attribute, AttributeChangeEvent.ADD, null,
				value);
	}

	/// <summary>
/// Send a "edge attribute changed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="edgeId"> The edge identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="oldValue"> The old attribute value.</param>
/// <param name="newValue"> The new attribute value.</param>
	public void sendEdgeAttributeChanged(string sourceId, string edgeId, string attribute, object oldValue,
			object newValue) {
		sendAttributeChangedEvent(sourceId, edgeId, ElementType.EDGE, attribute, AttributeChangeEvent.CHANGE, oldValue,
				newValue);
	}

	/// <summary>
/// Send a "edge attribute changed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="edgeId"> The edge identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="oldValue"> The old attribute value.</param>
/// <param name="newValue"> The new attribute value.</param>
	public void sendEdgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		sendAttributeChangedEvent(sourceId, timeId, edgeId, ElementType.EDGE, attribute, AttributeChangeEvent.CHANGE,
				oldValue, newValue);
	}

	/// <summary>
/// Send a "edge attribute removed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="edgeId"> The edge identifier.</param>
/// <param name="attribute"> The attribute name.</param>
	public void sendEdgeAttributeRemoved(string sourceId, string edgeId, string attribute) {
		sendAttributeChangedEvent(sourceId, edgeId, ElementType.EDGE, attribute, AttributeChangeEvent.REMOVE, null,
				null);
	}

	/// <summary>
/// Send a "edge attribute removed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="edgeId"> The edge identifier.</param>
/// <param name="attribute"> The attribute name.</param>
	public void sendEdgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		sendAttributeChangedEvent(sourceId, timeId, edgeId, ElementType.EDGE, attribute, AttributeChangeEvent.REMOVE,
				null, null);
	}

	/// <summary>
/// Send a "graph attribute added" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="value"> The attribute value.</param>
	public void sendGraphAttributeAdded(string sourceId, string attribute, object value) {
		sendAttributeChangedEvent(sourceId, null, ElementType.GRAPH, attribute, AttributeChangeEvent.ADD, null, value);
	}

	/// <summary>
/// Send a "graph attribute added" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="value"> The attribute value.</param>
	public void sendGraphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		sendAttributeChangedEvent(sourceId, timeId, null, ElementType.GRAPH, attribute, AttributeChangeEvent.ADD, null,
				value);
	}

	/// <summary>
/// Send a "graph attribute changed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="oldValue"> The attribute old value.</param>
/// <param name="newValue"> The attribute new value.</param>
	public void sendGraphAttributeChanged(string sourceId, string attribute, object oldValue, object newValue) {
		sendAttributeChangedEvent(sourceId, null, ElementType.GRAPH, attribute, AttributeChangeEvent.CHANGE, oldValue,
				newValue);
	}

	/// <summary>
/// Send a "graph attribute changed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="oldValue"> The attribute old value.</param>
/// <param name="newValue"> The attribute new value.</param>
	public void sendGraphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		sendAttributeChangedEvent(sourceId, timeId, null, ElementType.GRAPH, attribute, AttributeChangeEvent.CHANGE,
				oldValue, newValue);
	}

	/// <summary>
/// Send a "graph attribute removed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="attribute"> The attribute name.</param>
	public void sendGraphAttributeRemoved(string sourceId, string attribute) {
		sendAttributeChangedEvent(sourceId, null, ElementType.GRAPH, attribute, AttributeChangeEvent.REMOVE, null,
				null);
	}

	/// <summary>
/// Send a "graph attribute removed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="attribute"> The attribute name.</param>
	public void sendGraphAttributeRemoved(string sourceId, long timeId, string attribute) {
		sendAttributeChangedEvent(sourceId, timeId, null, ElementType.GRAPH, attribute, AttributeChangeEvent.REMOVE,
				null, null);
	}

	/// <summary>
/// Send a "node attribute added" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="nodeId"> The node identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="value"> The attribute value.</param>
	public void sendNodeAttributeAdded(string sourceId, string nodeId, string attribute, object value) {
		sendAttributeChangedEvent(sourceId, nodeId, ElementType.NODE, attribute, AttributeChangeEvent.ADD, null, value);
	}

	/// <summary>
/// Send a "node attribute added" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="nodeId"> The node identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="value"> The attribute value.</param>
	public void sendNodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		sendAttributeChangedEvent(sourceId, timeId, nodeId, ElementType.NODE, attribute, AttributeChangeEvent.ADD, null,
				value);
	}

	/// <summary>
/// Send a "node attribute changed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="nodeId"> The node identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="oldValue"> The attribute old value.</param>
/// <param name="newValue"> The attribute new value.</param>
	public void sendNodeAttributeChanged(string sourceId, string nodeId, string attribute, object oldValue,
			object newValue) {
		sendAttributeChangedEvent(sourceId, nodeId, ElementType.NODE, attribute, AttributeChangeEvent.CHANGE, oldValue,
				newValue);
	}

	/// <summary>
/// Send a "node attribute changed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="nodeId"> The node identifier.</param>
/// <param name="attribute"> The attribute name.</param>
/// <param name="oldValue"> The attribute old value.</param>
/// <param name="newValue"> The attribute new value.</param>
	public void sendNodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		sendAttributeChangedEvent(sourceId, timeId, nodeId, ElementType.NODE, attribute, AttributeChangeEvent.CHANGE,
				oldValue, newValue);
	}

	/// <summary>
/// Send a "node attribute removed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="nodeId"> The node identifier.</param>
/// <param name="attribute"> The attribute name.</param>
	public void sendNodeAttributeRemoved(string sourceId, string nodeId, string attribute) {
		sendAttributeChangedEvent(sourceId, nodeId, ElementType.NODE, attribute, AttributeChangeEvent.REMOVE, null,
				null);
	}

	/// <summary>
/// Send a "node attribute removed" event to all attribute sinks.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="timeId"></param>
/// <param name="nodeId"> The node identifier.</param>
/// <param name="attribute"> The attribute name.</param>
	public void sendNodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		sendAttributeChangedEvent(sourceId, timeId, nodeId, ElementType.NODE, attribute, AttributeChangeEvent.REMOVE,
				null, null);
	}

	/// <summary>
/// Send a add/change/remove attribute event on an element. This method is a generic way of notifying of an attribute change and is equivalent to individual send*Attribute*() methods.
/// </summary>
/// <param name="sourceId"> The source identifier.</param>
/// <param name="eltId"> The changed element identifier.</param>
/// <param name="eltType"> The changed element type.</param>
/// <param name="attribute"> The changed attribute.</param>
/// <param name="event"> The add/change/remove action.</param>
/// <param name="oldValue"> The old attribute value (null if the attribute is removed or added).</param>
/// <param name="newValue"> The new attribute value (null if removed).</param>
	public void sendAttributeChangedEvent(string sourceId, string eltId, ElementType eltType, string attribute,
			AttributeChangeEvent evt, object oldValue, object newValue) {
		sendAttributeChangedEvent(sourceId, sourceTime.newEvent(), eltId, eltType, attribute, evt, oldValue,
				newValue);
	}

	public void sendAttributeChangedEvent(string sourceId, long timeId, string eltId, ElementType eltType,
			string attribute, AttributeChangeEvent evt, object oldValue, object newValue) {
		if (!eventProcessing) {
			eventProcessing = true;
			manageEvents();

			if (evt == AttributeChangeEvent.ADD) {
				if (eltType == ElementType.NODE) {
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].nodeAttributeAdded(sourceId, timeId, eltId, attribute, newValue);
				} else if (eltType == ElementType.EDGE) {
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].edgeAttributeAdded(sourceId, timeId, eltId, attribute, newValue);
				} else {
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].graphAttributeAdded(sourceId, timeId, attribute, newValue);
				}
			} else if (evt == AttributeChangeEvent.REMOVE) {
				if (eltType == ElementType.NODE) {
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].nodeAttributeRemoved(sourceId, timeId, eltId, attribute);
				} else if (eltType == ElementType.EDGE) {
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].edgeAttributeRemoved(sourceId, timeId, eltId, attribute);
				} else {
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].graphAttributeRemoved(sourceId, timeId, attribute);
				}
			} else {
				if (eltType == ElementType.NODE) {
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].nodeAttributeChanged(sourceId, timeId, eltId, attribute, oldValue, newValue);
				} else if (eltType == ElementType.EDGE) {
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].edgeAttributeChanged(sourceId, timeId, eltId, attribute, oldValue, newValue);
				} else {
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].graphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
				}
			}

			manageEvents();
			eventProcessing = false;
		} else {
			eventQueue.Add(
					new AttributeChangedEvent(sourceId, timeId, eltId, eltType, attribute, evt, oldValue, newValue));
		}
	}

	// Deferred event management

	/// <summary>
/// If in "event processing mode", ensure all pending events are processed.
/// </summary>
	protected void manageEvents() {
		if (eventProcessing) {
			while (!eventQueue.Length == 0)
				eventQueue.Remove().trigger();
		}
	}

	// Events Management

	/// <summary>
/// Interface that provide general purpose classification for evens involved in graph modifications
/// </summary>
	abstract class GraphEvent {
		string sourceId;
		long timeId;

		GraphEvent(string sourceId, long timeId) {
			this.sourceId = sourceId;
			this.timeId = timeId;
		}

		abstract void trigger();
	}

	class AfterEdgeAddEvent : GraphEvent {
		string edgeId;
		string fromNodeId;
		string toNodeId;
		bool directed;

		AfterEdgeAddEvent(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
				bool directed) {
			base(sourceId, timeId);
			this.edgeId = edgeId;
			this.fromNodeId = fromNodeId;
			this.toNodeId = toNodeId;
			this.directed = directed;
		}

		void trigger() {
			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].edgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);
		}
	}

	class BeforeEdgeRemoveEvent : GraphEvent {
		string edgeId;

		BeforeEdgeRemoveEvent(string sourceId, long timeId, string edgeId) {
			base(sourceId, timeId);
			this.edgeId = edgeId;
		}

		void trigger() {
			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].edgeRemoved(sourceId, timeId, edgeId);
		}
	}

	class AfterNodeAddEvent : GraphEvent {
		string nodeId;

		AfterNodeAddEvent(string sourceId, long timeId, string nodeId) {
			base(sourceId, timeId);
			this.nodeId = nodeId;
		}

		void trigger() {
			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].nodeAdded(sourceId, timeId, nodeId);
		}
	}

	class BeforeNodeRemoveEvent : GraphEvent {
		string nodeId;

		BeforeNodeRemoveEvent(string sourceId, long timeId, string nodeId) {
			base(sourceId, timeId);
			this.nodeId = nodeId;
		}

		void trigger() {
			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].nodeRemoved(sourceId, timeId, nodeId);
		}
	}

	class BeforeGraphClearEvent : GraphEvent {
		BeforeGraphClearEvent(string sourceId, long timeId) {
			base(sourceId, timeId);
		}

		void trigger() {
			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].graphCleared(sourceId, timeId);
		}
	}

	class StepBeginsEvent : GraphEvent {
		double step;

		StepBeginsEvent(string sourceId, long timeId, double step) {
			base(sourceId, timeId);
			this.step = step;
		}

		void trigger() {
			for (int i = 0; i < eltsSinks.Count; i++)
				eltsSinks[i].stepBegins(sourceId, timeId, step);
		}
	}

	class AttributeChangedEvent : GraphEvent {
		ElementType eltType;

		string eltId;

		string attribute;

		AttributeChangeEvent evt;

		object oldValue;

		object newValue;

		AttributeChangedEvent(string sourceId, long timeId, string eltId, ElementType eltType, string attribute,
				AttributeChangeEvent evt, object oldValue, object newValue) {
			base(sourceId, timeId);
			this.eltType = eltType;
			this.eltId = eltId;
			this.attribute = attribute;
			this.evt = evt;
			this.oldValue = oldValue;
			this.newValue = newValue;
		}

		void trigger() {
			switch (evt) {
			case ADD:
				switch (eltType) {
				case NODE:
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].nodeAttributeAdded(sourceId, timeId, eltId, attribute, newValue);
					break;
				case EDGE:
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].edgeAttributeAdded(sourceId, timeId, eltId, attribute, newValue);
					break;
				default:
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].graphAttributeAdded(sourceId, timeId, attribute, newValue);
				}
				break;
			case REMOVE:
				switch (eltType) {
				case NODE:
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].nodeAttributeRemoved(sourceId, timeId, eltId, attribute);
					break;
				case EDGE:
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].edgeAttributeRemoved(sourceId, timeId, eltId, attribute);
					break;
				default:
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].graphAttributeRemoved(sourceId, timeId, attribute);
				}
				break;
			default:
				switch (eltType) {
				case NODE:
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].nodeAttributeChanged(sourceId, timeId, eltId, attribute, oldValue, newValue);
					break;
				case EDGE:
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].edgeAttributeChanged(sourceId, timeId, eltId, attribute, oldValue, newValue);
					break;
				default:
					for (int i = 0; i < attrSinks.Count; i++)
						attrSinks[i].graphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
				}
			}
		}
	}

	class AddToListEvent<T> : GraphEvent {
		List<T> l;
		T obj;

		AddToListEvent(List<T> l, T obj) {
			base(null, -1);
			this.l = l;
			this.obj = obj;
		}

		void trigger() {
			l.Add(obj);
		}
	}

	class RemoveFromListEvent<T> : GraphEvent {
		List<T> l;
		T obj;

		RemoveFromListEvent(List<T> l, T obj) {
			base(null, -1);
			this.l = l;
			this.obj = obj;
		}

		void trigger() {
			l.Remove(obj);
		}
	}

	class ClearListEvent<T> : GraphEvent {
		List<T> l;

		ClearListEvent(List<T> l) {
			base(null, -1);
			this.l = l;
		}

		void trigger() {
			l.Clear();
		}
	}
}
}
