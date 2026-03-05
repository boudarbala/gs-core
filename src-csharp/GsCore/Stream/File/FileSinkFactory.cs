using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.File
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
/// Try to instantiate the correct writer given a graph filename. <p> This class tries to instantiate a writer given a filename. Actually it purely tries to analyze the extension and propose the writer according to this extension. </p>
/// </summary>
public class FileSinkFactory {
	private static readonly Dictionary<string, Type> ext2sink;

	static FileSinkFactory() {
		ext2sink = new Dictionary<string, Type>();

		ext2sink["dgs"] = typeof(FileSinkDGS);
		ext2sink["dgsz"] = typeof(FileSinkDGS);
		ext2sink["dgml"] = typeof(FileSinkDynamicGML);
		ext2sink["gml"] = typeof(FileSinkGML);
		ext2sink["graphml"] = typeof(FileSinkGraphML);
		ext2sink["dot"] = typeof(FileSinkDOT);
		ext2sink["svg"] = typeof(FileSinkSVG);
		ext2sink["pgf"] = typeof(FileSinkTikZ);
		ext2sink["tikz"] = typeof(FileSinkTikZ);
		ext2sink["tex"] = typeof(FileSinkTikZ);
		ext2sink["gexf"] = typeof(FileSinkGEXF);
		ext2sink["xml"] = typeof(FileSinkGEXF);
		ext2sink["png"] = typeof(FileSinkImages);
		ext2sink["jpg"] = typeof(FileSinkImages);
	}

	/// <summary>
/// Looks at the file name given and its extension and propose a file output for the format that match this extension.
/// </summary>
/// <param name="filename"> The file name where the graph will be written.</param>
/// <returns>A file sink or null.</returns>
	public static IFileSink sinkFor(string filename) {
		if (filename.LastIndexOf('.') > 0) {
			string ext = filename.Substring(filename.LastIndexOf('.') + 1);
			ext = ext.ToLower();

			if (ext2sink.ContainsKey(ext)) {
				Type fsink = ext2sink[ext];

				try {
					return fsink.newInstance();
				} catch (InstantiationException e) {
					Console.Error.WriteLine(e);
				} catch (IllegalAccessException e) {
					Console.Error.WriteLine(e);
				}
			}
		}

		return null;
	}
}
}
