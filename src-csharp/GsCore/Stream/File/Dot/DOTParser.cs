using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File.Dot
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
/// This class defines a DOT parser. It respects the specifications of the DOT language that can be found <a href="http://www.graphviz.org/doc/info/lang.html">here</a>. Subgraph produces no error but has no effect on the graph.
/// </summary>

public class DOTParser : Parser, DOTParserConstants {
	/// <summary>
/// The DOT source associated with this parser.
/// </summary>
	private FileSourceDOT dot;

	/// <summary>
/// Id of the parser used in events.
/// </summary>
	private string sourceId;

	/// <summary>
/// Flag telling if the graph is directed.
/// </summary>
	private bool directed;

	/// <summary>
/// Flag telling if the graph is 'strict'.
/// </summary>
	private bool strict;

	/// <summary>
/// Global attributes of nodes.
/// </summary>
	private Dictionary<string, object> globalNodesAttributes;

	/// <summary>
/// Global attributes of edges.
/// </summary>
	private Dictionary<string, object> globalEdgesAttributes;

	/// <summary>
/// IDs of added nodes.
/// </summary>
	private HashSet<string> nodeAdded;

	/// <summary>
/// Create a new parser associated with a DOT source from an input stream.
/// </summary>
	public DOTParser(FileSourceDOT dot, System.IO.Stream stream) : this(stream) {
		init(dot);
	}

	/// <summary>
/// Create a new parser associated with a DOT source from a reader.
/// </summary>
	public DOTParser(FileSourceDOT dot, System.IO.TextReader stream) : this(stream) {
		init(dot);
	}

	/// <summary>
/// Closes the parser, closing the opened stream.
/// </summary>
	public void close(){
		jj_input_stream.Close();
	}

	private void init(FileSourceDOT dot) {
		this.dot = dot;
		this.sourceId = string.Format("<DOT stream {0}>", (DateTime.UtcNow.Ticks * 100L));

		globalNodesAttributes = new Dictionary<string, object>();
		globalEdgesAttributes = new Dictionary<string, object>();

		nodeAdded = new HashSet<string>();
	}

	private void addNode(string nodeId, string[] port, Dictionary<string, object> attr) {
		if (nodeAdded.Contains(nodeId)) {
			if (attr != null) {
				foreach (string key in attr.Keys)
					dot.sendAttributeChangedEvent(sourceId, nodeId, ElementType.NODE, key, AttributeChangeEvent.ADD,
							null, attr[key]);
			}
		} else {
			dot.sendNodeAdded(sourceId, nodeId);
			nodeAdded.Add(nodeId);

			if (attr == null) {
				foreach (string key in globalNodesAttributes.Keys)
					dot.sendAttributeChangedEvent(sourceId, nodeId, ElementType.NODE, key, AttributeChangeEvent.ADD,
							null, globalNodesAttributes[key]);
			} else {
				foreach (string key in globalNodesAttributes.Keys) {
					if (!attr.ContainsKey(key))
						dot.sendAttributeChangedEvent(sourceId, nodeId, ElementType.NODE, key, AttributeChangeEvent.ADD,
								null, globalNodesAttributes[key]);
				}

				foreach (string key in attr.Keys)
					dot.sendAttributeChangedEvent(sourceId, nodeId, ElementType.NODE, key, AttributeChangeEvent.ADD,
							null, attr[key]);
			}
		}
	}

