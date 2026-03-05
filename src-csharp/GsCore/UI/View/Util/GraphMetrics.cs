using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.UI.View.Util
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
/// p Various geometric informations on the graphic graph. <p> This class extends the GraphMetrics to provide not only metrics on the graphic graph but also on the rendering canvas, and allow to convert from graph metrics to canvas metrics and the reverse. </p> <p> Here we call the canvas "view port" since this class allows to place a view port inside the graph in order to zoom and pan the view. </p>
/// </summary>
public class GraphMetrics {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	// Attribute

	/// <summary>
/// Graph lower position (bottom,left,front).
/// </summary>
	public Point3 lo = new Point3();

	/// <summary>
/// Graph higher position (top,right,back).
/// </summary>
	public Point3 hi = new Point3();

	/// <summary>
/// The lowest visible point.
/// </summary>
	public Point3 loVisible = new Point3();

	/// <summary>
/// The highest visible point.
/// </summary>
	public Point3 hiVisible = new Point3();

	/// <summary>
/// Graph dimension.
/// </summary>
	public Vector3 size = new Vector3();

	/// <summary>
/// The graph diagonal.
/// </summary>
	public double diagonal = 1;

	/// <summary>
/// The view port size.
/// </summary>
	public double viewport[] = new double[4];

	/// <summary>
/// The scaling factor to pass from graph units to pixels.
/// </summary>
	public double ratioPx2Gu;

	/// <summary>
/// The length for one pixel, according to the current transformation.
/// </summary>
	public double px1;

	// Construction

	/// <summary>
/// New canvas metrics with default values.
/// </summary>
	public GraphMetrics() {
		setDefaults();
	}

	/// <summary>
/// Set defaults value in the lo, hi and size fields to (-1) and (1) respectively.
/// </summary>
	protected void setDefaults() {
		lo.set(-1, -1, -1);
		hi.set(1, 1, 1);
		size.set(2, 2, 2);

		diagonal = 1;
		ratioPx2Gu = 1;
		px1 = 1;
	}

	// Access

	/// <summary>
/// The graph diagonal (the overall width).
/// </summary>
/// <returns>The diagonal.</returns>
	public double getDiagonal() {
		return diagonal;
	}

	/// <summary>
/// The graph bounds.
/// </summary>
/// <returns>The size.</returns>
	public Vector3 getSize() {
		return size;
	}

	/// <summary>
/// The graph lowest (bottom,left,front) point.
/// </summary>
/// <returns>The lowest point.</returns>
	public Point3 getLowPoint() {
		return lo;
	}

	/// <summary>
/// The graph highest (top,right,back) point.
/// </summary>
/// <returns>The highest point.</returns>
	public Point3 getHighPoint() {
		return hi;
	}

	public double graphWidthGU() {
		return hi.x - lo.x;
	}

	public double graphHeightGU() {
		return hi.y - lo.y;
	}

	public double graphDepthGU() {
		return hi.z - lo.z;
	}

	// Access -- Convert values

	/// <summary>
/// Convert a value in given units to graph units.
/// </summary>
/// <param name="value"> The value to convert.</param>
/// <param name="units"> The units the value to convert is expressed in.</param>
/// <returns>The value converted to GU.</returns>
	public double lengthToGu(double value, StyleConstants.Units units) {
		switch (units) {
		case PX:
			// return (value - 0.01f) / ratioPx2Gu;
			return value / ratioPx2Gu;
		case PERCENTS:
			return (diagonal * value);
		case GU:
		default:
			return value;
		}
	}

	/// <summary>
/// Convert a value in a given units to graph units.
/// </summary>
/// <param name="value"> The value to convert (it contains its own units).</param>
	public double lengthToGu(Value value) {
		return lengthToGu(value.value, value.units);
	}

	/// <summary>
/// Convert one of the given values in a given units to graph units.
/// </summary>
/// <param name="values"> The values set containing the value to convert (it contains its own units).</param>
/// <param name="index"> Index of the value to convert.</param>
	public double lengthToGu(Values values, int index) {
		return lengthToGu(values[index], values.units);
	}

