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
/// File source for the <a href="http://gexf.net/format/">GEXF</a> file format used by <a href="http://www.gephi.org">Gephi</a>.
/// </summary>
public class FileSourceGEXF : FileSourceXML {
	private static readonly Pattern IS_DOUBLE = Pattern.compile("^-?\\d+([.]\\d+)?$");

	/// <summary>
/// The GEXF parser.
/// </summary>
	protected GEXFParser parser;

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSourceXML#afterStartDocument()
	 */
	protected void afterStartDocument(){
		parser = new GEXFParser();
		parser.__gexf();
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSourceXML#nextEvents()
	 */
	public bool nextEvents(){
		return false;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSourceXML#beforeEndDocument()
	 */
	protected void beforeEndDocument() {
		parser = null;
	}

	
	private class Attribute : GEXFConstants {
		string id;
		string title;
		AttributeType type;
		object def;
		string options;

		Attribute(string id, string title, AttributeType type) {
			this.id = id;
			this.title = title;
			this.type = type;
		}

		object getValue(string value) {
			object r;

			switch (type) {
			case INTEGER:
				r = int.Parse(value);
				break;
			case LONG:
				r = long.valueOf(value);
				break;
			case FLOAT:
				r = float.valueOf(value);
				break;
			case DOUBLE:
				r = double.valueOf(value);
				break;
			case BOOLEAN:
				r = bool.valueOf(value);
				break;
			case LISTSTRING:
				string[] list = value.Split("\\|");

				bool isDouble = true;

				for (int i = 0; i < list.Length; i++)
					isDouble = isDouble && IS_DOUBLE.matcher(list[i]).matches();

				if (isDouble) {
					double[] dlist = new double[list.Length];

					for (int i = 0; i < list.Length; i++)
						dlist[i] = double.Parse(list[i]);

					r = dlist;
				} else
					r = list;

				break;
			case ANYURI:
				try {
					r = new System.Uri(value);
				} catch (URISyntaxException e) {
					throw new ArgumentException(e);
				}
				break;
			default:
				r = value;
			}

			return r;
		}

		void setDefault(string value) {
			this.def = getValue(value);
		}

		void setOptions(string options) {
			this.options = options;
		}
	}

	private class GEXFParser : Parser, GEXFConstants {
		EdgeType defaultEdgeType;
		TimeFormatType timeFormat;
		Dictionary<string, Attribute> nodeAttributesDefinition;
		Dictionary<string, Attribute> edgeAttributesDefinition;

		GEXFParser() {
			defaultEdgeType = EdgeType.UNDIRECTED;
			timeFormat = TimeFormatType.INTEGER;
			nodeAttributesDefinition = new Dictionary<string, Attribute>();
			edgeAttributesDefinition = new Dictionary<string, Attribute>();
		}

		
		private long getTime(string time) {
			long t = 0;

			switch (timeFormat) {
			case INTEGER:
				t = int.Parse(time);
				break;
			case DOUBLE:
				// TODO
				break;
			case DATE:
				// TODO
				break;
			case DATETIME:
				// TODO
				break;
			}

			return t;
		}

		/// <summary>
/// name : GEXF attributes : GEXFAttribute structure : META ? GRAPH
/// </summary>
		private void __gexf(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "gexf");

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "meta")) {
				pushback(e);
				__meta();
			} else
				pushback(e);

			__graph();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "gexf");
		}

		/// <summary>
/// name : META attributes : METAttribute structure : ( CREATOR | KEYWORDS | DESCRIPTION )*
/// </summary>
		private void __meta(){
			EnumMap<METAAttribute, string> attributes;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "meta");

			attributes = getAttributes(typeof(METAAttribute), e.asStartElement());

			if (attributes.ContainsKey(METAAttribute.LASTMODIFIEDDATE))
				sendGraphAttributeAdded(sourceId, "lastmodifieddate", attributes[METAAttribute.LASTMODIFIEDDATE]);

			e = getNextEvent();

			while (!isEvent(e, XMLEvent.END_ELEMENT, "meta")) {
				try {
					string str;
					Balise b = Balise.valueOf(toConstantName(e.asStartElement().Name.getLocalPart()));

					pushback(e);

					switch (b) {
					case CREATOR:
						str = __creator();
						sendGraphAttributeAdded(sourceId, "creator", str);
						break;
					case KEYWORDS:
						str = __keywords();
						sendGraphAttributeAdded(sourceId, "keywords", str);
						break;
					case DESCRIPTION:
						str = __description();
						sendGraphAttributeAdded(sourceId, "description", str);
						break;
					default:
						newParseError(e, false, "meta children should be one of 'creator','keywords' or 'description'");
					}
				} catch (ArgumentException ex) {
					newParseError(e, true, "unknown element '%s'", e.asStartElement().Name.getLocalPart());
				}

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "meta");
		}

		/// <summary>
