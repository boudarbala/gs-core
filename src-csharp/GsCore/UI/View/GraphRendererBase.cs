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


abstract class GraphRendererBase<S, G> : IGraphRenderer<S, G>, StyleGroupListener {
	// Attribute

	/// <summary>
/// The graph to draw.
/// </summary>
	protected GraphicGraph graph;

	/// <summary>
/// Current selection or null.
/// </summary>
	protected Selection selection = null;

	/// <summary>
/// The surface we are rendering on (used only
/// </summary>
	protected S renderingSurface;

	// Initialisation

	public void open(GraphicGraph graph, S renderingSurface) {
		if (this.graph != null)
			throw new Exception("renderer already open, cannot open twice");

		this.graph = graph;
		this.renderingSurface = renderingSurface;

		this.graph.getStyleGroups().addListener(this);
	}

	public void close() {
		if (graph != null) {
			graph.getStyleGroups().removeListener(this);
			graph = null;
		}
	}

	// Access

	public S getRenderingSurface() {
		return renderingSurface;
	}

	// Selection

	public void beginSelectionAt(double x1, double y1) {
		if (selection == null)
			selection = new Selection();

		selection.x1 = x1;
		selection.y1 = y1;
		selection.x2 = x1;
		selection.y2 = y1;
	}

	public void selectionGrowsAt(double x, double y) {
		selection.x2 = x;
		selection.y2 = y;
	}

	public void endSelectionAt(double x2, double y2) {
		selection = null;
	}
}
}
