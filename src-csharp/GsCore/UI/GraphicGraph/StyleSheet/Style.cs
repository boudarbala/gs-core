using System.Collections.Generic;
using System.Linq;
using System.Text;
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
/// A style is a whole set of settings for a graphic element. <p> Styles inherit each others. By default a style is all set to invalid values meaning "unset". This means that the value is to be taken from the parent. The getters are able to resolve this process by themselves and therefore must be used instead of a direct access to fields. </p>
/// </summary>
public class Style : StyleConstants {
	// Attributes

	/// <summary>
/// The vertical part of the cascade.
/// </summary>
	protected Rule parent = null;

	/// <summary>
/// The values of each style property.
/// </summary>
	protected Dictionary<string, object> values = null;

	/// <summary>
/// The set of special styles that must override this style when some event occurs.
/// </summary>
	protected Dictionary<string, Rule> alternates = null;

	// Constructors

	/// <summary>
/// New style with all settings to a special value meaning "unset". In this modeField, all the settings are inherited from the parent (when set).
/// </summary>
	public Style() : this(null) {
	}

	/// <summary>
/// New style with all settings to a special value meaning "unset". In this modeField, all the settings are inherited from the parent.
/// </summary>
/// <param name="parent"> The parent style.</param>
	public Style(Rule parent) {
		this.parent = parent;
		this.values = new Dictionary<string, object>();
	}

	// Access

	/// <summary>
/// The parent style.
/// </summary>
/// <returns>a style from which some settings are inherited.</returns>
	public Rule getParent() {
		return parent;
	}

	/// <summary>
/// Get the value of a given property. This code is the same for all "getX" methods so we explain it once here. This is the implementation of style inheritance. First if some event is actually occurring, the alternative styles are searched first. If these events have unset values for the property, their parent are then searched. If the value for the property is not found in the alternative styles, alternative styles parents, or if there is no event occurring actually, this style is checked. If its value is unset, the parents of this style are checked. Classes are not checked here, they are processed in the {@link org.graphstream.ui.graphicGraph.StyleGroup} class.
/// </summary>
/// <param name="property"> The style property the value is searched for.</param>
	public object getValue(string property, params string[] events) {
		if (events != null && events.Length > 0)// && alternates != null )
		{
			object o = null;
			int i = events.Length - 1;

			do {
				o = getValueForEvent(property, events[i]);
				i--;
			} while (o == null && i >= 0);

			if (o != null)
				return o;
		}

		object value = values[property];

		if (value == null) {
			if (parent != null)
				return parent.style.getValue(property, events);
		}

		return value;
	}

	protected object getValueForEvent(string property, string evt) {
		if (alternates != null) {
			Rule rule = alternates[evt];

			if (rule != null) {
				object o = rule.getStyle().values[property];

				if (o != null)
					return o;
			}
		} else if (parent != null) {
			return parent.style.getValueForEvent(property, evt);
		}

		return null;
	}

	/// <summary>
/// True if the given field exists in this style only (not the parents).
/// </summary>
/// <param name="field"> The field to test.</param>
/// <returns>True if this style has a value for the given field.</returns>
	public bool hasValue(string field, params string[] events) {
		bool hasValue = false;

		if (events != null && events.Length > 0 && alternates != null) {
			foreach (string event in events) {
				Rule rule = alternates[evt];

				if (rule != null) {
					if (rule.getStyle().hasValue(field)) {
						hasValue = true;
						break;
					}
				}
			}
		}

		if (!hasValue) {
			hasValue = (values[field] != null);
		}

		return hasValue;
	}

	// Individual style properties.

	/// <summary>
/// How to fill the content of an element.
/// </summary>
	public FillMode getFillMode() {
		return (FillMode) getValue("fill-mode");
	}

	/// <summary>
/// Which color(s) to use for fill modes that use it.
/// </summary>
	public Colors getFillColors() {
		return (Colors) getValue("fill-color");
	}

	public int getFillColorCount() {
		Colors colors = (Colors) getValue("fill-color");

		if (colors != null)
			return colors.Count;

		return 0;
	}

	public Color getFillColor(int i) {
		Colors colors = (Colors) getValue("fill-color");

		if (colors != null)
			return colors[i];

		return null;
	}

	/// <summary>
/// Which image to use when filling the element contents with it.
/// </summary>
	public string getFillImage() {
		return (string) getValue("fill-image");
	}

	/// <summary>
/// How to draw the element contour.
/// </summary>
	public StrokeMode getStrokeMode() {
		return (StrokeMode) getValue("stroke-mode");
	}

	/// <summary>
/// How to color the element contour.
/// </summary>
	public Colors getStrokeColor() {
		return (Colors) getValue("stroke-color");
	}

