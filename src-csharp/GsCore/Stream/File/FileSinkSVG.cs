using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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


public class FileSinkSVG : IFileSink {

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#begin(java.lang.String)
	 */
	public void begin(string fileName){
		throw new NotSupportedException();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#begin(java.io.OutputStream)
	 */
	public void begin(System.IO.Stream stream){
		throw new NotSupportedException();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#begin(java.io.Writer)
	 */
	public void begin(System.IO.TextWriter writer){
		throw new NotSupportedException();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#end()
	 */
	public void end(){
		throw new NotSupportedException();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSink#flush()
	 */
	public void flush(){
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.FileSink#writeAll(org.graphstream.graph.Graph ,
	 * java.lang.String)
	 */
	public void writeAll(IGraph graph, string fileName){
		System.IO.StreamWriter output = new System.IO.StreamWriter(fileName);
		writeAll(graph, output);
		output.Close();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.FileSink#writeAll(org.graphstream.graph.Graph ,
	 * java.io.OutputStream)
	 */
	public void writeAll(IGraph graph, System.IO.Stream stream){
		System.IO.StreamWriter output = new System.IO.StreamWriter(stream);
		writeAll(graph, output);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.file.FileSink#writeAll(org.graphstream.graph.Graph ,
	 * java.io.Writer)
	 */
	public void writeAll(IGraph g, System.IO.TextWriter w){
		XMLWriter output = new XMLWriter();
		SVGContext ctx = new SVGContext();

		try {
			output.start(w);
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		} catch (Exception e) {
			throw new Exception(e);
		}

		try {
			ctx.init(output, g);
			ctx.writeElements(output, g);
			ctx.end(output);
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		}

		try {
			output.end();
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		}
	}

	private static string d(double d) {
		return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", d);
	}

	private static double getX(INode n) {
		if (n.hasNumber("x"))
			return n.getNumber("x");

		if (n.hasArray("xy")) {
			object[] xy = n.getArray("xy");

			if (xy != null && xy.Length > 0 && xy[0] is IConvertible)
				return ((IConvertible) xy[0]);
		}

		if (n.hasArray("xyz")) {
			object[] xyz = n.getArray("xyz");

			if (xyz != null && xyz.Length > 0 && xyz[0] is IConvertible)
				return ((IConvertible) xyz[0]);
		}

		System.err.printf("[WARNING] no x attribute for node \"{0}\" %s\n", n.getId(), n.hasAttribute("xyz"));

		return new Random().NextDouble();
	}

	private static double getY(INode n) {
		if (n.hasNumber("y"))
			return n.getNumber("y");

		if (n.hasArray("xy")) {
			object[] xy = n.getArray("xy");

			if (xy != null && xy.Length > 1 && xy[1] is IConvertible)
				return ((IConvertible) xy[1]);
		}

		if (n.hasArray("xyz")) {
			object[] xyz = n.getArray("xyz");

			if (xyz != null && xyz.Length > 1 && xyz[1] is IConvertible)
				return ((IConvertible) xyz[1]);
		}

		return new Random().NextDouble();
	}

	private static string getSize(Value v) {
		string u = v.units.ToString().ToLower();
		return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}", v.value, u);
	}

	private static string getSize(Values v, int index) {
		string u = v.units.ToString().ToLower();
		if (Units.PERCENTS.Equals(v.units))
			u = "%";
		return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}", v[index], u);
	}

	class SVGContext {
		StyleGroupSet groups;
		StyleSheet stylesheet;
		Dictionary<StyleGroup, SVGStyle> svgStyles;
		ViewBox viewBox;

		public SVGContext() {
			stylesheet = new StyleSheet();
			groups = new StyleGroupSet(stylesheet);
			svgStyles = new Dictionary<StyleGroup, SVGStyle>();
			viewBox = new ViewBox(0, 0, 1000, 1000);
		}

		public void init(XMLWriter out, IGraph g){
			if (g.hasAttribute("ui.stylesheet")) {
				stylesheet.load(((string) g.getAttribute("ui.stylesheet")));
			}

			groups.addElement(g);
			viewBox.compute(g, groups.getStyleFor(g));

			output.open("svg");
			output.attribute("xmlns", "http://www.w3.org/2000/svg");
			output.attribute("xmlns:dc", "http://purl.org/dc/elements/1.1/");
			output.attribute("xmlns:cc", "http://creativecommons.org/ns#");
			output.attribute("xmlns:rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
			output.attribute("xmlns:svg", "http://www.w3.org/2000/svg");

			output.attribute("viewBox",
					string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1} {2} {3}", viewBox.x1, viewBox.y1, viewBox.x2, viewBox.y2));

			output.attribute("id", g.getId());
			output.attribute("version", "1.1");

			try {
				g.edges().ToList().ForEach(e => {
					groups.addElement(e);

					if (e.hasAttribute("ui.style")) {
						try {
							stylesheet.parseStyleFromString(new Selector(Type.EDGE, e.getId(), null),
									(string) e.getAttribute("ui.style"));
						} catch (System.IO.IOException ex) {
							throw new Exception(ex);
						}
					}

					groups.checkElementStyleGroup(e);
				});

				g.nodes().ToList().ForEach(n => {
					groups.addElement(n);

					if (n.hasAttribute("ui.style")) {
						try {
							stylesheet.parseStyleFromString(new Selector(Type.NODE, n.getId(), null),
									(string) n.getAttribute("ui.style"));
						} catch (System.IO.IOException ex) {
							throw new Exception(ex);
						}
					}

					groups.checkElementStyleGroup(n);
				});
			} catch (Exception e) {
				if (e.getCause() is System.IO.IOException)
					throw (System.IO.IOException) e.getCause();

				if (e.getCause() is Exception)
					throw (System.IO.IOException) e.getCause();
			}

			foreach (StyleGroup group in groups.groups())
				svgStyles[group] = new SVGStyle(group);

			output.open("defs");
			foreach (SVGStyle svgStyle in ((svgStyles[])Enum.GetValues(typeof(svgStyles))))
				svgStyle.writeDef(output);
			output.Close();
		}

		public void end(XMLWriter out){
			output.Close();
		}

		public void writeElements(XMLWriter out, IGraph g){
			output.open("g");
			output.attribute("id", "graph-misc");
			writeElement(output, g);
			output.Close();

			IEnumerator<HashSet<StyleGroup>> it = groups.getZIterator();

			output.open("g");
			output.attribute("id", "elements");

			while (it.MoveNext()) {
				HashSet<StyleGroup> set = it.next();

				foreach (StyleGroup sg in set)
					foreach (IElement e in sg.elements())
						writeElement(output, e);
			}

			output.Close();
		}

		public void writeElement(XMLWriter out, IElement e){
			string id = "";
			SVGStyle style = null;
			string transform = null;
			if (e is IEdge) {
				id = string.Format("egde-{0}", e.getId());
				style = svgStyles[groups.getStyleFor((IEdge) e)];
			} else if (e is INode) {
				id = string.Format("node-{0}", e.getId());
				style = svgStyles[groups.getStyleFor((INode) e)];
				transform = string.Format(System.Globalization.CultureInfo.InvariantCulture, "translate({0},{1})", viewBox.convertX((INode) e),
						viewBox.convertY((INode) e));
			} else if (e is IGraph) {
				id = "graph-background";
				style = svgStyles[groups.getStyleFor((IGraph) e)];
			}

			output.open("g");
			output.attribute("id", id);
			output.open("path");

			if (style != null)
				output.attribute("style", style.getElementStyle(e));

			if (transform != null)
				output.attribute("transform", transform);

			output.attribute("d", getPath(e, style));
			output.Close();

			if (e.hasLabel("label"))
				writeElementText(output, (string) e.getAttribute("label"), e, style.group);

			output.Close();
		}

		public void writeElementText(XMLWriter out, string text, IElement e, StyleGroup style){
			if (style == null || style.getTextVisibilityMode() != StyleConstants.TextVisibilityMode.HIDDEN) {
				double x, y;

				x = 0;
				y = 0;

				if (e is INode) {
					x = viewBox.convertX((INode) e);
					y = viewBox.convertY((INode) e);
				} else if (e is IEdge) {
					INode n0, n1;

					n0 = ((IEdge) e).getNode0();
					n1 = ((IEdge) e).getNode0();

					x = viewBox.convertX((getX(n0) + getX(n1)) / 2);
					y = viewBox.convertY((getY(n0) + getY(n1)) / 2);
				}

				output.open("g");
				output.open("text");
				output.attribute("x", d(x));
				output.attribute("y", d(y));

				if (style != null) {
					if (style.getTextColorCount() > 0)
						output.attribute("fill", toHexColor(style.getTextColor(0)));

					switch (style.getTextAlignment()) {
					case CENTER:
						output.attribute("text-anchor", "middle");
						output.attribute("alignment-baseline", "central");
						break;
					case LEFT:
						output.attribute("text-anchor", "start");
						break;
					case RIGHT:
						output.attribute("text-anchor", "end");
						break;
					default:
						break;
					}

					switch (style.getTextSize().units) {
					case PX:
					case GU:
						output.attribute("font-size", d(style.getTextSize().value));
						break;
					case PERCENTS:
						output.attribute("font-size", d(style.getTextSize().value) + "%");
						break;
					}

					if (style.getTextFont() != null)
						output.attribute("font-family", style.getTextFont());

					switch (style.getTextStyle()) {
					case NORMAL:
						break;
					case ITALIC:
						output.attribute("font-style", "italic");
						break;
					case BOLD:
						output.attribute("font-weight", "bold");
						break;
					case BOLD_ITALIC:
						output.attribute("font-weight", "bold");
						output.attribute("font-style", "italic");
						break;
					}
				}

				output.characters(text);
				output.Close();
				output.Close();
			}
		}

		public string getPath(IElement e, SVGStyle style) {
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();

			if (e is INode) {
				double sx, sy;
				Values size = style.group.getSize();

				sx = getValue(size[0], size.units, true);

				if (size.getValueCount() > 1)
					sy = getValue(size[1], size.units, false);
				else
					sy = getValue(size[0], size.units, false);

				switch (style.group.getShape()) {
				case ROUNDED_BOX:
					double rx, ry;

					rx = Math.Min(5, sx / 2);
					ry = Math.Min(5, sy / 2);

					concat(buffer, " m ", d(-sx / 2 + rx), " ", d(-sy / 2));
					concat(buffer, " h ", d(sx - 2 * rx));
					concat(buffer, " a ", d(rx), ",", d(ry), " 0 0 1 ", d(rx), ",", d(ry));
					concat(buffer, " v ", d(sy - 2 * ry));
					concat(buffer, " a ", d(rx), ",", d(ry), " 0 0 1 -", d(rx), ",", d(ry));
					concat(buffer, " h ", d(-sx + 2 * rx));
					concat(buffer, " a ", d(rx), ",", d(ry), " 0 0 1 -", d(rx), ",-", d(ry));
					concat(buffer, " v ", d(-sy + 2 * ry));
					concat(buffer, " a ", d(rx), ",", d(ry), " 0 0 1 ", d(rx), "-", d(ry));
					concat(buffer, " z");
					break;
				case BOX:
					concat(buffer, " m ", d(-sx / 2), " ", d(-sy / 2));
					concat(buffer, " h ", d(sx));
					concat(buffer, " v ", d(sy));
					concat(buffer, " h ", d(-sx));
					concat(buffer, " z");
					break;
				case DIAMOND:
					concat(buffer, " m ", d(-sx / 2), " 0");
					concat(buffer, " l ", d(sx / 2), " ", d(-sy / 2));
					concat(buffer, " l ", d(sx / 2), " ", d(sy / 2));
					concat(buffer, " l ", d(-sx / 2), " ", d(sy / 2));
					concat(buffer, " z");
					break;
				case TRIANGLE:
					concat(buffer, " m ", d(0), " ", d(-sy / 2));
					concat(buffer, " l ", d(sx / 2), " ", d(sy));
					concat(buffer, " h ", d(-sx));
					concat(buffer, " z");
					break;
				default:
				case CIRCLE:
					concat(buffer, " m ", d(-sx / 2), " 0");
					concat(buffer, " a ", d(sx / 2), ",", d(sy / 2), " 0 1 0 ", d(sx), ",0");
					concat(buffer, " ", d(sx / 2), ",", d(sy / 2), " 0 1 0 -", d(sx), ",0");
					concat(buffer, " z");
					break;
				}
			} else if (e is IGraph) {
				concat(buffer, " M ", d(viewBox.x1), " ", d(viewBox.y1));
				concat(buffer, " L ", d(viewBox.x2), " ", d(viewBox.y1));
				concat(buffer, " L ", d(viewBox.x2), " ", d(viewBox.y2));
				concat(buffer, " L ", d(viewBox.x1), " ", d(viewBox.y2));
				concat(buffer, " Z");
			} else if (e is IEdge) {
				//---------- Size Edge
				double sizeEdge = getValue(style.group.getSize()[0], style.group.getSize().units, true);

				//---------- Size Arrow
				double sx, sy;
				Values sizeArrow = style.group.getArrowSize();

				sx = getValue(sizeArrow[0], sizeArrow.units, true);

				if (sizeArrow.getValueCount() > 1)
					sy = getValue(sizeArrow[1], sizeArrow.units, false);
				else
					sy = getValue(sizeArrow[0], sizeArrow.units, false);

				//-------------- Draw Edge
				IEdge edge = (IEdge) e;
				INode src, trg;

				double x1, y1;
				double x2, y2;

				src = edge.getSourceNode();
				trg = edge.getTargetNode();

				x1 = viewBox.convertX(src);
				y1 = viewBox.convertY(src);
				x2 = viewBox.convertX(trg);
				y2 = viewBox.convertY(trg);
				
				double nodeSize, xCenter, yCenter, xCenterCenter, yCenterCenter ;
				double[] perpen;
				switch(style.group.getShape()) {
					case ANGLE:
						double[] perpendicular = getPerpendicular(x1, y1, x2, y2, sizeEdge);
						double x1Prim = perpendicular[0];
						double y1Prim = perpendicular[1];
						double x2Prim = perpendicular[2];
						double y2Prim = perpendicular[3];
						
						concat(buffer, " M ", d(x1), " ", d(y1));
						concat(buffer, " L ", d(x1Prim), " ", d(y1Prim));
						concat(buffer, " L ", d(x2Prim), " ", d(y2Prim));
						concat(buffer, " Z");

						break;
					case CUBIC_CURVE:
						nodeSize = svgStyles[groups.getStyleFor(trg)].group.getSize()[0];
						if (svgStyles[groups.getStyleFor(trg)].group.getSize().getValueCount() > 1) {
							nodeSize = Math.Max(nodeSize,svgStyles[groups.getStyleFor(trg)].group.getSize()[1]);
						}
						
						// First part of the curve
						xCenter = (x1+x2)/2 ;
						yCenter = (y1+y2)/2 ;
						
						perpen = getPerpendicular(x1, y1, xCenter, yCenter, Math.Sqrt(Math.Pow(Math.Abs(x1-xCenter), 2)+Math.Pow(Math.Abs(y1-yCenter), 2))*2);
						
						double x45degrees = (x1+perpen[2])/2;
						double y45degrees = (y1+perpen[3])/2;
						
						xCenterCenter = (x1+xCenter)/2 ;
						yCenterCenter = (y1+yCenter)/2 ;
						
						double x20degrees = (xCenterCenter+x45degrees)/2;
						double y20degrees = (yCenterCenter+y45degrees)/2;
						
						concat(buffer, " M ", d(x1), " ", d(y1));
						concat(buffer, " C ", d(x20degrees), " ", d(y20degrees), " ", d(x20degrees), " ", d(y20degrees), " ", d(xCenter), " ", d(yCenter));
						
						/// Second part of the curve
						double x45degrees2nd = (x2+perpen[0])/2;
						double y45degrees2nd = (y2+perpen[1])/2;
						
						double xCenterCenter2nd = (x2+xCenter)/2;
						double yCenterCenter2nd = (y2+yCenter)/2;
						
						double x20degrees2nd = (xCenterCenter2nd+x45degrees2nd)/2;
						double y20degrees2nd = (yCenterCenter2nd+y45degrees2nd)/2;
						
						concat(buffer, " S ", d(x20degrees2nd), " ", d(y20degrees2nd), " ", d(x2), " ", d(y2));
						concat(buffer, " C ", d(x20degrees2nd), " ", d(y20degrees2nd), " ", d(x20degrees2nd), " ", d(y20degrees2nd), " ", d(xCenter), " ", d(yCenter));
						concat(buffer, " S ", d(x20degrees), " ", d(y20degrees), " ", d(x1), " ", d(y1));
						concat(buffer, " Z");

						break;
					case BLOB:
						nodeSize = svgStyles[groups.getStyleFor(trg)].group.getSize()[0];
						if (svgStyles[groups.getStyleFor(trg)].group.getSize().getValueCount() > 1) {
							nodeSize = Math.Max(nodeSize,svgStyles[groups.getStyleFor(trg)].group.getSize()[1]);
						}
						
						xCenter = (x1+x2)/2 ;
						yCenter = (y1+y2)/2 ;
						
						xCenterCenter = (x1+xCenter)/2 ;
						yCenterCenter = (y1+yCenter)/2 ;
						
						double[] perpenCenter = getPerpendicular(x1, y1, xCenter, yCenter, sizeEdge);
						
						double[] perpenX1 = getPerpendicular(xCenter, yCenter, x1, y1, nodeSize);
						
						double[] perpenXCenter1 = getPerpendicular(x1, y1, xCenterCenter, yCenterCenter, sizeEdge);
						
						concat(buffer, " M ", d(perpenX1[0]), " ", d(perpenX1[1]));
						concat(buffer, " Q ", d(perpenXCenter1[0]), " ", d(perpenXCenter1[1]), " ", d(perpenCenter[0]), " ", d(perpenCenter[1]));
						concat(buffer, " L ", d(x2), " ", d(y2));
						concat(buffer, " L ", d(perpenCenter[2]), " ", d(perpenCenter[3]));
						concat(buffer, " Q ", d(perpenXCenter1[2]), " ", d(perpenXCenter1[3]), " ", d(perpenX1[2]), " ", d(perpenX1[3]));
						concat(buffer, " Z");
						
						if(! edge.isDirected()) {
							double[] perpenX2 = getPerpendicular(xCenter, yCenter, x2, y2, nodeSize);
							
							xCenterCenter2nd = (x2+xCenter)/2;
							yCenterCenter2nd = (y2+yCenter)/2;
							
							double[] perpenXCenter2 = getPerpendicular(x2, y2, xCenterCenter2nd, yCenterCenter2nd, sizeEdge);

							concat(buffer, " M ", d(perpenX2[0]), " ", d(perpenX2[1]));
							concat(buffer, " Q ", d(perpenXCenter2[0]), " ", d(perpenXCenter2[1]), " ", d(perpenCenter[0]), " ", d(perpenCenter[1]));
							concat(buffer, " L ", d(x1), " ", d(y1));
							concat(buffer, " L ", d(perpenCenter[2]), " ", d(perpenCenter[3]));
							concat(buffer, " Q ", d(perpenXCenter2[2]), " ", d(perpenXCenter2[3]), " ", d(perpenX2[2]), " ", d(perpenX2[3]));
							concat(buffer, " Z");
						}
						
						break;
					default:
					case LINE:
						concat(buffer, " M ", d(x1), " ", d(y1));
						concat(buffer, " L ", d(x2), " ", d(y2));
						
						break;
				}
				
				//-------------------- draw arrow
				
				if(edge.isDirected()) {
					//--------------------- Size node ---------------------------------------
					nodeSize = svgStyles[groups.getStyleFor(trg)].group.getSize()[0];
					double diag = -1;
					if (svgStyles[groups.getStyleFor(trg)].group.getSize().getValueCount() > 1) {
						diag = Math.Sqrt(Math.Pow(nodeSize, 2)+Math.Pow(svgStyles[groups.getStyleFor(trg)].group.getSize()[1], 2));
						nodeSize = Math.Min(nodeSize,svgStyles[groups.getStyleFor(trg)].group.getSize()[1]);
					} else {
						diag = Math.Sqrt(Math.Pow(nodeSize, 2)+Math.Pow(nodeSize, 2));
					}
										
					if(svgStyles[groups.getStyleFor(trg)].group.getShape().Equals(Shape.CIRCLE)) {
						nodeSize = nodeSize/2;
					} else if (svgStyles[groups.getStyleFor(trg)].group.getShape().Equals(Shape.BOX) ||
							svgStyles[groups.getStyleFor(trg)].group.getShape().Equals(Shape.ROUNDED_BOX) ||
							svgStyles[groups.getStyleFor(trg)].group.getShape().Equals(Shape.DIAMOND) ||
							svgStyles[groups.getStyleFor(trg)].group.getShape().Equals(Shape.TRIANGLE)) {
						nodeSize = diag/2 ;
					}
					//----------------------------------------------------------------------
					
					double distance = Math.Sqrt(((x2-x1)*(x2-x1))+((y2-y1)*(y2-y1)));
					
					double ratioPoint, ratioLine ;
					double x2Root, y2Root ;
					double x2Point, y2Point ;
					
					double x1Prim, y1Prim, x2Prim, y2Prim ;
					switch (style.group.getArrowShape()) {
						case CIRCLE:
							ratioPoint = 1-(nodeSize/distance);
							ratioLine = 1-(((sx/2)+nodeSize)/distance);
							
							x2Root = (((1-ratioLine)*x1)+(ratioLine*x2));
							y2Root = (((1-ratioLine)*y1)+(ratioLine*y2));
							
							x2Point = (((1-ratioPoint)*x1)+(ratioPoint*x2));
							y2Point = (((1-ratioPoint)*y1)+(ratioPoint*y2));
							
							perpen = getPerpendicular(x2, y2, x2Root, y2Root, sy);
							x1Prim = perpen[0];
							y1Prim = perpen[1];
							x2Prim = perpen[2];
							y2Prim = perpen[3];
							
							concat(buffer, " M ", d(x1Prim), " ", d(y1Prim));
							concat(buffer, " A ", d(sx / 4), " ", d(sy / 4), " 0 1 0 ", d(x2Prim), " ", d(y2Prim));
							concat(buffer, " ", d(sx / 4), " ", d(sy / 4), " 0 1 0 ", d(x1Prim), " ", d(y1Prim));
							concat(buffer, " Z");
							break;

						case DIAMOND:
							ratioPoint = 1-(nodeSize/distance);
							ratioLine = 1-(((sx/2)+nodeSize)/distance);
							
							x2Root = (((1-ratioLine)*x1)+(ratioLine*x2));
							y2Root = (((1-ratioLine)*y1)+(ratioLine*y2));
							
							x2Point = (((1-ratioPoint)*x1)+(ratioPoint*x2));
							y2Point = (((1-ratioPoint)*y1)+(ratioPoint*y2));
							
							Console.WriteLine(nodeSize+" "+sx);
							double ratioEnd = 1-((sx+nodeSize)/distance);
							double x2End = (((1-ratioEnd)*x1)+(ratioEnd*x2));
							double y2End = (((1-ratioEnd)*y1)+(ratioEnd*y2));
							
														
							perpen = getPerpendicular(x2, y2, x2Root, y2Root, sy);
							x1Prim = perpen[0];
							y1Prim = perpen[1];
							x2Prim = perpen[2];
							y2Prim = perpen[3];
							
							concat(buffer, " M ", d(x2Point), " ", d(y2Point));
							concat(buffer, " L ", d(x1Prim), " ", d(y1Prim));
							concat(buffer, " L ", d(x2End), " ", d(y2End));
							concat(buffer, " L ", d(x2Prim), " ", d(y2Prim));

							//concat(buffer, " L ", d(x2Point), " ", d(y2Point));
							concat(buffer, " Z");


							
							break;
						default:
						case ARROW:
							ratioPoint = 1-(nodeSize/distance);
							ratioLine = 1-((sx+nodeSize)/distance);
							
							x2Root = (((1-ratioLine)*x1)+(ratioLine*x2));
							y2Root = (((1-ratioLine)*y1)+(ratioLine*y2));
							
							x2Point = (((1-ratioPoint)*x1)+(ratioPoint*x2));
							y2Point = (((1-ratioPoint)*y1)+(ratioPoint*y2));
														
							perpen = getPerpendicular(x2, y2, x2Root, y2Root, sy);
							x1Prim = perpen[0];
							y1Prim = perpen[1];
							x2Prim = perpen[2];
							y2Prim = perpen[3];
							
							if (style.group.getShape().Equals(Shape.CUBIC_CURVE)) {
								double rotation = 25 ;
								
								Console.WriteLine(y2Point-y2);
								if (y2Point-y2 <= 1)
									rotation = -rotation ;
								
								Vector2 v = rotatePoint(x2, y2, rotation, x2Point, y2Point);
								x2Point = v.x();
								y2Point = v.y();
								
								v = rotatePoint(x2, y2, rotation, x1Prim, y1Prim);
								x1Prim = v.x();
								y1Prim = v.y();
								
								v = rotatePoint(x2, y2, rotation, x2Prim, y2Prim);
								x2Prim = v.x();
								y2Prim = v.y();
							}
							
							concat(buffer, " M ", d(x1Prim), " ", d(y1Prim));
							concat(buffer, " L ", d(x2Prim), " ", d(y2Prim));
							concat(buffer, " L ", d(x2Point), " ", d(y2Point));
							concat(buffer, " Z");
							

							Console.WriteLine("Arrow = ("+x1Prim+", "+y1Prim+") ("+x1Prim+", "+y2Prim+") ("+x2Point+", "+y2Point+")");

							break;

					}
					
				}
			}

			return buffer.ToString();
		}
		
		/// <summary>
/// rotates the point around a center and returns the new point
/// </summary>
/// <param name="cx">x coordinate of the center</param>
/// <param name="cy">y coordinate of the center</param>
/// <param name="angle">in degrees (sign determines the direction + is counter-clockwise - is clockwise)</param>
/// <param name="px">x coordinate of point to rotate</param>
/// <param name="py">y coordinate of point to rotate</param>

		public static Vector2 rotatePoint(double cx, double cy, double angle, double px, double py){
			double absangl = Math.Abs(angle);
			double s = Math.Sin((Math.PI / 180.0 * (absangl));
			double c = Math.Cos((Math.PI / 180.0 * (absangl));

		    // translate point back to origin:
		    px -= cx;
		    py -= cy;

		    // rotate point
		    double xnew;
		    double ynew;
		    if (angle > 0) {
		        xnew = px * c - py * s;
		        ynew = px * s + py * c;
		    }
		    else {
		        xnew = px * c + py * s;
		        ynew = -px * s + py * c;
		    }

		    // translate point back:
		    px = xnew + cx;
		    py = ynew + cy;
		    return new Vector2(px, py);
		}
		
		public double[] getPerpendicular(double x1, double y1, double x2, double y2, double size) {
			double slope, slopePerpen;

			slope = (y2-y1)/(x2-x1);

			double x1Prim, x2Prim, y1Prim, y2Prim ;
			if(double.isInfinite(slope)) {								
				x1Prim = x2-(size/2);
				y1Prim = y2;
				
				x2Prim = x2+(size/2);
				y2Prim = y2;
			}
			else if (slope == 0) {								
				x1Prim = x2;
				y1Prim = y2-(size/2);
				
				x2Prim = x2;
				y2Prim = y2+(size/2);
			}
			else {
				slopePerpen = (-1/slope);
				
				//concat(buffer, " m ", d(x2), " ", d(y2));
				double deltaX = 1/(Math.Sqrt((slopePerpen*slopePerpen)+1));
				double deltaY = slopePerpen/(Math.Sqrt((slopePerpen*slopePerpen)+1));
				
				x1Prim = x2-((size/2)*deltaX);
				y1Prim = y2-((size/2)*deltaY);
				
				x2Prim = x2+((size/2)*deltaX);
				y2Prim = y2+((size/2)*deltaY);
			}
			
			return new double[] {x1Prim, y1Prim, x2Prim, y2Prim}; 
		}
		
		public double getValue(Value v, bool horizontal) {
			return getValue(v.value, v.units, horizontal);
		}

		public double getValue(double d, StyleConstants.Units units, bool horizontal) {
			switch (units) {
			case PX:
				// TODO
				return d;
			case GU:
				// TODO
				return d;
			case PERCENTS:
				if (horizontal)
					return (viewBox.x2 - viewBox.x1) * d / 100.0;
				else
					return (viewBox.y2 - viewBox.y1) * d / 100.0;
			}

			return d;
		}
	}

	class ViewBox {
		double x1, y1, x2, y2;
		double x3, y3, x4, y4;

		double[] padding = { 0, 0 };

		ViewBox(double x1, double y1, double x2, double y2) {
			this.x1 = x1;
			this.y1 = y1;
			this.x2 = x2;
			this.y2 = y2;
		}

		void compute(IGraph g, StyleGroup style) {
			x3 = y3 = double.MaxValue;
			x4 = y4 = double.Epsilon;

			g.nodes().ToList().ForEach(n => {
				x3 = Math.Min(x3, getX(n));
				y3 = Math.Min(y3, getY(n));

				x4 = Math.Max(x4, getX(n));
				y4 = Math.Max(y4, getY(n));
			});

			Values v = style.getPadding();

			if (v.getValueCount() > 0) {
				padding[0] = v[0];
				padding[1] = v.getValueCount() > 1 ? v[1] : v[0];
			}
		}

		double convertX(double x) {
			return (x2 - x1 - 2 * padding[0]) * (x - x3) / (x4 - x3) + x1 + padding[0];
		}

		double convertX(INode n) {
			return convertX(getX(n));
		}

		double convertY(double y) {
			return (y2 - y1 - 2 * padding[1]) * (y - y3) / (y4 - y3) + y1 + padding[1];
		}

		double convertY(INode n) {
			return convertY(getY(n));
		}
	}

	class SVGStyle {

		static int gradientId = 0;

		string style;
		StyleGroup group;
		bool gradient;
		bool dynfill;

		public SVGStyle(StyleGroup group){

			this.group = group;
			this.gradient = false;
			this.dynfill = false;

			switch (group.getType()) {
			case EDGE:
				buildEdgeStyle();
				break;
			case NODE:
				buildNodeStyle();
				break;
			case GRAPH:
				buildGraphStyle();
				break;
			case SPRITE:
			default:
				break;
			}
		}

		void buildNodeStyle() {
			System.Text.StringBuilder styleSB = new System.Text.StringBuilder();

			switch (group.getFillMode()) {
			case GRADIENT_RADIAL:
			case GRADIENT_HORIZONTAL:
			case GRADIENT_VERTICAL:
			case GRADIENT_DIAGONAL1:
			case GRADIENT_DIAGONAL2:
				concat(styleSB, "fill:url(#%gradient-id%);");
				this.gradient = true;
				break;
			case PLAIN:
				concat(styleSB, "fill:", toHexColor(group.getFillColor(0)), ";");
				concat(styleSB, "fill-opacity:", d(group.getFillColor(0).getAlpha() / 255.0), ";");
				break;
			case DYN_PLAIN:
				dynfill = true;
				concat(styleSB, "fill:%fill-color%;");
				concat(styleSB, "fill-opacity:%fill-opacity%;");
				break;
			case IMAGE_TILED:
			case IMAGE_SCALED:
			case IMAGE_SCALED_RATIO_MAX:
			case IMAGE_SCALED_RATIO_MIN:
			case NONE:
				break;
			}

			concat(styleSB, "fill-rule:nonzero;");

			if (group.getStrokeMode() != StrokeMode.NONE) {
				concat(styleSB, "stroke:", toHexColor(group.getStrokeColor(0)), ";");
				concat(styleSB, "stroke-width:", getSize(group.getStrokeWidth()), ";");
			}

			style = styleSB.ToString();
		}

		void buildGraphStyle() {
			buildNodeStyle();
		}

		void buildEdgeStyle() {
			System.Text.StringBuilder styleSB = new System.Text.StringBuilder();

			switch (group.getFillMode()) {
			case GRADIENT_RADIAL:
			case GRADIENT_HORIZONTAL:
			case GRADIENT_VERTICAL:
			case GRADIENT_DIAGONAL1:
			case GRADIENT_DIAGONAL2:
				concat(styleSB, "stroke:url(#%gradient-id%);");
				this.gradient = true;
				break;
			case PLAIN:
				concat(styleSB, "fill:", toHexColor(group.getFillColor(0)), ";");
				concat(styleSB, "fill-opacity:", d(group.getFillColor(0).getAlpha() / 255.0), ";");
				concat(styleSB, "stroke:", toHexColor(group.getFillColor(0)), ";");
				break;
			case DYN_PLAIN:
				concat(styleSB, "stroke:", toHexColor(group.getFillColor(0)), ";");
				break;
			case IMAGE_TILED:
			case IMAGE_SCALED:
			case IMAGE_SCALED_RATIO_MAX:
			case IMAGE_SCALED_RATIO_MIN:
			case NONE:
				break;
			}
			
			if (! group.getShape().Equals(Shape.ANGLE) && ! group.getShape().Equals(Shape.BLOB)) { // Size used in the path creation
				concat(styleSB, "stroke-width:", getSize(group.getSize(), 0), ";");
			}

			style = styleSB.ToString();
		}

		public void writeDef(XMLWriter out){
			if (gradient) {
				string gid = string.Format("gradient{0}", gradientId++);
				string type = "linearGradient";
				string x1 = null, x2 = null, y1 = null, y2 = null;

				switch (group.getFillMode()) {
				case GRADIENT_RADIAL:
					type = "radialGradient";
					break;
				case GRADIENT_HORIZONTAL:
					x1 = "0%";
					y1 = "50%";
					x2 = "100%";
					y2 = "50%";
					break;
				case GRADIENT_VERTICAL:
					x1 = "50%";
					y1 = "0%";
					x2 = "50%";
					y2 = "100%";
					break;
				case GRADIENT_DIAGONAL1:
					x1 = "0%";
					y1 = "0%";
					x2 = "100%";
					y2 = "100%";
					break;
				case GRADIENT_DIAGONAL2:
					x1 = "100%";
					y1 = "100%";
					x2 = "0%";
					y2 = "0%";
					break;
				default:
					break;
				}

				output.open(type);
				output.attribute("id", gid);
				output.attribute("gradientUnits", "objectBoundingBox");

				if (type.Equals("linearGradient")) {
					output.attribute("x1", x1);
					output.attribute("y1", y1);
					output.attribute("x2", x2);
					output.attribute("y2", y2);
				}

				for (int i = 0; i < group.getFillColorCount(); i++) {
					output.open("stop");
					output.attribute("stop-color", toHexColor(group.getFillColor(i)));
					output.attribute("stop-opacity", d(group.getFillColor(i).getAlpha() / 255.0));
					output.attribute("offset", double.toString(i / (double) (group.getFillColorCount() - 1)));
					output.Close();
				}

				output.Close();

				style = style.Replace("%gradient-id%", gid);
			}
		}

		public string getElementStyle(IElement e) {
			string st = style;

			if (dynfill) {
				if (group.getFillColorCount() > 1) {
					string color, opacity;
					double d = e.hasNumber("ui.color") ? e.getNumber("ui.color") : 0;

					double a, b;
					Colors colors = group.getFillColors();
					int s = Math.Min((int) (d * group.getFillColorCount()), colors.Count - 2);

					a = s / (double) (colors.Count - 1);
					b = (s + 1) / (double) (colors.Count - 1);

					d = (d - a) / (b - a);

					Color c1 = colors[s], c2 = colors[s + 1];

					color = string.Format("#{0}{1}{2}", (int) (c1.getRed() + d * (c2.getRed() - c1.getRed())),
							(int) (c1.getGreen() + d * (c2.getGreen() - c1.getGreen())),
							(int) (c1.getBlue() + d * (c2.getBlue() - c1.getBlue())));

					opacity = double.toString((c1.getAlpha() + d * (c2.getAlpha() - c1.getAlpha())) / 255.0);

					st = st.Replace("%fill-color%", color);
					st = st.Replace("%fill-opacity%", opacity);
				}
			}

			return st;
		}
	}

	class XMLWriter {
		XMLStreamWriter out;
		int depth;
		bool closed;

		void start(System.IO.TextWriter w){
			if (out != null)
				end();

			out = XMLOutputFactory.newInstance().createXMLStreamWriter(w);
			output.writeStartDocument();
		}

		void end(){
			output.writeEndDocument();
			/* output.Flush(); */
			output.Close();
			out = null;
		}

		void open(string name){
			output.writeCharacters("\n");
			for (int i = 0; i < depth; i++)
				output.writeCharacters("  ");

			output.writeStartElement(name);
			depth++;
		}

		void close(){
			output.writeEndElement();
			depth--;
		}

		void attribute(string key, string value){
			output.writeAttribute(key, value);
		}

		void characters(string data){
			output.writeCharacters(data);
		}
	}

	private static void concat(System.Text.StringBuilder buffer, params object[] args) {
		if (args != null) {
			for (int i = 0; i < args.Length; i++)
				buffer.Append(args[i].ToString());
		}
	}

	private static string toHexColor(Color c) {
		return string.Format("#{0}{1}{2}", c.getRed(), c.getGreen(), c.getBlue());
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void edgeAttributeAdded(string sourceId, long timeId, string edgeId, string attribute, object value) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void edgeAttributeChanged(string sourceId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#edgeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void edgeAttributeRemoved(string sourceId, long timeId, string edgeId, string attribute) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#graphAttributeAdded(java.lang.String ,
	 * long, java.lang.String, java.lang.object)
	 */
	public void graphAttributeAdded(string sourceId, long timeId, string attribute, object value) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeChanged(java.lang.
	 * String, long, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void graphAttributeChanged(string sourceId, long timeId, string attribute, object oldValue,
			object newValue) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.AttributeSink#graphAttributeRemoved(java.lang.
	 * String, long, java.lang.String)
	 */
	public void graphAttributeRemoved(string sourceId, long timeId, string attribute) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeAdded(java.lang.String,
	 * long, java.lang.String, java.lang.String, java.lang.object)
	 */
	public void nodeAttributeAdded(string sourceId, long timeId, string nodeId, string attribute, object value) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeChanged(java.lang.String ,
	 * long, java.lang.String, java.lang.String, java.lang.object, java.lang.object)
	 */
	public void nodeAttributeChanged(string sourceId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * org.graphstream.stream.AttributeSink#nodeAttributeRemoved(java.lang.String ,
	 * long, java.lang.String, java.lang.String)
	 */
	public void nodeAttributeRemoved(string sourceId, long timeId, string nodeId, string attribute) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeAdded(java.lang.String, long,
	 * java.lang.String, java.lang.String, java.lang.String, boolean)
	 */
	public void edgeAdded(string sourceId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#edgeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void edgeRemoved(string sourceId, long timeId, string edgeId) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#graphCleared(java.lang.String, long)
	 */
	public void graphCleared(string sourceId, long timeId) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeAdded(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeAdded(string sourceId, long timeId, string nodeId) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#nodeRemoved(java.lang.String, long,
	 * java.lang.String)
	 */
	public void nodeRemoved(string sourceId, long timeId, string nodeId) {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.ElementSink#stepBegins(java.lang.String, long,
	 * double)
	 */
	public void stepBegins(string sourceId, long timeId, double step) {
	}
}

}
