using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.GraphicGraph.StyleSheet
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
/// GraphStream representation of Color Usable by all UI
/// </summary>
public class Color {
	private int r;
	private int g;
	private int b;
	private int a;

	public static readonly Color white = new Color(255, 255, 255);
	public static readonly Color WHITE = white;
	public static readonly Color lightGray = new Color(192, 192, 192);
	public static readonly Color LIGHT_GRAY = lightGray;
	public static readonly Color gray = new Color(128, 128, 128);
	public static readonly Color GRAY = gray;
	public static readonly Color darkGray = new Color(64, 64, 64);
	public static readonly Color DARK_GRAY = darkGray;
	public static readonly Color black = new Color(0, 0, 0);
	public static readonly Color BLACK = black;
	public static readonly Color red = new Color(255, 0, 0);
	public static readonly Color RED = red;
	public static readonly Color pink = new Color(255, 175, 175);
	public static readonly Color PINK = pink;
	public static readonly Color orange = new Color(255, 200, 0);
	public static readonly Color ORANGE = orange;
	public static readonly Color yellow = new Color(255, 255, 0);
	public static readonly Color YELLOW = yellow;
	public static readonly Color green = new Color(0, 255, 0);
	public static readonly Color GREEN = green;
	public static readonly Color magenta = new Color(255, 0, 255);
	public static readonly Color MAGENTA = magenta;
	public static readonly Color cyan = new Color(0, 255, 255);
	public static readonly Color CYAN = cyan;
	public static readonly Color blue = new Color(0, 0, 255);
	public static readonly Color BLUE = blue;

	public Color(int r, int g, int b, int a) : base() {
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public Color(int r, int g, int b) : base() {
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = 255;
	}

	public int getRed() {
		return r;
	}

	public void setRed(int r) {
		this.r = r;
	}

	public int getGreen() {
		return g;
	}

	public void setGreen(int g) {
		this.g = g;
	}

	public int getBlue() {
		return b;
	}

	public void setBlue(int b) {
		this.b = b;
	}

	public int getAlpha() {
		return a;
	}

	public void setAlpha(int a) {
		this.a = a;
	}

	public static Color decode(string nm){
		int intval = int.decode(nm);
		int i = intval;
		return new Color((i >> 16) & 0xFF, (i >> 8) & 0xFF, i & 0xFF);
	}

	
	public bool equals(object o) {
		Color c = (Color) o;
		return (this.r == c.r && this.g == c.g && this.b == c.b && this.a == c.a);
	}
}

}
