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
/// Source to read GPX (GPS eXchange Format) data an XML extension to exchange gps coordinates, routes and tracks. Read more about GPX at <a href="https://en.wikipedia.org/wiki/GPS_eXchange_Format">Wikipedia</a>
/// </summary>
public class FileSourceGPX : FileSourceXML {

	/// <summary>
/// Parser used by this source.
/// </summary>
	protected GPXParser parser;

	public FileSourceGPX() {
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSourceXML#afterStartDocument()
	 */
	protected void afterStartDocument(){
		parser = new GPXParser();
		parser.__gpx();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSourceXML#beforeEndDocument()
	 */
	protected void beforeEndDocument(){
		parser = null;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see org.graphstream.stream.file.FileSourceXML#nextEvents()
	 */
	public bool nextEvents(){
		return false;
	}

	protected class WayPoint {
		string name;
		double lat, lon, ele;
		Dictionary<string, object> attributes;

		WayPoint() {
			attributes = new Dictionary<string, object>();
			name = null;
			lat = lon = ele = 0;
		}

		void deploy() {
			sendNodeAdded(sourceId, name);
			sendNodeAttributeAdded(sourceId, name, "xyz", new double[] { lon, lat, ele });

			foreach (string key in attributes.Keys)
				sendNodeAttributeAdded(sourceId, name, key, attributes[key]);
		}
	}

	protected class GPXParser : Parser, GPXConstants {

		int automaticPointId;
		int automaticRouteId;
		int automaticEdgeId;

		GPXParser() {
			automaticRouteId = 0;
			automaticPointId = 0;
			automaticEdgeId = 0;
		}

		/// <summary>
/// Base for read points since points can be one of "wpt", "rtept", "trkpt".
/// </summary>
/// <param name="elementName"></param>
/// <returns></returns>
		private WayPoint waypoint(string elementName){
			XMLEvent e;
			WayPoint wp = new WayPoint();
			EnumMap<WPTAttribute, string> attributes;
			List<string> links = new List<string>();

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, elementName);

			attributes = getAttributes(typeof(WPTAttribute), e.asStartElement());

			if (!attributes.ContainsKey(WPTAttribute.LAT)) {
				newParseError(e, false, "attribute 'lat' is required");
			}

			if (!attributes.ContainsKey(WPTAttribute.LON)) {
				newParseError(e, false, "attribute 'lon' is required");
			}

			wp.lat = double.Parse(attributes[WPTAttribute.LAT]);
			wp.lon = double.Parse(attributes[WPTAttribute.LON]);
			wp.ele = 0;

			wp.attributes["lat"] = wp.lat;
			wp.attributes["lon"] = wp.lon;

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "ele")) {
				pushback(e);
				wp.ele = __ele();
				wp.attributes["ele"] = wp.ele;

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "time")) {
				pushback(e);
				wp.attributes["time"] = __time();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "magvar")) {
				pushback(e);
				wp.attributes["magvar"] = __magvar();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "geoidheight")) {
				pushback(e);
				wp.attributes["geoidheight"] = __geoidheight();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "name")) {
				pushback(e);
				wp.name = __name();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "cmt")) {
				pushback(e);
				wp.attributes["cmt"] = __cmt();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				pushback(e);
				wp.attributes["desc"] = __desc();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "src")) {
				pushback(e);
				wp.attributes["src"] = __src();

				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "link")) {
				pushback(e);
				links.Add(__link());

				e = getNextEvent();
			}

			wp.attributes["link"] = links.toArray(new string[links.Count]);

			if (isEvent(e, XMLEvent.START_ELEMENT, "sym")) {
				pushback(e);
				wp.attributes["sym"] = __sym();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "type")) {
				pushback(e);
				wp.attributes["type"] = __type();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "fix")) {
				pushback(e);
				wp.attributes["fix"] = __fix();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "sat")) {
				pushback(e);
				wp.attributes["sat"] = __sat();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "hdop")) {
				pushback(e);
				wp.attributes["hdop"] = __hdop();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "vdop")) {
				pushback(e);
				wp.attributes["vdop"] = __vdop();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "pdop")) {
				pushback(e);
				wp.attributes["pdop"] = __pdop();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "ageofdgpsdata")) {
				pushback(e);
				wp.attributes["ageofdgpsdata"] = __ageofdgpsdata();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "dgpsid")) {
				pushback(e);
				wp.attributes["dgpsid"] = __dgpsid();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "extensions")) {
				pushback(e);
				__extensions();

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, elementName);

			if (wp.name == null)
				wp.name = string.Format("wp#{0}", automaticPointId++);

			return wp;
		}

		/// <summary>
