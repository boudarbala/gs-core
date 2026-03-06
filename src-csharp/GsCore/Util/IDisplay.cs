using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Util
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


public interface IDisplay {
	/// <summary>
/// Try to get the display according to the "org.graphstream.ui" property. <p> It will look for three class candidates: 1. name defined in the property; 2. #1 one with a ".util.Display" suffix; 3. #2 one with a "org.graphstream.ui." prefix. <p> If the property is not set, or if no valid candidate can be found, a;
/// </summary>
/// <returns>the Display object linked to the UI property</returns>
	static IDisplay getDefault(){
		string uiModule = (System.Environment.GetEnvironmentVariable("org.graphstream.ui"));

		if (uiModule == null) {
			throw new MissingDisplayException("No UI package detected! "
					+ "Please use System.Environment.SetEnvironmentVariable(\"org.graphstream.ui\") for the selected package.");
		} else {
			IDisplay display = null;
			string[] candidates = { uiModule, uiModule + ".util.IDisplay",
					"org.graphstream.ui." + uiModule + ".util.IDisplay" };

			foreach (string candidate in candidates) {
				try {
					Type clazz = Type.GetType(candidate);
					object obj = new clazz();

					if (obj is IDisplay) {
						display = (IDisplay) obj;
						break;
					}
				} catch (TypeLoadException e) {
					continue;
				} catch (Exception e) {
					throw new Exception("Failed to create object", e);
				}
			}

			if (display == null) {
				throw new MissingDisplayException("No valid display found. "
						+ "Please check your System.Environment.SetEnvironmentVariable(\"org.graphstream.ui\") statement.");
			} else {
				return display;
			}
		}
	}

	IViewer display(IGraph graph, bool autoLayout);
}

}