	private void addEdges(List<string> edges, Dictionary<string, object> attr) {
		Dictionary<string, int> hash = new Dictionary<string, int>();
		string[] ids = new string[(edges.Count - 1) / 2];
		bool[] directed = new bool[(edges.Count - 1) / 2];
		int count = 0;

		for (int i = 0; i < edges.Count - 1; i += 2) {
			string from = edges[i];
			string to = edges[i + 2];

			if (!nodeAdded.Contains(from))
				addNode(from, null, null);
			if (!nodeAdded.Contains(to))
				addNode(to, null, null);

			string edgeId = string.Format("({0};{1})", from, to);
			string rev = string.Format("({0};{1})", to, from);

			if (hash.ContainsKey(rev)) {
				directed[hash[rev]] = false;
			} else {
				hash[edgeId] = count;
				ids[count] = edgeId;
				directed[count] = edges[i + 1].Equals("->");

				count++;
			}
		}

		hash.Clear();

		if (count == 1 && attr != null && attr.ContainsKey("id")) {
			ids[0] = attr["id"].ToString();
			attr.Remove("id");
		}

		for (int i = 0; i < count; i++) {
			bool addedEdge = false;
			string IDtoTry = ids[i];
			while (!addedEdge) {
				try {
					dot.sendEdgeAdded(sourceId, ids[i], edges[i * 2], edges[(i + 1) * 2], directed[i]);
					addedEdge = true;
				} catch (IdAlreadyInUseException e) {
					IDtoTry += "'";
				}
			}

			if (attr == null) {
				foreach (string key in globalEdgesAttributes.Keys)
					dot.sendAttributeChangedEvent(sourceId, ids[i], ElementType.EDGE, key, AttributeChangeEvent.ADD,
							null, globalEdgesAttributes[key]);
			} else {
				foreach (string key in globalEdgesAttributes.Keys) {
					if (!attr.ContainsKey(key))
						dot.sendAttributeChangedEvent(sourceId, ids[i], ElementType.EDGE, key, AttributeChangeEvent.ADD,
								null, globalEdgesAttributes[key]);
				}

				foreach (string key in attr.Keys)
					dot.sendAttributeChangedEvent(sourceId, ids[i], ElementType.EDGE, key, AttributeChangeEvent.ADD,
							null, attr[key]);
			}
		}
	}

	private void setGlobalAttributes(string who, Dictionary<string, object> attr) {
		if (who.Equals("graph")) {
			foreach (string key in attr.Keys)
				dot.sendAttributeChangedEvent(sourceId, sourceId, ElementType.GRAPH, key, AttributeChangeEvent.ADD,
						null, attr[key]);
		} else if (who.Equals("node"))
			globalNodesAttributes.putAll(attr);
		else if (who.Equals("edge"))
			globalEdgesAttributes.putAll(attr);
	}

