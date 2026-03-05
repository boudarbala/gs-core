using System.Collections.Generic;
using System.IO;
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



public class GMLParser : Parser, GMLParserConstants {
	bool inGraph = false;
	GMLContext ctx;
	bool step;

	public GMLParser(FileSourceGML gml, System.IO.Stream stream) : this(stream) {
		this.ctx = new GMLContext(gml);
	}

	public GMLParser(FileSourceGML gml, System.IO.TextReader stream) : this(stream) {
		this.ctx = new GMLContext(gml);
	}

	public bool isInGraph() {
		return inGraph;
	}

	public void open(){
	}

	public bool next(){
		KeyValues kv = null;
		kv = nextEvents();
		ctx.handleKeyValues(kv);

		return (kv != null);
	}

	public bool step(){
		KeyValues kv = null;
		step = false;

		while ((kv = nextEvents()) != null && !step)
			ctx.handleKeyValues(kv);

		if (kv != null)
			ctx.setNextStep(kv);

		return (kv != null);
	}

	/// <summary>
/// Closes the parser, closing the opened stream.
/// </summary>
	public void close(){
		jj_input_stream.Close();
	}

	/// <summary>
/// *************************************************************
/// </summary>
	/* The parser. */
	/// <summary>
/// *************************************************************
/// </summary>

	/// <summary>
/// Unused rule, call it to slurp in the whole file.
/// </summary>
	public void start(){
		list();
	}

	public void all(){
		KeyValues values = new KeyValues();
		string key;
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case GRAPH:
			graphStart();
			ctx.setIsInGraph(true);
			ctx.setDirected(false);
			break;
		case DIGRAPH:
			diGraphStart();
			ctx.setIsInGraph(true);
			ctx.setDirected(true);
			break;
		default:
			jj_la1[0] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}
		label_1: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case STRING:
			case KEY:
			case COMMENT:
				;
				break;
			default:
				jj_la1[1] = jj_gen;
				break label_1;
			}
			key = keyValue(values);
			values.key = key;
			ctx.handleKeyValues(values);
			values.Clear();
		}
		graphEnd();
		values.key = null;
		inGraph = false;
		jj_consume_token(0);
	}

	public void graphStart(){
		jj_consume_token(GRAPH);
		jj_consume_token(LSQBR);
	}

	public void diGraphStart(){
		jj_consume_token(DIGRAPH);
		jj_consume_token(LSQBR);
	}

	public void graphEnd(){
		jj_consume_token(RSQBR);
	}

	/// <summary>
/// The top-level method to be called by the file source. Returns a set of top-level key values or null if the end of the file was reached. Top-level key values are nodes and edges as well as all key-values defined before and after the graph.
/// </summary>
	public KeyValues nextEvents(){
		KeyValues values = new KeyValues();
		string key;
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case GRAPH:
			graphStart();
			values.key = null;
			ctx.setIsInGraph(true);
			ctx.setDirected(false);
			break;
		case DIGRAPH:
			diGraphStart();
			values.key = null;
			ctx.setIsInGraph(true);
			ctx.setDirected(true);
			break;
		case RSQBR:
			graphEnd();
			values.key = null;
			ctx.setIsInGraph(false);
			break;
		case STRING:
		case KEY:
		case COMMENT:
			key = keyValue(values);
			values.key = key;
			break;
		case 0:
			jj_consume_token(0);
			values = null;
			break;
		default:
			jj_la1[2] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}
		{
			if (true)
				return values;
		}
		throw new Error("Missing return statement in function");
	}

	/// <summary>
/// A list of key values, all values are stored in a KeyValues object.
/// </summary>
	public KeyValues list(){
		KeyValues values = new KeyValues();
		label_2: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case STRING:
			case KEY:
			case COMMENT:
				;
				break;
			default:
				jj_la1[3] = jj_gen;
				break label_2;
			}
			keyValue(values);
		}
		{
			if (true)
				return values;
		}
		throw new Error("Missing return statement in function");
	}

	/// <summary>
