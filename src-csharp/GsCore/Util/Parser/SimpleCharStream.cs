using System.Collections.Generic;
using System.IO;
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
/// An implementation of interface CharStream, where the stream is assumed to contain only ASCII characters (without unicode processing).
/// </summary>

public class SimpleCharStream {
	/// <summary>
/// Whether parser is static.
/// </summary>
	public static readonly bool staticFlag = false;
	int bufsize;
	int available;
	int tokenBegin;
	/// <summary>
/// Position in buffer.
/// </summary>
	public int bufpos = -1;
	protected int[] bufline;
	protected int[] bufcolumn;

	protected int column = 0;
	protected int line = 1;

	protected bool prevCharIsCR = false;
	protected bool prevCharIsLF = false;

	protected java.io.System.IO.TextReader inputStream;

	protected char[] buffer;
	protected int maxNextCharInd = 0;
	protected int inBuf = 0;
	protected int tabSize = 8;

	protected void setTabSize(int i) {
		tabSize = i;
	}

	protected int getTabSize(int i) {
		return tabSize;
	}

	protected void ExpandBuff(bool wrapAround) {
		char[] newbuffer = new char[bufsize + 2048];
		int[] newbufline = new int[bufsize + 2048];
		int[] newbufcolumn = new int[bufsize + 2048];

		try {
			if (wrapAround) {
				Array.Copy(buffer, tokenBegin, newbuffer, 0, bufsize - tokenBegin);
				Array.Copy(buffer, 0, newbuffer, bufsize - tokenBegin, bufpos);
				buffer = newbuffer;

				Array.Copy(bufline, tokenBegin, newbufline, 0, bufsize - tokenBegin);
				Array.Copy(bufline, 0, newbufline, bufsize - tokenBegin, bufpos);
				bufline = newbufline;

				Array.Copy(bufcolumn, tokenBegin, newbufcolumn, 0, bufsize - tokenBegin);
				Array.Copy(bufcolumn, 0, newbufcolumn, bufsize - tokenBegin, bufpos);
				bufcolumn = newbufcolumn;

				maxNextCharInd = (bufpos += (bufsize - tokenBegin));
			} else {
				Array.Copy(buffer, tokenBegin, newbuffer, 0, bufsize - tokenBegin);
				buffer = newbuffer;

				Array.Copy(bufline, tokenBegin, newbufline, 0, bufsize - tokenBegin);
				bufline = newbufline;

				Array.Copy(bufcolumn, tokenBegin, newbufcolumn, 0, bufsize - tokenBegin);
				bufcolumn = newbufcolumn;

				maxNextCharInd = (bufpos -= tokenBegin);
			}
		} catch (Throwable t) {
			throw new Error(t.getMessage());
		}

		bufsize += 2048;
		available = bufsize;
		tokenBegin = 0;
	}

	protected void FillBuff(){
		if (maxNextCharInd == available) {
			if (available == bufsize) {
				if (tokenBegin > 2048) {
					bufpos = maxNextCharInd = 0;
					available = tokenBegin;
				} else if (tokenBegin < 0)
					bufpos = maxNextCharInd = 0;
				else
					ExpandBuff(false);
			} else if (available > tokenBegin)
				available = bufsize;
			else if ((tokenBegin - available) < 2048)
				ExpandBuff(true);
			else
				available = tokenBegin;
		}

		int i;
		try {
			if ((i = inputStream.Read(buffer, maxNextCharInd, available - maxNextCharInd)) == -1) {
				inputStream.Close();
				throw new java.io.System.IO.IOException();
			} else
				maxNextCharInd += i;
			return;
		} catch (java.io.System.IO.IOException e) {
			--bufpos;
			backup(0);
			if (tokenBegin == -1)
				tokenBegin = bufpos;
			throw e;
		}
	}

	/// <summary>
/// Start.
/// </summary>
	public char BeginToken(){
		tokenBegin = -1;
		char c = readChar();
		tokenBegin = bufpos;

		return c;
	}

