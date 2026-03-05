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
/// The various constants and static constant conversion methods used for styling.
/// </summary>
public class StyleConstants {
	// Constants

	/// <summary>
/// The available units for numerical values.
/// </summary>
	enum Units {
		PX, GU, PERCENTS
	}

	/// <summary>
/// How to fill the contents of the element.
/// </summary>
	enum FillMode {
		NONE, PLAIN, DYN_PLAIN, GRADIENT_RADIAL, GRADIENT_HORIZONTAL, GRADIENT_VERTICAL, GRADIENT_DIAGONAL1, GRADIENT_DIAGONAL2, IMAGE_TILED, IMAGE_SCALED, IMAGE_SCALED_RATIO_MAX, IMAGE_SCALED_RATIO_MIN
	}

	/// <summary>
/// How to draw the contour of the element.
/// </summary>
	enum StrokeMode {
		NONE, PLAIN, DASHES, DOTS, DOUBLE
	}

	/// <summary>
/// How to draw the shadow of the element.
/// </summary>
	enum ShadowMode {
		NONE, PLAIN, GRADIENT_RADIAL, GRADIENT_HORIZONTAL, GRADIENT_VERTICAL, GRADIENT_DIAGONAL1, GRADIENT_DIAGONAL2
	}

	/// <summary>
/// How to show an element.
/// </summary>
	enum VisibilityMode {
		NORMAL, HIDDEN, AT_ZOOM, UNDER_ZOOM, OVER_ZOOM, ZOOM_RANGE, ZOOMS
	}

	/// <summary>
/// How to draw the text of an element.
/// </summary>
	enum TextMode {
		NORMAL, TRUNCATED, HIDDEN
	}

	/// <summary>
/// How to show the text of an element.
/// </summary>
	enum TextVisibilityMode {
		NORMAL, HIDDEN, AT_ZOOM, UNDER_ZOOM, OVER_ZOOM, ZOOM_RANGE, ZOOMS
	}

	/// <summary>
/// Variant of the font.
/// </summary>
	enum TextStyle {
		NORMAL, ITALIC, BOLD, BOLD_ITALIC
	}

	/// <summary>
/// Where to place the icon around the text.
/// </summary>
	enum IconMode {
		NONE, AT_LEFT, AT_RIGHT, UNDER, ABOVE
	}

	/// <summary>
/// How to set the size of the element.
/// </summary>
	enum SizeMode {
		NORMAL, FIT, DYN_SIZE
	}

	/// <summary>
/// How to align words around their attach point.
/// </summary>
	enum TextAlignment {
		CENTER, LEFT, RIGHT, AT_LEFT, AT_RIGHT, UNDER, ABOVE, JUSTIFY,

		ALONG
	}

	enum TextBackgroundMode {
		NONE, PLAIN, ROUNDEDBOX
	}

	enum ShapeKind {
		ELLIPSOID, RECTANGULAR, LINEAR, CURVE
	}

	/// <summary>
/// Possible shapes for elements.
/// </summary>
	enum Shape {
		CIRCLE, BOX, ROUNDED_BOX, DIAMOND(
				ShapeKind.RECTANGULAR), POLYGON, TRIANGLE, CROSS(
						ShapeKind.RECTANGULAR), FREEPLANE, TEXT_BOX(
								ShapeKind.RECTANGULAR), TEXT_ROUNDED_BOX, TEXT_PARAGRAPH(
										ShapeKind.RECTANGULAR), TEXT_CIRCLE, TEXT_DIAMOND(
												ShapeKind.RECTANGULAR), JCOMPONENT,

		PIE_CHART, FLOW, ARROW, IMAGES(
				ShapeKind.RECTANGULAR),

		LINE, ANGLE, CUBIC_CURVE, POLYLINE(
				ShapeKind.LINEAR), POLYLINE_SCALED, SQUARELINE, LSQUARELINE(
						ShapeKind.LINEAR), HSQUARELINE(





		}
	}

	/// <summary>
/// Orientation of a sprite toward its attachment point.
/// </summary>
	enum SpriteOrientation {
		NONE, FROM, NODE0, TO, NODE1, PROJECTION
	}

	/// <summary>
/// Possible shapes for arrows on edges.
/// </summary>
	enum ArrowShape {
		NONE, ARROW, CIRCLE, DIAMOND, IMAGE
	}

	/// <summary>
/// Possible JComponents.
/// </summary>
	enum JComponents {
		BUTTON, TEXT_FIELD, PANEL
	}

	// Static

	/// <summary>
/// A set of colour names mapped to real AWT Colour objects.
/// </summary>
	protected static Dictionary<string, Color> colorMap;

	/// <summary>
/// Pattern to ensure a "#FFFFFF" colour is recognised.
/// </summary>
	protected static Pattern sharpColor1, sharpColor2;

	/// <summary>
/// Pattern to ensure a CSS style "rgb" colour is recognised.
/// </summary>
	protected static Pattern cssColor;

	/// <summary>
/// Pattern to ensure a CSS style "rgba(1,2,3,4)" colour is recognised.
/// </summary>
	protected static Pattern cssColorA;

