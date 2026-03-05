using System.Collections.Generic;
using System.IO;
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
/// Type responsible for parsing files in the DGS format (old versions of the format). <p> The DGS file format is especially designed for storing dynamic graph definitions into a file. More information about the DGS file format will be found on the GraphStream web site: <a href="http://graphstream-project.org/">http://graphstream-project.org/</a> </p>
/// </summary>
public class FileSourceDGS1And2 : FileSourceBase {
	// Constants

	/// <summary>
/// Types of attributes.
/// </summary>
	protected enum AttributeType {
		NUMBER, VECTOR, STRING
	}

	/// <summary>
/// Pair <name,type> defining an attribute.
/// </summary>
	class AttributeFormat {
		/// <summary>
/// Name of the attribute.
/// </summary>
		public string name;

		/// <summary>
/// Type of the attribute.
/// </summary>
		public AttributeType type;

		/// <summary>
/// New format descriptor for an attribute.
/// </summary>
/// <param name="name"> The attribute name.</param>
/// <param name="type"> The attribute type.</param>
		public AttributeFormat(string name, AttributeType type) {
			this.name = name;
			this.type = type;
		}

		/// <summary>
/// Attribute name.
/// </summary>
/// <returns>The name.</returns>
		public string getName() {
			return name;
		}

		/// <summary>
/// Attribute format.
/// </summary>
/// <returns>The format.</returns>
		public AttributeType getType() {
			return type;
		}
	}

	// Attributes

	/// <summary>
/// Format version.
/// </summary>
	protected int version;

	/// <summary>
/// Name of the graph.
/// </summary>
	protected string graphName;

	/// <summary>
/// Number of step given in the header.
/// </summary>
	protected int stepCountAnnounced;

	/// <summary>
/// Number of events given in the header.
/// </summary>
	protected int eventCountAnnounced;

	/// <summary>
/// Real number of step at current time.
/// </summary>
	protected int stepCount;

	/// <summary>
/// Real number of events at current time.
/// </summary>
	protected int eventCount;

	/// <summary>
/// Attribute count and type expected for each node add and modify command.
/// </summary>
	protected List<AttributeFormat> nodesFormat = new List<AttributeFormat>();

	/// <summary>
/// Attribute count and type expected for each edges add and modify command.
/// </summary>
	protected List<AttributeFormat> edgesFormat = new List<AttributeFormat>();

	/// <summary>
/// An attribute set.
/// </summary>
	protected Dictionary<string, object> attributes = new Dictionary<string, object>();

	// Constructors

	/// <summary>
/// New reader for the DGS graph file format versions 1 and 2.
/// </summary>
	public FileSourceDGS1And2() : base(true /* EOL is significant */) {
	}

	// Access

	// Command

	
	public bool nextEvents(){
		string key = getWordOrSymbolOrStringOrEolOrEof();
		string tag = null;

		if (key.Equals("ce")) {
			tag = getStringOrWordOrNumber();

			readAttributes(edgesFormat);

			foreach (string k in attributes.Keys) {
				object value = attributes[k];
				sendEdgeAttributeChanged(graphName, tag, k, null, value);
			}

			if (eatEolOrEof() == StreamTokenizer.TT_EOF)
				return false;
		} else if (key.Equals("cn")) {
			tag = getStringOrWordOrNumber();

			readAttributes(nodesFormat);

			foreach (string k in attributes.Keys) {
				object value = attributes[k];
				sendNodeAttributeChanged(graphName, tag, k, null, value);
			}

			if (eatEolOrEof() == StreamTokenizer.TT_EOF)
				return false;
		} else if (key.Equals("ae")) {
			tag = getStringOrWordOrNumber();
			string fromTag = getStringOrWordOrNumber();
			string toTag = getStringOrWordOrNumber();

			readAttributes(edgesFormat);

			sendEdgeAdded(graphName, tag, fromTag, toTag, false);

			if (attributes != null) {
				foreach (string k in attributes.Keys) {
					object value = attributes[k];
					sendEdgeAttributeAdded(graphName, tag, k, value);
				}
			}

			if (eatEolOrEof() == StreamTokenizer.TT_EOF)
				return false;
		} else if (key.Equals("an")) {
			tag = getStringOrWordOrNumber();

			readAttributes(nodesFormat);
			sendNodeAdded(graphName, tag);

			if (attributes != null) {
				foreach (string k in attributes.Keys) {
					object value = attributes[k];
					sendNodeAttributeAdded(graphName, tag, k, value);
				}
			}

			if (eatEolOrEof() == StreamTokenizer.TT_EOF)
				return false;
		} else if (key.Equals("de")) {
			tag = getStringOrWordOrNumber();

			sendEdgeRemoved(graphName, tag);

			if (eatEolOrEof() == StreamTokenizer.TT_EOF)
				return false;
		} else if (key.Equals("dn")) {
			tag = getStringOrWordOrNumber();

			sendNodeRemoved(graphName, tag);

			if (eatEolOrEof() == StreamTokenizer.TT_EOF)
				return false;
		} else if (key.Equals("st")) {
			string w = getWordOrNumber();

			try {
				double time = double.Parse(w);

				sendStepBegins(graphName, time);
			} catch (FormatException e) {
				parseError("expecting a number after `st', got `" + w + "'");
			}

			if (eatEolOrEof() == StreamTokenizer.TT_EOF)
				return false;
		} else if (key == "#") {
			eatAllUntilEol();
		} else if (key == "EOL") {
			return true;
		} else if (key == "EOF") {
			return false;
		} else {
			parseError("found an unknown key in file '" + key + "' (expecting an,ae,cn,ce,dn,de or st)");
		}

		return true;
	}

