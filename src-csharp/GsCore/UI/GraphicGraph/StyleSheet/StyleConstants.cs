using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Org.GraphStream.UI.GraphicGraph.StyleSheet
{
    /// <summary>
    /// The various constants and static constant conversion methods used for styling.
    /// </summary>
    public class StyleConstants
    {
        // Constants

        /// <summary>
        /// The available units for numerical values.
        /// </summary>
        public enum Units
        {
            PX, GU, PERCENTS
        }

        /// <summary>
        /// How to fill the contents of the element.
        /// </summary>
        public enum FillMode
        {
            NONE, PLAIN, DYN_PLAIN, GRADIENT_RADIAL, GRADIENT_HORIZONTAL, GRADIENT_VERTICAL, GRADIENT_DIAGONAL1, GRADIENT_DIAGONAL2, IMAGE_TILED, IMAGE_SCALED, IMAGE_SCALED_RATIO_MAX, IMAGE_SCALED_RATIO_MIN
        }

        /// <summary>
        /// How to draw an element.
        /// </summary>
        public enum ShapeKind
        {
            ELLIPSOID, RECTANGULAR, LINEAR, CURVE
        }

        /// <summary>
        /// Possible shapes for elements.
        /// </summary>
        public enum Shape
        {
            CIRCLE, BOX, ROUNDED_BOX, DIAMOND, POLYGON, TRIANGLE, CROSS, FREEPLANE, TEXT_BOX,
            TEXT_ROUNDED_BOX, TEXT_PARAGRAPH, TEXT_CIRCLE, TEXT_DIAMOND, JCOMPONENT,
            PIE_CHART, FLOW, ARROW, IMAGES,
            LINE, ANGLE, CUBIC_CURVE, POLYLINE, POLYLINE_SCALED, SQUARELINE, LSQUARELINE, HSQUARELINE
        }

        /// <summary>
        /// How to draw the shadow of the element.
        /// </summary>
        public enum ShadowMode
        {
            NONE, PLAIN, GRADIENT_RADIAL, GRADIENT_HORIZONTAL, GRADIENT_VERTICAL, GRADIENT_DIAGONAL1, GRADIENT_DIAGONAL2
        }

        /// <summary>
        /// How to show an element.
        /// </summary>
        public enum VisibilityMode
        {
            NORMAL, HIDDEN, AT_ZOOM, UNDER_ZOOM, OVER_ZOOM, ZOOM_RANGE, ZOOMS
        }

        /// <summary>
        /// How to draw the text of an element.
        /// </summary>
        public enum TextMode
        {
            NORMAL, TRUNCATED, HIDDEN
        }

        /// <summary>
        /// How to show the text of an element.
        /// </summary>
        public enum TextVisibilityMode
        {
            NORMAL, HIDDEN, AT_ZOOM, UNDER_ZOOM, OVER_ZOOM, ZOOM_RANGE, ZOOMS
        }

        /// <summary>
        /// Variant of the font.
        /// </summary>
        public enum TextStyle
        {
            NORMAL, ITALIC, BOLD, BOLD_ITALIC
        }

        /// <summary>
        /// Font weight.
        /// </summary>
        public enum TextWeight
        {
            NORMAL, BOLD
        }

        /// <summary>
        /// Text alignment.
        /// </summary>
        public enum TextAlignment
        {
            LEFT, CENTER, RIGHT, JUSTIFY
        }

        /// <summary>
        /// Text position.
        /// </summary>
        public enum TextPosition
        {
            CENTER, LEFT, RIGHT, AT_LEFT, AT_RIGHT
        }

        /// <summary>
        /// How to draw the stroke of an element.
        /// </summary>
        public enum StrokeMode
        {
            NONE, PLAIN, DASHED, DOTTED, DASHED_DOTTED
        }

        /// <summary>
        /// How to draw the stroke of an element.
        /// </summary>
        public enum StrokeLineCap
        {
            BUTT, ROUND, SQUARE
        }

        /// <summary>
        /// Stroke line join.
        /// </summary>
        public enum StrokeLineJoin
        {
            MITER, ROUND, BEVEL
        }

        /// <summary>
        /// Arrow mode.
        /// </summary>
        public enum ArrowMode
        {
            NONE, FORWARD, BACKWARD, BOTH
        }

        /// <summary>
        /// Arrow shape.
        /// </summary>
        public enum ArrowShape
        {
            NONE, TRIANGLE, CIRCLE, DIAMOND, WHITE_TRIANGLE, PARALLELOGRAM, ARROW
        }

        /// <summary>
        /// How to compute the node size.
        /// </summary>
        public enum SizeMode
        {
            COMPOSED, DYNAMIC, PIXELS
        }

        /// <summary>
        /// How to evaluate the value of an attribute for a sprite.
        /// </summary>
        public enum SpriteMode
        {
            STATIC, DYNAMIC
        }

        // Regex patterns
        protected static Regex numberUnit;
        protected static Regex number;

        static StyleConstants()
        {
            number = new Regex(@"\\s*(\\p{Digit}+([.]\\p{Digit})?)\\s*");
            numberUnit = new Regex(@"\\s*(\\p{Digit}+(?:[.]\\p{Digit}+)?)\\s*(gu|px|%)");
        }

        /// <summary>
        /// Try to convert the given string value to a colour.
        /// </summary>
        public static Color convertColor(object anyValue)
        {
            if (anyValue == null)
                return null;

            if (anyValue is Color)
                return (Color)anyValue;

            if (anyValue is string)
            {
                string value = (string)anyValue;

                if (value.StartsWith("#"))
                {
                    try
                    {
                        return ColorTranslator.FromHtml(value);
                    }
                    catch { }
                }
            }

            return null;
        }
    }
}
