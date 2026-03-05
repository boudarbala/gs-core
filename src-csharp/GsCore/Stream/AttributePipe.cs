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
/// Allows to filter the attribute event stream. <p> The filtering is based on attribute predicates. An attribute predicate is an object that you provide and that only defines one method {@link AttributePredicate#matches(String, object)}. If the "matches()" method return false, the attribute is discarded from the event stream, else it is passed to the listeners of this filter. </p> <p> You can setup a predicate from all attributes (graph, node and edge attributes) and specific predicates for graph, node and edge attributes. </p>
/// </summary>
public class AttributePipe : PipeBase {
	protected AttributePredicate globalPredicate = new FalsePredicate();

	protected AttributePredicate graphPredicate = new FalsePredicate();

	protected AttributePredicate nodePredicate = new FalsePredicate();

	protected AttributePredicate edgePredicate = new FalsePredicate();

	/// <summary>
/// Set an attribute filter for graph, node and edge attributes. If the filter is null, attributes will not be filtered globally.
/// </summary>
/// <param name="filter"> The filter to use, it can be null to disable global attribute filtering.</param>
	public void setGlobalAttributeFilter(AttributePredicate filter) {
		if (filter == null)
			globalPredicate = new FalsePredicate();
		else
			globalPredicate = filter;
	}

	/// <summary>
/// Set an attribute filter for graph attributes only (node an edge attributes are not filtered by this filter). If the filter is null, graph attributes will not be filtered.
/// </summary>
/// <param name="filter"> The filter to use, it can be null to disable graph attribute filtering.</param>
	public void setGraphAttributeFilter(AttributePredicate filter) {
		if (filter == null)
			graphPredicate = new FalsePredicate();
		else
			graphPredicate = filter;
	}

	/// <summary>
/// Set an attribute filter for node attributes only (graph an edge attributes are not filtered by this filter). If the filter is null, node attributes will not be filtered.
/// </summary>
/// <param name="filter"> The filter to use, it can be null to disable node attribute filtering.</param>
	public void setNodeAttributeFilter(AttributePredicate filter) {
		if (filter == null)
			nodePredicate = new FalsePredicate();
		else
			nodePredicate = filter;
	}

	/// <summary>
/// Set an attribute filter for edge attributes only (graph an node attributes are not filtered by this filter). If the filter is null, edge attributes will not be filtered.
/// </summary>
/// <param name="filter"> The filter to use, it can be null to disable edge attribute filtering.</param>
	public void setEdgeAttributeFilter(AttributePredicate filter) {
		if (filter == null)
			edgePredicate = new FalsePredicate();
		else
			edgePredicate = filter;
	}

	/// <summary>
/// The filter for all graph, node and edge attributes. This filter can be null.
/// </summary>
/// <returns>The global attribute filter or null if there is no global filter.</returns>
	public AttributePredicate getGlobalAttributeFilter() {
		return globalPredicate;
	}

	/// <summary>
/// The filter for all graph attributes. This filter can be null.
/// </summary>
/// <returns>The graph attribute filter or null if there is no graph filter.</returns>
	public AttributePredicate getGraphAttributeFilter() {
		return graphPredicate;
	}

	/// <summary>
/// The filter for all node attributes. This filter can be null.
/// </summary>
/// <returns>The node global attribute filter or null if there is no node filter.</returns>
	public AttributePredicate getNodeAttributeFilter() {
		return nodePredicate;
	}

	/// <summary>
/// The filter for all edge attributes. This filter can be null.
/// </summary>
/// <returns>The edge attribute filter or null of there is no edge filter.</returns>
	public AttributePredicate getEdgeAttributeFilter() {
		return edgePredicate;
	}

	// GraphListener

	
	public void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value) {
		if (!edgePredicate.matches(attribute, value)) {
			if (!globalPredicate.matches(attribute, value)) {
				sendEdgeAttributeAdded(graphId, timeId, edgeId, attribute, value);
			}
		}
	}

	
	public void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		if (!edgePredicate.matches(attribute, newValue)) {
			if (!globalPredicate.matches(attribute, newValue)) {
				sendEdgeAttributeChanged(graphId, timeId, edgeId, attribute, oldValue, newValue);
			}
		}
	}

	
	public void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute) {
		if (!edgePredicate.matches(attribute, null)) {
			if (!globalPredicate.matches(attribute, null)) {
				sendEdgeAttributeRemoved(graphId, timeId, edgeId, attribute);
			}
		}
	}

	
	public void graphAttributeAdded(string graphId, long timeId, string attribute, object value) {
		if (!graphPredicate.matches(attribute, value)) {
			if (!globalPredicate.matches(attribute, value)) {
				sendGraphAttributeAdded(graphId, timeId, attribute, value);
			}
		}
	}

	
	public void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue) {
		if (!graphPredicate.matches(attribute, newValue)) {
			if (!globalPredicate.matches(attribute, newValue)) {
				sendGraphAttributeChanged(graphId, timeId, attribute, oldValue, newValue);
			}
		}
	}

	
	public void graphAttributeRemoved(string graphId, long timeId, string attribute) {
		if (!graphPredicate.matches(attribute, null)) {
			if (!globalPredicate.matches(attribute, null)) {
				sendGraphAttributeRemoved(graphId, timeId, attribute);
			}
		}
	}

	
	public void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value) {
		if (!nodePredicate.matches(attribute, value)) {
			if (!globalPredicate.matches(attribute, value)) {
				sendNodeAttributeAdded(graphId, timeId, nodeId, attribute, value);
			}
		}
	}

	
	public void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		if (!nodePredicate.matches(attribute, newValue)) {
			if (!globalPredicate.matches(attribute, newValue)) {
				sendNodeAttributeChanged(graphId, timeId, nodeId, attribute, oldValue, newValue);
			}
		}
	}

	
	public void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute) {
		if (!nodePredicate.matches(attribute, null)) {
			if (!globalPredicate.matches(attribute, null)) {
				sendNodeAttributeRemoved(graphId, timeId, nodeId, attribute);
			}
		}
	}

	protected class FalsePredicate : AttributePredicate {
		public bool matches(string attributeName, object attributeValue) {
			return false;
		}

	}
}
}
