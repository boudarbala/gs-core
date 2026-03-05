using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Util.Parser
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
/// Token Manager Error.
/// </summary>
public class TokenMgrError : Error {

	/// <summary>
/// The version identifier for this Serializable class. Increment only if the <i>serialized</i> form of the class changes.
/// </summary>
	private static readonly long serialVersionUID = 1L;

	/*
	 * Ordinals for various reasons why an Error of this type can be thrown.
	 */

	/// <summary>
/// Lexical error occurred.
/// </summary>
	public static readonly int LEXICAL_ERROR = 0;

	/// <summary>
/// An attempt was made to create a second instance of a static token manager.
/// </summary>
	public static readonly int STATIC_LEXER_ERROR = 1;

	/// <summary>
/// Tried to change to an invalid lexical state.
/// </summary>
	public static readonly int INVALID_LEXICAL_STATE = 2;

	/// <summary>
/// Detected (and bailed out of) an infinite loop in the token manager.
/// </summary>
	public static readonly int LOOP_DETECTED = 3;

	/// <summary>
/// Indicates the reason why the exception is thrown. It will have one of the above 4 values.
/// </summary>
	int errorCode;

	/// <summary>
/// Replaces unprintable characters by their escaped (or unicode escaped) equivalents in the given string
/// </summary>
	protected static readonly string addEscapes(string str) {
		StringBuffer retval = new StringBuffer();
		char ch;
		for (int i = 0; i < str.Length; i++) {
			switch (str[i]) {
			case 0:
				continue;
			case '\b':
				retval.Append("\\b");
				continue;
			case '\t':
				retval.Append("\\t");
				continue;
			case '\n':
				retval.Append("\\n");
				continue;
			case '\f':
				retval.Append("\\f");
				continue;
			case '\r':
				retval.Append("\\r");
				continue;
			case '\"':
				retval.Append("\\\"");
				continue;
			case '\'':
				retval.Append("\\\'");
				continue;
			case '\\':
				retval.Append("\\\\");
				continue;
			default:
				if ((ch = str[i]) < 0x20 || ch > 0x7e) {
					string s = "0000" + int.toString(ch, 16);
					retval.Append("\\u" + s.Substring(s.Length - 4, s.Length));
				} else {
					retval.Append(ch);
				}
				continue;
			}
		}
		return retval.ToString();
	}

	/// <summary>
/// Returns a detailed message for the Error when it is thrown by the token manager to indicate a lexical error. Parameters : EOFSeen : indicates if EOF caused the lexical error curLexState : lexical state in which this error occurred errorLine : line number when the error occurred errorColumn : column number when the error occurred errorAfter : prefix that was seen before this error occurred curchar : the offending character Note: You can customize the lexical error message by modifying this method.
/// </summary>
	protected static string LexicalError(bool EOFSeen, int lexState, int errorLine, int errorColumn,
			string errorAfter, char curChar) {
		return ("Lexical error at line " + errorLine + ", column " + errorColumn + ".  Encountered: "
				+ (EOFSeen ? "<EOF> "
						: ("\"" + addEscapes(Convert.ToString(curChar)) + "\"") + " (" + (int) curChar + "), ")
				+ "after : \"" + addEscapes(errorAfter) + "\"");
	}

	/// <summary>
/// You can also modify the body of this method to customize your error messages. For example, cases like LOOP_DETECTED and INVALID_LEXICAL_STATE are not of end-users concern, so you can return something like : "Internal Error : Please file a bug report .... " from this method for such cases in the release version of your parser.
/// </summary>
	public string getMessage() {
		return base.getMessage();
	}

	/*
	 * Constructors of various flavors follow.
	 */

	/// <summary>
/// No arg constructor.
/// </summary>
	public TokenMgrError() {
	}

	/// <summary>
/// Constructor with message and reason.
/// </summary>
	public TokenMgrError(string message, int reason) : base(message) {
		errorCode = reason;
	}

	/// <summary>
/// Full Constructor.
/// </summary>
	public TokenMgrError(bool EOFSeen, int lexState, int errorLine, int errorColumn, string errorAfter, char curChar,
			int reason) {
		this(LexicalError(EOFSeen, lexState, errorLine, errorColumn, errorAfter, curChar), reason);
	}
}

}
