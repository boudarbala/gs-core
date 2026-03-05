using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File.Dgs
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
/// Type responsible for parsing files in the DGS format. <p> The DGS file format is especially designed for storing dynamic graph definitions into a file. More information about the DGS file format will be found on the GraphStream web site: <a href="http://graphstream-project.org/">http://graphstream-project.org/</a> </p> The usual file name extension used for this format is ".dgs".
/// </summary>
public class OldFileSourceDGS : FileSourceBase {
	// Attribute

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
/// An attribute set used everywhere.
/// </summary>
	protected Dictionary<string, object> attributes = new Dictionary<string, object>();

	/// <summary>
/// True as soon as the end of file is reached.
/// </summary>
	protected bool finished;

	// Construction

	/// <summary>
/// New reader for the DGS graph file format version 3.
/// </summary>
	public OldFileSourceDGS() : base(true /* EOL is significant */) {
	}

	// Command -- Parsing

	
	public bool nextEvents(){
		if (finished)
			return false;

		return next(false, false);
	}

	public bool nextStep(){
		if (finished)
			return false;

		return next(true, false);
	}

	/// <summary>
/// Read either one event or several.
/// </summary>
/// <param name="readSteps"> If true, read several events (usually starting with a step event, but it may be preceded by other events), until another step is encountered.</param>
/// <param name="stop"> If true stop at the next step encountered (and push it back so that is is readable at the next call to this method).</param>
/// <returns>True if it remains things to read.</returns>
	protected bool next(bool readSteps, bool stop){
		string key = null;
		bool loop = readSteps;

		// Sorted in probability of appearance ...

		do {
			key = getWordOrSymbolOrStringOrEolOrEof();

			if (key.Equals("ce")) {
				readCE();
			} else if (key.Equals("cn")) {
				readCN();
			} else if (key.Equals("ae")) {
				readAE();
			} else if (key.Equals("an")) {
				readAN();
			} else if (key.Equals("de")) {
				readDE();
			} else if (key.Equals("dn")) {
				readDN();
			} else if (key.Equals("cg")) {
				readCG();
			} else if (key.Equals("st")) {
				if (readSteps) {
					if (stop) {
						loop = false;
						pushBack();
					} else {
						stop = true;
						readST();
					}
				} else {
					readST();
				}
			} else if (key.Equals("#")) {
				eatAllUntilEol();
				return next(readSteps, stop);
			} else if (key.Equals("EOL")) {
				// Probably an empty line.
				// NOP
				return next(readSteps, stop);
			} else if (key.Equals("EOF")) {
				finished = true;
				return false;
			} else {
				parseError("unknown token '" + key + "'");
			}
		} while (loop);

		return true;
	}

	protected void readCE(){
		string tag = getStringOrWordOrNumber();

		readAttributes(attributes);

		foreach (string key in attributes.Keys) {
			object value = attributes[key];

			if (value == null)
				sendEdgeAttributeRemoved(graphName, tag, key);
			else
				sendEdgeAttributeChanged(graphName, tag, key, null, value);
		}

		if (eatEolOrEof() == StreamTokenizer.TT_EOF)
			pushBack();
	}

	protected void readCN(){
		string tag = getStringOrWordOrNumber();

		readAttributes(attributes);

		foreach (string key in attributes.Keys) {
			object value = attributes[key];

			if (value == null)
				sendNodeAttributeRemoved(graphName, tag, key);
			else
				sendNodeAttributeChanged(graphName, tag, key, null, value);
		}

		if (eatEolOrEof() == StreamTokenizer.TT_EOF)
			pushBack();
	}

	protected void readCG(){
		readAttributes(attributes);

		foreach (string key in attributes.Keys) {
			object value = attributes[key];

			if (value == null)
				sendGraphAttributeRemoved(graphName, key);
			else
				sendGraphAttributeChanged(graphName, key, null, value);
		}

		if (eatEolOrEof() == StreamTokenizer.TT_EOF)
			pushBack();
	}

	protected void readAE(){
		int dir = 0;
		bool directed = false;
		string dirc = null;
		string tag = null;
		string fromTag = null;
		string toTag = null;

		tag = getStringOrWordOrNumber();
		fromTag = getStringOrWordOrNumber();
		dirc = getWordOrSymbolOrNumberOrStringOrEolOrEof();

		if (dirc.Equals(">")) {
			directed = true;
			dir = 1;
		} else if (dirc.Equals("<")) {
			directed = true;
			dir = 2;
		} else {
			pushBack();
		}

		toTag = getStringOrWordOrNumber();

		if (dir == 2) {
			string tmp = toTag;
			toTag = fromTag;
			fromTag = tmp;
		}

		readAttributes(attributes);
		sendEdgeAdded(graphName, tag, fromTag, toTag, directed);

		foreach (string key in attributes.Keys) {
			object value = attributes[key];
			sendEdgeAttributeAdded(graphName, tag, key, value);
		}

		if (eatEolOrEof() == StreamTokenizer.TT_EOF)
			pushBack();
	}

	protected void readAN(){
		string tag = getStringOrWordOrNumber();

		readAttributes(attributes);

		sendNodeAdded(graphName, tag);

		foreach (string key in attributes.Keys) {
			object value = attributes[key];
			sendNodeAttributeAdded(graphName, tag, key, value);
		}

		if (eatEolOrEof() == StreamTokenizer.TT_EOF)
			pushBack();
	}

