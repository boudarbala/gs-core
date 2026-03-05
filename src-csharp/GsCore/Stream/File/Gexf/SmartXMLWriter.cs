using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File.Gexf
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


public class SmartXMLWriter {
	public XMLStreamWriter stream;

	bool smart;
	int depth;
	List<int> childrenCount;

	public SmartXMLWriter(System.IO.TextWriter output, bool smart){
		stream = XMLOutputFactory.newFactory().createXMLStreamWriter(output);
		stream.writeStartDocument("UTF-8", "1.0");

		this.smart = smart;
		this.depth = 0;
		this.childrenCount = new List<int>();
		this.childrenCount.Add(0);
	}

	public void startElement(string name){
		if (smart) {
			stream.writeCharacters("\n");

			for (int i = 0; i < depth; i++)
				stream.writeCharacters(" ");
		}

		childrenCount.set(0, childrenCount[0] + 1);
		childrenCount.addFirst(0);

		stream.writeStartElement(name);
		depth++;
	}

	public void endElement(){
		depth--;

		bool leaf = (childrenCount.pop() == 0);

		if (smart && !leaf) {
			stream.writeCharacters("\n");

			for (int i = 0; i < depth; i++)
				stream.writeCharacters(" ");
		}

		stream.writeEndElement();
	}

	public void leafWithText(string name, string content){
		startElement(name);
		stream.writeCharacters(content);
		endElement();
	}

	public void flush() {
		try {
			stream.Flush();
		} catch (Exception e) {
			// Ignored
		}
	}

	public void close(){
		stream.writeEndDocument();
		stream.Flush();

		stream.Close();
	}
}

}
