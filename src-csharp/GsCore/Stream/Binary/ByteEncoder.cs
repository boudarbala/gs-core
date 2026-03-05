using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.Binary
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
/// Define an encoder that transform received events into a binary buffer. <p/> A ByteEncoder is a sink that will produce a {@link System.IO.MemoryStream} from each received evt. Then these buffer can be sent to an end-point using a {@link ByteEncoder.Transport}. <p/> This is a generic way to define the encoding of events into bytes buffer. The main protocol used in GraphStream to do such things is NetStream, with is dedicated encoder {@link org.graphstream.stream.netstream.NetStreamEncoder}.
/// </summary>
public interface ByteEncoder : ISink {
	/// <summary>
/// Add a new transport to this encoder.
/// </summary>
/// <param name="transport"> the new transport</param>
	void addTransport(Transport transport);

	/// <summary>
/// Remove an existing transport from this encoder.
/// </summary>
/// <param name="transport"> the transport to remove</param>
	void removeTransport(Transport transport);

	/// <summary>
/// Define the object that will be called after an event has been transformed into a binary buffer.
/// </summary>
	interface Transport {
		/// <summary>
/// Called by the encoder once an event has been encoded. The buffer's position and limit should be correctly set so the Transport just has to read it.
/// </summary>
/// <param name="buffer"> buffer that has to be transported</param>
		void send(byte[] buffer);
	}
}

}
