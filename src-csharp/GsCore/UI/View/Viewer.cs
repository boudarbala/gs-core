using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.View
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
/// Set of views on a graphic graph. <p> The viewer class is in charge of maintaining : <ul> <li>A "graphic graph" (a special graph that internally stores the graph under the form of style sets of "graphic" elements, suitable to draw the graph, but not to adapted to used it as a general graph),</li> <li>The eventual proxy pipe from which the events come from (but graph events can come from any kind of source),</li> <li>A default view, and eventually more views on the graphic graph.</li> <li>A flag that allows to repaint the view only if the graphic graph changed. <li> </ul> </p> <p> The graphic graph can be created by the viewer or given at construction (to share it with another viewer). </p> <p> <u>Once created, the viewer runs in a loop inside the UI thread. You cannot call methods on it directly if you are not in this thread</u>. The only operation that you can use in other threads is the constructor, the {@link #addView(View)} {@link #removeView(String)} and the {@link #close()} methods. Other methods are not protected from concurrent accesses. </p> <p> Some constructors allow a {@link ProxyPipe} as argument. If given, the graphic graph is made listener of this pipe and the pipe is "pumped" during the view loop. This allows to run algorithms on a graph in the main thread (or any other thread) while letting the viewer run in the ui thread. </p> <p> Be very careful: due to the nature of graph events in GraphStream, the viewer is not aware of events that occured on the graph <u>before</u> its creation. There is a special mechanism that replay the graph if you use a proxy pipe or if you pass the graph directly. However, when you create the viewer by yourself and only pass a {@link Source}, the viewer <u>will not</u> display the events that occured on the source before it is connected to it. </p>
/// </summary>
abstract class Viewer {

	// Attributes

	/// <summary>
/// How does the viewer synchronise its internal graphic graph with the graph displayed. The graph we display can be in the Swing thread (as will be the viewer, therefore in the same thread as the viewer), in another thread, or on a distant machine.
/// </summary>
	public enum ThreadingModel {
		GRAPH_IN_GUI_THREAD, GRAPH_IN_ANOTHER_THREAD, GRAPH_ON_NETWORK
	}

	/// <summary>
/// Name of the default view.
/// </summary>
	public abstract string getDefaultID;

	// Attribute

	/// <summary>
/// If true the graph we display is in another thread, the synchronisation between the graph and the graphic graph must therefore use thread proxies.
/// </summary>
	protected bool graphInAnotherThread = true;

	/// <summary>
/// The graph observed by the views.
/// </summary>
	protected GraphicGraph graph;

	/// <summary>
/// If we have to pump events by ourself.
/// </summary>
	protected IProxyPipe pumpPipe;

	/// <summary>
/// If we take graph events from a source in this thread.
/// </summary>
	protected ISource sourceInSameThread;

	/// <summary>
/// The set of views.
/// </summary>
	protected Dictionary<string, IView> views = new SortedDictionary<string, IView>();

	/// <summary>
/// What to do when a view frame is closed.
/// </summary>
	protected CloseFramePolicy closeFramePolicy = CloseFramePolicy.EXIT;

	// Attribute

	/// <summary>
/// Optional layout algorithm running in another thread.
/// </summary>
	protected LayoutRunner optLayout = null;

	/// <summary>
/// If there is a layout in another thread, this is the pipe coming from it.
/// </summary>
	protected IProxyPipe layoutPipeIn = null;

	/// <summary>
/// What to do when a view frame is closed.
/// </summary>
	enum CloseFramePolicy {
		CLOSE_VIEWER, HIDE_ONLY, EXIT
	}

	/// <summary>
/// Create a new unique identifier for a graph.
/// </summary>
/// <returns>The new identifier.</returns>
	public string newGGId {
		return string.Format(Math.random * 10000));
	}

	/// <summary>
/// Initialise the viewer.
/// </summary>
/// <param name="graph"> The graphic graph.</param>
/// <param name="ppipe"> The source of events from another thread or machine.</param>
/// <param name="source"> The source of events from this thread.</param>
	public abstract void init;

	/// <summary>
/// Close definitively this viewer and all its views.
/// </summary>
	public abstract void close();
	// Access

	/// <summary>
