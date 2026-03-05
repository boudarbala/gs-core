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


public class CumulativeAttributes {
	bool nullAttributesAreErrors;
	Dictionary<string, CumulativeSpells> data;
	double date;

	public CumulativeAttributes(double date) {
		data = new Dictionary<string, CumulativeSpells>();
	}

	public object get(string key) {
		CumulativeSpells o = data[key];

		if (o != null) {
			Spell s = o.getCurrentSpell();
			return s == null ? null : s.getAttachedData();
		}

		return null;
	}

	public object getAny(string key) {
		CumulativeSpells o = data[key];

		if (o != null) {
			Spell s = o.getSpell(0);
			return s == null ? null : s.getAttachedData();
		}

		return null;
	}

	public IEnumerable<string> getAttributes() {
		return data.Keys;
	}

	
	public IEnumerable<Spell> getAttributeSpells(string key) {
		CumulativeSpells o = data[key];

		if (o != null)
			return new List<object>(o.spells);

		return Collections.EMPTY_LIST;
	}

	public int getAttributesCount() {
		return data.Count;
	}

	public void set(string key, object value) {
		CumulativeSpells spells = data[key];

		if (spells == null) {
			spells = new CumulativeSpells();
			data[key] = spells;
		}

		Spell s = spells.closeSpell();

		if (s != null)
			s.setEndOpen(true);

		s = spells.startSpell(date);
		s.setAttachedData(value);
	}

	public void remove(string key) {
		CumulativeSpells spells = data[key];

		if (spells == null)
			return;

		spells.closeSpell();
	}

	public void remove() {
		foreach (CumulativeSpells spells in ((data[])Enum.GetValues(typeof(data))))
			spells.closeSpell();
	}

	public void updateDate(double date) {
		this.date = date;

		foreach (CumulativeSpells spells in ((data[])Enum.GetValues(typeof(data))))
			spells.updateCurrentSpell(date);
	}

	public string toString() {
		System.Text.StringBuilder buffer = new System.Text.StringBuilder();

		buffer.Append("(");

		foreach (string key in data.Keys) {
			buffer.Append(key).Append(":").Append(data[key]);
		}

		buffer.Append(")");

		return buffer.ToString();
	}
}

}