/// <pre> name       : GPX attributes : GPXAttribute structure  : METADATA? WPT* RTE* TRK* EXTENSIONS? </pre>
/// </summary>
		private void __gpx(){
			XMLEvent e;
			EnumMap<GPXAttribute, string> attributes;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "gpx");

			attributes = getAttributes(typeof(GPXAttribute), e.asStartElement());

			if (!attributes.ContainsKey(GPXAttribute.VERSION)) {
				newParseError(e, false, "attribute 'version' is required");
			} else {
				sendGraphAttributeAdded(sourceId, "gpx.version", attributes[GPXAttribute.VERSION]);
			}

			if (!attributes.ContainsKey(GPXAttribute.CREATOR)) {
				newParseError(e, false, "attribute 'creator' is required");
			} else {
				sendGraphAttributeAdded(sourceId, "gpx.creator", attributes[GPXAttribute.CREATOR]);
			}

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "metadata")) {
				pushback(e);
				__metadata();

				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "wpt")) {
				pushback(e);
				__wpt();

				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "rte")) {
				pushback(e);
				__rte();

				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "trk")) {
				pushback(e);
				__trk();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "extensions")) {
				pushback(e);
				__extensions();

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "gpx");
		}

		/// <summary>
/// <pre> name       : METADATA attributes : structure  : NAME? DESC? AUTHOR? COPYRIGHT? LINK* TIME? KEYWORDS? BOUNDS? EXTENSIONS? </pre>
/// </summary>
		private void __metadata(){
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "metadata");

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "name")) {
				pushback(e);
				sendGraphAttributeAdded(sourceId, "gpx.metadata.name", __name());

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				pushback(e);
				sendGraphAttributeAdded(sourceId, "gpx.metadata.desc", __desc());

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "author")) {
				pushback(e);
				sendGraphAttributeAdded(sourceId, "gpx.metadata.author", __author());

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "copyright")) {
				pushback(e);
				sendGraphAttributeAdded(sourceId, "gpx.metadata.copyright", __copyright());

				e = getNextEvent();
			}

			List<string> links = new List<string>();

			while (isEvent(e, XMLEvent.START_ELEMENT, "link")) {
				pushback(e);
				links.Add(__link());

				e = getNextEvent();
			}

			if (links.Count > 0)
				sendGraphAttributeAdded(sourceId, "gpx.metadata.links", links.toArray(new string[links.Count]));

			if (isEvent(e, XMLEvent.START_ELEMENT, "time")) {
				pushback(e);
				sendGraphAttributeAdded(sourceId, "gpx.metadata.time", __time());

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "keywords")) {
				pushback(e);
				sendGraphAttributeAdded(sourceId, "gpx.metadata.keywords", __keywords());

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "bounds")) {
				pushback(e);
				__bounds();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "extensions")) {
				pushback(e);
				__extensions();

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "metadata");
		}

		/// <summary>
/// <pre> name       : WPT attributes : WPTAttribute structure  : ELE? TIME? MAGVAR? GEOIDHEIGHT? NAME? CMT? DESC? SRC? LINK* SYM? TYPE? FIX? SAT? HDOP? VDOP? PDOP? AGEOFDGPSDATA? DGPSID? EXTENSIONS? </pre>
/// </summary>
		private void __wpt(){
			WayPoint wp = waypoint("wpt");
			wp.deploy();
		}

		/// <summary>
