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
/// Adapter for the {@link Sink} interface. <p> All methods are empty. </p>
/// </summary>
public class SinkAdapter : ISink {
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
	}

	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
	}

	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
	}

	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
	}

	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
	}

	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
	}

	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
	}

	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
	}

	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
	}

	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
	}

	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
	}

	public void graphCleared(string sourceId, long timeId) {
	}

	public void nodeAdded(string sourceId, long timeId, string nodeId) {
	}

	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
	}

	public void stepBegins(string sourceId, long timeId, double step) {
	}
}
}
