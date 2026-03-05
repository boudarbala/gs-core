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
/// GraphML is a comprehensive and easy-to-use file format for graphs. It consists of a language core to describe the structural properties of a graph and a flexible extension mechanism to add application-specific data. Its main features include support of <ul> <li>directed, undirected, and mixed graphs,</li> <li>hypergraphs,</li> <li>hierarchical graphs,</li> <li>graphical representations,</li> <li>references to external data,</li> <li>application-specific attribute data, and</li> <li>light-weight parsers.</li> </ul> <p/> Unlike many other file formats for graphs, GraphML does not use a custom syntax. Instead, it is based on XML and hence ideally suited as a common denominator for all kinds of services generating, archiving, or processing graphs. <p/> <a href="http://graphml.graphdrawing.org/index.html">Source</a>
/// </summary>
public class FileSourceGraphML : FileSourceXML {
	private static readonly object /* Logger */ LOGGER = null /* Logger */;

	public interface GraphMLConstants {
		enum Balise {
			GRAPHML, GRAPH, NODE, EDGE, HYPEREDGE, DESC, DATA, LOCATOR, PORT, KEY, DEFAULT
		}

		enum GraphAttribute {
			ID, EDGEDEFAULT
		}

		enum LocatorAttribute {
			XMLNS_XLINK, XLINK_HREF, XLINK_TYPE
		}

		enum NodeAttribute {
			ID
		}

		enum EdgeAttribute {
			ID, SOURCE, SOURCEPORT, TARGET, TARGETPORT, DIRECTED
		}

		enum DataAttribute {
			KEY, ID
		}

		enum PortAttribute {
			NAME
		}

		enum EndPointAttribute {
			ID, NODE, PORT, TYPE
		}

		enum EndPointType {
			IN, OUT, UNDIR
		}

		enum HyperEdgeAttribute {
			ID
		}

		enum KeyAttribute {
			ID, FOR, ATTR_NAME, ATTR_TYPE
		}

		enum KeyDomain {
			GRAPHML, GRAPH, NODE, EDGE, HYPEREDGE, PORT, ENDPOINT, ALL
		}

		enum KeyAttrType {
			BOOLEAN, INT, LONG, FLOAT, DOUBLE, STRING
		}

		class Key {
			KeyDomain domain;
			string name;
			KeyAttrType type;
			string def = null;

			Key {
				domain = KeyDomain.ALL;
				name = null;
				type = KeyAttrType.STRING;
			}

			object getKeyValue {
				if
					return null;

				switch {
				case STRING:
					return value;
				case INT:
					return int.Parse(value);
				case LONG:
					return long.valueOf(value);
				case FLOAT:
					return float.valueOf(value);
				case DOUBLE:
					return double.valueOf(value);
				case BOOLEAN:
					return bool.valueOf(value);
				}

				return value;
			}

			object getDefaultValue() {
				return getKeyValue(def);
			}
		}

		class Data {
			Key key;
			string id;
			string value;
		}

		class Locator {
			string href;
			string xlink;
			string type;

			Locator() {
				xlink = "http://www.w3.org/TR/2000/PR-xlink-20001220/";
				type = "simple";
				href = null;
			}
		}

		class Port {
			string name;
			string desc;

			List<Data> datas;
			List<Port> ports;

			Port() {
				name = null;
				desc = null;

				datas = new List<Data>();
				ports = new List<Port>();
			}
		}

		class EndPoint {
			string id;
			string node;
			string port;
			string desc;
			EndPointType type;

			EndPoint() {
				id = null;
				node = null;
				port = null;
				desc = null;
				type = EndPointType.UNDIR;
			}
		}
	}

	protected GraphMLParser parser;

