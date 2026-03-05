using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Org.GraphStream.Util.Cumulative
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


public class CumulativeSpells {
	class Spell {
		private double start;
		private double end;

		private bool startOpen;
		private bool endOpen;

		private bool closed;

		private object data;

		public Spell(double start, bool startOpen, double end, bool endOpen) {
			this.start = start;
			this.startOpen = startOpen;
			this.end = end;
			this.endOpen = endOpen;

			this.closed = false;
		}

		public Spell(double start, double end) : this(start, false, end, false) {
		}

		public Spell(double start) : this(start, false, start, false) {
		}

		public double getStartDate() {
			return start;
		}

		public double getEndDate() {
			return end;
		}

		public bool isStartOpen() {
			return startOpen;
		}

		public bool isEndOpen() {
			return endOpen;
		}

		public bool isStarted() {
			return !double.IsNaN(start);
		}

		public bool isEnded() {
			return closed;
		}

		public void setStartOpen(bool open) {
			startOpen = open;
		}

		public void setEndOpen(bool open) {
			endOpen = open;
		}

		public object getAttachedData() {
			return data;
		}

		public void setAttachedData(object data) {
			this.data = data;
		}

		public string toString() {
			string str = "";

			if (isStarted()) {
				str += isStartOpen() ? "]" : "[";
				str += start + "; ";
			} else
				str += "[...; ";

			if (isEnded()) {
				str += end;
				str += isEndOpen() ? "[" : "]";
			} else
				str += "...]";

			return str;
		}
	}

	List<Spell> spells;
	double currentDate;

	public CumulativeSpells() {
		this.spells = new List<Spell>();
		currentDate = double.NaN;
	}

	public Spell startSpell(double date) {
		Spell s = new Spell(date);
		spells.Add(s);

		return s;
	}

	public void updateCurrentSpell(double date) {
		if (spells.Count > 0 && !double.IsNaN(currentDate)) {
			Spell s = spells.getLast();

			if (!s.closed)
				s.end = currentDate;
		}

		currentDate = date;
	}

	public Spell closeSpell() {
		if (spells.Count > 0) {
			Spell s = spells.getLast();

			if (!s.closed) {
				s.closed = true;
				return s;
			}
		}

		return null;
	}

	public Spell getCurrentSpell() {
		Spell s = spells.getLast();

		if (s == null)
			return null;

		return s.closed ? null : s;
	}

	public Spell getSpell(int i) {
		return spells[i];
	}

	public int getSpellCount() {
		return spells.Count;
	}

	public Spell getOrCreateSpell(double date) {
		Spell s = getCurrentSpell();

		if (s == null)
			s = startSpell(date);

		return s;
	}

	public bool isEternal() {
		return spells.Count == 1 && !spells[0].isStarted() && !spells[0].isEnded();
	}

	public string toString() {
		System.Text.System.Text.StringBuilder buffer = new System.Text.System.Text.StringBuilder();

		buffer.Append("{");

		for (int i = 0; i < spells.Count; i++) {
			if (i > 0)
				buffer.Append(", ");

			buffer.Append(spells[i].ToString());
		}

		buffer.Append("}");

		return buffer.ToString();
	}
}

}
