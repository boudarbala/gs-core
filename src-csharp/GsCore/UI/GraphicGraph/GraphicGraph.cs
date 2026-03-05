using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.GraphicGraph
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
/// Graph representation used in display classes. <p> <p> Warning: This class is NOT a general graph class, and it should NOT be used as it. This class is particularly dedicated to fast drawing of the graph and is internally arranged to be fast for this task only. It : graph solely to be easily susceptible to be used as a sink and source for graph events. Some of the common methods of the Graph interface are not functional and will throw an exception if used (as documented in their respective JavaDoc). </p> <p> <p> The purpose of the graphic graph is to represent a graph with some often used graphic attributes (like position, label, etc.) stored as fields in the nodes and edges and most of the style stored in styles pertaining to a style sheet that tries to imitate the way CSS works. For example, the GraphicNode class defines a label, a position (x,y,z) and a style that is taken from the style sheet. </p> <p> <p> The style sheet is uploaded on the graph using an attribute correspondingly named "stylesheet" or "ui.stylesheet" (the second one is better). It can be a string that contains the whole style sheet, or an URL of the form : </p> <p> <pre> url(name) </pre> <p> <p> The graphic graph does not completely duplicate a graph, it only store things that are useful for drawing it. Although it : "Graph", some methods are not implemented and will throw a runtime exception. These methods are mostly utility methods like write(), read(), and naturally display(). </p> <p> <p> The graphic graph has the ability to store attributes like any other graph element, however the attributes stored by the graphic graph are restricted. There is a filter on the attribute adding methods that let pass only: <ul> <li>All attributes starting with "ui.".</li> <li>The "x", "y", "z", "xy" and "xyz" attributes.</li> <li>The "stylesheet" attribute (although "ui.stylesheet" is preferred).</li> <li>The "label" attribute.</li> </ul> All other attributes are filtered and not stored. The result is that if the graphic graph is used as an input (a source of graph events) some attributes will not pass through the filter. </p> <p> <p> The implementation of this graph relies on the StyleGroupSet class and this is indeed its way to store its elements (grouped by style and Z level). </p> <p> <p> In addition to this, it provides, as all graphs do, the relational information for edges. </p> <p> TODO : this graph cannot handle modification inside event listener methods !!
/// </summary>
public class GraphicGraph : AbstractElement, IGraph, StyleGroupListener {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	/// <summary>
/// Set of styles.
/// </summary>
	protected StyleSheet styleSheet;

	/// <summary>
/// Associate graphic elements with styles.
/// </summary>
	protected StyleGroupSet styleGroups;

	/// <summary>
/// Connectivity. The way nodes are connected one with another via edges. The map is sorted by node. For each node an array of edges lists the connectivity.
/// </summary>
	protected Dictionary<GraphicNode, List<GraphicEdge>> connectivity;

	/// <summary>
/// The style of this graph. This is a shortcut to avoid searching it in the style sheet.
/// </summary>
	public StyleGroup style;

	/// <summary>
/// Memorize the step events.
/// </summary>
	public double step = 0;

	/// <summary>
/// Set to true each time the graph was modified internally and a redraw is needed.
/// </summary>
	public bool graphChanged;

	/// <summary>
/// Set to true each time a sprite or node moved.
/// </summary>
	protected bool boundsChanged = true;

	/// <summary>
/// Maximum position of a node or sprite in the graphic graph. Computed by {@link #computeBounds()}.
/// </summary>
	protected Point3 hi = new Point3();

	/// <summary>
/// Minimum position of a node or sprite in the graphic graph. Computed by {@link #computeBounds()}.
/// </summary>
	protected Point3 lo = new Point3();

	/// <summary>
/// Set of listeners of this graph.
/// </summary>
	protected GraphListeners listeners;

	/// <summary>
/// Time of other known sources.
/// </summary>
	// protected SinkTime sinkTime = new SinkTime();

	/// <summary>
/// Report back the XYZ events on nodes and sprites? If enabled, each change in the position of nodes and sprites will be sent to potential listeners of the graph. By default this is disabled as long there are no listeners.
/// </summary>
	protected bool feedbackXYZ = true;