/// <pre> name       : RTE attributes : structure  : NAME? CMT? DESC? SRC? LINK* NUMBER? TYPE? EXTENSIONS? RTEPT* </pre>
/// </summary>
		private void __rte(){
			XMLEvent e;
			string name, cmt, desc, src, type, time;
			int number;
			List<string> links = new List<string>();
			List<WayPoint> points = new List<WayPoint>();

			name = cmt = desc = src = type = time = null;
			number = -1;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "rte");

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "name")) {
				pushback(e);
				name = __name();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "time")) {
				pushback(e);
				time = __time();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "cmt")) {
				pushback(e);
				cmt = __cmt();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				pushback(e);
				desc = __desc();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "src")) {
				pushback(e);
				src = __src();

				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "link")) {
				pushback(e);
				links.Add(__link());

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "number")) {
				pushback(e);
				number = __number();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "type")) {
				pushback(e);
				type = __type();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "extensions")) {
				pushback(e);
				__extensions();

				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "rtept")) {
				pushback(e);
				points.Add(__rtept());

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "rte");

			if (name == null)
				name = string.Format("route#{0}", automaticRouteId++);

			sendGraphAttributeAdded(sourceId, "routes." + name, bool.TRUE);
			sendGraphAttributeAdded(sourceId, "routes." + name + ".desc", desc);
			sendGraphAttributeAdded(sourceId, "routes." + name + ".cmt", cmt);
			sendGraphAttributeAdded(sourceId, "routes." + name + ".src", src);
			sendGraphAttributeAdded(sourceId, "routes." + name + ".type", type);
			sendGraphAttributeAdded(sourceId, "routes." + name + ".time", time);
			sendGraphAttributeAdded(sourceId, "routes." + name + ".number", number);

			for (int i = 0; i < points.Count; i++) {
				points[i].deploy();

				if (i > 0) {
					string eid = string.Format("seg#{0}", automaticEdgeId++);
					sendEdgeAdded(sourceId, eid, points[i - 1].name, points[i].name, true);
					sendEdgeAttributeAdded(sourceId, eid, "route", name);
				}
			}
		}

		/// <summary>
/// <pre> name       : TRK attributes : structure  : NAME? CMT? DESC? SRC? LINK* NUMBER? TYPE? EXTENSIONS? TRKSEG* </pre>
/// </summary>
		private void __trk(){
			XMLEvent e;
			string name, cmt, desc, src, type, time;
			int number;
			List<string> links = new List<string>();

			name = cmt = desc = src = type = time = null;
			number = -1;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "trk");

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "name")) {
				pushback(e);
				name = __name();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "time")) {
				pushback(e);
				time = __time();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "cmt")) {
				pushback(e);
				cmt = __cmt();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "desc")) {
				pushback(e);
				desc = __desc();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "src")) {
				pushback(e);
				src = __src();

				e = getNextEvent();
			}

			while (isEvent(e, XMLEvent.START_ELEMENT, "link")) {
				pushback(e);
				links.Add(__link());

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "number")) {
				pushback(e);
				number = __number();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "type")) {
				pushback(e);
				type = __type();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "extensions")) {
				pushback(e);
				__extensions();

				e = getNextEvent();
			}

			if (name == null)
				name = string.Format("route#{0}", automaticRouteId++);

			sendGraphAttributeAdded(sourceId, "tracks." + name, bool.TRUE);
			sendGraphAttributeAdded(sourceId, "tracks." + name + ".desc", desc);
			sendGraphAttributeAdded(sourceId, "tracks." + name + ".cmt", cmt);
			sendGraphAttributeAdded(sourceId, "tracks." + name + ".src", src);
			sendGraphAttributeAdded(sourceId, "tracks." + name + ".type", type);
			sendGraphAttributeAdded(sourceId, "tracks." + name + ".time", time);
			sendGraphAttributeAdded(sourceId, "tracks." + name + ".number", number);

			while (isEvent(e, XMLEvent.START_ELEMENT, "trkseg")) {
				pushback(e);
				List<WayPoint> wps = __trkseg();

				for (int i = 0; i < wps.Count; i++) {
					wps[i].deploy();

					if (i > 0) {
						string eid = string.Format("seg#{0}", automaticEdgeId++);
						sendEdgeAdded(sourceId, eid, wps[i - 1].name, wps[i].name, true);
						sendEdgeAttributeAdded(sourceId, eid, "route", name);
					}
				}

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "trk");
		}

		/// <summary>
