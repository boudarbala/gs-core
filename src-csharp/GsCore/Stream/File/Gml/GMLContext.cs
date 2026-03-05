using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File.Gml
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


public class GMLContext {
	FileSourceGML gml;
	// GMLParser parser;
	string sourceId;
	bool directed;
	protected KeyValues nextStep = null;
	bool inGraph = false;

	GMLContext(FileSourceGML gml) {
		this.gml = gml;
		this.sourceId = string.Format("<GML stream {0}>", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
	}

	void handleKeyValues(KeyValues kv){
		if (nextStep != null) {
			insertKeyValues(nextStep);
			nextStep = null;
		}

		try {
			if (kv != null) {
				insertKeyValues(kv);
			}
		} catch (System.IO.IOException e) {
			throw new System.IO.IOException(e);
		}
	}

	void setNextStep(KeyValues kv) {
		nextStep = kv;
	}

	public void setDirected(bool on) {
		directed = on;
	}

	void setIsInGraph(bool on) {
		inGraph = on;
	}

	public void addNodeOrEdge(string element, KeyValues kv) {
		System.err.printf("adding {0} \n", element);
	}

	protected void insertKeyValues(KeyValues kv){
		if (kv.key != null) {
			if (inGraph) {
				if (kv.key.Equals("node") || kv.key.Equals("add-node")) {
					handleAddNode(kv);
				} else if (kv.key.Equals("edge") || kv.key.Equals("add-edge")) {
					handleAddEdge(kv);
				} else if (kv.key.Equals("del-node") || kv.key.Equals("-node")) {
					handleDelNode(kv);
				} else if (kv.key.Equals("del-edge") || kv.key.Equals("-edge")) {
					handleDelEdge(kv);
				} else if (kv.key.Equals("change-node") || kv.key.Equals("+node")) {
					handleChangeNode(kv);
				} else if (kv.key.Equals("change-edge") || kv.key.Equals("+edge")) {
					handleChangeEdge(kv);
				} else if (kv.key.Equals("step")) {
					handleStep(kv);
				} else if (kv.key.Equals("directed")) {
					setDirected(getBoolean(kv["directed"]));
				} else {
					if (kv.key.StartsWith("-")) {
						gml.sendAttributeChangedEvent(sourceId, sourceId, ElementType.GRAPH, kv.key.Substring(1),
								AttributeChangeEvent.REMOVE, null, null);
					} else {
						gml.sendAttributeChangedEvent(sourceId, sourceId, ElementType.GRAPH, kv.key,
								AttributeChangeEvent.ADD, null, compositeAttribute(kv));
					}
				}
			} else {
				// XXX Should we consider these events pertain to the graph ?
				// XXX

				if (kv.key.StartsWith("-")) {
					gml.sendAttributeChangedEvent(sourceId, sourceId, ElementType.GRAPH, kv.key.Substring(1),
							AttributeChangeEvent.REMOVE, null, null);
				} else {
					gml.sendAttributeChangedEvent(sourceId, sourceId, ElementType.GRAPH, kv.key,
							AttributeChangeEvent.ADD, null, compositeAttribute(kv));
				}
			}
		}
	}

	protected object compositeAttribute(KeyValues kv) {
		if (kv.Count < 2) {
			return kv[kv.key];
		} else {
			return kv;
		}
	}

	protected void handleAddNode(KeyValues kv){
		object thing = kv["node"];
		if (thing == null)
			thing = kv["add-node"];
		if (thing == null)
			kv.error("expecting a node or add-node token here");

		if (thing is string) {
			string id = (string) thing;
			gml.sendNodeAdded(sourceId, id);
		} else if (thing is KeyValues) {
			KeyValues node = (KeyValues) thing;
			string id = node.reqStringOrNumber("id");

			gml.sendNodeAdded(sourceId, id);
			handleNodeAttributes(id, node);
		} else {
			kv.error("unknown token type");
		}
	}

	protected long edgeid = 0;

	protected void handleAddEdge(KeyValues kv){
		object thing = kv["edge"];
		if (thing == null)
			thing = kv["add-edge"];
		if (thing == null)
			kv.error("expecting a edge or add-edge token here");
		if (!(thing is KeyValues))
			kv.error("expecting a set of values for the new edge");

		KeyValues edge = (KeyValues) thing;
		string id = edge.optString("id");

		string src = edge.reqStringOrNumber("source");
		string trg = edge.reqStringOrNumber("target");

		if (id == null)
			id = string.Format("{0}_{1}_{2}", src, trg, edgeid++);

		string dir = edge.optString("directed");

		bool directed = this.directed;

		if (dir != null) {
			directed = getBoolean(dir);
		}

		gml.sendEdgeAdded(sourceId, id, src, trg, directed);

		handleEdgeAttributes(id, edge);
	}

	protected void handleDelNode(KeyValues kv){
		object thing = kv["del-node"];
		if (thing == null)
			thing = kv["-node"];
		if (thing == null)
			kv.error("expecting a del-node or -node token here");

		if (thing is string) {
			string id = (string) thing;
			gml.sendNodeRemoved(sourceId, id);
		} else if (thing is KeyValues) {
			KeyValues node = (KeyValues) thing;
			string id = node.reqString("id");
			gml.sendNodeRemoved(sourceId, id);
		} else {
			kv.error("unknown token type");
		}
	}

	protected void handleDelEdge(KeyValues kv){
		object thing = kv["del-edge"];
		if (thing == null)
			thing = kv["-edge"];
		if (thing == null)
			kv.error("expecting a del-edge or -edge token here");

		if (thing is string) {
			string id = (string) thing;
			gml.sendEdgeRemoved(sourceId, id);
		} else if (thing is KeyValues) {
			KeyValues edge = (KeyValues) thing;
			string id = edge.reqString("id");
			gml.sendEdgeRemoved(sourceId, id);
		} else {
			kv.error("unknown token type");
		}
	}

	protected void handleChangeNode(KeyValues kv){
		object thing = kv["change-node"];
		if (thing == null)
			thing = kv["+node"];
		if (thing == null)
			kv.error("expecting a change-node or +node token here");
		if (!(thing is KeyValues))
			kv.error("expecting a set of values");

		KeyValues node = (KeyValues) thing;
		string id = node.reqString("id");

		handleNodeAttributes(id, node);
	}

	protected void handleChangeEdge(KeyValues kv){
		object thing = kv["change-edge"];
		if (thing == null)
			thing = kv["+edge"];
		if (thing == null)
			kv.error("expecting a change-edge or +edge token here");
		if (!(thing is KeyValues))
			kv.error("expecting a set of values");

		KeyValues edge = (KeyValues) thing;
		string id = edge.reqString("id");

		handleEdgeAttributes(id, edge);
	}

	protected void handleNodeAttributes(string id, KeyValues node) {
		foreach (string key in node.Keys) {
			if (key.StartsWith("-")) {
				if (key.Equals("-label"))
					key = "-ui.label";

				gml.sendAttributeChangedEvent(sourceId, id, ElementType.NODE, key.Substring(1),
						AttributeChangeEvent.REMOVE, null, null);
			} else {
				if (key.Equals("graphics") && node["graphics"] is KeyValues) {
					Graphics graphics = optNodeStyle((KeyValues) node["graphics"]);

					if (graphics != null) {
						if (graphics.position != null) {
							gml.sendAttributeChangedEvent(sourceId, id, ElementType.NODE, "xyz",
									AttributeChangeEvent.ADD, null, graphics.getPosition());
						}
						if (graphics.style != null) {
							gml.sendAttributeChangedEvent(sourceId, id, ElementType.NODE, "ui.style",
									AttributeChangeEvent.ADD, null, graphics.style);
						}
					}
				} else {
					string k = key;

					if (key.Equals("label"))
						k = "ui.label";

					gml.sendAttributeChangedEvent(sourceId, id, ElementType.NODE, k, AttributeChangeEvent.ADD, null,
							node[key]);
				}
			}
		}
	}

	protected void handleEdgeAttributes(string id, KeyValues edge) {
		foreach (string key in edge.Keys) {
			if (key.StartsWith("-")) {
				if (key.Equals("-label"))
					key = "-ui.label";

				gml.sendAttributeChangedEvent(sourceId, id, ElementType.EDGE, key.Substring(1),
						AttributeChangeEvent.REMOVE, null, null);
			} else {
				if (key.Equals("graphics") && edge["graphics"] is KeyValues) {
					Graphics graphics = optEdgeStyle((KeyValues) edge["graphics"]);

					if (graphics != null) {
						if (graphics.style != null) {
							gml.sendAttributeChangedEvent(sourceId, id, ElementType.EDGE, "ui.style",
									AttributeChangeEvent.ADD, null, graphics.style);
						}
					}
				} else {
					string k = key;

					if (key.Equals("label"))
						k = "ui.label";

					gml.sendAttributeChangedEvent(sourceId, id, ElementType.EDGE, k, AttributeChangeEvent.ADD, null,
							edge[key]);
				}
			}
		}
	}

	protected void handleStep(KeyValues kv){
		gml.sendStepBegins(sourceId, kv.reqNumber("step"));
	}

	protected Graphics optNodeStyle(KeyValues kv) {
		Graphics graphics = null;

		if (kv != null) {
			StringBuffer style = new StringBuffer();
			object w = null, h = null, d = null;
			graphics = new Graphics();

			if (kv["x"] != null) {
				graphics.setX(asDouble(kv["x"]));
			}
			if (kv["y"] != null) {
				graphics.setY(asDouble(kv["y"]));
			}
			if (kv["z"] != null) {
				graphics.setZ(asDouble(kv["z"]));
			}
			if (kv["w"] != null) {
				w = kv["w"];
			}
			if (kv["h"] != null) {
				h = kv["h"];
			}
			if (kv["d"] != null) {
				d = kv["d"];
			}
			if (w != null || h != null || d != null) {
				int ww = w != null ? (int) asDouble(w) : 0;
				int hh = h != null ? (int) asDouble(h) : 0;
				int dd = d != null ? (int) asDouble(d) : 0;
				style.Append(string.Format("size: {0}px, {1}px, {2}px; ", ww, hh, dd));
			}
			if (kv["type"] != null) {
				style.Append(string.Format("shape {0}; ", asNodeShape((string) kv["type"])));
			}

			commonGraphicsAttributes(kv, style);
			graphics.style = style.ToString();
		}

		return graphics;
	}

	protected Graphics optEdgeStyle(KeyValues kv) {
		Graphics graphics = null;

		if (kv != null) {
			StringBuffer style = new StringBuffer();
			object w = null;
			graphics = new Graphics();

			if (kv["width"] != null) {
				w = kv["width"];
			} else if (kv["w"] != null) {
				w = kv["w"];
			}
			if (w != null) {
				style.Append(string.Format("size: {0}px;", asDouble(w)));
			}
			if (kv["type"] != null) {
				style.Append(string.Format("shape {0}; ", asEdgeShape((string) kv["type"])));
			}

			commonGraphicsAttributes(kv, style);
			graphics.style = style.ToString();
		}

		return graphics;
	}

	protected void commonGraphicsAttributes(KeyValues kv, StringBuffer style) {
		if (kv["fill"] != null) {
			style.Append(string.Format("fill-color {0}; ", kv["fill"]));
		}
		if (kv["outline"] != null) {
			style.Append(string.Format("stroke-color {0}; ", kv["outline"]));
		}
		if (kv["outline_width"] != null) {
			style.Append(string.Format("stroke-width {0}px; ", kv["outline_width"]));
		}
		if ((kv["outline"] != null) || (kv["outline_width"] != null)) {
			style.Append("stroke-mode: plain; ");
		}
		if (kv["anchor"] != null) {
			style.Append(string.Format("text-alginment {0}; ", asTextAlignment((string) kv["anchor"])));
		}
		if (kv["image"] != null) {
			style.Append(string.Format("icon-mode: at-left; icon {0}; ", (string) kv["image"]));
		}
		if (kv["arrow"] != null) {
			style.Append(string.Format("arrow-shape {0}; ", asArrowShape((string) kv["arrow"])));
		}
		if (kv["font"] != null) {
			style.Append(string.Format("font {0}; ", (string) kv["font"]));
		}
	}

	protected double asDouble(object value) {
		if (value is IConvertible) {
			return ((IConvertible) value);
		} else if (value is string) {
			try {
				return double.Parse((string) value);
			} catch (FormatException e) {
				return 0.0;
			}
		} else {
			return 0.0;
		}
	}

	protected string asNodeShape(string type) {
		if (type.Equals("ellipse") || type.Equals("oval")) {
			return "circle";
		} else if (type.Equals("rectangle") || type.Equals("box")) {
			return "box";
		} else if (type.Equals("rounded-box")) {
			return "rounded-box";
		} else if (type.Equals("cross")) {
			return "cross";
		} else if (type.Equals("freeplane")) {
			return "freeplane";
		} else if (type.Equals("losange") || type.Equals("diamond")) {
			return "diamond";
		} else {
			return "circle";
		}
	}

	protected string asEdgeShape(string type) {
		if (type.Equals("line")) {
			return "line";
		} else if (type.Equals("cubic-curve")) {
			return "cubic-curve";
		} else if (type.Equals("angle")) {
			return "angle";
		} else if (type.Equals("blob")) {
			return "blob";
		} else if (type.Equals("freeplane")) {
			return "freeplane";
		} else {
			return "line";
		}
	}

	protected string asTextAlignment(string anchor) {
		if (anchor.Equals("c")) {
			return "center";
		} else if (anchor.Equals("n")) {
			return "above";
		} else if (anchor.Equals("ne")) {
			return "at-right";
		} else if (anchor.Equals("e")) {
			return "at-right";
		} else if (anchor.Equals("se")) {
			return "at-right";
		} else if (anchor.Equals("s")) {
			return "under";
		} else if (anchor.Equals("sw")) {
			return "at-left";
		} else if (anchor.Equals("w")) {
			return "at-left";
		} else if (anchor.Equals("nw")) {
			return "at-left";
		} else {
			return "center";
		}
	}

	protected string asArrowShape(string arrow) {
		if (arrow.Equals("none")) {
			return "none";
		} else if (arrow.Equals("last")) {
			return "arrow";
		} else {
			return "none";
		}
	}

	protected bool getBoolean(object bool) {
		if (bool is string) {
			return (bool.Equals("1") || bool.Equals("true") || bool.Equals("yes") || bool.Equals("y"));
		} else if (bool is IConvertible) {
			return (((IConvertible) bool) != 0);
		}
		return false;
	}
}

class Graphics {
	public double[] position = null;
	public string style = null;