/// A set of key and value, the value can recursively be a list of key-values. Only the key-value list "graph [ ... ]" is not parsed by this rule, and parsed by another rules, so that the nextEvent() rule can be called repeatedly.
/// </summary>
	public string keyValue(KeyValues values){
		Token k;
		string key;
		object v;
		bool isGraph = false;
		label_3: while (true) {
			switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
			case COMMENT:
				;
				break;
			default:
				jj_la1[4] = jj_gen;
				break label_3;
			}
			jj_consume_token(COMMENT);
		}
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case KEY:
			k = jj_consume_token(KEY);
			key = k.image;
			if (key.Equals("step"))
				step = true;
			break;
		case STRING:
			k = jj_consume_token(STRING);
			key = k.image.Substring(1, k.image.Length - 2);
			break;
		default:
			jj_la1[5] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}
		v = value(key);
		values[key] = v;
		values.line = k.beginLine;
		values.column = k.beginColumn;
		{
			if (true)
				return key;
		}
		throw new Error("Missing return statement in function");
	}

	/// <summary>
/// A value for a key, either a number, a string or a recursive list of key-values.
/// </summary>
	public object value(string key){
		Token t;
		object val;
		KeyValues kv;
		switch ((jj_ntk == -1) ? jj_ntk() : jj_ntk) {
		case REAL:
			t = jj_consume_token(REAL);
			if (t.image.IndexOf('.') < 0)
				val = int.Parse(t.image);
			else
				val = double.valueOf(t.image);
			break;
		case STRING:
			t = jj_consume_token(STRING);
			val = t.image.Substring(1, t.image.Length - 1);
			break;
		case KEY:
			t = jj_consume_token(KEY);
			val = t.image;
			break;
		case LSQBR:
			jj_consume_token(LSQBR);
			kv = list();
			val = kv;
			jj_consume_token(RSQBR);
			break;
		default:
			jj_la1[6] = jj_gen;
			jj_consume_token(-1);
			throw new ParseException();
		}
		{
			if (true)
				return val;
		}
		throw new Error("Missing return statement in function");
	}

	/// <summary>
/// Generated Token Manager.
/// </summary>
	public GMLParserTokenManager token_source;
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
	private int jj_gen;
	private int[] jj_la1 = new int[7];
	static private int[] jj_la1_0;
	static GMLParser() {
		jj_la1_init_0();
	}

	private static void jj_la1_init_0() {
		jj_la1_0 = new int[] { 0x3000, 0xc800, 0xfa01, 0xc800, 0x8000, 0x4800, 0x4d00, };
	}

	/// <summary>
/// Constructor with InputStream.
/// </summary>
	public GMLParser(java.io.System.IO.Stream stream) : this(stream, null) {
	}

	/// <summary>
/// Constructor with InputStream and supplied encoding
/// </summary>
	public GMLParser(java.io.System.IO.Stream stream, string encoding) {
		try {
			jj_input_stream = new SimpleCharStream(stream, encoding, 1, 1);
		} catch (java.io.UnsupportedEncodingException e) {
			throw new Exception(e);
		}
		token_source = new GMLParserTokenManager(jj_input_stream);
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 7; i++)
			jj_la1[i] = -1;
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
		for (int i = 0; i < 7; i++)
			jj_la1[i] = -1;
	}

	/// <summary>
/// Constructor.
/// </summary>
	public GMLParser(java.io.System.IO.TextReader stream) {
		jj_input_stream = new SimpleCharStream(stream, 1, 1);
		token_source = new GMLParserTokenManager(jj_input_stream);
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 7; i++)
			jj_la1[i] = -1;
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
		for (int i = 0; i < 7; i++)
			jj_la1[i] = -1;
	}

	/// <summary>
/// Constructor with generated Token Manager.
/// </summary>
	public GMLParser(GMLParserTokenManager tm) {
		token_source = tm;
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 7; i++)
			jj_la1[i] = -1;
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(GMLParserTokenManager tm) {
		token_source = tm;
		token = new Token();
		jj_ntk = -1;
		jj_gen = 0;
		for (int i = 0; i < 7; i++)
			jj_la1[i] = -1;
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
			return token;
		}
		token = oldToken;
		jj_kind = kind;
		throw generateParseException();
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

	/// <summary>
/// Generate ParseException.
/// </summary>
	public ParseException generateParseException() {
		jj_expentries.Clear();
		bool[] la1tokens = new bool[16];
		if (jj_kind >= 0) {
			la1tokens[jj_kind] = true;
			jj_kind = -1;
		}
		for (int i = 0; i < 7; i++) {
			if (jj_la1[i] == jj_gen) {
				for (int j = 0; j < 32; j++) {
					if ((jj_la1_0[i] & (1 << j)) != 0) {
						la1tokens[j] = true;
					}
				}
			}
		}
		for (int i = 0; i < 16; i++) {
			if (la1tokens[i]) {
				jj_expentry = new int[1];
				jj_expentry[0] = i;
				jj_expentries.Add(jj_expentry);
			}
		}
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

}

}