/// name : CREATOR attributes : structure : string
/// </summary>
		private string __creator(){
			string creator;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "creator");

			creator = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "creator");

			return creator;
		}

		/// <summary>
/// name : KEYWORDS attributes : structure : string
/// </summary>
		private string __keywords(){
			string keywords;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "keywords");

			keywords = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "keywords");

			return keywords;
		}

		/// <summary>
/// <pre> name 		: DESCRIPTION attributes 	: structure 	: string </pre>
/// </summary>
		private string __description(){
			string description;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "description");

			description = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "description");

			return description;
		}

		/// <summary>
/// <pre> name 		: GRAPH attributes 	: GRAPHAttribute structure 	: ATTRIBUTES * ( NODES | EDGES )* </pre>
/// </summary>
		private void __graph(){
			XMLEvent e;
			EnumMap<GRAPHAttribute, string> attributes;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "graph");

			attributes = getAttributes(typeof(GRAPHAttribute), e.asStartElement());

			if (attributes.ContainsKey(GRAPHAttribute.DEFAULTEDGETYPE)) {
				try {
					defaultEdgeType = EdgeType.valueOf(toConstantName(attributes[GRAPHAttribute.DEFAULTEDGETYPE]));
				} catch (ArgumentException ex) {
					newParseError(e, true,
							"'defaultedgetype' value should be one of 'directed', 'undirected' or 'mutual'");
				}
			}

			if (attributes.ContainsKey(GRAPHAttribute.TIMEFORMAT)) {
				try {
					timeFormat = TimeFormatType.valueOf(toConstantName(attributes[GRAPHAttribute.TIMEFORMAT]));
				} catch (ArgumentException ex) {
					newParseError(e, true,
							"'timeformat' value should be one of 'integer', 'double', 'date' or 'datetime'");
				}
			}

			e = getNextEvent();

			while (isEvent(e, XMLEvent.START_ELEMENT, "attributes")) {
				pushback(e);

				__attributes();
				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "nodes") || isEvent(e, XMLEvent.START_ELEMENT, "edges")) {
				if (isEvent(e, XMLEvent.START_ELEMENT, "nodes")) {
					pushback(e);
					__nodes();
				} else {
					pushback(e);
					__edges();
				}

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "graph");
		}

		/// <summary>
/// <pre> name 		: ATTRIBUTES attributes 	: ATTRIBUTESAttributes { CLASS!, MODE, START, STARTOPEN, END, ENDOPEN } structure 	: ATTRIBUTE * </pre>
/// </summary>
		private void __attributes(){
			XMLEvent e;
			EnumMap<ATTRIBUTESAttribute, string> attributes;
			Attribute a;
			ClassType type = null;
			Dictionary<string, Attribute> attr;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "attributes");

			attributes = getAttributes(typeof(ATTRIBUTESAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, ATTRIBUTESAttribute.CLASS);

			try {
				type = ClassType.valueOf(toConstantName(attributes[ATTRIBUTESAttribute.CLASS]));
			} catch (ArgumentException ex) {
				newParseError(e, true, "'class' value shoudl be one of 'node' or 'edge'");
			}

			if (type == ClassType.NODE)
				attr = nodeAttributesDefinition;
			else
				attr = edgeAttributesDefinition;

			e = getNextEvent();

			while (isEvent(e, XMLEvent.START_ELEMENT, "attribute")) {
				pushback(e);

				a = __attribute();
				attr[a.id] = a;
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "attributes");
		}

		/// <summary>