	public void setX(double value) {
		if (position == null)
			position = new double[3];

		position[0] = value;
	}

	public void setY(double value) {
		if (position == null)
			position = new double[3];

		position[1] = value;
	}

	public void setZ(double value) {
		if (position == null)
			position = new double[3];

		position[2] = value;
	}

	public object[] getPosition() {
		object p[] = new object[3];
		p[0] = (double) position[0];
		p[1] = (double) position[1];
		p[2] = (double) position[2];
		return p;
	}
}

class KeyValues : Dictionary<string, object> {
	private static readonly long serialVersionUID = 5920553787913520204L;

	public string key;
	public int line;
	public int column;

	public void print() {
		System.err.printf("{0}:\n", key);
		foreach (string k in keySet()) {
			System.err.printf("    {0}: {1}\n", k, get(k));
		}
	}

	public string optString(string key){
		object o = get(key);

		if (o == null)
			return null;

		if (o is IConvertible)
			o = o.ToString();

		if (!(o is string))
			throw new System.IO.IOException(string.Format(
					"{0}:{1}: expecting a string or number value for tag {2}, got a list of values", line, column, key));

		remove(key);
		return (string) o;
	}

	protected string reqString(string key){
		object o = get(key);

		if (o == null)
			throw new System.IO.IOException(string.Format("{0} {1}: expecting a tag {2} but none found", line, column, key));

		if (!(o is string))
			throw new System.IO.IOException(string.Format(
					"{0}:{1}: expecting a string or number value for tag {2}, got a list of values", line, column, key));

		remove(key);

		return (string) o;
	}

