using System.Collections.Generic;
using System.Linq;
using System;

namespace Org.GraphStream.Util.Set
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
/// Array list with immutable element indices. <p> A fixed array list is like an array list, but it ensures the property that each element will always stay at the same index, even if elements are removed in between. The counterpart of this property is that the array handles by itself the insertion of new elements (since when an element is removed in the middle, this position can be reused), and therefore indices cannot be chosen (i.e. only the {@link #add(object)} and {@link #addAll(Collection)} methods are usable to insert new elements in the array). </p> <p> This is the reason why this does not implement the List interface, because the add(int,E) method cannot be implemented. </p> <p> Furthermore, this array cannot contain null values, because it marks unused positions within the array using the null value. </p>
/// </summary>
public class FixedArrayList<E> : ICollection<E> {
	// Attributes

	/// <summary>
/// List of elements.
/// </summary>
	protected List<E> elements = new List<E>();

	/// <summary>
/// List of free indices.
/// </summary>
	protected List<int> freeIndices = new List<int>();

	/// <summary>
/// Last inserted element index.
/// </summary>
	protected int lastIndex = -1;

	// Constructors

	public FixedArrayList() {
		elements = new List<E>();
		freeIndices = new List<int>(16);
	}

	public FixedArrayList(int capacity) {
		elements = new List<E>(capacity);
		freeIndices = new List<int>(16);
	}

	// Accessors

	/// <summary>
/// Number of elements in the array.
/// </summary>
/// <returns>The number of elements in the array.</returns>
	public int size() {
		return elements.Count - freeIndices.Count;
	}

	/// <summary>
/// Real size of the array, counting elements that have been erased.
/// </summary>
	public int realSize() {
		return elements.Count;
	}

	public bool isEmpty() {
		return (size() == 0);
	}

	/// <summary>
/// I-th element.
/// </summary>
/// <param name="i"> The element index.</param>
/// <returns>The element at index <code>i</code>.</returns>
	public E get(int i) {
		E e = elements[i];

		if (e == null)
			throw new InvalidOperationException("no element at index " + i);

		return e;
	}

	/// <summary>
/// I-th element. Like the {@link #get(int)} method but it does not check the element does not exists at the given index.
/// </summary>
/// <param name="i"> The element index.</param>
/// <returns>The element at index <code>i</code>.</returns>
	public E unsafeGet(int i) {
		return elements[i];
	}

	public bool contains(object o) {
		int n = elements.Count;

		for (int i = 0; i < n; ++i) {
			E e = elements[i];

			if (e != null) {
				if (e == o)
					return true;

				if (elements.Equals(o))
					return true;
			}
		}

		return false;
	}

	public bool containsAll(ICollection<object> c) {
		foreach (object o in c) {
			if (!contains(o))
				return false;
		}

		return true;
	}

	
	
	public bool equals(object o) {
		if (o is FixedArrayList) {
			FixedArrayList<E> other = (FixedArrayList<E>) o;

			int n = size();

			if (other.Count == n) {
				for (int i = 0; i < n; ++i) {
					E e0 = elements[i];
					E e1 = other.elements[i];

					if (e0 != e1) {
						if (e0 == null && e1 != null)
							return false;

						if (e0 != null && e1 == null)
							return false;

						if (!e0.Equals(e1))
							return false;
					}
				}

				return true;
			}
		}

		return false;
	}

	public java.util.IEnumerator<E> iterator() {
		return new FixedArrayIterator();
	}

	/// <summary>
/// Last index used by the {@link #add(object)} method.
/// </summary>
/// <returns>The last insertion index.</returns>
	public int getLastIndex() {
		return lastIndex;
	}

	/// <summary>
/// The index that will be used in case of a next insertion in this array.
/// </summary>
/// <returns></returns>
	public int getNextAddIndex() {
		int n = freeIndices.Count;

		if (n > 0)
			return freeIndices[n - 1];
		else
			return elements.Count;
	}

	public object[] toArray() {
		int n = size();
		int m = elements.Count;
		int j = 0;
		object[] a = new object[n];

		for (int i = 0; i < m; ++i) {
			E e = elements[i];

			if (e != null)
				a[j++] = e;
		}

		System.Diagnostics.Debug.Assert((j == n));
		return a;
	}

	public T[] toArray<T>(T[] a) {
		// TODO
		throw new Exception("not implemented yet");
	}

	// Commands

	/// <summary>
/// Add one <code>element</code> in the array. The index used for inserting the element is then available using {@link #getLastIndex()}. If a null value is inserted.
/// </summary>
/// <param name="element"> The element to add.</param>
/// <returns>Always true.</returns>
	public bool add(E element){
		if (element == null)
			throw new java.lang.NullReferenceException("this array cannot contain null value");

		int n = freeIndices.Count;

		if (n > 0) {
			int i = freeIndices.Remove(n - 1);
			elements.set(i, element);
			lastIndex = i;
		} else {
			elements.Add(element);
			lastIndex = elements.Count - 1;
		}

		return true;
	}

	public bool addAll(ICollection<E> c){
		java.util.IEnumerator<E> k = c.GetEnumerator();

		while (k.MoveNext()) {
			add(k.next());
		}

		return true;
	}

	/// <summary>
/// Remove the element at index <code>i</code>.
/// </summary>
/// <param name="i"> Index of the element to remove.</param>
/// <returns>The removed element.</returns>
	public E remove(int i) {
		int n = elements.Count;

		if (i < 0 || i >= n)
			throw new IndexOutOfRangeException("index " + i + " does not exist");

		if (n > 0) {
			if (elements[i] == null)
				throw new NullReferenceException("no element stored at index " + i);

			if (i == (n - 1)) {
				return elements.Remove(i);
			} else {
				E e = elements[i];
				elements.set(i, null);
				freeIndices.Add(i);
				return e;
			}
		}

		throw new IndexOutOfRangeException("index " + i + " does not exist");
	}

	protected void removeIt(int i) {
		remove(i);
	}

	/// <summary>
/// Remove the element <code>e</code>.
/// </summary>
/// <param name="e"> The element to remove.</param>
/// <returns>True if removed.</returns>
	public bool remove(object e) {
		int n = elements.Count;

		for (int i = 0; i < n; ++i) {
			if (elements[i] == e) {
				elements.Remove(i);
				return true;
			}
		}

		return false;
	}

	public bool removeAll(ICollection<object> c) {
		throw new NotSupportedException("not implemented yet");
	}

	public bool retainAll(ICollection<object> c) {
		throw new NotSupportedException("not implemented yet");
	}

	public void clear() {
		elements.Clear();
		freeIndices.Clear();
	}

	// Nested classes

	protected class FixedArrayIterator : java.util.IEnumerator<E> {
		int i;

		public FixedArrayIterator() {
			i = -1;
		}

		public bool hasNext() {
			int n = elements.Count;

			for (int j = i + 1; j < n; ++j) {
				if (elements[j] != null)
					return true;
			}

			return false;
		}

		public E next() {
			int n = elements.Count;

			for (int j = i + 1; j < n; ++j) {
				E e = elements[j];

				if (e != null) {
					i = j;
					return e;
				}
			}

			throw new InvalidOperationException("no more elements in iterator");
		}

		public void remove(){
			// throw new UnsupportedOperationException( "not implemented yet" );

			if (i >= 0 && i < elements.Count && elements[i] != null) {
				removeIt(i); // A parent class method cannot be called if it has
								// the same name as one in the inner class
								// (normal), but even if they have distinct
								// arguments types. Hence this strange removeIt()
								// method...
			} else {
				throw new InvalidOperationException("no such element");
			}

		}
	}

}

}
