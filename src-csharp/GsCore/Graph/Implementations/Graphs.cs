using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace Org.GraphStream.Graph.Implementations
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


public class Graphs {

	private static readonly object /* Logger */ logger = null /* Logger */;

	public static IGraph unmutableGraph(IGraph g) {
		return null;
	}

	/// <summary>
/// Synchronizes a graph. The returned graph can be accessed and modified by several threads. You lose genericity in methods returning edge or node because each element (graph, nodes and edges) is wrapped into a wrapper which breaks original elements class.
/// </summary>
/// <param name="g"> the graph to synchronize</param>
/// <returns>a wrapper for g</returns>
	public static IGraph synchronizedGraph(IGraph g) {
		return new SynchronizedGraph(g);
	}

	/// <summary>
/// Merge several graphs in one. A new graph is created, that will contain the result. The method will try to create a graph of the same class that the first graph to merge (it needs to have a constructor with a String). Else, a MultiGraph is used.
/// </summary>
/// <param name="graphs"> graphs to merge</param>
/// <returns>merge result</returns>
	public static IGraph merge(params Graph[] graphs) {
		if (graphs == null)
			return new DefaultGraph("void-merge");

		string id = "merge";

		foreach (IGraph g in graphs)
			id += "-" + g.getId();

		IGraph result;

		try {
			Type cls = graphs[0].GetType();
			result = cls.getConstructor(typeof(string)).newInstance(id);
		} catch (Exception e) {
			Console.Error.WriteLine(string.Format("Cannot create a graph of {0}.", graphs[0].GetType().Name));
			result = new MultiGraph(id);
		}

		mergeIn(result, graphs);

		return result;
	}

	/// <summary>
/// Merge several graphs in one. The first parameter is the graph in which the other graphs will be merged.
/// </summary>
/// <param name="result"> destination graph.</param>
/// <param name="graphs"> all graphs that will be merged in result.</param>
	public static void mergeIn(IGraph result, params Graph[] graphs) {
		bool strict = result.isStrict();
		GraphReplay replay = new GraphReplay(string.Format("replay-{0}", (DateTime.UtcNow.Ticks * 100L)));

		replay.addSink(result);
		result.setStrict(false);

		if (graphs != null)
			foreach (IGraph g in graphs)
				replay.replay(g);

		replay.removeSink(result);
		result.setStrict(strict);
	}

	/// <summary>
/// Clone a given graph with same node/edge structure and same attributes.
/// </summary>
/// <param name="g"> the graph to clone</param>
/// <returns>a copy of g</returns>
	public static IGraph clone(IGraph g) {
		IGraph copy;

		try {
			Type cls = g.GetType();
			copy = cls.getConstructor(typeof(string)).newInstance(g.getId());
		} catch (Exception e) {
			Console.Error.WriteLine(string.Format("Cannot create a graph of {0}.", g.GetType().Name));
			copy = new AdjacencyListGraph(g.getId());
		}

		copyAttributes(g, copy);

		for (int i = 0; i < g.getNodeCount(); i++) {
			INode source = g.getNode(i);
			INode target = copy.addNode(source.getId());

			copyAttributes(source, target);
		}

		for (int i = 0; i < g.getEdgeCount(); i++) {
			IEdge source = g.getEdge(i);
			IEdge target = copy.addEdge(source.getId(), source.getSourceNode().getId(), source.getTargetNode().getId(),
					source.isDirected());

			copyAttributes(source, target);
		}

		return copy;
	}

	/// <param name="source"></param>
/// <param name="target"></param>
	public static void copyAttributes(IElement source, IElement target) {
		source.attributeKeys().ToList().ForEach(key => {
			object value = source.getAttribute(key);
			value = checkedArrayOrCollectionCopy(value);

			target.setAttribute(key, value);
		});
	}

	
	private static object checkedArrayOrCollectionCopy(object o) {
		if (o == null)
			return null;

		if (o.GetType().IsArray) {

			object c = Array.newInstance(o.GetType().getComponentType(), Array.getLength(o));

			for (int i = 0; i < Array.getLength(o); i++) {
				object t = checkedArrayOrCollectionCopy(Array[o, i]);
				Array.set(c, i, t);
			}

			return c;
		}

