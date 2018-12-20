using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelPropertyChecker
{

    public class PropertyException : Exception
    {
        public PropertyException() {}
        public PropertyException(string message) : base(message) {}
        public PropertyException(string message, Exception inner) : base(message, inner){}
    }

    interface PropertyVerifier
    {
        bool verifyProperty(Model model, Tuple<string, string> property, float sourceResolution);
    }

    class PropVerify_IsNotEmpty : PropertyVerifier
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, float sourceResolution)
        {
            if (string.IsNullOrWhiteSpace(property.Item2))
                throw new PropertyException("Property value is empty.");
            return true;
        }
    }


    class PropVerify_IsNumber : PropertyVerifier
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, float sourceResolution)
        {
            var isNumeric = double.TryParse(property.Item2, out double n);
            if (!isNumeric)
                throw new PropertyException("Property is not a number.");
            return isNumeric;
        }
    }

    class PropVerify_IsBoolean : PropertyVerifier
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, float sourceResolution)
        {
            if (property.Item2 == "0" || property.Item2 == "1") return true;

            throw new PropertyException("Property is not a boolean. Only 0/1 values are allowed");
            //#TODO suggest quick fix if it's yes/no or true/false
        }
    }

    class PropVerify_IsLodIndex : PropertyVerifier
    {
        private float lodAdd = 0;

        //adds a number to the value found in the property
        PropVerify_IsLodIndex(float add = 0) => lodAdd = add;

        public bool verifyProperty(Model model, Tuple<string, string> property, float sourceResolution)
        {
            float.TryParse(property.Item2, out float n);

            float actualLod = n + lodAdd;

            if (model.lods.ContainsKey(actualLod)) return true;

            throw new PropertyException($"Property does not match a existing lod. Couldn't find lod {actualLod}");
        }
    }

    class PropVerify_IsEnum : PropertyVerifier
    {
        private HashSet<string> possibleValues;
        PropVerify_IsEnum(HashSet<string> values) => possibleValues = values;

        public bool verifyProperty(Model model, Tuple<string, string> property, float sourceResolution)
        {
            if (possibleValues.Contains(property.Item2))
                return true;
            else
                throw new PropertyException($"Property does not match Enum. Value \"{property.Item2}\" is not valid");
        }
    }


}
