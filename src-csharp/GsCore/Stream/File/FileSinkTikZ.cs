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
/// An export of a graph to PGF/TikZ format. <a>http://sourceforge.net/projects/pgf/</a> This allows to include graph in a latex document. Only <code>writeAll(Graph,*)</code> is working, dynamics is not handle. If the exported graph is a GraphicGraph, then CSS style of the graph will be used. For a better rendering, it is strongly recommended to run previously a layout algorithm that will add coordinates on nodes. Else, random coordinates will be choosen for nodes. Layout can be run in this way : <code> Graph g; ... SpringBox sbox = new SpringBox(); g.addSink(sbox); sbox.addAttributeSink(g); do sbox.compute(); while (sbox.getStabilization() < 0.9); g.removeSink(sbox); sbox.remoteAttributeSink(g); </code> TikZ pictures are scalable so pixel units is not handle here. The picture is bounded in a box which width and height can be defined by adding attributes to the graph: <ul> <li>"ui.tikz.width"</li> <li>"ui.tikz.height"</li> </ul> The value of these attributes has to be considered as centimeters. Common supported style : <ul> <li>"fill-color", alpha is supported to</li> <li>"size" in "gu"</li> </ul> Node supported style : <ul> <li>"shape" with "box", "rounded-box", "circle", "triangle", "diamond"</li> <li>"stroke-mode" with "plain"</li> <li>"stroke-color", alpha is supported to</li> <li>"stroke-width" in "gu"</li> </ul> Edge supported style : <ul> </ul>
/// </summary>
public class FileSinkTikZ : FileSinkBase {
	private static readonly object /* Logger */ LOGGER = null /* Logger */;

	/// <summary>
/// Node attribute storing coordinates.
/// </summary>
	public static readonly string XYZ_ATTR = "xyz";

	/// <summary>
/// Graph attribute storing width of the TikZ picture.
/// </summary>
	public static readonly string WIDTH_ATTR = "ui.tikz.width";

	/// <summary>
/// Graph attribute storing height of the TikZ picture.
/// </summary>
	public static readonly string HEIGHT_ATTR = "ui.tikz.height";

	public static readonly double DEFAULT_WIDTH = 10;

	public static readonly double DEFAULT_HEIGHT = 10;

	/// <summary>
/// Define the default minimum size of nodes when using a dynamic size. This size is in millimeter.
/// </summary>
	public static readonly double DISPLAY_MIN_SIZE_IN_MM = 2;

	/// <summary>
/// Define the default maximum size of nodes when using a dynamic size. This size is in millimeter.
/// </summary>
	public static readonly double DISPLAY_MAX_SIZE_IN_MM = 10;

	protected System.IO.StreamWriter out;

	protected Dictionary<string, string> colors = new Dictionary<string, string>();
	protected Dictionary<string, string> classes = new Dictionary<string, string>();
	protected Dictionary<string, string> classNames = new Dictionary<string, string>();

	protected int classIndex = 0;
	protected int colorIndex = 0;

	protected double width = double.NaN;
	protected double height = double.NaN;

	protected bool layout = false;

	protected GraphicGraph buffer;

	protected string css = null;

	protected double minSize = 0;

	protected double maxSize = 0;

	protected double displayMinSize = DISPLAY_MIN_SIZE_IN_MM;

	protected double displayMaxSize = DISPLAY_MAX_SIZE_IN_MM;

	private double xmin, ymin, xmax, ymax;

	private PointsWrapper points;
	private Locale l = System.Globalization.CultureInfo.InvariantCulture;

	protected static string formatId(string id) {
		return "node" + id.Replace("\\W", "_");
	}

	public FileSinkTikZ() {
		buffer = new GraphicGraph("tikz-buffer");
	}

	public double getWidth() {
		return width;
	}

	public void setWidth(double width) {
		this.width = width;
	}

	public double getHeight() {
		return height;
	}

	public void setHeight(double height) {
		this.height = height;
	}

	public void setDisplaySize(double min, double max) {
		this.displayMinSize = min;
		this.displayMaxSize = max;
	}

