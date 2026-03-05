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
/// Base for various graph file input. <p> This class is a piece of crap. However it is still used in many places... :-( TODO use a parser generator to replace it. </p> <p> This class provides parsing utilities to help the creation of new graph readers/parsers. It handles a stack of input files that allow to easily : "includes" (that is interrupting the parsing of a file to input another one). It wraps stream tokenizers allowing to eat or get specific token types easily. </p> <p> It is well suited for graph formats using text (not binary), but not for XML based files where a real XML parser would probably be better. </p>
/// </summary>
abstract class FileSourceBase : SourceBase, IFileSource {
	// Attributes

	/// <summary>
/// The quote character. Can be changed in descendants.
/// </summary>
	protected int QUOTE_CHAR = '"';

	/// <summary>
/// The comment character. Can be changed in descendants.
/// </summary>
	protected int COMMENT_CHAR = '#';

	/// <summary>
/// Is EOL significant?.
/// </summary>
	protected bool eol_is_significant = false;

	/// <summary>
/// Stack of tokenizers/filenames. Each tokenizer is open on a file. When an include is found, the current tokenizer is pushed on the stack and a new one for the included file is created. Once the included file is parsed, the tokenizer is popped of the stack and the previous one is used.
/// </summary>
	protected List<CurrentFile> tok_stack = new List<CurrentFile>();

	/// <summary>
/// Current tokenizer.
/// </summary>
	protected StreamTokenizer st;

	/// <summary>
/// Current file name.
/// </summary>
	protected string filename;

	/// <summary>
/// Map of unknown attributes to corresponding classes.
/// </summary>
	protected Dictionary<string, string> attribute_classes = new Dictionary<string, string>();

	// Constructors

	/// <summary>
/// No-op constructor.
/// </summary>
	protected FileSourceBase() {
	}

	/// <summary>
/// Setup the reader End-Of-Line policy.
/// </summary>
/// <param name="eol_is_significant"> If true EOL will be returned as a token, else it is ignored.</param>
	protected FileSourceBase(bool eol_is_significant) {
		this.eol_is_significant = eol_is_significant;
	}

	/// <summary>
/// Setup the reader End-Of-Line policy and specific comment and quote characters.
/// </summary>
/// <param name="eol_is_significant"> If true EOL will be returned as a token, else it is ignored.</param>
/// <param name="commentChar"> char used for one line comments.</param>
/// <param name="quoteChar"> char used to enclose quotations.</param>
	protected FileSourceBase(bool eol_is_significant, int commentChar, int quoteChar) {
		this.eol_is_significant = eol_is_significant;

		this.COMMENT_CHAR = commentChar;
		this.QUOTE_CHAR = quoteChar;
	}

	// Access

	// Command -- Complete modeField.

	public void readAll(string filename){
		begin(filename);
		while (nextEvents())
			;
		end();
	}

	public void readAll(System.Uri url){
		begin(url);
		while (nextEvents())
			;
		end();
	}

	public void readAll(System.IO.Stream stream){
		begin(stream);
		while (nextEvents())
			;
		end();
	}

	public void readAll(System.IO.TextReader reader){
		begin(reader);
		while (nextEvents())
			;
		end();
	}

	// Commands -- By-event modeField.

	public void begin(string filename){
		pushTokenizer(filename);
	}

	public void begin(System.IO.Stream stream){
		pushTokenizer(stream);
	}

	public void begin(System.Uri url){
		pushTokenizer(url);
	}

	public void begin(System.IO.TextReader reader){
		pushTokenizer(reader);
	}

	public abstract bool nextEvents();

	public void end(){
		popTokenizer();
	}

	// Command