		if (typeof(Collection).isAssignableFrom(o.GetType())) {
			ICollection<object> t;

			try {
				t = (ICollection<object>) o.GetType().newInstance();
				t.AddRange((Collection) o);

				return t;
			} catch (Exception e) {
				Console.Error.WriteLine(e);
			}
		}

		return o;
	}

	class SynchronizedElement<U> : IElement where U : IElement {

		private static readonly object attributeLock = new object();  // Static to lock the attributes from different sources (graph/node/edge). Fix issue #293
		protected U wrappedElement;

		SynchronizedElement(U e) {
			this.wrappedElement = e;
		}

		public void setAttribute(string attribute, params object[] values) {
			System.Threading.Monitor.Enter(attributeLock);

			try {
				wrappedElement.setAttribute(attribute, values);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}
		}

		public void setAttributes(Dictionary<string, object> attributes) {
			System.Threading.Monitor.Enter(attributeLock);

			try {
				wrappedElement.setAttributes(attributes);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}
		}

		public void clearAttributes() {
			System.Threading.Monitor.Enter(attributeLock);

			try {
				wrappedElement.clearAttributes();
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}
		}

		public object[] getArray(string key) {
			object[] o;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				o = wrappedElement.getArray(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return o;
		}

		public object getAttribute(string key) {
			object o;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				o = wrappedElement.getAttribute(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return o;
		}

		public T getAttribute<T>(string key, Type clazz) {
			T o;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				o = wrappedElement.getAttribute(key, clazz);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return o;
		}

		public int getAttributeCount() {
			int c;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				c = wrappedElement.getAttributeCount();
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return c;
		}

		
		public IEnumerable<string> attributeKeys() {
			IEnumerable<string> s = null;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				s = wrappedElement.attributeKeys();

				if (!s.spliterator().hasCharacteristics(Spliterator.CONCURRENT))
					s = s.ToList();
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return s;
		}

		public object getFirstAttributeOf(params string[] keys) {
			object o;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				o = wrappedElement.getFirstAttributeOf(keys);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return o;
		}

		public T getFirstAttributeOf<T>(Type clazz, params string[] keys) {
			T o;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				o = wrappedElement.getFirstAttributeOf(clazz, keys);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return o;
		}