	protected void UpdateLineColumn(char c) {
		column++;

		if (prevCharIsLF) {
			prevCharIsLF = false;
			line += (column = 1);
		} else if (prevCharIsCR) {
			prevCharIsCR = false;
			if (c == '\n') {
				prevCharIsLF = true;
			} else
				line += (column = 1);
		}

		switch (c) {
		case '\r':
			prevCharIsCR = true;
			break;
		case '\n':
			prevCharIsLF = true;
			break;
		case '\t':
			column--;
			column += (tabSize - (column % tabSize));
			break;
		default:
			break;
		}

		bufline[bufpos] = line;
		bufcolumn[bufpos] = column;
	}

	/// <summary>
/// Read a character.
/// </summary>
	public char readChar(){
		if (inBuf > 0) {
			--inBuf;

			if (++bufpos == bufsize)
				bufpos = 0;

			return buffer[bufpos];
		}

		if (++bufpos >= maxNextCharInd)
			FillBuff();

		char c = buffer[bufpos];

		UpdateLineColumn(c);
		return c;
	}

	[Obsolete] 
	
	public int getColumn() {
		return bufcolumn[bufpos];
	}

	[Obsolete] 
	
	public int getLine() {
		return bufline[bufpos];
	}

	/// <summary>
/// Get token end column number.
/// </summary>
	public int getEndColumn() {
		return bufcolumn[bufpos];
	}

	/// <summary>
/// Get token end line number.
/// </summary>
	public int getEndLine() {
		return bufline[bufpos];
	}

	/// <summary>
/// Get token beginning column number.
/// </summary>
	public int getBeginColumn() {
		return bufcolumn[tokenBegin];
	}

	/// <summary>
/// Get token beginning line number.
/// </summary>
	public int getBeginLine() {
		return bufline[tokenBegin];
	}

	/// <summary>
/// Backup a number of characters.
/// </summary>
	public void backup(int amount) {

		inBuf += amount;
		if ((bufpos -= amount) < 0)
			bufpos += bufsize;
	}

	/// <summary>
/// Constructor.
/// </summary>
	public SimpleCharStream(java.io.System.IO.TextReader dstream, int startline, int startcolumn, int buffersize) {
		inputStream = dstream;
		line = startline;
		column = startcolumn - 1;

		available = bufsize = buffersize;
		buffer = new char[buffersize];
		bufline = new int[buffersize];
		bufcolumn = new int[buffersize];
	}

	/// <summary>
/// Constructor.
/// </summary>
	public SimpleCharStream(java.io.System.IO.TextReader dstream, int startline, int startcolumn) : this(dstream, startline, startcolumn, 4096) {
	}

	/// <summary>
/// Constructor.
/// </summary>
	public SimpleCharStream(java.io.System.IO.TextReader dstream) : this(dstream, 1, 1, 4096) {
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.TextReader dstream, int startline, int startcolumn, int buffersize) {
		inputStream = dstream;
		line = startline;
		column = startcolumn - 1;

		if (buffer == null || buffersize != buffer.Length) {
			available = bufsize = buffersize;
			buffer = new char[buffersize];
			bufline = new int[buffersize];
			bufcolumn = new int[buffersize];
		}
		prevCharIsLF = prevCharIsCR = false;
		tokenBegin = inBuf = maxNextCharInd = 0;
		bufpos = -1;
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.TextReader dstream, int startline, int startcolumn) {
		ReInit(dstream, startline, startcolumn, 4096);
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.TextReader dstream) {
		ReInit(dstream, 1, 1, 4096);
	}

	/// <summary>
/// Constructor.
/// </summary>
	public SimpleCharStream(java.io.System.IO.Stream dstream, string encoding, int startline, int startcolumn,
			int buffersize){
		this(encoding == null ? new java.io.System.IO.StreamReader(dstream)
				: new java.io.System.IO.StreamReader(dstream, encoding), startline, startcolumn, buffersize);
	}

	/// <summary>
/// Constructor.
/// </summary>
	public SimpleCharStream(java.io.System.IO.Stream dstream, int startline, int startcolumn, int buffersize) : this(new java.io.System.IO.StreamReader(dstream), startline, startcolumn, buffersize) {
	}

