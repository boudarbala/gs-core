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
/// File source factory. <p> A graph reader factory allow to create readers according to a given file. It both tries to read the start of the file to infer its type (works well for file formats with a magic cookie or header), and if it fails it tries to look at the file name extension. </p>
/// </summary>
public class FileSourceFactory {
	/// <summary>
/// Create a file input for the given file name. <p> This method first tests if the file is a regular file and is readable. If so, it opens it and reads the magic cookie to test the known file formats that can be inferred from their header. If it works, it returns a file input for the format. Else it looks at the file name extension, and returns a file input for the extension. Finally if all fail, it throws a NotFoundException. </p> <p> Notice that this method only creates the file input and does not connect it to a graph. </p> If the file is not readable or accessible.
/// </summary>
/// <param name="fileName"> Name of the graph file.</param>
/// <returns>A graph reader suitable for the fileName graph format.</returns>
	public static IFileSource sourceFor(string fileName){
		System.IO.FileInfo file = new System.IO.FileInfo(fileName);

		if (!file.isFile())
			throw new System.IO.IOException("not a regular file '" + fileName + "'");

		if (!file.canRead())
			throw new System.IO.IOException("not a readable file '" + fileName + "'");

		// Try to read the beginning of the file.

		RandomAccessFile input = new RandomAccessFile(fileName, "r");

		byte b[] = new byte[10];
		int n = input.Read(b, 0, 10);

		// System.err.printf( "[" );
		// for( int i=0; i<n; ++i )
		// {
		// System.err.printf( "%c", (char)b[i] );
		// }
		// System.err.printf( "]%n" );

		input.Close();

		// Surely match a DGS file, as DGS files are well done and have a
		// signature.

		if (n >= 3 && b[0] == 'D' && b[1] == 'G' && b[2] == 'S') {
			if (n >= 6 && b[3] == '0' && b[4] == '0') {
				if (b[5] == '1' || b[5] == '2') {
					return new FileSourceDGS1And2();
				} else if (b[5] == '3' || b[5] == '4') {
					return new FileSourceDGS();
				}
			}
		}

		// Maybe match a GML file as most GML files begin by the line "graph [",
		// but not sure, you may create a GML file that starts by a comment, an
		// empty line, with any kind of spaces, etc.

		if (n >= 7 && b[0] == 'g' && b[1] == 'r' && b[2] == 'a' && b[3] == 'p' && b[4] == 'h' && b[5] == ' '
				&& b[6] == '[') {
			return new org.graphstream.stream.file.FileSourceGML();
		}

		if (n >= 4 && b[0] == '(' && b[1] == 't' && b[2] == 'l' && b[3] == 'p')
			return new FileSourceTLP();

		// The web reader.

		string flc = fileName.ToLower();

		// If we did not found anything, we try with the filename extension ...

		if (flc.EndsWith(".dgs")) {
			return new FileSourceDGS();
		}

		if (flc.EndsWith(".gml") || flc.EndsWith(".dgml")) {
			return new org.graphstream.stream.file.FileSourceGML();
		}

		if (flc.EndsWith(".net")) {
			return new FileSourcePajek();
		}

		if (flc.EndsWith(".chaco") || flc.EndsWith(".graph")) {
			// return new GraphReaderChaco();
		}

		if (flc.EndsWith(".dot")) {
			return new org.graphstream.stream.file.FileSourceDOT();
		}

		if (flc.EndsWith(".edge")) {
			return new FileSourceEdge();
		}

		if (flc.EndsWith(".lgl")) {
			return new FileSourceLGL();
		}

		if (flc.EndsWith(".ncol")) {
			return new FileSourceNCol();
		}

		if (flc.EndsWith(".tlp")) {
			return new FileSourceTLP();
		}

		if (flc.EndsWith(".xml")) {
			string root = getXMLRootElement(fileName);

			if (root.Equals("gexf"))
				return new FileSourceGEXF();

			return new FileSourceGraphML();
		}

		if (flc.EndsWith(".gexf")) {
			return new FileSourceGEXF();
		}

		return null;
	}

	public static string getXMLRootElement(string fileName){
		System.IO.StreamReader stream = new System.IO.StreamReader(fileName);
		XMLEventReader reader;
		XMLEvent e;
		string root;

		try {
			reader = System.Xml.new XmlReaderSettings().createXMLEventReader(stream);

			do {
				e = reader.nextEvent();
			} while (!e.isStartElement() && !e.isEndDocument());

			if (e.isEndDocument())
				throw new System.IO.IOException("document ended before catching root element");

			root = e.asStartElement().Name.getLocalPart();
			reader.Close();
			stream.Close();

			return root;
		} catch (Exception ex) {
			throw new System.IO.IOException(ex);
		} catch (Exception ex) {
			throw new System.IO.IOException(ex);
		}
	}
}
}
