using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using System.IO;
using System.Text;
using Netherlands3D.Coordinates;

namespace Netherlands3D.ModelParsing
{
    public class StreamreadOBJ : MonoBehaviour
    {
        const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //add the characters you want
        public System.Action<float> broadcastProgressPercentage;
        public System.Action<string> BroadcastErrorMessage;
        bool needToCancel;
        StringBuilder sb = new StringBuilder();
        [HideInInspector]
        public GameObject createdGameObject;
        [HideInInspector]
        public List<MaterialData> materialDataSlots;

        string fileNameWithoutExtention;
        int currentCharStartIndex;
        int currentcharindex;
        char[] readCharsinArray = new char[1024];
        int charlistlength;
        UTF8Encoding utf8 = new UTF8Encoding();

        // OBJ File Tags
        const char COMMENT = '#';
        const char O = 'o';
        const char SPACE = ' ';

        const string Freeform = "vp";
        const string V = "v";
        const string VT = "vt";
        const string VN = "vn";
        const string F = "f";
        const string MTLLIB = "mtllib";
        const string USEMTL = "usemtl";

        enum objTag
        {
            Object,
            Vertex,
            VertexTexture,
            VertexNormal,
            Face,
            MtlLib,
            UseMTL,
            Other,
            LineEnd
        }

        private bool RDCoordinates = false;
        private bool flipFaceDirection = false; // set to true if windingOrder in obj-file is not the standard CounterClockWise
        private bool flipYZ = false; // set to true if Y and Z axes in the obj have been flipped
        public bool ObjectUsesRDCoordinates { get => RDCoordinates; set => RDCoordinates = value; }
        public bool FlipYZ { get => flipYZ; set => flipYZ = value; }
        public bool FlipFaceDirection { get => flipFaceDirection; set => flipFaceDirection = value; }
        SubMeshRawData rawdata = new SubMeshRawData();
        StreamReader streamReader;
        [HideInInspector]
        public bool isFinished = false;
        [HideInInspector]
        public bool succes = true;

        private List<FaceIndices> faces = new List<FaceIndices>();
        public Vector3List vertices = new Vector3List();
        public Vector3List normals = new Vector3List();

        public Vector2List uvs = new Vector2List();

        private Dictionary<char, int> CharToDecimal;

        public Dictionary<string, Submesh> submeshes = new Dictionary<string, Submesh>();

        private Submesh activeSubmesh = new Submesh();

        void AddSubMesh(string submeshName)
        {
            if (activeSubmesh.name == submeshName)
            {
                return;
            }

            if (activeSubmesh.name != null)
            {
                rawdata.EndWriting();
                if (activeSubmesh.vertexCount > 0)
                {
                    if (submeshes.ContainsKey(activeSubmesh.name))
                    {
                        submeshes[activeSubmesh.name] = activeSubmesh;

                    }
                }
                else
                {
                    if (submeshes.ContainsKey(activeSubmesh.name))
                    {
                        submeshes.Remove(activeSubmesh.name);
                    }
                }

            }
            if (submeshes.ContainsKey(submeshName))
            {
                activeSubmesh = submeshes[submeshName];
                rawdata.SetupWriting(activeSubmesh.filename);

            }
            else
            {
                activeSubmesh = new Submesh();
                activeSubmesh.name = submeshName;
                activeSubmesh.filename = randomString(30);
                rawdata.SetupWriting(activeSubmesh.filename);

                int startindexFilenameInMaterialName = submeshName.IndexOf(fileNameWithoutExtention);
                if (startindexFilenameInMaterialName > 0)
                {
                    activeSubmesh.displayName = submeshName.Substring(0, (startindexFilenameInMaterialName - 1));
                }
                activeSubmesh.startIndex = 0;
                submeshes.Add(submeshName, activeSubmesh);
            }
        }

        public void Cancel()
        {
            needToCancel = true;
        }