	public void all(){
		graph();
		label_1: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case GRAPH:
			case SUBGRAPH:
			case NODE:
			case EDGE:
			case REAL:
			case STRING:
			case WORD:
				;
				break;
			default:
				jj_la1[0] = jj_gen;
				break label_1;
			}
			statement();
		}
		jj_consume_token(RBRACE);
	}

	public bool next(){
		bool hasMore = false;
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case GRAPH:
		case SUBGRAPH:
		case NODE:
		case EDGE:
		case REAL:
		case STRING:
		case WORD:
			statement();
			hasMore = true;
			break;
		case RBRACE:
			jj_consume_token(RBRACE);
			break;
		case 0:
			jj_consume_token(0);
			break;
		default:
			jj_la1[1] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}

		return hasMore;
	}

	public void open(){
		graph();
	}

	private void graph(){
		directed = false;
		strict = false;

		globalNodesAttributes.Clear();
		globalEdgesAttributes.Clear();
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case STRICT:
			jj_consume_token(STRICT);
			strict = true;
			break;
		default:
			jj_la1[2] = jj_gen;
			;
		}
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case GRAPH:
			jj_consume_token(GRAPH);
			break;
		case DIGRAPH:
			jj_consume_token(DIGRAPH);
			directed = true;
			break;
		default:
			jj_la1[3] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case REAL:
		case STRING:
		case WORD:
			this.sourceId = id();
			break;
		default:
			jj_la1[4] = jj_gen;
			;
		}
		jj_consume_token(LBRACE);
	}

	private void subgraph(){
		jj_consume_token(SUBGRAPH);
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case REAL:
		case STRING:
		case WORD:
			id();
			break;
		default:
			jj_la1[5] = jj_gen;
			;
		}
		jj_consume_token(LBRACE);
		label_2: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case GRAPH:
			case SUBGRAPH:
			case NODE:
			case EDGE:
			case REAL:
			case STRING:
			case WORD:
				;
				break;
			default:
				jj_la1[6] = jj_gen;
				break label_2;
			}
			statement();
		}
		jj_consume_token(RBRACE);
	}

	private string id(){
		Token t;
		string id;
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case STRING:
			t = jj_consume_token(STRING);
			id = t.image.Substring(1, t.image.Length - 1);
			break;
		case REAL:
			t = jj_consume_token(REAL);
			id = t.image;
			break;
		case WORD:
			t = jj_consume_token(WORD);
			id = t.image;
			break;
		default:
			jj_la1[7] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}

		return id;
	}

	private void statement(){
		if (jj_2_1(3)) {
			edgeStatement();
		} else {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case REAL:
			case STRING:
			case WORD:
				nodeStatement();
				break;
			case GRAPH:
			case NODE:
			case EDGE:
				attributeStatement();
				break;
			case SUBGRAPH:
				subgraph();
				break;
			default:
				jj_la1[8] = jj_gen;
				jj_consume_token(-1);
				throw new ParseException();
			}
		}
		jj_consume_token(27);
	}

	private void nodeStatement(){
		string nodeId;
		string[] port;
		Dictionary<string, object> attr = null;

		port = null;
		nodeId = id();
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case COLON:
			port = port();
			break;
		default:
			jj_la1[9] = jj_gen;
			;
		}
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case LSQBR:
			attr = attributesList();
			break;
		default:
			jj_la1[10] = jj_gen;
			;
		}
		addNode(nodeId, port, attr);
	}

	private string compassPoint(){
		Token pt = null;
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case 28:
			pt = jj_consume_token(28);
			break;
		case 29:
			pt = jj_consume_token(29);
			break;
		case 30:
			pt = jj_consume_token(30);
			break;
		case 31:
			pt = jj_consume_token(31);
			break;
		case 32:
			pt = jj_consume_token(32);
			break;
		case 33:
			pt = jj_consume_token(33);
			break;
		case 34:
			pt = jj_consume_token(34);
			break;
		case 35:
			pt = jj_consume_token(35);
			break;
		case 36:
			pt = jj_consume_token(36);
			break;
		case 37:
			pt = jj_consume_token(37);
			break;
		default:
			jj_la1[11] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}

		return pt.image;
	}

	private string[] port(){
		string[] p = { null, null };
		jj_consume_token(COLON);
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case REAL:
		case STRING:
		case WORD:
			p[0] = id();
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case COLON:
				jj_consume_token(COLON);
				p[1] = compassPoint();
				break;
			default:
				jj_la1[12] = jj_gen;
				;
			}
			break;
		case 28:
		case 29:
		case 30:
		case 31:
		case 32:
		case 33:
		case 34:
		case 35:
		case 36:
		case 37:
			p[1] = compassPoint();
			break;
		default:
			jj_la1[13] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}

		return p;
	}

	private void edgeStatement(){
		string id;
		List<string> edges = new List<string>();
		Dictionary<string, object> attr = null;
		id = id();
		edges.Add(id);
		edgeRHS(edges);
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case LSQBR:
			attr = attributesList();
			break;
		default:
			jj_la1[14] = jj_gen;
			;
		}
		addEdges(edges, attr);
	}

	private void edgeRHS(List<string> edges){
		Token t;
		string i;
		t = jj_consume_token(EDGE_OP);
		edges.Add(t.image);
		i = id();
		edges.Add(i);
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case EDGE_OP:
			edgeRHS(edges);
			break;
		default:
			jj_la1[15] = jj_gen;
			;
		}
	}

	private void attributeStatement(){
		Token t;
		Dictionary<string, object> attr;
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case GRAPH:
			t = jj_consume_token(GRAPH);
			break;
		case NODE:
			t = jj_consume_token(NODE);
			break;
		case EDGE:
			t = jj_consume_token(EDGE);
			break;
		default:
			jj_la1[16] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}
		attr = attributesList();
		setGlobalAttributes(t.image, attr);
	}

	private Dictionary<string, object> attributesList(){
		Dictionary<string, object> attributes = new Dictionary<string, object>();
		label_3: while (true) {
			jj_consume_token(LSQBR);
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case REAL:
			case STRING:
			case WORD:
				attributeList(attributes);
				label_4: while (true) {
					switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
					case COMMA:
						;
						break;
					default:
						jj_la1[17] = jj_gen;
						break label_4;
					}
					jj_consume_token(COMMA);
					attributeList(attributes);
				}
				break;
			default:
				jj_la1[18] = jj_gen;
				;
			}
			jj_consume_token(RSQBR);
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case LSQBR:
				;
				break;
			default:
				jj_la1[19] = jj_gen;
				break label_3;
			}
		}

		return attributes;
	}

	private void attributeList(Dictionary<string, object> attributes){
		string key;
		object val;

		Token t;
		key = id();
		val = bool.TRUE;
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case EQUALS:
			jj_consume_token(EQUALS);
			if (jj_2_2(2)) {
				t = jj_consume_token(REAL);
				val = double.Parse(t.image);
			} else {
				switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
				case REAL:
				case STRING:
				case WORD:
					val = id();
					break;
				default:
					jj_la1[20] = jj_gen;
					jj_consume_token(-1);
					throw new ParseException();
				}
			}
			break;
		default:
			jj_la1[21] = jj_gen;
			;
		}
		attributes[key] = val;
	}

	private bool jj_2_1(int xla) {
		jj_la = xla;
		jj_lastpos = jj_scanpos = token;
		try {
			return !jj_3_1();
		} catch (LookaheadSuccess ls) {
			return true;
		} finally {
			jj_save(0, xla);
		}
	}

	private bool jj_2_2(int xla) {
		jj_la = xla;
		jj_lastpos = jj_scanpos = token;
		try {
			return !jj_3_2();
		} catch (LookaheadSuccess ls) {
			return true;
		} finally {
			jj_save(1, xla);
		}
	}

	private bool jj_3R_6() {
		Token xsp;
		xsp = jj_scanpos;
		if (jj_3R_8()) {
			jj_scanpos = xsp;
			if (jj_3R_9()) {
				jj_scanpos = xsp;
				if (jj_3R_10())
					return true;
			}
		}
		return false;
	}

	private bool jj_3_2() {
		if (jj_scan_token(REAL))
			return true;
		return false;
	}

	private bool jj_3R_8() {
		if (jj_scan_token(STRING))
			return true;
		return false;
	}

	private bool jj_3R_10() {
		if (jj_scan_token(WORD))
			return true;
		return false;
	}

	private bool jj_3R_7() {
		if (jj_scan_token(EDGE_OP))
			return true;
		if (jj_3R_6())
			return true;
		return false;
	}

	private bool jj_3R_9() {
		if (jj_scan_token(REAL))
			return true;
		return false;
	}

	private bool jj_3R_5() {
		if (jj_3R_6())
			return true;
		if (jj_3R_7())
			return true;
		return false;
	}

	private bool jj_3_1() {
		if (jj_3R_5())
			return true;
		return false;
	}

	/// <summary>
