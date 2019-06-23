using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DxfOpener
{
    [Serializable]
    public class BarsPositions : ICollection<ToolBarPosition>
    {
        List<ToolBarPosition> positions;
        public int Count => positions.Count;

        public bool IsReadOnly => true;

        public BarsPositions()
        {
            positions = new List<ToolBarPosition>();
        }

        public ToolBarPosition this[int index]
        {
            get
            {
                if (index >= positions.Count)
                    throw new ArgumentOutOfRangeException("index");
                return positions[index];
            }
            set
            {
                if (index >= positions.Count)
                    throw new ArgumentOutOfRangeException("index");
                positions.RemoveAt(index);
                if (index == positions.Count)
                    positions.Add(value);
                else
                    positions.Insert(index, value);
            }
        }

        public void Add(ToolBarPosition item)
        {
            positions.Add(item);
        }

        public void Clear()
        {
            positions.Clear();
        }

        public bool Contains(ToolBarPosition item)
        {
            foreach (ToolBarPosition pos in positions)
                if (pos.Equals(item))
                    return true;
            return false;
        }

        public void CopyTo(ToolBarPosition[] array, int arrayIndex)
        {
            positions.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ToolBarPosition> GetEnumerator()
        {
            return new ToolbarPositionEnumerator(this);
        }

        public bool Remove(ToolBarPosition item)
        {
            return positions.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ToolbarPositionEnumerator(this);
        }
    }
}