        void populateCharToDecimal()
        {
            if (CharToDecimal != null)
            {
                return;
            }
            CharToDecimal = new Dictionary<char, int>();
            CharToDecimal.Add('0', 0);
            CharToDecimal.Add('1', 1);
            CharToDecimal.Add('2', 2);
            CharToDecimal.Add('3', 3);
            CharToDecimal.Add('4', 4);
            CharToDecimal.Add('5', 5);
            CharToDecimal.Add('6', 6);
            CharToDecimal.Add('7', 7);
            CharToDecimal.Add('8', 8);
            CharToDecimal.Add('9', 9);

        }
        public void ReadOBJ(string filename, System.Action<bool> callback)
        {
			Debug.Log("PACKAGE STREAMREAD");

            fileNameWithoutExtention = System.IO.Path.GetFileName(filename).Replace(".obj", "");
            populateCharToDecimal();
            needToCancel = false;
            faces.Capacity = 4;
            submeshes.Clear();
            submeshes.Clear();
            activeSubmesh = new Submesh();
            faces.Clear();
            RDCoordinates = false;
            flipFaceDirection = false;
            flipYZ = true;
            isFinished = false;
            succes = true;

            if (!File.Exists(filename))
            {
                if (BroadcastErrorMessage != null) BroadcastErrorMessage("can't find file :" + filename);
                succes = false;
                isFinished = true;
                callback(false);
                return;
            }

            StartCoroutine(StreamReadFile(filename, callback));
        }

        IEnumerator StreamReadFile(string filename, System.Action<bool> callback)
        {
            Debug.Log("StreamReadFile: " + filename);

            vertices.SetupWriting("vertices");
            normals.SetupWriting("normals");
            uvs.SetupWriting("uvs");

            //setup first submesh;
            AddSubMesh("default");
            bool lineRead = true;
            int totalLinesCount = 0;
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);

            streamReader = new StreamReader(fileStream, System.Text.Encoding.UTF8);

            charlistlength = 0;
            currentCharStartIndex = 0;
            currentcharindex = 0;
            System.DateTime time = System.DateTime.UtcNow;

            while (lineRead)
            {

                if ((System.DateTime.UtcNow - time).TotalMilliseconds > 400)
                {
                    if (broadcastProgressPercentage != null) broadcastProgressPercentage(100 * streamReader.BaseStream.Position / streamReader.BaseStream.Length);
                    yield return null;
                    time = System.DateTime.UtcNow;
                }
                if (streamReader.Peek() == -1)
                {
                    lineRead = false;
                    continue;
                }
                totalLinesCount++;
                ReadLine();
                if (needToCancel)
                {
                    Debug.Log("cancelling while reading the obj");
                    streamReader.Close();
                    fileStream.Close();
                    vertices.EndWriting();

                    normals.EndWriting();
                    uvs.EndWriting();

                    rawdata.EndWriting();
                    File.Delete(filename);
                    rollback();
                    isFinished = true;
                    lineRead = false;
                    callback(false);
                    yield break;
                }
            }

            streamReader.Close();
            fileStream.Close();
            vertices.EndWriting();
            normals.EndWriting();
            uvs.EndWriting();


            rawdata.EndWriting();

            File.Delete(filename);
            isFinished = true;
            callback(true);
        }
        void rollback()
        {
            foreach (var item in submeshes)
            {
                rawdata.RemoveData(item.Value.filename);

            }
            vertices.RemoveData();
            normals.RemoveData();
            submeshes.Clear();

        }

        private void ReadLine()
        {
            objTag tag = FindOBJTag();
            switch (tag)
            {
                case objTag.Object:
                    SkipLine();
                    break;
                case objTag.Vertex:
                    ReadVertex();
                    break;
                case objTag.VertexTexture:
                    //SkipLine();
                    ReadVertexTexture();
                    break;
                case objTag.VertexNormal:
                    ReadNormal();
                    break;
                case objTag.Face:
                    ReadFaceLine();
                    break;
                case objTag.MtlLib:
                    SkipLine();
                    break;
                case objTag.UseMTL:
                    AddSubMesh(ReadString());
                    break;
                case objTag.Other:
                    SkipLine();
                    break;
                default:
                    break;
            }
        }

        char makeCharLowerCase(char character)
        {
            int value = (int)character;
            if (value < 91)
            {
                if (value > 64)
                {
                    value += 32;
                    return (char)value;
                }
            }
            return character;
        }