	/// <summary>
/// New empty graphic graph. <p> A default style sheet is created, it then can be "cascaded" with other style sheets.
/// </summary>
	public GraphicGraph(string id) : base(id) {

		listeners = new GraphListeners(this);
		styleSheet = new StyleSheet();
		styleGroups = new StyleGroupSet(styleSheet);
		connectivity = new Dictionary<GraphicNode, List<GraphicEdge>>();

		styleGroups.addListener(this);
		styleGroups.addElement(this); // Add style to this graph.

		style = styleGroups.getStyleFor(this);
	}

	// Access

	/// <summary>
/// True if the graph was edited or changed in any way since the last reset of the "changed" flag.
/// </summary>
/// <returns>true if the graph was changed.</returns>
	public bool graphChangedFlag() {
		return graphChanged;
	}

	/// <summary>
/// Reset the "changed" flag.
/// </summary>
	public void resetGraphChangedFlag() {
		graphChanged = false;
	}

	/// <summary>
/// The style sheet. This style sheet is the result of the "cascade" or accumulation of styles added via attributes of the graph.
/// </summary>
/// <returns>A style sheet.</returns>
	public StyleSheet getStyleSheet() {
		return styleSheet;
	}

	/// <summary>
/// The graph style group.
/// </summary>
/// <returns>A style group.</returns>
	public StyleGroup getStyle() {
		return style;
	}

	/// <summary>
/// The complete set of style groups.
/// </summary>
/// <returns>The style groups.</returns>
	public StyleGroupSet getStyleGroups() {
		return styleGroups;
	}

	
	public string toString() {
		return string.Format("[{0} {1} nodes {2} edges]", getId(), getNodeCount(), getEdgeCount());
	}

	public double getStep() {
		return step;
	}

	/// <summary>
/// The maximum position of a node or sprite. Notice that this is updated only each time the {@link #computeBounds()} method is called.
/// </summary>
/// <returns>The maximum node or sprite position.</returns>
	public Point3 getMaxPos() {
		return hi;
	}

	/// <summary>
/// The minimum position of a node or sprite. Notice that this is updated only each time the {@link #computeBounds()} method is called.
/// </summary>
/// <returns>The minimum node or sprite position.</returns>
	public Point3 getMinPos() {
		return lo;
	}

	/// <summary>
/// Does the graphic graph publish via attribute changes the XYZ changes on nodes and sprites when changed ?. This is disabled by default, and enabled as soon as there is at least one listener.
/// </summary>
	public bool feedbackXYZ() {
		return feedbackXYZ;
	}

	// Command

	/// <summary>
/// Should the graphic graph publish via attribute changes the XYZ changes on nodes and sprites when changed ?.
/// </summary>
	public void feedbackXYZ(bool on) {
		feedbackXYZ = on;
	}

	/// <summary>
/// Compute the overall bounds of the graphic graph according to the nodes and sprites positions. We can only compute the graph bounds from the nodes and sprites centres since the node and graph bounds may in certain circumstances be computed according to the graph bounds. The bounds are stored in the graph metrics. <p> This operation will process each node and sprite and is therefore costly. However it does this computation again only when a node or sprite moved. Therefore it can be called several times, if nothing moved in the graph, the computation will not be redone.
/// </summary>
	public void computeBounds() {
		if (boundsChanged) {
			AtomicBoolean effectiveChange = new AtomicBoolean(false);

			lo.x = lo.y = lo.z = double.MaxValue;
			hi.x = hi.y = hi.z = -double.MaxValue;

			nodes().ToList().ForEach(n => {
				GraphicNode node = (GraphicNode) n;

				if (!node.hidden && node.positionned) {
					effectiveChange.set(true);

					if (node.x < lo.x)
						lo.x = node.x;
					if (node.x > hi.x)
						hi.x = node.x;
					if (node.y < lo.y)
						lo.y = node.y;
					if (node.y > hi.y)
						hi.y = node.y;
					if (node.z < lo.z)
						lo.z = node.z;
					if (node.z > hi.z)
						hi.z = node.z;
				}
			});

			sprites().ToList().ForEach(sprite => {
				if (!sprite.isAttached() && sprite.getUnits() == StyleConstants.Units.GU) {
					double x = sprite.getX();
					double y = sprite.getY();
					double z = sprite.getZ();

					if (!sprite.hidden) {
						effectiveChange.set(true);

						if (x < lo.x)
							lo.x = x;
						if (x > hi.x)
							hi.x = x;
						if (y < lo.y)
							lo.y = y;
						if (y > hi.y)
							hi.y = y;
						if (z < lo.z)
							lo.z = z;
						if (z > hi.z)
							hi.z = z;
					}
				}
			});

			if (hi.x - lo.x < 0.000001) {
				hi.x = hi.x + 1;
				lo.x = lo.x - 1;
			}
			if (hi.y - lo.y < 0.000001) {
				hi.y = hi.y + 1;
				lo.y = lo.y - 1;
			}
			if (hi.z - lo.z < 0.000001) {
				hi.z = hi.z + 1;
				lo.z = lo.z - 1;
			}

			//
			// Prevent infinities that can be produced by double.MAX_VALUE.
			//
			if (effectiveChange[])
				boundsChanged = false;
			else {
				lo.x = lo.y = lo.z = -1;
				hi.x = hi.y = hi.z = 1;
			}
		}
	}