	/// <summary>
/// Declare that when <code>attribute</code> is found, the corresponding <code>attribute_class</code> must be instantiated and inserted in the current element being parsed. This is equivalent to the "map" keyword of the GML file. An attribute appears in a GML file as a name followed by a "[...]" block. The contents of this block defines sub-attributes that must map to public fields of the attribute. Only attributes that are not handled specifically by this parser can be added.
/// </summary>
/// <param name="attribute"> must name the attribute.</param>
/// <param name="attribute_class"> must be the complete name of a Java class that will represent the attribute.</param>
	public void addAttributeClass(string attribute, string attribute_class) {
		attribute_classes[attribute] = attribute_class;
	}

	// Command -- Parsing -- Include mechanism

	/// <summary>
/// Include the content of a <code>file</code>. This pushes a new tokenizer on the input stack, calls the {@link #continueParsingInInclude()} method (that must be implemented to read the include contents) and when finished pops the tokenizer of the input stack.
/// </summary>
	protected void include(string file){
		pushTokenizer(file);
		continueParsingInInclude();
		popTokenizer();
	}

	/// <summary>
/// Must be implemented to read the content of an include. The current tokenizer will be set to the included file. When this method returns, the include file will be closed an parsing will continue where it was before inclusion.
/// </summary>
	protected abstract void continueParsingInInclude();

	/// <summary>
/// Push a tokenizer created from a file name on the file stack and make it current.
/// </summary>
/// <param name="file"> Name of the file used as source for the tokenizer.</param>
	protected void pushTokenizer(string file){
		StreamTokenizer tok;
		CurrentFile cur;
		System.IO.TextReader reader;

		try {
			reader = createReaderFrom(file);
			tok = createTokenizer(reader);

			cur = new CurrentFile(file, tok, reader);
		} catch (FileNotFoundException e) {
			throw new System.IO.IOException("cannot read file '" + file + "', not found: " + e.getMessage());
		}

		configureTokenizer(tok);
		tok_stack.Add(cur);

		st = tok;
		filename = file;
	}

	/// <summary>
/// Create a reader for by the tokenizer. If the given file does not exist or un readable.
/// </summary>
/// <param name="file"> File name to be opened.</param>
/// <returns>a reader for the tokenizer.</returns>
	protected System.IO.TextReader createReaderFrom(string file){
		return new System.IO.StreamReader(new System.IO.StreamReader(file));
	}

	/// <summary>
/// Create a stream that can be read by the tokenizer.
/// </summary>
/// <param name="stream"> Input stream to be open as a reader.</param>
/// <returns>a reader for the tokenizer.</returns>
	protected System.IO.TextReader createReaderFrom(System.IO.Stream stream) {
		return new System.IO.StreamReader(new System.IO.StreamReader(stream));
	}

	/// <summary>
/// Push a tokenizer created from a stream on the file stack and make it current.
/// </summary>
/// <param name="url"> The URL used as source for the tokenizer.</param>
	protected void pushTokenizer(System.Uri url){
		pushTokenizer(url.openStream(), url.ToString());
	}

	/// <summary>
/// Push a tokenizer created from a stream on the file stack and make it current.
/// </summary>
/// <param name="stream"> The stream used as source for the tokenizer.</param>
	protected void pushTokenizer(System.IO.Stream stream){
		pushTokenizer(stream, "<?input-stream?>");
	}

	/// <summary>
/// Push a tokenizer created from a stream on the file stack and make it current.
/// </summary>
/// <param name="stream"> The stream used as source for the tokenizer.</param>
/// <param name="name"> The name of the input stream.</param>
	protected void pushTokenizer(System.IO.Stream stream, string name){
		StreamTokenizer tok;
		CurrentFile cur;
		System.IO.TextReader reader;

		reader = createReaderFrom(stream);
		tok = createTokenizer(reader);
		cur = new CurrentFile(name, tok, reader);

		configureTokenizer(tok);
		tok_stack.Add(cur);

		st = tok;
		filename = name;
	}

