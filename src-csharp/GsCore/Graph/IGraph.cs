using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Graph
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
/// An Interface that advises general purpose methods for handling graphs. <p> <p> This interface is one of the main interfaces of GraphStream. It defines the services provided by a graph structure. Graphs implementations must at least implement this interface (but are free to provide more services). </p> <p> <p> With {@link org.graphstream.stream.Source} {@link org.graphstream.stream.Sink} and {@link org.graphstream.stream.Pipe}, this interface is one of the most important. A graph is a {@link org.graphstream.stream.Pipe} that buffers the graph events and present the graph structure as it is actually. </p> <p> <p> In other words, it allows to browse the graph structure, to explore it, to modify it, and to implement algorithms on it. This class can be seen as a snapshot of a stream of event at current time. </p> <p> <p> With factories ({@link org.graphstream.graph.NodeFactory} {@link org.graphstream.graph.EdgeFactory}), users can define their own models of nodes or edges. Problem is that when you define such model, you want to access to elements with the valid type, without cast if possible. To improve the access to elements in such cases, Graph offers implicit genericity to access nodes or edges. The following is an example of an access without genericity : <p> <pre> Graph g = ... ; g.setNodeFactory( new MyNodeFactory() ); g.addNode("root"); MyNode n = (MyNode) g.getNode("root"); for( Node o : g.getEachNode() ) { MyNode node = (MyNode) o; // Do something with node } </pre> <p> With implicit genericity offers by Graph, this can be done easier: <p> <pre> Graph g = ... ; g.setNodeFactory( new MyNodeFactory() ); g.addNode("root"); MyNode n = g.getNode("root"); for( MyNode node : g.getEachNode() ) { // Do something with node } </pre> <p> </p> <p> <p> Graph elements (nodes and edges) can be accessed using their identifier or their index. Each node / edge has a unique string identifier assigned when the element is created. Each element has an automatically maintained unique index between 0 and {@link #getNodeCount()} - 1 or {@link #getEdgeCount()} - 1. When a new element is added, its index is <code>getNodeCount() - 1</code> or <code>getEdgeCount() - 1</code>. When an element is removed, the element with the biggest index takes its place. Unlike identifiers, indices can change when the graph is modified, but they are always successive. A loop of the form <p> <pre> for (int i = 0; i &lt; g.getNodeCount(); i++) { Node node = g.getNode(i); // Do something with node } </pre> <p> will always iterate on all the nodes of <code>g</code>. </p>
/// </summary>
public interface IGraph : IElement, IPipe, IEnumerable<INode>, IStructure {
	// Access

	/// <summary>
/// Get a node by its identifier. This method is implicitly generic and returns something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedNode node = graph.getNode(&quot;...&quot;); </pre> <p> the method will return an ExtendedNode node. If no left part exists, method will just return a Node.
/// </summary>
/// <param name="id"> Identifier of the node to find.</param>
/// <returns>The searched node or null if not found.</returns>
	INode getNode(string id);

	/// <summary>
/// Get an edge by its identifier. This method is implicitly generic and returns something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedEdge edge = graph.getEdge(&quot;...&quot;); </pre> <p> the method will return an ExtendedEdge edge. If no left part exists, method will just return an Edge.
/// </summary>
/// <param name="id"> Identifier of the edge to find.</param>
/// <returns>The searched edge or null if not found.</returns>
	IEdge getEdge(string id);

	/// <summary>
/// The factory used to create node instances. The factory can be changed to refine the node class generated for this graph.
/// </summary>
	INodeFactory<INode> nodeFactory();

	/// <summary>
/// The factory used to create edge instances. The factory can be changed to refine the edge class generated for this graph.
/// </summary>
	IEdgeFactory<IEdge> edgeFactory();

	/// <summary>
/// Is strict checking enabled? If strict checking is enabled the graph checks for name space conflicts (e.g. insertion of two nodes with the same name), removal of non-existing elements, use of non existing elements (create an edge between two non existing nodes). Graph implementations are free to respect strict checking or not.
/// </summary>
/// <returns>True if enabled.</returns>
	bool isStrict();

	/// <summary>
/// Is the automatic creation of missing elements enabled?. If strict checking is disabled and auto-creation is enabled, when an edge is created and one or two of its nodes are not already present in the graph, the nodes are automatically created.
/// </summary>
/// <returns>True if enabled.</returns>
	bool isAutoCreationEnabled();

