using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace Org.GraphStream.Stream.Thread
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
/// Filter that allows to pass graph events between two threads without explicit synchronization. <p> This filter allows to register it as an output for some source of events in a source thread (hereafter called the input thread) and to register listening outputs in a destination thread (hereafter called the sink thread). </p> <pre> | Source ---> ThreadProxyFilter ----> Sink Thread 1             |              Thread 2 | </pre> <p> In other words, this class allows to listen in a sink thread graph events that are produced in another source thread without any explicit synchronization on the source of events. </p> <p> The only restriction is that the sink thread must regularly call the {@link #pump()} method to dispatch events coming from the source to all sinks registered (see the explanation in {@link org.graphstream.stream.ProxyPipe}). </p> <p> You can register any kind of input as source of event, but if the input is a graph, then you can choose to "replay" all the content of the graph so that at the other end of the filter, all outputs receive the complete content of the graph. This is the default behavior if this filter is constructed with a graph as input. </p>
/// </summary>
public class ThreadProxyPipe : SourceBase, IProxyPipe {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	/// <summary>
/// Proxy id.
/// </summary>
	protected string id;

	/// <summary>
/// The event sender name, usually the graph name.
/// </summary>
	protected string from;

	/// <summary>
/// The message box used to exchange messages between the two threads.
/// </summary>
	protected List<GraphEvents> events;
	protected List<object[]> eventsData;

	protected object lockObj;
	protected object notEmpty; /* Condition */

	/// <summary>
/// Used only to remove the listener. We ensure this is done in the source thread.
/// </summary>
	protected ISource input;

	/// <summary>
/// Signals that this proxy must be removed from the source input.
/// </summary>
	protected bool unregisterWhenPossible = false;

	public ThreadProxyPipe() {
		this.events = new List<GraphEvents>();
		this.eventsData = new List<object[]>();
		this.lockObj = new object();
		this.notEmpty = this.lockObj;
		this.from = "<in>";
		this.input = null;
	}

	/// <summary>
/// {@link #init(Source)} method.
/// </summary>
/// <param name="input"> The source of events we listen at.</param>
	[Obsolete] 
	public ThreadProxyPipe(ISource input) : this(input, null, input is IReplayable) {
	}

	/// <summary>
/// {@link #init(Source)} method.
/// </summary>
/// <param name="input"></param>
/// <param name="replay"></param>
	[Obsolete] 
	public ThreadProxyPipe(ISource input, bool replay) : this(input, null, replay) {
	}

	/// <summary>
/// {@link #init(Source)} method.
/// </summary>
/// <param name="input"></param>
/// <param name="initialListener"></param>
/// <param name="replay"></param>
	[Obsolete] 
	public ThreadProxyPipe(ISource input, ISink initialListener, bool replay) : this() {

		if (initialListener != null)
			addSink(initialListener);

		init(input, replay);
	}

	public void init() {
		init(null, false);
	}

	/// <summary>
/// Init the proxy. If there are previous events, they will be cleared.
/// </summary>
/// <param name="source"> source of the events</param>
	public void init(ISource source) {
		init(source, source is IReplayable);
	}

	/// <summary>
/// Init the proxy. If there are previous events, they will be cleared.
/// </summary>
/// <param name="source"> source of the events</param>
/// <param name="replay"> true if the source should be replayed. You need a {@link org.graphstream.stream.Replayable} source to enable replay, else nothing happens.</param>
	public void init(ISource source, bool replay) {
		System.Threading.Monitor.Enter(lockObj);

		try {
			if (this.input != null)
				this.input.removeSink(this);

			this.input = source;

			this.events.Clear();
			this.eventsData.Clear();
		} finally {
			System.Threading.Monitor.Exit(lockObj);
		}

		if (source != null) {
			if (source is IGraph)
				this.from = ((IGraph) source).getId();

			this.input.addSink(this);

			if (replay && source is IReplayable) {
				IReplayable r = (IReplayable) source;
				Controller rc = r.getReplayController();

				rc.addSink(this);
				rc.replay();
			}
		}
	}

	
	public string toString() {
		string dest = "nil";

		if (attrSinks.Count > 0)
			dest = attrSinks[0].ToString();

		return string.Format("thread-proxy(from {0} to {1})", from, dest);
	}

	/// <summary>
/// Ask the proxy to unregister from the event input source (stop receive events) as soon as possible (when the next event will occur in the graph).
/// </summary>
	public void unregisterFromSource() {
		unregisterWhenPossible = true;
	}

