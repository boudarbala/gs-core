using System.Collections.Generic;
using System.IO;
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
/// Type responsible for parsing files in the DGS format. <p> The DGS file format is especially designed for storing dynamic graph definitions into a file. More information about the DGS file format will be found on the GraphStream web site: <a href="http://graphstream-project.org/">http://graphstream-project.org/</a> </p> The usual file name extension used for this format is ".dgs".
/// </summary>
public class FileSourceDGS : FileSourceParser {
	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSourceParser#getNewParserFactory()
	 */
	public ParserFactory getNewParserFactory() {
		return null /* TODO: implement ParserFactory */;
	}

	
	public bool nextStep(){
		try {
			return ((DGSParser) parser).nextStep();
		} catch (ParseException e) {
			throw new System.IO.IOException(e);
		}
	}

	
	protected System.IO.TextReader createReaderForFile(string filename){
		System.IO.Stream stream = null;

		inputStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

		if (stream.CanSeek)
			stream.Seek(128, SeekOrigin.Begin);

		try {
			inputStream = new GZipStream(stream, CompressionMode.Decompress);
		} catch (System.IO.IOException e1) {
			//
			// This is not a gzip input.
			// But gzip has eat some bytes so we reset the stream
			// or close and open it again.
			//
			if (stream.CanSeek) {
				try {
					stream.Seek(0, System.IO.SeekOrigin.Begin);
				} catch (System.IO.IOException e2) {
					//
					// Dirty but we hope do not get there
					//
					Console.Error.WriteLine(e2);
				}
			} else {
				try {
					stream.Close();
				} catch (System.IO.IOException e2) {
					//
					// Dirty but we hope do not get there
					//
					Console.Error.WriteLine(e2);
				}

				inputStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
			}
		}

		return new System.IO.StreamReader(new System.IO.StreamReader(stream));
	}
}

}
