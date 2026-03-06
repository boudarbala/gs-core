using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Stream.Rmi
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


public class RMISource : object /* RMIAdapterIn */, ISource {
	private static readonly long serialVersionUID = 6635146473737922832L;

	Queue<IAttributeSink> attributesListeners;
	Queue<IElementSink> elementsListeners;

	public RMISource(): base() {

		attributesListeners = new Queue<IAttributeSink>();
		elementsListeners = new Queue<IElementSink>();
	}

	public RMISource(string name): this() {
		bind(name);
	}

	public void bind(string name) {
		try {
			/* RMI Naming.rebind not available */, this);
		} catch (Exception e) {
			Console.Error.WriteLine(e);
		}
	}

	public void edgeAdded(string graphId, long timeId, string edgeId, string fromNodeId, string toNodeId,
			bool directed){
		foreach (IElementSink gel in elementsListeners)
			gel.edgeAdded(graphId, timeId, edgeId, fromNodeId, toNodeId, directed);
	}

	public void edgeAttributeAdded(string graphId, long timeId, string edgeId, string attribute, object value){
		foreach (IAttributeSink gal in attributesListeners)
			gal.edgeAttributeAdded(graphId, timeId, edgeId, attribute, value);
	}

	public void edgeAttributeChanged(string graphId, long timeId, string edgeId, string attribute, object oldValue,
			object newValue){
		foreach (IAttributeSink gal in attributesListeners)
			gal.edgeAttributeChanged(graphId, timeId, edgeId, attribute, oldValue, newValue);
	}

	public void edgeAttributeRemoved(string graphId, long timeId, string edgeId, string attribute){
		foreach (IAttributeSink gal in attributesListeners)
			gal.edgeAttributeRemoved(graphId, timeId, edgeId, attribute);
	}

	public void edgeRemoved(string graphId, long timeId, string edgeId){
		foreach (IElementSink gel in elementsListeners)
			gel.edgeRemoved(graphId, timeId, edgeId);
	}

	public void graphAttributeAdded(string graphId, long timeId, string attribute, object value){
		foreach (IAttributeSink gal in attributesListeners)
			gal.graphAttributeAdded(graphId, timeId, attribute, value);
	}

	public void graphAttributeChanged(string graphId, long timeId, string attribute, object oldValue, object newValue){
		foreach (IAttributeSink gal in attributesListeners)
			gal.graphAttributeChanged(graphId, timeId, attribute, oldValue, newValue);
	}

	public void graphAttributeRemoved(string graphId, long timeId, string attribute){
		foreach (IAttributeSink gal in attributesListeners)
			gal.graphAttributeRemoved(graphId, timeId, attribute);
	}

	public void graphCleared(string graphId, long timeId){
		foreach (IElementSink gel in elementsListeners)
			gel.graphCleared(graphId, timeId);
	}

	public void nodeAdded(string graphId, long timeId, string nodeId){
		foreach (IElementSink gel in elementsListeners)
			gel.nodeAdded(graphId, timeId, nodeId);
	}

	public void nodeAttributeAdded(string graphId, long timeId, string nodeId, string attribute, object value){
		foreach (IAttributeSink gal in attributesListeners)
			gal.nodeAttributeAdded(graphId, timeId, nodeId, attribute, value);
	}

	public void nodeAttributeChanged(string graphId, long timeId, string nodeId, string attribute, object oldValue,
			object newValue){
		foreach (IAttributeSink gal in attributesListeners)
			gal.nodeAttributeChanged(graphId, timeId, nodeId, attribute, oldValue, newValue);
	}

	public void nodeAttributeRemoved(string graphId, long timeId, string nodeId, string attribute){
		foreach (IAttributeSink gal in attributesListeners)
			gal.nodeAttributeRemoved(graphId, timeId, nodeId, attribute);
	}

	public void nodeRemoved(string graphId, long timeId, string nodeId){
		foreach (IElementSink gel in elementsListeners)
			gel.nodeRemoved(graphId, timeId, nodeId);
	}

	public void stepBegins(string graphId, long timeId, double step){
		foreach (IElementSink gel in elementsListeners)
			gel.stepBegins(graphId, timeId, step);
	}

	public void addAttributeSink(IAttributeSink listener) {
		attributesListeners.Add(listener);
	}

	public void addElementSink(IElementSink listener) {
		elementsListeners.Add(listener);
	}

	public void addSink(ISink listener) {
		attributesListeners.Add(listener);
		elementsListeners.Add(listener);
	}

	public void removeAttributeSink(IAttributeSink listener) {
		attributesListeners.Remove(listener);
	}

	public void removeElementSink(IElementSink listener) {
		elementsListeners.Remove(listener);
	}

	public void removeSink(ISink listener) {
		attributesListeners.Remove(listener);
		elementsListeners.Remove(listener);
	}

	public void clearAttributeSinks() {
		attributesListeners.Clear();
		elementsListeners.Clear();
	}

	public void clearElementSinks() {
		elementsListeners.Clear();
	}

	public void clearSinks() {
		attributesListeners.Clear();
	}
}

}
