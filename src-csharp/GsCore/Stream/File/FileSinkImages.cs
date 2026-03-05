using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
/// Output graph in image files. <p> <p> Given a prefix "dir/prefix_" and an output policy, this sink will output graph in an image file which name is prefix + a growing counter. </p> <p> Then images can be processed to produce a movie. For example, with mencoder, the following produce high quality movie : </p> <p> <pre> #!/bin/bash EXT=png BITRATE=2M FPS=15 PREFIX=$1 OUTPUT=$2 ffmpeg -framerate $FPS -i "$PREFIX%06d.$EXT" -b:v $BITRATE -r $FPS -an $OUTPUT </pre>
/// </summary>
abstract class FileSinkImages : IFileSink {
	/// <summary>
/// Create a FileSinkImages object according to the UI module specified in "org.graphstream.ui" property. If no valid module has been set null will be returned.
/// </summary>
/// <returns>an implementation of FileSinkImages using the current UI module</returns>
	public static FileSinkImages createDefault() {
		try {
			IDisplay display = Display.getDefault();

			if (display is FileSinkImagesFactory) {
				return ((FileSinkImagesFactory) display).createFileSinkImages();
			} else {
				Console.Error.WriteLine("Default UI module does not provide a FileSinkImages implementation");
			}
		} catch (MissingDisplayException e) {
			Console.Error.WriteLine("No valid UI module specified in \"org.graphstream.ui\" system property");
		}

		return null;
	}

	/// <summary>
/// Output image type.
/// </summary>
	enum OutputType {
		PNG, JPG, png(






		}
	}

	/// <summary>
/// Output policy. Specify when an image is written. This is an important choice. Best choice is to divide the graph in steps and choose the *ByStepOutput*. Remember that if your graph has x nodes and *ByEventOutput* or *ByNodeEventOutput* is chosen, this will produce x images just for nodes creation.
/// </summary>
	enum OutputPolicy {
		BY_EVENT, BY_ELEMENT_EVENT, BY_ATTRIBUTE_EVENT, BY_NODE_EVENT, BY_EDGE_EVENT, BY_GRAPH_EVENT, BY_STEP, BY_NODE_ADDED_REMOVED, BY_EDGE_ADDED_REMOVED, BY_NODE_ATTRIBUTE, BY_EDGE_ATTRIBUTE, BY_GRAPH_ATTRIBUTE, BY_LAYOUT_STEP, BY_NODE_MOVED, ON_RUNNER, NONE
	}

	/// <summary>
/// Layout policy. Specify how layout is computed. It can be computed in its own thread with a LayoutRunner but if image rendering takes too much time, node positions will be very different between two images. To have a better result, we can choose to compute layout when a new image is rendered. This will smooth the move of nodes in the movie.
/// </summary>
	enum LayoutPolicy {
		NO_LAYOUT, COMPUTED_IN_LAYOUT_RUNNER, COMPUTED_ONCE_AT_NEW_IMAGE, COMPUTED_FULLY_AT_NEW_IMAGE
	}

	/// <summary>
/// Defines the quality of the rendering. It uses "ui.quality" and "ui.antialias" graph attributes. If quality is set to low, both attributes will be disabled. On medium quality, only "ui.quality" is enable, and on high quality, both attributes are enabled.
/// </summary>
	enum Quality {
		LOW, MEDIUM, HIGH
	}

