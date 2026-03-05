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
/// A view on a graphic graph.
/// </summary>
public interface IView {
	/// <summary>
/// Get the unique view id.
/// </summary>
/// <returns>a view id</returns>
	string getIdView();

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
/// Search for all the graphic elements contained inside the rectangle (x1,y1)-(x2,y2).
/// </summary>
/// <param name="types"> The set of types to check</param>
/// <param name="x1"> The rectangle lowest point abscissa.</param>
/// <param name="y1"> The rectangle lowest point ordinate.</param>
/// <param name="x2"> The rectangle highest point abscissa.</param>
/// <param name="y2"> The rectangle highest point ordinate.</param>
/// <returns>The set of sprites, nodes, and edges in the given rectangle.</returns>
	ICollection<GraphicElement> allGraphicElementsIn(HashSet<InteractiveElement> types, double x1, double y1, double x2,
			double y2);

	/// <summary>
/// Redisplay or update the view contents. Called by the Viewer.
/// </summary>
/// <param name="graph"> The graphic graph to represent.</param>
/// <param name="graphChanged"> True if the graph changed since the last call to this method.</param>
	void display(GraphicGraph graph, bool graphChanged);

	/// <summary>
/// Open this view in a frame. The argument allows to put the view in a new frame or to remove it from the frame (if it already exists). Called by the Viewer.
/// </summary>
/// <param name="on"> Add the view in its own frame or remove it if it already was in its own frame.</param>
	void openInAFrame(bool on);

	/// <summary>
/// Close definitively this view. Called by the Viewer.
/// </summary>
/// <param name="graph"> The graphic graph.</param>
	void close(GraphicGraph graph);

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
/// Freeze an element so that the optional layout cannot move it.
/// </summary>
/// <param name="element"> The element.</param>
/// <param name="frozen"> If true the element cannot be moved automatically.</param>
	void freezeElement(GraphicElement element, bool frozen);

	/// <summary>
/// Force an element to move at the given location in pixels.
/// </summary>
/// <param name="element"> The element.</param>
/// <param name="x"> The requested position abscissa in pixels.</param>
/// <param name="y"> The requested position ordinate in pixels.</param>
	void moveElementAtPx(GraphicElement element, double x, double y);

	/// <summary>
/// Change the manager for mouse events on this view. If the value for the new manager is null, a manager is installed. The;
/// </summary>
/// <param name="manager"> The new manager, or null to set the manager.</param>
	void setMouseManager(MouseManager manager);

	/// <summary>
/// Change the manager for key and shortcuts events on this view. If the value for the new manager is null, a manager is installed. The;
/// </summary>
/// <param name="manager"> The new manager, or null to set the manager</param>
	void setShortcutManager(ShortcutManager manager);
	
	/// <summary>
/// This is a shortcut to a call setShortcutManager with a MouseOverMouseManager instance and with (InteractiveElement.EDGE, InteractiveElement.NODE, InteractiveElement.SPRITE).
/// </summary>
	void enableMouseOptions();
	
	/// <summary>
/// Request ui focus.
/// </summary>
/// <returns>optional object used to confirm the request</returns>
	object requireFocus();

	/// <summary>
/// Generic method for add a new Listener.
/// </summary>
/// <param name="T"> Describe the listener</param>
/// <param name="U"> Listener</param>
	void addListener<T, U>(T descriptor, U listener);

	/// <summary>
/// Generic method for remove a Listener.
/// </summary>
/// <param name="T"> Describe the listener</param>
/// <param name="U"> Listener</param>
	void removeListener<T, U>(T descriptor, U listener);
}

}