	public int getStrokeColorCount() {
		Colors colors = (Colors) getValue("stroke-color");

		if (colors != null)
			return colors.Count;

		return 0;
	}

	public Color getStrokeColor(int i) {
		Colors colors = (Colors) getValue("stroke-color");

		if (colors != null)
			return colors[i];

		return null;
	}

	/// <summary>
/// Width of the element contour.
/// </summary>
	public Value getStrokeWidth() {
		return (Value) getValue("stroke-width");
	}

	/// <summary>
/// How to draw the shadow of the element.
/// </summary>
	public ShadowMode getShadowMode() {
		return (ShadowMode) getValue("shadow-mode");
	}

	/// <summary>
/// Color(s) of the element shadow.
/// </summary>
	public Colors getShadowColors() {
		return (Colors) getValue("shadow-color");
	}

	public int getShadowColorCount() {
		Colors colors = (Colors) getValue("shadow-color");

		if (colors != null)
			return colors.Count;

		return 0;
	}

	public Color getShadowColor(int i) {
		Colors colors = (Colors) getValue("shadow-color");

		if (colors != null)
			return colors[i];

		return null;
	}

	/// <summary>
/// Width of the element shadow.
/// </summary>
	public Value getShadowWidth() {
		return (Value) getValue("shadow-width");
	}

	/// <summary>
/// Offset of the element shadow centre according to the element centre.
/// </summary>
	public Values getShadowOffset() {
		return (Values) getValue("shadow-offset");
	}

	/// <summary>
/// Additional space to add inside the element between its contour and its contents.
/// </summary>
	public Values getPadding() {
		return (Values) getValue("padding");
	}

	/// <summary>
/// How to draw the text of the element.
/// </summary>
	public TextMode getTextMode() {
		return (TextMode) getValue("text-mode");
	}

	/// <summary>
/// How and when to show the text of the element.
/// </summary>
	public TextVisibilityMode getTextVisibilityMode() {
		return (TextVisibilityMode) getValue("text-visibility-mode");
	}

	/// <summary>
/// Visibility values if the text visibility changes.
/// </summary>
	public Values getTextVisibility() {
		return (Values) getValue("text-visibility");
	}

	/// <summary>
/// The text color(s).
/// </summary>
	public Colors getTextColor() {
		return (Colors) getValue("text-color");
	}

	public int getTextColorCount() {
		Colors colors = (Colors) getValue("text-color");

		if (colors != null)
			return colors.Count;

		return 0;
	}

	public Color getTextColor(int i) {
		Colors colors = (Colors) getValue("text-color");

		if (colors != null)
			return colors[i];

		return null;
	}

	/// <summary>
/// The text font style variation.
/// </summary>
	public TextStyle getTextStyle() {
		return (TextStyle) getValue("text-style");
	}

	/// <summary>
/// The text font.
/// </summary>
	public string getTextFont() {
		return (string) getValue("text-font");
	}

	/// <summary>
/// The text size in points.
/// </summary>
	public Value getTextSize() {
		return (Value) getValue("text-size");
	}

	/// <summary>
/// How to draw the icon around the text (or instead of the text).
/// </summary>
	public IconMode getIconMode() {
		return (IconMode) getValue("icon-mode");
	}

	/// <summary>
/// The icon image to use.
/// </summary>
	public string getIcon() {
		return (string) getValue("icon");
	}

	/// <summary>
/// How and when to show the element.
/// </summary>
	public VisibilityMode getVisibilityMode() {
		return (VisibilityMode) getValue("visibility-mode");
	}

	/// <summary>
/// The element visibility if it is variable.
/// </summary>
	public Values getVisibility() {
		return (Values) getValue("visibility");
	}

	/// <summary>
/// How to size the element.
/// </summary>
	public SizeMode getSizeMode() {
		return (SizeMode) getValue("size-mode");
	}

	/// <summary>
/// The element dimensions.
/// </summary>
	public Values getSize() {
		return (Values) getValue("size");
	}

	/// <summary>
/// The element polygonal shape.
/// </summary>
	public Values getShapePoints() {
		return (Values) getValue("shape-points");
	}

	/// <summary>
/// How to align the text according to the element centre.
/// </summary>
	public TextAlignment getTextAlignment() {
		return (TextAlignment) getValue("text-alignment");
	}

	public TextBackgroundMode getTextBackgroundMode() {
		return (TextBackgroundMode) getValue("text-background-mode");
	}

	public Colors getTextBackgroundColor() {
		return (Colors) getValue("text-background-color");
	}

	public Color getTextBackgroundColor(int i) {
		Colors colors = (Colors) getValue("text-background-color");

		if (colors != null)
			return colors[i];

		return null;
	}

