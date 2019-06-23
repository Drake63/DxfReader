using System;
using System.IO;

namespace DxfReader
{
    class DxfCodePair
    {
        public int Code { get; private set; }
        public string Value { get; private set; }

        public DxfCodePair(TextReader reader)
        {
            if (!int.TryParse(reader.ReadLine(), out int code))
                throw new ArgumentException(Properties.Resource.NoInt32);
            Code = code;
            Value = reader.ReadLine();
        }

        public DxfCodePair(string code, string value)
        {
            if(!int.TryParse(code, out int val))
                throw new ArgumentException(Properties.Resource.NoInt32);
            Code = val;
            Value = value;
        }

        public int AsInt
        {
            get
            {
                if (!int.TryParse(Value, out int result))
                    throw new ArgumentException(Properties.Resource.NoInt32);
                return result;
            }
        }
        public double AsDouble
        {
            get
            {
                return DxfHelper.StringToDouble(Value);
            }
        }
        public byte AsByte
        {
            get
            {
                if (!byte.TryParse(Value, out byte result))
                    throw new ArgumentException(Properties.Resource.NoByte);
                return result;
            }
        }
        public short AsShort
        {
            get
            {
                if (!short.TryParse(Value, out short result))
                    throw new ArgumentException(Properties.Resource.NoShort);
                return result;
            }
        }

        public ushort AsUshort
        {
            get
            {
                if (!ushort.TryParse(Value, out ushort result))
                    throw new ArgumentException(Properties.Resource.NoUshort);
                return result;
            }
        }
    }
}
