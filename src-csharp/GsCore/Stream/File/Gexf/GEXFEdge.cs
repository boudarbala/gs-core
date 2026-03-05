using System.Collections.Generic;
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


public class GEXFEdge : IGEXFElement {
	GEXF root;

	string id;
	string label;
	string source;
	string target;
	bool directed;

	double weight = double.NaN;

	GEXFAttValues attvalues;
	GEXFSpells spells;

	public GEXFEdge(GEXF root, string id, string source, string target, bool directed) {
		this.root = root;

		this.id = id;
		this.label = id;
		this.source = source;
		this.target = target;
		this.directed = directed;

		spells = new GEXFSpells(root);
		attvalues = new GEXFAttValues(root);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.gexf.GEXFElement#export(org.graphstream.stream
	 * .file.gexf.SmartXMLWriter)
	 */
	public void export(SmartXMLWriter stream){
		stream.startElement("edge");

		stream.stream.writeAttribute("id", id);
		stream.stream.writeAttribute("label", label);
		stream.stream.writeAttribute("source", source);
		stream.stream.writeAttribute("target", target);
		stream.stream.writeAttribute("type", directed ? "directed" : "undirected");

		if (!double.IsNaN(weight))
			stream.stream.writeAttribute("weight", double.toString(weight));

		if (root.isExtensionEnable(Extension.DYNAMICS))
			spells.export(stream);

		attvalues.export(stream);

		stream.endElement(); // EDGE
	}
}

}