	/// <summary>
/// Pattern to ensure that java.awt.Color.toString() strings are recognised as colour.
/// </summary>
	protected static Pattern awtColor;

	/// <summary>
/// Pattern to ensure an hexadecimal number is a recognised colour.
/// </summary>
	protected static Pattern hexaColor;

	/// <summary>
/// Pattern to ensure a string is a Value in various units.
/// </summary>
	protected static Pattern numberUnit, number;

	static StyleConstants() {
		// Prepare some pattern matchers.

		number = Pattern.compile("\\s*(\\p{Digit}+([.]\\p{Digit})?)\\s*");
		numberUnit = Pattern.compile("\\s*(\\p{Digit}+(?:[.]\\p{Digit}+)?)\\s*(gu|px|%)\\s*");

		sharpColor1 = Pattern.compile(
				"#(\\p{XDigit}\\p{XDigit})(\\p{XDigit}\\p{XDigit})(\\p{XDigit}\\p{XDigit})((\\p{XDigit}\\p{XDigit})?)");
		sharpColor2 = Pattern.compile("#(\\p{XDigit})(\\p{XDigit})(\\p{XDigit})((\\p{XDigit})?)");
		hexaColor = Pattern.compile(
				"0[xX](\\p{XDigit}\\p{XDigit})(\\p{XDigit}\\p{XDigit})(\\p{XDigit}\\p{XDigit})((\\p{XDigit}\\p{XDigit})?)");
		cssColor = Pattern.compile("rgb\\s*\\(\\s*([0-9]+)\\s*,\\s*([0-9]+)\\s*,\\s*([0-9]+)\\s*\\)");
		cssColorA = Pattern
				.compile("rgba\\s*\\(\\s*([0-9]+)\\s*,\\s*([0-9]+)\\s*,\\s*([0-9]+)\\s*,\\s*([0-9]+)\\s*\\)");
		awtColor = Pattern.compile("java.awt.Color\\[r=([0-9]+),g=([0-9]+),b=([0-9]+)\\]");
		colorMap = new Dictionary<string, Color>();

		// Load all the X11 predefined colour names and their RGB definition
		// from a file stored in the graphstream.jar. This allows the DOT
		// import to correctly map colour names to real AWT Color objects.
		// There are more than 800 such params colours[] System.Uri url = typeof(StyleConstants).getResource("rgb.properties");

		if (url == null)
			throw new Exception(
					"corrupted graphstream.jar ? the org/miv/graphstream/ui/graphicGraph/rgb.properties file is not found");

		Properties p = new Properties();

		try {
			p.load(url.openStream());
		} catch (System.IO.IOException e) {
			Console.Error.WriteLine(e);
		}

		foreach (object o in p.Keys) {
			string key = (string) o;
			string val = p.getProperty(key);
			Color col = Color.decode(val);

			colorMap[key.ToLower()] = col;
		}
	}

	/// <summary>
/// Try to convert the given string value to a colour. It understands the 600 colour names of the X11 RGB data base. It also understands colours given in the "#FFFFFF" format and the hexadecimal "0xFFFFFF" format. Finally, it understands colours given as a "rgb(1,10,100)", CSS-like format. If the input value is null, the result is null.
/// </summary>
/// <param name="anyValue"> The value to convert.</param>
/// <returns>the converted colour or null if the conversion failed.</returns>
	public static Color convertColor(object anyValue) {
		if (anyValue == null)
			return null;

		if (anyValue is Color)
			return (Color) anyValue;

		if (anyValue is string) {
			Color c = null;
			string value = (string) anyValue;

			if (value.StartsWith("#")) {
				Matcher m = sharpColor1.matcher(value);

				if (m.matches()) {
					if (value.Length == 7) {
						try {
							c = Color.decode(value);

							return c;
						} catch (FormatException e) {
							c = null;
						}
					} else if (value.Length == 9) {
						int r = int.Parse(m.group(1), 16);
						int g = int.Parse(m.group(2), 16);
						int b = int.Parse(m.group(3), 16);
						int a = int.Parse(m.group(4), 16);

						return new Color(r, g, b, a);
					}
				}

				m = sharpColor2.matcher(value);

				if (m.matches()) {
					if (value.Length >= 4) {
						int r = int.Parse(m.group(1), 16) * 16;
						int g = int.Parse(m.group(2), 16) * 16;
						int b = int.Parse(m.group(3), 16) * 16;
						int a = 255;

						if (value.Length == 5)
							a = int.Parse(m.group(4), 16) * 16;

						return new Color(r, g, b, a);
					}
				}
			} else if (value.StartsWith("rgb")) {
				Matcher m = cssColorA.matcher(value);

				if (m.matches()) {
					int r = int.Parse(m.group(1));
					int g = int.Parse(m.group(2));
					int b = int.Parse(m.group(3));
					int a = int.Parse(m.group(4));

					return new Color(r, g, b, a);
				}

				m = cssColor.matcher(value);

				if (m.matches()) {
					int r = int.Parse(m.group(1));
					int g = int.Parse(m.group(2));
					int b = int.Parse(m.group(3));

					return new Color(r, g, b);
				}
			} else if (value.StartsWith("0x") || value.StartsWith("0X")) {
				Matcher m = hexaColor.matcher(value);

				if (m.matches()) {
					if (value.Length == 8) {
						try {
							return Color.decode(value);
						} catch (FormatException e) {
							c = null;
						}
					} else if (value.Length == 10) {
						string r = m.group(1);
						string g = m.group(2);
						string b = m.group(3);
						string a = m.group(4);

						return new Color(int.Parse(r, 16), int.Parse(g, 16), int.Parse(b, 16),
								int.Parse(a, 16));
					}
				}
			} else if (value.StartsWith("java.awt.Color[")) {
				Matcher m = awtColor.matcher(value);

				if (m.matches()) {
					int r = int.Parse(m.group(1));
					int g = int.Parse(m.group(2));
					int b = int.Parse(m.group(3));

					return new Color(r, g, b);
				}
			}

			return colorMap[value.ToLower()];
		}

		// TODO throw an exception instead ??
		return null;
	}