	/// <summary>
/// Build a new source to parse an xml stream in GraphML format.
/// </summary>
	public FileSourceGraphML() {
	}

	
	protected void afterStartDocument(){
		parser = new GraphMLParser();
		parser.__graphml();
	}

	
	protected void beforeEndDocument(){
		parser = null;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#nextEvents()
	 */
	public bool nextEvents(){
		return false;
	}

	protected class GraphMLParser : Parser, GraphMLConstants {
		protected Dictionary<string, Key> keys;
		protected Stack<string> graphId;
		protected int graphCounter;

		public GraphMLParser() {
			keys = new Dictionary<string, Key>();
			graphId = new Stack<string>();
			graphCounter = 0;
		}

		private object getValue(Data data) {
			return getValue(data.key, data.value);
		}

		private object getValue(Key key, string value) {
			switch (key.type) {
			case BOOLEAN:
				return bool.Parse(value);
			case INT:
				return int.Parse(value);
			case LONG:
				return long.Parse(value);
			case FLOAT:
				return float.Parse(value);
			case DOUBLE:
				return double.Parse(value);
			case STRING:
				return value;
			}

			return value;
		}

		private object getDefaultValue(Key key) {
			switch (key.type) {
			case BOOLEAN:
				return bool.TRUE;
			case INT:
				if (key.def != null)
					return int.Parse(key.def);

				return int.Parse(0);
			case LONG:
				if (key.def != null)
					return long.valueOf(key.def);

				return long.valueOf(0);
			case FLOAT:
				if (key.def != null)
					return float.valueOf(key.def);

				return float.valueOf(0.0f);
			case DOUBLE:
				if (key.def != null)
					return double.valueOf(key.def);

				return double.valueOf(0.0);
			case STRING:
				if (key.def != null)
					return key.def;

				return "";
			}

			return key.def != null ? key.def : bool.TRUE;
		}

		/// <summary>
/// <pre> <!ELEMENT graphml  ((desc)?,(key)*,((data)|(graph))*)> </pre>
/// </summary>
		private void __graphml(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "graphml");

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				pushback(e);
				__desc();

				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "key")) {
				pushback(e);
				__key();

				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "data") || isEvent(e, XMLEvent.START_ELEMENT, "graph")) {
				pushback(e);

				if (isEvent(e, XMLEvent.START_ELEMENT, "data")) {
					__data();
				} else {
					__graph();
				}

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "graphml");
		}

		/// <summary>
/// <pre> <!ELEMENT desc (#PCDATA)> </pre>
/// </summary>
/// <returns></returns>
		private string __desc(){
			XMLEvent e;
			string desc;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "desc");

			desc = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "desc");

			return desc;
		}

		/// <summary>
/// <pre> <!ELEMENT locator EMPTY> <!ATTLIST locator xmlns:xlink   CDATA    #FIXED    "http://www.w3.org/TR/2000/PR-xlink-20001220/" xlink:href    CDATA    #REQUIRED xlink:type    (simple) #FIXED    "simple" > </pre>
/// </summary>
/// <returns></returns>
		private Locator __locator(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "locator");

			
			IEnumerator<Attribute> attributes = e.asStartElement().getAttributes();

			Locator loc = new Locator();

			while (attributes.MoveNext()) {
				Attribute a = attributes.next();

				try {
					LocatorAttribute attribute = LocatorAttribute.valueOf(toConstantName(a));

					switch (attribute) {
					case XMLNS_XLINK:
						loc.xlink = a.Value;
						break;
					case XLINK_HREF:
						loc.href = a.Value;
						break;
					case XLINK_TYPE:
						loc.type = a.Value;
						break;
					}
				} catch (ArgumentException ex) {
					newParseError(e, false, "invalid locator attribute '%s'", a.Name.getLocalPart());
				}
			}

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "locator");

			if (loc.href == null)
				newParseError(e, true, "locator requires an href");

			return loc;
		}

		/// <summary>