	/// <summary>
/// Push a tokenizer created from a reader on the file stack and make it current.
/// </summary>
/// <param name="reader"> The reader used as source for the tokenizer.</param>
	protected void pushTokenizer(System.IO.TextReader reader){
		StreamTokenizer tok;
		CurrentFile cur;

		tok = createTokenizer(reader);
		cur = new CurrentFile("<?reader?>", tok, reader);
		configureTokenizer(tok);
		tok_stack.Add(cur);

		st = tok;
		filename = "<?reader?>";

	}

	/// <summary>
/// Create a tokenizer from an input source. For any I/O error.
/// </summary>
/// <param name="reader"> The reader.</param>
/// <returns>The new tokenizer.</returns>
	private StreamTokenizer createTokenizer(System.IO.TextReader reader){
		return new StreamTokenizer(new System.IO.StreamReader(reader));
	}

	/// <summary>
/// Method to override to configure the tokenizer behaviour. It is called each time a tokenizer is created (for the parsed file and all included files).
/// </summary>
	protected void configureTokenizer(StreamTokenizer tok){
		if (COMMENT_CHAR > 0)
			tok.commentChar(COMMENT_CHAR);
		tok.quoteChar(QUOTE_CHAR);
		tok.eolIsSignificant(eol_is_significant);
		tok.wordChars('_', '_');
		tok.parseNumbers();
	}

	/// <summary>
/// Remove the current tokenizer from the stack and restore the previous one (if any).
/// </summary>
	protected void popTokenizer(){
		int n = tok_stack.Count;

		if (n <= 0)
			throw new Exception("poped one too many tokenizer");

		n -= 1;

		CurrentFile cur = tok_stack.Remove(n);
		cur.reader.Close();

		if (n > 0) {
			n -= 1;

			cur = tok_stack[n];

			st = cur.tok;
			filename = cur.file;
		}
	}

	// Low level parsing

	/// <summary>
/// Push back the last read thing, so that it can be read anew. This allows to explore one token ahead, and if not corresponding to what is expected, go back.
/// </summary>
	protected void pushBack() {
		st.pushBack();
	}

	/// <summary>
/// Read EOF or report garbage at end of file.
/// </summary>
	protected void eatEof(){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_EOF)
			parseError("garbage at end of file, expecting EOF, " + gotWhat(tok));
	}

	/// <summary>
/// Read EOL.
/// </summary>
	protected void eatEol(){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_EOL)
			parseError("expecting EOL, " + gotWhat(tok));
	}

	/// <summary>
/// Read EOL or EOF.
/// </summary>
/// <returns>The token read StreamTokenizer.TT_EOL or StreamTokenizer.TT_EOF.</returns>
	protected int eatEolOrEof(){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_EOL && tok != StreamTokenizer.TT_EOF)
			parseError("expecting EOL or EOF, " + gotWhat(tok));

		return tok;
	}

	/// <summary>
/// Read an expected <code>word</code> token or generate a parse error.
/// </summary>
	protected void eatWord(string word){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_WORD)
			parseError("expecting `" + word + "', " + gotWhat(tok));

		if (!st.sval.Equals(word))
			parseError("expecting `" + word + "' got `" + st.sval + "'");
	}

	/// <summary>
/// Read an expected word among the given word list or generate a parse error.
/// </summary>
/// <param name="words"> The expected words.</param>
	protected void eatWords(params string[] words){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_WORD)
			parseError("expecting one of `[" + Arrays.toString(words) + "]', " + gotWhat(tok));

		bool found = false;

		foreach (string word in words) {
			if (st.sval.Equals(word)) {
				found = true;
				break;
			}
		}

		if (!found)
			parseError("expecting one of `[" + Arrays.toString(words) + "]', got `" + st.sval + "'");
	}

	/// <summary>