	/// <summary>
/// Offset of the text from its computed position.
/// </summary>
	public Values getTextOffset() {
		return (Values) getValue("text-offset");
	}

	/// <summary>
/// Padding of the text inside its background, if any.
/// </summary>
	public Values getTextPadding() {
		return (Values) getValue("text-padding");
	}

	/// <summary>
/// The element shape.
/// </summary>
	public Shape getShape() {
		return (Shape) getValue("shape");
	}

	/// <summary>
/// The element JComponent type if available.
/// </summary>
	public JComponents getJComponent() {
		return (JComponents) getValue("jcomponent");
	}

	/// <summary>
/// How to orient a sprite according to its attachement.
/// </summary>
	public SpriteOrientation getSpriteOrientation() {
		return (SpriteOrientation) getValue("sprite-orientation");
	}

	/// <summary>
/// The shape of edges arrows.
/// </summary>
	public ArrowShape getArrowShape() {
		return (ArrowShape) getValue("arrow-shape");
	}

	/// <summary>
/// Image to use for the arrow.
/// </summary>
	public string getArrowImage() {
		return (string) getValue("arrow-image");
	}

	/// <summary>
/// Edge arrow dimensions.
/// </summary>
	public Values getArrowSize() {
		return (Values) getValue("arrow-size");
	}

	/// <summary>
/// Colour of all non-graph, non-edge, non-node, non-sprite things.
/// </summary>
	public Colors getCanvasColor() {
		return (Colors) getValue("canvas-color");
	}

	public int getCanvasColorCount() {
		Colors colors = (Colors) getValue("canvas-color");

		if (colors != null)
			return colors.Count;

		return 0;
	}

	public Color getCanvasColor(int i) {
		Colors colors = (Colors) getValue("canvas-color");

		if (colors != null)
			return colors[i];

		return null;
	}

	public int getZIndex() {
		return (int) getValue("z-index");
	}

	// Commands

	/// <summary>
/// Set the default values for each setting.
/// </summary>
	public void setDefaults() {
		Colors fillColor = new Colors();
		Colors strokeColor = new Colors();
		Colors shadowColor = new Colors();
		Colors textColor = new Colors();
		Colors canvasColor = new Colors();
		Colors textBgColor = new Colors();

		fillColor.Add(Color.BLACK);
		strokeColor.Add(Color.BLACK);
		shadowColor.Add(Color.GRAY);
		textColor.Add(Color.BLACK);
		canvasColor.Add(Color.WHITE);
		textBgColor.Add(Color.WHITE);

		values["z-index"] = new int(0);

		values["fill-mode"] = FillMode.PLAIN;
		values["fill-color"] = fillColor;
		values["fill-image"] = null;

		values["stroke-mode"] = StrokeMode.NONE;
		values["stroke-color"] = strokeColor;
		values["stroke-width"] = new Value(Units.PX, 1);

		values["shadow-mode"] = ShadowMode.NONE;
		values["shadow-color"] = shadowColor;
		values["shadow-width"] = new Value(Units.PX, 3);
		values["shadow-offset"] = new Values(Units.PX, 3, 3);

		values["padding"] = new Values(Units.PX, 0, 0, 0);

		values["text-mode"] = TextMode.NORMAL;
		values["text-visibility-mode"] = TextVisibilityMode.NORMAL;
		values["text-visibility"] = null;
		values["text-color"] = textColor;
		values["text-style"] = TextStyle.NORMAL;
		values["text-font"] = "default";
		values["text-size"] = new Value(Units.PX, 10);
		values["text-alignment"] = TextAlignment.CENTER;
		values["text-background-mode"] = TextBackgroundMode.NONE;
		values["text-background-color"] = textBgColor;
		values["text-offset"] = new Values(Units.PX, 0, 0);
		values["text-padding"] = new Values(Units.PX, 0, 0);

		values["icon-mode"] = IconMode.NONE;
		values["icon"] = null;

		values["visibility-mode"] = VisibilityMode.NORMAL;
		values["visibility"] = null;

		values["size-mode"] = SizeMode.NORMAL;
		values["size"] = new Values(Units.PX, 10, 10, 10);

		values["shape"] = Shape.CIRCLE;
		values["shape-points"] = null;
		values["jcomponent"] = null;

		values["sprite-orientation"] = SpriteOrientation.NONE;

		values["arrow-shape"] = ArrowShape.ARROW;
		values["arrow-size"] = new Values(Units.PX, 8, 4);
		values["arrow-image"] = null;

		values["canvas-color"] = canvasColor;

	}