/// What to do when a frame is closed.
/// </summary>
	public CloseFramePolicy getCloseFramePolicy() {
		return closeFramePolicy;
	}

	/// <summary>
/// New proxy pipe on events coming from the viewer through a thread.
/// </summary>
/// <returns>The new proxy pipe.</returns>
	public IProxyPipe newThreadProxyOnGraphicGraph() {
		ThreadProxyPipe tpp = new ThreadProxyPipe();
		tpp.init(graph);
		return tpp;
	}

	/// <summary>
/// New viewer pipe on the events coming from the viewer through a thread.
/// </summary>
/// <returns>The new viewer pipe.</returns>
	public IViewerPipe newViewerPipe() {
		ThreadProxyPipe tpp = new ThreadProxyPipe();
		tpp.init(graph, false);

		enableXYZfeedback(true);

		return new IViewerPipe(string.Format("viewer_{0}", (int) (new Random().NextDouble() * 10000)), tpp);
	}

	/// <summary>
/// The underlying graphic graph. Caution : Use the returned graph only in the UI thread !!
/// </summary>
	public GraphicGraph getGraphicGraph() {
		return graph;
	}

	/// <summary>
/// The view that correspond to the given identifier.
/// </summary>
/// <param name="id"> The view identifier.</param>
/// <returns>A view or null if not found.</returns>
	public IView getView(string id) {
		lock (views) {
			return views[id];
		}
	}

	/// <summary>
/// The default view. This is a shortcut to a call to {@link #getView(String)} with {@link #DEFAULT_VIEW_ID} as parameter.
/// </summary>
/// <returns>The default view or null if no default view has been installed.</returns>
	public IView getDefaultView() {
		return getView(getDefaultID());
	}

	// Command
	/// <summary>
/// Create a new instance of the default graph renderer.
/// </summary>
	public abstract IGraphRenderer<object, object> newDefaultGraphRenderer();

	/// <summary>
/// Build the default graph view and insert it. The view identifier is {@link #DEFAULT_VIEW_ID}. You can request the view to be open in its own frame.
/// </summary>
/// <param name="openInAFrame"> It true, the view is placed in a frame, else the view is only created and you must embed it yourself in your application.</param>
	public IView addDefaultView(bool openInAFrame) {
		lock (views) {
			IGraphRenderer<object, object> renderer = newDefaultGraphRenderer();
			IView view = renderer.createDefaultView(this, getDefaultID());

			addView(view);

			if (openInAFrame)
				view.openInAFrame(true);

			return view;
		}
	}

	/// <summary>
/// Add a view using its identifier. If there was already a view with this identifier, it is closed and returned (if different of the one added).
/// </summary>
/// <param name="view"> The view to add.</param>
/// <returns>The old view that was at the given identifier, if any, else null.</returns>
	public IView addView(IView view) {
		lock (views) {
			IView old = views[view.getIdView()] = view;

			if (old != null && old != view)
				old.close(graph);

			return old;
		}
	}

	/// <summary>
/// Add a new default view with a specific renderer. If a view with the same id exists, it is removed and closed. By default the view is open in a frame.
/// </summary>
/// <param name="id"> The new view identifier.</param>
/// <param name="renderer"> The renderer to use.</param>
/// <returns>The created view.</returns>
	public IView addView(string id, IGraphRenderer<object, object> renderer) {
		return addView(id, renderer, true);
	}

	/// <summary>
/// Same as {@link #addView(String, GraphRenderer)} but allows to specify that the view uses a frame or not.
/// </summary>
/// <param name="id"> The new view identifier.</param>
/// <param name="renderer"> The renderer to use.</param>
/// <param name="openInAFrame"> If true the view is open in a frame, else the returned view is a JPanel that can be inserted in a GUI.</param>
/// <returns>The created view.</returns>
	public IView addView(string id, IGraphRenderer<object, object> renderer, bool openInAFrame) {
		lock (views) {
			IView view = renderer.createDefaultView(this, id);
			addView(view);

			if (openInAFrame)
				view.openInAFrame(true);

			return view;
		}
	}

	/// <summary>