		public Dictionary<object, object> getMap(string key) {
			Dictionary<object, object> o;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				o = wrappedElement.getMap(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return o;
		}

		public string getId() {
			return wrappedElement.getId();
		}

		public int getIndex() {
			return wrappedElement.getIndex();
		}

		public string getLabel(string key) {
			string o;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				o = wrappedElement.getLabel(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return o;
		}

		public double getNumber(string key) {
			double o;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				o = wrappedElement.getNumber(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return o;
		}

		public List<IConvertible> getVector(string key) {
			List<IConvertible> o;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				o = wrappedElement.getVector(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return o;
		}

		public bool hasArray(string key) {
			bool b;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				b = wrappedElement.hasArray(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return b;
		}

		public bool hasAttribute(string key) {
			bool b;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				b = wrappedElement.hasAttribute(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return b;
		}

		public bool hasAttribute(string key, Type clazz) {
			bool b;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				b = wrappedElement.hasAttribute(key, clazz);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return b;
		}

		public bool hasMap(string key) {
			bool b;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				b = wrappedElement.hasMap(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return b;
		}

		public bool hasLabel(string key) {
			bool b;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				b = wrappedElement.hasLabel(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return b;
		}

		public bool hasNumber(string key) {
			bool b;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				b = wrappedElement.hasNumber(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return b;
		}

		public bool hasVector(string key) {
			bool b;

			System.Threading.Monitor.Enter(attributeLock);

			try {
				b = wrappedElement.hasVector(key);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}

			return b;
		}

		public void removeAttribute(string attribute) {
			System.Threading.Monitor.Enter(attributeLock);

			try {
				wrappedElement.removeAttribute(attribute);
			} finally {
				System.Threading.Monitor.Exit(attributeLock);
			}
		}
	}

	class SynchronizedGraph : SynchronizedElement<IGraph>, IGraph {

		object elementLock;
		Dictionary<string, INode> synchronizedNodes;
		Dictionary<string, IEdge> synchronizedEdges;

		SynchronizedGraph(IGraph g) {
			base(g);

			elementLock = new object();

			synchronizedNodes = g.nodes().collect(Collectors.toMap(INode::getId, n => new SynchronizedNode(this, n)));
			synchronizedEdges = g.edges().collect(Collectors.toMap(IEdge::getId, e => new SynchronizedEdge(this, e)));
		}

		
		public IEnumerable<INode> nodes() {
			ICollection<INode> nodes;

			System.Threading.Monitor.Enter(elementLock);

			try {
				nodes = new Vector<object>(((synchronizedNodes[])Enum.GetValues(typeof(synchronizedNodes))));
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return nodes;
		}

		
		public IEnumerable<IEdge> edges() {
			ICollection<IEdge> edges;

			System.Threading.Monitor.Enter(elementLock);

			try {
				edges = new Vector<object>(((synchronizedEdges[])Enum.GetValues(typeof(synchronizedEdges))));
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return edges;
		}

		
		public IEdge addEdge(string id, string node1, string node2){
			IEdge e;
			IEdge se;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.addEdge(id, node1, node2);
				se = new SynchronizedEdge(this, e);
				synchronizedEdges[id] = se;
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public IEdge addEdge(string id, string from, string to, bool directed){
			IEdge e;
			IEdge se;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.addEdge(id, from, to, directed);
				se = new SynchronizedEdge(this, e);
				synchronizedEdges[id] = se;
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public IEdge addEdge(string id, int index1, int index2) {
			IEdge e;
			IEdge se;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.addEdge(id, index1, index2);
				se = new SynchronizedEdge(this, e);
				synchronizedEdges[id] = se;
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public IEdge addEdge(string id, int fromIndex, int toIndex, bool directed) {
			IEdge e;
			IEdge se;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.addEdge(id, fromIndex, toIndex, directed);
				se = new SynchronizedEdge(this, e);
				synchronizedEdges[id] = se;
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		
		public IEdge addEdge(string id, INode node1, INode node2) {
			IEdge e;
			IEdge se;
			INode unsyncNode1, unsyncNode2;

			unsyncNode1 = ((SynchronizedElement<INode>) node1).wrappedElement;
			unsyncNode2 = ((SynchronizedElement<INode>) node2).wrappedElement;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.addEdge(id, unsyncNode1, unsyncNode2);
				se = new SynchronizedEdge(this, e);
				synchronizedEdges[id] = se;
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		
		public IEdge addEdge(string id, INode from, INode to, bool directed) {
			IEdge e;
			IEdge se;
			INode unsyncFrom, unsyncTo;

			unsyncFrom = ((SynchronizedElement<INode>) from).wrappedElement;
			unsyncTo = ((SynchronizedElement<INode>) to).wrappedElement;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.addEdge(id, unsyncFrom, unsyncTo, directed);
				se = new SynchronizedEdge(this, e);
				synchronizedEdges[id] = se;
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public INode addNode(string id){
			INode n;
			INode sn;

			System.Threading.Monitor.Enter(elementLock);

			try {
				n = wrappedElement.addNode(id);
				sn = new SynchronizedNode(this, n);
				synchronizedNodes[id] = sn;
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return sn;
		}

		
		public IEnumerable<IAttributeSink> attributeSinks() {
			List<IAttributeSink> sinks = new List<IAttributeSink>();

			System.Threading.Monitor.Enter(elementLock);

			try {
				foreach (IAttributeSink as in wrappedElement.attributeSinks())
					sinks.Add(as);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return sinks;
		}

		
		public void clear() {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.Clear();
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public IEdgeFactory<IEdge> edgeFactory() {
			return wrappedElement.edgeFactory();
		}

		
		public IEnumerable<IElementSink> elementSinks() {
			List<IElementSink> sinks = new List<IElementSink>();

			System.Threading.Monitor.Enter(elementLock);

			try {
				foreach (IElementSink es in wrappedElement.elementSinks())
					sinks.Add(es);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return sinks;
		}

		
		public IEdge getEdge(string id) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = synchronizedEdges[id];
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdge(int index){
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.getEdge(index);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e == null ? null : getEdge(e.getId());
		}

		
		public int getEdgeCount() {
			int c;

			System.Threading.Monitor.Enter(elementLock);

			try {
				c = synchronizedEdges.Count;
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return c;
		}

		
		public INode getNode(string id) {
			INode n;

			System.Threading.Monitor.Enter(elementLock);

			try {
				n = synchronizedNodes[id];
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return n;
		}

		
		public INode getNode(int index){
			INode n;

			System.Threading.Monitor.Enter(elementLock);

			try {
				n = wrappedElement.getNode(index);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return n == null ? null : getNode(n.getId());
		}

		
		public int getNodeCount() {
			int c;

			System.Threading.Monitor.Enter(elementLock);

			try {
				c = synchronizedNodes.Count;
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return c;
		}

		
		public double getStep() {
			double s;

			System.Threading.Monitor.Enter(elementLock);

			try {
				s = wrappedElement.getStep();
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return s;
		}

		
		public bool isAutoCreationEnabled() {
			return wrappedElement.isAutoCreationEnabled();
		}

		public IViewer display() {
			return wrappedElement.display();
		}

		public IViewer display(bool autoLayout) {
			return wrappedElement.display(autoLayout);
		}

		
		public bool isStrict() {
			return wrappedElement.isStrict();
		}

		
		public INodeFactory<INode> nodeFactory() {
			return wrappedElement.nodeFactory();
		}

		
		public void read(string filename){
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.Read(filename);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void read(IFileSource input, string filename){
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.Read(input, filename);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public IEdge removeEdge(string from, string to){
			IEdge e;
			IEdge se;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.removeEdge(from, to);
				se = synchronizedEdges.Remove(e.getId());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public IEdge removeEdge(string id){
			IEdge e;
			IEdge se;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.removeEdge(id);
				se = synchronizedEdges.Remove(e.getId());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public IEdge removeEdge(int index) {
			IEdge e;
			IEdge se;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.removeEdge(index);
				se = synchronizedEdges.Remove(e.getId());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public IEdge removeEdge(int fromIndex, int toIndex) {
			IEdge e;
			IEdge se;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.removeEdge(fromIndex, toIndex);
				se = synchronizedEdges.Remove(e.getId());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public IEdge removeEdge(INode node1, INode node2) {
			IEdge e;
			IEdge se;

			if (node1 is SynchronizedNode)
				node1 = ((SynchronizedNode) node1).wrappedElement;

			if (node2 is SynchronizedNode)
				node2 = ((SynchronizedNode) node1).wrappedElement;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.removeEdge(node1, node2);
				se = synchronizedEdges.Remove(e.getId());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public IEdge removeEdge(IEdge edge) {
			IEdge e;
			IEdge se;

			if (edge is SynchronizedEdge)
				edge = ((SynchronizedEdge) edge).wrappedElement;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = wrappedElement.removeEdge(edge);
				se = synchronizedEdges.Remove(e.getId());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return se;
		}

		
		public INode removeNode(string id){
			INode n;
			INode sn;

			System.Threading.Monitor.Enter(elementLock);

			try {
				n = wrappedElement.removeNode(id);
				sn = synchronizedNodes.Remove(n.getId());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return sn;
		}

		
		public INode removeNode(int index) {
			INode n;
			INode sn;

			System.Threading.Monitor.Enter(elementLock);

			try {
				n = wrappedElement.removeNode(index);
				sn = synchronizedNodes.Remove(n.getId());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return sn;
		}

		
		public INode removeNode(INode node) {
			INode n;
			INode sn;

			if (node is SynchronizedNode)
				node = ((SynchronizedNode) node).wrappedElement;

			System.Threading.Monitor.Enter(elementLock);

			try {
				n = wrappedElement.removeNode(node);
				sn = synchronizedNodes.Remove(n.getId());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return sn;
		}

		
		public void setAutoCreate(bool on) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.setAutoCreate(on);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void setEdgeFactory(IEdgeFactory<IEdge> ef) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.setEdgeFactory(ef);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void setNodeFactory(INodeFactory<INode> nf) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.setNodeFactory(nf);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void setStrict(bool on) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.setStrict(on);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void stepBegins(double time) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.stepBegins(time);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void write(string filename){
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.Write(filename);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void write(IFileSink output, string filename){
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.Write(output, filename);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void addAttributeSink(IAttributeSink sink) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.addAttributeSink(sink);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void addElementSink(IElementSink sink) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.addElementSink(sink);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void addSink(ISink sink) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.addSink(sink);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void clearAttributeSinks() {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.clearAttributeSinks();
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void clearElementSinks() {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.clearElementSinks();
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void clearSinks() {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.clearSinks();
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void removeAttributeSink(IAttributeSink sink) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.removeAttributeSink(sink);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void removeElementSink(IElementSink sink) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.removeElementSink(sink);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void removeSink(ISink sink) {
			System.Threading.Monitor.Enter(elementLock);

			try {
				wrappedElement.removeSink(sink);
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}
		}

		
		public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
			wrappedElement.edgeAttributeAdded(sourceId, timeId, edgeId, attribute, value);
		}

		
		public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
				object newValue) {
			wrappedElement.edgeAttributeChanged(sourceId, timeId, edgeId, attribute, oldValue, newValue);
		}

		
		public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
			wrappedElement.edgeAttributeRemoved(sourceId, timeId, edgeId, attribute);
		}

		
		public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
			wrappedElement.graphAttributeAdded(sourceId, timeId, attribute, value);
		}

		
		public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
				object newValue) {
			wrappedElement.graphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
		}

		
		public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
			wrappedElement.graphAttributeRemoved(sourceId, timeId, attribute);
		}

		
		public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
			wrappedElement.nodeAttributeAdded(sourceId, timeId, nodeId, attribute, value);
		}

		
		public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
				object newValue) {
			wrappedElement.nodeAttributeChanged(sourceId, timeId, nodeId, attribute, oldValue, newValue);
		}

		
		public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
			wrappedElement.nodeAttributeRemoved(sourceId, timeId, nodeId, attribute);
		}

		
		public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
				bool directed) {
			wrappedElement.edgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);
		}

		
		public void edgeRemoved(string sourceId, long timeId, string edgeId) {
			wrappedElement.edgeRemoved(sourceId, timeId, edgeId);
		}

		
		public void graphCleared(string sourceId, long timeId) {
			wrappedElement.graphCleared(sourceId, timeId);
		}

		
		public void nodeAdded(string sourceId, long timeId, string nodeId) {
			wrappedElement.nodeAdded(sourceId, timeId, nodeId);
		}

		
		public void nodeRemoved(string sourceId, long timeId, string nodeId) {
			wrappedElement.nodeRemoved(sourceId, timeId, nodeId);
		}

		
		public void stepBegins(string sourceId, long timeId, double step) {
			wrappedElement.stepBegins(sourceId, timeId, step);
		}

		
		public IEnumerator<INode> iterator() {
			return nodes().GetEnumerator();
		}

	}

	class SynchronizedNode : SynchronizedElement<INode>, INode {

		private SynchronizedGraph sg;
		private object elementLock;

		SynchronizedNode(SynchronizedGraph sg, INode n) {
			base(n);

			this.sg = sg;
			this.elementLock = new object();
		}

		
		public IEnumerable<INode> neighborNodes() {
			List<INode> nodes;

			System.Threading.Monitor.Enter(elementLock);
			sg.System.Threading.Monitor.Enter(elementLock);

			try {
				nodes = wrappedElement.neighborNodes().map(n => sg.getNode(n.getIndex())).ToList();
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
				System.Threading.Monitor.Exit(elementLock);
			}

			return nodes;
		}

		
		public IEnumerable<IEdge> edges() {
			List<IEdge> edges;

			System.Threading.Monitor.Enter(elementLock);
			sg.System.Threading.Monitor.Enter(elementLock);

			try {

				edges = wrappedElement.edges().map(e => sg.getEdge(e.getIndex())).ToList();
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
				System.Threading.Monitor.Exit(elementLock);
			}

			return edges;
		}

		
		public IEnumerable<IEdge> leavingEdges() {
			List<IEdge> edges;

			System.Threading.Monitor.Enter(elementLock);
			sg.System.Threading.Monitor.Enter(elementLock);

			try {

				edges = wrappedElement.leavingEdges().map(e => sg.getEdge(e.getIndex())).ToList();
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
				System.Threading.Monitor.Exit(elementLock);
			}

			return edges;
		}

		
		public IEnumerable<IEdge> enteringEdges() {
			List<IEdge> edges;

			System.Threading.Monitor.Enter(elementLock);
			sg.System.Threading.Monitor.Enter(elementLock);

			try {

				edges = wrappedElement.enteringEdges().map(e => sg.getEdge(e.getIndex())).ToList();
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
				System.Threading.Monitor.Exit(elementLock);
			}

			return edges;
		}

		
		public IEnumerator<INode> getBreadthFirstIterator() {
			return getBreadthFirstIterator(false);
		}

		
		public IEnumerator<INode> getBreadthFirstIterator(bool directed) {
			List<INode> l = new List<INode>();
			IEnumerator<INode> it;

			System.Threading.Monitor.Enter(elementLock);
			sg.System.Threading.Monitor.Enter(elementLock);

			try {

				it = wrappedElement.getBreadthFirstIterator(directed);

				while (it.MoveNext())
					l.Add(sg.getNode(it.next().getIndex()));
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
				System.Threading.Monitor.Exit(elementLock);
			}

			return l.GetEnumerator();
		}

		
		public int getDegree() {
			int d;

			System.Threading.Monitor.Enter(elementLock);

			try {
				d = wrappedElement.getDegree();
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return d;
		}

		
		public IEnumerator<INode> getDepthFirstIterator() {
			return getDepthFirstIterator(false);
		}

		
		public IEnumerator<INode> getDepthFirstIterator(bool directed) {
			List<INode> l = new List<INode>();
			IEnumerator<INode> it;

			System.Threading.Monitor.Enter(elementLock);
			sg.System.Threading.Monitor.Enter(elementLock);

			try {
				it = wrappedElement.getDepthFirstIterator();

				while (it.MoveNext())
					l.Add(sg.getNode(it.next().getIndex()));
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
				System.Threading.Monitor.Exit(elementLock);
			}

			return l.GetEnumerator();
		}

		
		public IEdge getEdge(int i) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdge(i).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEnteringEdge(int i) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEnteringEdge(i).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getLeavingEdge(int i) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getLeavingEdge(i).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdgeBetween(string id) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdgeBetween(id).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdgeBetween(INode n) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdgeBetween(n).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdgeBetween(int index) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdgeBetween(index).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdgeFrom(string id) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdgeFrom(id).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdgeFrom(INode n) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdgeFrom(n).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdgeFrom(int index) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdgeFrom(index).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdgeToward(string id) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdgeToward(id).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdgeToward(INode n) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdgeToward(n).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IEdge getEdgeToward(int index) {
			IEdge e;

			System.Threading.Monitor.Enter(elementLock);

			try {
				e = sg.getEdge(wrappedElement.getEdgeToward(index).getIndex());
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return e;
		}

		
		public IGraph getGraph() {
			return sg;
		}

		
		public int getInDegree() {
			int d;

			System.Threading.Monitor.Enter(elementLock);

			try {
				d = wrappedElement.getInDegree();
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return d;
		}

		
		public int getOutDegree() {
			int d;

			System.Threading.Monitor.Enter(elementLock);

			try {
				d = wrappedElement.getOutDegree();
			} finally {
				System.Threading.Monitor.Exit(elementLock);
			}

			return d;
		}

		
		public IEnumerator<IEdge> iterator() {
			return edges().GetEnumerator();
		}
	}

	class SynchronizedEdge : SynchronizedElement<IEdge>, IEdge {

		SynchronizedGraph sg;

		SynchronizedEdge(SynchronizedGraph sg, IEdge e) {
			base(e);
			this.sg = sg;
		}

		
		public INode getNode0() {
			INode n;

			sg.System.Threading.Monitor.Enter(elementLock);

			try {
				n = sg.getNode(wrappedElement.getNode0().getIndex());
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
			}

			return n;
		}

		
		public INode getNode1() {
			INode n;

			sg.System.Threading.Monitor.Enter(elementLock);

			try {
				n = sg.getNode(wrappedElement.getNode1().getIndex());
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
			}

			return n;
		}

		
		public INode getOpposite(INode node) {
			INode n;

			if (node is SynchronizedNode)
				node = ((SynchronizedNode) node).wrappedElement;

			sg.System.Threading.Monitor.Enter(elementLock);

			try {
				n = sg.getNode(wrappedElement.getOpposite(node).getIndex());
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
			}

			return n;
		}

		
		public INode getSourceNode() {
			INode n;

			sg.System.Threading.Monitor.Enter(elementLock);

			try {
				n = sg.getNode(wrappedElement.getSourceNode().getIndex());
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
			}

			return n;
		}

		
		public INode getTargetNode() {
			INode n;

			sg.System.Threading.Monitor.Enter(elementLock);

			try {
				n = sg.getNode(wrappedElement.getTargetNode().getIndex());
			} finally {
				sg.System.Threading.Monitor.Exit(elementLock);
			}

			return n;
		}

		
		public bool isDirected() {
			return wrappedElement.isDirected();
		}

		
		public bool isLoop() {
			return wrappedElement.isLoop();
		}
	}
}

}