	public void setCSS(string css) {
		this.css = css;
	}

	public void setLayout(bool layout) {
		this.layout = layout;
	}

	protected double getNodeX(INode n) {
		if (n.hasAttribute(XYZ_ATTR))
			return ((IConvertible) (n.getArray(XYZ_ATTR)[0]));

		if (n.hasAttribute("x"))
			return n.getNumber("x");

		return double.NaN;
	}

	protected double getNodeY(INode n) {
		if (n.hasAttribute(XYZ_ATTR))
			return ((IConvertible) (n.getArray(XYZ_ATTR)[1]));

		if (n.hasAttribute("y"))
			return n.getNumber("y");

		return double.NaN;
	}

	protected string getNodeStyle(INode n) {
		string style = "tikzgsnode";

		if (n is GraphicNode) {
			GraphicNode gn = (GraphicNode) n;

			style = classNames[gn.style.getId()];

			if (gn.style.getFillMode() == FillMode.DYN_PLAIN) {
				double uicolor = gn.getNumber("ui.color");

				if (double.IsNaN(uicolor))
					uicolor = 0;

				int c = gn.style.getFillColorCount();
				int s = 1;
				double d = 1.0 / (c - 1);

				while (s * d < uicolor && s < c)
					s++;

				uicolor -= (s - 1) * d;
				uicolor *= c;

				style += string.Format(System.Globalization.CultureInfo.InvariantCulture, ", fill={0}!{1}!{2}", checkColor(gn.style.getFillColor(0)),
						(int) (uicolor * 100), checkColor(gn.style.getFillColor(1)));
			}

			if (gn.style.getSizeMode() == SizeMode.DYN_SIZE) {
				double uisize = gn.getNumber("ui.size");

				if (double.IsNaN(uisize))
					uisize = minSize;

				uisize = (uisize - minSize) / (maxSize - minSize);
				uisize = uisize * (displayMaxSize - displayMinSize) + displayMinSize;

				style += string.Format(System.Globalization.CultureInfo.InvariantCulture, ", minimum size={0}mm", uisize);
			}
		}

		return style;
	}

	protected string getEdgeStyle(IEdge e) {
		string style = "tikzgsnode";

		if (e is GraphicEdge) {
			GraphicEdge ge = (GraphicEdge) e;

			style = classNames[ge.style.getId()];

			if (ge.style.getFillMode() == FillMode.DYN_PLAIN) {
				double uicolor = ge.getNumber("ui.color");

				if (double.IsNaN(uicolor))
					uicolor = 0;

				int c = ge.style.getFillColorCount();
				int s = 1;
				double d = 1.0 / (c - 1);

				while (s * d < uicolor && s < c)
					s++;

				uicolor -= (s - 1) * d;
				uicolor *= c;

				style += string.Format(System.Globalization.CultureInfo.InvariantCulture, ", draw={0}!{1}!{2}", checkColor(ge.style.getFillColor(s - 1)),
						(int) (uicolor * 100), checkColor(ge.style.getFillColor(s)));
			}

			if (ge.style.getSizeMode() == SizeMode.DYN_SIZE) {
				double uisize = ge.getNumber("ui.size");

				if (double.IsNaN(uisize) || uisize < 0.01)
					uisize = 1;

				style += string.Format(System.Globalization.CultureInfo.InvariantCulture, ", line width={0}pt", uisize);
			}
		}

		return style;
	}

	protected string checkColor(Color c) {
		string rgb = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", c.getRed() / 255.0f, c.getGreen() / 255.0f,
				c.getBlue() / 255.0f);

		if (colors.ContainsKey(rgb))
			return colors[rgb];

		string key = string.Format("tikzC{0}", colorIndex++);
		colors[rgb] = key;

		return key;
	}

	/// <summary>
