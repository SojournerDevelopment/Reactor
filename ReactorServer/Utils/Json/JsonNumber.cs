using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactorServer.Utils.Json
{
    class JsonNumber : JsonValue
    {

        private readonly string str;

        internal JsonNumber(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            this.str = str;
        }

        public override string ToString()
        {
            return str;
        }

        internal override void write(JsonWriter writer)
        {
            writer.write(str);
        }

        public override bool isNumber()
        {
            return true;
        }

        public override int asInt()
        {
            return int.Parse(str);
        }

        public override long asLong()
        {
            return long.Parse(str);
        }

        public override float asFloat()
        {
            return float.Parse(str, CultureInfo.InvariantCulture);
        }

        public override double asDouble()
        {
            return double.Parse(str, CultureInfo.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return str.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            JsonNumber other = (JsonNumber)obj;
            return str == other.str;
        }
    }
}