/// Eat either a word or another, and return the eated one.
/// </summary>
/// <param name="word1"> The first word to eat.</param>
/// <param name="word2"> The alternative word to eat.</param>
/// <returns>The word eaten.</returns>
	protected string eatOneOfTwoWords(string word1, string word2){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_WORD)
			parseError("expecting `" + word1 + "' or  `" + word2 + "', " + gotWhat(tok));

		if (st.sval.Equals(word1))
			return word1;

		if (st.sval.Equals(word2))
			return word2;

		parseError("expecting `" + word1 + "' or `" + word2 + "' got `" + st.sval + "'");
		return null;
	}

	/// <summary>
/// Eat the expected symbol or generate a parse error.
/// </summary>
	protected void eatSymbol(char symbol){
		int tok = st.nextToken();

		if (tok != symbol)
			parseError("expecting symbol `" + symbol + "', " + gotWhat(tok));
	}

	/// <summary>
/// Eat one of the list of expected <code>symbols</code> or generate a parse error none of <code>symbols</code> can be found.
/// </summary>
	protected int eatSymbols(string symbols){
		int tok = st.nextToken();
		int n = symbols.Length;
		bool f = false;

		for (int i = 0; i < n; ++i) {
			if (tok == symbols[i]) {
				f = true;
				i = n;
			}
		}

		if (!f)
			parseError("expecting one of symbols `" + symbols + "', " + gotWhat(tok));

		return tok;
	}

	/// <summary>
/// Eat the expected <code>word</code> or push back what was read so that it can be read anew.
/// </summary>
	protected void eatWordOrPushbak(string word){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_WORD)
			pushBack();

		if (!st.sval.Equals(word))
			pushBack();
	}

	/// <summary>
/// Eat the expected <code>symbol</code> or push back what was read so that it can be read anew.
/// </summary>
	protected void eatSymbolOrPushback(char symbol){
		int tok = st.nextToken();

		if (tok != symbol)
			pushBack();
	}

	/// <summary>
/// Eat all until an EOL is found. The EOL is also eaten. This works only if EOL is significant (else it does nothing).
/// </summary>
	protected void eatAllUntilEol(){
		if (!eol_is_significant)
			return;

		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_EOF)
			return;

		while ((tok != StreamTokenizer.TT_EOL) && (tok != StreamTokenizer.TT_EOF)) {
			tok = st.nextToken();
		}
	}

	/// <summary>
/// Eat all availables EOLs.
/// </summary>
	protected void eatAllEols(){
		if (!eol_is_significant)
			return;

		int tok = st.nextToken();

		while (tok == StreamTokenizer.TT_EOL)
			tok = st.nextToken();

		pushBack();
	}

	/// <summary>
/// Read a word or generate a parse error.
/// </summary>
	protected string getWord(){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_WORD)
			parseError("expecting a word, " + gotWhat(tok));

		return st.sval;
	}

	/// <summary>
/// Get a symbol.
/// </summary>
	protected char getSymbol(){
		int tok = st.nextToken();

		if (tok > 0 && tok != StreamTokenizer.TT_WORD && tok != StreamTokenizer.TT_NUMBER
				&& tok != StreamTokenizer.TT_EOL && tok != StreamTokenizer.TT_EOF && tok != QUOTE_CHAR
				&& tok != COMMENT_CHAR) {
			return (char) tok;
		}

		parseError("expecting a symbol, " + gotWhat(tok));
		return (char) 0; // Never reached.
	}

	/// <summary>
/// Get a symbol or push back what was read so that it can be read anew. If no symbol is found, 0 is returned.
/// </summary>
	protected char getSymbolOrPushback(){
		int tok = st.nextToken();

		if (tok > 0 && tok != StreamTokenizer.TT_WORD && tok != StreamTokenizer.TT_NUMBER
				&& tok != StreamTokenizer.TT_EOL && tok != StreamTokenizer.TT_EOF && tok != QUOTE_CHAR
				&& tok != COMMENT_CHAR) {
			return (char) tok;
		}

		pushBack();

		return (char) 0;
	}

	/// <summary>