/// <pre> name       : EXTENSIONS attributes : structure  : </pre>
/// </summary>
		private void __extensions(){
			XMLEvent e;
			int stack = 0;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "extensions");

			e = getNextEvent();

			while (!(isEvent(e, XMLEvent.END_ELEMENT, "extensions") && stack == 0)) {
				if (isEvent(e, XMLEvent.END_ELEMENT, "extensions"))
					stack--;
				else if (isEvent(e, XMLEvent.START_ELEMENT, "extensions"))
					stack++;

				e = getNextEvent();
			}
		}

		/// <summary>
/// <pre> name       : NAME attributes : structure  : string </pre>
/// </summary>
		private string __name(){
			string name;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "name");

			name = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "name");

			return name;
		}

		/// <summary>
/// <pre> name       : DESC attributes : structure  : string </pre>
/// </summary>
		private string __desc(){
			string desc;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "desc");

			desc = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "desc");

			return desc;
		}

		/// <summary>
/// <pre> name       : AUTHOR attributes : structure  : NAME? EMAIL? LINK? </pre>
/// </summary>
		private string __author(){
			string author = "";
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "author");

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "name")) {
				pushback(e);
				author += __name();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "email")) {
				pushback(e);
				author += " <" + __email() + ">";

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "link")) {
				pushback(e);
				author += " (" + __link() + ")";

				e = getNextEvent();
			}

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "author");

			return author;
		}

		/// <summary>
/// <pre> name       : COPYRIGHT attributes : COPYRIGHTAttribute structure  : YEAR? LICENCE? </pre>
/// </summary>
		private string __copyright(){
			string copyright;
			XMLEvent e;
			EnumMap<COPYRIGHTAttribute, string> attributes;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "copyright");

			attributes = getAttributes(typeof(COPYRIGHTAttribute), e.asStartElement());

			if (!attributes.ContainsKey(COPYRIGHTAttribute.AUTHOR)) {
				newParseError(e, false, "attribute 'author' is required");
				copyright = "unknown";
			} else
				copyright = attributes[COPYRIGHTAttribute.AUTHOR];

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "year")) {
				pushback(e);
				copyright += " " + __year();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "license")) {
				pushback(e);
				copyright += " " + __license();

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "copyright");

			return copyright;
		}

		/// <summary>
/// <pre> name       : LINK attributes : LINKAttribute structure  : TEXT? TYPE? </pre>
/// </summary>
		private string __link(){
			string link;
			XMLEvent e;
			EnumMap<LINKAttribute, string> attributes;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "link");

			attributes = getAttributes(typeof(LINKAttribute), e.asStartElement());

			if (!attributes.ContainsKey(LINKAttribute.HREF)) {
				newParseError(e, false, "attribute 'href' is required");
				link = "unknown";
			} else
				link = attributes[LINKAttribute.HREF];

			e = getNextEvent();

			if (isEvent(e, XMLEvent.START_ELEMENT, "text")) {
				pushback(e);
				__text();

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "type")) {
				pushback(e);
				__type();

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "link");

			return link;
		}

		/// <summary>
/// <pre> name       : TIME attributes : structure  : string </pre>
/// </summary>
		private string __time(){
			string time;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "time");

			time = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "time");

			return time;
		}

		/// <summary>
/// <pre> name       : KEYWORDS attributes : structure  : string </pre>
/// </summary>
		private string __keywords(){
			string keywords;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "keywords");

			keywords = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "keywords");

			return keywords;
		}

		/// <summary>