	/// <summary>
/// Constructor.
/// </summary>
	public SimpleCharStream(java.io.System.IO.Stream dstream, string encoding, int startline, int startcolumn){
		this(dstream, encoding, startline, startcolumn, 4096);
	}

	/// <summary>
/// Constructor.
/// </summary>
	public SimpleCharStream(java.io.System.IO.Stream dstream, int startline, int startcolumn) : this(dstream, startline, startcolumn, 4096) {
	}

	/// <summary>
/// Constructor.
/// </summary>
	public SimpleCharStream(java.io.System.IO.Stream dstream, string encoding): this(dstream, encoding, 1, 1, 4096) {
	}

	/// <summary>
/// Constructor.
/// </summary>
	public SimpleCharStream(java.io.System.IO.Stream dstream) : this(dstream, 1, 1, 4096) {
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.Stream dstream, string encoding, int startline, int startcolumn, int buffersize){
		ReInit(encoding == null ? new java.io.System.IO.StreamReader(dstream)
				: new java.io.System.IO.StreamReader(dstream, encoding), startline, startcolumn, buffersize);
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.Stream dstream, int startline, int startcolumn, int buffersize) {
		ReInit(new java.io.System.IO.StreamReader(dstream), startline, startcolumn, buffersize);
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.Stream dstream, string encoding){
		ReInit(dstream, encoding, 1, 1, 4096);
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.Stream dstream) {
		ReInit(dstream, 1, 1, 4096);
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.Stream dstream, string encoding, int startline, int startcolumn){
		ReInit(dstream, encoding, startline, startcolumn, 4096);
	}

	/// <summary>
/// Reinitialise.
/// </summary>
	public void ReInit(java.io.System.IO.Stream dstream, int startline, int startcolumn) {
		ReInit(dstream, startline, startcolumn, 4096);
	}

	/// <summary>
/// Get token literal value.
/// </summary>
	public string GetImage() {
		if (bufpos >= tokenBegin)
			return new string(buffer, tokenBegin, bufpos - tokenBegin + 1);
		else
			return new string(buffer, tokenBegin, bufsize - tokenBegin) + new string(buffer, 0, bufpos + 1);
	}

	/// <summary>
/// Get the suffix.
/// </summary>
	public char[] GetSuffix(int len) {
		char[] ret = new char[len];

		if ((bufpos + 1) >= len)
			Array.Copy(buffer, bufpos - len + 1, ret, 0, len);
		else {
			Array.Copy(buffer, bufsize - (len - bufpos - 1), ret, 0, len - bufpos - 1);
			Array.Copy(buffer, 0, ret, len - bufpos - 1, bufpos + 1);
		}

		return ret;
	}

	/// <summary>
/// Reset buffer when finished.
/// </summary>
	public void Done() {
		buffer = null;
		bufline = null;
		bufcolumn = null;
	}

	public void close(){
		inputStream.Close();
	}

	/// <summary>
/// Method to adjust line and column numbers for the start of a token.
/// </summary>
	public void adjustBeginLineColumn(int newLine, int newCol) {
		int start = tokenBegin;
		int len;

		if (bufpos >= tokenBegin) {
			len = bufpos - tokenBegin + inBuf + 1;
		} else {
			len = bufsize - tokenBegin + bufpos + 1 + inBuf;
		}

		int i = 0, j = 0, k = 0;
		int nextColDiff = 0, columnDiff = 0;

		while (i < len && bufline[j = start % bufsize] == bufline[k = ++start % bufsize]) {
			bufline[j] = newLine;
			nextColDiff = columnDiff + bufcolumn[k] - bufcolumn[j];
			bufcolumn[j] = newCol + columnDiff;
			columnDiff = nextColDiff;
			i++;
		}

		if (i < len) {
			bufline[j] = newLine++;
			bufcolumn[j] = newCol + columnDiff;

			while (i++ < len) {
				if (bufline[j = start % bufsize] != bufline[++start % bufsize])
					bufline[j] = newLine++;
				else
					bufline[j] = newLine;
			}
		}

		line = bufline[j];
		column = bufcolumn[j];
	}

}

}