/// Read a string constant (between quotes) or generate a parse error. Return the content of the string without the quotes.
/// </summary>
	protected string getString(){
		int tok = st.nextToken();

		if (tok != QUOTE_CHAR)
			parseError("expecting a string constant, " + gotWhat(tok));

		return st.sval;
	}

	/// <summary>
/// Read a word or number or generate a parse error. If it is a number it is converted to a string before being returned.
/// </summary>
	protected string getWordOrNumber(){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_WORD && tok != StreamTokenizer.TT_NUMBER)
			parseError("expecting a word or number, " + gotWhat(tok));

		if (tok == StreamTokenizer.TT_NUMBER) {
			// If st.nval is an integer, as it is stored into a double,
			// toString() will transform it by automatically adding ".0", we
			// prevent this. The tokenizer does not allow to read integers.

			if ((st.nval - ((int) st.nval)) == 0)
				return int.toString((int) st.nval);
			else
				return double.toString(st.nval);
		} else {
			return st.sval;
		}
	}

	/// <summary>
/// Read a string or number or generate a parse error. If it is a number it is converted to a string before being returned.
/// </summary>
	protected string getStringOrNumber(){
		int tok = st.nextToken();

		if (tok != QUOTE_CHAR && tok != StreamTokenizer.TT_NUMBER)
			parseError("expecting a string constant or a number, " + gotWhat(tok));

		if (tok == StreamTokenizer.TT_NUMBER) {
			if ((st.nval - ((int) st.nval)) == 0)
				return int.toString((int) st.nval);
			else
				return double.toString(st.nval);
		} else {
			return st.sval;
		}
	}

	/// <summary>
/// Read a string or number or pushback and return null. If it is a number it is converted to a string before being returned.
/// </summary>
	protected string getStringOrWordOrNumberOrPushback(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_EOL || tok == StreamTokenizer.TT_EOF) {
			pushBack();
			return null;
		}

		if (tok == StreamTokenizer.TT_NUMBER) {
			if ((st.nval - ((int) st.nval)) == 0)
				return int.toString((int) st.nval);
			else
				return double.toString(st.nval);
		} else if (tok == StreamTokenizer.TT_WORD || tok == QUOTE_CHAR) {
			return st.sval;
		} else {
			pushBack();
			return null;
		}
	}

	/// <summary>
/// Read a string or number or generate a parse error. If it is a number it is converted to a string before being returned.
/// </summary>
	protected string getStringOrWordOrNumber(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_EOL || tok == StreamTokenizer.TT_EOF)
			parseError("expecting word, string or number, " + gotWhat(tok));

		if (tok == StreamTokenizer.TT_NUMBER) {
			if ((st.nval - ((int) st.nval)) == 0)
				return int.toString((int) st.nval);
			else
				return double.toString(st.nval);
		} else {
			return st.sval;
		}
	}

	/// <summary>
/// Read a string or number or generate a parse error. The returned value is converted to a Number of a String depending on its type.
/// </summary>
	protected object getStringOrWordOrNumberO(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_EOL || tok == StreamTokenizer.TT_EOF)
			parseError("expecting word, string or number, " + gotWhat(tok));

		if (tok == StreamTokenizer.TT_NUMBER) {
			return st.nval;
		} else {
			return st.sval;
		}
	}

	/// <summary>
/// Read a string or number or generate a parse error. The returned value is converted to a Number of a String depending on its type.
/// </summary>
	protected object getStringOrWordOrSymbolOrNumberO(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_EOL || tok == StreamTokenizer.TT_EOF)
			parseError("expecting word, string or number, " + gotWhat(tok));

		if (tok == StreamTokenizer.TT_NUMBER) {
			return st.nval;
		} else if (tok == StreamTokenizer.TT_WORD) {
			return st.sval;
		} else
			return char.toString((char) tok);
	}

	/// <summary>
/// Read a word or string or generate a parse error.
/// </summary>
	protected string getWordOrString(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_WORD || tok == QUOTE_CHAR)
			return st.sval;

		parseError("expecting a word or string, " + gotWhat(tok));
		return null;
	}

	/// <summary>
