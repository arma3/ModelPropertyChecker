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

    interface PropertyCondition
    {
        bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution);
    }

    class PropVerify_IsNotEmpty : PropertyCondition
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (string.IsNullOrWhiteSpace(property.Item2))
                throw new PropertyException("Property value is empty.");
            return true;
        }
    }


    class PropVerify_IsNumber : PropertyCondition
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            var isNumeric = double.TryParse(property.Item2, out double n);
            if (!isNumeric)
                throw new PropertyException("Property is not a number.");
            return isNumeric;
        }
    }

    class PropVerify_IsBoolean : PropertyCondition
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (property.Item2 == "0" || property.Item2 == "1") return true;

            throw new PropertyException("Property is not a boolean. Only 0/1 values are allowed");
            //#TODO suggest quick fix if it's yes/no or true/false
        }
    }

    class PropVerify_IsLodIndex : PropertyCondition
    {
        private float lodAdd = 0;

        //adds a number to the value found in the property
        PropVerify_IsLodIndex(float add = 0) => lodAdd = add;

        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            float.TryParse(property.Item2, out float n);

            float actualLod = n + lodAdd;

            if (model.lods.ContainsKey(actualLod)) return true;

            throw new PropertyException($"Property does not match a existing lod. Couldn't find lod {actualLod}");
        }
    }

    class PropVerify_IsOnGeoLod : PropertyCondition
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (sourceResolution == 1e13f) //#TODO if there is no geo lod, check that it's in the first lod.
                return true;
            else
                throw new PropertyException($"Property is not in Geometry LOD");
            //#TODO log name of which lod it was found on.
        }
    }

    class PropVerify_IsEnum : PropertyCondition
    {
        private HashSet<string> possibleValues;
        public PropVerify_IsEnum(HashSet<string> values) => possibleValues = values;

        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (possibleValues.Contains(property.Item2.ToLower()))
                return true;
            else
                throw new PropertyException($"Property does not match Enum. Value \"{property.Item2}\" is not valid");
        }
    }

    class PropertyVerifier
    {
        private static List<PropertyCondition> CreateBooleanCondition(bool forceGeometryLod = false)
        {
            return new List<PropertyCondition>
            {
                new PropVerify_IsNotEmpty(),
                new PropVerify_IsNumber(),
                new PropVerify_IsBoolean(),
                forceGeometryLod ? new PropVerify_IsOnGeoLod() : null
            };
        }





        public static Dictionary<string, List<PropertyCondition>> verifiers = new Dictionary<string, List<PropertyCondition>>
        {
            {
                "aicovers",
                CreateBooleanCondition(true)
            },
            {
                "autocenter",
                CreateBooleanCondition(true)
            },
            {
                "buoyancy",
                CreateBooleanCondition(true)
            },
            {
                "canbeoccluded",
                CreateBooleanCondition(true)
            },
            {
                "canocclude",
                CreateBooleanCondition(true)
            },
            {
                "class",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsEnum(
                        new HashSet<string> 
                        {
                            "treehard", //#TODO order alphabetically and make sure we got all of them
                            "treesoft",
                            "bushhard",
                            "bushsoft",
                            "forest",
                            "house",
                            "church",
                            "road",
                            "thing",
                            "land_decal",
                            "thingx",
                            "clutter",
                            "bridge",
                            "streetlamp",
                            "housesimulated",
                            "tower",
                            "vehicle",
                            "breakablehouseanimated",
                            "pond"
                        })
                }
            },
            {
                "damage",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsEnum(
                        new HashSet<string> 
                        {
                            "building", //#TODO order alphabetically and make sure we got all of them
                            "no",
                            "tent",
                            "tree",
                            "wall",
                            "wreck"
                        })
                }
            },
            {
                "dammage", //#TODO add a warning that this should be renamed to damage
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsEnum(
                        new HashSet<string> 
                        {
                            "building", //#TODO order in some way and make sure we got all of them
                            "no",
                            "tent",
                            "tree",
                            "wall",
                            "wreck"
                        })
                }
            },
            {
                "drawimportance",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsNumber()
                }
            },

        };


    }

    
















}