	/// <summary>
/// The current step.
/// </summary>
/// <returns>The step.</returns>
	double getStep();

	// Command

	/// <summary>
/// Set the node factory used to create nodes.
/// </summary>
/// <param name="nf"> the new NodeFactory</param>
	void setNodeFactory(INodeFactory<INode> nf);

	/// <summary>
/// Set the edge factory used to create edges.
/// </summary>
/// <param name="ef"> the new EdgeFactory</param>
	void setEdgeFactory(IEdgeFactory<IEdge> ef);

	/// <summary>
/// Enable or disable strict checking.
/// </summary>
/// <param name="on"> True or false.</param>
	void setStrict(bool on);

	/// <summary>
/// Enable or disable the automatic creation of missing elements.
/// </summary>
/// <param name="on"> True or false.</param>
	void setAutoCreate(bool on);

	// Graph construction

	/// <summary>
/// Empty the graph completely by removing any references to nodes or edges. Every attribute is also removed. However, listeners are kept.
/// </summary>
	void clear();

	/// <summary>
/// Add a node in the graph. <p> This acts as a factory, creating the node instance automatically (and eventually using the node factory provided). An event is generated toward the listeners. If strict checking is enabled, and a node already exists with this identifier, an {@link org.graphstream.graph.IdAlreadyInUseException} is raised. Else the error is silently ignored and the already existing node is returned. </p> <p> This method is implicitly generic and returns something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedNode n = graph.addNode(&quot;...&quot;); </pre> <p> the method will return an ExtendedNode. If no left part exists, method will just return a Node. </p> If strict checking is enabled the identifier is already used.
/// </summary>
/// <param name="id"> Arbitrary and unique string identifying the node.</param>
/// <returns>The created node (or the already existing node).</returns>
	INode addNode(string id);

	/// <summary>
/// Adds an undirected edge between nodes. <p> <p> The behavior of this method depends on many conditions. It can be summarized as follows. </p> <p> <p> First of all, the method checks if the graph already contains an edge with the same id. If this is the case and strict checking is enabled {@code IdAlreadyInUseException} is thrown. If the strict checking is disabled the method returns a reference to the existing edge if it has endpoints {@code node1} and {@code node2} (in the same order if the edge is directed) or {@code null} otherwise. </p> <p> <p> In the case when the graph does not contain an edge with the same id, the method checks if {@code node1} and {@code node2} exist. If one or both of them do not exist, and strict checking is enabled {@code ElementNotFoundException} is thrown. Otherwise if auto-creation is disabled, the method returns {@code null}. If auto-creation is enabled, the method creates the missing endpoints. <p> <p> When the edge id is not already in use and the both endpoints exist (or created), the edge can still be rejected. It may happen for example when it connects two already connected nodes in a single graph. If the edge is rejected, the method throws {@code EdgeRejectedException} if strict checking is enabled or returns {@code null} otherwise. Finally, if the edge is accepted, it is created using the corresponding edge factory and a reference to it is returned. <p> <p> An edge creation event is sent toward the listeners. If new nodes are created, the corresponding events are also sent to the listeners. </p> <p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedEdge e = graph.addEdge(&quot;...&quot;, &quot;...&quot;, &quot;...&quot;); </pre> <p> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p> If an edge with the same id already exists and strict checking is enabled. If strict checking is enabled, and 'node1' or 'node2' are not registered in the graph. If strict checking is enabled and the edge is not accepted.
/// </summary>
/// <param name="id"> Unique and arbitrary string identifying the edge.</param>
/// <param name="node1"> The first node identifier.</param>
/// <param name="node2"> The second node identifier.</param>
/// <returns>The newly created edge, an existing edge or {@code null} (see the detailed description above)</returns>
	IEdge addEdge(string id, string node1, string node2){
		return addEdge(id, node1, node2, false);
	}

