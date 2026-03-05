using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File.Images.Filters
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
/// This filter adds a logo-picture on each outputted image.
/// </summary>
public class AddLogoFilter : IFilter {
	/// <summary>
/// The logo.
/// </summary>
	private object /* BufferedImage */ logo;
	/// <summary>
/// Logo position on images.
/// </summary>
	private int x, y;

	/// <summary>
/// Create a new filter to add a logo on top of images.
/// </summary>
/// <param name="logoFile">path to the logo picture-file</param>
/// <param name="x">x position of the logo (top-left corner is (0;0))</param>
/// <param name="y">y position of the logo</param>
	public AddLogoFilter(string logoFile, int x, int y){
		File f = new System.IO.FileInfo(logoFile);

		if (f.exists())
			this.logo = ImageIO.Read(f);
		else
			this.logo = ImageIO.Read(ClassLoader.getSystemResource(logoFile));

		this.x = x;
		this.y = y;
	}

	public void apply(object /* BufferedImage */ image) {
		Graphics2D g = image.createGraphics();
		g.drawImage(logo, x, y, null);
	}
}

}
