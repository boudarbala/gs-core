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
/// Shell around a proxy pipe coming from the viewer allowing to put viewer listeners on a viewer that runs in a distinct thread. <p> This pipe is a probe that you can place in the event loop between the viewer and the graph. It will transmit all events coming from the viewer to the graph (or any sink you connect to it). But in addition it will monitor standard attribute changes to redistribute them to specify "viewer listeners". </p> <p> As any proxy pipe, a viewer pipe must be "pumped" to receive events coming from other threads. </p>
/// </summary>
public class ViewerPipe : SourceBase, IProxyPipe {
	// Attribute

	private string id;

	/// <summary>
/// The incoming event stream.
/// </summary>
	protected IProxyPipe pipeIn;

	/// <summary>
/// Listeners on the viewer specific events.
/// </summary>
	protected HashSet<IViewerListener> viewerListeners = new HashSet<IViewerListener>();

	// Construction

	/// <summary>
/// A shell around a pipe coming from a viewer in another thread.
/// </summary>
	public IViewerPipe(string id, IProxyPipe pipeIn) {
		this.id = id;
		this.pipeIn = pipeIn;
		pipeIn.addSink(this);
	}

	// Access

	public string getId() {
		return id;
	}

	// Commands

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ProxyPipe#pump()
	 */
	public void pump() {
		pipeIn.pump();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ProxyPipe#blockingPump()
	 */
	public void blockingPump(){
		pipeIn.blockingPump();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ProxyPipe#blockingPump(long)
	 */
	public void blockingPump(long timeout){
		pipeIn.blockingPump(timeout);
	}

	public void addViewerListener(IViewerListener listener) {
		viewerListeners.Add(listener);
	}

	public void removeViewerListener(IViewerListener listener) {
		viewerListeners.Remove(listener);
	}

	// Sink interface

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		sendEdgeAttributeAdded(sourceId, timeId, edgeId, attribute, value);
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
		sendEdgeAttributeChanged(sourceId, timeId, edgeId, attribute, oldValue, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		sendEdgeAttributeRemoved(sourceId, timeId, edgeId, attribute);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang.String ,
	 * long, java.lang.String, java.lang.object)
	 */
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		sendGraphAttributeAdded(sourceId, timeId, attribute, value);

		if (attribute.Equals("ui.viewClosed") && value is string) {
			foreach (IViewerListener listener in viewerListeners)
				listener.viewClosed((string) value);

			sendGraphAttributeRemoved(id, attribute);
		} else if (attribute.Equals("ui.clicked") && value is string) {
			foreach (IViewerListener listener in viewerListeners)
				listener.buttonPushed((string) value);

			sendGraphAttributeRemoved(id, attribute);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang.
	 * String, long, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		sendGraphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang.
	 * String, long, java.lang.String)
	 */
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		sendGraphAttributeRemoved(sourceId, timeId, attribute);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		sendNodeAttributeAdded(sourceId, timeId, nodeId, attribute, value);

		if (attribute.Equals("ui.clicked")) {
			foreach (IViewerListener listener in viewerListeners)
				listener.buttonPushed(nodeId);
		}

		if (attribute.Equals("ui.mouseOver")) {
			foreach (IViewerListener listener in viewerListeners)
				listener.mouseOver(nodeId);
		}
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
		sendNodeAttributeChanged(sourceId, timeId, nodeId, attribute, oldValue, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		sendNodeAttributeRemoved(sourceId, timeId, nodeId, attribute);

		if (attribute.Equals("ui.clicked")) {
			foreach (IViewerListener listener in viewerListeners)
				listener.buttonReleased(nodeId);
		}

		if (attribute.Equals("ui.mouseOver")) {
			foreach (IViewerListener listener in viewerListeners)
				listener.mouseLeft(nodeId);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeAdded(java.lang.String, long,
	 * java.lang.String, java.lang.String, java.lang.String, boolean)
	 */
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		sendEdgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		sendEdgeRemoved(sourceId, timeId, edgeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#graphCleared(java.lang.String, long)
	 */
	public void graphCleared(string sourceId, long timeId) {
		sendGraphCleared(sourceId, timeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeAdded(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		sendNodeAdded(sourceId, timeId, nodeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		sendNodeRemoved(sourceId, timeId, nodeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#stepBegins(java.lang.String, long,
	 * double)
	 */
	public void stepBegins(string sourceId, long timeId, double step) {
		sendStepBegins(sourceId, timeId, step);
	}
}
}