/// Generated Token Manager.
/// </summary>
	public DOTParserTokenManager token_source;
	SimpleCharStream jj_input_stream;
	/// <summary>
/// Current token.
/// </summary>
	public Token token;
	/// <summary>
/// Next token.
/// </summary>
	public Token jj_nt;
	private int jj_ntk;
	private Token jj_scanpos, jj_lastpos;
	private int jj_la;
	private int jj_gen;
	private int[] jj_la1 = new int[22];
	static private int[] jj_la1_0;
	static private int[] jj_la1_1;
	static DOTParser() {
		jj_la1_init_0();
		jj_la1_init_1();
	}

	private static void jj_la1_init_0() {
		jj_la1_0 = new int[] { 0x73a0000, 0x73a2001, 0x400000, 0x60000, 0x7000000, 0x7000000, 0x73a0000, 0x7000000,
				0x73a0000, 0x4000, 0x400, 0xf0000000, 0x4000, 0xf7000000, 0x400, 0x800000, 0x320000, 0x8000, 0x7000000,
				0x400, 0x7000000, 0x10000, };
	}

	private static void jj_la1_init_1() {
		jj_la1_1 = new int[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x3f, 0x0, 0x3f, 0x0, 0x0, 0x0,
				0x0, 0x0, 0x0, 0x0, 0x0, };
	}

	private JJCalls[] jj_2_rtns = new JJCalls[2];
	private bool jj_rescan = false;
	private int jj_gc = 0;

	/// <summary>
/// Constructor with InputStream.
/// </summary>
	public DOTParser(java.io.System.IO.Stream stream) : this(stream, null) {
	}

	/// <summary>