/// Read a word or symbol or generate a parse error.
/// </summary>
	protected string getWordOrSymbol(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_NUMBER || tok == QUOTE_CHAR || tok == StreamTokenizer.TT_EOF)
			parseError("expecting a word or symbol, " + gotWhat(tok));

		if (tok == StreamTokenizer.TT_WORD)
			return st.sval;
		else
			return char.toString((char) tok);
	}

	/// <summary>
/// Read a word or symbol or push back the read thing so that it is readable anew. In the second case, null is returned.
/// </summary>
	protected string getWordOrSymbolOrPushback(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_NUMBER || tok == QUOTE_CHAR || tok == StreamTokenizer.TT_EOF) {
			pushBack();
			return null;
		}

		if (tok == StreamTokenizer.TT_WORD)
			return st.sval;
		else
			return char.toString((char) tok);
	}

	/// <summary>
/// Read a word or symbol or string or generate a parse error.
/// </summary>
	protected string getWordOrSymbolOrString(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_NUMBER || tok == StreamTokenizer.TT_EOF)
			parseError("expecting a word, symbol or string, " + gotWhat(tok));

		if (tok == QUOTE_CHAR)
			return st.sval;

		if (tok == StreamTokenizer.TT_WORD)
			return st.sval;
		else
			return char.toString((char) tok);
	}

	/// <summary>
/// Read a word or symbol or string or number or generate a parse error.
/// </summary>
	protected string getAllExceptedEof(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_EOF)
			parseError("expecting all excepted EOF, " + gotWhat(tok));

		if (tok == StreamTokenizer.TT_NUMBER || tok == StreamTokenizer.TT_EOF) {
			if ((st.nval - ((int) st.nval)) == 0)
				return int.toString((int) st.nval);
			else
				return double.toString(st.nval);
		}

		if (tok == QUOTE_CHAR)
			return st.sval;

		if (tok == StreamTokenizer.TT_WORD)
			return st.sval;
		else
			return char.toString((char) tok);
	}

	/// <summary>
/// Read a word, a symbol or EOF, or generate a parse error. If this is EOF, the string "EOF" is returned.
/// </summary>
	protected string getWordOrSymbolOrEof(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_NUMBER || tok == QUOTE_CHAR)
			parseError("expecting a word or symbol, " + gotWhat(tok));

		if (tok == StreamTokenizer.TT_WORD)
			return st.sval;
		else if (tok == StreamTokenizer.TT_EOF)
			return "EOF";
		else
			return char.toString((char) tok);
	}

	/// <summary>
/// Read a word or symbol or string or EOL/EOF or generate a parse error. If EOL is read the "EOL" string is returned. If EOF is read the "EOF" string is returned.
/// </summary>
/// <returns>A string.</returns>
	protected string getWordOrSymbolOrStringOrEolOrEof(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_NUMBER)
			parseError("expecting a word, symbol or string, " + gotWhat(tok));

		if (tok == QUOTE_CHAR)
			return st.sval;

		if (tok == StreamTokenizer.TT_WORD)
			return st.sval;

		if (tok == StreamTokenizer.TT_EOF)
			return "EOF";

		if (tok == StreamTokenizer.TT_EOL)
			return "EOL";

		return char.toString((char) tok);
	}

	/// <summary>
/// Read a word or number or string or EOL/EOF or generate a parse error. If EOL is read the "EOL" string is returned. If EOF is read the "EOF" string is returned. If a number is returned, it is converted to a string as follows: if it is an integer, only the integer part is converted to a string without dot or comma and no leading zeros. If it is a float the fractional part is also converted and the dot is used as separator.
/// </summary>
/// <returns>A string.</returns>
	protected string getWordOrNumberOrStringOrEolOrEof(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_NUMBER) {
			if (st.nval - ((int) st.nval) != 0)
				return double.toString(st.nval);

			return int.toString((int) st.nval);
		}

		if (tok == QUOTE_CHAR)
			return st.sval;

		if (tok == StreamTokenizer.TT_WORD)
			return st.sval;

		if (tok == StreamTokenizer.TT_EOF)
			return "EOF";

		if (tok == StreamTokenizer.TT_EOL)
			return "EOL";

		parseError("expecting a word, a number, a string, EOL or EOF, " + gotWhat(tok));
		return null; // Never happen, parseError throws unconditionally an
						// exception.
	}

	/// <summary>
