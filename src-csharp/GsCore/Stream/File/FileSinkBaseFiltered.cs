using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File
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
/// Base implementation for filtered graph output to files. <p> This class provides the list of possible filters which could be used by the user to write graphs into files using a specific file format. Thus, it allows to create an output stream where the dynamic events of addition/deletion/modification can be filtered. </p> <p> Since it extends FileSinkBase, you have to override the same methods in order to implement an output. </p>
/// </summary>
abstract class FileSinkBaseFiltered : FileSinkBase {
	/*
	 * List of possible filters
	 */
	protected bool noFilterGraphAttributeAdded;
	protected bool noFilterGraphAttributeChanged;
	protected bool noFilterGraphAttributeRemoved;
	protected bool noFilterNodeAttributeAdded;
	protected bool noFilterNodeAttributeChanged;
	protected bool noFilterNodeAttributeRemoved;
	protected bool noFilterNodeAdded;
	protected bool noFilterNodeRemoved;
	protected bool noFilterEdgeAttributeAdded;
	protected bool noFilterEdgeAttributeChanged;
	protected bool noFilterEdgeAttributeRemoved;
	protected bool noFilterEdgeAdded;
	protected bool noFilterEdgeRemoved;
	protected bool noFilterGraphCleared;
	protected bool noFilterStepBegins;
	protected List<string> graphAttributesFiltered;
	protected List<string> nodeAttributesFiltered;
	protected List<string> edgeAttributesFiltered;

	/// <summary>
/// Initialize with no filter
/// </summary>
	public FileSinkBaseFiltered() {
		noFilterGraphAttributeAdded = true;
		noFilterGraphAttributeChanged = true;
		noFilterGraphAttributeRemoved = true;
		noFilterNodeAttributeAdded = true;
		noFilterNodeAttributeChanged = true;
		noFilterNodeAttributeRemoved = true;
		noFilterNodeAdded = true;
		noFilterNodeRemoved = true;
		noFilterEdgeAttributeAdded = true;
		noFilterEdgeAttributeChanged = true;
		noFilterEdgeAttributeRemoved = true;
		noFilterEdgeAdded = true;
		noFilterEdgeRemoved = true;
		noFilterGraphCleared = true;
		noFilterStepBegins = true;
		graphAttributesFiltered = new List<string>();
		nodeAttributesFiltered = new List<string>();
		edgeAttributesFiltered = new List<string>();
	}

	/// <returns>the list of every node attributes filtered</returns>
	public List<string> getGraphAttributesFiltered() {
		return graphAttributesFiltered;
	}

	/// <summary>
/// Set the whole list of graph attributes filtered
/// </summary>
/// <param name="graphAttributesFiltered"> the new list</param>
	public void setGraphAttributesFiltered(List<string> graphAttributesFiltered) {
		this.graphAttributesFiltered = graphAttributesFiltered;
	}

	/// <summary>
/// Add a new attribute to filter
/// </summary>
/// <param name="attr"> the filtered attribute</param>
/// <returns>true if the attribute has been added, false otherwise</returns>
	public bool addGraphAttributeFiltered(string attr) {
		return graphAttributesFiltered.Add(attr);
	}

	/// <summary>
/// Remove an attribute to filter
/// </summary>
/// <param name="attr"> the no more filtered attribute</param>
/// <returns>true if the attribute has been removed, false otherwise</returns>
	public bool removeGraphAttributeFilter(string attr) {
		return graphAttributesFiltered.Remove(attr);
	}

	/// <returns>the list of every node attributes filtered</returns>
	public List<string> getNodeAttributesFiltered() {
		return graphAttributesFiltered;
	}

	/// <summary>
/// Set the whole list of node attributes filtered
/// </summary>
/// <param name="nodeAttributesFiltered"> the new list</param>
	public void setNodeAttributesFiltered(List<string> nodeAttributesFiltered) {
		this.nodeAttributesFiltered = nodeAttributesFiltered;
	}

	/// <summary>
/// Add a new attribute to filter
/// </summary>
/// <param name="attr"> the filtered attribute</param>
/// <returns>true if the attribute has been added, false otherwise</returns>
	public bool addNodeAttributeFiltered(string attr) {
		return nodeAttributesFiltered.Add(attr);
	}

	/// <summary>
/// Remove an attribute to filter
/// </summary>
/// <param name="attr"> the no more filtered attribute</param>
/// <returns>true if the attribute has been removed, false otherwise</returns>
	public bool removeNodeAttributeFilter(string attr) {
		return nodeAttributesFiltered.Remove(attr);
	}

	/// <returns>the list of every edge attributes filtered</returns>
	public List<string> getEdgeAttributesFiltered() {
		return edgeAttributesFiltered;
	}

	/// <summary>
/// Set the whole list of edge attributes filtered
/// </summary>
/// <param name="edgeAttributesFiltered"> the new list</param>
	public void setEdgeAttributesFiltered(List<string> edgeAttributesFiltered) {
		this.edgeAttributesFiltered = edgeAttributesFiltered;
	}