	/// <summary>
/// tries to read all the events between 2 steps
/// </summary>
	public bool nextStep(){
		string key = "";
		string tag = null;

		while (!key.Equals("st") && !key.Equals("EOF")) {
			key = getWordOrSymbolOrStringOrEolOrEof();

			if (key.Equals("ce")) {
				tag = getStringOrWordOrNumber();

				readAttributes(edgesFormat);

				foreach (string k in attributes.Keys) {
					object value = attributes[k];
					sendEdgeAttributeChanged(graphName, tag, k, null, value);
				}

				if (eatEolOrEof() == StreamTokenizer.TT_EOF)
					return false;
			} else if (key.Equals("cn")) {
				tag = getStringOrWordOrNumber();

				readAttributes(nodesFormat);

				foreach (string k in attributes.Keys) {
					object value = attributes[k];
					sendNodeAttributeChanged(graphName, tag, k, null, value);
				}

				if (eatEolOrEof() == StreamTokenizer.TT_EOF)
					return false;
			} else if (key.Equals("ae")) {
				tag = getStringOrWordOrNumber();
				string fromTag = getStringOrWordOrNumber();
				string toTag = getStringOrWordOrNumber();

				readAttributes(edgesFormat);
				sendEdgeAdded(graphName, tag, fromTag, toTag, false);

				if (attributes != null) {
					foreach (string k in attributes.Keys) {
						object value = attributes[k];
						sendNodeAttributeAdded(graphName, tag, k, value);
					}
				}

				if (eatEolOrEof() == StreamTokenizer.TT_EOF)
					return false;
			} else if (key.Equals("an")) {
				tag = getStringOrWordOrNumber();

				readAttributes(nodesFormat);
				sendNodeAdded(graphName, tag);

				if (attributes != null) {
					foreach (string k in attributes.Keys) {
						object value = attributes[k];
						sendNodeAttributeAdded(graphName, tag, k, value);
					}
				}

				if (eatEolOrEof() == StreamTokenizer.TT_EOF)
					return false;
			} else if (key.Equals("de")) {
				tag = getStringOrWordOrNumber();

				sendEdgeRemoved(graphName, tag);

				if (eatEolOrEof() == StreamTokenizer.TT_EOF)
					return false;
			} else if (key.Equals("dn")) {
				tag = getStringOrWordOrNumber();

				sendNodeRemoved(graphName, tag);

				if (eatEolOrEof() == StreamTokenizer.TT_EOF)
					return false;
			} else if (key.Equals("st")) {
				string w = getWordOrNumber();

				try {
					double time = double.Parse(w);
					sendStepBegins(graphName, time);
				} catch (FormatException e) {
					parseError("expecting a number after `st', got `" + w + "'");
				}

				if (eatEolOrEof() == StreamTokenizer.TT_EOF)
					return false;
			} else if (key == "#") {
				eatAllUntilEol();
			} else if (key == "EOL") {
				// NOP
			} else if (key == "EOF") {
				return false;
			} else {
				parseError("found an unknown key in file '" + key + "' (expecting an,ae,cn,ce,dn,de or st)");
			}
		}

		return true;
	}

	protected void readAttributes(List<AttributeFormat> formats){
		attributes.Clear();

		if (formats.Count > 0) {
			foreach (AttributeFormat format in formats) {
				if (format.type == AttributeType.NUMBER) {
					readNumberAttribute(format.name);
				} else if (format.type == AttributeType.VECTOR) {
					readVectorAttribute(format.name);
				} else if (format.type == AttributeType.STRING) {
					readStringAttribute(format.name);
				}
			}
		}
	}

	protected void readNumberAttribute(string name){
		int tok = st.nextToken();

		if (isNull(tok)) {
			attributes[name] = new double(0);
		} else {
			st.pushBack();

			double n = getNumber();

			attributes[name] = new double(n);
		}
	}

	protected void readVectorAttribute(string name){
		int tok = st.nextToken();

		if (isNull(tok)) {
			attributes[name] = new List<double>();
		} else {

			bool loop = true;

			List<double> vector = new List<double>();

			while (loop) {
				if (tok != StreamTokenizer.TT_NUMBER)
					parseError("expecting a number, " + gotWhat(tok));

				vector.Add(st.nval);

				tok = st.nextToken();

				if (tok != ',') {
					loop = false;
					st.pushBack();
				} else {
					tok = st.nextToken();
				}
			}

			attributes[name] = vector;
		}
	}

