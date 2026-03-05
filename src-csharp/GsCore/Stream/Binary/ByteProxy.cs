using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
/// This class is a proxy that can exchange event binary-encoded (opposed to text-encoder) with another proxy. <p/> It can be either a server that will listen to connections, or a client that will connect to a server. The {@link org.graphstream.stream.binary.ByteFactory} passed to the constructor will define the encoder and decoder of binary data. <p/> Proxy can run on its own thread, just by calling the {@link ByteProxy#start()} method. It can be manually used with the {@link ByteProxy#poll()} method that process available {@link java.nio.channels.SelectionKey}.
/// </summary>
public class ByteProxy : SourceBase, IPipe, Action {
	private static readonly object /* Logger */ LOGGER = null /* Logger */;

	/// <summary>
/// Defines the mode of this proy, server or client.
/// </summary>
	public enum Mode {
		/// <summary>
/// The proxy is a server. It has its own {@link java.nio.channels.ServerSocketChannel} and can listen to entering connections.
/// </summary>
		SERVER,
		/// <summary>
/// The proxy is just a client that connects to another proxy server.
/// </summary>
		CLIENT
	}

	protected static readonly int BUFFER_INITIAL_SIZE = 8192;

	protected ByteFactory byteFactory;
	protected ByteEncoder encoder;
	protected ByteDecoder decoder;

	/// <summary>
/// Flag to tell is the proxy is running or not.
/// </summary>
	protected AtomicBoolean running;

	/// <summary>
/// Proxy mode.
/// </summary>
	public Mode mode;

	/// <summary>
/// The address the proxy is bound to. If in server mode, this is the address where the server is listening to connections. If in client mode, this is the address where the proxy is connected to.
/// </summary>
	public System.Net.IPAddress address;

	/// <summary>
/// The port listened or connected to.
/// </summary>
	public int port;

	/// <summary>
/// The main channel of the proxy. If in server mode, it will be the {@link java.nio.channels.ServerSocketChannel}. Else, just the {@link java.nio.channels.SocketChannel} connected to the server.
/// </summary>
	protected SelectableChannel mainChannel;

	/// <summary>
/// Multiplexor.
/// </summary>
	protected Selector selector;

	/// <summary>
/// The thread processing selection key when the proxy has been started. If the proxy is not started, the field will be null.
/// </summary>
	protected Thread thread;

	/// <summary>
/// List of opened channels that can be written when new events are received by the proxy.
/// </summary>
	protected ICollection<SocketChannel> writableChannels;

	/// <summary>
/// If not null, this will be replayed when a new connection occured.
/// </summary>
	protected IReplayable replayable;

	/// <summary>
/// Create a new ByteProxy, in server mode, which will be bound to a local address and the given port. if troubles occurred while connecting the socket
/// </summary>
/// <param name="factory"> the factory to create encoder and decoder</param>
/// <param name="port"> port to bind the server to</param>
	public ByteProxy(ByteFactory factory, int port): this(factory, Mode.SERVER, System.Net.IPAddress.getLocalHost(), port) {
	}

	/// <summary>
/// Complete constructor of the proxy. if troubles occurred while connecting the socket
/// </summary>
/// <param name="factory"> the factory to create encoder and decoder</param>
/// <param name="mode"> mode of the proxy</param>
/// <param name="address"> address to listen or to connect to</param>
/// <param name="port"> port to listen or to connect to</param>
	public ByteProxy(ByteFactory factory, Mode mode, System.Net.IPAddress address, int port){
		running = new AtomicBoolean(false);
		writableChannels = new List<object>();
		replayable = null;
		thread = null;

		this.mode = mode;
		this.address = address;
		this.port = port;

		byteFactory = factory;
		encoder = factory.createByteEncoder();
		decoder = factory.createByteDecoder();

		encoder.addTransport(new ByteEncoder.Transport() {
			
			public void send(byte[] buffer) {
				doSend(buffer);
			}
		});

		decoder.addSink(null /* TODO */);

		init();
	}

	protected void init(){
		System.Net.IPEndPoint isa = new System.Net.IPEndPoint(address, port);

		selector = Selector.open();

		switch (mode) {
		case SERVER:
			ServerSocketChannel serverChannel = ServerSocketChannel.open();
			serverChannel.configureBlocking(false);
			serverChannel.bind(isa);

			mainChannel = serverChannel;
			mainChannel.register(selector, SelectionKey.OP_ACCEPT);

			break;
		case CLIENT:
			SocketChannel socketChannel = SocketChannel.open();
			socketChannel.connect(isa);
			socketChannel.finishConnect();
			socketChannel.configureBlocking(false);

			mainChannel = socketChannel;
			mainChannel.register(selector, SelectionKey.OP_READ + SelectionKey.OP_WRITE);
			writableChannels.Add(socketChannel);
			break;
		}
	}