	protected void moveNode(string id, double x, double y, double z) {
		GraphicNode node = (GraphicNode) styleGroups.getNode(id);

		if (node != null) {
			node.x = x;
			node.y = y;
			node.z = z;
			node.setAttribute("x", x);
			node.setAttribute("y", y);
			node.setAttribute("z", z);

			graphChanged = true;
		}
	}

	
	public INode getNode(string id) {
		return styleGroups.getNode(id);
	}

	
	public IEdge getEdge(string id) {
		return styleGroups.getEdge(id);
	}

	public GraphicSprite getSprite(string id) {
		return styleGroups.getSprite(id);
	}

	
	protected void attributeChanged(AttributeChangeEvent evt, string attribute, object oldValue, object newValue) {

		// One of the most important method. Most of the communication comes
		// from attributes.

		if (attribute.Equals("ui.repaint")) {
			graphChanged = true;
		} else if (attribute.Equals("ui.stylesheet") || attribute.Equals("stylesheet")) {
			if (evt == AttributeChangeEvent.ADD || evt == AttributeChangeEvent.CHANGE) {
				if (newValue is string) {
					try {
						styleSheet.load((string) newValue);
						graphChanged = true;
					} catch (Exception e) {
						Console.Error.WriteLine(string.Format("Error while parsing style sheet for graph '{0}'.", getId()), e);
					}
				} else {
					Console.Error.WriteLine(
							string.Format("Error with stylesheet specification what to do with '{0}'.", newValue));
				}
			} else // Remove the style.
			{
				styleSheet.Clear();
				graphChanged = true;
			}
		} else if (attribute.StartsWith("ui.sprite.")) {
			// Defers the sprite handling to the sprite API.
			spriteAttribute(evt, null, attribute, newValue);
			graphChanged = true;
		}

		listeners.sendAttributeChangedEvent(getId(), ElementType.GRAPH, attribute, evt, oldValue, newValue);
	}

	/// <summary>
/// Display the node/edge relations.
/// </summary>
	public void printConnectivity() {
		IEnumerator<GraphicNode> keys = connectivity.Keys.GetEnumerator();

		System.err.printf("Graphic graph connectivity:%n");

		while (keys.MoveNext()) {
			GraphicNode node = keys.next();
			System.err.printf("    [{0}] -> ", node.getId());
			IEnumerable<GraphicEdge> edges = connectivity[node];
			foreach (GraphicEdge edge in edges)
				System.err.printf(" ({0} {1})", edge.getId(), edge.getMultiIndex());
			System.err.printf("%n");
		}
	}

	// Style group listener interface

	public void elementStyleChanged(IElement element, StyleGroup oldStyle, StyleGroup style) {
		if (element is GraphicElement) {
			GraphicElement ge = (GraphicElement) element;
			ge.style = style;
			graphChanged = true;
		} else if (element is GraphicGraph) {
			GraphicGraph gg = (GraphicGraph) element;
			gg.style = style;
			graphChanged = true;
		} else {
			throw new Exception("WTF ?");
		}
	}

	public void styleChanged(StyleGroup style) {

	}

	// Graph interface

	
	public IEnumerable<INode> nodes() {
		return styleGroups.nodes();
	}

	
	public IEnumerable<IEdge> edges() {
		return styleGroups.edges();
	}

