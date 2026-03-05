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


public class GEXFAttributes : IGEXFElement, IAttributeSink {
	GEXF root;

	ClassType type;
	Mode mode;

	Dictionary<string, GEXFAttribute> attributes;

	public GEXFAttributes(GEXF root, ClassType type) {
		this.root = root;

		this.type = type;
		this.mode = Mode.STATIC;
		this.attributes = new Dictionary<string, GEXFAttribute>();

		root.addAttributeSink(this);
	}

	protected void checkAttribute(string key, object value) {
		AttrType type = detectType(value);

		if (!attributes.ContainsKey(key))
			attributes[key] = new GEXFAttribute(root, key, type);
		else {
			GEXFAttribute a = attributes[key];

			if (a.type != type && value != null)
				a.type = AttrType.STRING;
		}
	}

	protected AttrType detectType(object value) {
		if (value == null)
			return AttrType.STRING;

		if (value is int || value is short)
			return AttrType.INTEGER;
		else if (value is long)
			return AttrType.LONG;
		else if (value is float)
			return AttrType.FLOAT;
		else if (value is double)
			return AttrType.DOUBLE;
		else if (value is bool)
			return AttrType.BOOLEAN;
		else if (value is System.Uri || value is System.Uri)
			return AttrType.ANYURI;
		else if (value.GetType().IsArray || value is Collection)
			return AttrType.LISTSTRING;

		return AttrType.STRING;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.gexf.GEXFElement#export(org.graphstream.stream
	 * .file.gexf.SmartXMLWriter)
	 */
	public void export(SmartXMLWriter stream){
		if (attributes.Count == 0)
			return;

		stream.startElement("attributes");
		stream.stream.WriteAttributeString("class", type.qname);

		foreach (GEXFAttribute attribute in ((attributes[])Enum.GetValues(typeof(attributes))))
			attribute.export(stream);

		stream.endElement(); // ATTRIBUTES
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		if (type == ClassType.NODE)
			checkAttribute(attribute, value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		if (type == ClassType.NODE)
			checkAttribute(attribute, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		if (type == ClassType.EDGE)
			checkAttribute(attribute, value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		if (type == ClassType.EDGE)
			checkAttribute(attribute, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang.String ,
	 * long, java.lang.String, java.lang.object)
	 */
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang.
	 * String, long, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang.
	 * String, long, java.lang.String)
	 */
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
	}
}

}
