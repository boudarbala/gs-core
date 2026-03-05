using System.Collections.Generic;
using System.Linq;
using System.Text;
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


public class GEXFAttValues : IGEXFElement {
	GEXF root;
	Dictionary<int, List<GEXFAttValue>> values;

	public GEXFAttValues(GEXF root) {
		this.root = root;
		this.values = new Dictionary<int, List<GEXFAttValue>>();
	}

	public void attributeUpdated(GEXFAttribute decl, object value) {
		if (!values.ContainsKey(decl.id))
			values[decl.id] = new List<GEXFAttValue>();

		List<GEXFAttValue> attr = values[decl.id];

		if (value != null) {
			if (attr.Count > 0) {
				if (attr.getLast().start == root.step)
					attr.removeLast();
				else
					attr.getLast().end = root.step;
			}

			GEXFAttValue av = new GEXFAttValue(root, int.toString(decl.id), formatValue(value));
			attr.Add(av);
		} else {
			if (attr.Count > 0)
				attr.getLast().end = root.step;
		}
	}

	string formatValue(object o) {
		if (o == null)
			return "<null>";

		if (o.GetType().IsArray) {
			System.Text.System.Text.StringBuilder buffer = new System.Text.System.Text.StringBuilder();

			for (int i = 0; i < Array.getLength(o); i++) {
				object ochild = Array[o, i];

				if (i > 0)
					buffer.Append("|");

				if (ochild != null)
					buffer.Append(ochild.ToString());
			}

			o = buffer;
		}

		return o.ToString();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.gexf.GEXFElement#export(org.graphstream.stream
	 * .file.gexf.SmartXMLWriter)
	 */
	public void export(SmartXMLWriter stream){
		if (values.Count == 0)
			return;

		stream.startElement("attvalues");

		foreach (List<GEXFAttValue> attrValues in values.Values) {
			for (int i = 0; i < attrValues.Count; i++)
				attrValues[i].export(stream);
		}

		stream.endElement(); // ATTVALUES
	}
}

}
