using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File.Tlp
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
/// This class defines a TLP parser.
/// </summary>

public class TLPParser : Parser, TLPParserConstants {

	enum PropertyType {
		BOOL, COLOR, DOUBLE, LAYOUT, INT, SIZE, STRING
	}

	class Cluster {
		int index;
		string name;

		List<string> nodes;
		List<string> edges;

		Cluster {
			this.index = index;
			this.name = name;
			this.nodes = new List<string>();
			this.edges = new List<string>();
		}
	}

	/// <summary>
/// The DOT source associated with this parser.
/// </summary>
	private FileSourceTLP tlp;

	/// <summary>
/// Id of the parser used in events.
/// </summary>
	private string sourceId;

	private Cluster root;
	private Dictionary<int, Cluster> clusters;
	private Stack<Cluster> stack;

	/// <summary>
/// Create a new parser associated with a TLP source from an input stream.
/// </summary>
	public TLPParser(FileSourceTLP tlp, System.IO.Stream stream) : this(stream) {
		init(tlp);
	}

	/// <summary>
/// Create a new parser associated with a DOT source from a reader.
/// </summary>
	public TLPParser(FileSourceTLP tlp, System.IO.TextReader stream) : this(stream) {
		init(tlp);
	}

	/// <summary>
/// Closes the parser, closing the opened stream.
/// </summary>
	public void close(){
		jj_input_stream.Close();
		clusters.Clear();
	}

	private void init(FileSourceTLP tlp) {
		this.tlp = tlp;
		this.sourceId = string.Format("<DOT stream {0}>", (DateTime.UtcNow.Ticks * 100L));

		this.clusters = new Dictionary<int, Cluster>();
		this.stack = new Stack<Cluster>();

		this.root = new Cluster(0, "<root>");
		this.clusters[0] = this.root;
		this.stack.push(this.root);
	}

	private void addNode(string id){
		if (stack.Count > 1 && (!root.nodes.Contains(id) || !stack[stack.Count - 2].nodes.Contains(id)))
			throw new ParseException("parent cluster do not contain the node");

		if (stack.Count == 1)
			tlp.sendNodeAdded(sourceId, id);

		stack.peek().nodes.Add(id);
	}

	private void addEdge(string id, string source, string target){
		if (stack.Count > 1 && (!root.edges.Contains(id) || !stack[stack.Count - 2].edges.Contains(id)))
			throw new ParseException("parent cluster " + stack[stack.Count - 2].name + " do not contain the edge");

		if (stack.Count == 1)
			tlp.sendEdgeAdded(sourceId, id, source, target, false);

		stack.peek().edges.Add(id);
	}

	private void includeEdge(string id){
		if (stack.Count > 1 && (!root.edges.Contains(id) || !stack[stack.Count - 2].edges.Contains(id)))
			throw new ParseException("parent cluster " + stack[stack.Count - 2].name + " do not contain the edge");

		stack.peek().edges.Add(id);
	}

	private void graphAttribute(string key, object value) {
		tlp.sendAttributeChangedEvent(sourceId, sourceId, ElementType.GRAPH, key, AttributeChangeEvent.ADD, null,
				value);
	}

	private void pushCluster(int i, string name) {
		Cluster c = new Cluster(i, name);
		clusters[i] = c;
		stack.push(c);
	}

	private void popCluster() {
		if (stack.Count > 1)
			stack.pop();
	}

	private void newProperty(int cluster, string name, PropertyType type, string nodeDefault, string edgeDefault,
			Dictionary<string, string> nodes, Dictionary<string, string> edges) {
		object nodeDefaultValue = convert(type, nodeDefault);
		object edgeDefaultValue = convert(type, edgeDefault);
		Cluster c = clusters[cluster];

		foreach (string id in c.nodes) {
			object value = nodeDefaultValue;

			if (nodes.ContainsKey(id))
				value = convert(type, nodes[id]);

			tlp.sendAttributeChangedEvent(sourceId, id, ElementType.NODE, name, AttributeChangeEvent.ADD, null, value);
		}

		foreach (string id in c.edges) {
			object value = edgeDefaultValue;

			if (edges.ContainsKey(id))
				value = convert(type, edges[id]);

			tlp.sendAttributeChangedEvent(sourceId, id, ElementType.EDGE, name, AttributeChangeEvent.ADD, null, value);
		}
	}

	private object convert(PropertyType type, string value) {
		switch (type) {
		case BOOL:
			return bool.valueOf(value);
		case INT:
			return int.Parse(value);
		case DOUBLE:
			return double.valueOf(value);
		case LAYOUT:
		case COLOR:
		case SIZE:
		case STRING:
			return value;
		}

		return value;
	}

