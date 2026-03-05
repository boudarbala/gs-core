using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.Util.Cumulative
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


public class GraphSpells : ISink {
	private static readonly object /* Logger */ logger = null /* Logger */;

	CumulativeSpells graph;
	CumulativeAttributes graphAttributes;

	Dictionary<string, CumulativeSpells> nodes;
	Dictionary<string, CumulativeAttributes> nodesAttributes;

	Dictionary<string, CumulativeSpells> edges;
	Dictionary<string, CumulativeAttributes> edgesAttributes;
	Dictionary<string, EdgeData> edgesData;

	double date;

	public GraphSpells() {
		graph = new CumulativeSpells();
		graphAttributes = new CumulativeAttributes(0);

		nodes = new Dictionary<string, CumulativeSpells>();
		nodesAttributes = new Dictionary<string, CumulativeAttributes>();

		edges = new Dictionary<string, CumulativeSpells>();
		edgesAttributes = new Dictionary<string, CumulativeAttributes>();
		edgesData = new Dictionary<string, EdgeData>();

		date = double.NaN;
	}

	class EdgeData {
		string source;
		string target;
		bool directed;

		public string getSource() {
			return source;
		}

		public string getTarget() {
			return target;
		}

		public bool isDirected() {
			return directed;
		}
	}

	public IEnumerable<string> getNodes() {
		return nodes.Keys;
	}

	public IEnumerable<string> getEdges() {
		return edges.Keys;
	}

	public CumulativeSpells getNodeSpells(string nodeId) {
		return nodes[nodeId];
	}

	public CumulativeAttributes getNodeAttributes(string nodeId) {
		return nodesAttributes[nodeId];
	}

	public CumulativeSpells getEdgeSpells(string edgeId) {
		return edges[edgeId];
	}

	public CumulativeAttributes getEdgeAttributes(string edgeId) {
		return edgesAttributes[edgeId];
	}

	public EdgeData getEdgeData(string edgeId) {
		return edgesData[edgeId];
	}

	public void stepBegins(string sourceId, long timeId, double step) {
		this.date = step;

		graphAttributes.updateDate(step);
		graph.updateCurrentSpell(step);

		foreach (string id in nodes.Keys) {
			nodes[id].updateCurrentSpell(step);
			nodesAttributes[id].updateDate(step);
		}

		foreach (string id in edges.Keys) {
			edges[id].updateCurrentSpell(step);
			edgesAttributes[id].updateDate(step);
		}
	}

	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		if (!nodes.ContainsKey(nodeId)) {
			nodes[nodeId] = new CumulativeSpells();
			nodesAttributes[nodeId] = new CumulativeAttributes(date);
		}

		nodes[nodeId].startSpell(date);
	}

	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		if (nodes.ContainsKey(nodeId)) {
			nodes[nodeId].closeSpell();
			nodesAttributes[nodeId].Remove();
		}
	}

	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		if (!edges.ContainsKey(edgeId)) {
			edges[edgeId] = new CumulativeSpells();
			edgesAttributes[edgeId] = new CumulativeAttributes(date);

			EdgeData data = new EdgeData();
			data.source = fromNodeId;
			data.target = toNodeId;
			data.directed = directed;

			edgesData[edgeId] = data;
		}

		edges[edgeId].startSpell(date);

		EdgeData data = edgesData[edgeId];

		if (!data.source.Equals(fromNodeId) || !data.target.Equals(toNodeId) || data.directed != directed)
			Console.Error.WriteLine("An edge with this id but different properties" + " has already be created in the past.");
	}

	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		if (edges.ContainsKey(edgeId)) {
			edges[edgeId].closeSpell();
			edgesAttributes[edgeId].Remove();
		}
	}

	public void graphCleared(string sourceId, long timeId) {
		foreach (string id in nodes.Keys) {
			nodes[id].closeSpell();
			nodesAttributes[id].Remove();
		}

		foreach (string id in edges.Keys) {
			edges[id].closeSpell();
			edgesAttributes[id].Remove();
		}
	}

	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		graphAttributes.set(attribute, value);
	}

	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		graphAttributes.set(attribute, newValue);
	}

	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		graphAttributes.Remove(attribute);
	}

	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		nodesAttributes[nodeId].set(attribute, value);
	}

	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		nodesAttributes[nodeId].set(attribute, newValue);
	}

	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		nodesAttributes[nodeId].Remove(attribute);
	}

	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		edgesAttributes[edgeId].set(attribute, value);
	}

	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		edgesAttributes[edgeId].set(attribute, newValue);
	}

	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		edgesAttributes[edgeId].Remove(attribute);
	}

	public string toString() {
		System.Text.StringBuilder buffer = new System.Text.StringBuilder();

		foreach (string id in nodes.Keys) {
			buffer.Append("node#\"").Append(id).Append("\" ").Append(nodes[id]).Append(" ")
					.Append(nodesAttributes[id]).Append("\n");
		}

		foreach (string id in edges.Keys) {
			buffer.Append("edge#\"").Append(id).Append("\" ").Append(edges[id]).Append("\n");
		}

		return buffer.ToString();
	}

	public static void main(params string[] args) {
		GraphSpells graphSpells = new GraphSpells();
		IGraph g = new AdjacencyListGraph("g");

		g.addSink(graphSpells);

		g.addNode("A");
		g.addNode("B");
		g.addNode("C");
		g.stepBegins(1);
		g.getNode("A").setAttribute("test1", 100);
		g.addEdge("AB", "A", "B");
		g.addEdge("AC", "A", "C");
		g.stepBegins(2);
		g.addEdge("CB", "C", "B");
		g.removeNode("A");
		g.stepBegins(3);
		g.addNode("A");
		g.addEdge("AB", "A", "B");
		g.stepBegins(4);
		g.removeNode("C");
		g.stepBegins(5);

		Console.WriteLine(graphSpells);
	}
}

}
