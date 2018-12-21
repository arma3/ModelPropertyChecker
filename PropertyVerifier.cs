using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelPropertyChecker
{

    public class PropertyException : Exception
    {
        public bool isError { get; } = true; //Can also be warning
        public string propertyName { get; }
        public PropertyException() {}

        public PropertyException(string propertyName, string message) : base(message)
        {
            this.propertyName = propertyName;
        }

        public PropertyException(string message, Exception inner) : base(message, inner){}
        
        public PropertyException(string propertyName, string message, bool isError) : base(message)
        {
            this.propertyName = propertyName;
            this.isError = isError;
        }

        public override string ToString()
        {
            return Message;
        }

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
                throw new PropertyException(property.Item1,"Property value is empty.");
            return true;
        }
    }

    class PropVerify_IsNumber : PropertyCondition
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            var isNumeric = double.TryParse(property.Item2, out double n);
            if (!isNumeric)
                throw new PropertyException(property.Item1,"Property is not a number.");
            return true;
        }
    }

    class PropVerify_IsBoolean : PropertyCondition
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (property.Item2 == "0" || property.Item2 == "1") return true;

            var isNumeric = float.TryParse(property.Item2, out float n);
            if (!isNumeric)
                throw new PropertyException(property.Item1,"Property is not a boolean. Only 0/1 values are allowed");

            //It's still a number, but outside of the valid range
            throw new PropertyException(property.Item1,"Property is outside of range for boolean. Only 0/1 values are allowed", false);

            //#TODO suggest quick fix if it's yes/no or true/false
        }
    }

    class PropVerify_IsLodIndex : PropertyCondition
    {
        private readonly float lodAdd;
        private readonly bool needAdd;

        //adds a number to the value found in the property
        public PropVerify_IsLodIndex(float add = 0, bool optionalAdd = false)
        {
            lodAdd = add;
            needAdd = !optionalAdd;
        }

        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            float.TryParse(property.Item2, out float n);

            float actualLod = n + lodAdd;
            if (model.lods.ContainsKey(actualLod)) return true;
            if (!needAdd)
            {

                if (!needAdd) //Check if value is already lod resolution
                {
                    if (model.lods.ContainsKey(n)) return true;
                }

                throw new PropertyException(property.Item1, $"Property does not match a existing lod. Couldn't find lod {actualLod} or {n + lodAdd}");
            }

            throw new PropertyException(property.Item1, $"Property does not match a existing lod. Couldn't find lod {actualLod}");
        }
    }

    class PropVerify_IsOnGeoLod : PropertyCondition
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (sourceResolution == 1e13f) //#TODO if there is no geo lod, check that it's in the first lod.
                return true;
            else
            {
                if (
                    !model.lods.ContainsKey(1e13f) //No geo lod. Check if property is in first lod.
                    && model.lods.First().Key != sourceResolution //Not on first lod! Error.
                )
                    throw new PropertyException(property.Item1,"Property is not in Geometry LOD");
                return true; //No geo lod, but property is on first lod. All fine.
            }
        }
    }

    class PropVerify_IsOnVisLod : PropertyCondition
    {
        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if ((float)sourceResolution < 901)
                return true;
            else
                throw new PropertyException(property.Item1, "Property is not in Resolution LOD");
            //#TODO log name of which lod it was found on.
        }
    }

    class PropVerify_IsEnum : PropertyCondition
    {
        private readonly HashSet<string> possibleValues;
        public PropVerify_IsEnum(HashSet<string> values) => possibleValues = values;

        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (possibleValues.Contains(property.Item2.ToLower()))
                return true;
            else
                throw new PropertyException(property.Item1,$"Property does not match Enum. Value \"{property.Item2}\" is not valid");
        }
    }

    class PropVerify_ExpectPropertyExists : PropertyCondition
    {
        private readonly string propertyName;
        public PropVerify_ExpectPropertyExists(string name) => propertyName = name;

        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (model.lods[sourceResolution].properties.ContainsKey(propertyName))
                return true;
            else
                throw new PropertyException(property.Item1,$"Property is only valid if property \"{propertyName}\" exists");
        }
    }

    class PropVerify_ExpectPropertyNotExist : PropertyCondition
    {
        private readonly string propertyName;
        public PropVerify_ExpectPropertyNotExist(string name) => propertyName = name;

        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (model.lods[sourceResolution].properties.ContainsKey(propertyName))
                throw new PropertyException(property.Item1,$"Property is only valid if property \"{propertyName}\" doesn't exist");
            else
                return true;
        }
    }

    class PropVerify_ExpectPropertyValue : PropertyCondition
    {
        private readonly string propertyName;
        private readonly string propertyValue;

        public PropVerify_ExpectPropertyValue(string name, string value)
        {
            propertyName = name; 
            propertyValue = value;
        }

        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (model.lods[sourceResolution].properties.ContainsKey(propertyName))
            {
                var value = model.lods[sourceResolution].properties[propertyName];
                throw new PropertyException(property.Item1,$"Property is only valid if property \"{propertyName}\" is set to value \"{propertyValue}\". Current value is \"{value}\"");
                //#TODO suggest quick fix
            }

            throw new PropertyException(property.Item1,$"Property is only valid if property \"{propertyName}\" exists");
        }
    }

    class PropVerify_ObsoleteValue : PropertyCondition
    {
        private readonly string propertyValue; //#TODO add a "cause"

        public PropVerify_ObsoleteValue(string value) => propertyValue = value;

        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            if (property.Item2.Equals(propertyValue, StringComparison.CurrentCultureIgnoreCase))
                throw new PropertyException(property.Item1,$"Property value \"{property.Item2}\" is obsolete");
            return true;
        }
    }

    class PropVerify_LogicOr : PropertyCondition
    {
        private readonly PropertyCondition first;
        private readonly PropertyCondition second;

        public PropVerify_LogicOr(PropertyCondition item1, PropertyCondition item2)
        {
            first = item1;
            second = item2;
        }


        public bool verifyProperty(Model model, Tuple<string, string> property, LODResolution sourceResolution)
        {
            try
            {
                var result = first.verifyProperty(model, property, sourceResolution);
                if (result) return true;
            } catch (PropertyException exception)
            {
                try
                {
                    var result = second.verifyProperty(model, property, sourceResolution);
                    if (result) return true;
                } catch (PropertyException exception2)
                {
                    throw new PropertyException(property.Item1,$"Or Condition failed. Either: {exception.Message} or {exception2.Message}");
                }
            }

            return false;
        }
    }





    static class PropertyVerifier
    {
        private static List<PropertyCondition> CreateBooleanCondition(bool forceGeometryLod = false)
        {
            return new List<PropertyCondition>
            {
                new PropVerify_IsNotEmpty(),
                new PropVerify_IsBoolean(),
                forceGeometryLod ? new PropVerify_IsOnGeoLod() : null
            };
        }

        private static readonly Dictionary<string, List<PropertyCondition>> verifiers = new Dictionary<string, List<PropertyCondition>>
        {
            {
                "aicovers",
                CreateBooleanCondition(true)
            },
            {
                "armor",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsNumber()
                }
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
                            "pond",
                            "man"
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
            {
                "explosionshielding",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsNumber()
                }
            },
            {
                "forcenotalpha",
                CreateBooleanCondition(true)
            },
            {
                "frequent",
                CreateBooleanCondition(true)
            },
            {
                "keyframe",
                CreateBooleanCondition(true)
            },
            {
                "viewdensitycoef",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(), //#TODO make a CreateNumberCondition utility function for these
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsNumber()
                }
            },
            {
                "loddensitycoef",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(), //#TODO make a CreateNumberCondition utility function for these
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsNumber()
                }
            },
            {
                "lodnoshadow",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsBoolean(),
                    new PropVerify_IsOnVisLod()
                }
            },
            {
                "map",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsEnum(
                        new HashSet<string> 
                        {



                            "building", //#TODO order in some way and make sure we got all of them
                            "bunker",
                            "bush",
                            "busstop",	
                            "chapel",	
                            "church",	
                            "cross",	
                            "fence",	
                            "fortress",	
                            "fountain",	
                            "fuelstation",	
                            "hide",	
                            "hospital",	
                            "house",	
                            "lighthouse",	
                            "main road",
                            "power lines",
                            "powersolar",
                            "powerwave",
                            "powerwind",	
                            "quay",	
                            "railway",	
                            "road",	
                            "rock",	
                            "ruin",	
                            "shipwreck",	
                            "small tree",
                            "stack",	
                            "tourism",	
                            "track",	
                            "transmitter",	
                            "tree",	
                            "view-tower",	
                            "wall",	
                            "watertower",
                        })
                }
            },
            {
                "mass",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(), //#TODO make a CreateNumberCondition utility function for these
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsNumber()
                }
            },
            {
                "placement",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsEnum(
                        new HashSet<string> 
                        {
                            "slope",
                            "slopex",
                            "slopez",
                            "slopelandcontact"
                        })
                }
            },
            {
                "prefershadowvolume",
                CreateBooleanCondition(true)
            },
            {
                "sbsource",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsEnum(
                        new HashSet<string> 
                        {
                            "explicit",
                            "none",
                            "shadow",
                            "shadowvolume",
                            "visual",
                            "visualex"
                        })
                }
            },
            {
                "shadow",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_ExpectPropertyNotExist("sbsource"),
                    new PropVerify_IsEnum(
                        new HashSet<string> 
                        {
                            "hybrid"
                        })
                }
            },
            {
                "shadowlod",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_ObsoleteValue("-1"), //-1 is already default if undefined
                    new PropVerify_LogicOr(
                        new PropVerify_IsLodIndex(11000, true),
                        new PropVerify_IsLodIndex(10000, true) 
                        //This is fallback in case the others are not defined, so this can be either.
                    )
                }
            },
            {
                "shadowvolumelod",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_ObsoleteValue("-1"), //-1 is already default if undefined
                    new PropVerify_IsLodIndex(10000, true)
                }
            },

            {
                "shadowbufferlod",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_ObsoleteValue("-1"), //-1 is already default if undefined
                    new PropVerify_LogicOr(
                        new PropVerify_IsLodIndex(11000, true),
                        new PropVerify_IsLodIndex(10000, true) 
                        //There is a bug in OB where shadowBuffer 0 would be displayed as
                        // "shadowVolume 1000"
                    )
                }
            },
            {
                "shadowbufferlodvis",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(),
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_ObsoleteValue("-1"), //-1 is already default if undefined
                    new PropVerify_LogicOr(
                        new PropVerify_IsLodIndex(11000, true),
                        new PropVerify_IsLodIndex(10000, true) 
                        //There is a bug in OB where shadowBuffer 0 would be displayed as
                        // "shadowVolume 1000"
                    )
                    //#TODO verify hat the lod index is < than minShadow. Only vis
                }
            },
            {
                "shadowoffset",
                new List<PropertyCondition>
                {
                    new PropVerify_IsNotEmpty(), //#TODO make a CreateNumberCondition utility function for these
                    new PropVerify_IsOnGeoLod(),
                    new PropVerify_IsNumber()
                    //#TODO I think we should set reasonable limits here. Only small values really make sense
                }
            }
        };

        public static void verifyModel(ref Model model)
        {
            foreach (var lod in model.lods)
            {
                foreach (var property in lod.Value.properties)
                {
                    if (!verifiers.ContainsKey(property.Key))
                    {
                        lod.Value.propertyExceptions.Add(new PropertyException(property.Key, "Unknown Property", false));
                        continue;
                    }
                        
                    //#TODO detect potential typo's here by checking how big the difference to the known values is

                    var tuple = new Tuple<string, string>(property.Key, property.Value.value);

                    foreach (var condition in verifiers[property.Key])
                    {
                        try
                        {
                            condition.verifyProperty(model, tuple, lod.Key);
                        } catch (PropertyException exception) { lod.Value.propertyExceptions.Add(exception); }
                    }
                }


            }
        }





    }

    
















}
