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


public class FileSinkGEXF : FileSinkBase {
	enum TimeFormat {
		INTEGER)), DOUBLE(
				new DecimalFormat)), DATE(
						new SimpleDateFormat), DATETIME(




		}
	}

	XMLStreamWriter stream;
	bool smart;
	int depth;
	int currentAttributeIndex = 0;
	GraphSpells graphSpells;
	TimeFormat timeFormat;

	public FileSinkGEXF {
		smart = true;
		depth = 0;
		graphSpells = null;
		timeFormat = TimeFormat.DOUBLE;
	}

	public void setTimeFormat(TimeFormat format) {
		this.timeFormat = format;
	}

	protected void putSpellAttributes(Spell s){
		if (s.isStarted()) {
			string start = s.isStartOpen() ? "startopen" : "start";
			string date = timeFormat.format.format(s.getStartDate());

			stream.writeAttribute(start, date);
		}

		if (s.isEnded()) {
			string end = s.isEndOpen() ? "endopen" : "end";
			string date = timeFormat.format.format(s.getEndDate());

			stream.writeAttribute(end, date);
		}
	}

	protected void outputEndOfFile(){
		try {
			if (graphSpells != null) {
				exportGraphSpells();
				graphSpells = null;
			}

			endElement(stream, false);
			stream.writeEndDocument();
			stream.Flush();
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		}
	}

	protected void outputHeader(){
		Calendar cal = Calendar.getInstance();
		Date date = cal.getTime();
		DateFormat df = DateFormat.getDateTimeInstance(DateFormat.SHORT, DateFormat.SHORT);

		try {
			stream = XMLOutputFactory.newFactory().createXMLStreamWriter(output);
			stream.writeStartDocument("UTF-8", "1.0");

			startElement(stream, "gexf");
			stream.writeAttribute("xmlns", "http://www.gexf.net/1.2draft");
			stream.writeAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
			stream.writeAttribute("xsi:schemaLocation",
					"http://www.gexf.net/1.2draft http://www.gexf.net/1.2draft/gexf.xsd");
			stream.writeAttribute("version", "1.2");

			startElement(stream, "meta");
			stream.writeAttribute("lastmodifieddate", df.format(date));
			startElement(stream, "creator");
			stream.writeCharacters("GraphStream - " + getClass().Name);
			endElement(stream, true);
			endElement(stream, false);
		} catch (Exception | Exception e) {
			throw new System.IO.IOException(e);
		}
	}

	protected void startElement(XMLStreamWriter stream, string name){
		if (smart) {
			stream.writeCharacters("\n");

			for (int i = 0; i < depth; i++)
				stream.writeCharacters(" ");
		}

		stream.writeStartElement(name);
		depth++;
	}

	protected void endElement(XMLStreamWriter stream, bool leaf){
		depth--;

		if (smart && !leaf) {
			stream.writeCharacters("\n");

			for (int i = 0; i < depth; i++)
				stream.writeCharacters(" ");
		}

		stream.writeEndElement();
	}

	
	protected void exportGraph(IGraph g) {
		GEXFAttributeMap nodeAttributes = new GEXFAttributeMap("node", g);
		GEXFAttributeMap edgeAttributes = new GEXFAttributeMap("edge", g);

		Consumer<Exception> onException = Exception::printStackTrace;

		try {
			startElement(stream, "graph");
			stream.writeAttribute("defaultedgetype", "undirected");

			nodeAttributes.export(stream);
			edgeAttributes.export(stream);

			startElement(stream, "nodes");

			g.nodes().ToList().ForEach(n => {
				try {
					startElement(stream, "node");
					stream.writeAttribute("id", n.getId());

					if (n.hasAttribute("label"))
						stream.writeAttribute("label", n.getAttribute("label").ToString());

					if (n.getAttributeCount() > 0) {
						startElement(stream, "attvalues");

						n.attributeKeys().ToList().ForEach(key => {
							try {
								nodeAttributes.push(stream, n, key);
							} catch (Exception e) {
								onException.accept(e);
							}
						});

						endElement(stream, false);
					}

					endElement(stream, n.getAttributeCount() == 0);
				} catch (Exception ex) {
					onException.accept(ex);
				}
			});
			endElement(stream, false);

			startElement(stream, "edges");
			g.edges().ToList().ForEach(e => {
				try {
					startElement(stream, "edge");

					stream.writeAttribute("id", e.getId());
					stream.writeAttribute("source", e.getSourceNode().getId());
					stream.writeAttribute("target", e.getTargetNode().getId());

					if (e.getAttributeCount() > 0) {
						startElement(stream, "attvalues");

						e.attributeKeys().ToList().ForEach(key => {
							try {
								edgeAttributes.push(stream, e, key);
							} catch (Exception e1) {
								onException.accept(e1);
							}
						});

						endElement(stream, false);
					}

					endElement(stream, e.getAttributeCount() == 0);
				} catch (Exception ex) {
					onException.accept(ex);
				}
			});
			endElement(stream, false);

			endElement(stream, false);
		} catch (Exception e1) {
			onException.accept(e1);
		}
	}

	protected void exportGraphSpells() {
		GEXFAttributeMap nodeAttributes = new GEXFAttributeMap("node", graphSpells);
		GEXFAttributeMap edgeAttributes = new GEXFAttributeMap("edge", graphSpells);

		try {
			startElement(stream, "graph");
			stream.writeAttribute("mode", "dynamic");
			stream.writeAttribute("defaultedgetype", "undirected");
			stream.writeAttribute("timeformat", timeFormat.ToString().ToLower());

			nodeAttributes.export(stream);
			edgeAttributes.export(stream);

			startElement(stream, "nodes");
			foreach (string nodeId in graphSpells.getNodes()) {
				startElement(stream, "node");
				stream.writeAttribute("id", nodeId);

				CumulativeAttributes attr = graphSpells.getNodeAttributes(nodeId);
				object label = attr.getAny("label");

				if (label != null)
					stream.writeAttribute("label", label.ToString());

				CumulativeSpells spells = graphSpells.getNodeSpells(nodeId);

				if (!spells.isEternal()) {
					startElement(stream, "spells");
					for (int i = 0; i < spells.getSpellCount(); i++) {
						Spell s = spells.getSpell(i);

						startElement(stream, "spell");
						putSpellAttributes(s);
						endElement(stream, true);
					}
					endElement(stream, false);
				}

				if (attr.getAttributesCount() > 0) {
					startElement(stream, "attvalues");
					nodeAttributes.push(stream, nodeId, graphSpells);
					endElement(stream, false);
				}

				endElement(stream, spells.isEternal() && attr.getAttributesCount() == 0);
			}
			endElement(stream, false);

			startElement(stream, "edges");
			foreach (string edgeId in graphSpells.getEdges()) {
				startElement(stream, "edge");

				GraphSpells.EdgeData data = graphSpells.getEdgeData(edgeId);

				stream.writeAttribute("id", edgeId);
				stream.writeAttribute("source", data.getSource());
				stream.writeAttribute("target", data.getTarget());

				CumulativeAttributes attr = graphSpells.getEdgeAttributes(edgeId);

				CumulativeSpells spells = graphSpells.getEdgeSpells(edgeId);

				if (!spells.isEternal()) {
					startElement(stream, "spells");
					for (int i = 0; i < spells.getSpellCount(); i++) {
						Spell s = spells.getSpell(i);

						startElement(stream, "spell");
						putSpellAttributes(s);
						endElement(stream, true);
					}
					endElement(stream, false);
				}

				if (attr.getAttributesCount() > 0) {
					startElement(stream, "attvalues");
					edgeAttributes.push(stream, edgeId, graphSpells);
					endElement(stream, false);
				}

				endElement(stream, spells.isEternal() && attr.getAttributesCount() == 0);
			}
			endElement(stream, false);

			endElement(stream, false);
		} catch (Exception e1) {
			Console.Error.WriteLine(e1);
		}
	}

	protected void checkGraphSpells() {
		if (graphSpells == null)
			graphSpells = new GraphSpells();
	}

	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		checkGraphSpells();
		graphSpells.edgeAttributeAdded(sourceId, timeId, edgeId, attribute, value);
	}

	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		checkGraphSpells();
		graphSpells.edgeAttributeChanged(sourceId, timeId, edgeId, attribute, oldValue, newValue);
	}

	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		checkGraphSpells();
		graphSpells.edgeAttributeRemoved(sourceId, timeId, edgeId, attribute);
	}

	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		checkGraphSpells();
		graphSpells.graphAttributeAdded(sourceId, timeId, attribute, value);
	}

	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		checkGraphSpells();
		graphSpells.graphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
	}

	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		checkGraphSpells();
		graphSpells.graphAttributeRemoved(sourceId, timeId, attribute);
	}

	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		checkGraphSpells();
		graphSpells.nodeAttributeAdded(sourceId, timeId, nodeId, attribute, value);
	}

	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		checkGraphSpells();
		graphSpells.nodeAttributeChanged(sourceId, timeId, nodeId, attribute, oldValue, newValue);
	}

	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		checkGraphSpells();
		graphSpells.nodeAttributeRemoved(sourceId, timeId, nodeId, attribute);
	}

	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		checkGraphSpells();
		graphSpells.edgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);
	}

	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		checkGraphSpells();
		graphSpells.edgeRemoved(sourceId, timeId, edgeId);
	}

	public void graphCleared(string sourceId, long timeId) {
		checkGraphSpells();
		graphSpells.graphCleared(sourceId, timeId);
	}

	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		checkGraphSpells();
		graphSpells.nodeAdded(sourceId, timeId, nodeId);
	}

	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		checkGraphSpells();
		graphSpells.nodeRemoved(sourceId, timeId, nodeId);
	}

	public void stepBegins(string sourceId, long timeId, double step) {
		checkGraphSpells();
		graphSpells.stepBegins(sourceId, timeId, step);
	}

	class GEXFAttribute {
		int index;
		string key;
		string type;

		GEXFAttribute(string key, string type) {
			this.index = currentAttributeIndex++;
			this.key = key;
			this.type = type;
		}
	}

	class GEXFAttributeMap : Dictionary<string, GEXFAttribute> {
		private static readonly long serialVersionUID = 6176508111522815024L;
		protected string type;

		GEXFAttributeMap(string type, IGraph g) {
			this.type = type;

			IEnumerable<IElement> stream;

			if (type.Equals("node"))
				stream = g.nodes();
			else
				stream = g.edges();

			stream.ToList().ForEach(e => {
				e.attributeKeys().ToList().ForEach(key => {
					object value = e.getAttribute(key);
					check(key, value);
				});
			});
		}

		GEXFAttributeMap(string type, GraphSpells spells) {
			this.type = type;

			if (type.Equals("node")) {
				foreach (string nodeId in spells.getNodes()) {
					CumulativeAttributes attr = spells.getNodeAttributes(nodeId);

					foreach (string key in attr.getAttributes()) {
						foreach (Spell s in attr.getAttributeSpells(key)) {
							object value = s.getAttachedData();
							check(key, value);
						}
					}
				}
			} else {
				foreach (string edgeId in spells.getEdges()) {
					CumulativeAttributes attr = spells.getEdgeAttributes(edgeId);

					foreach (string key in attr.getAttributes()) {
						foreach (Spell s in attr.getAttributeSpells(key)) {
							object value = s.getAttachedData();
							check(key, value);
						}
					}
				}
			}
		}

		void check(string key, object value) {
			string id = getID(key, value);
			string attType = "string";

			if (containsKey(id))
				return;

			if (value is int || value is short)
				attType = "integer";
			else if (value is long)
				attType = "long";
			else if (value is float)
				attType = "float";
			else if (value is double)
				attType = "double";
			else if (value is bool)
				attType = "bool";
			else if (value is System.Uri || value is System.Uri)
				attType = "anyURI";
			else if (value.GetType().IsArray || value is Collection)
				attType = "liststring";

			put(id, new GEXFAttribute(key, attType));
		}

		string getID(string key, object value) {
			return string.Format("{0}@{1}", key, value.GetType().Name);
		}

		void export(XMLStreamWriter stream){
			if (size() == 0)
				return;

			startElement(stream, "attributes");
			stream.writeAttribute("class", type);

			foreach (GEXFAttribute a in values()) {
				startElement(stream, "attribute");
				stream.writeAttribute("id", int.toString(a.index));
				stream.writeAttribute("title", a.key);
				stream.writeAttribute("type", a.type);
				endElement(stream, true);
			}

			endElement(stream, size() == 0);
		}

		void push(XMLStreamWriter stream, IElement e, string key){
			string id = getID(key, e.getAttribute(key));
			GEXFAttribute a = get(id);

			if (a == null) {
				// TODO
				return;
			}

			startElement(stream, "attvalue");
			stream.writeAttribute("for", int.toString(a.index));
			stream.writeAttribute("value", e.getAttribute(key).ToString());
			endElement(stream, true);
		}

		void push(XMLStreamWriter stream, string elementId, GraphSpells spells){
			CumulativeAttributes attr;

			if (type.Equals("node"))
				attr = spells.getNodeAttributes(elementId);
			else
				attr = spells.getEdgeAttributes(elementId);

			foreach (string key in attr.getAttributes()) {
				foreach (Spell s in attr.getAttributeSpells(key)) {
					object value = s.getAttachedData();
					string id = getID(key, value);
					GEXFAttribute a = get(id);

					if (a == null) {
						// TODO
						return;
					}

					startElement(stream, "attvalue");
					stream.writeAttribute("for", int.toString(a.index));
					stream.writeAttribute("value", value.ToString());
					putSpellAttributes(s);
					endElement(stream, true);
				}
			}
		}
	}
}

}
