using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.UI.Layout
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
/// A factory in charge or creating various layout implementations. This class is mainly used to create the default layout for the graph viewer. You can also use layouts directly on your graphs, but in this case you do not need this factory. This class looks at the "gs.ui.layout" system property to create a layout class. You can change this property using <code>System.setProperty("gs.ui.layout", you_layout_class_name)</code>.
/// </summary>
public class Layouts {

	/// <summary>
/// class level logger
/// </summary>
	private static readonly object /* Logger */ logger = null /* Logger */;

	/// <summary>
/// Creates a layout according to the "org.graphstream.ui.layout" system property.
/// </summary>
/// <returns>The new layout or the default GraphStream "Spring-Box" layout if the "gs.ui.layout" system property is either not set or contains a class that cannot be found.</returns>
	public static ILayout newLayoutAlgorithm() {
		string layoutClassName;

		try {
			layoutClassName = (System.Environment.GetEnvironmentVariable("gs.ui.layout");
			if (layoutClassName != null) {
				Console.Error.WriteLine("\"gs.ui.layout\" is deprecated, use \"org.graphstream.ui.layout\" instead.");
			} else {
				layoutClassName = (System.Environment.GetEnvironmentVariable("org.graphstream.ui.layout");
			}
		} catch (Exception e) {
			layoutClassName = null;
		}

		if (layoutClassName != null) {
			try {
				Type c = Type.GetType(layoutClassName);
				object object = c.newInstance();

				if (object is ILayout) {
					return (ILayout) object;
				} else {
					Console.Error.WriteLine(string.Format("class '{0}' is not a 'IGraphRenderer'\n", object));
				}
			} catch (Exception e) {
				Console.Error.WriteLine("Cannot create layout." + " " + e);
			}
		}

		return new org.graphstream.ui.layout.springbox.implementations.SpringBox(false);
	}
}
}
