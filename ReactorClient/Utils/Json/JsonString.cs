using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactorClient.Utils.Json
{
    class JsonString : JsonValue
    {

        private readonly string str;

        internal JsonString(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            this.str = str;
        }

        internal override void write(JsonWriter writer)
        {
            writer.writeString(str);
        }

        public override bool isString()
        {
            return true;
        }

        public override string asString()
        {
            return str;
        }

        public override int GetHashCode()
        {
            return str.GetHashCode();
        }

        public override bool Equals(Object obj)
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
            JsonString other = (JsonString)obj;
            return str == other.str;
        }
    }
}
