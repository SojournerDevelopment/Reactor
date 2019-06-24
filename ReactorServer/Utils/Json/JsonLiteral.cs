﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactorServer.Utils.Json
{
    class JsonLiteral : JsonValue
    {

        private readonly string value;

        internal JsonLiteral(string value)
        {
            this.value = value;
        }

        internal override void write(JsonWriter writer)
        {
            writer.write(value);
        }

        public override string ToString()
        {
            return value;
        }

        public override bool asBool()
        {
            return isBool() ? isTrue() : base.asBool();
        }

        public override bool isNull()
        {
            return this == NULL;
        }

        public override bool isBool()
        {
            return this == TRUE || this == FALSE;
        }

        public override bool isTrue()
        {
            return this == TRUE;
        }

        public override bool isFalse()
        {
            return this == FALSE;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
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
            JsonLiteral other = (JsonLiteral)obj;
            return value == other.value;
        }
    }
}
