using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.Util
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


public class GraphDiff {
	enum ElementType {
		NODE, EDGE, GRAPH
	}

	private Bridge bridge;
	private List<Event> events;

	/// <summary>
/// Create a new empty diff.
/// </summary>
	public GraphDiff {
		this.events = new List<Event>();
		this.bridge = null;
	}

	/// <summary>
/// Create a diff between two graphs.
/// </summary>
/// <param name="g1"></param>
/// <param name="g2"></param>
	public GraphDiff(IGraph g1, IGraph g2) : this() {

		if (g2.getNodeCount() == 0 && g2.getEdgeCount() == 0 && g2.getAttributeCount() == 0
				&& (g1.getNodeCount() > 0 || g1.getEdgeCount() > 0)) {
			events.Add(new GraphCleared(g1));
		} else {
			attributeDiff(ElementType.GRAPH, g1, g2);

			for (int idx = 0; idx < g1.getEdgeCount(); idx++) {
				IEdge e1 = g1.getEdge(idx);
				IEdge e2 = g2.getEdge(e1.getId());

				if (e2 == null) {
					attributeDiff(ElementType.EDGE, e1, e2);
					events.Add(new EdgeRemoved(e1.getId(), e1.getSourceNode().getId(), e1.getTargetNode().getId(),
							e1.isDirected()));
				}
			}

			for (int idx = 0; idx < g1.getNodeCount(); idx++) {
				INode n1 = g1.getNode(idx);
				INode n2 = g2.getNode(n1.getId());

				if (n2 == null) {
					attributeDiff(ElementType.NODE, n1, n2);
					events.Add(new NodeRemoved(n1.getId()));
				}
			}

			for (int idx = 0; idx < g2.getNodeCount(); idx++) {
				INode n2 = g2.getNode(idx);
				INode n1 = g1.getNode(n2.getId());

				if (n1 == null)
					events.Add(new NodeAdded(n2.getId()));

				attributeDiff(ElementType.NODE, n1, n2);
			}

			for (int idx = 0; idx < g2.getEdgeCount(); idx++) {
				IEdge e2 = g2.getEdge(idx);
				IEdge e1 = g1.getEdge(e2.getId());

				if (e1 == null)
					events.Add(new EdgeAdded(e2.getId(), e2.getSourceNode().getId(), e2.getTargetNode().getId(),
							e2.isDirected()));

				attributeDiff(ElementType.EDGE, e1, e2);
			}
		}
	}

	/// <summary>
/// Start to record changes. If a record is already started, then it will be ended.
/// </summary>
/// <param name="g"> the graph to start listening for changes.</param>
	public void start(IGraph g) {
		if (bridge != null)
			end();

		bridge = new Bridge(g);
	}

	/// <summary>
/// Stop to record changes. If there is no record, calling this method has no effect.
/// </summary>
	public void end() {
		if (bridge != null) {
			bridge.end();
			bridge = null;
		}
	}

	/// <summary>
/// Clear all recorded changes.
/// </summary>
	public void reset() {
		events.Clear();
	}

	/// <summary>
/// Considering this object is a diff between g1 and g2, calling this method will applied changes on g1 such that g1 will look like g2.
/// </summary>
/// <param name="g1"></param>
	public void apply(ISink g1) {
		string sourceId = string.Format("GraphDiff@{0}", (DateTime.UtcNow.Ticks * 100L));
		apply(sourceId, g1);
	}

	public void apply(string sourceId, ISink g1) {
		for (int i = 0; i < events.Count; i++)
			events[i].apply(sourceId, i, g1);
	}

	/// <summary>
/// Considering this object is a diff between g1 and g2, calling this method will applied changes on g2 such that g2 will look like g1.
/// </summary>
/// <param name="g2"></param>
	public void reverse(ISink g2) {
		string sourceId = string.Format("GraphDiff@{0}", (DateTime.UtcNow.Ticks * 100L));
		reverse(sourceId, g2);
	}

	public void reverse(string sourceId, ISink g2) {
		for (int i = events.Count - 1; i >= 0; i--)
			events[i].reverse(sourceId, events.Count + 1 - i, g2);
	}