/// Convert a StyleGroup to tikz style.
/// </summary>
/// <param name="group"> the style group to convert</param>
/// <returns>string representation of the style group usable in TikZ.</returns>
	protected string getTikzStyle(StyleGroup group) {
		System.Text.StringBuilder buffer = new System.Text.StringBuilder();
		List<string> style = new List<string>();

		for (int i = 0; i < group.getFillColorCount(); i++)
			checkColor(group.getFillColor(i));

		switch (group.getType()) {
		case NODE {
			if (group.getFillMode() != FillMode.DYN_PLAIN) {
				string fill = checkColor(group.getFillColor(0));
				style.Add("fill=" + fill);
			}

			if (group.getFillColor(0).getAlpha() < 255)
				style.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "fill opacity={0}", group.getFillColor(0).getAlpha() / 255.0f));

			switch (group.getStrokeMode()) {
			case DOTS:
			case DASHES:
			case PLAIN:
				string stroke = checkColor(group.getStrokeColor(0));
				style.Add("draw=" + stroke);
				style.Add("line width=" + string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}pt", group.getStrokeWidth().value));

				if (group.getStrokeColor(0).getAlpha() < 255)
					style.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "draw opacity={0}",
							group.getStrokeColor(0).getAlpha() / 255.0f));

				break;
			default:
				Console.Error.WriteLine(string.Format("unhandled stroke mode  {0}\n", group.getStrokeMode()));
			}

			switch (group.getShape()) {
			case CIRCLE:
				style.Add("circle");
				break;
			case ROUNDED_BOX:
				style.Add("rounded corners=2pt");
			case BOX:
				style.Add("rectangle");
				break;
			case TRIANGLE:
				style.Add("isosceles triangle");
				break;
			case DIAMOND:
				style.Add("diamond");
				break;
			default:
				Console.Error.WriteLine(string.Format("unhandled shape  {0}\n", group.getShape()));
			}

			string text = checkColor(group.getTextColor(0));
			style.Add("text=" + text);

			switch (group.getSize().units) {
			case GU:
				style.Add("minimum size=" + string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}cm", group.getSize().values[0]));
				break;
			case PX:
				style.Add("minimum size=" + string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}pt", group.getSize().values[0]));
				break;
			default:
				Console.Error.WriteLine(
						string.Format("% [warning] units {0} are not compatible with TikZ.\n", group.getSize().units));
			}

			style.Add("inner sep=0pt");
		}
			break;
		case EDGE {
			if (group.getFillMode() != FillMode.DYN_PLAIN) {
				string fill = checkColor(group.getFillColor(0));
				style.Add("draw=" + fill);
			}

			if (group.getFillColor(0).getAlpha() < 255)
				style.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "draw opacity={0}", group.getFillColor(0).getAlpha() / 255.0f));

			switch (group.getSize().units) {
			case PX:
			case GU:
				style.Add("line width=" + string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}pt", group.getSize().values[0]));
				break;
			default:
				Console.Error.WriteLine(
						string.Format("% [warning] units {0} are not compatible with TikZ.\n", group.getSize().units));
			}
		}
			break;
		default:
			Console.Error.WriteLine(string.Format("unhandled group type  {0}\n", group.getType()));
		}

		for (int i = 0; i < style.Count; i++) {
			if (i > 0)
				buffer.Append(",");

			buffer.Append(style[i]);
		}

		return buffer.ToString();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSinkBase#outputHeader()
	 */
	protected void outputHeader(){
		out = (System.IO.StreamWriter) output;

		colors.Clear();
		classes.Clear();
		classNames.Clear();

		buffer.Clear();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSinkBase#outputEndOfFile()
	 */
	protected void outputEndOfFile(){
		if (double.IsNaN(width)) {
			if (buffer.hasNumber(WIDTH_ATTR))
				width = buffer.getNumber(WIDTH_ATTR);
			else
				width = DEFAULT_WIDTH;
		}

		if (double.IsNaN(height)) {
			if (buffer.hasNumber(HEIGHT_ATTR))
				height = buffer.getNumber(HEIGHT_ATTR);
			else
				height = DEFAULT_WIDTH;
		}

		checkLayout();

		if (css != null)
			buffer.setAttribute("ui.stylesheet", css);

		points = new PointsWrapper();

		//
		// Begin tikzpicture
		//
		output.printf("%%%n%% Do not forget \\usepackage{tikz} in header.%n%%%n");
		output.printf("\\begin{tikzpicture}");

		checkAndOutputStyle();
		checkXYandSize();

		buffer.nodes().ToList().ForEach(n => {
			double x, y;

			x = getNodeX(n);
			y = getNodeY(n);

			if (double.IsNaN(x) || double.IsNaN(y)) {
				x = new Random().NextDouble() * width;
				y = new Random().NextDouble() * height;
			} else {
				x = width * (x - xmin) / (xmax - xmin);
				y = height * (y - ymin) / (ymax - ymin);
			}

			output.printf(l, "\t\\node[inner sep=0pt] ({0}) at ({1},{2}) {};\n", formatId(n.getId()), x, y);
		});

		StyleGroupSet sgs = buffer.getStyleGroups();

		foreach (HashSet<StyleGroup> groups in sgs.zIndex()) {
			foreach (StyleGroup group in groups) {
				switch (group.getType()) {
				case NODE:
					foreach (IElement e in group.elements())
						outputNode((INode) e);
					break;
				case EDGE:
					foreach (IElement e in group.elements())
						outputEdge((IEdge) e);
					break;
				default:
				}
			}
		}

		//
		// End of tikzpicture.
		//
		output.printf("\\end{tikzpicture}%n");
	}

	private void checkLayout() {
		if (!layout)
			return;

		SpringBox sbox = new SpringBox();

		GraphReplay replay = new GraphReplay("replay");
		replay.addSink(sbox);
		sbox.addAttributeSink(buffer);

		replay.replay(buffer);

		do
			sbox.compute();
		while (sbox.getStabilization() < 0.9);

		buffer.removeSink(sbox);
		sbox.removeAttributeSink(buffer);
	}

	private void checkXYandSize() {
		xmin = ymin = double.MaxValue;
		xmax = ymax = double.Epsilon;

		buffer.nodes().ToList().ForEach(n => {
			double x, y;

			x = getNodeX(n);
			y = getNodeY(n);

			if (!double.IsNaN(x) && !double.IsNaN(y)) {
				xmin = Math.Min(xmin, x);
				xmax = Math.Max(xmax, x);
				ymin = Math.Min(ymin, y);
				ymax = Math.Max(ymax, y);
			} else {
				Console.Error.WriteLine(string.Format("% [warning] missing node (x,y).\n"));
			}

			if (n.hasNumber("ui.size")) {
				minSize = Math.Min(minSize, n.getNumber("ui.size"));
				maxSize = Math.Max(maxSize, n.getNumber("ui.size"));
			}
		});

		if (minSize == maxSize)
			maxSize += 1;

		buffer.edges().ToList().ForEach(e => {
			points.setElement(e);

			if (points.check()) {
				for (int i = 0; i < points.getPointsCount(); i++) {
					double x = points.getX(i);
					double y = points.getY(i);

					xmin = Math.Min(xmin, x);
					xmax = Math.Max(xmax, x);
					ymin = Math.Min(ymin, y);
					ymax = Math.Max(ymax, y);
				}
			}
		});
	}

	private void checkAndOutputStyle() {
		string nodeStyle = "circle,draw=black,fill=black";
		string edgeStyle = "draw=black";
		StyleGroupSet sgs = buffer.getStyleGroups();

		foreach (StyleGroup sg in sgs.groups()) {
			string key = string.Format("class{0}", classIndex++);
			classNames[sg.getId()] = key;
			classes[key] = getTikzStyle(sg);
		}

		output.printf("[%n");

		foreach (string key in classes.Keys)
			output.printf(l, "\t{0}/.style={{1}},\n", key, classes[key]);

		output.printf(l, "\ttikzgsnode/.style={{0}},\n", nodeStyle);
		output.printf(l, "\ttikzgsedge/.style={{0}}\n", edgeStyle);

		output.printf("]%n");

		foreach (string rgb in colors.Keys)
			output.printf(l, "\t\\definecolor{{0}}{rgb}{{1}}\n", colors[rgb], rgb);
	}

	private void outputNode(INode n) {
		string label;
		string style = getNodeStyle(n);

		label = n.hasAttribute("label") ? (string) n.getLabel("label")
				: (n.hasAttribute("ui.label") ? (string) n.getLabel("ui.label") : "");

		output.printf(l, "\t\\node[{0}] at ({1}) {{2}};\n", style, formatId(n.getId()), label);
	}

	private void outputEdge(IEdge e) {
		string style = getEdgeStyle(e);
		string uiPoints = "";
		points.setElement(e);

		if (points.check()) {
			for (int i = 0; i < points.getPointsCount(); i++) {
				double x, y;

				x = points.getX(i);
				y = points.getY(i);
				x = width * (x - xmin) / (xmax - xmin);
				y = height * (y - ymin) / (ymax - ymin);

				uiPoints = string.Format(l, "{0}-- ({1},{2}) ", uiPoints, x, y);
			}
		}

		output.printf(l, "\t\\draw[{0}] ({1}) {2}{3} ({4});\n", style, formatId(e.getSourceNode().getId()), uiPoints,
				e.isDirected() ? "->" : "--", formatId(e.getTargetNode().getId()));
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang.String ,
	 * long, java.lang.String, java.lang.object)
	 */
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		buffer.graphAttributeAdded(sourceId, timeId, attribute, value);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang.
	 * String, long, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		buffer.graphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang.
	 * String, long, java.lang.String)
	 */
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		buffer.graphAttributeRemoved(sourceId, timeId, attribute);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		buffer.nodeAttributeAdded(sourceId, timeId, nodeId, attribute, value);
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
		buffer.nodeAttributeChanged(sourceId, timeId, nodeId, attribute, oldValue, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		buffer.nodeAttributeRemoved(sourceId, timeId, nodeId, attribute);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		buffer.edgeAttributeAdded(sourceId, timeId, edgeId, attribute, value);
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
		buffer.edgeAttributeChanged(sourceId, timeId, edgeId, attribute, oldValue, newValue);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		buffer.edgeAttributeRemoved(sourceId, timeId, edgeId, attribute);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeAdded(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		buffer.nodeAdded(sourceId, timeId, nodeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		buffer.nodeRemoved(sourceId, timeId, nodeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeAdded(java.lang.String, long,
	 * java.lang.String, java.lang.String, java.lang.String, boolean)
	 */
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		buffer.edgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		buffer.edgeRemoved(sourceId, timeId, edgeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#graphCleared(java.lang.String, long)
	 */
	public void graphCleared(string sourceId, long timeId) {
		buffer.graphCleared(sourceId, timeId);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#stepBegins(java.lang.String, long,
	 * double)
	 */
	public void stepBegins(string sourceId, long timeId, double step) {
		buffer.stepBegins(sourceId, timeId, step);
	}

	protected class PointsWrapper {
		object[] points;

		PointsWrapper() {
		}

		public void setElement(IElement e) {
			if (e.hasArray("ui.points"))
				points = e.getArray("ui.points");
			else
				points = null;
		}

		public bool check() {
			if (points == null)
				return false;

			for (int i = 0; i < points.Length; i++) {
				if (!(points[i] is Point3) && !points[i].GetType().IsArray)
					return false;
			}

			return true;
		}

		public int getPointsCount() {
			return points == null ? 0 : points.Length;
		}

		public double getX(int i) {
			if (points == null || i >= points.Length)
				return double.NaN;

			object p = points[i];

			if (p is Point3)
				return ((Point3) p).x;
			else {
				object x = Array[p, 0];

				if (x is IConvertible)
					return ((IConvertible) x);
				else
					return Array.getDouble(p, 0);
			}
		}

		public double getY(int i) {
			if (i >= points.Length)
				return double.NaN;

			object p = points[i];

			if (p is Point3)
				return ((Point3) p).y;
			else {
				object y = Array[p, 0];

				if (y is IConvertible)
					return ((IConvertible) y);
				else
					return Array.getDouble(p, 1);
			}
		}
	}
}

}