	/// <summary>
/// Set the stream that can be replayed on a new connection.
/// </summary>
/// <param name="replayable"> the stream to replay, or null if nothing has to be replayed.</param>
	public void setReplayable(IReplayable replayable) {
		this.replayable = replayable;
	}

	/// <summary>
/// Starts the proxy worker.
/// </summary>
	public void start() {
		if (thread != null) {
			Console.Error.WriteLine("Already started.");
		} else {
			Thread t = new Thread(this);
			t.Start();
		}
	}

	/// <summary>
/// Stops the proxy worker, if running, and wait the end of the worker thread. if an interruption occurred while waiting for the end of the worker thread.
/// </summary>
	public void stop(){
		if (thread != null) {
			Thread t = thread;
			running.set(false);

			t.Join();
		}
	}

	
	public void run() {
		thread = System.Threading.Thread.CurrentThread;
		running.set(true);

		Console.WriteLine(string.Format("[{0}] started on {1} {2}...", mode, address.getHostName(), port));

		while (running[]) {
			poll();
		}

		thread = null;
	}

	protected void processSelectedKeys(){
		HashSet<object> readyKeys = selector.selectedKeys();
		IEnumerator<object> i = readyKeys.GetEnumerator();

		while (i.MoveNext()) {
			SelectionKey key = (SelectionKey) i.next();

			i.Remove();

			if (key.isAcceptable()) {
				//
				// If a new connection occurs, register the new socket
				// in the multiplexer.
				//

				System.Diagnostics.Debug.Assert(mode == Mode.SERVER);

				ServerSocketChannel ssocket = (ServerSocketChannel) key.channel();
				SocketChannel socketChannel = ssocket.accept();

				Console.WriteLine(string.Format("accepting socket {0} {1}", socketChannel.socket().getInetAddress(),
						socketChannel.socket().getPort()));

				socketChannel.finishConnect();
				socketChannel.configureBlocking(false);

				if (decoder != null)
					socketChannel.register(selector, SelectionKey.OP_READ);

				replay(socketChannel);
				writableChannels.Add(socketChannel);
			} else if (key.isReadable()) {
				//
				// If a message arrives, read it.
				//

				readDataChunk(key);
			} else if (key.isWritable() && key.attachment() != null) {
				byte[] buffer = (byte[]) key.attachment();
				WritableByteChannel out = (WritableByteChannel) key.channel();

				try {
					output.Write(buffer);
				} catch (System.IO.IOException e) {
					Console.Error.WriteLine("I/O error while writing to channel.");
					close(output);
				} finally {
					key.cancel();
				}
			}
		}
	}

	/// <summary>
/// Same as calling {@link #poll(boolean)} with blocking flag set to true.
/// </summary>
	public void poll() {
		poll(true);
	}

	/// <summary>
/// Wait until one or several chunks of message are acceptable. This method should be called in a loop. It can be used to block a program until some data is available.
/// </summary>
/// <param name="blocking"> flag true if method has to wait for some keys to be ready. If false, just process the available keys.</param>
	public void poll(bool blocking) {
		try {
			if (blocking) {
				if (selector.select() > 0) {
					processSelectedKeys();
				}
			} else {
				if (selector.selectNow() > 0) {
					processSelectedKeys();
				}
			}
		} catch (System.IO.IOException e) {
			Console.Error.WriteLine(string.Format("I/O error in receiver // {0} thread: aborting {1}", port, e.getMessage()));
			running.set(false);
		} catch (Throwable e) {
			Console.Error.WriteLine(string.Format("Unknown error {0}", e.getMessage()));
			Console.Error.WriteLine(e);
			running.set(false);
		}
	}