/// Remove a view. The view is not closed.
/// </summary>
/// <param name="id"> The view identifier.</param>
	public void removeView(string id) {
		lock (views) {
			views.Remove(id);
		}
	}

	/// <summary>
/// Compute the overall bounds of the graphic graph according to the nodes and sprites positions. We can only compute the graph bounds from the nodes and sprites centres since the node and graph bounds may in certain circumstances be computed according to the graph bounds. The bounds are stored in the graph metrics.
/// </summary>
	public void computeGraphMetrics() {
		graph.computeBounds();

		lock (views) {
			Point3 lo = graph.getMinPos();
			Point3 hi = graph.getMaxPos();
			foreach (IView view in views.Values) {
				ICamera camera = view.getCamera();
				if (camera != null) {
					camera.setBounds(lo.x, lo.y, lo.z, hi.x, hi.y, hi.z);
				}
			}
		}
	}

	/// <summary>
/// What to do when the frame containing one or more views is closed.
/// </summary>
/// <param name="policy"> The close frame policy.</param>
	public void setCloseFramePolicy(CloseFramePolicy policy) {
		lock (views) {
			closeFramePolicy = policy;
		}
	}

	// Optional layout algorithm

	/// <summary>
/// Enable or disable the "xyz" attribute change when a node is moved in the views. By default the "xyz" attribute is changed. By default, each time a node of the graphic graph is moved, its "xyz" attribute is reset to follow the node position. This is useful only if someone listen at the graphic graph or use the graphic graph directly. But this operation is quite costly. Therefore by default if this viewer runs in its own thread, and the main graph is in another thread, xyz attribute change will be disabled until a listener is added. When the viewer is created to be used only in the ui thread, this feature is always on.
/// </summary>
	public void enableXYZfeedback(bool on) {
		lock (views) {
			graph.feedbackXYZ(on);
		}
	}

	/// <summary>
/// Launch an automatic layout process that will position nodes in the background.
/// </summary>
	public void enableAutoLayout() {
		enableAutoLayout(Layouts.newLayoutAlgorithm());
	}

	/// <summary>
/// Launch an automatic layout process that will position nodes in the background.
/// </summary>
/// <param name="layoutAlgorithm"> The algorithm to use (see Layouts.newLayoutAlgorithm() for the default algorithm).</param>
	public void enableAutoLayout(ILayout layoutAlgorithm) {
		lock (views) {
			if (optLayout == null) {
				// optLayout = new LayoutRunner(graph, layoutAlgorithm, true,
				// true);
				optLayout = new LayoutRunner(graph, layoutAlgorithm, true, false);
				graph.replay();
				layoutPipeIn = optLayout.newLayoutPipe();
				layoutPipeIn.addAttributeSink(graph);
			}
		}
	}

	/// <summary>
/// Disable the running automatic layout process, if any.
/// </summary>
	public void disableAutoLayout() {
		lock (views) {
			if (optLayout != null) {
				((ThreadProxyPipe) layoutPipeIn).unregisterFromSource();
				layoutPipeIn.removeSink(graph);
				layoutPipeIn = null;
				optLayout.release();
				optLayout = null;
			}
		}
	}

	/// <summary>
/// Dirty replay of the graph.
/// </summary>
	public void replayGraph(IGraph graph) {
		// Replay all graph attributes.

		graph.attributeKeys().ToList().ForEach(key => {
			this.graph.setAttribute(key, graph.getAttribute(key));
		});

		// Replay all nodes and their attributes.

		graph.nodes().ToList().ForEach(node => {
			INode n = this.graph.addNode(node.getId());

			node.attributeKeys().ToList().ForEach(key => {
				n.setAttribute(key, node.getAttribute(key));
			});
		});

		// Replay all edges and their attributes.

		graph.edges().ToList().ForEach(edge => {
			IEdge e = this.graph.addEdge(edge.getId(), edge.getSourceNode().getId(), edge.getTargetNode().getId(),
					edge.isDirected());

			edge.attributeKeys().ToList().ForEach(key => {
				e.setAttribute(key, edge.getAttribute(key));
			});
		});
	}
}

}