	private void attributeDiff(ElementType type, IElement e1, IElement e2) {
		if (e1 == null && e2 == null)
			return;
		else if (e1 == null) {
			e2.attributeKeys()
					.ToList().ForEach(key => events.Add(new AttributeAdded(type, e2.getId(), key, e2.getAttribute(key))));
		} else if (e2 == null) {
			e1.attributeKeys()
					.ToList().ForEach(key => events.Add(new AttributeRemoved(type, e1.getId(), key, e1.getAttribute(key))));
		} else {
			e2.attributeKeys().ToList().ForEach(key => {
				if (e1.hasAttribute(key)) {
					object o1 = e1.getAttribute(key);
					object o2 = e2.getAttribute(key);

					if (!(o1 == null ? o2 == null : o1.Equals(o2)))
						events.Add(new AttributeChanged(type, e1.getId(), key, o2, o1));
				} else
					events.Add(new AttributeAdded(type, e1.getId(), key, e2.getAttribute(key)));
			});

			e1.attributeKeys().ToList().ForEach(key => {
				if (!e2.hasAttribute(key))
					events.Add(new AttributeRemoved(type, e1.getId(), key, e1.getAttribute(key)));
			});
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see java.lang.object#toString()
	 */
	
	public string toString() {
		System.Text.StringBuilder buffer = new System.Text.StringBuilder();

		for (int i = 0; i < events.Count; i++)
			buffer.Append(events[i].ToString()).Append("\n");

		return buffer.ToString();
	}

	abstract class Event {
		/// <summary>
/// Apply this event on a given graph.
/// </summary>
/// <param name="g"> the graph on which the action should be applied</param>
		abstract void apply(string sourceId, long timeId, ISink g);

		/// <summary>
/// Apply the dual event on a given graph.
/// </summary>
/// <param name="g"> the graph on which the dual action should be applied.</param>
		abstract void reverse(string sourceId, long timeId, ISink g);
	}

	protected class NodeAdded : Event {
		string nodeId;

		public NodeAdded(string nodeId) {
			this.nodeId = nodeId;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#apply(org.graphstream.graph. Graph)
		 */
		public void apply(string sourceId, long timeId, ISink g) {
			g.nodeAdded(sourceId, timeId, nodeId);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#reverse(org.graphstream.graph
		 * .Graph)
		 */
		public void reverse(string sourceId, long timeId, ISink g) {
			g.nodeRemoved(sourceId, timeId, nodeId);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.object#toString()
		 */
		
		public string toString() {
			return string.Format("an \"{0}\"", nodeId);
		}
	}

	protected class NodeRemoved : NodeAdded {
		public NodeRemoved(string nodeId) : base(nodeId) {
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.NodeAdded#apply(org.graphstream.graph
		 * .Graph)
		 */
		public void apply(string sourceId, long timeId, ISink g) {
			base.reverse(sourceId, timeId, g);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.NodeAdded#reverse(org.graphstream.
		 * graph.Graph)
		 */
		public void reverse(string sourceId, long timeId, ISink g) {
			base.apply(sourceId, timeId, g);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.NodeAdded#toString()
		 */
		
		public string toString() {
			return string.Format("dn \"{0}\"", nodeId);
		}
	}

	abstract class ElementEvent : Event {
		ElementType type;
		string elementId;

		protected ElementEvent(ElementType type, string elementId) {
			this.type = type;
			this.elementId = elementId;
		}

		protected IElement getElement(IGraph g) {
			IElement e;

			switch (type) {
			case NODE:
				e = g.getNode(elementId);
				break;
			case EDGE:
				e = g.getEdge(elementId);
				break;
			case GRAPH:
				e = g;
				break;
			default:
				e = null;
			}

			if (e == null)
				throw new ElementNotFoundException();

			return e;
		}

		protected string toStringHeader() {
			string header;

			switch (type) {
			case NODE:
				header = "cn";
				break;
			case EDGE:
				header = "ce";
				break;
			case GRAPH:
				header = "cg";
				break;
			default:
				header = "??";
				break;
			}

			return string.Format("{0} \"{0}\"", header, elementId);
		}

		protected string toStringValue(object o) {
			if (o == null)
				return "null";
			else if (o is string)
				return "\"" + o.ToString() + "\"";
			else if (o is IConvertible)
				return o.ToString();
			else
				return o.ToString();
		}
	}

	protected class AttributeAdded : ElementEvent {
		string attrId;
		object value;

		public AttributeAdded(ElementType type, string elementId, string attrId, object value) : base(type, elementId) {

			this.attrId = attrId;
			this.value = value;

			if (value != null && value.GetType().IsArray && Array.getLength(value) > 0) {
				object o = Array.newInstance(Array[value, 0].GetType(), Array.getLength(value));

				for (int i = 0; i < Array.getLength(value); i++)
					Array.set(o, i, Array[value, i]);

				this.value = o;
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#apply(org.graphstream.graph. Graph)
		 */
		public void apply(string sourceId, long timeId, ISink g) {
			switch (type) {
			case NODE:
				g.nodeAttributeAdded(sourceId, timeId, elementId, attrId, value);
				break;
			case EDGE:
				g.edgeAttributeAdded(sourceId, timeId, elementId, attrId, value);
				break;
			case GRAPH:
				g.graphAttributeAdded(sourceId, timeId, attrId, value);
				break;
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#reverse(org.graphstream.graph
		 * .Graph)
		 */
		public void reverse(string sourceId, long timeId, ISink g) {
			switch (type) {
			case NODE:
				g.nodeAttributeRemoved(sourceId, timeId, elementId, attrId);
				break;
			case EDGE:
				g.edgeAttributeRemoved(sourceId, timeId, elementId, attrId);
				break;
			case GRAPH:
				g.graphAttributeRemoved(sourceId, timeId, attrId);
				break;
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.object#toString()
		 */
		
		public string toString() {
			return string.Format("{0} +\"{0}\":%s", toStringHeader(), attrId, toStringValue(value));
		}
	}

	protected class AttributeChanged : ElementEvent {
		string attrId;
		object newValue;
		object oldValue;

		public AttributeChanged(ElementType type, string elementId, string attrId, object newValue, object oldValue) : base(type, elementId) {

			this.attrId = attrId;
			this.newValue = newValue;
			this.oldValue = oldValue;

			if (newValue != null && newValue.GetType().IsArray && Array.getLength(newValue) > 0) {
				object o = Array.newInstance(Array[newValue, 0].GetType(), Array.getLength(newValue));

				for (int i = 0; i < Array.getLength(newValue); i++)
					Array.set(o, i, Array[newValue, i]);

				this.newValue = o;
			}

			if (oldValue != null && oldValue.GetType().IsArray && Array.getLength(oldValue) > 0) {
				object o = Array.newInstance(Array[oldValue, 0].GetType(), Array.getLength(oldValue));

				for (int i = 0; i < Array.getLength(oldValue); i++)
					Array.set(o, i, Array[oldValue, i]);

				this.oldValue = o;
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#apply(org.graphstream.graph. Graph)
		 */
		public void apply(string sourceId, long timeId, ISink g) {
			switch (type) {
			case NODE:
				g.nodeAttributeChanged(sourceId, timeId, elementId, attrId, oldValue, newValue);
				break;
			case EDGE:
				g.edgeAttributeChanged(sourceId, timeId, elementId, attrId, oldValue, newValue);
				break;
			case GRAPH:
				g.graphAttributeChanged(sourceId, timeId, attrId, oldValue, newValue);
				break;
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#reverse(org.graphstream.graph
		 * .Graph)
		 */
		public void reverse(string sourceId, long timeId, ISink g) {
			switch (type) {
			case NODE:
				g.nodeAttributeChanged(sourceId, timeId, elementId, attrId, newValue, oldValue);
				break;
			case EDGE:
				g.edgeAttributeChanged(sourceId, timeId, elementId, attrId, newValue, oldValue);
				break;
			case GRAPH:
				g.graphAttributeChanged(sourceId, timeId, attrId, newValue, oldValue);
				break;
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.object#toString()
		 */
		
		public string toString() {
			return string.Format("{0} \"{0}\":%s", toStringHeader(), attrId, toStringValue(newValue));
		}
	}

	protected class AttributeRemoved : ElementEvent {
		string attrId;
		object oldValue;

		public AttributeRemoved(ElementType type, string elementId, string attrId, object oldValue) : base(type, elementId) {

			this.attrId = attrId;
			this.oldValue = oldValue;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#apply(org.graphstream.graph. Graph)
		 */
		public void apply(string sourceId, long timeId, ISink g) {
			switch (type) {
			case NODE:
				g.nodeAttributeRemoved(sourceId, timeId, elementId, attrId);
				break;
			case EDGE:
				g.edgeAttributeRemoved(sourceId, timeId, elementId, attrId);
				break;
			case GRAPH:
				g.graphAttributeRemoved(sourceId, timeId, attrId);
				break;
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#reverse(org.graphstream.graph
		 * .Graph)
		 */
		public void reverse(string sourceId, long timeId, ISink g) {
			switch (type) {
			case NODE:
				g.nodeAttributeAdded(sourceId, timeId, elementId, attrId, oldValue);
				break;
			case EDGE:
				g.edgeAttributeAdded(sourceId, timeId, elementId, attrId, oldValue);
				break;
			case GRAPH:
				g.graphAttributeAdded(sourceId, timeId, attrId, oldValue);
				break;
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.object#toString()
		 */
		
		public string toString() {
			return string.Format("{0} -\"{0}\":%s", toStringHeader(), attrId, toStringValue(oldValue));
		}
	}

	protected class EdgeAdded : Event {
		string edgeId;
		string source, target;
		bool directed;

		public EdgeAdded(string edgeId, string source, string target, bool directed) {
			this.edgeId = edgeId;
			this.source = source;
			this.target = target;
			this.directed = directed;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#apply(org.graphstream.graph. Graph)
		 */
		public void apply(string sourceId, long timeId, ISink g) {
			g.edgeAdded(sourceId, timeId, edgeId, source, target, directed);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#reverse(org.graphstream.graph
		 * .Graph)
		 */
		public void reverse(string sourceId, long timeId, ISink g) {
			g.edgeRemoved(sourceId, timeId, edgeId);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.object#toString()
		 */
		
		public string toString() {
			return string.Format("ae \"{0}\" \"{0}\" %s \"{0}\"", edgeId, source, directed ? ">" : "--", target);
		}
	}

	protected class EdgeRemoved : EdgeAdded {
		public EdgeRemoved(string edgeId, string source, string target, bool directed) : base(edgeId, source, target, directed) {
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.EdgeAdded#apply(org.graphstream.graph
		 * .Graph)
		 */
		public void apply(string sourceId, long timeId, ISink g) {
			base.reverse(sourceId, timeId, g);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.EdgeAdded#reverse(org.graphstream.
		 * graph.Graph)
		 */
		public void reverse(string sourceId, long timeId, ISink g) {
			base.apply(sourceId, timeId, g);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.EdgeAdded#toString()
		 */
		
		public string toString() {
			return string.Format("de \"{0}\"", edgeId);
		}
	}

	protected class StepBegins : Event {
		double newStep, oldStep;

		public StepBegins(double oldStep, double newStep) {
			this.newStep = newStep;
			this.oldStep = oldStep;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#apply(org.graphstream.graph. Graph)
		 */
		public void apply(string sourceId, long timeId, ISink g) {
			g.stepBegins(sourceId, timeId, newStep);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#reverse(org.graphstream.graph
		 * .Graph)
		 */
		public void reverse(string sourceId, long timeId, ISink g) {
			g.stepBegins(sourceId, timeId, oldStep);
		}

		
		public string toString() {
			return string.Format("st {0}", newStep);
		}
	}

	protected class GraphCleared : Event {
		byte[] data;

		public GraphCleared(IGraph g) {
			this.data = null;

			try {
				FileSinkDGS sink = new FileSinkDGS();
				ByteArrayOutputStream bytes = new ByteArrayOutputStream();
				GZIPOutputStream output = new GZIPOutputStream(bytes);

				sink.writeAll(g, output);
				/* output.Flush(); */
				output.Close();

				this.data = bytes.toByteArray();
			} catch (System.IO.IOException e) {
				Console.Error.WriteLine(e);
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#apply(org.graphstream.graph. Graph)
		 */
		public void apply(string sourceId, long timeId, ISink g) {
			g.graphCleared(sourceId, timeId);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.util.GraphDiff.Event#reverse(org.graphstream.graph
		 * .Graph)
		 */
		public void reverse(string sourceId, long timeId, ISink g) {
			try {
				ByteArrayInputStream bytes = new ByteArrayInputStream(this.data);
				GZIPInputStream input = new GZIPInputStream(bytes);
				FileSourceDGS dgs = new FileSourceDGS();

				dgs.addSink(g);
				dgs.readAll(in);
				dgs.removeSink(g);

				input.Close();
			} catch (System.IO.IOException e) {
				Console.Error.WriteLine(e);
			}
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see java.lang.object#toString()
		 */
		
		public string toString() {
			return "cl";
		}
	}

	private class Bridge : ISink {
		IGraph g;

		Bridge(IGraph g) {

			this.g = g;
			g.addSink(this);
		}

		void end() {
			g.removeSink(this);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang
		 * .String, long, java.lang.String, java.lang.object)
		 */
		public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
			Event e;
			e = new AttributeAdded(ElementType.GRAPH, null, attribute, value);
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang
		 * .String, long, java.lang.String, java.lang.object, java.lang.object)
		 */
		public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
				object newValue) {
			Event e;
			e = new AttributeChanged(ElementType.GRAPH, null, attribute, newValue, g.getAttribute(attribute));
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang
		 * .String, long, java.lang.String)
		 */
		public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
			Event e;
			e = new AttributeRemoved(ElementType.GRAPH, null, attribute, g.getAttribute(attribute));
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang
		 * .String, long, java.lang.String, java.lang.String, java.lang.object)
		 */
		public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
			Event e;
			e = new AttributeAdded(ElementType.NODE, nodeId, attribute, value);
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.AttributeSink#nodeAttributeChanged(java.lang
		 * .String, long, java.lang.String, java.lang.String, java.lang.object,
		 * java.lang.object)
		 */
		public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
				object newValue) {
			Event e;
			e = new AttributeChanged(ElementType.NODE, nodeId, attribute, newValue,
					g.getNode(nodeId).getAttribute(attribute));
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang
		 * .String, long, java.lang.String, java.lang.String)
		 */
		public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
			Event e;
			e = new AttributeRemoved(ElementType.NODE, nodeId, attribute, g.getNode(nodeId).getAttribute(attribute));
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang
		 * .String, long, java.lang.String, java.lang.String, java.lang.object)
		 */
		public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
			Event e;
			e = new AttributeAdded(ElementType.EDGE, edgeId, attribute, value);
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.AttributeSink#edgeAttributeChanged(java.lang
		 * .String, long, java.lang.String, java.lang.String, java.lang.object,
		 * java.lang.object)
		 */
		public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
				object newValue) {
			Event e;
			e = new AttributeChanged(ElementType.EDGE, edgeId, attribute, newValue,
					g.getEdge(edgeId).getAttribute(attribute));
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang
		 * .String, long, java.lang.String, java.lang.String)
		 */
		public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
			Event e;
			e = new AttributeRemoved(ElementType.EDGE, edgeId, attribute, g.getEdge(edgeId).getAttribute(attribute));
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.ElementSink#nodeAdded(java.lang.String, long,
		 * java.lang.String)
		 */
		public void nodeAdded(string sourceId, long timeId, string nodeId) {
			Event e;
			e = new NodeAdded(nodeId);
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.ElementSink#nodeRemoved(java.lang.String, long,
		 * java.lang.String)
		 */
		public void nodeRemoved(string sourceId, long timeId, string nodeId) {
			INode n = g.getNode(nodeId);

			n.attributeKeys().ToList().ForEach(key => nodeAttributeRemoved(sourceId, timeId, nodeId, key));

			Event e;
			e = new NodeRemoved(nodeId);
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.ElementSink#edgeAdded(java.lang.String, long,
		 * java.lang.String, java.lang.String, java.lang.String, boolean)
		 */
		public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
				bool directed) {
			Event e;
			e = new EdgeAdded(edgeId, fromNodeId, toNodeId, directed);
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.ElementSink#edgeRemoved(java.lang.String, long,
		 * java.lang.String)
		 */
		public void edgeRemoved(string sourceId, long timeId, string edgeId) {
			IEdge edge = g.getEdge(edgeId);

			edge.attributeKeys().ToList().ForEach(key => edgeAttributeRemoved(sourceId, timeId, edgeId, key));

			Event e;
			e = new EdgeRemoved(edgeId, edge.getSourceNode().getId(), edge.getTargetNode().getId(), edge.isDirected());
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.ElementSink#graphCleared(java.lang.String, long)
		 */
		public void graphCleared(string sourceId, long timeId) {
			Event e = new GraphCleared(g);
			events.Add(e);
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see org.graphstream.stream.ElementSink#stepBegins(java.lang.String, long,
		 * double)
		 */
		public void stepBegins(string sourceId, long timeId, double step) {
			Event e = new StepBegins(g.getStep(), step);
			events.Add(e);
		}
	}

	public static void main(params string[] args){
		IGraph g1 = new AdjacencyListGraph("g1");
		IGraph g2 = new AdjacencyListGraph("g2");

		INode a1 = g1.addNode("A");
		a1.setAttribute("attr1", "test");
		a1.setAttribute("attr2", 10.0);
		a1.setAttribute("attr3", 12);

		INode a2 = g2.addNode("A");
		a2.setAttribute("attr1", "test1");
		a2.setAttribute("attr2", 10.0);
		g2.addNode("B");
		g2.addNode("C");

		GraphDiff diff = new GraphDiff(g2, g1);
		Console.WriteLine(diff);
	}
}

}
