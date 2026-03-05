using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.View
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
/// Interface for classes that draw a GraphicGraph in a swing component. <p> There are two rendering mechanisms in the Swing ui package : the viewer and the renderers. The viewer is a complete architecture to render a graph in a panel or frame, handling all the details. The renderer architecture is a way to only render the graph in any surface, handled directly by the developer. When using the render you are must handle the graphic graph by yourself, but you have a lot more flexibility. </p> <p> The viewer mechanisms uses graph renderers. </p>
/// </summary>
public interface IGraphRenderer<S, G> {
	// Initialisation

	void open(GraphicGraph graph, S drawingSurface);

	void close();

	// Access

	IView createDefaultView(IViewer viewer, string id);

	/// <summary>
/// Get a camera object to provide control commands on the view.
/// </summary>
/// <returns>a Camera instance</returns>
	ICamera getCamera();

	/// <summary>
/// Search for the first GraphicElement among the specified types (precedence: node, edge, sprite) that contains the point at coordinates (x, y).
/// </summary>
/// <param name="types"> The types to check</param>
/// <param name="x"> The point abscissa.</param>
/// <param name="y"> The point ordinate.</param>
/// <returns>The first GraphicElement among the specified types at the given coordinates or null if nothing found.</returns>
	GraphicElement findGraphicElementAt(HashSet<InteractiveElement> types, double x, double y);

	/// <summary>
/// Search for all the graphic elements of the specified types contained inside the rectangle (x1,y1)-(x2,y2).
/// </summary>
/// <param name="types"> The types to check</param>
/// <param name="x1"> The rectangle lowest point abscissa.</param>
/// <param name="y1"> The rectangle lowest point ordinate.</param>
/// <param name="x2"> The rectangle highest point abscissa.</param>
/// <param name="y2"> The rectangle highest point ordinate.</param>
/// <returns>The set of GraphicElements in the given rectangle.</returns>
	ICollection<GraphicElement> allGraphicElementsIn(HashSet<InteractiveElement> types, double x1, double y1, double x2,
			double y2);

	// Command

	/// <summary>
/// Redisplay or update the graph.
/// </summary>
	void render(G g, int x, int y, int width, int height);

	/// <summary>
/// Called by the mouse manager to specify where a node and sprite selection started.
/// </summary>
/// <param name="x1"> The selection start abscissa.</param>
/// <param name="y1"> The selection start ordinate.</param>
	void beginSelectionAt(double x1, double y1);

	/// <summary>
/// The selection already started grows toward position (x, y).
/// </summary>
/// <param name="x"> The new end selection abscissa.</param>
/// <param name="y"> The new end selection ordinate.</param>
	void selectionGrowsAt(double x, double y);

	/// <summary>
/// Called by the mouse manager to specify where a node and spite selection stopped.
/// </summary>
/// <param name="x2"> The selection stop abscissa.</param>
/// <param name="y2"> The selection stop ordinate.</param>
	void endSelectionAt(double x2, double y2);

	/// <summary>
/// Force an element to move at the given location in pixels.
/// </summary>
/// <param name="element"> The element.</param>
/// <param name="x"> The requested position abscissa in pixels.</param>
/// <param name="y"> The requested position ordinate in pixels.</param>
	void moveElementAtPx(GraphicElement element, double x, double y);

	void screenshot(string filename, int width, int height);

	/// <summary>
/// Set a layer renderer that will be called each time the graph needs to be redrawn before the graph is rendered. Pass "null" to remove the layer renderer.
/// </summary>
/// <param name="renderer"> The renderer (or null to remove it).</param>
	void setBackLayerRenderer(LayerRenderer<G> renderer);

	/// <summary>
/// Set a layer renderer that will be called each time the graph needs to be redrawn after the graph is rendered. Pass "null" to remove the layer renderer.
/// </summary>
/// <param name="renderer"> The renderer (or null to remove it).</param>
	void setForeLayoutRenderer(LayerRenderer<G> renderer);

}
}
