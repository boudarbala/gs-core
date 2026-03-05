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


/// <summary>
/// Count the step of a stream.
/// </summary>
public class StepCounter : SinkAdapter {
	/// <summary>
/// Count step contains in a file.
/// </summary>
/// <param name="path"> path to the file</param>
/// <returns>count of step event in the file</returns>
	public static int countStepInFile(string path){
		StepCounter counter = new StepCounter();
		IFileSource source = FileSourceFactory.sourceFor(path);

		source.addElementSink(counter);
		source.readAll(path);

		return counter.getStepCount();
	}

	protected int step;

	/// <summary>
/// Default constructor. Count is set to zero.
/// </summary>
	public StepCounter() {
		reset();
	}

	/// <summary>
/// Reset the step count to zero.
/// </summary>
	public void reset() {
		step = 0;
	}

	/// <summary>
/// Get the step count.
/// </summary>
/// <returns>the count of step</returns>
	public int getStepCount() {
		return step;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.SinkAdapter#stepBegins(java.lang.String, long,
	 * double)
	 */
	public void stepBegins(string sourceId, long timeId, double time) {
		step++;
	}
}

}