/// <pre> name 		: ATTRIBUTE attributes 	: ATTRIBUTEAttribute { ID, TITLE, TYPE } structure 	: ( DEFAULT | OPTIONS ) * </pre>
/// </summary>
		private Attribute __attribute(){
			XMLEvent e;
			EnumMap<ATTRIBUTEAttribute, string> attributes;
			string id, title;
			AttributeType type = null;
			Attribute theAttribute;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "attribute");

			attributes = getAttributes(typeof(ATTRIBUTEAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, ATTRIBUTEAttribute.ID, ATTRIBUTEAttribute.TITLE,
					ATTRIBUTEAttribute.TYPE);

			id = attributes[ATTRIBUTEAttribute.ID];
			title = attributes[ATTRIBUTEAttribute.TITLE];

			try {
				type = AttributeType.valueOf(toConstantName(attributes[ATTRIBUTEAttribute.TYPE]));
			} catch (ArgumentException ex) {
				newParseError(e, true,
						"'type' of attribute should be one of 'integer', 'long', 'float, 'double', 'string', 'liststring', 'anyURI' or 'bool'");
			}

			theAttribute = new Attribute(id, title, type);

			e = getNextEvent();

			while (!isEvent(e, XMLEvent.END_ELEMENT, "attribute")) {
				try {
					Balise b = Balise.valueOf(toConstantName(e.asStartElement().Name.getLocalPart()));

					pushback(e);

					switch (b) {
					case DEFAULT:
						try {
							theAttribute.setDefault(__default());
						} catch (Exception invalid) {
							newParseError(e, false, "invalid 'default' value");
						}

						break;
					case OPTIONS:
						theAttribute.setOptions(__options());
						break;
					default:
						newParseError(e, true, "attribute children should be one of 'default' or 'options'");
					}
				} catch (ArgumentException ex) {
					newParseError(e, true, "unknown element '%s'", e.asStartElement().Name.getLocalPart());
				}

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "attribute");

			return theAttribute;
		}

		/// <summary>
/// <pre> name 		: DEFAULT attributes 	: structure 	: string </pre>
/// </summary>
		private string __default(){
			string def;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "default");

			def = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "default");

			return def;
		}

		/// <summary>
/// <pre> name 		: OPTIONS attributes 	: structure 	: string </pre>
/// </summary>
		private string __options(){
			string options;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "options");

			options = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "options");

			return options;
		}

		/// <summary>
/// <pre> name 		: NODES attributes 	: NODESAttribute { 'count' } structure 	: NODE * </pre>
/// </summary>
		private void __nodes(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "nodes");

			e = getNextEvent();

			while (isEvent(e, XMLEvent.START_ELEMENT, "node")) {
				pushback(e);

				__node();
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "nodes");

		}

		/// <summary>
/// <pre> name 		: NODE attributes 	: NODEAttribute { 'pid', 'id', 'label', 'start', 'startopen', 'end', 'endopen' } structure 	: ( ATTVALUES | SPELLS | ( NODES | EDGES ) | PARENTS | ( COLOR | POSITION | SIZE | NODESHAPE ) ) * </pre>
/// </summary>
		private void __node(){
			XMLEvent e;
			EnumMap<NODEAttribute, string> attributes;
			string id;
			HashSet<string> defined = new HashSet<string>();

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "node");

			attributes = getAttributes(typeof(NODEAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, NODEAttribute.ID);

			id = attributes[NODEAttribute.ID];
			sendNodeAdded(sourceId, id);

			if (attributes.ContainsKey(NODEAttribute.LABEL))
				sendNodeAttributeAdded(sourceId, id, "label", attributes[NODEAttribute.LABEL]);

			e = getNextEvent();

			while (!isEvent(e, XMLEvent.END_ELEMENT, "node")) {
				try {
					Balise b = Balise.valueOf(toConstantName(e.asStartElement().Name.getLocalPart()));

					pushback(e);

					switch (b) {
					case ATTVALUES:
						defined.AddRange(__attvalues(ClassType.NODE, id));
						break;
					case COLOR:
						__color(ClassType.NODE, id);
						break;
					case POSITION:
						__position(id);
						break;
					case SIZE:
						__size(id);
						break;
					case SHAPE:
						__node_shape(id);
						break;
					case SPELLS:
						__spells();
						break;
					case NODES:
						__nodes();
						break;
					case EDGES:
						__edges();
						break;
					case PARENTS:
						__parents(id);
						break;
					default:
						newParseError(e, true,
								"attribute children should be one of 'attvalues', 'color', 'position', 'size', shape', 'spells', 'nodes, 'edges' or 'parents'");
					}
				} catch (ArgumentException ex) {
					newParseError(e, true, "unknown element '%s'", e.asStartElement().Name.getLocalPart());
				}

				e = getNextEvent();
			}

			foreach (Attribute theAttribute in nodeAttributesDefinition.Values) {
				if (!defined.Contains(theAttribute.id)) {
					sendNodeAttributeAdded(sourceId, id, theAttribute.title, theAttribute.def);
				}
			}

			checkValid(e, XMLEvent.END_ELEMENT, "node");
		}

		/// <summary>
