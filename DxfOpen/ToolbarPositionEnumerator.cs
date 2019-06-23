using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DxfOpener
{
    class ToolbarPositionEnumerator : IEnumerator<ToolBarPosition>
    {
        BarsPositions collection;
        int curIndex;
        ToolBarPosition curPos;

        public ToolbarPositionEnumerator(BarsPositions collection)
        {
            this.collection = collection;
            curIndex = -1;
            curPos = default(ToolBarPosition);
        }
        public ToolBarPosition Current => curPos;

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (++curIndex >= collection.Count)
                return false;
            curPos = collection[curIndex];
            return true;
        }

        public void Reset()
        {
            curIndex = -1;
        }
    }
}