/// <pre> <!ELEMENT key (#PCDATA)> <!ATTLIST key id  ID                                            #REQUIRED for (graphml|graph|node|edge|hyperedge|port|endpoint|all) "all" > </pre>
/// </summary>
		private void __key(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "key");

			
			IEnumerator<Attribute> attributes = e.asStartElement().getAttributes();

			string id = null;
			KeyDomain domain = KeyDomain.ALL;
			KeyAttrType type = KeyAttrType.STRING;
			string name = null;
			string def = null;

			while (attributes.MoveNext()) {
				Attribute a = attributes.next();

				try {
					KeyAttribute attribute = KeyAttribute.valueOf(toConstantName(a));

					switch (attribute) {
					case ID:
						id = a.Value;

						break;
					case FOR:
						try {
							domain = KeyDomain.valueOf(toConstantName(a.Value));
						} catch (ArgumentException ex) {
							newParseError(e, false, "invalid key domain '%s'", a.Value);
						}

						break;
					case ATTR_TYPE:
						try {
							type = KeyAttrType.valueOf(toConstantName(a.Value));
						} catch (ArgumentException ex) {
							newParseError(e, false, "invalid key type '%s'", a.Value);
						}

						break;
					case ATTR_NAME:
						name = a.Value;

						break;
					}
				} catch (ArgumentException ex) {
					newParseError(e, false, "invalid key attribute '%s'", a.Name.getLocalPart());
				}
			}

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "default")) {
				def = __characters();

				e = getNextEvent();
				checkValid(e, XMLEvent.END_ELEMENT, "default");

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "key");

			if (id == null)
				newParseError(e, true, "key requires an id");

			if (name == null)
				name = id;

			Key k = new Key();
			k.name = name;
			k.domain = domain;
			k.type = type;
			k.def = def;

			keys[id] = k;
		}

		/// <summary>
/// <pre> <!ELEMENT port ((desc)?,((data)|(port))*)> <!ATTLIST port name    NMTOKEN  #REQUIRED > </pre>
/// </summary>
/// <returns></returns>
		private Port __port(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "port");

			Port port = new Port();
			
			IEnumerator<Attribute> attributes = e.asStartElement().getAttributes();
			while (attributes.MoveNext()) {
				Attribute a = attributes.next();

				try {
					PortAttribute attribute = PortAttribute.valueOf(toConstantName(a));

					switch (attribute) {
					case NAME:
						port.name = a.Value;
						break;
					}
				} catch (ArgumentException ex) {
					newParseError(e, false, "invalid attribute '%s' for '<port>'", a.Name.getLocalPart());
				}
			}

			if (port.name == null)
				newParseError(e, true, "'<port>' element requires a 'name' attribute");

			e = getNextEvent();
			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				pushback(e);
				port.desc = __desc();
			} else {
				while (isEvent(e, XMLEvent.START_ELEMENT, "data") || isEvent(e, XMLEvent.START_ELEMENT, "port")) {
					if (isEvent(e, XMLEvent.START_ELEMENT, "data")) {
						Data data;

						pushback(e);
						data = __data();

						port.datas.Add(data);
					} else {
						Port portChild;

						pushback(e);
						portChild = __port();

						port.ports.Add(portChild);
					}

					e = getNextEvent();
				}
			}

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "port");

			return port;
		}

		/// <summary>
/// <pre> <!ELEMENT endpoint ((desc)?)> <!ATTLIST endpoint id    ID             #IMPLIED node  IDREF          #REQUIRED port  NMTOKEN        #IMPLIED type  (in|out|undir) "undir" > </pre>
/// </summary>
/// <returns></returns>
		private EndPoint __endpoint(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "endpoint");

			
			IEnumerator<Attribute> attributes = e.asStartElement().getAttributes();
			EndPoint ep = new EndPoint();

			while (attributes.MoveNext()) {
				Attribute a = attributes.next();

				try {
					EndPointAttribute attribute = EndPointAttribute.valueOf(toConstantName(a));

					switch (attribute) {
					case NODE:
						ep.node = a.Value;
						break;
					case ID:
						ep.id = a.Value;
						break;
					case PORT:
						ep.port = a.Value;
						break;
					case TYPE:
						try {
							ep.type = EndPointType.valueOf(toConstantName(a.Value));
						} catch (ArgumentException ex) {
							newParseError(e, false, "invalid end point type '%s'", a.Value);
						}

						break;
					}
				} catch (ArgumentException ex) {
					newParseError(e, false, "invalid attribute '%s' for '<endpoint>'", a.Name.getLocalPart());
				}
			}

			if (ep.node == null)
				newParseError(e, true, "'<endpoint>' element requires a 'node' attribute");

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				pushback(e);
				ep.desc = __desc();
			}

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "endpoint");

			return ep;
		}

		/// <summary>