/// <pre> name : ATTVALUES attributes : structure : ATTVALUE * </spell>
/// </summary>
		private HashSet<string> __attvalues(ClassType type, string elementId){
			XMLEvent e;
			HashSet<string> defined = new HashSet<string>();

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "attvalues");

			e = getNextEvent();

			while (isEvent(e, XMLEvent.START_ELEMENT, "attvalue")) {
				pushback(e);

				defined.Add(__attvalue(type, elementId));
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "attvalues");

			return defined;
		}

		/// <summary>
/// <pre> name 		: ATTVALUE attributes 	: ATTVALUEAttribute { FOR!, VALUE!, START, STARTOPEN, END, ENDOPEN } structure 	: </pre>
/// </summary>
		private string __attvalue(ClassType type, string elementId){
			XMLEvent e;
			EnumMap<ATTVALUEAttribute, string> attributes;
			Attribute theAttribute;
			object value = null;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "attvalue");

			attributes = getAttributes(typeof(ATTVALUEAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, ATTVALUEAttribute.FOR, ATTVALUEAttribute.VALUE);

			if (type == ClassType.NODE)
				theAttribute = nodeAttributesDefinition[attributes.get(ATTVALUEAttribute.FOR)];
			else
				theAttribute = edgeAttributesDefinition[attributes.get(ATTVALUEAttribute.FOR)];

			if (theAttribute == null)
				newParseError(e, false, "undefined attribute \"%s\"", attributes[ATTVALUEAttribute.FOR]);
			else {
				try {
					value = theAttribute.getValue(attributes[ATTVALUEAttribute.VALUE]);
				} catch (Exception ex) {
					newParseError(e, true, "invalid 'value' value");
				}

				switch (type) {
				case NODE:
					sendNodeAttributeAdded(sourceId, elementId, theAttribute.title, value);
					break;
				case EDGE:
					sendEdgeAttributeAdded(sourceId, elementId, theAttribute.title, value);
					break;
				}
			}

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "attvalue");

			return theAttribute == null ? null : theAttribute.id;
		}

		/// <summary>
/// <pre> name 		: SPELLS attributes 	: structure 	: SPELL + </pre>
/// </summary>
		private void __spells(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "spells");

			do {
				__spell();
				e = getNextEvent();
			} while (isEvent(e, XMLEvent.START_ELEMENT, "spell"));

			checkValid(e, XMLEvent.END_ELEMENT, "spells");
		}

		/// <summary>
/// <pre> name 		: SPELL attributes 	: SPELLAttribute structure 	: </pre>
/// </summary>
		
		private void __spell(){
			XMLEvent e;
			EnumMap<SPELLAttribute, string> attributes;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "spell");

			attributes = getAttributes(typeof(SPELLAttribute), e.asStartElement());

			// TODO Handle spell

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "spell");
		}

		/// <summary>
/// <pre> name 		: PARENTS attributes 	: structure 	: PARENT * </pre>
/// </summary>
		private void __parents(string nodeId){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "parents");

			e = getNextEvent();

			while (isEvent(e, XMLEvent.START_ELEMENT, "parent")) {
				__parent(nodeId);
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "parents");
		}

		/// <summary>
/// <pre> name 		: PARENT attributes 	: PARENTAttribute { FOR! } structure 	: </pre>
/// </summary>
		private void __parent(string nodeId){
			XMLEvent e;
			EnumMap<PARENTAttribute, string> attributes;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "parent");

			attributes = getAttributes(typeof(PARENTAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, PARENTAttribute.FOR);
			sendNodeAttributeAdded(sourceId, attributes[PARENTAttribute.FOR], "parent", nodeId);

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "parent");
		}

		/// <summary>