	/// <summary>
/// Check if the given value is an instance of CharSequence (String is) and return it as a string. Else return null. If the input value is null, the return value is null. If the value returned is larger than 128 characters, this method cuts it to 128 characters. TODO: allow to set the max length of these strings.
/// </summary>
/// <param name="value"> The value to convert.</param>
/// <returns>The corresponding string, or null.</returns>
	public static string convertLabel(object value) {
		string label = null;

		if (value != null) {
			if (value is string)
				label = ((string) value).ToString();
			else
				label = value.ToString();

			if (label.Length > 128)
				label = string.Format("{0}...", label.Substring(0, 128));
		}

		return label;
	}

	/// <summary>
/// Try to convert an arbitrary value to a float. If it is a descendant of Number, the float value is returned. If it is a string, a conversion is tried to change it into a number and if successful, this number is returned as a float. Else, the -1 value is returned as no width can be negative to indicate the conversion failed. If the input is null, the return value is -1.
/// </summary>
/// <param name="value"> The input to convert.</param>
/// <returns>The value or -1 if the conversion failed. TODO should be named convertNumber</returns>
	public static float convertWidth(object value) {
		if (value is string) {
			try {
				float val = float.Parse(((string) value).ToString());

				return val;
			} catch (FormatException e) {
				return -1;
			}
		} else if (value is IConvertible) {
			return ((IConvertible) value);
		}

		return -1;
	}

	/// <summary>
/// Convert an object to a value with units. The object can be a number, in which case the value returned contains this number in pixel units. The object can be a string. In this case the strings understood by this method are of the form (spaces, number, spaces, unit, spaces). For example "3px", "45gu", "5.5%", " 25.3 gu ", "4", " 28.1 ".
/// </summary>
/// <param name="value"> A Number or a CharSequence.</param>
/// <returns>A value.</returns>
	public static Value convertValue(object value) {
		if (value is string) {
			string string = (string) value;

			// if (string == null)
			// throw new Exception("null size string ...");

			if (string.Length < 0)
				throw new Exception("empty size string ...");

			Matcher m = numberUnit.matcher(string);

			if (m.matches())
				return new Value(convertUnit(m.group(2)), float.Parse(m.group(1)));

			m = number.matcher(string);

			if (m.matches())
				return new Value(Units.PX, float.Parse(m.group(1)));

			throw new Exception(string.Format("string is not convertible to a value ({0})", string));
		} else if (value is IConvertible) {
			return new Value(Units.PX, ((IConvertible) value));
		}

		if (value == null)
			throw new Exception("cannot convert null value");

		throw new Exception(string.Format("value is of class {0}\n", value.GetType().Name));
	}

	/// <summary>
/// Convert "gu", "px" and "%" to Units.GU, Units.PX, Units.PERCENTS.
/// </summary>
	protected static Units convertUnit(string unit) {
		if (unit.Equals("gu"))
			return Units.GU;
		else if (unit.Equals("px"))
			return Units.PX;
		else if (unit.Equals("%"))
			return Units.PERCENTS;

		return Units.PX;
	}

	/*
	 * Try to convert an arbitrary value to a EdgeStyle. If the value is a
	 * descendant of CharSequence, it is used and parsed to see if it maps to one of
	 * the possible values.
	 * 
	 * @param value The value to convert.
	 * 
	 * @return The converted edge style or null if the value does not identifies an
	 * edge style. public static EdgeStyle convertEdgeStyle( object value ) { if(
	 * value instanceof CharSequence ) { String s = ( (CharSequence) value
	 * ).toString().toLowerCase();
	 * 
	 * if( s.equals( "dots" ) ) { return EdgeStyle.DOTS; } else if( s.equals(
	 * "dashes" ) ) { return EdgeStyle.DASHES; } else { return EdgeStyle.PLAIN; } }
	 * 
	 * return null; }
	 */
}
}