/// Read a word or string or EOL/EOF or generate a parse error. If EOL is read the "EOL" string is returned. If EOF is read the "EOF" string is returned.
/// </summary>
/// <returns>A string.</returns>
	protected string getWordOrStringOrEolOrEof(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_WORD)
			return st.sval;

		if (tok == QUOTE_CHAR)
			return st.sval;

		if (tok == StreamTokenizer.TT_EOL)
			return "EOL";

		if (tok == StreamTokenizer.TT_EOF)
			return "EOF";

		parseError("expecting a word, a string, EOL or EOF, " + gotWhat(tok));
		return null; // Never happen, parseError throws unconditionally an
						// exception.
	}

	// Order: Word | String | Symbol | Number | Eol | Eof

	/// <summary>
/// Read a word or number or string or EOL/EOF or generate a parse error. If EOL is read the "EOL" string is returned. If EOF is read the "EOF" string is returned. If a number is returned, it is converted to a string as follows: if it is an integer, only the integer part is converted to a string without dot or comma and no leading zeros. If it is a float the fractional part is also converted and the dot is used as separator.
/// </summary>
/// <returns>A string.</returns>
	protected string getWordOrSymbolOrNumberOrStringOrEolOrEof(){
		int tok = st.nextToken();

		if (tok == StreamTokenizer.TT_NUMBER) {
			if (st.nval - ((int) st.nval) != 0)
				return double.toString(st.nval);

			return int.toString((int) st.nval);
		}

		if (tok == QUOTE_CHAR)
			return st.sval;

		if (tok == StreamTokenizer.TT_WORD)
			return st.sval;

		if (tok == StreamTokenizer.TT_EOF)
			return "EOF";

		if (tok == StreamTokenizer.TT_EOL)
			return "EOL";

		return char.toString((char) tok);
	}

	/// <summary>
/// Read a number or generate a parse error.
/// </summary>
	protected double getNumber(){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_NUMBER)
			parseError("expecting a number, " + gotWhat(tok));

		return st.nval;
	}

	/// <summary>
/// Read a number (possibly with an exponent) or generate a parse error.
/// </summary>
	protected double getNumberExp(){
		int tok = st.nextToken();

		if (tok != StreamTokenizer.TT_NUMBER)
			parseError("expecting a number, " + gotWhat(tok));

		double nb = st.nval;

		tok = st.nextToken();

		if (tok == StreamTokenizer.TT_WORD && (st.sval.StartsWith("e-") || st.sval.StartsWith("e+"))) {
			double exp = double.Parse(st.sval.Substring(2));
			return Math.Pow(nb, exp);
		} else {
			st.pushBack();
		}

		return nb;
	}

	/// <summary>
/// Return a string containing "got " then the content of the current <code>token</code>.
/// </summary>
	protected string gotWhat(int token) {
		switch (token) {
		case StreamTokenizer.TT_NUMBER:
			return "got number `" + st.nval + "'";
		case StreamTokenizer.TT_WORD:
			return "got word `" + st.sval + "'";
		case StreamTokenizer.TT_EOF:
			return "got EOF";
		default:
			if (token == QUOTE_CHAR)
				return "got string constant `" + st.sval + "'";
			else
				return "unknown symbol `" + token + "' (" + ((char) token) + ")";
		}
	}

	/// <summary>
