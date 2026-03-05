using System.Collections.Generic;
using System.Linq;
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
/// A base pipe that merely let all events pass. <p> This pipe does nothing and let all events pass. It can be used as a base to implement more specific filters by refining some of its methods. </p> <p> Another use of this pipe is to duplicate a stream of events from one input toward several outputs. </p>
/// </summary>
public class PipeBase : SourceBase, IPipe {
	public void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value) {
		sendEdgeAttributeAdded(graphId, timeId, edgeId, attribute, value);
	}

	public void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		sendEdgeAttributeChanged(graphId, timeId, edgeId, attribute, oldValue, newValue);
	}

	public void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute) {
		sendEdgeAttributeRemoved(graphId, timeId, edgeId, attribute);
	}

	public void graphAttributeAdded(string graphId, long timeId, string attribute, object value) {
		sendGraphAttributeAdded(graphId, timeId, attribute, value);
	}

	public void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue) {
		sendGraphAttributeChanged(graphId, timeId, attribute, oldValue, newValue);
	}

	public void graphAttributeRemoved(string graphId, long timeId, string attribute) {
		sendGraphAttributeRemoved(graphId, timeId, attribute);
	}

	public void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value) {
		sendNodeAttributeAdded(graphId, timeId, nodeId, attribute, value);
	}

	public void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		sendNodeAttributeChanged(graphId, timeId, nodeId, attribute, oldValue, newValue);
	}

	public void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute) {
		sendNodeAttributeRemoved(graphId, timeId, nodeId, attribute);
	}

	public void edgeAdded(string graphId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		sendEdgeAdded(graphId, timeId, edgeId, fromNodeId, toNodeId, directed);
	}

	public void edgeRemoved(string graphId, long timeId, string edgeId) {
		sendEdgeRemoved(graphId, timeId, edgeId);
	}

	public void graphCleared(string graphId, long timeId) {
		sendGraphCleared(graphId, timeId);
	}

	public void nodeAdded(string graphId, long timeId, string nodeId) {
		sendNodeAdded(graphId, timeId, nodeId);
	}

	public void nodeRemoved(string graphId, long timeId, string nodeId) {
		sendNodeRemoved(graphId, timeId, nodeId);
	}

	public void stepBegins(string graphId, long timeId, double step) {
		sendStepBegins(graphId, timeId, step);
	}
}
}
