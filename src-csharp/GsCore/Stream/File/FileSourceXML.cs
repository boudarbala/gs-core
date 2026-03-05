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


/// <summary>
/// Base for XML-based file format. It uses an xml events stream ( {@link javax.xml.stream}). One who want to define a new xml-based fiel source has to define actions after the document start and before the document end. The {@link #nextEvents()}, called between start and end, has to be defined too.
/// </summary>
abstract class FileSourceXML : SourceBase, IFileSource, XMLStreamConstants {
	private static readonly object /* Logger */ LOGGER = null /* Logger */;

	/// <summary>
/// XML events stream. Should not be used directly but with {@link #getNextEvent()}.
/// </summary>
	protected XMLEventReader reader;
	/*
	 * Used to allow 'pushback' of events.
	 */
	private Stack<XMLEvent> events;

	protected bool strictMode = true;

	protected FileSourceXML() {
		events = new Stack<object>();
	}

	/// <summary>
/// If strict mode is enabled, will produce errors while encountering unexpected attributes or elements. This is enabled by default.
/// </summary>
/// <returns>true if strict mode is enabled</returns>
	public bool isStrictMode() {
		return strictMode;
	}

	public void setStrictMode(bool strictMode) {
		this.strictMode = strictMode;
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#readAll(java.lang.String)
	 */
	public void readAll(string fileName){
		readAll(new System.IO.StreamReader(fileName));
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#readAll(java.net.URL)
	 */
	public void readAll(System.Uri url){
		readAll(url /* .openStream() */);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#readAll(java.io.InputStream)
	 */
	public void readAll(System.IO.Stream stream){
		readAll(new System.IO.StreamReader(stream));
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#readAll(java.io.Reader)
	 */
	public void readAll(System.IO.TextReader reader){
		begin(reader);
		while (nextEvents())
			;
		end();
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#begin(java.lang.String)
	 */
	public void begin(string fileName){
		begin(new System.IO.StreamReader(fileName));
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#begin(java.net.URL)
	 */
	public void begin(System.Uri url){
		begin(url /* .openStream() */);
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#begin(java.io.InputStream)
	 */
	public void begin(System.IO.Stream stream){
		begin(new System.IO.StreamReader(stream));
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#begin(java.io.Reader)
	 */
	public void begin(System.IO.TextReader reader){
		openStream(reader);
	}

	/// <summary>
/// Called after the event {@link javax.xml.stream.events.XMLEvent#START_DOCUMENT} has been received.
/// </summary>
	protected abstract void afterStartDocument();

	/// <summary>
/// Called before trying to receive the events {@link javax.xml.stream.events.XMLEvent#END_DOCUMENT}.
/// </summary>
	protected abstract void beforeEndDocument();

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#nextEvents()
	 */
	public abstract bool nextEvents();

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#nextStep()
	 */
	public bool nextStep(){
		return nextEvents();
	}

	/*
	 * (non-Javadoc)
	 *
	 * @see org.graphstream.stream.file.FileSource#end()
	 */
	public void end(){
		closeStream();
	}

	/// <summary>
/// Get a new event from the stream. This method has to be used to allow the {@link #pushback(XMLEvent)} method to work.
/// </summary>
/// <returns>the next event in the stream</returns>
	protected XMLEvent getNextEvent(){
		skipWhiteSpaces();

		if (events.Count > 0)
			return events.Pop();

		return reader.nextEvent();
	}

	/// <summary>
/// Pushback an event in the stream.
/// </summary>
/// <param name="e"> the event</param>
	protected void pushback(XMLEvent e) {
		events.Push(e);
	}

	/// <summary>
/// Generate a new parse exception.
/// </summary>
/// <param name="e"> event producing an error</param>
/// <param name="critical"> if true, will always produce an exception, else if strict mode is disable, will only produce a warning</param>
/// <param name="msg"> message to put in the exception</param>
/// <param name="args"> arguments of the message</param>
	protected void newParseError(XMLEvent e, bool critical, string msg, params object[] args){
		if (!critical && !strictMode) {
			Console.Error.WriteLine(string.Format(msg, args));
		} else {
			throw new Exception(string.Format(msg, args), e.getLocation());
		}
	}

	/// <summary>
/// Check is an event has an expected type and name.
/// </summary>
/// <param name="e"> event to check</param>
/// <param name="type"> expected type</param>
/// <param name="name"> expected name</param>
/// <returns>true is type and name are valid</returns>
	protected bool isEvent(XMLEvent e, int type, string name) {
		bool valid = e.getEventType() == type;

		if (valid) {
			switch (type) {
			case START_ELEMENT:
				valid = e.asStartElement().Name.getLocalPart().Equals(name);
				break;
			case END_ELEMENT:
				valid = e.asEndElement().Name.getLocalPart().Equals(name);
				break;
			case ATTRIBUTE:
				valid = ((Attribute) e).Name.getLocalPart().Equals(name);
				break;
			case CHARACTERS:
			case NAMESPACE:
			case PROCESSING_INSTRUCTION:
			case COMMENT:
			case START_DOCUMENT:
			case END_DOCUMENT:
			case DTD:
			}
		}

		return valid;
	}

	/// <summary>
/// Check is the event has valid type and name. If not, a new exception is thrown. if event has invalid type or name
/// </summary>
/// <param name="e"> event to check</param>
/// <param name="type"> expected type</param>
/// <param name="name"> expected name</param>
	protected void checkValid(XMLEvent e, int type, string name){
		bool valid = isEvent(e, type, name);

		if (!valid)
			newParseError(e, true, "expecting {0}, got {1}", gotWhat(type, name), gotWhat(e));
	}

	private string gotWhat(XMLEvent e) {
		string v = null;

		switch (e.getEventType()) {
		case START_ELEMENT:
			v = e.asStartElement().Name.getLocalPart();
			break;
		case END_ELEMENT:
			v = e.asEndElement().Name.getLocalPart();
			break;
		case ATTRIBUTE:
			v = ((Attribute) e).Name.getLocalPart();
			break;
		}

		return gotWhat(e.getEventType(), v);
	}

	private string gotWhat(int type, string v) {
		switch (type) {
		case START_ELEMENT:
			return string.Format("'<{0}>'", v);
		case END_ELEMENT:
			return string.Format("'</{0}>'", v);
		case ATTRIBUTE:
			return string.Format("attribute '{0}'", v);
		case NAMESPACE:
			return "namespace";
		case PROCESSING_INSTRUCTION:
			return "processing instruction";
		case COMMENT:
			return "comment";
		case START_DOCUMENT:
			return "document start";
		case END_DOCUMENT:
			return "document end";
		case DTD:
			return "dtd";
		case CHARACTERS:
			return "characters";
		default:
			return "UNKNOWN";
		}
	}

	private void skipWhiteSpaces(){
		XMLEvent e;

		do {
			if (events.Count > 0)
				e = events.Pop();
			else
				e = reader.nextEvent();
		} while (isEvent(e, XMLEvent.CHARACTERS, null) && e.asCharacters().getData().matches("^\\s*$"));

		pushback(e);
	}

	/// <summary>
/// Open a new xml events stream.
/// </summary>
/// <param name="stream"></param>
	protected void openStream(System.IO.TextReader stream){
		if (reader != null)
			closeStream();

		try {
			XMLEvent e;

			reader = XMLInputFactory.newInstance().createXMLEventReader(stream);

			e = getNextEvent();
			checkValid(e, XMLEvent.START_DOCUMENT, null);

			afterStartDocument();
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		}
	}

	/// <summary>
/// Close the current opened stream.
/// </summary>
	protected void closeStream(){
		try {
			beforeEndDocument();
			reader.Close();
		} catch (Exception e) {
			throw new System.IO.IOException(e);
		} finally {
			reader = null;
		}
	}

	/// <summary>
/// Convert an attribute to a valid constant name.
/// </summary>
/// <param name="a"></param>
/// <returns></returns>
	protected string toConstantName(Attribute a) {
		return toConstantName(a.Name.getLocalPart());
	}

	/// <summary>
/// Convert a string to a valid constant name. String is put to upper case and all non-word characters are replaced by '_'.
/// </summary>
/// <param name="value"></param>
/// <returns></returns>
	protected string toConstantName(string value) {
		return value.ToUpper().Replace("\\W", "_");
	}

	/// <summary>
/// Base for parsers, providing some usefull features.
/// </summary>
	protected class Parser {
		/// <summary>
/// Read a sequence of characters and return these characters as a string. Characters are read until a non-character event is reached.
/// </summary>
/// <returns>a sequence of characters</returns>
		protected string __characters(){
			XMLEvent e;
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();

			e = getNextEvent();

			while (e.getEventType() == XMLEvent.CHARACTERS) {
				buffer.Append(e.asCharacters().getData());
				e = getNextEvent();
			}

			pushback(e);

			return buffer.ToString();
		}

		/// <summary>
/// Get attributes of a start element in a map. Attributes should be described in an enumeration such that {@link FileSourceXML#toConstantName(Attribute)} correspond to names of enumeration constants. type of the enumeration describing attributes
/// </summary>
/// <param name="cls"> class of the enumeration T</param>
/// <param name="e"> start event from which attributes have to be extracted</param>
/// <returns>a mapping between enum constants and attribute values.</returns>
		protected <T extends Enum<T>> EnumMap<T, string> getAttributes {
			EnumMap<T, string> values = new EnumMap<T, string>(cls);

			@SuppressWarnings
			IEnumerator<Attribute> attributes = e.asStartElement.getAttributes;

			while) {
				Attribute a = attributes.next;

				for.Length; i++) {
					if[i].name.equals)) {
						values.put[i], a.getValue);
						break;
					}
				}
			}

			return values;
		}

		/// <summary>
/// Check if all required attributes are present. type of the enumeration describing attributes if at least one required attribute is not found
/// </summary>
/// <param name="e"> the event</param>
/// <param name="attributes"> extracted attributes</param>
/// <param name="required"> array of required attributes</param>
		protected void checkRequiredAttributes<T>(XMLEvent e, EnumMap<T, string> attributes, where T : Enum<T>
				params T[] required){
			if (required != null) {
				for (int i = 0; i < required.Length; i++) {
					if (!attributes.ContainsKey(required[i]))
						newParseError(e, true, "'{0}' attribute is required for <{1}> element",
								required[i].ToString().ToLower(), e.asStartElement().Name.getLocalPart());
				}
			}
		}
	}
}

}