/// Constructor with InputStream and supplied encoding
/// </summary>
	public DOTParser(java.io.System.IO.Stream stream, string encoding) {
		try {
			jj_input_stream = new SimpleCharStream(stream, encoding, 1, 1);
		} catch (java.io.UnsupportedEncodingException e) {
			throw new Exception(e);
		}
		token_source = new DOTParserTokenManager(jj_input_stream);
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 22; i++)
			jj_la1[i] = -1;
		for (int i = 0; i < jj_2_rtns.Length; i++)
			jj_2_rtns[i] = new JJCalls();
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.Stream stream) {
		ReInit(stream, null);
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.Stream stream, string encoding) {
		try {
			jj_input_stream.ReInit(stream, encoding, 1, 1);
		} catch (java.io.UnsupportedEncodingException e) {
			throw new Exception(e);
		}
		token_source.ReInit(jj_input_stream);
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 22; i++)
			jj_la1[i] = -1;
		for (int i = 0; i < jj_2_rtns.Length; i++)
			jj_2_rtns[i] = new JJCalls();
	}

	/// <summary>
/// Constructor.
/// </summary>
	public DOTParser(java.io.System.IO.TextReader stream) {
		jj_input_stream = new SimpleCharStream(stream, 1, 1);
		token_source = new DOTParserTokenManager(jj_input_stream);
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 22; i++)
			jj_la1[i] = -1;
		for (int i = 0; i < jj_2_rtns.Length; i++)
			jj_2_rtns[i] = new JJCalls();
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.TextReader stream) {
		jj_input_stream.ReInit(stream, 1, 1);
		token_source.ReInit(jj_input_stream);
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 22; i++)
			jj_la1[i] = -1;
		for (int i = 0; i < jj_2_rtns.Length; i++)
			jj_2_rtns[i] = new JJCalls();
	}

	/// <summary>
/// Constructor with generated Token Manager.
/// </summary>
	public DOTParser(DOTParserTokenManager tm) {
		token_source = tm;
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 22; i++)
			jj_la1[i] = -1;
		for (int i = 0; i < jj_2_rtns.Length; i++)
			jj_2_rtns[i] = new JJCalls();
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(DOTParserTokenManager tm) {
		token_source = tm;
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 22; i++)
			jj_la1[i] = -1;
		for (int i = 0; i < jj_2_rtns.Length; i++)
			jj_2_rtns[i] = new JJCalls();
	}

	private Token jj_consume_token(int kind){
		Token oldToken;
		if ((oldToken = token).next != null)
			token = token.next;
		else
			token = token.next = token_source.getNextToken();
		jj_ntk = -1;
		if (token.kind == kind) {
			jj_gen++;
			if (++jj_gc > 100) {
				jj_gc = 0;
				for (int i = 0; i < jj_2_rtns.Length; i++) {
					JJCalls c = jj_2_rtns[i];
					while (c != null) {
						if (c.gen < jj_gen)
							c.first = null;
						c = c.next;
					}
				}
			}
			return token;
		}
		token = oldToken;
		jj_kind = kind;
		throw generateParseException();
	}

	
	static private class LookaheadSuccess : java.lang.Error {
	}

	private LookaheadSuccess jj_ls = new LookaheadSuccess();

	private bool jj_scan_token(int kind) {
		if (jj_scanpos == jj_lastpos) {
			jj_la--;
			if (jj_scanpos.next == null) {
				jj_lastpos = jj_scanpos = jj_scanpos.next = token_source.getNextToken();
			} else {
				jj_lastpos = jj_scanpos = jj_scanpos.next;
			}
		} else {
			jj_scanpos = jj_scanpos.next;
		}
		if (jj_rescan) {
			int i = 0;
			Token tok = token;
			while (tok != null && tok != jj_scanpos) {
				i++;
				tok = tok.next;
			}
			if (tok != null)
				jj_add_error_token(kind, i);
		}
		if (jj_scanpos.kind != kind)
			return true;
		if (jj_la == 0 && jj_scanpos == jj_lastpos)
			throw jj_ls;
		return false;
	}

	/// <summary>
/// Get the next Token.
/// </summary>
	public Token getNextToken() {
		if (token.next != null)
			token = token.next;
		else
			token = token.next = token_source.getNextToken();
		jj_ntk = -1;
		jj_gen++;
		return token;
	}

	/// <summary>