	/// <summary>
/// This method must be called regularly in the output thread to check if the input source sent events. If some event occurred, the listeners will be called.
/// </summary>
	public void pump() {
		GraphEvents e = null;
		object[] data = null;

		do {
			System.Threading.Monitor.Enter(lockObj);

			try {
				e = eventsevents.Count > 0 ? events[0] : default;
				data = eventsDataeventsData.Count > 0 ? eventsData[0] : default;
			} finally {
				System.Threading.Monitor.Exit(lockObj);
			}

			if (e != null)
				processMessage(e, data);
		} while (e != null);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ProxyPipe#blockingPump()
	 */
	public void blockingPump(){
		blockingPump(0);
	}

	public void blockingPump(long timeout){
		GraphEvents e;
		object[] data;

		System.Threading.Monitor.Enter(lockObj);

		try {
			if (timeout > 0)
				while (events.Count == 0)
					Monitor.Wait(lockObj, (int)timeout);
			else
				while (events.Count == 0)
					Monitor.Wait(lockObj);
		} finally {
			System.Threading.Monitor.Exit(lockObj);
		}

		do {
			System.Threading.Monitor.Enter(lockObj);

			try {
				e = eventsevents.Count > 0 ? events[0] : default;
				data = eventsDataeventsData.Count > 0 ? eventsData[0] : default;
			} finally {
				System.Threading.Monitor.Exit(lockObj);
			}

			if (e != null)
				processMessage(e, data);
		} while (e != null);
	}

	public bool hasPostRemaining() {
		bool r = true;
		System.Threading.Monitor.Enter(lockObj);

		try {
			r = events.Count > 0;
		} finally {
			System.Threading.Monitor.Exit(lockObj);
		}

		return r;
	}

	/// <summary>
/// Set of events sent via the message box.
/// </summary>
	enum GraphEvents {
		ADD_NODE, DEL_NODE, ADD_EDGE, DEL_EDGE, STEP, CLEARED, ADD_GRAPH_ATTR, CHG_GRAPH_ATTR, DEL_GRAPH_ATTR, ADD_NODE_ATTR, CHG_NODE_ATTR, DEL_NODE_ATTR, ADD_EDGE_ATTR, CHG_EDGE_ATTR, DEL_EDGE_ATTR
	}

	protected bool maybeUnregister() {
		if (unregisterWhenPossible) {
			input.removeSink(this);
			return true;
		}

		return false;
	}

	protected void post(GraphEvents e, params object[] data) {
		Monitor.Enter(lockObj);

		try {
			events.Add(e);
			eventsData.Add(data);

			Monitor.Pulse(lockObj);
		} finally {
			System.Threading.Monitor.Exit(lockObj);
		}
	}

	public void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value) {
		if (maybeUnregister())
			return;

		post(GraphEvents.ADD_EDGE_ATTR, graphId, timeId, edgeId, attribute, value);
	}

	public void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		if (maybeUnregister())
			return;