	public void all(){
		tlp();
		label_1: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case OBRACKET:
				;
				break;
			default:
				jj_la1[0] = jj_gen;
				break label_1;
			}
			statement();
		}
		jj_consume_token(CBRACKET);
		jj_consume_token(0);
	}

	public bool next(){
		bool hasMore = false;
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case OBRACKET:
			statement();
			hasMore = true;
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
		tlp();
	}

	private void tlp(){
		jj_consume_token(OBRACKET);
		jj_consume_token(TLP);
		jj_consume_token(STRING);
		label_2: while (true) {
			if (jj_2_1(2)) {
				;
			} else {
				break label_2;
			}
			headers();
		}
	}

	private void headers(){
		string s;
		jj_consume_token(OBRACKET);
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case DATE:
			jj_consume_token(DATE);
			s = string();
			graphAttribute("date", s);
			break;
		case AUTHOR:
			jj_consume_token(AUTHOR);
			s = string();
			graphAttribute("author", s);
			break;
		case COMMENTS:
			jj_consume_token(COMMENTS);
			s = string();
			graphAttribute("comments", s);
			break;
		default:
			jj_la1[2] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}
		jj_consume_token(CBRACKET);
	}

	private void statement(){
		if (jj_2_2(2)) {
			nodes();
		} else if (jj_2_3(2)) {
			edge();
		} else if (jj_2_4(2)) {
			cluster();
		} else if (jj_2_5(2)) {
			property();
		} else {
			jj_consume_token(-1);
			throw new ParseException();
		}
	}

	private void nodes(){
		Token i;
		jj_consume_token(OBRACKET);
		jj_consume_token(NODES);
		label_3: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case INTEGER:
				;
				break;
			default:
				jj_la1[3] = jj_gen;
				break label_3;
			}
			i = jj_consume_token(INTEGER);
			addNode(i.image);
		}
		jj_consume_token(CBRACKET);
	}

	private void edge(){
		Token i, s, t;
		jj_consume_token(OBRACKET);
		jj_consume_token(EDGE);
		i = jj_consume_token(INTEGER);
		s = jj_consume_token(INTEGER);
		t = jj_consume_token(INTEGER);
		jj_consume_token(CBRACKET);
		addEdge(i.image, s.image, t.image);
	}

	private void edges(){
		Token i;
		jj_consume_token(OBRACKET);
		jj_consume_token(EDGES);
		label_4: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case INTEGER:
				;
				break;
			default:
				jj_la1[4] = jj_gen;
				break label_4;
			}
			i = jj_consume_token(INTEGER);
			includeEdge(i.image);
		}
		jj_consume_token(CBRACKET);
	}

	private void cluster(){
		Token index;
		string name;
		jj_consume_token(OBRACKET);
		jj_consume_token(CLUSTER);
		index = jj_consume_token(INTEGER);
		name = string();
		pushCluster(int.Parse(index.image), name);
		nodes();
		edges();
		label_5: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case OBRACKET:
				;
				break;
			default:
				jj_la1[5] = jj_gen;
				break label_5;
			}
			cluster();
		}
		jj_consume_token(CBRACKET);
		popCluster();
	}

	private void property(){
		PropertyType type;
		int cluster;
		string name;
		string nodeDefault, edgeDefault;
		string value;
		Token t;

		Dictionary<string, string> nodes = new Dictionary<string, string>();
		Dictionary<string, string> edges = new Dictionary<string, string>();
		jj_consume_token(OBRACKET);
		jj_consume_token(PROPERTY);
		cluster = integer();
		type = type();
		name = string();
		jj_consume_token(OBRACKET);
		jj_consume_token(DEF);
		nodeDefault = string();
		edgeDefault = string();
		jj_consume_token(CBRACKET);
		label_6: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case OBRACKET:
				;
				break;
			default:
				jj_la1[6] = jj_gen;
				break label_6;
			}
			jj_consume_token(OBRACKET);
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case NODE:
				jj_consume_token(NODE);
				t = jj_consume_token(INTEGER);
				value = string();
				nodes[t.image] = value;
				break;
			case EDGE:
				jj_consume_token(EDGE);
				t = jj_consume_token(INTEGER);
				value = string();
				edges[t.image] = value;
				break;
			default:
				jj_la1[7] = jj_gen;
				jj_consume_token(-1);
				throw new ParseException();
			}
			jj_consume_token(CBRACKET);
		}
		jj_consume_token(CBRACKET);
		newProperty(cluster, name, type, nodeDefault, edgeDefault, nodes, edges);
	}

	private PropertyType type(){
		Token t;
		t = jj_consume_token(PTYPE);

		return PropertyType.valueOf(t.image.ToUpper());
	}

	private string string(){
		Token t;
		t = jj_consume_token(STRING);

		return t.image.Substring(1, t.image.Length - 1);
	}

	private int integer(){
		Token t;
		t = jj_consume_token(INTEGER);

		return int.Parse(t.image);
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

	private bool jj_2_3(int xla) {
		jj_la = xla;
		jj_lastpos = jj_scanpos = token;
		try {
			return !jj_3_3();
		} catch (LookaheadSuccess ls) {
			return true;
		} finally {
			jj_save(2, xla);
		}
	}

	private bool jj_2_4(int xla) {
		jj_la = xla;
		jj_lastpos = jj_scanpos = token;
		try {
			return !jj_3_4();
		} catch (LookaheadSuccess ls) {
			return true;
		} finally {
			jj_save(3, xla);
		}
	}

	private bool jj_2_5(int xla) {
		jj_la = xla;
		jj_lastpos = jj_scanpos = token;
		try {
			return !jj_3_5();
		} catch (LookaheadSuccess ls) {
			return true;
		} finally {
			jj_save(4, xla);
		}
	}

	private bool jj_3_1() {
		if (jj_3R_7())
			return true;
		return false;
	}

	private bool jj_3_5() {
		if (jj_3R_11())
			return true;
		return false;
	}

	private bool jj_3R_9() {
		if (jj_scan_token(OBRACKET))
			return true;
		if (jj_scan_token(EDGE))
			return true;
		return false;
	}

	private bool jj_3_4() {
		if (jj_3R_10())
			return true;
		return false;
	}

	private bool jj_3_3() {
		if (jj_3R_9())
			return true;
		return false;
	}

	private bool jj_3R_14() {
		if (jj_scan_token(COMMENTS))
			return true;
		return false;
	}

	private bool jj_3R_13() {
		if (jj_scan_token(AUTHOR))
			return true;
		return false;
	}

	private bool jj_3_2() {
		if (jj_3R_8())
			return true;
		return false;
	}

	private bool jj_3R_12() {
		if (jj_scan_token(DATE))
			return true;
		return false;
	}

	private bool jj_3R_11() {
		if (jj_scan_token(OBRACKET))
			return true;
		if (jj_scan_token(PROPERTY))
			return true;
		return false;
	}

	private bool jj_3R_10() {
		if (jj_scan_token(OBRACKET))
			return true;
		if (jj_scan_token(CLUSTER))
			return true;
		return false;
	}

	private bool jj_3R_7() {
		if (jj_scan_token(OBRACKET))
			return true;
		Token xsp;
		xsp = jj_scanpos;
		if (jj_3R_12()) {
			jj_scanpos = xsp;
			if (jj_3R_13()) {
				jj_scanpos = xsp;
				if (jj_3R_14())
					return true;
			}
		}
		return false;
	}

	private bool jj_3R_8() {
		if (jj_scan_token(OBRACKET))
			return true;
		if (jj_scan_token(NODES))
			return true;
		return false;
	}

	/// <summary>
/// Generated Token Manager.
/// </summary>
	public TLPParserTokenManager token_source;
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
	private int[] jj_la1 = new int[8];
	static private int[] jj_la1_0;
	static TLPParser() {
		jj_la1_init_0();
	}

	private static void jj_la1_init_0() {
		jj_la1_0 = new int[] { 0x400, 0x401, 0x380000, 0x1000000, 0x1000000, 0x400, 0x400, 0x14000, };
	}

	private JJCalls[] jj_2_rtns = new JJCalls[5];
	private bool jj_rescan = false;
	private int jj_gc = 0;

	/// <summary>
/// Constructor with InputStream.
/// </summary>
	public TLPParser(java.io.System.IO.Stream stream) : this(stream, null) {
	}

	/// <summary>
/// Constructor with InputStream and supplied encoding
/// </summary>
	public TLPParser(java.io.System.IO.Stream stream, string encoding) {
		try {
			jj_input_stream = new SimpleCharStream(stream, encoding, 1, 1);
		} catch (java.io.UnsupportedEncodingException e) {
			throw new Exception(e);
		}
		token_source = new TLPParserTokenManager(jj_input_stream);
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 8; i++)
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
		for (int i = 0; i < 8; i++)
			jj_la1[i] = -1;
		for (int i = 0; i < jj_2_rtns.Length; i++)
			jj_2_rtns[i] = new JJCalls();
	}

	/// <summary>
