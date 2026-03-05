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
/// A simple counter that allows to count the number of frames per second.
/// </summary>
public class FpsCounter {
	// Attribute

	/// <summary>
/// Time measure.
/// </summary>
	protected double t1, t2;

	/// <summary>
/// The last frame time.
/// </summary>
	protected double time;

	/// <summary>
/// Counter for the average.
/// </summary>
	protected int count = 0;

	/// <summary>
/// The average time.
/// </summary>
	protected double avgTime;

	// Construction

	public FpsCounter() {
	}

	// Access

	/// <summary>
/// The number of frames per second according to the last measured frame (instantaneous measure).
/// </summary>
/// <returns>The estimated frame-per-second measure of the last frame.</returns>
	public double getFramesPerSecond() {
		return (1000000000.0 / time);
	}

	/// <summary>
/// The duration in seconds of the last measured frame.
/// </summary>
/// <returns>The last frame time in seconds.</returns>
	public double getLastFrameTimeInSeconds() {
		return (time / 1000000000.0);
	}

	/// <summary>
/// The number of frames times used to compute the average frame-per-second and frame time. This number augments with the measures until a maximum, where it is reset to 0.
/// </summary>
/// <returns>The number of frames measure.</returns>
	public int getAverageMeasureCount() {
		return count;
	}

	/// <summary>
/// The average frame-per-second measure.
/// </summary>
/// <returns>The average number of frames per second.</returns>
	public double getAverageFramesPerSecond() {
		return (1000000000.0 / (avgTime / count));
	}

	/// <summary>
/// The average frame time.
/// </summary>
/// <returns>The time used by a frame in average.</returns>
	public double getAverageFrameTimeInSeconds() {
		return ((avgTime / count) * 1000000000.0);
	}

	// Command

	public void resetAverages() {
		count = 0;
		avgTime = 0;
	}

	/// <summary>
/// Start a frame measure.
/// </summary>
	public void beginFrame() {
		t1 = (DateTime.UtcNow.Ticks * 100L);
	}

	/// <summary>
/// End a frame measure.
/// </summary>
	public void endFrame() {
		if (count > 1000000) {
			count = 0;
			avgTime = 0;
		}

		t2 = (DateTime.UtcNow.Ticks * 100L);
		time = (t2 - t1);
		avgTime += time;
		count += 1;
	}
}
}
