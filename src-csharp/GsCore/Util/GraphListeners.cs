using System.Collections.Generic;
using System.Linq;
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


/// <summary>
/// Helper object to handle events producted by a graph.
/// </summary>
public class GraphListeners : SourceBase, IPipe {

	SinkTime sinkTime;
	bool passYourWay, passYourWayAE;
	string dnSourceId;
	long dnTimeId;

	IGraph g;

	public GraphListeners(IGraph g) : base(g.getId()) {

		this.sinkTime = new SinkTime();
		this.sourceTime.setSinkTime(sinkTime);
		this.passYourWay = false;
		this.passYourWayAE = false;
		this.dnSourceId = null;
		this.dnTimeId = long.MIN_VALUE;
		this.g = g;
	}

	public long newEvent() {
		return sourceTime.newEvent();
	}

	public void sendAttributeChangedEvent(string eltId, ElementType eltType, string attribute,
			AttributeChangeEvent evt, object oldValue, object newValue) {
		//
		// Attributes with name beginnig with a dot are hidden.
		//
		if (passYourWay || attribute[0] == '.')
			return;

		sendAttributeChangedEvent(sourceId, newEvent(), eltId, eltType, attribute, evt, oldValue, newValue);
	}

	public void sendNodeAdded(string nodeId) {
		if (passYourWay)
			return;

		sendNodeAdded(sourceId, newEvent(), nodeId);
	}

	public void sendNodeRemoved(string nodeId) {
		if (dnSourceId != null) {
			sendNodeRemoved(dnSourceId, dnTimeId, nodeId);
		} else {
			sendNodeRemoved(sourceId, newEvent(), nodeId);
		}
	}

	public void sendEdgeAdded(string edgeId, string source, string target, bool directed) {
		if (passYourWayAE)
			return;

		sendEdgeAdded(sourceId, newEvent(), edgeId, source, target, directed);
	}

	public void sendEdgeRemoved(string edgeId) {
		if (passYourWay)
			return;

		sendEdgeRemoved(sourceId, newEvent(), edgeId);
	}

	public void sendGraphCleared() {
		if (passYourWay)
			return;

		sendGraphCleared(sourceId, newEvent());
	}

	public void sendStepBegins(double step) {
		if (passYourWay)
			return;

		sendStepBegins(sourceId, newEvent(), step);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang
	 * .String, long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			IEdge edge = g.getEdge(edgeId);
			if (edge != null) {
				passYourWay = true;

				try {
					edge.setAttribute(attribute, value);
				} finally {
					passYourWay = false;
				}

				sendEdgeAttributeAdded(sourceId, timeId, edgeId, attribute, value);
			}
		}
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
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			IEdge edge = g.getEdge(edgeId);
			if (edge != null) {
				passYourWay = true;

				if (oldValue == null)
					oldValue = edge.getAttribute(attribute);

				try {
					edge.setAttribute(attribute, newValue);
				} finally {
					passYourWay = false;
				}

				sendEdgeAttributeChanged(sourceId, timeId, edgeId, attribute, oldValue, newValue);
			}
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang
	 * .String, long, java.lang.String, java.lang.String)
	 */
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			IEdge edge = g.getEdge(edgeId);
			if (edge != null) {
				sendEdgeAttributeRemoved(sourceId, timeId, edgeId, attribute);
				passYourWay = true;

				try {
					edge.removeAttribute(attribute);
				} finally {
					passYourWay = false;
				}

			}
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang
	 * .String, long, java.lang.String, java.lang.object)
	 */
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			passYourWay = true;

			try {
				g.setAttribute(attribute, value);
			} finally {
				passYourWay = false;
			}

			sendGraphAttributeAdded(sourceId, timeId, attribute, value);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang
	 * .String, long, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			passYourWay = true;

			if (oldValue == null)
				oldValue = g.getAttribute(attribute);

			try {
				g.setAttribute(attribute, newValue);
			} finally {
				passYourWay = false;
			}

			sendGraphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang
	 * .String, long, java.lang.String)
	 */
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			sendGraphAttributeRemoved(sourceId, timeId, attribute);
			passYourWay = true;

			try {
				g.removeAttribute(attribute);
			} finally {
				passYourWay = false;
			}
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang
	 * .String, long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			INode node = g.getNode(nodeId);
			if (node != null) {
				passYourWay = true;

				try {
					node.setAttribute(attribute, value);
				} finally {
					passYourWay = false;
				}

				sendNodeAttributeAdded(sourceId, timeId, nodeId, attribute, value);
			}
		}
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
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			INode node = g.getNode(nodeId);
			if (node != null) {
				passYourWay = true;

				if (oldValue == null)
					oldValue = node.getAttribute(attribute);

				try {
					node.setAttribute(attribute, newValue);
				} finally {
					passYourWay = false;
				}

				sendNodeAttributeChanged(sourceId, timeId, nodeId, attribute, oldValue, newValue);
			}
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang
	 * .String, long, java.lang.String, java.lang.String)
	 */
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			INode node = g.getNode(nodeId);
			if (node != null) {
				sendNodeAttributeRemoved(sourceId, timeId, nodeId, attribute);
				passYourWay = true;

				try {
					node.removeAttribute(attribute);
				} finally {
					passYourWay = false;
				}
			}
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
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			passYourWayAE = true;

			try {
				g.addEdge(edgeId, fromNodeId, toNodeId, directed);
			} finally {
				passYourWayAE = false;
			}

			sendEdgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			sendEdgeRemoved(sourceId, timeId, edgeId);
			passYourWay = true;

			try {
				g.removeEdge(edgeId);
			} finally {
				passYourWay = false;
			}
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#graphCleared(java.lang.String, long)
	 */
	public void graphCleared(string sourceId, long timeId) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			sendGraphCleared(sourceId, timeId);
			passYourWay = true;

			try {
				g.Clear();
			} finally {
				passYourWay = false;
			}
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeAdded(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			passYourWay = true;

			try {
				g.addNode(nodeId);
			} finally {
				passYourWay = false;
			}

			sendNodeAdded(sourceId, timeId, nodeId);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			// sendNodeRemoved(sourceId, timeId, nodeId);
			dnSourceId = sourceId;
			dnTimeId = timeId;

			try {
				g.removeNode(nodeId);
			} finally {
				dnSourceId = null;
				dnTimeId = long.MIN_VALUE;
			}
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#stepBegins(java.lang.String, long,
	 * double)
	 */
	public void stepBegins(string sourceId, long timeId, double step) {
		if (sinkTime.isNewEvent(sourceId, timeId)) {
			passYourWay = true;

			try {
				g.stepBegins(step);
			} finally {
				passYourWay = false;
			}

			sendStepBegins(sourceId, timeId, step);
		}
	}

	
	public string toString() {
		return string.Format("GraphListeners of {0}.{1}", g.GetType().Name, g.getId());
	}
}
}