/// Generate a parse error.
/// </summary>
	protected void parseError(string message){
		throw new System.IO.IOException("parse error: " + filename + ": " + st.lineno() + ": " + message);
	}

	// Access

	/// <summary>
/// True if the <code>string</code> represents a truth statement ("1", "true", "yes", "on").
/// </summary>
	protected bool isTrue(string string) {
		string = string.ToLower();

		if (string.Equals("1"))
			return true;
		if (string.Equals("true"))
			return true;
		if (string.Equals("yes"))
			return true;
		if (string.Equals("on"))
			return true;

		return false;
	}

	/// <summary>
/// True if the <code>string</code> represents a false statement ("0", "false", "no", "off").
/// </summary>
	protected bool isFalse(string string) {
		string = string.ToLower();

		if (string.Equals("0"))
			return true;
		if (string.Equals("false"))
			return true;
		if (string.Equals("no"))
			return true;
		if (string.Equals("off"))
			return true;

		return false;
	}

	/// <summary>
/// Uses {@link #isTrue(String)} and {@link #isFalse(String)} to determine if <code>value</code> is a truth value and return the corresponding boolean. if the <code>value</code> is not a truth value.
/// </summary>
	protected bool getBoolean(string value){
		if (isTrue(value))
			return true;
		if (isFalse(value))
			return false;
		throw new FormatException("not a truth value `" + value + "'");
	}

	/// <summary>
/// Try to transform <code>value</code> into a double. if the <code>value</code> is not a double.
/// </summary>
	protected double getReal(string value){
		return double.Parse(value);
	}

	/// <summary>
/// Try to transform <code>value</code> into a long. if the <code>value</code> is not a long.
/// </summary>
	protected long getInteger(string value){
		return long.Parse(value);
	}

	/// <summary>
/// Get a number triplet with numbers separated by comas and return a new point for it. For example "0,1,2".
/// </summary>
	protected Point3 getPoint3(string value){
		int p0 = value.IndexOf(',');
		int p1 = value.IndexOf(',', p0 + 1);

		if (p0 > 0 && p1 > 0) {
			string n0, n1, n2;
			float v0, v1, v2;

			n0 = value.Substring(0, p0);
			n1 = value.Substring(p0 + 1, p1);
			n2 = value.Substring(p1 + 1);

			v0 = float.Parse(n0);
			v1 = float.Parse(n1);
			v2 = float.Parse(n2);

			return new Point3(v0, v1, v2);
		}

		throw new FormatException("value '" + value + "' not in a valid point3 format");
	}

	/*
	 * Get a number triplet with numbers separated by comas and return new bounds
	 * for it. For example "0,1,2". protected Bounds3 getBounds3(String value)
	 * throws NumberFormatException { int p0 = value.indexOf(','); int p1 =
	 * value.indexOf(',', p0 + 1);
	 * 
	 * if (p0 > 0 && p1 > 0) { String n0, n1, n2; float v0, v1, v2;
	 * 
	 * n0 = value.substring(0, p0); n1 = value.substring(p0 + 1, p1); n2 =
	 * value.substring(p1 + 1);
	 * 
	 * v0 = float.parseFloat(n0); v1 = float.parseFloat(n1); v2 =
	 * float.parseFloat(n2);
	 * 
	 * return new Bounds3(v0, v1, v2); }
	 * 
	 * throw new NumberFormatException("value '" + value +
	 * "' not in a valid point3 format"); }
	 */

	// Nested classes

	/// <summary>
/// Currently processed file. <p> The graph reader base can process includes in files, and handles a stack of files. </p>
/// </summary>
	class CurrentFile {
		/// <summary>
/// The file name.
/// </summary>
		public string file;

		/// <summary>
/// The stream tokenizer.
/// </summary>
		public StreamTokenizer tok;

		public System.IO.TextReader reader;

		public CurrentFile(string f, StreamTokenizer t, System.IO.TextReader reader) {
			file = f;
			tok = t;
			this.reader = reader;
		}
	}
}
}