/// Get the specific Token.
/// </summary>
	public Token getToken(int index) {
		Token t = token;
		for (int i = 0; i < index; i++) {
			if (t.next != null)
				t = t.next;
			else
				t = t.next = token_source.getNextToken();
		}
		return t;
	}

	private int jj_ntk() {
		if ((jj_nt = token.next) == null)
			return (jj_ntk = (token.next = token_source.getNextToken()).kind);
		else
			return (jj_ntk = jj_nt.kind);
	}

	private java.util.List<int[]> jj_expentries = new java.util.List<int[]>();
	private int[] jj_expentry;
	private int jj_kind = -1;
	private int[] jj_lasttokens = new int[100];
	private int jj_endpos;

	private void jj_add_error_token(int kind, int pos) {
		if (pos >= 100)
			return;
		if (pos == jj_endpos + 1) {
			jj_lasttokens[jj_endpos++] = kind;
		} else if (jj_endpos != 0) {
			jj_expentry = new int[jj_endpos];
			for (int i = 0; i < jj_endpos; i++) {
				jj_expentry[i] = jj_lasttokens[i];
			}
			jj_entries_loop: for (java.util.IEnumerator<object> it = jj_expentries.GetEnumerator(); it.MoveNext();) {
				int[] oldentry = (int[]) (it.next());
				if (oldentry.Length == jj_expentry.Length) {
					for (int i = 0; i < jj_expentry.Length; i++) {
						if (oldentry[i] != jj_expentry[i]) {
							continue jj_entries_loop;
						}
					}
					jj_expentries.Add(jj_expentry);
					break jj_entries_loop;
				}
			}
			if (pos != 0)
				jj_lasttokens[(jj_endpos = pos) - 1] = kind;
		}
	}

	/// <summary>
/// Generate ParseException.
/// </summary>
	public ParseException generateParseException() {
		jj_expentries.Clear();
		bool[] la1tokens = new bool[38];
		if (jj_kind >= 0) {
			la1tokens[jj_kind] = true;
			jj_kind = -1;
		}
		for (int i = 0; i < 22; i++) {
			if (jj_la1[i] == jj_gen) {
				for (int j = 0; j < 32; j++) {
					if ((jj_la1_0[i] & (1 << j)) != 0) {
						la1tokens[j] = true;
					}
					if ((jj_la1_1[i] & (1 << j)) != 0) {
						la1tokens[32 + j] = true;
					}
				}
			}
		}
		for (int i = 0; i < 38; i++) {
			if (la1tokens[i]) {
				jj_expentry = new int[1];
				jj_expentry[0] = i;
				jj_expentries.Add(jj_expentry);
			}
		}
		jj_endpos = 0;
		jj_rescan_token();
		jj_add_error_token(0, 0);
		int[][] exptokseq = new int[jj_expentries.Count][];
		for (int i = 0; i < jj_expentries.Count; i++) {
			exptokseq[i] = jj_expentries[i];
		}
		return new ParseException(token, exptokseq, tokenImage);
	}

	/// <summary>
/// Enable tracing.
/// </summary>
	public void enable_tracing() {
	}

	/// <summary>
/// Disable tracing.
/// </summary>
	public void disable_tracing() {
	}

	private void jj_rescan_token() {
		jj_rescan = true;
		for (int i = 0; i < 2; i++) {
			try {
				JJCalls p = jj_2_rtns[i];
				do {
					if (p.gen > jj_gen) {
						jj_la = p.arg;
						jj_lastpos = jj_scanpos = p.first;
						switch (i) {
						case 0:
							jj_3_1();
							break;
						case 1:
							jj_3_2();
							break;
						}
					}
					p = p.next;
				} while (p != null);
			} catch (LookaheadSuccess ls) {
			}
		}
		jj_rescan = false;
	}

	private void jj_save(int index, int xla) {
		JJCalls p = jj_2_rtns[index];
		while (p.gen > jj_gen) {
			if (p.next == null) {
				p = p.next = new JJCalls();
				break;
			}
			p = p.next;
		}
		p.gen = jj_gen + xla - jj_la;
		p.first = token;
		p.arg = xla;
	}

	static readonly class JJCalls {
		int gen;
		Token first;
		int arg;
		JJCalls next;
	}

}

}