        objTag FindOBJTag()
        {
            char readChar;

            while (true)
            {
                if (NextChar(out readChar))
                {
                    readChar = makeCharLowerCase(readChar);
                    if (readChar == COMMENT)
                    {// found a commentLine
                        return objTag.Other;
                    }
                    else if (readChar == SPACE)
                    {// start with a space, just keep reading

                    }
                    else if (readChar == '\n')
                    {// start with a line-end, continue on next line;

                    }
                    else if (readChar == '\r')
                    {// start with a line-end, continue on next line;

                    }
                    else if (readChar == 'v')
                    {// could be v, vt or vn.
                        if (NextChar(out readChar))
                        {
                            readChar = makeCharLowerCase(readChar);
                            if (readChar == ' ')
                            {// found v
                                return objTag.Vertex;
                            }
                            else if (readChar == 'n')
                            {   // probably found vn. check if it is followed by a space
                                if (NextChar(out readChar))
                                {
                                    readChar = makeCharLowerCase(readChar);
                                    if (readChar == ' ')
                                    { // did find vn
                                        return objTag.VertexNormal;
                                    }
                                    else
                                    {// found something else after all
                                        return objTag.Other;
                                    }
                                }
                            }
                            else if (readChar == 't')
                            {// probably found vt. check if it is followed bij a space
                                if (NextChar(out readChar))
                                {
                                    readChar = makeCharLowerCase(readChar);
                                    if (readChar == ' ')
                                    { // did find vt
                                        return objTag.VertexTexture;
                                    }
                                    else
                                    {// found something else after all
                                        return objTag.Other;
                                    }
                                }

                            }
                            else
                            { // found someting else
                                return objTag.Other;
                            }
                        }
                        else { return objTag.Other; }
                    }
                    else if (readChar == 'o')
                    {// possibly found object, check if it is followed by a space
                        if (NextChar(out readChar))
                        {
                            if (readChar == ' ')
                            {// found object
                                return objTag.Object;
                            }
                            else
                            {// found someting else
                                return objTag.Other;
                            }
                        }
                        else { return objTag.Other; }
                    }
                    else if (readChar == 'f')
                    {//possibly found a face, check if it is followed by a space
                        if (NextChar(out readChar))
                        {
                            if (readChar == ' ')
                            {// found a face
                                return objTag.Face;
                            }
                            else
                            {// it was something else
                                return objTag.Other;
                            }
                        }
                        else { return objTag.Other; };
                    }
                    else if (readChar == 'u')
                    {// could have found usemtl
                        if (NextChar(out readChar))
                        {
                            readChar = makeCharLowerCase(readChar);
                            if (readChar == 's')
                            {
                                readChar = makeCharLowerCase(readChar);
                                if (NextChar(out readChar))
                                {
                                    readChar = makeCharLowerCase(readChar);
                                    if (readChar == 'e')
                                    {
                                        if (NextChar(out readChar))
                                        {
                                            readChar = makeCharLowerCase(readChar);
                                            if (readChar == 'm')
                                            {
                                                if (NextChar(out readChar))
                                                {
                                                    readChar = makeCharLowerCase(readChar);
                                                    if (readChar == 't')
                                                    {
                                                        if (NextChar(out readChar))
                                                        {
                                                            readChar = makeCharLowerCase(readChar);
                                                            if (readChar == 'l')
                                                            {
                                                                if (NextChar(out readChar))
                                                                {
                                                                    readChar = makeCharLowerCase(readChar);
                                                                    if (readChar == ' ')
                                                                    {// found usemtl
                                                                        return objTag.UseMTL;
                                                                    }
                                                                    else { return objTag.Other; }
                                                                }
                                                                else { return objTag.Other; };
                                                            }
                                                            else { return objTag.Other; }
                                                        }
                                                        else { return objTag.Other; };
                                                    }
                                                    else { return objTag.Other; }
                                                }
                                                else { return objTag.Other; };
                                            }
                                            else { return objTag.Other; }
                                        }
                                        else { return objTag.Other; };
                                    }
                                    else { return objTag.Other; }
                                }
                                else { return objTag.Other; };
                            }
                            else { return objTag.Other; }
                        }
                        else { return objTag.Other; };
                    }
                    else if (readChar == 'm')
                    {
                        if (NextChar(out readChar))
                        {
                            readChar = makeCharLowerCase(readChar);
                            if (readChar == 't')
                            {
                                if (NextChar(out readChar))
                                {
                                    readChar = makeCharLowerCase(readChar);
                                    if (readChar == 'l')
                                    {
                                        if (NextChar(out readChar))
                                        {
                                            readChar = makeCharLowerCase(readChar);
                                            if (readChar == 'l')
                                            {
                                                if (NextChar(out readChar))
                                                {
                                                    readChar = makeCharLowerCase(readChar);
                                                    if (readChar == 'i')
                                                    {
                                                        if (NextChar(out readChar))
                                                        {
                                                            readChar = makeCharLowerCase(readChar);
                                                            if (readChar == 'b')
                                                            {
                                                                if (NextChar(out readChar))
                                                                {
                                                                    if (readChar == ' ')
                                                                    { // found mtllib
                                                                        return objTag.MtlLib;
                                                                    }
                                                                    else { return objTag.Other; }
                                                                }
                                                                else { return objTag.Other; };
                                                            }
                                                            else { return objTag.Other; }
                                                        }
                                                        else { return objTag.Other; };
                                                    }
                                                    else { return objTag.Other; }
                                                }
                                                else { return objTag.Other; };
                                            }
                                            else { return objTag.Other; }
                                        }
                                        else { return objTag.Other; };
                                    }
                                    else { return objTag.Other; }
                                }
                                else { return objTag.Other; };
                            }
                            else { return objTag.Other; }
                        }
                        else { return objTag.Other; };
                    }
                    else
                    {// found something else
                        return objTag.Other;
                    }

                }
                else
                {
                    // reached the end of the file
                    return objTag.Other;
                }
            }
        }