	/// <summary>
/// Like {@link #addEdge(String, String, String)}, but this edge can be directed between the two given nodes. If directed, the edge goes in the 'from' -&gt; 'to' direction. An event is sent toward the listeners. If an edge with the same id already exists and strict checking is enabled. If strict checking is enabled, and 'node1' or 'node2' are not registered in the graph. If strict checking is enabled and the edge is not accepted.
/// </summary>
/// <param name="id"> Unique and arbitrary string identifying the edge.</param>
/// <param name="from"> The first node identifier.</param>
/// <param name="to"> The second node identifier.</param>
/// <param name="directed"> Is the edge directed?</param>
/// <returns>The newly created edge, an existing edge or {@code null} (see the detailed description in {@link #addEdge(String, String, String)})</returns>
	IEdge addEdge(string id, string from, string to, bool directed){
		INode src = getNode(from);
		INode dst = getNode(to);

		if (src == null || dst == null) {
			if (isStrict())
				throw new ElementNotFoundException("INode '%s'", src == null ? from : to);

			if (!isAutoCreationEnabled())
				return null;

			if (src == null)
				src = addNode(from);

			if (dst == null)
				dst = addNode(to);
		}

		return addEdge(id, src, dst, directed);
	}

	/// <summary>
/// <p> Since dynamic graphs are based on discrete event modifications, the notion of step is defined to simulate elapsed time between events. So a step is a event that occurs in the graph, it does not modify it but it gives a kind of timestamp that allows the tracking of the progress of the graph over the time. </p> <p> This kind of event is useful for dynamic algorithms that listen to the dynamic graph and need to measure the time in the graph's evolution. </p>
/// </summary>
/// <param name="time"> A numerical value that may give a timestamp to track the evolution of the graph over the time.</param>
	void stepBegins(double time);

	// Source
	// XXX do we put the iterable attributeSinks and elementSinks in Source ?

	/// <summary>
/// Returns an "iterable" of {@link AttributeSink} objects registered to this graph.
/// </summary>
/// <returns>the set of {@link AttributeSink} under the form of an iterable object.</returns>
	IEnumerable<IAttributeSink> attributeSinks();

	/// <summary>
/// Returns an "iterable" of {@link ElementSink} objects registered to this graph.
/// </summary>
/// <returns>the list of {@link ElementSink} under the form of an iterable object.</returns>
	IEnumerable<IElementSink> elementSinks();

	// Utility shortcuts (should be mixins or traits, what are you doing Mr Java
	// ?)
	// XXX use a Readable/Writable/Displayable interface for this ?

	/// <summary>
/// Utility method to read a graph. This method tries to identify the graph format by itself and instantiates the corresponding reader automatically. If this process fails, a NotFoundException is raised. If the file cannot be found or if the format is not recognized. If there is a parsing error while reading the file. If an input output error occurs during the graph reading.
/// </summary>
/// <param name="filename"> The graph filename (or URL).</param>
	void read(string filename);

	/// <summary>
/// Utility method to read a graph using the given reader. If the file cannot be found or if the format is not recognised. If there is a parsing error while reading the file. If an input/output error occurs during the graph reading.
/// </summary>
/// <param name="input"> An appropriate reader for the filename.</param>
/// <param name="filename"> The graph filename (or URL).</param>
	void read(IFileSource input, string filename);

	/// <summary>
/// Utility method to write a graph in DGS format to a file. If an input/output error occurs during the graph writing.
/// </summary>
/// <param name="filename"> The file that will contain the saved graph (or URL).</param>
	void write(string filename);

	/// <summary>
/// Utility method to write a graph in the chosen format to a file. If an input/output error occurs during the graph writing.
/// </summary>
/// <param name="filename"> The file that will contain the saved graph (or URL).</param>
/// <param name="output"> The output format to use.</param>
	void write(IFileSink output, string filename);

	/// <summary>
/// Utility method that creates a new graph viewer, and register the graph in it. Notice that this method is a quick way to see a graph, and only this. It can be used to prototype a program, but may be limited. This method automatically launch a graph layout algorithm in its own thread to compute best node positions.
/// </summary>
/// <returns>a graph viewer that allows to command the viewer (it often run in another thread).</returns>
	IViewer display();

	/// <summary>
/// Utility method that creates a new graph viewer, and register the graph in it. Notice that this method is a quick way to see a graph, and only this. It can be used to prototype a program, but is very limited.
/// </summary>
/// <param name="autoLayout"> If true a layout algorithm is launched in its own thread to compute best node positions.</param>
/// <returns>a graph viewer that allows to command the viewer (it often run in another thread).</returns>
	IViewer display(bool autoLayout);

	// New methods

