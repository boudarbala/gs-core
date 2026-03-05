using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
/// Transforms a graph into a SVG description. <p> Do not confuse this with the SVG export capabilities of the graph viewer. The SVG export of the viewer provides the most exact copy of what you see on screen. This class is made to export only nodes and edges without styling to SVG. </p> <p> Although there is no styling, each node and edge is put in a SVG group with the identifier of the corresponding element in the graph. A minimal CSS style sheet is included in the generated file and it is easy to add another. </p>
/// </summary>
public class FileSinkUnstyledSVG : FileSinkBase {
	// Attribute

	/// <summary>
/// The output.
/// </summary>
	protected System.IO.StreamWriter out;

	/// <summary>
/// What element ?.
/// </summary>
	protected enum What {
		NODE, EDGE, OTHER
	}

	/// <summary>
/// The positions of each node.
/// </summary>
	protected Dictionary<string, Point3> nodePos = new Dictionary<string, Point3>();

	// Construction

	public FileSinkUnstyledSVG {
		// NOP.
	}

	// Command

	
	public void end throws System.IO.IOException {
		if {
			output.flush;
			output.Close();
			out = null;
		}
	}

	// Command

	
	protected void outputHeader(){
		out = (System.IO.StreamWriter) output;

		output.printf("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>%n");
		output.printf("<svg" + " xmlns:svg=\"http://www.w3.org/2000/svg\"" + " width=\"100%%\"" + " height=\"100%%\""
				+ ">%n");

		// TODO
		// outputStyle( styleSheet );
	}

	
	protected void outputEndOfFile(){
		outputNodes();
		output.printf("</svg>%n");
	}

	public void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value) {
		// NOP
	}

	public void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		// NOP
	}

	public void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute) {
		// NOP
	}

	public void graphAttributeAdded(string graphId, long timeId, string attribute, object value) {
		// NOP
	}

	public void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue) {
		// NOP
	}

	public void graphAttributeRemoved(string graphId, long timeId, string attribute) {
		// NOP
	}

	public void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value) {
		setNodePos(nodeId, attribute, value);
	}

	public void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		setNodePos(nodeId, attribute, newValue);
	}

	public void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute) {
		// NOP
	}

	public void edgeAdded(string graphId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		Point3 p0 = nodePos[fromNodeId];
		Point3 p1 = nodePos[toNodeId];

		if (p0 != null && p1 != null) {
			output.printf("  <g id=\"{0}\">%n", edgeId);
			output.printf("    <line x1=\"%f\" y1=\"%f\" x2=\"%f\" y2=\"%f\"/>%n", p0.x, p0.y, p1.x, p1.y);
			output.printf("  </g>%n");
		}
	}

	public void edgeRemoved(string graphId, long timeId, string edgeId) {
		// NOP
	}

	public void graphCleared(string graphId, long timeId) {
		// NOP
	}

	public void nodeAdded(string graphId, long timeId, string nodeId) {
		nodePos[nodeId] = new Point3();
	}

	public void nodeRemoved(string graphId, long timeId, string nodeId) {
		nodePos.Remove(nodeId);
	}

	public void stepBegins(string graphId, long timeId, double time) {
		// NOP
	}

	// Utility

	protected void setNodePos(string nodeId, string attribute, object value) {
		Point3 p = nodePos[nodeId];

		double x, y, z;
		if (p == null) {
			x = new Random().NextDouble();
			y = new Random().NextDouble();
			z = 0;
		} else {
			x = p.x;
			y = p.y;
			z = p.z;
		}

		if (attribute.Equals("x")) {
			if (value is IConvertible)
				x = ((IConvertible) value);
		} else if (attribute.Equals("y")) {
			if (value is IConvertible)
				y = ((IConvertible) value);
		} else if (attribute.Equals("z")) {
			if (value is IConvertible)
				z = ((IConvertible) value);
		}

		else if (attribute.Equals("xy")) {
			if (value is object[]) {
				object xy[] = ((object[]) value);

				if (xy.Length > 1) {
					x = ((IConvertible) xy[0]);
					y = ((IConvertible) xy[1]);
				}
			}
		} else if (attribute.Equals("xyz")) {
			if (value is object[]) {
				object xyz[] = ((object[]) value);

				if (xyz.Length > 1) {
					x = ((IConvertible) xyz[0]);
					y = ((IConvertible) xyz[1]);
				}

				if (xyz.Length > 2) {
					z = ((IConvertible) xyz[2]);
				}
			}
		}
		nodePos[nodeId] = new Point3(x, y, z);
	}

	protected void outputStyle(string styleSheet) {
		string style = null;

		if (styleSheet != null) {
			StyleSheet ssheet = new StyleSheet();

			try {
				if (styleSheet.StartsWith("url(")) {
					styleSheet = styleSheet.Substring(5);

					int pos = styleSheet.LastIndexOf(')');

					styleSheet = styleSheet.Substring(0, pos);

					ssheet.parseFromFile(styleSheet);
				} else {
					ssheet.parseFromString(styleSheet);
				}

				style = styleSheetToSVG(ssheet);
			} catch (System.IO.IOException e) {
				Console.Error.WriteLine(e);
				ssheet = null;
			}
		}

		if (style == null)
			style = "circle { fill: grey; stroke: none; } line { stroke-width: 1; stroke: black; }";

		output.printf("<defs><style type=\"text/css\"><![CDATA[%n");
		output.printf("    {0}\n", style);
		output.printf("]]></style></defs>%n");
	}

	protected void outputNodes() {
		IEnumerator<string> keys = nodePos.Keys.GetEnumerator();

		while (keys.MoveNext()) {
			string key = keys.next();
			Point3 pos = nodePos[key];

			output.printf("  <g id=\"{0}\">%n", key);
			output.printf("    <circle cx=\"%f\" cy=\"%f\" r=\"4\"/>%n", pos.x, pos.y);
			output.printf("  </g>%n");
		}
	}

	protected string styleSheetToSVG(StyleSheet sheet) {
		System.Text.StringBuilder output = new System.Text.StringBuilder();

		addRule(output, sheet.getDefaultGraphRule());

		return output.ToString();
	}

	protected void addRule(System.Text.StringBuilder out, Rule rule) {
		// Style style = rule.getStyle();

		// TODO
	}
}
}