	/// <summary>
/// Convert a value in a given units to pixels.
/// </summary>
/// <param name="value"> The value to convert.</param>
/// <param name="units"> The units the value to convert is expressed in.</param>
/// <returns>The value converted in pixels.</returns>
	public double lengthToPx(double value, StyleConstants.Units units) {
		switch (units) {
		case GU:
			// return (value - 0.01f) * ratioPx2Gu;
			return value * ratioPx2Gu;
		case PERCENTS:
			return (diagonal * value) * ratioPx2Gu;
		case PX:
		default:
			return value;
		}
	}

	/// <summary>
/// Convert a value in a given units to pixels.
/// </summary>
/// <param name="value"> The value to convert (it contains its own units).</param>
	public double lengthToPx(Value value) {
		return lengthToPx(value.value, value.units);
	}

	/// <summary>
/// Convert one of the given values in a given units pixels.
/// </summary>
/// <param name="values"> The values set containing the value to convert (it contains its own units).</param>
/// <param name="index"> Index of the value to convert.</param>
	public double lengthToPx(Values values, int index) {
		return lengthToPx(values[index], values.units);
	}

	public double positionPixelToGu(int pixels, int index) {
		double l = lengthToGu(pixels, Units.PX);

		switch (index) {
		case 0:
			l -= graphWidthGU() / 2.0;
			l = (hi.x + lo.x) / 2.0 + l;
			break;
		case 1:
			l -= graphHeightGU() / 2.0;
			l = (hi.y + lo.y) / 2.0 + l;
			break;
		default:
			throw new ArgumentException();
		}

		Console.WriteLine(string.Format("{0}pixel[{1}] {2} --> {3}gu", this, index, pixels, l));

		return l;
	}

	
	public string toString() {
		System.Text.System.Text.StringBuilder builder = new System.Text.System.Text.StringBuilder(string.Format("IGraph Metrics :\n"));

		builder.Append(string.Format("        lo         = {0}\n", lo));
		builder.Append(string.Format("        hi         = {0}\n", hi));
		builder.Append(string.Format("        visible lo = {0}\n", loVisible));
		builder.Append(string.Format("        visible hi = {0}\n", hiVisible));
		builder.Append(string.Format("        size       = {0}\n", size));
		builder.Append(string.Format("        diag       = {0}\n", diagonal));
		builder.Append(string.Format("        viewport   = {0}\n", viewport));
		builder.Append(string.Format("        ratio      = {0}px = 1gu\n", ratioPx2Gu));

		return builder.ToString();
	}

	// Command

	/// <summary>
/// Set the output view port size in pixels.
/// </summary>
/// <param name="viewportWidth"> The width in pixels of the view port.</param>
/// <param name="viewportHeight"> The width in pixels of the view port.</param>
	public void setViewport(double viewportX, double viewportY, double viewportWidth, double viewportHeight) {
		viewport[0] = viewportX;
		viewport[1] = viewportY;
		viewport[2] = viewportWidth;
		viewport[3] = viewportHeight;
	}

	/// <summary>
/// The ratio to pass by multiplication from pixels to graph units. This ratio must be larger than zero, else nothing is changed.
/// </summary>
/// <param name="ratio"> The ratio.</param>
	public void setRatioPx2Gu(double ratio) {
		if (ratio > 0) {
			ratioPx2Gu = ratio;
			px1 = 0.95f / ratioPx2Gu;
		} else if (ratio == 0)
			throw new Exception("ratio PX to GU cannot be zero");
		else if (ratio < 0)
			throw new Exception(string.Format("ratio PX to GU cannot be negative ({0})", ratio));
	}

	/// <summary>
/// Set the graphic graph bounds (the lowest and highest points).
/// </summary>
/// <param name="minx"> Lowest abscissa.</param>
/// <param name="miny"> Lowest ordinate.</param>
/// <param name="minz"> Lowest depth.</param>
/// <param name="maxx"> Highest abscissa.</param>
/// <param name="maxy"> Highest ordinate.</param>
/// <param name="maxz"> Highest depth.</param>
	public void setBounds(double minx, double miny, double minz, double maxx, double maxy, double maxz) {
		lo.x = minx;
		lo.y = miny;
		lo.z = minz;
		hi.x = maxx;
		hi.y = maxy;
		hi.z = maxz;

		size.data[0] = hi.x - lo.x;
		size.data[1] = hi.y - lo.y;
		size.data[2] = hi.z - lo.z;
		diagonal = Math.Sqrt(size.data[0] * size.data[0] + size.data[1] * size.data[1] + size.data[2] * size.data[2]);
	}
}
}