	/// <summary>
/// Get a node by its index. This method is implicitly generic and returns something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedNode node = graph.getNode(index); </pre> <p> the method will return an ExtendedNode node. If no left part exists, method will just return a Node. If the index is negative or greater than {@code getNodeCount() - 1}.
/// </summary>
/// <param name="index"> Index of the node to find.</param>
/// <returns>The node with the given index</returns>
	INode getNode(int index);

	/// <summary>
/// Get an edge by its index. This method is implicitly generic and returns something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedEdge edge = graph.getEdge(index); </pre> <p> the method will return an ExtendedEdge edge. If no left part exists, method will just return an Edge. if the index is less than 0 or greater than {@code getNodeCount() - 1}.
/// </summary>
/// <param name="index"> The index of the edge to find.</param>
/// <returns>The edge with the given index</returns>
	IEdge getEdge(int index);

	/// <summary>
/// Like {@link #addEdge(String, String, String)} but the nodes are identified by their indices. If node indices are negative or greater than {@code getNodeCount() - 1} If an edge with the same id already exists and strict checking is enabled. If strict checking is enabled and the edge is not accepted.
/// </summary>
/// <param name="id"> Unique and arbitrary string identifying the edge.</param>
/// <param name="index1"> The first node index</param>
/// <param name="index2"> The second node index</param>
/// <returns>The newly created edge, an existing edge or {@code null}</returns>
	IEdge addEdge(string id, int index1, int index2){
		return addEdge(id, getNode(index1), getNode(index2), false);
	}

	/// <summary>
/// Like {@link #addEdge(String, String, String, boolean)} but the nodes are identified by their indices. If node indices are negative or greater than {@code getNodeCount() - 1} If an edge with the same id already exists and strict checking is enabled. If strict checking is enabled and the edge is not accepted.
/// </summary>
/// <param name="id"> Unique and arbitrary string identifying the edge.</param>
/// <param name="toIndex"> The first node index</param>
/// <param name="fromIndex"> The second node index</param>
/// <param name="directed"> Is the edge directed?</param>
/// <returns>The newly created edge, an existing edge or {@code null}</returns>
	IEdge addEdge(string id, int fromIndex, int toIndex, bool directed){
		return addEdge(id, getNode(fromIndex), getNode(toIndex), directed);
	}

	/// <summary>
/// Like {@link #addEdge(String, String, String)} but the node references are given instead of node identifiers. If an edge with the same id already exists and strict checking is enabled. If strict checking is enabled and the edge is not accepted.
/// </summary>
/// <param name="id"> Unique and arbitrary string identifying the edge.</param>
/// <param name="node1"> The first node</param>
/// <param name="node2"> The second node</param>
/// <returns>The newly created edge, an existing edge or {@code null}</returns>
	IEdge addEdge(string id, INode node1, INode node2);

	/// <summary>
/// Like {@link #addEdge(String, String, String, boolean)} but the node references are given instead of node identifiers. If an edge with the same id already exists and strict checking is enabled. If strict checking is enabled and the edge is not accepted.
/// </summary>
/// <param name="id"> Unique and arbitrary string identifying the edge.</param>
/// <param name="from"> The first node</param>
/// <param name="to"> The second node</param>
/// <param name="directed"> Is the edge directed?</param>
/// <returns>The newly created edge, an existing edge or {@code null}</returns>
	IEdge addEdge(string id, INode from, INode to, bool directed);

	/// <summary>
/// Removes an edge with a given index. An event is sent toward the listeners. <p> <p> This method is implicitly generic and returns something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedEdge edge = graph.removeEdge(i); </pre> <p> the method will return an ExtendedEdge edge. If no left part exists, method will just return an Edge. </p> if the index is negative or greater than {@code getEdgeCount() - 1}
/// </summary>
/// <param name="index"> The index of the edge to be removed.</param>
/// <returns>The removed edge</returns>
	IEdge removeEdge(int index);

	/// <summary>
/// Removes an edge between two nodes. Like {@link #removeEdge(String, String)} but the nodes are identified by their indices. If one of the node indices is negative or greater than {@code getNodeCount() - 1}. if strict checking is enabled and there is no edge between the two nodes.
/// </summary>
/// <param name="fromIndex"> the index of the source node</param>
/// <param name="toIndex"> the index of the target node</param>
/// <returns>the removed edge or {@code null} if no edge is removed</returns>
	IEdge removeEdge(int fromIndex, int toIndex);

