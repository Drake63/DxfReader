using System;

namespace DxfOpener
{
    [Serializable]
    public struct ToolBarPosition
    {
        public int Band;
        public int BandIndex;

        public ToolBarPosition(int band, int bandIndex)
        {
            Band = band;
            BandIndex = bandIndex;
        }
    }
}