	private static readonly object /* Logger */ LOGGER = object /* Logger */.getLogger);

	protected IResolution resolution;
	protected OutputType outputType;
	protected string filePrefix;
	protected GraphicGraph gg;
	protected ISink sink;
	protected int counter;
	protected OutputPolicy outputPolicy;
	protected List<IFilter> filters;
	protected LayoutPolicy layoutPolicy;
	protected LayoutRunner optLayout;
	protected IProxyPipe layoutPipeIn;
	protected ILayout layout;
	protected float layoutStabilizationLimit = 0.9f;
	protected int layoutStepAfterStabilization = 10;
	protected int layoutStepPerFrame = 4;
	protected int layoutStepWithoutFrame = 0;
	protected long outputRunnerDelay = 10;
	protected bool outputRunnerAlive = false;
	protected OutputRunner outputRunner;
	protected ThreadProxyPipe outputRunnerProxy;
	protected bool clearImageBeforeOutput = false;
	protected bool hasBegun = false;
	protected bool autofit = true;
	protected string styleSheet = null;

	protected FileSinkImages() : this(OutputType.PNG, Resolutions.HD720) {
	}

	protected FileSinkImages(OutputType type, IResolution resolution) : this(type, resolution, OutputPolicy.NONE) {
	}

	protected FileSinkImages(OutputType type, IResolution resolution, OutputPolicy outputPolicy) {
		this.resolution = resolution;
		this.outputType = type;
		this.filePrefix = "frame_";
		this.counter = 0;
		this.gg = new GraphicGraph(string.Format("images-{0}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
		this.filters = new List<IFilter>();
		this.layoutPolicy = LayoutPolicy.NO_LAYOUT;
		this.layout = null;
		this.optLayout = null;
		this.layoutPipeIn = null;
		this.sink = gg;

		setOutputPolicy(outputPolicy);
	}

	/// <summary>
/// Get the camera that controls view position and boundaries.
/// </summary>
/// <returns>a camera object, associated with the {@link org.graphstream.ui.view.GraphRenderer} use for rendering.</returns>
	protected abstract ICamera getCamera();

	/// <summary>
/// Render the graph.
/// </summary>
	protected abstract void render();

	/// <summary>
/// Get the image in which graph has been rendered.
/// </summary>
/// <returns>an image of the graph</returns>
	protected abstract object /* BufferedImage */ getRenderedImage();

	/// <summary>
/// Initialize the image data. This method is called at sink creation and each time there is a change in image specifications (resolution, type).
/// </summary>
	protected abstract void initImage();

	/// <summary>
/// Clear the image. This will fill the image with the specified color.
/// </summary>
/// <param name="color">color to fill the image with</param>
	protected abstract void clearImage(int color);

	/// <summary>
/// Enable high-quality rendering and anti-aliasing.
/// </summary>
	public void setQuality(Quality q) {
		switch (q) {
		case LOW:
			if (gg.hasAttribute("ui.quality"))
				gg.removeAttribute("ui.quality");
			if (gg.hasAttribute("ui.antialias"))
				gg.removeAttribute("ui.antialias");

			break;
		case MEDIUM:
			if (!gg.hasAttribute("ui.quality"))
				gg.setAttribute("ui.quality");
			if (gg.hasAttribute("ui.antialias"))
				gg.removeAttribute("ui.antialias");

			break;
		case HIGH:
			if (!gg.hasAttribute("ui.quality"))
				gg.setAttribute("ui.quality");
			if (!gg.hasAttribute("ui.antialias"))
				gg.setAttribute("ui.antialias");

			break;
		}
	}

	/// <summary>
/// Defines style of the graph as a css stylesheet.
/// </summary>
/// <param name="styleSheet">the style sheet</param>
	public void setStyleSheet(string styleSheet) {
		this.styleSheet = styleSheet;
		gg.setAttribute("ui.stylesheet", styleSheet);
	}

	/// <summary>
/// Set resolution of images.
/// </summary>
/// <param name="r">resolution</param>
	public void setResolution(IResolution r) {
		if (r != resolution) {
			resolution = r;
			initImage();
		}
	}

	/// <summary>
/// Set a custom resolution.
/// </summary>
/// <param name="width"></param>
/// <param name="height"></param>
	public void setResolution(int width, int height) {
		if (resolution == null || resolution.getWidth() != width || resolution.getHeight() != height) {
			setResolution(new CustomResolution(width, height));
		}
	}

	/// <summary>
/// Set the output policy.
/// </summary>
/// <param name="policy">policy defining when images are produced</param>
	public void setOutputPolicy(OutputPolicy policy) {
		this.outputPolicy = policy;
	}

	/// <summary>
/// Set the output type.
/// </summary>
/// <param name="outputType">type of outputted images</param>
	public void setOutputType(OutputType outputType) {
		if (outputType != this.outputType) {
			this.outputType = outputType;
			initImage();
		}
	}

	/// <summary>
/// Set the layout policy.
/// </summary>
/// <param name="policy">policy defining how the layout is computed</param>
	public void setLayoutPolicy(LayoutPolicy policy) {
		if (policy != layoutPolicy) {
			switch (layoutPolicy) {
			case COMPUTED_IN_LAYOUT_RUNNER:
				// layout.removeListener(this);
				optLayout.release();
				optLayout = null;
				layoutPipeIn.removeAttributeSink(gg);
				layoutPipeIn = null;
				layout = null;
				break;
			case COMPUTED_ONCE_AT_NEW_IMAGE:
				// layout.removeListener(this);
				gg.removeSink(layout);
				layout.removeAttributeSink(gg);
				layout = null;
				break;
			default:
				break;
			}

			switch (policy) {
			case COMPUTED_IN_LAYOUT_RUNNER:
				layout = Layouts.newLayoutAlgorithm();
				optLayout = new InnerLayoutRunner();
				break;
			case COMPUTED_FULLY_AT_NEW_IMAGE:
			case COMPUTED_ONCE_AT_NEW_IMAGE:
				layout = Layouts.newLayoutAlgorithm();
				gg.addSink(layout);
				layout.addAttributeSink(gg);
				break;
			default:
				break;
			}

			// layout.addListener(this);
			layoutPolicy = policy;
		}
	}

	/// <summary>
/// Set the amount of step before output a new image. This is used only in ByLayoutStepOutput output policy.
/// </summary>
/// <param name="spf">step per frame</param>
	public void setLayoutStepPerFrame(int spf) {
		this.layoutStepPerFrame = spf;
	}

	/// <summary>
/// Set the amount of steps after the stabilization of the algorithm.
/// </summary>
/// <param name="sas">step after stabilization.</param>
	public void setLayoutStepAfterStabilization(int sas) {
		this.layoutStepAfterStabilization = sas;
	}

	/// <summary>
/// Set the stabilization limit of the layout used to compute coordinates of nodes. See {@link org.graphstream.ui.layout.Layout#setStabilizationLimit(double)} for more informations about this limit.
/// </summary>
/// <param name="limit"></param>
	public void setLayoutStabilizationLimit(double limit) {
		if (layout == null)
			throw new NullReferenceException("did you enable layout ?");

		layout.setStabilizationLimit(limit);
	}

	/// <summary>
/// Add a filter.
/// </summary>
/// <param name="filter">the filter to add</param>
	public void addFilter(IFilter filter) {
		filters.Add(filter);
	}

	/// <summary>
/// Remove a filter.
/// </summary>
/// <param name="filter">the filter to remove</param>
	public void removeFilter(IFilter filter) {
		filters.Remove(filter);
	}

	public void setOutputRunnerEnabled(bool on) {
		if (!on && outputRunnerAlive) {
			outputRunnerAlive = false;

			try {
				if (outputRunner != null)
					outputRunner.join();
			} catch (ThreadInterruptedException e) {
				// ... ?
			}

			outputRunner = null;
			sink = gg;

			if (outputRunnerProxy != null)
				outputRunnerProxy.pump();
		}

		outputRunnerAlive = on;

		if (outputRunnerAlive) {
			if (outputRunnerProxy == null) {
				outputRunnerProxy = new ThreadProxyPipe();
				outputRunnerProxy.init(gg);
			}

			sink = outputRunnerProxy;
			outputRunner = new OutputRunner();
			outputRunner.start();
		}
	}

	public void setOutputRunnerDelay(long delay) {
		outputRunnerDelay = delay;
	}

	public void stabilizeLayout(double limit) {
		if (layout != null) {
			while (layout.getStabilization() < limit)
				layout.compute();
		}
	}

	public Point3 getViewCenter() {
		return getCamera().getViewCenter();
	}

	public void setViewCenter(double x, double y) {
		getCamera().setViewCenter(x, y, 0);
	}

	public double getViewPercent() {
		return getCamera().getViewPercent();
	}

	public void setViewPercent(double zoom) {
		getCamera().setViewPercent(zoom);
	}

	public void setGraphViewport(double minx, double miny, double maxx, double maxy) {
		getCamera().setGraphViewport(minx, miny, maxx, maxy);
	}

	public void setClearImageBeforeOutputEnabled(bool on) {
		clearImageBeforeOutput = on;
	}

	public void setAutofit(bool on) {
		autofit = on;
	}

	protected void clearGG() {
		gg.Clear();

		if (styleSheet != null)
			gg.setAttribute("ui.stylesheet", styleSheet);

		if (layout != null)
			layout.Clear();
	}

	/// <summary>
/// Produce a new image.
/// </summary>
	public void outputNewImage() {
		outputNewImage(string.Format("{0}{1}.{2}", filePrefix, counter++, outputType.ext));
	}

	public void outputNewImage(string filename) {
		switch (layoutPolicy) {
		case COMPUTED_IN_LAYOUT_RUNNER:
			layoutPipeIn.pump();
			break;
		case COMPUTED_ONCE_AT_NEW_IMAGE:
			if (layout != null)
				layout.compute();
			break;
		case COMPUTED_FULLY_AT_NEW_IMAGE:
			stabilizeLayout(layout.getStabilizationLimit());
			break;
		default:
			break;
		}

		if (clearImageBeforeOutput || gg.getNodeCount() == 0) {
			clearImage(0x00000000);
		}

		if (gg.getNodeCount() > 0) {
			if (autofit) {
				gg.computeBounds();

				Point3 lo = gg.getMinPos();
				Point3 hi = gg.getMaxPos();

				getCamera().setBounds(lo.x, lo.y, lo.z, hi.x, hi.y, hi.z);
			}

			render();
		}

		object /* BufferedImage */ image = getRenderedImage();

		foreach (IFilter action in filters)
			action.apply(image);

		image.Flush();

		try {
			writeImage(image, filename);
			printProgress();
		} catch (System.IO.IOException e) {
			Console.Error.WriteLine("Failed to write image" + " " + e);
		}
	}

	protected void writeImage(object /* BufferedImage */ image, string filename){
		File output = new System.IO.FileInfo(filename);

		if (output.getParent() != null && !output.getParentFile().exists())
			output.getParentFile().mkdirs();

		ImageIO.Write(image, outputType.ToString(), output);
	}

	protected void printProgress() {
		Console.WriteLine(string.Format("\033[s\033[K{0} images written\033[u", counter));
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSink#begin(java.io.OutputStream)
	 */
	public void begin(System.IO.Stream stream){
		throw new System.IO.IOException("not implemented");
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSink#begin(java.io.Writer)
	 */
	public void begin(System.IO.TextWriter writer){
		throw new System.IO.IOException("not implemented");
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSink#begin(java.lang.String)
	 */
	public void begin(string prefix){
		initImage();

		this.filePrefix = prefix;
		this.hasBegun = true;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSink#flush()
	 */
	public void flush(){
		// Nothing to do
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSink#end()
	 */
	public void end(){
		flush();
		this.hasBegun = false;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.file.FileSink#writeAll(org.graphstream.graph.Graph ,
	 * java.io.OutputStream)
	 */
	public void writeAll(IGraph g, System.IO.Stream stream){
		throw new System.IO.IOException("not implemented");
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.file.FileSink#writeAll(org.graphstream.graph.Graph ,
	 * java.io.Writer)
	 */
	public void writeAll(IGraph g, System.IO.TextWriter writer){
		throw new System.IO.IOException("not implemented");
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see
	 * org.graphstream.stream.file.FileSink#writeAll(org.graphstream.graph.Graph ,
	 * java.lang.String)
	 */
	public void writeAll(IGraph g, string filename){
		clearGG();

		GraphReplay replay = new GraphReplay(string.Format("file_sink_image-write_all-replay-{0}", (DateTime.UtcNow.Ticks * 100L)));

		replay.addSink(gg);
		replay.replay(g);
		replay.removeSink(gg);

		initImage();
		outputNewImage(filename);

		clearGG();
	}

	
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
		sink.edgeAttributeAdded(sourceId, timeId, edgeId, attribute, value);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_EDGE_EVENT:
		case BY_EDGE_ATTRIBUTE:
		case BY_ATTRIBUTE_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
		sink.edgeAttributeChanged(sourceId, timeId, edgeId, attribute, oldValue, newValue);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_EDGE_EVENT:
		case BY_EDGE_ATTRIBUTE:
		case BY_ATTRIBUTE_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
		sink.edgeAttributeRemoved(sourceId, timeId, edgeId, attribute);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_EDGE_EVENT:
		case BY_EDGE_ATTRIBUTE:
		case BY_ATTRIBUTE_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
		sink.graphAttributeAdded(sourceId, timeId, attribute, value);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_GRAPH_EVENT:
		case BY_GRAPH_ATTRIBUTE:
		case BY_ATTRIBUTE_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
		sink.graphAttributeChanged(sourceId, timeId, attribute, oldValue, newValue);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_GRAPH_EVENT:
		case BY_GRAPH_ATTRIBUTE:
		case BY_ATTRIBUTE_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
		sink.graphAttributeRemoved(sourceId, timeId, attribute);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_GRAPH_EVENT:
		case BY_GRAPH_ATTRIBUTE:
		case BY_ATTRIBUTE_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
		sink.nodeAttributeAdded(sourceId, timeId, nodeId, attribute, value);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_NODE_EVENT:
		case BY_NODE_ATTRIBUTE:
		case BY_ATTRIBUTE_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
		sink.nodeAttributeChanged(sourceId, timeId, nodeId, attribute, oldValue, newValue);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_NODE_EVENT:
		case BY_NODE_ATTRIBUTE:
		case BY_ATTRIBUTE_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
		sink.nodeAttributeRemoved(sourceId, timeId, nodeId, attribute);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_NODE_EVENT:
		case BY_NODE_ATTRIBUTE:
		case BY_ATTRIBUTE_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
		sink.edgeAdded(sourceId, timeId, edgeId, fromNodeId, toNodeId, directed);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_EDGE_EVENT:
		case BY_EDGE_ADDED_REMOVED:
		case BY_ELEMENT_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
		sink.edgeRemoved(sourceId, timeId, edgeId);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_EDGE_EVENT:
		case BY_EDGE_ADDED_REMOVED:
		case BY_ELEMENT_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void graphCleared(string sourceId, long timeId) {
		sink.graphCleared(sourceId, timeId);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_GRAPH_EVENT:
		case BY_NODE_ADDED_REMOVED:
		case BY_EDGE_ADDED_REMOVED:
		case BY_ELEMENT_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
		sink.nodeAdded(sourceId, timeId, nodeId);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_NODE_EVENT:
		case BY_NODE_ADDED_REMOVED:
		case BY_ELEMENT_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
		sink.nodeRemoved(sourceId, timeId, nodeId);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_NODE_EVENT:
		case BY_NODE_ADDED_REMOVED:
		case BY_ELEMENT_EVENT:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	
	public void stepBegins(string sourceId, long timeId, double step) {
		sink.stepBegins(sourceId, timeId, step);

		switch (outputPolicy) {
		case BY_EVENT:
		case BY_STEP:
			if (hasBegun)
				outputNewImage();
			break;
		default:
			break;
		}
	}

	// public void nodeMoved(String id, double x, double y, double z) {
	// switch (outputPolicy) {
	// case BY_NODE_MOVED:
	// if (hasBegun)
	// outputNewImage();
	// break;
	// }
	// }

	// public void nodeInfos(String id, double dx, double dy, double dz) {
	// }

	// public void edgeChanged(String id, double[] points) {
	// }

	// public void nodesMoved(Map<String, double[]> nodes) {
	// switch (outputPolicy) {
	// case BY_NODE_MOVED:
	// if (hasBegun)
	// outputNewImage();
	// break;
	// }
	// }

	// public void edgesChanged(Map<String, double[]> edges) {
	// }

	// public void stepCompletion(double percent) {
	// switch (outputPolicy) {
	// case BY_LAYOUT_STEP:
	// layoutStepWithoutFrame++;
	//
	// if (layoutStepWithoutFrame >= layoutStepPerFrame) {
	// if (hasBegun)
	// outputNewImage();
	// layoutStepWithoutFrame = 0;
	// }
	//
	// break;
	// }
	// }

	enum Option {
		IMAGE_PREFIX, IMAGE_TYPE("image-type",
				't', "image type. one of " + Arrays.toString), true, true, "PNG"), IMAGE_RESOLUTION(
				"image-resolution", 'r',
				"defines images resolution. \"width x height\" or one of " + Arrays.toString),
				true, true, "HD720"), OUTPUT_POLICY("output-policy", 'e',
				"defines when images are outputted. one of " + Arrays.toString), true, true,
				"ByStepOutput"), LOGO, STYLESHEET("stylesheet",
				's', "defines stylesheet of graph. can be a file or a string.", true, true, null), QUALITY("quality",










		}
	}

	protected class InnerLayoutRunner : LayoutRunner {

		public InnerLayoutRunner() : base(this.gg, this.layout, true, true) {

			this.layoutPipeIn = newLayoutPipe();
			this.layoutPipeIn.addAttributeSink(this.gg);
		}

		public void run() {

			int stepAfterStabilization = 0;

			do {
				pumpPipe.pump();
				layout.compute();

				if (layout.getStabilization() > layout.getStabilizationLimit())
					stepAfterStabilization++;
				else
					stepAfterStabilization = 0;

				nap(80);

				if (stepAfterStabilization > layoutStepAfterStabilization)
					loop = false;
			} while (loop);
		}
	}

	protected class OutputRunner : Thread {
		public OutputRunner() {
			setDaemon(true);
		}

		public void run() {
			while (outputRunnerAlive && outputPolicy == OutputPolicy.ON_RUNNER) {
				outputRunnerProxy.pump();
				if (hasBegun)
					outputNewImage();

				try {
					System.Threading.Thread.Sleep((int)outputRunnerDelay);
				} catch (ThreadInterruptedException e) {
					outputRunnerAlive = false;
				}
			}
		}
	}

	public static void usage() {
		Console.WriteLine(string.Format("usage: java {0} [options] fichier.dgs\n", typeof(FileSinkImages).Name));
		Console.WriteLine(string.Format("where options in:\n"));
		foreach (Option option in Option.Values) {
			Console.WriteLine(string.Format("\n --{0}{1} , -{2} {3}\n{4}\n", option.fullopts, option.valuable ? "=..." : "",
					option.shortopts, option.valuable ? "..." : "", option.description));
		}
	}

	public static void main(params string[] args){

		Dictionary<Option, string> options = new Dictionary<Option, string>();
		List<string> others = new List<string>();

		foreach (Option option in Option.Values)
			if (option.defaultValue != null)
				options[option] = option.defaultValue;

		if (args != null && args.Length > 0) {
			Pattern valueGetter = Pattern.compile("^--\\w[\\w-]*\\w?(?:=(?:\"([^\"]*)\"|([^\\s]*)))$");

			for (int i = 0; i < args.Length; i++) {

				if (args[i].matches("^--\\w[\\w-]*\\w?(=(\"[^\"]*\"|[^\\s]*))?$")) {
					bool found = false;
					foreach (Option option in Option.Values) {
						if (args[i].StartsWith("--" + option.fullopts + "=")) {
							Matcher m = valueGetter.matcher(args[i]);

							if (m.matches()) {
								options[option] = m.group(1) == null ? m.group(2) : m.group(1);
							}

							found = true;
							break;
						}
					}

					if (!found) {
						Console.Error.WriteLine(
								string.Format("unknown option {0}\n", args[i].Substring(0, args[i].IndexOf('='))));
						System.Environment.Exit(1);
					}
				} else if (args[i].matches("^-\\w$")) {
					bool found = false;

					foreach (Option option in Option.Values) {
						if (args[i].Equals("-" + option.shortopts)) {
							options[option] = args[++i];
							break;
						}
					}

					if (!found) {
						Console.Error.WriteLine(string.Format("unknown option {0}\n", args[i]));
						System.Environment.Exit(1);
					}
				} else {
					others.addLast(args[i]);
				}
			}
		} else {
			usage();
			System.Environment.Exit(0);
		}

		List<string> errors = new List<string>();

		if (others.Count == 0) {
			errors.Add("dgs file name missing.");
		}

		string imagePrefix;
		OutputType outputType = null;
		OutputPolicy outputPolicy = null;
		IResolution resolution = null;
		Quality quality = null;
		string logo;
		string stylesheet;

		imagePrefix = options[Option.IMAGE_PREFIX];

		try {
			outputType = OutputType.valueOf(options[Option.IMAGE_TYPE]);
		} catch (ArgumentException e) {
			errors.Add("bad image type: " + options[Option.IMAGE_TYPE]);
		}

		try {
			outputPolicy = OutputPolicy.valueOf(options[Option.OUTPUT_POLICY]);
		} catch (ArgumentException e) {
			errors.Add("bad output policy: " + options[Option.OUTPUT_POLICY]);
		}

		try {
			quality = Quality.valueOf(options[Option.QUALITY]);
		} catch (ArgumentException e) {
			errors.Add("bad quality: " + options[Option.QUALITY]);
		}

		logo = options[Option.LOGO];
		stylesheet = options[Option.STYLESHEET];

		try {
			resolution = Resolutions.valueOf(options[Option.IMAGE_RESOLUTION]);
		} catch (ArgumentException e) {
			Pattern p = Pattern.compile("^\\s*(\\d+)\\s*x\\s*(\\d+)\\s*$");
			Matcher m = p.matcher(options[Option.IMAGE_RESOLUTION]);

			if (m.matches()) {
				resolution = new CustomResolution(int.Parse(m.group(1)), int.Parse(m.group(2)));
			} else {
				errors.Add("bad resolution: " + options[Option.IMAGE_RESOLUTION]);
			}
		}

		if (stylesheet != null && stylesheet.Length < 1024) {
			File test = new System.IO.FileInfo(stylesheet);

			if (test.exists()) {
				System.IO.StreamReader input = new System.IO.StreamReader(test);
				char[] buffer = new char[128];
				string content = "";

				while (input.ready()) {
					int c = input.Read(buffer, 0, 128);
					content += new string(buffer, 0, c);
				}

				stylesheet = content;
				input.Close();
			}
		}

		{
			File test = new System.IO.FileInfo(others.peek());
			if (!test.exists())
				errors.Add(string.Format("file \"%s\" does not exist", others.peek()));
		}

		if (errors.Count > 0) {
			Console.WriteLine(string.Format("error:\n"));

			foreach (string error in errors)
				Console.WriteLine(string.Format("- {0}\n", error));

			System.Environment.Exit(1);
		}

		FileSourceDGS dgs = new FileSourceDGS();
		FileSinkImages fsi = FileSinkImages.createDefault();

		fsi.setOutputPolicy(outputPolicy);
		fsi.setResolution(resolution);
		fsi.setOutputType(outputType);

		dgs.addSink(fsi);

		if (logo != null)
			fsi.addFilter(new AddLogoFilter(logo, 0, 0));

		fsi.setQuality(quality);
		if (stylesheet != null)
			fsi.setStyleSheet(stylesheet);

		bool next = true;

		dgs.begin(others[0]);

		while (next)
			next = dgs.nextStep();

		dgs.end();
	}
}

}