/// <pre> <!ELEMENT data  (#PCDATA)> <!ATTLIST data key      IDREF        #REQUIRED id       ID           #IMPLIED > </pre>
/// </summary>
/// <returns></returns>
		private Data __data(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "data");

			
			IEnumerator<Attribute> attributes = e.asStartElement().getAttributes();
			string key = null, id = null, value;

			while (attributes.MoveNext()) {
				Attribute a = attributes.next();

				try {
					DataAttribute attribute = DataAttribute.valueOf(toConstantName(a));

					switch (attribute) {
					case KEY:
						key = a.Value;
						break;
					case ID:
						id = a.Value;
						break;
					}
				} catch (ArgumentException ex) {
					newParseError(e, false, "invalid attribute '%s' for '<data>'", a.Name.getLocalPart());
				}
			}

			if (key == null)
				newParseError(e, true, "'<data>' element must have a 'key' attribute");

			value = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "data");

			if (!keys.ContainsKey(key))
				newParseError(e, true, "unknown key '%s'", key);

			Data d = new Data();

			d.key = keys[key];
			d.id = id;
			d.value = value;

			return d;
		}

		/// <summary>
/// <pre> <!ELEMENT graph    ((desc)?,((((data)|(node)|(edge)|(hyperedge))*)|(locator)))> <!ATTLIST graph id          ID                    #IMPLIED edgedefault (directed|undirected) #REQUIRED > </pre>
/// </summary>
		private void __graph(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "graph");

			
			IEnumerator<Attribute> attributes = e.asStartElement().getAttributes();

			string id = null;
			string desc = null;
			bool directed = false;
			bool directedSet = false;

			while (attributes.MoveNext()) {
				Attribute a = attributes.next();

				try {
					GraphAttribute attribute = GraphAttribute.valueOf(toConstantName(a));

					switch (attribute) {
					case ID:
						id = a.Value;
						break;
					case EDGEDEFAULT:
						if (a.Value.Equals("directed"))
							directed = true;
						else if (a.Value.Equals("undirected"))
							directed = false;
						else
							newParseError(e, true, "invalid 'edgedefault' value '%s'", a.Value);

						directedSet = true;

						break;
					}
				} catch (ArgumentException ex) {
					newParseError(e, false, "invalid node attribute '%s'", a.Name.getLocalPart());
				}
			}

			if (!directedSet)
				newParseError(e, false, "graph requires attribute 'edgedefault'");

			string gid = "";

			if (graphId.Count > 0)
				gid = graphId.peek() + ":";

			if (id != null)
				gid += id;
			else
				gid += int.toString(graphCounter++);

			graphId.push(gid);

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				pushback(e);
				desc = __desc();

				sendGraphAttributeAdded(sourceId, "desc", desc);

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "locator")) {
				pushback(e);
				__locator();
				// TODO
				e = getNextEvent();
			} else {
				while (isEvent(e, XMLEvent.START_ELEMENT, "data") || isEvent(e, XMLEvent.START_ELEMENT, "node")
						|| isEvent(e, XMLEvent.START_ELEMENT, "edge")
						|| isEvent(e, XMLEvent.START_ELEMENT, "hyperedge")) {
					pushback(e);

					if (isEvent(e, XMLEvent.START_ELEMENT, "data")) {
						Data data = __data();
						sendGraphAttributeAdded(sourceId, data.key.name, getValue(data));
					} else if (isEvent(e, XMLEvent.START_ELEMENT, "node")) {
						__node();
					} else if (isEvent(e, XMLEvent.START_ELEMENT, "edge")) {
						__edge(directed);
					} else {
						__hyperedge();
					}

					e = getNextEvent();
				}
			}

			graphId.pop();
			checkValid(e, XMLEvent.END_ELEMENT, "graph");
		}

		/// <summary>