	/// <summary>
/// Add a new attribute to filter
/// </summary>
/// <param name="attr"> the filtered attribute</param>
/// <returns>true if the attribute has been added, false otherwise</returns>
	public bool addEdgeAttributeFiltered(string attr) {
		return edgeAttributesFiltered.Add(attr);
	}

	/// <summary>
/// Remove an attribute to filter
/// </summary>
/// <param name="attr"> the filtered attribute</param>
/// <returns>true if the attribute has been removed, false otherwise</returns>
	public bool removeEdgeAttributeFilter(string attr) {
		return edgeAttributesFiltered.Remove(attr);
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterGraphAttributeAdded() {
		return noFilterGraphAttributeAdded;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterGraphAttributeAdded"></param>
	public void setNoFilterGraphAttributeAdded(bool noFilterGraphAttributeAdded) {
		this.noFilterGraphAttributeAdded = noFilterGraphAttributeAdded;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterGraphAttributeChanged() {
		return noFilterGraphAttributeChanged;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterGraphAttributeChanged"></param>
	public void setNoFilterGraphAttributeChanged(bool noFilterGraphAttributeChanged) {
		this.noFilterGraphAttributeChanged = noFilterGraphAttributeChanged;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterGraphAttributeRemoved() {
		return noFilterGraphAttributeRemoved;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterGraphAttributeRemoved"></param>
	public void setNoFilterGraphAttributeRemoved(bool noFilterGraphAttributeRemoved) {
		this.noFilterGraphAttributeRemoved = noFilterGraphAttributeRemoved;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterNodeAttributeAdded() {
		return noFilterNodeAttributeAdded;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterNodeAttributeAdded"></param>
	public void setNoFilterNodeAttributeAdded(bool noFilterNodeAttributeAdded) {
		this.noFilterNodeAttributeAdded = noFilterNodeAttributeAdded;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterNodeAttributeChanged() {
		return noFilterNodeAttributeChanged;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterNodeAttributeChanged"></param>
	public void setNoFilterNodeAttributeChanged(bool noFilterNodeAttributeChanged) {
		this.noFilterNodeAttributeChanged = noFilterNodeAttributeChanged;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterNodeAttributeRemoved() {
		return noFilterNodeAttributeRemoved;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterNodeAttributeRemoved"></param>
	public void setNoFilterNodeAttributeRemoved(bool noFilterNodeAttributeRemoved) {
		this.noFilterNodeAttributeRemoved = noFilterNodeAttributeRemoved;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterNodeAdded() {
		return noFilterNodeAdded;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterNodeAdded"></param>
	public void setNoFilterNodeAdded(bool noFilterNodeAdded) {
		this.noFilterNodeAdded = noFilterNodeAdded;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterNodeRemoved() {
		return noFilterNodeRemoved;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterNodeRemoved"></param>
	public void setNoFilterNodeRemoved(bool noFilterNodeRemoved) {
		this.noFilterNodeRemoved = noFilterNodeRemoved;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterEdgeAttributeAdded() {
		return noFilterEdgeAttributeAdded;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterEdgeAttributeAdded"></param>
	public void setNoFilterEdgeAttributeAdded(bool noFilterEdgeAttributeAdded) {
		this.noFilterEdgeAttributeAdded = noFilterEdgeAttributeAdded;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterEdgeAttributeChanged() {
		return noFilterEdgeAttributeChanged;
	}

	/// <param name="noFilterEdgeAttributeChanged"></param>
	public void setNoFilterEdgeAttributeChanged(bool noFilterEdgeAttributeChanged) {
		this.noFilterEdgeAttributeChanged = noFilterEdgeAttributeChanged;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterEdgeAttributeRemoved() {
		return noFilterEdgeAttributeRemoved;
	}

	/// <param name="noFilterEdgeAttributeRemoved"></param>
	public void setNoFilterEdgeAttributeRemoved(bool noFilterEdgeAttributeRemoved) {
		this.noFilterEdgeAttributeRemoved = noFilterEdgeAttributeRemoved;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterEdgeAdded() {
		return noFilterEdgeAdded;
	}

	/// <param name="noFilterEdgeAdded"></param>
	public void setNoFilterEdgeAdded(bool noFilterEdgeAdded) {
		this.noFilterEdgeAdded = noFilterEdgeAdded;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterEdgeRemoved() {
		return noFilterEdgeRemoved;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterEdgeRemoved"></param>
	public void setNoFilterEdgeRemoved(bool noFilterEdgeRemoved) {
		this.noFilterEdgeRemoved = noFilterEdgeRemoved;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterGraphCleared() {
		return noFilterGraphCleared;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterGraphCleared"></param>
	public void setNoFilterGraphCleared(bool noFilterGraphCleared) {
		this.noFilterGraphCleared = noFilterGraphCleared;
	}

	/// <returns>true if this filter is disable, false otherwise</returns>
	public bool isNoFilterStepBegins() {
		return noFilterStepBegins;
	}

	/// <summary>
/// Disable or enable this filter
/// </summary>
/// <param name="noFilterStepBegins"></param>
	public void setNoFilterStepBegins(bool noFilterStepBegins) {
		this.noFilterStepBegins = noFilterStepBegins;
	}

}

}
