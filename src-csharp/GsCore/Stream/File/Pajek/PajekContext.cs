using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File.Pajek
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


public class PajekContext {
	FileSourcePajek pajek;
	string sourceId;

	protected bool directed = false;

	protected string weightAttributeName = "weight";

	public PajekContext(FileSourcePajek pajek) {
		this.pajek = pajek;
		this.sourceId = string.Format("<Pajek stream {0}>", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
	}

	protected void setDirected(bool on) {
		directed = on;
	}

	protected int addNodes(Token nb){
		int n = getInt(nb);

		for (int i = 1; i <= n; ++i) {
			pajek.sendNodeAdded(sourceId, string.Format("{0}", i));
		}

		return n;
	}

	protected void addGraphAttribute(string attr, string value) {
		pajek.sendAttributeChangedEvent(sourceId, sourceId, ElementType.GRAPH, attr, AttributeChangeEvent.ADD, null,
				value);
	}

	protected void addNodeLabel(string nb, string label) {
		pajek.sendAttributeChangedEvent(sourceId, nb, ElementType.NODE, "ui.label", AttributeChangeEvent.ADD, null,
				label);
	}

	protected void addNodeGraphics(string id, NodeGraphics graphics) {
		pajek.sendAttributeChangedEvent(sourceId, id, ElementType.NODE, "ui.style", AttributeChangeEvent.ADD, null,
				graphics.getStyle());
	}

	protected void addNodePosition(string id, Token x, Token y, Token z){
		object pos[] = new object[3];
		pos[0] = (double) getReal(x);
		pos[1] = (double) getReal(y);
		pos[2] = z != null ? (double) getReal(z) : 0;

		pajek.sendAttributeChangedEvent(sourceId, id, ElementType.NODE, "xyz", AttributeChangeEvent.ADD, null, pos);
	}

	protected string addEdge(string src, string trg) {
		string id = string.Format("{0}_{1}_{2}", src, trg, (long) (new Random().NextDouble() * 100000) + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

		pajek.sendEdgeAdded(sourceId, id, src, trg, directed);

		return id;
	}

	protected void addEdges(EdgeMatrix mat) {
		int size = mat.Count;
		int edgeid = 0;

		for (int line = 0; line < size; line++) {
			for (int col = 0; col < size; col++) {
				if (mat.hasEdge(line, col)) {
					string id = string.Format("{0}_{1}_{2}", line + 1, col + 1, edgeid++);
					if (mat.hasEdge(col, line)) {
						pajek.sendEdgeAdded(sourceId, id, string.Format("{0}", line + 1), string.Format("{0}", col + 1),
								false);
						mat.set(col, line, false);
					} else {
						pajek.sendEdgeAdded(sourceId, id, string.Format("{0}", line + 1), string.Format("{0}", col + 1),
								true);
					}
				}
			}
		}
	}

	protected void addEdgeWeight(string id, Token nb){
		pajek.sendAttributeChangedEvent(sourceId, id, ElementType.EDGE, weightAttributeName, AttributeChangeEvent.ADD,
				null, getReal(nb));
	}

	protected void addEdgeGraphics(string id, EdgeGraphics graphics) {
		pajek.sendAttributeChangedEvent(sourceId, id, ElementType.EDGE, "ui.style", AttributeChangeEvent.ADD, null,
				graphics.getStyle());
	}

	protected static int getInt(Token nb){
		try {
			return int.Parse(nb.image);
		} catch (Exception e) {
			throw new ParseException(string.Format("{0} {1} {2} not an integer", nb.beginLine, nb.beginColumn, nb.image));
		}
	}

	protected static double getReal(Token nb){
		try {
			return double.Parse(nb.image);
		} catch (Exception e) {
			throw new ParseException(string.Format("{0} {1} {2} not a real", nb.beginLine, nb.beginColumn, nb.image));
		}
	}

	public static string toColorValue(Token R, Token G, Token B){
		double r = getReal(R);
		double g = getReal(G);
		double b = getReal(B);

		return string.Format("rgb({0}, {1}, {2})", (int) (r * 255), (int) (g * 255), (int) (b * 255));
	}
}

abstract class Graphics {
	protected StringBuffer graphics = new StringBuffer();

	public abstract void addKey(string key, string value, Token tk);

	public string getStyle() {
		return graphics.ToString();
	}

	protected double getReal(string nb, Token tk){
		try {
			return double.Parse(nb);
		} catch (Exception e) {
			throw new ParseException(string.Format("{0} {1} {2} not a real", tk.beginLine, tk.beginColumn, nb));
		}
	}

	protected int getInt(string nb, Token tk){
		try {
			return int.Parse(nb);
		} catch (Exception e) {
			throw new ParseException(string.Format("{0} {1} {2} not an integer", tk.beginLine, tk.beginColumn, nb));
		}
	}
}

class NodeGraphics : Graphics {
	
	public void addKey(string key, string value, Token tk){
		if (key.Equals("shape")) {
			graphics.Append(string.Format("shape {0};", value));
		} else if (key.Equals("ic")) {
			graphics.Append(string.Format("fill-color {0};", value));
		} else if (key.Equals("bc")) {
			graphics.Append(string.Format("stroke-color {0}; stroke-mode: plain;", value));
		} else if (key.Equals("bw")) {
			graphics.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "stroke-width: {0}px;", getReal(value, tk)));
		} else if (key.Equals("s_size")) {
			graphics.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "size: {0}px;", getReal(value, tk)));
		} else if (key.Equals("lc")) {
			graphics.Append(string.Format("text-color {0};", value));
		} else if (key.Equals("fos")) {
			graphics.Append(string.Format("text-size: {0};", getInt(value, tk)));
		} else if (key.Equals("font")) {
			graphics.Append(string.Format("text-font {0};", value));
		}
	}
}

