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
/// Describes the input token stream.
/// </summary>

public class Token : java.io.System.Runtime.Serialization.ISerializable {

	/// <summary>
/// The version identifier for this Serializable class. Increment only if the <i>serialized</i> form of the class changes.
/// </summary>
	private static readonly long serialVersionUID = 1L;

	/// <summary>
/// An integer that describes the kind of this token. This numbering system is determined by JavaCCParser, and a table of these numbers is stored in the file ...Constants.java.
/// </summary>
	public int kind;

	/// <summary>
/// The line number of the first character of this Token.
/// </summary>
	public int beginLine;
	/// <summary>
/// The column number of the first character of this Token.
/// </summary>
	public int beginColumn;
	/// <summary>
/// The line number of the last character of this Token.
/// </summary>
	public int endLine;
	/// <summary>
/// The column number of the last character of this Token.
/// </summary>
	public int endColumn;

	/// <summary>
/// The string image of the token.
/// </summary>
	public string image;

	/// <summary>
/// A reference to the next regular (non-special) token from the input stream. If this is the last token from the input stream, or if the token manager has not read tokens beyond this one, this field is set to null. This is true only if this token is also a regular token. Otherwise, see below for a description of the contents of this field.
/// </summary>
	public Token next;

	/// <summary>
/// This field is used to access special tokens that occur prior to this token, but after the immediately preceding regular (non-special) token. If there are no such special tokens, this field is set to null. When there are more than one such special token, this field refers to the last of these special tokens, which in turn refers to the next previous special token through its specialToken field, and so on until the first special token (whose specialToken field is null). The next fields of special tokens refer to other special tokens that immediately follow it (without an intervening regular token). If there is no such token, this field is null.
/// </summary>
	public Token specialToken;

	/// <summary>
/// An optional attribute value of the Token. Tokens which are not used as syntactic sugar will often contain meaningful values that will be used later on by the compiler or interpreter. This attribute value is often different from the image. Any subclass of Token that actually wants to return a non-null value can override this method as appropriate.
/// </summary>
	public object getValue() {
		return null;
	}

	/// <summary>
/// No-argument constructor
/// </summary>
	public Token() {
	}

	/// <summary>
/// Constructs a new token for the specified Image.
/// </summary>
	public Token(int kind) : this(kind, null) {
	}

	/// <summary>
/// Constructs a new token for the specified Image and Kind.
/// </summary>
	public Token(int kind, string image) {
		this.kind = kind;
		this.image = image;
	}

	/// <summary>
/// Returns the image.
/// </summary>
	public string toString() {
		return image;
	}

	/// <summary>
/// Returns a new Token object, by default. However, if you want, you can create and return subclass objects based on the value of ofKind. Simply add the cases to the switch for all those special cases. For example, if you have a subclass of Token called IDToken that you want to create if ofKind is ID, simply add something like : case MyParserConstants.ID : return new IDToken(ofKind, image); to the following switch statement. Then you can cast matchedToken variable to the appropriate type and use sit in your lexical actions.
/// </summary>
	public static Token newToken(int ofKind, string image) {
		switch (ofKind) {
		default:
			return new Token(ofKind, image);
		}
	}

	public static Token newToken(int ofKind) {
		return newToken(ofKind, null);
	}

}

}