/// <pre> name       : BOUNDS attributes : BOUNDSAttribute structure  : </pre>
/// </summary>
		private void __bounds(){
			XMLEvent e;
			EnumMap<BOUNDSAttribute, string> attributes;
			double minlat, maxlat, minlon, maxlon;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "bounds");

			attributes = getAttributes(typeof(BOUNDSAttribute), e.asStartElement());

			if (!attributes.ContainsKey(BOUNDSAttribute.MINLAT)) {
				newParseError(e, false, "attribute 'minlat' is required");
			}

			if (!attributes.ContainsKey(BOUNDSAttribute.MAXLAT)) {
				newParseError(e, false, "attribute 'maxlat' is required");
			}

			if (!attributes.ContainsKey(BOUNDSAttribute.MINLON)) {
				newParseError(e, false, "attribute 'minlon' is required");
			}

			if (!attributes.ContainsKey(BOUNDSAttribute.MAXLON)) {
				newParseError(e, false, "attribute 'maxlon' is required");
			}

			minlat = double.Parse(attributes[BOUNDSAttribute.MINLAT]);
			maxlat = double.Parse(attributes[BOUNDSAttribute.MAXLAT]);
			minlon = double.Parse(attributes[BOUNDSAttribute.MINLON]);
			maxlon = double.Parse(attributes[BOUNDSAttribute.MAXLON]);

			sendGraphAttributeAdded(sourceId, "gpx.bounds", new double[] { minlat, minlon, maxlat, maxlon });

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "bounds");
		}

		/// <summary>
/// <pre> name       : ELE attributes : structure  : double </pre>
/// </summary>
		private double __ele(){
			string ele;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "ele");

			ele = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "ele");

			return double.Parse(ele);
		}

		/// <summary>
/// <pre> name       : MAGVAR attributes : structure  : double in [0,360] </pre>
/// </summary>
		private double __magvar(){
			string magvar;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "magvar");

			magvar = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "magvar");

			return double.Parse(magvar);
		}

		/// <summary>
/// <pre> name       : GEOIDHEIGHT attributes : structure  : double </pre>
/// </summary>
		private double __geoidheight(){
			string geoidheight;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "geoidheight");

			geoidheight = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "geoidheight");

			return double.Parse(geoidheight);
		}

		/// <summary>
/// <pre> name       : CMT attributes : structure  : string </pre>
/// </summary>
		private string __cmt(){
			string cmt;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "cmt");

			cmt = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "cmt");

			return cmt;
		}

		/// <summary>
/// <pre> name       : SRC attributes : structure  : string </pre>
/// </summary>
		private string __src(){
			string src;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "src");

			src = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "src");

			return src;
		}

		/// <summary>
/// <pre> name       : SYM attributes : structure  : string </pre>
/// </summary>
		private string __sym(){
			string sym;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "sym");

			sym = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "sym");

			return sym;
		}

		/// <summary>
/// <pre> name       : TEXT attributes : structure  : string </pre>
/// </summary>
		private string __text(){
			string text;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "text");

			text = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "text");

			return text;
		}

		/// <summary>
/// <pre> name       : TYPE attributes : structure  : string </pre>
/// </summary>
		private string __type(){
			string type;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "type");

			type = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "type");

			return type;
		}

		/// <summary>
/// <pre> name       : FIX attributes : structure  : enum FixType </pre>
/// </summary>
		private string __fix throws System.IO.IOException, Exception {
			string fix;
			XMLEvent e;

			e = getNextEvent;
			checkValid;

			fix = __characters;

			if.matches$"))
				newParseError;

			e = getNextEvent;
			checkValid;

			return fix;
		}

		/// <summary>
/// <pre> name       : SAT attributes : structure  : positive integer </pre>
/// </summary>
		private int __sat(){
			string sat;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "sat");

			sat = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "sat");

			return int.Parse(sat);
		}

		/// <summary>
/// <pre> name       : HDOP attributes : structure  : double </pre>
/// </summary>
		private double __hdop(){
			string hdop;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "hdop");

			hdop = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "hdop");

			return double.Parse(hdop);
		}

		/// <summary>
/// <pre> name       : VDOP attributes : structure  : double </pre>
/// </summary>
		private double __vdop(){
			string vdop;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "vdop");

			vdop = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "vdop");

			return double.Parse(vdop);
		}

		/// <summary>
/// <pre> name       : PDOP attributes : structure  : double </pre>
/// </summary>
		private double __pdop(){
			string pdop;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "pdop");

			pdop = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "pdop");

			return double.Parse(pdop);
		}

		/// <summary>
/// <pre> name       : AGEOFDGPSDATA attributes : structure  : double </pre>
/// </summary>
		private double __ageofdgpsdata(){
			string ageofdgpsdata;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "ageofdgpsdata");

			ageofdgpsdata = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "ageofdgpsdata");

			return double.Parse(ageofdgpsdata);
		}

		/// <summary>