class EdgeGraphics : Graphics {
	
	public void addKey(string key, string value, Token tk){
		if (key.Equals("w")) {
			graphics.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "size: {0}px;", getReal(value, tk)));
		} else if (key.Equals("c")) {
			graphics.Append(string.Format("fill-color {0};", value));
		} else if (key.Equals("s")) {
			double s = getReal(value, tk);
			graphics.Append(string.Format("arrow-size {0}px, {1}px;", s * 5, s * 3));
		} else if (key.Equals("l")) {
			// ?
		} else if (key.Equals("p")) {
			// ?
		} else if (key.Equals("lc")) {
			graphics.Append(string.Format("text-color {0};", value));
		} else if (key.Equals("fos")) {
			graphics.Append(string.Format("text-size: {0};", getInt(value, tk)));
		} else if (key.Equals("font")) {
			graphics.Append(string.Format("text-font {0};", value));
		}
	}
}

class EdgeMatrix {
	// Line first, col second.
	// Line = from node, col = to node.
	protected bool mat[][];

	protected int curLine = 0;

	public EdgeMatrix(int size) {
		mat = new bool[size][size]; // Horror !
	}

	public int size() {
		return mat.Length;
	}

	public bool hasEdge(int line, int col) {
		return mat[line][col];
	}

	public void set(int line, int col, bool value) {
		mat[line][col] = value;
	}

	public void addLine(List<string> line) {
		if (curLine < mat.Length) {
			if (line.Count == mat.Length) {
				for (int i = 0; i < mat.Length; i++) {
					mat[curLine][i] = line[i].Equals("1");
				}
				curLine++;
			} else if (line.Count == mat.Length * mat.Length) {
				int n = mat.Length * mat.Length;
				curLine = -1;
				for (int i = 0; i < n; i++) {
					if (i % mat.Length == 0)
						curLine++;
					mat[curLine][i - (curLine * mat.Length)] = line[i].Equals("1");
				}
			}
		}
	}

	
	public string toString() {
		StringBuffer buffer = new StringBuffer();
		for (int line = 0; line < mat.Length; line++) {
			for (int col = 0; col < mat.Length; col++) {
				buffer.Append(string.Format("{0} ", mat[line][col] ? "1" : "0"));
			}
			buffer.Append(string.Format("\n"));
		}

		return buffer.ToString();
	}
}

}
