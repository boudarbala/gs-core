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
/// This defines source using a {@link org.graphstream.util.parser.Parser} object to parse a stream and generate graph events.
/// </summary>
abstract class FileSourceParser : SourceBase, IFileSource {
	/// <summary>
/// Factory used to create parser.
/// </summary>
	protected ParserFactory factory;

	/// <summary>
/// Parser opened by a call to {@link #begin(Reader)}.
/// </summary>
	protected Parser parser;

	/// <summary>
/// Get a new parser factory.
/// </summary>
/// <returns>a parser factory</returns>
	public abstract ParserFactory getNewParserFactory();

	protected FileSourceParser() {
		factory = getNewParserFactory();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#readAll(java.lang.String)
	 */
	public void readAll(string fileName){
		Parser parser = factory.newParser(createReaderForFile(fileName));

		try {
			parser.all();
			parser.Close();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#readAll(java.net.URL)
	 */
	public void readAll(System.Uri url){
		Parser parser = factory.newParser(new System.IO.StreamReader(url /* .openStream() */));

		try {
			parser.all();
			parser.Close();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#readAll(java.io.InputStream)
	 */
	public void readAll(System.IO.Stream stream){
		Parser parser = factory.newParser(new System.IO.StreamReader(stream));

		try {
			parser.all();
			parser.Close();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#readAll(java.io.Reader)
	 */
	public void readAll(System.IO.TextReader reader){
		Parser parser = factory.newParser(reader);

		try {
			parser.all();
			parser.Close();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#begin(java.lang.String)
	 */
	public void begin(string fileName){
		if (parser != null)
			end();

		parser = factory.newParser(createReaderForFile(fileName));

		try {
			parser.open();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#begin(java.net.URL)
	 */
	public void begin(System.Uri url){
		parser = factory.newParser(new System.IO.StreamReader(url /* .openStream() */));

		try {
			parser.open();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#begin(java.io.InputStream)
	 */
	public void begin(System.IO.Stream stream){
		parser = factory.newParser(new System.IO.StreamReader(stream));

		try {
			parser.open();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#begin(java.io.Reader)
	 */
	public void begin(System.IO.TextReader reader){
		parser = factory.newParser(reader);

		try {
			parser.open();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#nextEvents()
	 */
	public bool nextEvents(){
		try {
			return parser.next();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	/// <summary>
/// Since there is no step in DOT, this does the same action than {@link #nextEvents()}.
/// </summary>
	public bool nextStep(){
		return nextEvents();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSource#end()
	 */
	public void end(){
		parser.Close();
		parser = null;
	}

	protected System.IO.TextReader createReaderForFile(string filename){
		return new System.IO.StreamReader(filename);
	}
}

}