/// <pre> name 		: COLOR attributes 	: COLORAttribute { R!, G!, B!, A, START, STARTOPEN, END, ENDOPEN } structure 	: SPELLS ? </pre>
/// </summary>
		private void __color(ClassType type, string id){
			XMLEvent e;
			EnumMap<COLORAttribute, string> attributes;
			Color color;
			int r, g, b, a = 255;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "color");

			attributes = getAttributes(typeof(COLORAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, COLORAttribute.R, COLORAttribute.G, COLORAttribute.B);

			r = int.Parse(attributes[COLORAttribute.R]);
			g = int.Parse(attributes[COLORAttribute.G]);
			b = int.Parse(attributes[COLORAttribute.B]);

			if (attributes.ContainsKey(COLORAttribute.A))
				a = int.Parse(attributes[COLORAttribute.A]);

			color = new Color(r, g, b, a);

			switch (type) {
			case NODE:
				sendNodeAttributeAdded(sourceId, id, "ui.color", color);
				break;
			case EDGE:
				sendEdgeAttributeAdded(sourceId, id, "ui.color", color);
				break;
			}

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "spells")) {
				pushback(e);

				__spells();
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "color");
		}

		/// <summary>
/// <pre> name 		: POSITION attributes 	: POSITIONAttribute { X!, Y!, Z!, START, STARTOPEN, END, ENDOPEN } structure 	: SPELLS ? </pre>
/// </summary>
		private void __position(string nodeId){
			XMLEvent e;
			EnumMap<POSITIONAttribute, string> attributes;
			double[] xyz = { 0, 0, 0 };

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "position");

			attributes = getAttributes(typeof(POSITIONAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, POSITIONAttribute.X, POSITIONAttribute.Y, POSITIONAttribute.Z);

			xyz[0] = double.valueOf(attributes[POSITIONAttribute.X]);
			xyz[1] = double.valueOf(attributes[POSITIONAttribute.Y]);
			xyz[2] = double.valueOf(attributes[POSITIONAttribute.Z]);

			sendNodeAttributeAdded(sourceId, nodeId, "xyz", xyz);

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "spells")) {
				pushback(e);

				__spells();
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "position");
		}

		/// <summary>
/// <pre> name 		: SIZE attributes 	: SIZEAttribute { VALUE!, START, STARTOPEN, END, ENDOPEN } structure 	: SPELLS ? </pre>
/// </summary>
		private void __size(string nodeId){
			XMLEvent e;
			EnumMap<SIZEAttribute, string> attributes;
			double value;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "size");

			attributes = getAttributes(typeof(SIZEAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, SIZEAttribute.VALUE);

			value = double.valueOf(attributes[SIZEAttribute.VALUE]);

			sendNodeAttributeAdded(sourceId, nodeId, "ui.size", value);

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "spells")) {
				pushback(e);

				__spells();
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "size");
		}

		/// <summary>
/// <pre> name 		: NODESHAPE attributes 	: NODESHAPEAttributes { VALUE!, URI, START, STARTOPEN, END, ENDOPEN } structure 	: SPELLS ? </pre>
/// </summary>
		private void __node_shape(string nodeId){
			XMLEvent e;
			EnumMap<NODESHAPEAttribute, string> attributes;
			NodeShapeType type = null;
			string uri;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "shape");

			attributes = getAttributes(typeof(NODESHAPEAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, NODESHAPEAttribute.VALUE);

			try {
				type = NodeShapeType.valueOf(toConstantName(attributes[NODESHAPEAttribute.VALUE]));
			} catch (ArgumentException ex) {
				newParseError(e, true, "'value' should be one of 'disc', 'diamond', 'triangle', 'square' or 'image'");
			}

			switch (type) {
			case IMAGE:
				if (!attributes.ContainsKey(NODESHAPEAttribute.System.Uri))
					newParseError(e, true, "'image' shape type needs 'uri' attribute");

				uri = attributes[NODESHAPEAttribute.System.Uri];
				sendNodeAttributeAdded(sourceId, nodeId, "ui.style",
						string.Format("fill-mode: image-scaled; fill-image: url('{0}');", uri));

				break;
			default:
				sendNodeAttributeAdded(sourceId, nodeId, "ui.style",
						string.Format("shape {0};", type.ToString().ToLower()));
			}

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "spells")) {
				pushback(e);

				__spells();
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "shape");
		}

		/// <summary>