	public IEnumerable<GraphicSprite> sprites() {
		return styleGroups.sprites();
	}

	
	public IEnumerator<INode> iterator() {
		return (IEnumerator<INode>) styleGroups.getNodeIterator();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#addSink(org.graphstream.stream.Sink)
	 */
	public void addSink(ISink listener) {
		listeners.addSink(listener);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#removeSink(org.graphstream.stream.Sink)
	 */
	public void removeSink(ISink listener) {
		listeners.removeSink(listener);
	}

	/*
	 * *(non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#addAttributeSink(org.graphstream.stream
	 * .AttributeSink)
	 */
	public void addAttributeSink(IAttributeSink listener) {
		listeners.addAttributeSink(listener);
	}

	/*
	 * *(non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#removeAttributeSink(org.graphstream.stream
	 * .AttributeSink)
	 */
	public void removeAttributeSink(IAttributeSink listener) {
		listeners.removeAttributeSink(listener);
	}

	/*
	 * *(non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#addElementSink(org.graphstream.stream.
	 * ElementSink)
	 */
	public void addElementSink(IElementSink listener) {
		listeners.addElementSink(listener);
	}

	/*
	 * *(non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#removeElementSink(org.graphstream.stream
	 * .ElementSink)
	 */
	public void removeElementSink(IElementSink listener) {
		listeners.removeElementSink(listener);
	}

	/*
	 * *(non-Javadoc)
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
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.graph.Graph#addEdge(java.lang.String, java.lang.String,
	 * java.lang.String, boolean)
	 */
	
	public IEdge addEdge(string id, string from, string to, bool directed){
		GraphicEdge edge = (GraphicEdge) styleGroups.getEdge(id);

		if (edge == null) {
			GraphicNode n1 = (GraphicNode) styleGroups.getNode(from);
			GraphicNode n2 = (GraphicNode) styleGroups.getNode(to);

			if (n1 == null)
				throw new ElementNotFoundException("node \"{0}\"", from);

			if (n2 == null)
				throw new ElementNotFoundException("node \"{0}\"", to);

			edge = new GraphicEdge(id, n1, n2, directed, null);// , attributes);

			styleGroups.addElement(edge);

			List<GraphicEdge> l1 = connectivity[n1];
			List<GraphicEdge> l2 = connectivity[n2];

			if (l1 == null) {
				l1 = new List<GraphicEdge>();
				connectivity[n1] = l1;
			}

			if (l2 == null) {
				l2 = new List<GraphicEdge>();
				connectivity[n2] = l2;
			}

			l1.Add(edge);
			l2.Add(edge);
			edge.countSameEdges(l1);

			graphChanged = true;

			listeners.sendEdgeAdded(id, from, to, directed);
		}

		return edge;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.graph.Graph#addNode(java.lang.String)
	 */
	
	public INode addNode(string id){
		GraphicNode node = (GraphicNode) styleGroups.getNode(id);

		if (node == null) {
			node = new GraphicNode(this, id, null);// , attributes);

			styleGroups.addElement(node);

			graphChanged = true;

			listeners.sendNodeAdded(id);
		}

		return node;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.graph.Graph#clear()
	 */
	
	public void clear() {
		listeners.sendGraphCleared();

		clearAttributesWithNoEvent();

		connectivity.Clear();
		styleGroups.Clear();
		styleSheet.Clear();

		step = 0;
		graphChanged = true;

		styleGroups.addElement(this);
		style = styleGroups.getStyleFor(this);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.graph.Graph#removeEdge(java.lang.String)
	 */
	
	public IEdge removeEdge(string id){
		GraphicEdge edge = (GraphicEdge) styleGroups.getEdge(id);

		if (edge != null) {
			listeners.sendEdgeRemoved(id);

			if (connectivity[edge.from] != null)
				connectivity[edge.from].Remove(edge);
			if (connectivity[edge.to] != null)
				connectivity[edge.to].Remove(edge);

			styleGroups.removeElement(edge);
			edge.removed();

			graphChanged = true;
		}

		return edge;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.graph.Graph#removeEdge(java.lang.String,
	 * java.lang.String)
	 */
	
	public IEdge removeEdge(string from, string to){
		GraphicNode node0 = (GraphicNode) styleGroups.getNode(from);
		GraphicNode node1 = (GraphicNode) styleGroups.getNode(to);

		if (node0 != null && node1 != null) {
			ICollection<GraphicEdge> edges0 = connectivity[node0];
			ICollection<GraphicEdge> edges1 = connectivity[node1];

			foreach (GraphicEdge edge0 in edges0) {
				foreach (GraphicEdge edge1 in edges1) {
					if (edge0 == edge1) {
						removeEdge(edge0.getId());
						return edge0;
					}
				}
			}
		}

		return null;
	}

	/*
	 * *(non-Javadoc)
	 * 
	 * @see org.graphstream.graph.Graph#removeNode(java.lang.String)
	 */
	
	public INode removeNode(string id){
		GraphicNode node = (GraphicNode) styleGroups.getNode(id);

		if (node != null) {
			listeners.sendNodeRemoved(id);

			if (connectivity[node] != null) {
				// We must do a copy of the connectivity set for the node
				// since we will be modifying the connectivity as we process
				// edges.
				List<GraphicEdge> l = new List<GraphicEdge>(connectivity[node]);

				foreach (GraphicEdge edge in l)
					removeEdge(edge.getId());

				connectivity.Remove(node);
			}

			styleGroups.removeElement(node);
			node.removed();

			graphChanged = true;
		}

		return node;
	}

	public IViewer display() {
		throw new Exception("GraphicGraph is used by display() and cannot recursively define display()");
	}

	public IViewer display(bool autoLayout) {
		throw new Exception("GraphicGraph is used by display() and cannot recursively define display()");
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.graph.Graph#stepBegins(double)
	 */
	
	public void stepBegins(double step) {
		listeners.sendStepBegins(step);
		this.step = step;
	}

	
	public IEdgeFactory<IEdge> edgeFactory() {
		throw new Exception("GraphicGraph does not support IEdgeFactory");
	}

	
	public int getEdgeCount() {
		return styleGroups.getEdgeCount();
	}

	
	public int getNodeCount() {
		return styleGroups.getNodeCount();
	}

	public int getSpriteCount() {
		return styleGroups.getSpriteCount();
	}

	
	public bool isAutoCreationEnabled() {
		return false;
	}

	
	public INodeFactory<INode> nodeFactory() {
		throw new Exception("GraphicGraph does not support INodeFactory");
	}

	
	public void setAutoCreate(bool on) {
		throw new Exception("GraphicGraph does not support auto-creation");
	}

	
	public bool isStrict() {
		return false;
	}

	
	public void setStrict(bool on) {
		throw new Exception("GraphicGraph does not support strict checking");
	}

	
	public void setEdgeFactory(IEdgeFactory<IEdge> ef) {
		throw new Exception("you cannot change the edge factory for graphic graphs !");
	}

	
	public void setNodeFactory(INodeFactory<INode> nf) {
		throw new Exception("you cannot change the node factory for graphic graphs !");
	}

	
	public void read(string filename){
		throw new Exception("GraphicGraph does not support I/O");
	}

	
	public void read(IFileSource input, string filename){
		throw new Exception("GraphicGraph does not support I/O");
	}

	
	public void write(IFileSink output, string filename){
		throw new Exception("GraphicGraph does not support I/O");
	}

	
	public void write(string filename){
		throw new Exception("GraphicGraph does not support I/O");
	}

	// Output interface

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		listeners.edgeAttributeAdded(sourceId, timeId, edgeId, attribute, value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		listeners.edgeAttributeChanged(sourceId, timeId, edgeId, attribute, oldValue, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		listeners.edgeAttributeRemoved(sourceId, timeId, edgeId, attribute);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang.String ,
	 * long, java.lang.String, java.lang.object)
	 */
	
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		listeners.graphAttributeAdded(sourceId, timeId, attribute, value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang.
	 * String, long, java.lang.String, java.lang.object, java.lang.object)
	 */
	
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		listeners.graphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang.
	 * String, long, java.lang.String)
	 */
	
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		listeners.graphAttributeRemoved(sourceId, timeId, attribute);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		listeners.nodeAttributeAdded(sourceId, timeId, nodeId, attribute, value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		listeners.nodeAttributeChanged(sourceId, timeId, nodeId, attribute, oldValue, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		listeners.nodeAttributeRemoved(sourceId, timeId, nodeId, attribute);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeAdded(java.lang.String, long,
	 * java.lang.String, java.lang.String, java.lang.String, boolean)
	 */
	
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		listeners.edgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);
	}

	/*
	 * *(non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		listeners.edgeRemoved(sourceId, timeId, edgeId);
	}

	/*
	 * *(non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#graphCleared(java.lang.String, long)
	 */
	
	public void graphCleared(string sourceId, long timeId) {
		listeners.graphCleared(sourceId, timeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeAdded(java.lang.String, long,
	 * java.lang.String)
	 */
	
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		listeners.nodeAdded(sourceId, timeId, nodeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		listeners.nodeRemoved(sourceId, timeId, nodeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#stepBegins(java.lang.String, long,
	 * double)
	 */
	
	public void stepBegins(string sourceId, long timeId, double time) {
		listeners.sendStepBegins(sourceId, timeId, time);
		stepBegins(time);
	}

	// Sprite interface

	protected void spriteAttribute(AttributeChangeEvent evt, IElement element, string attribute, object value) {
		string spriteId = attribute.Substring(10); // Remove the "ui.sprite."
		// prefix.
		int pos = spriteId.IndexOf('.'); // Look if there is something after the
		// sprite id.
		string attr = null;

		if (pos > 0) {
			attr = spriteId.Substring(pos + 1); // Cut the sprite id.
			spriteId = spriteId.Substring(0, pos); // Cut the sprite attribute
			// name.
		}

		if (attr == null) {
			addOrChangeSprite(evt, element, spriteId, value);
		} else {
			if (evt == AttributeChangeEvent.ADD) {
				GraphicSprite sprite = styleGroups.getSprite(spriteId);

				// We add the sprite, in case of a replay, some attributes of
				// the sprite can be
				// changed before the sprite is declared.
				if (sprite == null) {
					addOrChangeSprite(AttributeChangeEvent.ADD, element, spriteId, null);
					sprite = styleGroups.getSprite(spriteId);
				}

				sprite.setAttribute(attr, value);
			} else if (evt == AttributeChangeEvent.CHANGE) {
				GraphicSprite sprite = styleGroups.getSprite(spriteId);

				if (sprite == null) {
					addOrChangeSprite(AttributeChangeEvent.ADD, element, spriteId, null);
					sprite = styleGroups.getSprite(spriteId);
				}

				sprite.setAttribute(attr, value);
			} else if (evt == AttributeChangeEvent.REMOVE) {
				GraphicSprite sprite = styleGroups.getSprite(spriteId);

				if (sprite != null)
					sprite.removeAttribute(attr);
			}
		}
	}

	protected void addOrChangeSprite(AttributeChangeEvent evt, IElement element, string spriteId, object value) {

		if (evt == AttributeChangeEvent.ADD || evt == AttributeChangeEvent.CHANGE) {
			GraphicSprite sprite = styleGroups.getSprite(spriteId);

			if (sprite == null)
				sprite = addSprite_(spriteId);

			if (element != null) {
				if (element is GraphicNode)
					sprite.attachToNode((GraphicNode) element);
				else if (element is GraphicEdge)
					sprite.attachToEdge((GraphicEdge) element);
			}

			if (value != null && (!(value is bool)))
				positionSprite(sprite, value);
		} else if (evt == AttributeChangeEvent.REMOVE) {
			if (element == null) {
				if (styleGroups.getSprite(spriteId) != null) {
					removeSprite_(spriteId);
				}
			} else {
				GraphicSprite sprite = styleGroups.getSprite(spriteId);

				if (sprite != null)
					sprite.detach();
			}
		}
	}

	public GraphicSprite addSprite(string id) {
		string prefix = string.Format("ui.sprite.{0}", id);
		Console.WriteLine(string.Format("Added sprite {0}.", id));
		setAttribute(prefix, 0, 0, 0);
		GraphicSprite s = styleGroups.getSprite(id);
		System.Diagnostics.Debug.Assert((s != null));
		return s;
	}

	protected GraphicSprite addSprite_(string id) {
		GraphicSprite s = new GraphicSprite(id, this);
		styleGroups.addElement(s);
		graphChanged = true;

		return s;
	}

	public void removeSprite(string id) {
		string prefix = string.Format("ui.sprite.{0}", id);
		removeAttribute(prefix);
	}

	protected GraphicSprite removeSprite_(string id) {
		GraphicSprite sprite = (GraphicSprite) styleGroups.getSprite(id);

		if (sprite != null) {
			sprite.detach();
			styleGroups.removeElement(sprite);
			sprite.removed();

			graphChanged = true;
		}

		return sprite;
	}

	protected void positionSprite(GraphicSprite sprite, object value) {
		if (value is object[]) {
			object[] values = (object[]) value;

			if (values.Length == 4) {
				if (values[0] is IConvertible && values[1] is IConvertible && values[2] is IConvertible
						&& values[3] is Style.Units) {
					sprite.setPosition(((IConvertible) values[0]), ((IConvertible) values[1]),
							((IConvertible) values[2]), (Style.Units) values[3]);
				} else {
					Console.Error.WriteLine("Cannot parse values[4] for sprite position.");
				}
			} else if (values.Length == 3) {
				if (values[0] is IConvertible && values[1] is IConvertible && values[2] is IConvertible) {
					sprite.setPosition(((IConvertible) values[0]), ((IConvertible) values[1]),
							((IConvertible) values[2]), Units.GU);
				} else {
					Console.Error.WriteLine("Cannot parse values[3] for sprite position.");
				}
			} else if (values.Length == 1) {
				if (values[0] is IConvertible) {
					sprite.setPosition(((IConvertible) values[0]));
				} else {
					Console.Error.WriteLine("ISprite position percent is not a number.");
				}
			} else {
				Console.Error.WriteLine(string.Format("Cannot transform value '{0}' (length={1}) into a position\n",
						"[values]"(values), values.Length));
			}
		} else if (value is IConvertible) {
			sprite.setPosition(((IConvertible) value));
		} else if (value is Value) {
			sprite.setPosition(((Value) value).value);
		} else if (value is Values) {
			sprite.setPosition((Values) value);
		} else if (value == null) {
			throw new Exception("What do you expect with a null value ?");
		} else {
			Console.Error.WriteLine(string.Format("Cannot place sprite with posiiton '{0}' (instance of {1})\n", value,
					value.GetType().Name));
		}
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
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.Source#clearSinks()
	 */
	
	public void clearSinks() {
		listeners.clearSinks();
	}

	// stubs for the new methods

	
	public IEdge addEdge(string id, int index1, int index2) {
		throw new Exception("not implemented !");
	}

	
	public IEdge addEdge(string id, int fromIndex, int toIndex, bool directed) {
		throw new Exception("not implemented !");
	}

	
	public IEdge addEdge(string id, INode node1, INode node2) {
		throw new Exception("not implemented !");
	}

	
	public IEdge addEdge(string id, INode from, INode to, bool directed) {
		throw new Exception("not implemented !");
	}

	
	public IEdge getEdge(int index){
		throw new Exception("not implemented !");
	}

	
	public INode getNode(int index){
		throw new Exception("not implemented !");
	}

	
	public IEdge removeEdge(int index) {
		throw new Exception("not implemented !");
	}

	
	public IEdge removeEdge(int fromIndex, int toIndex) {
		throw new Exception("not implemented !");
	}

	
	public IEdge removeEdge(INode node1, INode node2) {
		throw new Exception("not implemented !");
	}

	
	public IEdge removeEdge(IEdge edge) {
		throw new Exception("not implemented !");
	}

	
	public INode removeNode(int index) {
		throw new Exception("not implemented !");
	}

	
	public INode removeNode(INode node) {
		throw new Exception("not implemented !");
	}

	/// <summary>
/// Replay all the elements of the graph and all attributes as new events to all connected sinks. <p> Be very careful with this method, it introduces new events in the event stream and some sinks may therefore receive them twice !! Graph replay is always dangerous !
/// </summary>
	public void replay() {
		// Replay all graph attributes.

		attributeKeys().ToList().ForEach(key => {
			listeners.sendGraphAttributeAdded(id, key, getAttribute(key));
		});

		// Replay all nodes and their attributes.

		nodes().ToList().ForEach(node => {
			listeners.sendNodeAdded(id, node.getId());

			node.attributeKeys().ToList().ForEach(key => {
				listeners.sendNodeAttributeAdded(id, node.getId(), key, node.getAttribute(key));
			});
		});

		// Replay all edges and their attributes.

		edges().ToList().ForEach(edge => {
			listeners.sendEdgeAdded(id, edge.getId(), edge.getSourceNode().getId(), edge.getTargetNode().getId(),
					edge.isDirected());

			edge.attributeKeys().ToList().ForEach(key => {
				listeners.sendEdgeAttributeAdded(id, edge.getId(), key, edge.getAttribute(key));
			});
		});
	}
}
}