	/// <summary>
/// When data is readable on a socket, send it to the appropriate buffer (creating it if needed).
/// </summary>
	protected void readDataChunk(SelectionKey key){
		byte[] buffer = (byte[]) key.attachment();
		SocketChannel socket = (SocketChannel) key.channel();

		if (buffer == null) {
			buffer = new byte[BUFFER_INITIAL_SIZE];
			key.attach(buffer);

			Console.WriteLine(string.Format("creating buffer for new connection from {0} {1}", socket.socket().getInetAddress(),
					socket.socket().getPort()));
		}

		try {
			int r = socket.Read(buffer);

			if (r < 0) {
				//
				// End-of-stream
				//

				Console.WriteLine("end-of-stream reached. Closing the mainChannel.");
				close(socket);
			} else if (r == 0) {
				Console.Error.WriteLine("Strange, no binary read.");
			} else {
				while (decoder.validate(buffer)) {
					buffer.flip();
					decoder.decode(buffer);
					buffer.compact();
				}

				if (!buffer.hasRemaining()) {
					byte[] bigger = byte[].allocate(buffer.Length + BUFFER_INITIAL_SIZE);
					bigger.Add(buffer);
					key.attach(bigger);
				}
			}
		} catch (System.IO.IOException e) {
			Console.Error.WriteLine(string.Format("receiver //{0} {1} cannot read object socket mainChannel (I/O error) {2}",
					address.getHostName(), port, e.getMessage()));

			close(key.channel());
		}
	}

	protected void doSend(byte[] buffer) {
		byte[] sendBuffer = byte[].allocate(buffer.remaining());
		sendBuffer.Add(buffer);
		/* sendBuffer.rewind() */;

		IEnumerator<SocketChannel> channels = writableChannels.GetEnumerator();

		while (channels.MoveNext()) {
			SocketChannel writableChannel = channels.next();

			try {
				try {
					writableChannel.Write(sendBuffer.duplicate());
				} catch (NotYetConnectedException e) {
					writableChannel.register(selector, SelectionKey.OP_WRITE, sendBuffer.duplicate());
				}
			} catch (System.IO.IOException e) {
				Console.Error.WriteLine("I/O error while writing to channel : " + e.getMessage());

				channels.Remove();
				close(writableChannel);
			}
		}
	}

	protected void replay(SocketChannel channel) {
		if (replayable != null) {
			Replayable.Controller controller = replayable.getReplayController();
			ByteEncoder encoder = byteFactory.createByteEncoder();

			encoder.addTransport(new ByteEncoder.Transport() {
				
				public void send(byte[] buffer) {
					try {
						channel.Write(buffer);
					} catch (System.IO.IOException e) {
						Console.Error.WriteLine("Failled to replay : " + e.getMessage());
						controller.removeSink(encoder);
					}
				}
			});

			controller.addSink(encoder);
			controller.replay();
		}
	}

	protected void close(Channel channel) {
		writableChannels.Remove(channel);

		if (channel == mainChannel) {
			Console.Error.WriteLine("Closing main channel.");

			if (running[]) {
				try {
					stop();
				} catch (ThreadInterruptedException e) {
					Console.Error.WriteLine("Failed to properly terminate the worker.");
				}
			}
		}

		try {
			channel.Close();
		} catch (System.IO.IOException e) {
			Console.Error.WriteLine("closing channel: " + e.getMessage());
		}
	}

	
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		encoder.graphAttributeAdded(sourceId, timeId, attribute, value);
	}

	
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		encoder.graphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);
	}

	
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		encoder.graphAttributeRemoved(sourceId, timeId, attribute);
	}

	
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		encoder.nodeAttributeAdded(sourceId, timeId, nodeId, attribute, value);
	}

	
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		encoder.nodeAttributeChanged(sourceId, timeId, nodeId, attribute, oldValue, newValue);
	}

	
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		encoder.nodeAttributeRemoved(sourceId, timeId, nodeId, attribute);
	}

	
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		encoder.edgeAttributeAdded(sourceId, timeId, edgeId, attribute, value);
	}

	
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		encoder.edgeAttributeChanged(sourceId, timeId, edgeId, attribute, oldValue, newValue);
	}

	
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		encoder.edgeAttributeRemoved(sourceId, timeId, edgeId, attribute);
	}

	
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		encoder.nodeAdded(sourceId, timeId, nodeId);
	}

	
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		encoder.nodeRemoved(sourceId, timeId, nodeId);
	}

	
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		encoder.edgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);
	}

	
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		encoder.edgeRemoved(sourceId, timeId, edgeId);
	}

	
	public void graphCleared(string sourceId, long timeId) {
		encoder.graphCleared(sourceId, timeId);
	}

	
	public void stepBegins(string sourceId, long timeId, double step) {
		encoder.stepBegins(sourceId, timeId, step);
	}
}

}