/// <pre> <!ELEMENT node   (desc?,(((data|port)*,graph?)|locator))> <!ATTLIST node id        ID      #REQUIRED > </pre>
/// </summary>
		private void __node(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "node");

			
			IEnumerator<Attribute> attributes = e.asStartElement().getAttributes();

			string id = null;
			HashSet<Key> sentAttributes = new HashSet<Key>();
			HashSet<Attribute> unexpectedAttributes = new HashSet<object>();

			while (attributes.MoveNext()) {
				Attribute a = attributes.next();

				try {
					NodeAttribute attribute = NodeAttribute.valueOf(toConstantName(a));

					switch (attribute) {
					case ID:
						id = a.Value;
						break;
					}
				} catch (ArgumentException ex) {
					if (strictMode)
						newParseError(e, false, "invalid node attribute '%s'", a.Name.getLocalPart());
					unexpectedAttributes.Add(a);
				}
			}

			if (id == null)
				newParseError(e, true, "node requires an id");

			sendNodeAdded(sourceId, id);

			if (!strictMode && unexpectedAttributes.Count > 0) {
				foreach (Attribute a in unexpectedAttributes) {
					string name = a.Name.getLocalPart();
					Key key = keys[name];
					object value = key == null ? a.Value : getValue(key, a.Value);

					sendNodeAttributeAdded(sourceId, id, name, value);

					if (key != null)
						sentAttributes.Add(key);
				}
			}

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				string desc;

				pushback(e);
				desc = __desc();

				sendNodeAttributeAdded(sourceId, id, "desc", desc);
			} else if (isEvent(e, XMLEvent.START_ELEMENT, "locator")) {
				// TODO
				pushback(e);
				__locator();
			} else {
				while (isEvent(e, XMLEvent.START_ELEMENT, "data") || isEvent(e, XMLEvent.START_ELEMENT, "port")) {
					if (isEvent(e, XMLEvent.START_ELEMENT, "data")) {
						Data data;

						pushback(e);
						data = __data();

						sendNodeAttributeAdded(sourceId, id, data.key.name, getValue(data));

						sentAttributes.Add(data.key);
					} else {
						pushback(e);
						__port();
					}

					e = getNextEvent();
				}
			}

			foreach (Key k in keys.Values) {
				if ((k.domain == KeyDomain.NODE || k.domain == KeyDomain.ALL) && !sentAttributes.Contains(k))
					sendNodeAttributeAdded(sourceId, id, k.name, getDefaultValue(k));
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "graph")) {
				Location loc = e.getLocation();

				System.err.printf("[WARNING] %d:%d graph inside node is not implemented", loc.getLineNumber(),
						loc.getColumnNumber());

				pushback(e);
				__graph();

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "node");
		}

		/// <summary>