/// <pre> name 		: EDGES attributes 	: EDGESAttribute { 'count' } structure 	: EDGE * </pre>
/// </summary>
		private void __edges(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "edges");

			e = getNextEvent();

			while (isEvent(e, XMLEvent.START_ELEMENT, "edge")) {
				pushback(e);

				__edge();
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "edges");
		}

		/// <summary>
/// <pre> name 		: EDGE attributes 	: EDGEAttribute { START, STARTOPEN, END, ENDOPEN, ID!, TYPE, LABEL, SOURCE!, TARGET!, WEIGHT } structure 	: ( ATTVALUES | SPELLS | ( COLOR | THICKNESS | EDGESHAPE ) ) * </pre>
/// </summary>
		private void __edge(){
			XMLEvent e;
			EnumMap<EDGEAttribute, string> attributes;
			string id, source, target;
			EdgeType type = defaultEdgeType;
			HashSet<string> defined = new HashSet<string>();

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "edge");

			attributes = getAttributes(typeof(EDGEAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, EDGEAttribute.ID, EDGEAttribute.SOURCE, EDGEAttribute.TARGET);

			id = attributes[EDGEAttribute.ID];
			source = attributes[EDGEAttribute.SOURCE];
			target = attributes[EDGEAttribute.TARGET];

			if (attributes.ContainsKey(EDGEAttribute.TYPE)) {
				try {
					type = EdgeType.valueOf(toConstantName(attributes[EDGEAttribute.TYPE]));
				} catch (ArgumentException ex) {
					newParseError(e, true, "edge type should be one of 'undirected', 'undirected' or 'mutual'");
				}
			}

			switch (type) {
			case DIRECTED:
				sendEdgeAdded(sourceId, id, source, target, true);
				break;
			case MUTUAL:
			case UNDIRECTED:
				sendEdgeAdded(sourceId, id, source, target, false);
				break;
			}

			if (attributes.ContainsKey(EDGEAttribute.LABEL))
				sendEdgeAttributeAdded(sourceId, id, "ui.label", attributes[EDGEAttribute.LABEL]);

			if (attributes.ContainsKey(EDGEAttribute.WEIGHT)) {
				try {
					double d = double.valueOf(attributes[EDGEAttribute.WEIGHT]);
					sendEdgeAttributeAdded(sourceId, id, "weight", d);
				} catch (FormatException ex) {
					newParseError(e, true, "'weight' attribute of edge should be a real");
				}
			}

			e = getNextEvent();

			while (!isEvent(e, XMLEvent.END_ELEMENT, "edge")) {
				try {
					Balise b = Balise.valueOf(toConstantName(e.asStartElement().Name.getLocalPart()));

					pushback(e);

					switch (b) {
					case ATTVALUES:
						defined.AddRange(__attvalues(ClassType.EDGE, id));
						break;
					case SPELLS:
						__spells();
						break;
					case COLOR:
						__color(ClassType.EDGE, id);
						break;
					case THICKNESS:
						__thickness(id);
						break;
					case SHAPE:
						__edge_shape(id);
						break;
					default:
						newParseError(e, true,
								"edge children should be one of 'attvalues', 'color', 'thicknes', 'shape' or 'spells'");
					}
				} catch (ArgumentException ex) {
					newParseError(e, true, "unknown tag '%s'", e.asStartElement().Name.getLocalPart());
				}

				e = getNextEvent();
			}

			foreach (string key in edgeAttributesDefinition.Keys) {
				if (!defined.Contains(key))
					sendEdgeAttributeAdded(sourceId, id, key, edgeAttributesDefinition[key].def);
			}

			checkValid(e, XMLEvent.END_ELEMENT, "edge");
		}

		/// <summary>
