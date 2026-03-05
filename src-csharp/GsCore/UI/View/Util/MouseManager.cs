using System.Collections.Generic;
using System.Linq;
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
/// A global behavior for all mouse events on graphic elements.
/// </summary>
public interface MouseManager {
	/// <summary>
/// Make the manager active on the given graph and view.
/// </summary>
/// <param name="graph"> The graph to control.</param>
/// <param name="view"> The view to control.</param>
	void init(GraphicGraph graph, IView view);

	/// <summary>
/// Release the links between this manager and the view and the graph.
/// </summary>
	void release();

	/// <summary>
/// Returns the set of InteractiveElements managed by the MouseManager
/// </summary>
/// <returns>the set of InteractiveElements managed by the MouseManager</returns>
	public HashSet<InteractiveElement> getManagedTypes();
}
}