/// <pre> <!ELEMENT edge ((desc)?,(data)*,(graph)?)> <!ATTLIST edge id         ID           #IMPLIED source     IDREF        #REQUIRED sourceport NMTOKEN      #IMPLIED target     IDREF        #REQUIRED targetport NMTOKEN      #IMPLIED directed   (true|false) #IMPLIED > </pre>
/// </summary>
/// <param name="edgedefault"></param>
		private void __edge(bool edgedefault){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "edge");

			
			IEnumerator<Attribute> attributes = e.asStartElement().getAttributes();

			HashSet<Key> sentAttributes = new HashSet<Key>();
			HashSet<Attribute> unexpectedAttributes = new HashSet<object>();
			string id = null;
			bool directed = edgedefault;
			string source = null;
			string target = null;

			while (attributes.MoveNext()) {
				Attribute a = attributes.next();

				try {
					EdgeAttribute attribute = EdgeAttribute.valueOf(toConstantName(a));

					switch (attribute) {
					case ID:
						id = a.Value;
						break;
					case DIRECTED:
						directed = bool.Parse(a.Value);
						break;
					case SOURCE:
						source = a.Value;
						break;
					case TARGET:
						target = a.Value;
						break;
					case SOURCEPORT:
					case TARGETPORT:
						newParseError(e, false, "sourceport and targetport not implemented");
					}
				} catch (ArgumentException ex) {
					if (strictMode)
						newParseError(e, false, "invalid graph attribute '%s'", a.Name.getLocalPart());
					unexpectedAttributes.Add(a);
				}
			}

			if (source == null || target == null)
				newParseError(e, true, "edge must have a source and a target");

			if (id == null) {
				id = string.Format("{0}--{1}", source, target);
			}

			sendEdgeAdded(sourceId, id, source, target, directed);

			if (!strictMode && unexpectedAttributes.Count > 0) {
				foreach (Attribute a in unexpectedAttributes) {
					string name = a.Name.getLocalPart();
					Key key = keys[name];
					object value = key == null ? a.Value : getValue(key, a.Value);

					sendEdgeAttributeAdded(sourceId, id, name, value);

					if (key != null)
						sentAttributes.Add(key);
				}
			}

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				string desc;

				pushback(e);
				desc = __desc();

				sendEdgeAttributeAdded(sourceId, id, "desc", desc);
			} else {
				while (isEvent(e, XMLEvent.START_ELEMENT, "data")) {
					Data data;

					pushback(e);
					data = __data();

					sendEdgeAttributeAdded(sourceId, id, data.key.name, getValue(data));

					sentAttributes.Add(data.key);

					e = getNextEvent();
				}
			}

			foreach (Key k in keys.Values) {
				if ((k.domain == KeyDomain.EDGE || k.domain == KeyDomain.ALL) && !sentAttributes.Contains(k))
					sendEdgeAttributeAdded(sourceId, id, k.name, getDefaultValue(k));
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "graph")) {
				newParseError(e, false, "graph inside node is not implemented");

				pushback(e);
				__graph();

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "edge");
		}

		/// <summary>
/// <pre> <!ELEMENT hyperedge  ((desc)?,((data)|(endpoint))*,(graph)?)> <!ATTLIST hyperedge id     ID      #IMPLIED > </pre>
/// </summary>
		private void __hyperedge(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "hyperedge");

			newParseError(e, false, "hyperedge feature is not implemented");

			string id = null;

			
			IEnumerator<Attribute> attributes = e.asStartElement().getAttributes();

			while (attributes.MoveNext()) {
				Attribute a = attributes.next();

				try {
					HyperEdgeAttribute attribute = HyperEdgeAttribute.valueOf(toConstantName(a));

					switch (attribute) {
					case ID:
						id = a.Value;
						break;
					}
				} catch (ArgumentException ex) {
					newParseError(e, false, "invalid attribute '%s' for '<endpoint>'", a.Name.getLocalPart());
				}
			}

			if (id == null)
				newParseError(e, true, "'<hyperedge>' element requires a 'node' attribute");

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				pushback(e);
				__desc();
			} else {
				while (isEvent(e, XMLEvent.START_ELEMENT, "data") || isEvent(e, XMLEvent.START_ELEMENT, "endpoint")) {
					if (isEvent(e, XMLEvent.START_ELEMENT, "data")) {
						pushback(e);
						__data();
					} else {
						pushback(e);
						__endpoint();
					}

					e = getNextEvent();
				}
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "graph")) {
				newParseError(e, false, "graph inside node is not implemented");

				pushback(e);
				__graph();

				e = getNextEvent();
			}

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "hyperedge");
		}
	}
}

}