/// <pre> name       : DGPSID attributes : structure  : integer in [0,1023] </pre>
/// </summary>
		private int __dgpsid(){
			string dgpsid;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "dgpsid");

			dgpsid = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "dgpsid");

			return int.Parse(dgpsid);
		}

		/// <summary>
/// <pre> name       : NUMBER attributes : structure  : positive integer </pre>
/// </summary>
		private int __number(){
			string number;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "number");

			number = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "number");

			return int.Parse(number);
		}

		/// <summary>
/// <pre> name       : RTEPT attributes : structure  : __wptType </pre>
/// </summary>
		private WayPoint __rtept(){
			return waypoint("rtept");
		}

		/// <summary>
/// <pre> name       : TRKPT attributes : structure  : __wptType </pre>
/// </summary>
		private WayPoint __trkpt(){
			return waypoint("trkpt");
		}

		/// <summary>
/// <pre> name       : TRKSEG attributes : structure  : TRKPT* EXTENSIONS? </pre>
/// </summary>
		private List<WayPoint> __trkseg(){
			List<WayPoint> points = new List<WayPoint>();
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "trkseg");

			e = getNextEvent();

			while (isEvent(e, XMLEvent.START_ELEMENT, "trkpt")) {
				pushback(e);
				points.Add(__trkpt());

				e = getNextEvent();
			}

			if (isEvent(e, XMLEvent.START_ELEMENT, "extensions")) {
				pushback(e);
				__extensions();

				e = getNextEvent();
			}

			checkValid(e, XMLEvent.END_ELEMENT, "trkseg");

			return points;
		}

		/// <summary>
/// <pre> name       : EMAIL attributes : EMAILAttribute structure  : </pre>
/// </summary>
		private string __email(){
			XMLEvent e;
			EnumMap<EMAILAttribute, string> attributes;
			string email = "";

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "email");

			attributes = getAttributes(typeof(EMAILAttribute), e.asStartElement());

			if (!attributes.ContainsKey(EMAILAttribute.ID)) {
				newParseError(e, false, "attribute 'version' is required");
			} else
				email += attributes[EMAILAttribute.ID];

			email += "@";

			if (!attributes.ContainsKey(EMAILAttribute.DOMAIN)) {
				newParseError(e, false, "attribute 'version' is required");
			} else
				email += attributes[EMAILAttribute.DOMAIN];

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "email");

			return email;
		}

		/// <summary>
/// <pre> name       : YEAR attributes : structure  : string </pre>
/// </summary>
		private string __year(){
			string year;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "year");

			year = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "year");

			return year;
		}

		/// <summary>
/// <pre> name       : LICENSE attributes : structure  : string </pre>
/// </summary>
		private string __license(){
			string license;
			XMLEvent e;

			e = getNextEvent();
			checkValid(e, XMLEvent.START_ELEMENT, "license");

			license = __characters();

			e = getNextEvent();
			checkValid(e, XMLEvent.END_ELEMENT, "license");

			return license;
		}
	}

	static interface GPXConstants {
		enum Balise {
			GPX, METADATA, WPT, RTE, TRK, EXTENSIONS, NAME, DESC, AUTHOR, COPYRIGHT, LINK, TIME, KEYWORDS, BOUNDS, ELE, MAGVAR, GEOIDHEIGHT, CMT, SRC, SYM, TYPE, FIX, SAT, HDOP, VDOP, PDOP, AGEOFDGPSDATA, DGPSID, NUMBER, RTEPT, TRKSEG, TRKPT, YEAR, LICENCE, TEXT, EMAIL, PT
		}

		enum GPXAttribute {
			CREATOR, VERSION
		}

		enum WPTAttribute {
			LAT, LON
		}

		enum LINKAttribute {
			HREF
		}

		enum EMAILAttribute {
			ID, DOMAIN
		}

		enum PTAttribute {
			LAT, LON
		}

		enum BOUNDSAttribute {
			MINLAT, MAXLAT, MINLON, MAXLON
		}

		enum COPYRIGHTAttribute {
			AUTHOR
		}

		enum FixType {
			T_NONE, T_2D, T_3D, T_DGPS, T_PPS
		}
	}
}

}
