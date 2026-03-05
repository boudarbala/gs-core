using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.View.Camera
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


public interface ICamera {
	/// <summary>
/// The view centre (a point in graph units).
/// </summary>
/// <returns>The view centre.</returns>
	Point3 getViewCenter();

	/// <summary>
/// Change the view centre.
/// </summary>
/// <param name="x"> The new abscissa.</param>
/// <param name="y"> The new ordinate.</param>
/// <param name="z"> The new depth.</param>
	void setViewCenter(double x, double y, double z);

	/// <summary>
/// The portion of the graph visible.
/// </summary>
/// <returns>A real for which value 1 means the graph is fully visible and uses the whole view port.</returns>
	double getViewPercent();

	/// <summary>
/// Zoom the view.
/// </summary>
/// <param name="percent"> Percent of the graph visible.</param>
	void setViewPercent(double percent);

	/// <summary>
/// The current rotation angle.
/// </summary>
/// <returns>The rotation angle in degrees.</returns>
	double getViewRotation();

	/// <summary>
/// Rotate the view around its centre point by a given theta angles (in degrees).
/// </summary>
/// <param name="theta"> The rotation angle in degrees.</param>
	void setViewRotation(double theta);

	/// <summary>
/// A number in GU that gives the approximate graph size (often the diagonal of the graph). This allows to compute displacements in the graph as percent of its overall size. For example this can be used to move the view centre.
/// </summary>
/// <returns>The graph estimated size in graph units.</returns>
	double getGraphDimension();

	/// <summary>
/// Remove the specified graph view port.
/// </summary>
	void removeGraphViewport();

	/// <summary>
/// Specify exactly the minimum and maximum points in GU that are visible (more points may be visible due to aspect-ratio constraints).
/// </summary>
/// <param name="minx"> The minimum abscissa visible.</param>
/// <param name="miny"> The minimum ordinate visible.</param>
/// <param name="maxx"> The maximum abscissa visible.</param>
/// <param name="maxy"> The maximum abscissa visible.</param>
	void setGraphViewport(double minx, double miny, double maxx, double maxy);

	/// <summary>
/// Reset the view to the automatic mode.
/// </summary>
	void resetView();

	/// <summary>
/// Set the bounds of the graphic graph in GU. Called by the Viewer.
/// </summary>
/// <param name="minx"> Lowest abscissa.</param>
/// <param name="miny"> Lowest ordinate.</param>
/// <param name="minz"> Lowest depth.</param>
/// <param name="maxx"> Highest abscissa.</param>
/// <param name="maxy"> Highest ordinate.</param>
/// <param name="maxz"> Highest depth.</param>
	void setBounds(double minx, double miny, double minz, double maxx, double maxy, double maxz);

	/// <summary>
/// Get the {@link org.graphstream.ui.swingViewer.util.GraphMetrics} object linked to this Camera. It can be used to convert pixels to graphic units and vice versa.
/// </summary>
/// <returns>a GraphMetrics instance</returns>
	GraphMetrics getMetrics();

	/// <summary>
/// Enable or disable automatic adjustment of the view to see the entire graph.
/// </summary>
/// <param name="on"> If true, automatic adjustment is enabled.</param>
	void setAutoFitView(bool on);

	/// <summary>
/// Transform a point in graph units into pixels.
/// </summary>
/// <returns>The transformed point.</returns>
	Point3 transformGuToPx(double x, double y, double z);

	/// <summary>
/// Return the given point in pixels converted in graph units (GU) using the inverse transformation of the current projection matrix. The inverse matrix is computed only once each time a new projection matrix is created.
/// </summary>
/// <param name="x"> The source point abscissa in pixels.</param>
/// <param name="y"> The source point ordinate in pixels.</param>
/// <returns>The resulting points in graph units.</returns>
	Point3 transformPxToGu(double x, double y);

	/// <summary>
/// True if the element would be visible on screen. The method used is to transform the center of the element (which is always in graph units) using the camera actual transformation to put it in pixel units. Then to look in the style sheet the size of the element and to test if its enclosing rectangle intersects the view port. For edges, its two nodes are used.
/// </summary>
/// <param name="element"> The element to test.</param>
/// <returns>True if the element is visible and therefore must be rendered.</returns>
	bool isVisible(GraphicElement element);

	GraphicElement findGraphicElementAt(GraphicGraph graph, HashSet<InteractiveElement> types, double x, double y);

	ICollection<GraphicElement> allGraphicElementsIn(GraphicGraph graph, HashSet<InteractiveElement> types, double x1,
			double y1, double x2, double y2);
}
}