	/// <summary>
/// Copy all the settings of the other style that are set, excepted the parent. Only the settings that have a value (different from "unset") are copied. The parent field is never copied.
/// </summary>
/// <param name="other"> Another style.</param>
	public void augment(Style other) {
		if (other != this) {
			augmentField("z-index", other);
			augmentField("fill-mode", other);
			augmentField("fill-color", other);
			augmentField("fill-image", other);

			augmentField("stroke-mode", other);
			augmentField("stroke-color", other);
			augmentField("stroke-width", other);

			augmentField("shadow-mode", other);
			augmentField("shadow-color", other);
			augmentField("shadow-width", other);
			augmentField("shadow-offset", other);

			augmentField("padding", other);

			augmentField("text-mode", other);
			augmentField("text-visibility-mode", other);
			augmentField("text-visibility", other);
			augmentField("text-color", other);
			augmentField("text-style", other);
			augmentField("text-font", other);
			augmentField("text-size", other);
			augmentField("text-alignment", other);
			augmentField("text-background-mode", other);
			augmentField("text-background-color", other);
			augmentField("text-offset", other);
			augmentField("text-padding", other);

			augmentField("icon-mode", other);
			augmentField("icon", other);

			augmentField("visibility-mode", other);
			augmentField("visibility", other);

			augmentField("size-mode", other);
			augmentField("size", other);

			augmentField("shape", other);
			augmentField("shape-points", other);
			augmentField("jcomponent", other);

			augmentField("sprite-orientation", other);

			augmentField("arrow-shape", other);
			augmentField("arrow-size", other);
			augmentField("arrow-image", other);

			augmentField("canvas-color", other);
		}
	}

	protected void augmentField(string field, Style other) {
		object value = other.values[field];

		if (value != null) {
			if (value is Value)
				setValue(field, new Value((Value) value));
			else if (value is Values)
				setValue(field, new Values((Values) value));
			else if (value is Colors)
				setValue(field, new Colors((Colors) value));
			else
				setValue(field, value);
		}
	}

	/// <summary>
/// Set or change the parent of the style.
/// </summary>
/// <param name="parent"> The new parent.</param>
	public void reparent(Rule parent) {
		this.parent = parent;
	}

	/// <summary>
/// Add an alternative style for specific events.
/// </summary>
/// <param name="event"> The event that triggers the alternate style.</param>
/// <param name="alternateStyle"> The alternative style.</param>
	public void addAlternateStyle(string evt, Rule alternateStyle) {
		if (alternates == null)
			alternates = new Dictionary<string, Rule>();

		alternates[evt] = alternateStyle;
	}

	// Commands -- Setters

	public void setValue(string field, object value) {
		values[field] = value;
	}

	// Utility

	
	public string toString() {
		return toString(-1);
	}

	public string toString(int level) {
		System.Text.StringBuilder builder = new System.Text.StringBuilder();
		string prefix = "";
		string sprefix = "    ";

		if (level > 0) {
			for (int i = 0; i < level; i++)
				prefix += "    ";
		}

		// builder.append( String.format( "{0}{1}\n", prefix, base.toString() )
		// );

		if (parent != null) {
			Rule p = parent;

			while (!(p == null)) {
				builder.Append(string.Format(" -> {0}", p.selector.ToString()));
				p = p.getStyle().Directory?.FullName;
			}

		}

		builder.Append(string.Format("\n"));

		IEnumerator<string> i = values.Keys.GetEnumerator();

		while (i.MoveNext()) {
			string key = i.next();
			object o = values[key];

			if (o is List<object>) {
				List<object> array = (List<object>) o;

				if (array.Count > 0) {
					builder.Append(string.Format("{0}{1}{2}{3}: ", prefix, sprefix, sprefix, key));

					foreach (object p in array)
						builder.Append(string.Format("{0} ", p.ToString()));

					builder.Append(string.Format("\n"));
				} else {
					builder.Append(string.Format("{0}{1}{2}{3}: <empty>\n", prefix, sprefix, sprefix, key));
				}
			} else {
				builder.Append(string.Format("{0}{1}{2}{3} {4}\n", prefix, sprefix, sprefix, key,
						o != null ? o.ToString() : "<null>"));
			}
		}

		if (alternates != null && alternates.Count > 0) {
			foreach (Rule rule in ((alternates[])Enum.GetValues(typeof(alternates)))) {
				// We use "level-1" to ensure that these styles line up with those above
				builder.Append(rule.toString(level - 1));
			}
		}

		/*
		 * if( level >= 0 ) { if( parent != null ) { String rec = parent.style.toString(
		 * level + 1 );
		 * 
		 * builder.append( rec ); } }
		 */
		string res = builder.ToString();

		if (res.Length == 0)
			return string.Format("{0}{1}<empty>\n", prefix, prefix);

		return res;
	}
}
}