	protected void readStringAttribute(string name){
		string s = getStringOrWordOrNumber();

		attributes[name] = s;
	}

	protected bool isNull(int tok) {
		if (tok == StreamTokenizer.TT_WORD)
			return (st.sval.Equals("null"));

		return false;
	}

	
	public void begin(string filename){
		base.begin(filename);
		init();
	}

	
	public void begin(System.IO.Stream stream){
		base.begin(stream);
		init();
	}

	
	public void begin(System.IO.TextReader reader){
		base.begin(reader);
		init();
	}

	
	public void begin(System.Uri url){
		base.begin(url);
		init();
	}

	protected void init(){
		st.parseNumbers();

		string magic = eatOneOfTwoWords("DGS001", "DGS002");

		if (magic.Equals("DGS001"))
			version = 1;
		else
			version = 2;

		eatEol();
		graphName = getWord();
		stepCountAnnounced = (int) getNumber();// int.Parse( getWord() );
		eventCountAnnounced = (int) getNumber();// int.Parse( getWord()
												// );
		eatEol();

		if (graphName != null) {
			attributes.Clear();
			attributes["label"] = graphName;
			sendGraphAttributeAdded(graphName, "label", graphName);
		} else {
			graphName = "DGS_";
		}

		graphName = string.Format("{0}_{1}", graphName, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ((long) new Random().NextDouble() * 10));

		readAttributeFormat();
	}

	protected void readAttributeFormat(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_WORD && st.sval.Equals("nodes")) {
			parseAttributeFormat(nodesFormat);
			tok = st.nextToken();
		}

		if (tok == StreamTokenizer.TT_WORD && st.sval.Equals("edges")) {
			parseAttributeFormat(edgesFormat);
		} else {
			st.pushBack();
		}
	}

	protected void parseAttributeFormat(List<AttributeFormat> format){
		int tok = st.nextToken();

		while (tok != StreamTokenizer.TT_EOL) {
			if (tok == StreamTokenizer.TT_WORD) {
				string name = st.sval;

				eatSymbol(':');

				tok = st.nextToken();

				if (tok == StreamTokenizer.TT_WORD) {
					string type = st.sval.ToLower();

					if (type.Equals("number") || type.Equals("n")) {
						format.Add(new AttributeFormat(name, AttributeType.NUMBER));
					} else if (type.Equals("string") || type.Equals("s")) {
						format.Add(new AttributeFormat(name, AttributeType.STRING));
					} else if (type.Equals("vector") || type.Equals("v")) {
						format.Add(new AttributeFormat(name, AttributeType.VECTOR));
					} else {
						parseError("unknown attribute type `" + type
								+ "' (only `number', `vector' and `string' are accepted)");
					}
				} else {
					parseError("expecting an attribute type, got `" + gotWhat(tok) + "'");
				}
			} else {
				parseError("expecting an attribute name, got `" + gotWhat(tok) + "'");
			}

			tok = st.nextToken();
		}
	}

	
	protected void continueParsingInInclude(){
	}

	
	protected System.IO.TextReader createReaderFrom(string file){
		System.IO.Stream is = null;

		try {
			is = new GZIPInputStream(new FileInputStream(file));
		} catch (System.IO.IOException e) {
			is = new FileInputStream(file);
		}

		return new System.IO.StreamReader(new System.IO.StreamReader(is));
	}

	
	protected System.IO.TextReader createReaderFrom(System.IO.Stream stream) {

		return new System.IO.StreamReader(new System.IO.StreamReader(stream));
	}

	
	protected void configureTokenizer(StreamTokenizer tok){
		if (COMMENT_CHAR > 0)
			tok.commentChar(COMMENT_CHAR);
		// tok.quoteChar( QUOTE_CHAR );
		tok.eolIsSignificant(eol_is_significant);
		tok.wordChars('_', '_');
		tok.ordinaryChar('1');
		tok.ordinaryChar('2');
		tok.ordinaryChar('3');
		tok.ordinaryChar('4');
		tok.ordinaryChar('5');
		tok.ordinaryChar('6');
		tok.ordinaryChar('7');
		tok.ordinaryChar('8');
		tok.ordinaryChar('9');
		tok.ordinaryChar('0');
		tok.ordinaryChar('.');
		tok.ordinaryChar('-');
		tok.wordChars('1', '1');
		tok.wordChars('2', '2');
		tok.wordChars('3', '3');
		tok.wordChars('4', '4');
		tok.wordChars('5', '5');
		tok.wordChars('6', '6');
		tok.wordChars('7', '7');
		tok.wordChars('8', '8');
		tok.wordChars('9', '9');
		tok.wordChars('0', '0');
		tok.wordChars('.', '.');
		tok.wordChars('-', '-');
		// tok.parseNumbers();
	}
}
}