	protected void readDE(){
		string tag = getStringOrWordOrNumber();

		sendEdgeRemoved(graphName, tag);

		if (eatEolOrEof() == StreamTokenizer.TT_EOF)
			pushBack();
	}

	protected void readDN(){
		string tag = getStringOrWordOrNumber();

		sendNodeRemoved(graphName, tag);

		if (eatEolOrEof() == StreamTokenizer.TT_EOF)
			pushBack();
	}

	protected void readST(){
		string w = getWordOrNumber();

		try {
			double time = double.Parse(w);

			sendStepBegins(graphName, time);
		} catch (FormatException e) {
			parseError("expecting a number after `st', got `" + w + "'");
		}

		if (eatEolOrEof() == StreamTokenizer.TT_EOF)
			pushBack();
	}

	protected void readAttributes(Dictionary<string, object> attributes){
		bool del = false;
		string key = getWordOrSymbolOrStringOrEolOrEof();

		attributes.Clear();

		if (key.Equals("-")) {
			key = getWordOrSymbolOrStringOrEolOrEof();
			del = true;
		}

		if (key.Equals("+"))
			key = getWordOrSymbolOrStringOrEolOrEof();

		while (!key.Equals("EOF") && !key.Equals("EOL") && !key.Equals("]")) {
			if (del)
				attributes[key] = null;
			else
				attributes[key] = readAttributeValue(key);

			key = getWordOrSymbolOrStringOrEolOrEof();

			if (key.Equals("-")) {
				key = getWordOrStringOrEolOrEof();
				del = true;
			}

			if (key.Equals("+")) {
				key = getWordOrStringOrEolOrEof();
				del = false;
			}
		}

		pushBack();
	}

	/// <summary>
/// Read an attribute. The "key" (attribute name) is already read.
/// </summary>
/// <param name="key"> The attribute name, already read.</param>
	protected object readAttributeValue(string key){
		List<object> vector = null;
		object value = null;
		object value2 = null;
		string next = null;

		if (key != null)
			eatSymbols(":=");

		value = getStringOrWordOrSymbolOrNumberO();

		if (value.Equals("[")) {
			Dictionary<string, object> map = new Dictionary<string, object>();

			readAttributes(map);
			;
			eatSymbol(']');

			value = map;
		} else if (value.Equals("{")) {
			vector = readAttributeArray(key);
			eatSymbol('}');
		} else {
			pushBack();

			value = getStringOrWordOrNumberO();

			if (key != null) {
				next = getWordOrSymbolOrNumberOrStringOrEolOrEof();

				while (next.Equals(",")) {
					if (vector == null) {
						vector = new List<object>();
						vector.Add(value);
					}

					value2 = getStringOrWordOrNumberO();
					next = getWordOrSymbolOrNumberOrStringOrEolOrEof();

					vector.Add(value2);
				}

				pushBack();
			}
		}

		if (vector != null)
			return vector.ToArray();
		else
			return value;
	}

	/// <summary>
/// Read a list of values.
/// </summary>
/// <param name="key"> attribute key</param>
/// <returns>a vector</returns>
	protected List<object> readAttributeArray(string key){
		List<object> list = new List<object>();

		object value;
		string next;

		do {
			value = readAttributeValue(null);
			next = getWordOrSymbolOrNumberOrStringOrEolOrEof();

			list.Add(value);
		} while (next.Equals(","));

		pushBack();

		return list;
	}

	// Command -- Basic parsing

	
	public void begin(string filename){
		base.begin(filename);
		begin();
	}

	
	public void begin(System.Uri url){
		base.begin(url);
		begin();
	}

	
	public void begin(System.IO.Stream stream){
		base.begin(stream);
		begin();
	}

	
	public void begin(System.IO.TextReader reader){
		base.begin(reader);
		begin();
	}

	protected void begin(){
		st.parseNumbers();
		eatWords("DGS003", "DGS004");

		version = 3;

		eatEol();
		graphName = getWordOrString();
		stepCountAnnounced = (int) getNumber();// int.Parse( getWord() );
		eventCountAnnounced = (int) getNumber();// int.Parse( getWord()
												// );
		eatEol();

		if (graphName != null)
			sendGraphAttributeAdded(graphName, "label", graphName);
		else
			graphName = "DGS_";

		graphName = string.Format("{0}_{1}", graphName, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ((long) new Random().NextDouble() * 10));
	}

	
	protected void continueParsingInInclude(){
	}

	
	protected System.IO.TextReader createReaderFrom(string file){
		System.IO.Stream is = null;

		is = new FileInputStream(file);

		if (is.markSupported())
			is.mark(128);

		try {
			is = new GZIPInputStream(is);
		} catch (System.IO.IOException e1) {
			//
			// This is not a gzip input.
			// But gzip has eat some bytes so we reset the stream
			// or close and open it again.
			//
			if (is.markSupported()) {
				try {
					is.reset();
				} catch (System.IO.IOException e2) {
					//
					// Dirty but we hope do not get there
					//
					Console.Error.WriteLine(e2);
				}
			} else {
				try {
					is.Close();
				} catch (System.IO.IOException e2) {
					//
					// Dirty but we hope do not get there
					//
					Console.Error.WriteLine(e2);
				}

				is = new FileInputStream(file);
			}
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
		tok.parseNumbers();
		tok.wordChars('_', '_');
	}
}
}