/// Constructor.
/// </summary>
	public TLPParser(java.io.System.IO.TextReader stream) {
		jj_input_stream = new SimpleCharStream(stream, 1, 1);
		token_source = new TLPParserTokenManager(jj_input_stream);
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 8; i++)
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
		for (int i = 0; i < 8; i++)
			jj_la1[i] = -1;
		for (int i = 0; i < jj_2_rtns.Length; i++)
			jj_2_rtns[i] = new JJCalls();
	}

	/// <summary>
/// Constructor with generated Token Manager.
/// </summary>
	public TLPParser(TLPParserTokenManager tm) {
		token_source = tm;
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 8; i++)
			jj_la1[i] = -1;
		for (int i = 0; i < jj_2_rtns.Length; i++)
			jj_2_rtns[i] = new JJCalls();
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(TLPParserTokenManager tm) {
		token_source = tm;
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 8; i++)
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
		private static readonly long serialVersionUID = -7986896058452164869L;
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
		bool[] la1tokens = new bool[28];
		if (jj_kind >= 0) {
			la1tokens[jj_kind] = true;
			jj_kind = -1;
		}
		for (int i = 0; i < 8; i++) {
			if (jj_la1[i] == jj_gen) {
				for (int j = 0; j < 32; j++) {
					if ((jj_la1_0[i] & (1 << j)) != 0) {
						la1tokens[j] = true;
					}
				}
			}
		}
		for (int i = 0; i < 28; i++) {
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
		for (int i = 0; i < 5; i++) {
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
						case 2:
							jj_3_3();
							break;
						case 3:
							jj_3_4();
							break;
						case 4:
							jj_3_5();
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