	protected string reqStringOrNumber(string key){
		object o = get(key);

		if (o == null)
			throw new System.IO.IOException(string.Format("{0} {1}: expecting a tag {2} but none found", line, column, key));

		if (!(o is string) && !(o is IConvertible))
			throw new System.IO.IOException(string.Format(
					"{0}:{1}: expecting a string or number value for tag {2}, got a list of values", line, column, key));

		remove(key);

		if (o is IConvertible) {
			o = o.ToString();
		}

		return (string) o;
	}

	protected double reqNumber(string key){
		object o = get(key);
		double v = 0.0;

		if (o == null)
			throw new System.IO.IOException(string.Format("{0} {1}: expecting a tag {2} but none found", line, column, key));

		if (!(o is string))
			throw new System.IO.IOException(string.Format(
					"{0}:{1} expecting a string or number value for tag {2}, got a list of values", line, column, key));

		try {
			remove(key);
			v = double.Parse((string) o);
		} catch (FormatException e) {
			throw new System.IO.IOException(
					string.Format("{0} {1}: expecting a number value for tag {2}, got a string", line, column, key));
		}

		return v;
	}

	protected KeyValues optKeyValues(string key){
		object o = get(key);

		if (o == null)
			return null;

		if (!(o is KeyValues))
			throw new System.IO.IOException(string.Format("{0} {1}: expecting a list of values for tag {2}, got a string or number",
					line, column, key));

		remove(key);

		return (KeyValues) o;
	}

	protected KeyValues reqKeyValues(string key){
		object o = get(key);

		if (o == null)
			throw new System.IO.IOException(string.Format("{0} {1}: expecting a tag {2} but none found", line, column, key));

		if (!(o is KeyValues))
			throw new System.IO.IOException(string.Format("{0} {1}: expecting a list of values for tag {2}, got a string or number",
					line, column, key));

		remove(key);

		return (KeyValues) o;
	}

	protected void error(string message){
		throw new System.IO.IOException(string.Format("{0} {1} {2}", line, column, message));
	}
}

}