        void SkipLine()
        {
            char readChar;
            while (true)
            {
                if (NextChar(out readChar))
                {
                    if (readChar == '\r')
                    {
                        return;
                    }
                    else if (readChar == '\n')
                    {
                        return;
                    }
                }
                else { return; }
            }
        }
        string ReadString()
        {
            char readChar;

            sb.Clear();
            while (true)
            {
                if (NextChar(out readChar))
                {
                    if (readChar == '\r')
                    {
                        return sb.ToString();
                    }
                    else if (readChar == '\n')
                    {
                        return sb.ToString();
                    }
                    else
                    {
                        sb.Append(readChar);
                    }
                }
                else { return ""; }
            }
        }

        void ReadVertex()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();

            if (x != float.NaN && y != float.NaN & z != float.NaN)
            {
                if (vertices.Count() == 0)
                {// this is the first vertex, check if it is in rd-coordinates
                    CheckForRD(x, y, z);
                }
                if (ObjectUsesRDCoordinates)
                {
                    Vector3 coord;
                    if (flipYZ)
                    {
                        coord = CoordinateConverter.RDtoUnity(new Vector3(x, z, y));
                    }
                    else
                    {
                        coord = CoordinateConverter.RDtoUnity(new Vector3(x, y, z));
                    }
                    vertices.Add(coord.x, coord.y, coord.z);
                    return;
                }

                if (FlipYZ)
                {
                    vertices.Add(x, z, y);

                }
                else
                {
                    vertices.Add(x, y, -z);
                }
            }
        }
        void CheckForRD(float x, float y, float z)
        {
            if (CoordinateConverter.RDIsValid(new Vector3RD(x, z, y)))
            {
                ObjectUsesRDCoordinates = true;
                FlipYZ = true;
            }
            else if (CoordinateConverter.RDIsValid(new Vector3RD(x, y, z)))
            {
                ObjectUsesRDCoordinates = true;
                FlipYZ = false;
            }
            else
            {
                ObjectUsesRDCoordinates = false;
                FlipYZ = false;
            }

        }

        void ReadVertexTexture()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            if (x != float.NaN && y != float.NaN)
            {
                //buffer.PushUV(new Vector2(x, y));
                uvs.Add(x, y);
            }
        }

        void ReadFaceLine()
        {
            faces.Clear();
            bool keepGoing = true;
            char lastChar;
            FaceIndices faceIndex;
            while (keepGoing)
            {
                faceIndex = new FaceIndices();
                if (ReadSingleFace(out faceIndex, out lastChar))
                {//succesfully read a face
                    faces.Add(faceIndex);
                    if (lastChar == '\r')
                    { // reached the end of the line
                        keepGoing = false;
                    }
                    if (lastChar == '\n')
                    {// reached the end of the line
                        keepGoing = false;

                    }
                }
                else
                {
                    keepGoing = false;
                }

            }
            /// process faces
            if (faces.Count == 3)
            {
                //Vector3 triangleNormal = CalculateNormal(vertices[faces[0].vertexIndex], vertices[faces[1].vertexIndex], vertices[faces[2].vertexIndex]);
                if (!FlipFaceDirection)
                {
                    SaveVertexToSubmesh(faces[0]);
                    SaveVertexToSubmesh(faces[2]);
                    SaveVertexToSubmesh(faces[1]);
                }
                else
                {
                    SaveVertexToSubmesh(faces[0]);
                    SaveVertexToSubmesh(faces[1]);
                    SaveVertexToSubmesh(faces[2]);
                }




            }
            else if (faces.Count == 4)
            {
                //Vector3 triangleNormal = CalculateNormal(vertices[faces[0].vertexIndex], vertices[faces[1].vertexIndex], vertices[faces[3].vertexIndex]);
                if (!FlipFaceDirection)
                {
                    SaveVertexToSubmesh(faces[0]);
                    SaveVertexToSubmesh(faces[1]);
                    SaveVertexToSubmesh(faces[3]);
                    SaveVertexToSubmesh(faces[3]);
                    SaveVertexToSubmesh(faces[1]);
                    SaveVertexToSubmesh(faces[2]);
                }
                else
                {
                    SaveVertexToSubmesh(faces[0]);
                    SaveVertexToSubmesh(faces[1]);
                    SaveVertexToSubmesh(faces[2]);
                    SaveVertexToSubmesh(faces[0]);
                    SaveVertexToSubmesh(faces[2]);
                    SaveVertexToSubmesh(faces[3]);
                }

            }
            else
            {
                Debug.Log(faces.Count + " vertices in a face");
            }
        }
        void SaveVertexToSubmesh(FaceIndices v1)
        {
            rawdata.Add(v1.vertexIndex, v1.vertexNormal, v1.vertexUV);
            activeSubmesh.vertexCount++;
            return;
        }

        bool ReadSingleFace(out FaceIndices faceIndex, out char readChar)
        {
            FaceIndices face = new FaceIndices();
            face.vertexNormal = -1;
            int number;
            char lastChar;

            if (ReadInt(out number, out lastChar))
            {
                if (number < 0)
                {
                    number += vertices.Count();
                }
                face.vertexIndex = number - 1; //subtract 1 because objindexes start with 1

                if (lastChar == '/') // vertex is followed by texture and / or normal;
                {

                    if (ReadInt(out number, out lastChar))
                    {// succesfully read vertexUV
                        face.vertexUV = number - 1;
                    }

                    if (lastChar != '\n')
                    {
                        if (ReadInt(out number, out lastChar))
                        {
                            if (number < 0)
                            {
                                number += normals.Count();
                            }
                            // succesfully read Normal
                            face.vertexNormal = number - 1;
                        }
                    }

                }



            }
            else
            {// couldn't read a valid integer
                faceIndex = face;
                readChar = lastChar;
                return false;
            }
            faceIndex = face;
            readChar = lastChar;
            return true;
        }

        bool ReadInt(out int number, out char lastChar)
        {// return if succesfully found a bool
            char readChar = ' ';
            bool numberFound = false;
            number = 0;
            int sign = 1;
            bool keepGoing = true;
            while (keepGoing)
            {
                if (NextChar(out readChar))
                {
                    if (isDigit(readChar))
                    {
                        numberFound = true;
                        number = (number * 10) + (int)readChar - 48;
                        //number = (number * 10) + (int)char.GetNumericValue(readChar);
                    }
                    else if (readChar == '-')
                    {
                        sign = -1;
                    }
                    else if (readChar == ' ' && numberFound == false)
                    {// found a space before the start of the number

                    }
                    else
                    {
                        keepGoing = false;

                    }
                }
                else
                {
                    keepGoing = false;
                }
            }
            number = number * sign;
            lastChar = readChar;
            return numberFound;
        }
        void ReadNormal()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();
            if (flipFaceDirection)
            {
                x = -x;
                y = -y;
                z = -z;
            }
            if (x != float.NaN && y != float.NaN & z != float.NaN)
            {
                if (FlipYZ)
                {
                    normals.Add(x, z, y);
                }
                else
                {
                    normals.Add(x, y, -z);
                }
            }
        }

        float ReadFloat()
        {
            char readChar;
            bool numberFound = false;
            long number = 0;
            int decimalPlaces = 0;
            bool isDecimal = false;
            int sign = 1;
            bool hasExponent = false;
            int exponentSign = 1;
            int exponent = 0;
            bool keepGoing = true;
            while (keepGoing)
            {
                if (NextChar(out readChar))
                {
                    switch (readChar)
                    {
                        case '\0':
                            //found a null-value
                            if (numberFound)
                            { // if we start with a space we continue
                                keepGoing = false;
                            }
                            break;
                        case ' ':
                            //found a space
                            if (numberFound)
                            { // if we start with a space we continue
                                keepGoing = false;
                            }
                            break;
                        case '\r':
                            // end of the line, end of the floatvalue
                            keepGoing = false;
                            break;
                        case '\n':
                            // end of the line, end of the floatvalue
                            keepGoing = false;
                            break;
                        case '.':
                            // found a decimalpoint
                            isDecimal = true;
                            break;
                        case 'e':
                            if (numberFound)
                            {
                                hasExponent = true;
                            }
                            break;
                        case '-':
                            // found a negative-sign
                            if (hasExponent)
                            {
                                exponentSign = -1;
                            }
                            else
                            {
                                sign = -1;
                            }
                            break;
                        default:
                            // no space or endof the line
                            if (isDigit(readChar))
                            //if (char.IsDigit(readChar))
                            {
                                numberFound = true;
                                if (!hasExponent)
                                {


                                    //number = (number * 10) + (int)char.GetNumericValue(readChar);
                                    number = (number * 10) + (int)readChar - 48;

                                    if (isDecimal)
                                    {
                                        decimalPlaces++;
                                    }
                                }
                                else
                                {
                                    exponent = (exponent * 10) + (int)readChar - 48; ;
                                }
                            }
                            else
                            {// found something else, so we stop
                                keepGoing = false;
                            }
                            break;
                    }
                }
                else { keepGoing = false; }
            }
            if (numberFound)
            {
                float value = sign * number / (Mathf.Pow(10, decimalPlaces));
                if (hasExponent)
                {
                    value *= Mathf.Pow(10, (exponentSign * exponent));
                }
                return value;
            }
            else
            { // no number found, so we return NAN;
                return float.NaN;
            }

        }
        bool isDigit(char character)
        {
            int waarde = (int)character;
            if (waarde < 58)
            {
                if (waarde > 47)
                {
                    return true;
                }
            }
            return false;
        }

        private bool NextChar(out char character)
        {

            //if (streamReader.Peek() > -1)
            //{
            //	character = (char)streamReader.Read();
            //	return true;
            //}
            //character = 'e';
            //return false;

            if (currentcharindex == charlistlength)
            {
                //currentCharStartIndex += 100;
                charlistlength = streamReader.Read(readCharsinArray, 0, 1024);
                currentcharindex = 0; // JLN: bug fix, needs to be above if!

                if (charlistlength == 0)
                {
                    character = 'e';
                    return false;
                }

            }

            if (currentcharindex >= readCharsinArray.Length)
            {
                int x = 0;
            }
            character = readCharsinArray[currentcharindex];
            currentcharindex++;
            return true;

        }

        public struct FaceIndices
        {
            public int vertexIndex;
            public int vertexUV;
            public int vertexNormal;
        }
        string randomString(int length)
        {
            string returnstring = "";
            for (int i = 0; i < length; i++)
            {
                returnstring += glyphs[Random.Range(0, glyphs.Length)];
            }
            return returnstring;
        }
    }
}
