using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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


// import org.graphstream.util.time.ISODateIO;

public class DGSParser : Parser {
	enum Token {
		AN, CN, DN, AE, CE, DE, CG, ST, CL, TF, EOF
	}

	protected static readonly int BUFFER_SIZE = 4096;

	public static readonly int ARRAY_OPEN = '{';
	public static readonly int ARRAY_CLOSE = '}';

	public static readonly int MAP_OPEN = '[';
	public static readonly int MAP_CLOSE = ']';

	System.IO.TextReader reader;
	int line, column;
	int bufferCapacity, bufferPosition;
	char[] buffer;
	int[] pushback;
	int pushbackOffset;
	FileSourceDGS dgs;
	string sourceId;
	Token lastDirective;

	// ISODateIO dateIO;

	public DGSParser(FileSourceDGS dgs, System.IO.TextReader reader) {
		this.dgs = dgs;
		this.reader = reader;
		bufferCapacity = 0;
		buffer = new char[BUFFER_SIZE];
		pushback = new int[10];
		pushbackOffset = -1;
		this.sourceId = string.Format("<DGS stream {0}>", (DateTime.UtcNow.Ticks * 100L));

		// try {
		// dateIO = new ISODateIO();
		// } catch (Exception e) {
		// Console.Error.WriteLine(e);
		// }
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.util.parser.Parser#close()
	 */
	public void close(){
		reader.Close();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.util.parser.Parser#open()
	 */
	public void open(){
		header();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.util.parser.Parser#all()
	 */
	public void all(){
		header();

		while (next())
			;
	}

	protected int nextChar(){
		int c;

		if (pushbackOffset >= 0)
			return pushback[pushbackOffset--];

		if (bufferCapacity == 0 || bufferPosition >= bufferCapacity) {
			bufferCapacity = reader.Read(buffer, 0, BUFFER_SIZE);
			bufferPosition = 0;
		}

		if (bufferCapacity <= 0)
			return -1;

		c = buffer[bufferPosition++];

		//
		// Handle special EOL
		// - LF
		// - CR
		// - CR+LF
		//
		if (c == '\r') {
			if (bufferPosition < bufferCapacity) {
				if (buffer[bufferPosition] == '\n')
					bufferPosition++;
			} else {
				c = nextChar();

				if (c != '\n')
					pushback(c);
			}

			c = '\n';
		}

		if (c == '\n') {
			line++;
			column = 0;
		} else
			column++;

		return c;
	}

	protected void pushback(int c){
		if (c < 0)
			return;

		if (pushbackOffset + 1 >= pushback.Length)
			throw new System.IO.IOException("pushback buffer overflow");

		pushback[++pushbackOffset] = c;
	}

	protected void skipLine(){
		int c;

		while ((c = nextChar()) != '\n' && c >= 0)
			;
	}

	protected void skipWhitespaces(){
		int c;

		while ((c = nextChar()) == ' ' || c == '\t')
			;

		pushback(c);
	}

	protected void header(){
		int[] dgs = new int[6];

		for (int i = 0; i < 6; i++)
			dgs[i] = nextChar();

		if (dgs[0] != 'D' || dgs[1] != 'G' || dgs[2] != 'S')
			throw parseException(
					string.Format("bad magic header, 'DGS' expected, got '{0}{1}{2}'", dgs[0], dgs[1], dgs[2]));

		if (dgs[3] != '0' || dgs[4] != '0' || dgs[5] < '0' || dgs[5] > '5')
			throw parseException(string.Format("bad version \"%c%c%c\"", dgs[0], dgs[1], dgs[2]));

		if (nextChar() != '\n')
			throw parseException("end-of-line is missing");

		skipLine();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.util.parser.Parser#next()
	 */
	public bool next(){
		int c;
		string nodeId;
		string edgeId, source, target;

		lastDirective = directive();

		switch (lastDirective) {
		case AN:
			nodeId = id();
			dgs.sendNodeAdded(sourceId, nodeId);

			attributes(ElementType.NODE, nodeId);
			break;
		case CN:
			nodeId = id();
			attributes(ElementType.NODE, nodeId);
			break;
		case DN:
			nodeId = id();
			dgs.sendNodeRemoved(sourceId, nodeId);
			break;
		case AE:
			edgeId = id();
			source = id();

			skipWhitespaces();
			c = nextChar();

			if (c != '<' && c != '>')
				pushback(c);

			target = id();

			switch (c) {
			case '>':
				dgs.sendEdgeAdded(sourceId, edgeId, source, target, true);
				break;
			case '<':
				dgs.sendEdgeAdded(sourceId, edgeId, target, source, true);
				break;
			default:
				dgs.sendEdgeAdded(sourceId, edgeId, source, target, false);
				break;
			}

			attributes(ElementType.EDGE, edgeId);
			break;
		case CE:
			edgeId = id();
			attributes(ElementType.EDGE, edgeId);
			break;
		case DE:
			edgeId = id();
			dgs.sendEdgeRemoved(sourceId, edgeId);
			break;
		case CG:
			attributes(ElementType.GRAPH, null);
			break;
		case ST:
			// TODO release 1.2 : read timestamp
			// Version for 1.2 :
			// --------------------------------
			// long step;
			// step = timestamp();
			// sendStepBegins(sourceId, ste);

			double step;

			step = double.valueOf(id());
			dgs.sendStepBegins(sourceId, step);
			break;
		case CL:
			dgs.sendGraphCleared(sourceId);
			break;
		case TF:
			// TODO for release 1.2
			// String tf;
			// tf = string();

			// try {
			// dateIO.setFormat(tf);
			// } catch (Exception e) {
			// throw parseException("invalid time format \"%s\"", tf);
			// }

			break;
		case EOF:
			return false;
		}

		skipWhitespaces();
		c = nextChar();

		if (c == '#') {
			skipLine();
			return true;
		}

		if (c < 0)
			return false;

		if (c != '\n')
			throw parseException("eol expected, got '%c'", c);

		return true;
	}

	public bool nextStep(){
		bool r;
		Token next;

		do {
			r = next();
			next = directive();

			if (next != Token.EOF) {
				pushback(next.ToString()[1]);
				pushback(next.ToString()[0]);
			}
		} while (next != Token.ST && next != Token.EOF);

		return r;
	}

	protected void attributes(ElementType type, string id){
		int c;

		skipWhitespaces();

		while ((c = nextChar()) != '\n' && c != '#' && c >= 0) {
			pushback(c);
			attribute(type, id);
			skipWhitespaces();
		}

		pushback(c);
	}

	protected void attribute(ElementType type, string elementId){
		string key;
		object value = null;
		int c;
		AttributeChangeEvent ch = AttributeChangeEvent.CHANGE;

		skipWhitespaces();
		c = nextChar();

		if (c == '+')
			ch = AttributeChangeEvent.ADD;
		else if (c == '-')
			ch = AttributeChangeEvent.REMOVE;
		else
			pushback(c);

		key = id();

		if (key == null)
			throw parseException("attribute key expected");

		if (ch != AttributeChangeEvent.REMOVE) {

			skipWhitespaces();
			c = nextChar();

			if (c == '=' || c == ':') {
				skipWhitespaces();
				value = value(true);
			} else {
				value = bool.TRUE;
				pushback(c);
			}
		}

		dgs.sendAttributeChangedEvent(sourceId, elementId, type, key, ch, null, value);
	}

	protected object value(bool array){
		int c;
		List<object> l = null;
		object o;

		do {
			skipWhitespaces();
			c = nextChar();
			pushback(c);

			switch (c) {
			case '\'':
			case '\"':
				o = string();
				break;
			case '#':
				o = color();
				break;
			case ARRAY_OPEN:
				//
				// Skip ARRAY_OPEN
				nextChar();
				//

				skipWhitespaces();
				o = value(true);
				skipWhitespaces();

				//
				// Check if next char is ARRAY_CLOSE
				if (nextChar() != ARRAY_CLOSE)
					throw parseException("'%c' expected", ARRAY_CLOSE);
				//

				if (!o.GetType().IsArray)
					o = new object[] { o };

				break;
			case MAP_OPEN:
				o = map();
				break;
			default {
				string word = id();

				if (word == null)
					throw parseException("missing value");

				if ((c >= '0' && c <= '9') || c == '-') {
					try {
						if (word.IndexOf('.') > 0)
							o = double.valueOf(word);
						else {
							try {
								o = int.Parse(word);
							} catch (FormatException e) {
								o = long.valueOf(word);
							}
						}
					} catch (FormatException e) {
						throw parseException("invalid number format '%s'", word);
					}
				} else {
					if (word.Equals("true"))
						o = bool.TRUE;
					else if (word.Equals("false"))
						o = bool.FALSE;
					else
						o = word;
				}

				break;
			}
			}

			c = nextChar();

			if (l == null && array && c == ',') {
				l = new List<object>();
				l.Add(o);
			} else if (l != null)
				l.Add(o);
		} while (array && c == ',');

		pushback(c);

		if (l == null)
			return o;

		return l.ToArray();
	}

	protected Color color(){
		int c;
		int r, g, b, a;
		System.Text.System.Text.StringBuilder hexa = new System.Text.System.Text.StringBuilder();

		c = nextChar();

		if (c != '#')
			throw parseException("'#' expected");

		for (int i = 0; i < 6; i++) {
			c = nextChar();

			if ((c >= 0 && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
				hexa.appendCodePoint(c);
			else
				throw parseException("hexadecimal value expected");
		}

		r = int.Parse(hexa.Substring(0, 2), 16);
		g = int.Parse(hexa.Substring(2, 4), 16);
		b = int.Parse(hexa.Substring(4, 6), 16);

		c = nextChar();

		if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')) {
			hexa.appendCodePoint(c);

			c = nextChar();

			if ((c >= 0 && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
				hexa.appendCodePoint(c);
			else
				throw parseException("hexadecimal value expected");

			a = int.Parse(hexa.Substring(6, 8), 16);
		} else {
			a = 255;
			pushback(c);
		}

		return new Color(r, g, b, a);
	}

	protected object array(){
		int c;
		List<object> array = new List<object>();

		c = nextChar();

		if (c != ARRAY_OPEN)
			throw parseException("'%c' expected", ARRAY_OPEN);

		skipWhitespaces();
		c = nextChar();

		while (c != ARRAY_CLOSE) {
			pushback(c);
			array.Add(value(false));

			skipWhitespaces();
			c = nextChar();

			if (c != ARRAY_CLOSE && c != ',')
				throw parseException("'%c' or ',' expected, got '%c'", ARRAY_CLOSE, c);

			if (c == ',') {
				skipWhitespaces();
				c = nextChar();
			}
		}

		if (c != ARRAY_CLOSE)
			throw parseException("'%c' expected", ARRAY_CLOSE);

		return array.ToArray();
	}

	protected object map(){
		int c;
		Dictionary<string, object> map = new Dictionary<string, object>();
		string key;
		object value;

		c = nextChar();

		if (c != MAP_OPEN)
			throw parseException("'%c' expected", MAP_OPEN);

		c = nextChar();

		while (c != MAP_CLOSE) {
			pushback(c);
			key = id();

			if (key == null)
				throw parseException("id expected here, '%c'", c);

			skipWhitespaces();
			c = nextChar();

			if (c == '=' || c == ':') {
				skipWhitespaces();
				value = value(false);
			} else {
				value = bool.TRUE;
				pushback(c);
			}

			map[key] = value;

			skipWhitespaces();
			c = nextChar();

			if (c != MAP_CLOSE && c != ',')
				throw parseException("'%c' or ',' expected, got '%c'", MAP_CLOSE, c);

			if (c == ',') {
				skipWhitespaces();
				c = nextChar();
			}
		}

		if (c != MAP_CLOSE)
			throw parseException("'%c' expected", MAP_CLOSE);

		return map;
	}

	protected Token directive(){
		int c1, c2;

		//
		// Skip comment and empty lines
		//
		do {
			c1 = nextChar();

			if (c1 == '#')
				skipLine();

			if (c1 < 0)
				return Token.EOF;
		} while (c1 == '#' || c1 == '\n');

		c2 = nextChar();

		if (c1 >= 'A' && c1 <= 'Z')
			c1 -= 'A' - 'a';

		if (c2 >= 'A' && c2 <= 'Z')
			c2 -= 'A' - 'a';

		switch (c1) {
		case 'a':
			if (c2 == 'n')
				return Token.AN;
			else if (c2 == 'e')
				return Token.AE;

			break;
		case 'c':
			switch (c2) {
			case 'n':
				return Token.CN;
			case 'e':
				return Token.CE;
			case 'g':
				return Token.CG;
			case 'l':
				return Token.CL;
			}

			break;
		case 'd':
			if (c2 == 'n')
				return Token.DN;
			else if (c2 == 'e')
				return Token.DE;

			break;
		case 's':
			if (c2 == 't')
				return Token.ST;

			break;
		case 't':
			if (c1 == 'f')
				return Token.TF;

			break;
		}

		throw parseException("unknown directive '%c%c'", c1, c2);
	}

	protected string string(){
		int c, s;
		System.Text.System.Text.StringBuilder builder;
		bool slash;

		slash = false;
		builder = new System.Text.System.Text.StringBuilder();
		c = nextChar();

		if (c != '\"' && c != '\'')
			throw parseException("string expected");

		s = c;

		while ((c = nextChar()) != s || slash) {
			if (slash && c != s)
				builder.Append("\\");

			slash = c == '\\';

			if (!slash) {
				if (!char.isValidCodePoint(c))
					throw parseException("invalid code-point 0x%X", c);

				builder.appendCodePoint(c);
			}
		}

		return builder.ToString();
	}

	protected string id(){
		int c;
		System.Text.System.Text.StringBuilder builder = new System.Text.System.Text.StringBuilder();

		skipWhitespaces();
		c = nextChar();
		pushback(c);

		if (c == '\"' || c == '\'') {
			return string();
		} else {
			bool stop = false;

			while (!stop) {
				c = nextChar();

				switch (char.getType(c)) {
				case char.LOWERCASE_LETTER:
				case char.UPPERCASE_LETTER:
				case char.DECIMAL_DIGIT_NUMBER:
					break;
				case char.DASH_PUNCTUATION:
					if (c != '-')
						stop = true;

					break;
				case char.MATH_SYMBOL:
					if (c != '+')
						stop = true;

					break;
				case char.CONNECTOR_PUNCTUATION:
					if (c != '_')
						stop = true;

					break;
				case char.OTHER_PUNCTUATION:
					if (c != '.')
						stop = true;

					break;
				default:
					stop = true;
					break;
				}

				if (!stop)
					builder.appendCodePoint(c);
			}

			pushback(c);
		}

		if (builder.Length == 0)
			return null;

		return builder.ToString();
	}

	/*
	 * protected long timestamp(){ int c; String
	 * time;
	 * 
	 * c = nextChar(); pushback(c);
	 * 
	 * switch (c) { case '"': case '\'': time = string(); break; default:
	 * System.Text.StringBuilder builder = new System.Text.StringBuilder();
	 * 
	 * while ((c = nextChar()) != '\n' && c != '"') builder.appendCodePoint(c);
	 * 
	 * pushback(c); time = builder.toString(); break; }
	 * 
	 * pushback(c); return dateIO.parse(time).getTimeInMillis(); }
	 */

	protected ParseException parseException(string message, params object[] args) {
		return new ParseException(
				string.Format(string.Format("parse error at ({0};{1})  {2}", line, column, message), args));
	}
}

}