/// <pre> name 		: EDGESHAPE attributes 	: EDGESHAPEAttributes { VALUE!, START, STARTOPEN, END, ENDOPEN } structure 	: SPELLS ? </pre>
/// </summary>
		
		private void __edge_shape(string edgeId){
			XMLEvent e;
			EnumMap<EDGESHAPEAttribute, string> attributes;
			EdgeShapeType type;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "shape");

			attributes = getAttributes(typeof(EDGESHAPEAttribute), e.asStartElement());
			checkRequiredAttributes(e, attributes, EDGESHAPEAttribute.VALUE);

			try {
				type = EdgeShapeType.valueOf(toConstantName(attributes[EDGESHAPEAttribute.VALUE]));
			} catch (ArgumentException ex) {
				newParseError(e, true, "'value' of shape should be one of 'solid', 'dotted', 'dashed' or 'double'");
			}

			// TODO Handle shape of edges

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "spells")) {
				pushback(e);

				__spells();
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "shape");
		}

		/// <summary>
/// <pre> name 		: THICKNESS attributes 	: THICKNESSAttribute { VALUE!, START, STARTOPEN, END, ENDOPEN } structure 	: SPELLS ? </pre>
/// </summary>
		private void __thickness(string edgeId){
			XMLEvent e;
			EnumMap<THICKNESSAttribute, string> attributes;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "thickness");

			attributes = getAttributes(typeof(THICKNESSAttribute), e.asStartElement());

			checkRequiredAttributes(e, attributes, THICKNESSAttribute.VALUE);

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "spells")) {
				pushback(e);

				__spells();
				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "thickness");
		}
	}

	static interface GEXFConstants {
		enum Balise {
			GEXF, GRAPH, META, CREATOR, KEYWORDS, DESCRIPTION, NODES, NODE, EDGES, EDGE, COLOR, POSITION, SIZE, SHAPE, THICKNESS, DEFAULT, OPTIONS, ATTVALUES, PARENTS, SPELLS
		}

		enum GEXFAttribute {
			XMLNS, VERSION
		}

		enum METAAttribute {
			LASTMODIFIEDDATE
		}

		enum GRAPHAttribute {
			TIMEFORMAT, START, STARTOPEN, END, ENDOPEN, DEFAULTEDGETYPE, IDTYPE, MODE
		}

		enum ATTRIBUTESAttribute {
			CLASS, MODE, START, STARTOPEN, END, ENDOPEN
		}

		enum ATTRIBUTEAttribute {
			ID, TITLE, TYPE
		}

		enum NODESAttribute {
			COUNT
		}

		enum NODEAttribute {
			START, STARTOPEN, END, ENDOPEN, PID, ID, LABEL
		}

		enum ATTVALUEAttribute {
			FOR, VALUE, START, STARTOPEN, END, ENDOPEN
		}

		enum PARENTAttribute {
			FOR
		}

		enum EDGESAttribute {
			COUNT
		}

		enum SPELLAttribute {
			START, STARTOPEN, END, ENDOPEN
		}

		enum COLORAttribute {
			R, G, B, A, START, STARTOPEN, END, ENDOPEN
		}

		enum POSITIONAttribute {
			X, Y, Z, START, STARTOPEN, END, ENDOPEN
		}

		enum SIZEAttribute {
			VALUE, START, STARTOPEN, END, ENDOPEN
		}

		enum NODESHAPEAttribute {
			VALUE, System.Uri, START, STARTOPEN, END, ENDOPEN
		}

		enum EDGEAttribute {
			START, STARTOPEN, END, ENDOPEN, ID, TYPE, LABEL, SOURCE, TARGET, WEIGHT
		}

		enum THICKNESSAttribute {
			VALUE, START, STARTOPEN, END, ENDOPEN
		}

		enum EDGESHAPEAttribute {
			VALUE, START, STARTOPEN, END, ENDOPEN
		}

		enum IDType {
			INTEGER, STRING
		}

		enum ModeType {
			STATIC, DYNAMIC
		}

		enum WeightType {
			FLOAT
		}

		enum EdgeType {
			DIRECTED, UNDIRECTED, MUTUAL
		}

		enum NodeShapeType {
			DISC, SQUARE, TRIANGLE, DIAMOND, IMAGE
		}

		enum EdgeShapeType {
			SOLID, DOTTED, DASHED, DOUBLE
		}

		enum AttributeType {
			INTEGER, LONG, FLOAT, DOUBLE, BOOLEAN, ANYURI, LISTSTRING, STRING
		}

		enum ClassType {
			NODE, EDGE
		}

		enum TimeFormatType {
			INTEGER, DOUBLE, DATE, DATETIME
		}
	}
}

}