		post(GraphEvents.CHG_EDGE_ATTR, graphId, timeId, edgeId, attribute, oldValue, newValue);
	}

	public void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute) {
		if (maybeUnregister())
			return;

		post(GraphEvents.DEL_EDGE_ATTR, graphId, timeId, edgeId, attribute);
	}

	public void graphAttributeAdded(string graphId, long timeId, string attribute, object value) {
		if (maybeUnregister())
			return;

		post(GraphEvents.ADD_GRAPH_ATTR, graphId, timeId, attribute, value);
	}

	public void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue) {
		if (maybeUnregister())
			return;

		post(GraphEvents.CHG_GRAPH_ATTR, graphId, timeId, attribute, oldValue, newValue);
	}

	public void graphAttributeRemoved(string graphId, long timeId, string attribute) {
		if (maybeUnregister())
			return;

		post(GraphEvents.DEL_GRAPH_ATTR, graphId, timeId, attribute);
	}

	public void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value) {
		if (maybeUnregister())
			return;

		post(GraphEvents.ADD_NODE_ATTR, graphId, timeId, nodeId, attribute, value);
	}

	public void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		if (maybeUnregister())
			return;

		post(GraphEvents.CHG_NODE_ATTR, graphId, timeId, nodeId, attribute, oldValue, newValue);
	}

	public void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute) {
		if (maybeUnregister())
			return;

		post(GraphEvents.DEL_NODE_ATTR, graphId, timeId, nodeId, attribute);
	}

	public void edgeAdded(string graphId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		if (maybeUnregister())
			return;

		post(GraphEvents.ADD_EDGE, graphId, timeId, edgeId, fromNodeId, toNodeId, directed);
	}

	public void edgeRemoved(string graphId, long timeId, string edgeId) {
		if (maybeUnregister())
			return;

		post(GraphEvents.DEL_EDGE, graphId, timeId, edgeId);
	}

	public void graphCleared(string graphId, long timeId) {
		if (maybeUnregister())
			return;

		post(GraphEvents.CLEARED, graphId, timeId);
	}

	public void nodeAdded(string graphId, long timeId, string nodeId) {
		if (maybeUnregister())
			return;

		post(GraphEvents.ADD_NODE, graphId, timeId, nodeId);
	}

	public void nodeRemoved(string graphId, long timeId, string nodeId) {
		if (maybeUnregister())
			return;

		post(GraphEvents.DEL_NODE, graphId, timeId, nodeId);
	}

	public void stepBegins(string graphId, long timeId, double step) {
		if (maybeUnregister())
			return;

		post(GraphEvents.STEP, graphId, timeId, step);
	}

	// MBoxListener

	protected void processMessage(GraphEvents e, object[] data) {
		string graphId, elementId, attribute;
		long timeId;
		object newValue, oldValue;

		switch (e) {
		case GraphEvents.ADD_NODE:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];

			sendNodeAdded(graphId, timeId, elementId);
			break;
		case GraphEvents.DEL_NODE:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];

			sendNodeRemoved(graphId, timeId, elementId);
			break;
		case GraphEvents.ADD_EDGE:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];

			string fromId = (string) data[3];
			string toId = (string) data[4];
			bool directed = (bool) data[5];

			sendEdgeAdded(graphId, timeId, elementId, fromId, toId, directed);
			break;
		case GraphEvents.DEL_EDGE:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];

			sendEdgeRemoved(graphId, timeId, elementId);
			break;
		case GraphEvents.STEP:
			graphId = (string) data[0];
			timeId = (long) data[1];

			double step = (double) data[2];

			sendStepBegins(graphId, timeId, step);
			break;
		case GraphEvents.ADD_GRAPH_ATTR:
			graphId = (string) data[0];
			timeId = (long) data[1];
			attribute = (string) data[2];
			newValue = data[3];

			sendGraphAttributeAdded(graphId, timeId, attribute, newValue);
			break;
		case GraphEvents.CHG_GRAPH_ATTR:
			graphId = (string) data[0];
			timeId = (long) data[1];
			attribute = (string) data[2];
			oldValue = data[3];
			newValue = data[4];

			sendGraphAttributeChanged(graphId, timeId, attribute, oldValue, newValue);
			break;
		case GraphEvents.DEL_GRAPH_ATTR:
			graphId = (string) data[0];
			timeId = (long) data[1];
			attribute = (string) data[2];

			sendGraphAttributeRemoved(graphId, timeId, attribute);
			break;
		case GraphEvents.ADD_EDGE_ATTR:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];
			attribute = (string) data[3];
			newValue = data[4];

			sendEdgeAttributeAdded(graphId, timeId, elementId, attribute, newValue);
			break;
		case GraphEvents.CHG_EDGE_ATTR:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];
			attribute = (string) data[3];
			oldValue = data[4];
			newValue = data[5];

			sendEdgeAttributeChanged(graphId, timeId, elementId, attribute, oldValue, newValue);
			break;
		case GraphEvents.DEL_EDGE_ATTR:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];
			attribute = (string) data[3];

			sendEdgeAttributeRemoved(graphId, timeId, elementId, attribute);
			break;
		case GraphEvents.ADD_NODE_ATTR:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];
			attribute = (string) data[3];
			newValue = data[4];

			sendNodeAttributeAdded(graphId, timeId, elementId, attribute, newValue);
			break;
		case GraphEvents.CHG_NODE_ATTR:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];
			attribute = (string) data[3];
			oldValue = data[4];
			newValue = data[5];

			sendNodeAttributeChanged(graphId, timeId, elementId, attribute, oldValue, newValue);
			break;
		case GraphEvents.DEL_NODE_ATTR:
			graphId = (string) data[0];
			timeId = (long) data[1];
			elementId = (string) data[2];
			attribute = (string) data[3];

			sendNodeAttributeRemoved(graphId, timeId, elementId, attribute);
			break;
		case GraphEvents.CLEARED:
			graphId = (string) data[0];
			timeId = (long) data[1];

			sendGraphCleared(graphId, timeId);
			break;
		default:
			Console.Error.WriteLine(string.Format("Unknown message {0}.", e));
			break;
		}
	}
}
}