	/// <summary>
/// Removes an edge between two nodes. Like {@link #removeEdge(String, String)} but node references are given instead of node identifiers. if strict checking is enabled and there is no edge between the two nodes.
/// </summary>
/// <param name="node1"> the first node</param>
/// <param name="node2"> the second node</param>
/// <returns>the removed edge or {@code null} if no edge is removed</returns>
	IEdge removeEdge(INode node1, INode node2);

	/// <summary>
/// Remove an edge given the identifiers of its two endpoints. <p> If the edge is directed it is removed only if its source and destination nodes are identified by 'from' and 'to' respectively. If the graph is a multi-graph and there are several edges between the two nodes, one of the edges at random is removed. An event is sent toward the listeners. If strict checking is enabled and at least one of the two given nodes does not exist or if they are not connected, a not found exception is raised. Else the error is silently ignored, and null is returned. </p> <p> This method is implicitly generic and return something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedEdge e = graph.removeEdge(&quot;...&quot;, &quot;...&quot;); </pre> <p> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p> If the 'from' or 'to' node is not registered in the graph or not connected and strict checking is enabled.
/// </summary>
/// <param name="from"> The origin node identifier to select the edge.</param>
/// <param name="to"> The destination node identifier to select the edge.</param>
/// <returns>The removed edge, or null if strict checking is disabled and at least one of the two given nodes does not exist or there is no edge between them</returns>
	IEdge removeEdge(string from, string to);

	/// <summary>
/// Removes an edge knowing its identifier. An event is sent toward the listeners. If strict checking is enabled and the edge does not exist {@code ElementNotFoundException} is raised. Otherwise the error is silently ignored and null is returned. <p> This method is implicitly generic and returns something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedEdge e = graph.removeEdge(&quot;...&quot;); </pre> <p> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p> If no edge matches the identifier and strict checking is enabled.
/// </summary>
/// <param name="id"> Identifier of the edge to remove.</param>
/// <returns>The removed edge, or null if strict checking is disabled and the edge does not exist.</returns>
	IEdge removeEdge(string id);

	/// <summary>
/// Removes an edge. An event is sent toward the listeners. <p> This method is implicitly generic and returns something which extends Edge. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedEdge e = graph.removeEdge(...); </pre> <p> the method will return an ExtendedEdge. If no left part exists, method will just return an Edge. </p>
/// </summary>
/// <param name="edge"> The edge to be removed</param>
/// <returns>The removed edge</returns>
	IEdge removeEdge(IEdge edge);

	/// <summary>
/// Removes a node with a given index. <p> An event is generated toward the listeners. Note that removing a node may remove all edges it is connected to. In this case corresponding events will also be generated toward the listeners. </p> <p> This method is implicitly generic and return something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedNode n = graph.removeNode(index); </pre> <p> the method will return an ExtendedNode. If no left part exists, method will just return a Node. </p> if the index is negative or greater than {@code getNodeCount() - 1}.
/// </summary>
/// <param name="index"> The index of the node to be removed</param>
/// <returns>The removed node</returns>
	INode removeNode(int index);

	/// <summary>
/// Remove a node using its identifier. <p> An event is generated toward the listeners. Note that removing a node may remove all edges it is connected to. In this case corresponding events will also be generated toward the listeners. </p> <p> This method is implicitly generic and return something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedNode n = graph.removeNode(&quot;...&quot;); </pre> <p> the method will return an ExtendedNode. If no left part exists, method will just return a Node. </p> If no node matches the given identifier and strict checking is enabled.
/// </summary>
/// <param name="id"> The unique identifier of the node to remove.</param>
/// <returns>The removed node. If strict checking is disabled, it can return null if the node to remove does not exist.</returns>
	INode removeNode(string id);

	/// <summary>
/// Removes a node. <p> An event is generated toward the listeners. Note that removing a node may remove all edges it is connected to. In this case corresponding events will also be generated toward the listeners. </p> <p> This method is implicitly generic and return something which extends Node. The return type is the one of the left part of the assignment. For example, in the following call : <p> <pre> ExtendedNode n = graph.removeNode(...); </pre> <p> the method will return an ExtendedNode. If no left part exists, method will just return a Node. </p>
/// </summary>
/// <param name="node"> The node to be removed</param>
/// <returns>The removed node</returns>
	INode removeNode(INode node);

}
}
