using BIS.Core.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace ModelPropertyChecker
{
    public struct LODResolution : IComparable, IComparable<float>, IEquatable<float>, IFormattable
    {
        private readonly float _value;

        public LODResolution(float value)
        {
            _value = value;
        }

        public static implicit operator LODResolution(float value)
        {
            return new LODResolution(value);
        }

        public static explicit operator float(LODResolution value)
        {
            return value._value;
        }


        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case LODResolution otherRes:
                    return Equals(otherRes)
                        ? 0 //Equals
                        : _value < otherRes._value
                            ? -1 //Smaller than
                            : 1; //Bigger than
                case float otherResFloat:
                    return Equals(otherResFloat)
                        ? 0 //Equals
                        : _value < otherResFloat
                            ? -1 //Smaller than
                            : 1; //Bigger than
                default:
                    throw new ArgumentException("Object is not a Resolution");
            }
        }

        public bool Equals(float other)
        {
            return Math.Abs(_value - other) <= other * 1e-3f;
        }

        public bool Equals(LODResolution other)
        {
            return Equals(other._value);
        }

        public int CompareTo(float other)
        {
            return Equals(other)
                ? 0 //Equals
                : _value < other
                    ? -1 //Smaller than
                    : 1; //Bigger than
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            return 
                obj is LODResolution other && Equals(other)
                ||
                obj is float otherFl && Equals(otherFl)
                ||
                obj is double otherDbl && Equals((float) otherDbl)
                ||
                obj is int otherInt && Equals(otherInt)
                ;
        }

        public static bool operator ==(LODResolution res1, LODResolution res2)
        {
            return res1.Equals(res2);
        }

        public static bool operator !=(LODResolution res1, LODResolution res2)
        {
            return !(res1 == res2);
        }

        public static bool operator ==(LODResolution res1, float res2)
        {
            return res1.Equals(res2);
        }

        public static bool operator !=(LODResolution res1, float res2)
        {
            return !(res1 == res2);
        }


        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (Equals(1e13)) return "geometry";
            if (Equals(2e13)) return "geometrySimple";
            if (Equals(3e13)) return "geometryPhysOld";
            if (Equals(4e13)) return "geometryPhys";
            if (Equals(1e15)) return "memory";
            if (Equals(2e15)) return "landContact";
            if (Equals(3e15)) return "roadway";
            if (Equals(4e15)) return "paths";
            if (Equals(5e15)) return "hitpoints";
            if (Equals(6e15)) return "geometryView";
            if (Equals(7e15)) return "geometryFire";
            if (Equals(8e15)) return "geometryViewCargo";
            if (Equals(13e15)) return "geometryViewPilot";
            if (Equals(15e15)) return "geometryViewGunner";
            if (Equals(21e15)) return "wreck";
            if (Equals(1000)) return "viewGunner";
            if (Equals(1100)) return "viewPilot";
            if (Equals(1200)) return "viewCargo";


            if (Equals(10000) && _value < 11000)
                return $"ShadowVolume {_value - 10000}";
            if (Equals(11000) && _value < 12000)
                return $"ShadowBuffer {_value - 11000}";

            return $"{_value}";
        }
    }

    public struct Property
    {
        public string value { get; }
        public readonly long valuePos;

        public Property(string value, long valuePos)
        {
            this.value = value;
            this.valuePos = valuePos;
        }
    }



    public class LOD
    {
        public Dictionary<string, Property> properties { get; } = new Dictionary<string, Property>();
        public LODResolution resolution { get; set; } = 0;
        public List<PropertyException> propertyExceptions { get; set; } = new List<PropertyException>();//Set by PropertyVerifier

        public bool hasErrors
        {
            get
            {
                if (propertyExceptions != null)
                    foreach (var exception in propertyExceptions)
                    {
                        if (exception.isError) return true;
                    }

                return false;
            }
        }

        public bool hasWarnings
        {
            get
            {
                if (propertyExceptions != null)
                    foreach (var exception in propertyExceptions)
                    {
                        if (!exception.isError) return true;
                    }

                return false;
            }
        }



        public void loadFromODOL(BinaryReaderEx reader)
        {
            var numProxies = reader.ReadUInt32();
            for (int i = 0; i < numProxies; i++)
            {
                while (reader.ReadByte() != 0) ; //skip name
                reader.BaseStream.Seek(
                    4 * 3 * 4 //Transform xyzn 4 vectors of 3 floats each
                    + 4
                    + 4
                    + 4
                    + 4, SeekOrigin.Current);
            }

            var numBonesSub = reader.ReadUInt32();
            reader.BaseStream.Seek(numBonesSub * 4, SeekOrigin.Current);


            var numBones = reader.ReadUInt32();
            for (int i = 0; i < numBones; i++)
            {
                var numLinks = reader.ReadUInt32();
                reader.BaseStream.Seek(numLinks * 4, SeekOrigin.Current);
            }


            reader.BaseStream.Seek(4+4+4+4 +4*3 +4*3 +4*3 +4, SeekOrigin.Current);
            var numTextures = reader.ReadUInt32();

            for (int i = 0; i < numTextures; i++)
            {
                var texName = reader.ReadAsciiz();
            }

            var numMaterials = reader.ReadUInt32();

            for (int i = 0; i < numMaterials; i++)
            {
                var name1 = reader.ReadAsciiz();

                reader.BaseStream.Seek(4 + 4 * 4 * 6 + 4 + 8 + 8, SeekOrigin.Current);
                var name2 = reader.ReadAsciiz();
                reader.BaseStream.Seek(8, SeekOrigin.Current);

                var numTex = reader.ReadUInt32();
                var numTrans = reader.ReadUInt32();
                for (int i2 = 0; i2 < numTex; i2++)
                {
                    reader.ReadUInt32();
                    var name3 = reader.ReadAsciiz();
                    reader.ReadUInt32();
                    reader.ReadByte();
                }

                reader.BaseStream.Seek((4 + 4 * 12) * numTrans, SeekOrigin.Current);

                reader.ReadUInt32();//#TODO skip TI stage if version <11
                var nameTI = reader.ReadAsciiz();
                reader.ReadUInt32();
                reader.ReadByte();
            }

            var t1 = reader.ReadCompressedIntArray();
            var t2 = reader.ReadCompressedIntArray();



            //reader.BaseStream.Seek(8, SeekOrigin.Current); //ptv vtp

            var numFaces2 = reader.ReadUInt32();
            var faceSize = reader.ReadUInt32(); //This doesn't match.. Dunno why
            reader.BaseStream.Seek(2, SeekOrigin.Current);
            //reader.BaseStream.Seek(faceSize, SeekOrigin.Current);

            //int sz = 0;

            for (int i = 0; i < numFaces2; i++)
            {
                var numVerts = reader.ReadByte();
                //sz += 1;
                //sz += numVerts * 4;
                reader.BaseStream.Seek(numVerts * 4, SeekOrigin.Current);
            }


            var numSections = reader.ReadUInt32();
            for (int i = 0; i < numSections; i++)
            {
                reader.BaseStream.Seek(16 + 4 + 2 + 4, SeekOrigin.Current);
                var matIndex = reader.ReadInt32();
                if (matIndex == -1)
                {
                    var matname = reader.ReadAsciiz();
                }

                reader.BaseStream.Seek(4 + 8 + 4, SeekOrigin.Current);
            }

            var numSelections = reader.ReadUInt32();
            for (int i = 0; i < numSelections; i++)
            {
                string selectionName = reader.ReadAsciiz();

                int nElements = reader.ReadInt32();
                if (nElements != 0)
                {
                    var b = reader.ReadByte();//#TODO do this properly. Search in binary for `"Error decompressing block %d..%d of %s"`
                    var expectedDataSize = (uint)(nElements * 4);
                    var stream = reader.ReadCompressed(expectedDataSize, b == 2);
                }

                var always0 = reader.ReadUInt32();
                if (always0 != 0)
                    throw new ArgumentOutOfRangeException();


                var isSectional = reader.ReadByte();

                nElements = reader.ReadInt32(); //sections
                if (nElements != 0)
                {
                    var b = reader.ReadByte();
                    var expectedDataSize = (uint) (nElements * 4);
                    var stream = reader.ReadCompressed(expectedDataSize, b==2);
                }

                nElements = reader.ReadInt32(); //vertices
                if (nElements != 0)
                {
                    var b = reader.ReadByte();
                    var expectedDataSize = (uint) (nElements * 4);
                    var stream = reader.ReadCompressed(expectedDataSize, b == 2);
                }


                nElements = reader.ReadInt32();//weights
                if (nElements != 0)
                {
                    var b = reader.ReadByte();
                    var expectedDataSize = (uint) nElements;
                    var stream = reader.ReadCompressed(expectedDataSize, b == 2);
                }
               
            }


            var numProperties = reader.ReadUInt32();
            for (int i = 0; i < numProperties; i++)
            {
                string key = reader.ReadAsciiz();
                var valuePos = reader.Position;
                string value = reader.ReadAsciiz();

                if (properties.ContainsKey(key.ToLower()))
                    propertyExceptions.Add(new PropertyException(key, "duplicate property " + value));
                else
                    properties.Add(key.ToLower(), new Property(value, valuePos)); //#TODO maybe we also want to keep a version with original casing?
            }

            var numFrames = reader.ReadUInt32();
            if (numFrames != 0)
            {
                throw new NotImplementedException("Animation frames not implemented");
            }

            reader.BaseStream.Seek(13, SeekOrigin.Current);
            var stuff = reader.ReadUInt32();
            reader.BaseStream.Seek(stuff, SeekOrigin.Current);

            /*

            var numPoints = reader.ReadUInt32();
            reader.ReadByte();
            if (numPoints != 0)
            {
                throw new NotImplementedException();
            }


            reader.BaseStream.Seek(16, SeekOrigin.Current); //uv limits


            var numUVs = reader.ReadUInt32();
            reader.ReadByte();
            if (numUVs != 0)
            {
                reader.ReadByte();
                reader.BaseStream.Seek(4*numUVs, SeekOrigin.Current); //uv limits
            }

            var secondUV = reader.ReadUInt32();

            if (secondUV != 0)
            {
                throw new NotImplementedException();
            }

            var numPoints2 = reader.ReadUInt32();
            if (numPoints2 != 0)
                reader.ReadByte();
            for (int i = 0; i < numPoints2; i++)
            {
                reader.BaseStream.Seek((3*4 + 1)*numPoints2, SeekOrigin.Current);
            }

            var numNormals = reader.ReadUInt32();
            reader.ReadByte();
            if (numNormals != 0)
                reader.ReadByte();
            for (int i = 0; i < numNormals; i++)
            {
                reader.BaseStream.Seek(4 * numNormals, SeekOrigin.Current);
            }


            reader.BaseStream.Seek(4, SeekOrigin.Current);
            var numVertBone = reader.ReadUInt32();
            if (numVertBone != 0)
                reader.ReadByte();

            for (int i = 0; i < numVertBone; i++)
            {
                reader.BaseStream.Seek(4 +(4*2), SeekOrigin.Current);
            }*/


            reader.BaseStream.Seek(4 + 4 + 1, SeekOrigin.Current);

            //End of lod

        }

        public float loadFromMLOD(BinaryReaderEx reader)
        {
            var head = reader.ReadAscii(4); //P3DM header


            var appid = reader.ReadInt32();
            var vers = reader.ReadInt32(); //version

            var numPoints = reader.ReadUInt32();
            var numNormals = reader.ReadUInt32();
            var numFaces = reader.ReadUInt32();
            reader.ReadUInt32();
            reader.BaseStream.Seek(numPoints * 16, SeekOrigin.Current);
            reader.BaseStream.Seek(numNormals * 4 * 3, SeekOrigin.Current);

            for (int i = 0; i < numFaces; i++)
            {
                reader.BaseStream.Seek(72, SeekOrigin.Current);
                while (reader.ReadByte() != 0) ; //texture
                while (reader.ReadByte() != 0) ; //material
            }

            string tagTex = reader.ReadAscii(4);
            if (tagTex != "TAGG")
                throw new NotImplementedException(); //#TODO
            string tagName;
            do
            {
                reader.ReadByte();

                tagName = reader.ReadAsciiz();
                var tagLen = reader.ReadUInt32();

                if (tagName == "#Property#")
                {
                    string key = reader.ReadAscii(64);
                    key = key.Substring(0, key.IndexOf('\0'));
                    var valuePos = reader.Position;
                    string value = reader.ReadAscii(64);
                    value = value.Substring(0, value.IndexOf('\0'));

                    if (properties.ContainsKey(key.ToLower()))
                        propertyExceptions.Add(new PropertyException(key, "duplicate property "+ value));
                    else
                        properties.Add(key.ToLower(), new Property(value,valuePos)); //#TODO maybe we also want to keep a version with original casing?
                } else
                {
                    reader.BaseStream.Seek(tagLen, SeekOrigin.Current);
                }
            } while (tagName != "#EndOfFile#"); //tagName != "" && 

            return reader.ReadSingle(); //resolution
        }
    }

    public class Model
    {

        public Dictionary<LODResolution, LOD> lods { get; } = new Dictionary<LODResolution, LOD>();
        private uint numLods;


        //These are set by ModelLoader
        //#TODO constructor
        public string subPath { get; set; }
        public string totalPath { get; set; }


        public bool hasErrors
        {
            get
            {
                foreach (var lod in lods)
                {
                    if (lod.Value.hasErrors) return true;
                }

                return false;
            }
        }

        public bool hasWarnings
        {
            get
            {
                foreach (var lod in lods)
                {
                    if (lod.Value.hasWarnings) return true;
                }

                return false;
            }
        }


        private void skipAnimations(BinaryReaderEx reader)
        {
            var numAnims = reader.ReadUInt32();
            uint[] animTypes = new uint[numAnims];
            for (int i = 0; i < numAnims; i++)
            {

                var type = reader.ReadUInt32();
                animTypes[i] = type;
                while (reader.ReadByte() != 0) ; //skip name 
                while (reader.ReadByte() != 0) ; //skip name 
                reader.BaseStream.Seek(4 * 7, SeekOrigin.Current);

                switch (type)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 9:
                        reader.BaseStream.Seek(8, SeekOrigin.Current);
                        break;
                    case 8:
                        reader.BaseStream.Seek(4 * 3 * 2 + 8, SeekOrigin.Current);
                        break;
                }
            }

            var numB2A = reader.ReadUInt32();

            for (int i = 0; i < numB2A; i++)
            {
                var numBones2 = reader.ReadUInt32();
                for (int i2 = 0; i2 < numBones2; i2++)
                {
                    //reader.ReadUInt32();
                    reader.BaseStream.Seek(reader.ReadUInt32() * 4, SeekOrigin.Current);
                }
            }


            for (int i = 0; i < numLods; i++)
            {
                for (int i2 = 0; i2 < numAnims; i2++)
                {
                    var test = reader.ReadInt32();

                    if (test == -1) continue;
                    if (animTypes[i2] == 8 || animTypes[i2] == 9) continue;
            
                    reader.BaseStream.Seek(4*3*2, SeekOrigin.Current);
                }
            }



        }

        private void loadFromODOL(BinaryReaderEx reader)
        {
            var version = reader.ReadUInt32();//version

            if (version < 72)
            {
                //#TODO
                return;
            }



            reader.ReadUInt32();
            var muz = reader.ReadAsciiz();
            numLods = reader.ReadUInt32();
            float[] lodResolutions = new float[numLods];
            for (int i = 0; i < numLods; i++)
            {
                lodResolutions[i] = reader.ReadSingle();
            }



            reader.BaseStream.Seek(8 + 8 + 8 + 4 * 3 + 4 + 4 + 4 + 4 * 3 + 4 * 3
                                   //lod dens coef
                                   + 4 + 4 + 4 * 3 * 5 //bb boxes
                                   + 4 * 9 //matrix
                                   + 4
                                   + 1 //only if v>=73
                                   + 4 * 6
                                   + 1 + 4 + 1 + 4 + 1,
                SeekOrigin.Current
            );
            var skeletonName = reader.ReadAsciiz();


            if (skeletonName.Length != 0) //skeleton
            {
                reader.ReadByte();
                var numBones = reader.ReadUInt32();

                for (int i = 0; i < numBones; i++)
                {
                    while (reader.ReadByte() != 0) ; //skip name 
                    while (reader.ReadByte() != 0) ; //skip name 
                }

                reader.ReadByte();
            }

            reader.ReadByte();
            var massSize = reader.ReadUInt32();
            if (massSize != 0)
            {
                throw new NotImplementedException();
            }

            reader.BaseStream.Seek(16, SeekOrigin.Current);
            reader.BaseStream.Seek(4, SeekOrigin.Current); //#TODO only if >72


            reader.BaseStream.Seek(14 
                                   + 4 + 1, SeekOrigin.Current); //#TODO only if >72

            var name1 = reader.ReadAsciiz();
            var name2 = reader.ReadAsciiz();
            reader.ReadByte();
            var numUnused = reader.ReadUInt32();
            if (numUnused != 0)
            {
                throw new NotImplementedException();
            }

            reader.BaseStream.Seek(12*numLods, SeekOrigin.Current);




            if (reader.ReadByte() != 0)
            {
                skipAnimations(reader);
            }

            uint[] lodOffs = new uint[numLods];

            for (int i = 0; i < numLods; i++)
            {
                lodOffs[i] = reader.ReadUInt32();
            }




            reader.BaseStream.Seek(numLods, SeekOrigin.Current);

            for (int i = 0; i < numLods; i++)
            {
                reader.BaseStream.Seek(lodOffs[i], SeekOrigin.Begin);
                LOD x = new LOD();
                x.loadFromODOL(reader);
                x.resolution = lodResolutions[i];

                if (lods.ContainsKey(x.resolution))
                {
                    x.propertyExceptions.Add(new PropertyException("<Model Loader>", "Duplicate LOD. This lod is "+x.resolution.ToString()));
                    lods.Add(lodResolutions[i]+5, x);
                } else
                    lods.Add(lodResolutions[i], x);

                lods.Add(lodResolutions[i],x);
            }
       
        }

        private void loadFromMLOD(BinaryReaderEx reader)
        {
            reader.ReadUInt32();
            numLods = reader.ReadUInt32();
            for (int i = 0; i < numLods; i++)
            {
                LOD x = new LOD();
                var resolution = x.loadFromMLOD(reader);
                x.resolution = resolution;
                lods.Add(resolution, x);
            }

        }

        public void load(BinaryReaderEx reader)
        {
            var type = reader.ReadAscii(4);

            if (type == "ODOL")
            {
                loadFromODOL(reader);
            } else if (type == "MLOD")
            {
                loadFromMLOD(reader);
            } else
            {
                throw new NotImplementedException();
            }
        }
    }
}
