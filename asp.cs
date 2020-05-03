//Console.WriteLine("*");
#pragma warning disable 169, 414, 649

//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
  using System; using System.IO; using System.Linq; using System.Text; 
  using System.Text.RegularExpressions; using System.Collections; 
  using System.Globalization; using System.Collections.Generic; 
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو

sealed class Test 
{
    private static AspModel   model;
    private static float     modelScale = 1.0f;
    private static string    texFileNameExt = ".tga";
    private static    int   currentSubMeshIndex;

// ========================================================

    public static void Main()
    {
    SearchOption SOAD = SearchOption.AllDirectories;
    string[] filesName = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.asp",  SOAD); 
    System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

// ========================================================

      foreach (var file in filesName)
      {
          using (BinaryReader br = new BinaryReader(File.Open(file, FileMode.Open)))
          {
            string name = file.Replace(".asp","");

            using (StreamWriter objw = new StreamWriter(name + ".obj"))
            {
              using (StreamWriter mtlw = new StreamWriter(name + ".mtl"))
              {
                  Console.WriteLine("====== Beginning ASP import... ======");

                  model = new AspModel();

                  FourCC chunkId = new FourCC();

                  while (model.readFourCC(br, ref chunkId))
                  {
                    if      (chunkId.ToString() == "BMSH") model.readBMSH(br);  

                    else if (chunkId.ToString() == "BONH") model.readBONH(br); 
                    else if (chunkId.ToString() == "BSUB") model.readBSUB(br); 
                    else if (chunkId.ToString() == "BSMM") model.readBSMM(br); 
                    else if (chunkId.ToString() == "BVTX") model.readBVTX(br); 
                    else if (chunkId.ToString() == "BCRN") model.readBCRN(br); 
                    else if (chunkId.ToString() == "WCRN") model.readWCRN(br); 
                    else if (chunkId.ToString() == "BVMP") model.readBVMP(br);  
                    else if (chunkId.ToString() == "BTRI") model.readBTRI(br); 
                    else if (chunkId.ToString() == "BVWL") model.readBVWL(br);  
                    else if (chunkId.ToString() == "STCH") model.readSTCH(br);  
                    else if (chunkId.ToString() == "RPOS") model.readRPOS(br);  
                    else if (chunkId.ToString() == "BBOX") model.readBBOX(br);  
//                  else if (chunkId.ToString() == "BEND") model.readBEND(br); // َ ىهيے îّèلêà EOF -_-
                    else { } // Unhandled chunk; Ignore it.
                  }

                  Console.WriteLine("====== Reached end of ASP data ======");

              //==========================================================

                objw.WriteLine("");

                  Console.WriteLine("Writing OBJ...");

                  int subMeshIndex = 0;
                  var subMeshes = model.subMeshes;

                  objw.WriteLine("# File generated by asp2obj from DSiege ASPECT \"" + file + "\".");
                  objw.WriteLine("mtllib " + name + ".mtl");
                  
                  // Per-vertex info:

                  foreach (var mesh in subMeshes)
                  {
                    objw.WriteLine("g AspMesh_" + subMeshIndex++ );

                    // Vertexes: //----------------------------------------------------------
                    
                    foreach (var c in mesh.wCorners) {
                        Vec3 v = new Vec3 { 
                        X = (c.pos.X * modelScale),
                        Y = -(c.pos.Z * modelScale),
                        Z = (c.pos.Y * modelScale) };

                        objw.WriteLine("v " + v.X + " " + v.Y + " " + v.Z);
                    }   objw.WriteLine();

                    // Vertex normals: //----------------------------------------------------
                    
                    foreach (var c in mesh.wCorners) {
                        Vec3 n = c.normal;
                        objw.WriteLine("vn " + n.X + " " + n.Y + " " + n.Z);
                    }   objw.WriteLine();

                    // Texture coordinates: //-----------------------------------------------
                    
                    foreach (var c in mesh.wCorners) {
                      Vec2 t = c.texCoord;
                      objw.WriteLine("vt " + t.U + " " + t.V);
                    } objw.WriteLine();
                  }

                  // Faces:
                  
                  subMeshIndex = 0;
                  
                  int cornerOffset = 0;

                  var modelTextures = model.textureNames;

                  foreach (var mesh in subMeshes)
                  {
                    objw.WriteLine("g AspMesh_" + subMeshIndex++ );

                    int f = 0;

                    for (int i = 0; i < mesh.textureCount; ++i)
                    {
                      objw.WriteLine("usemtl " + modelTextures[mesh.matInfo[i].textureIndex]);
                      objw.WriteLine("s 1\n"); // Allow smooth shading.

                      for (int j = 0; j < mesh.matInfo[i].faceSpan; ++j)
                      {
                        var offset = mesh.faceInfo.cornerStart[i] + cornerOffset;
                        var a = mesh.faceInfo.cornerIndex[f].index[0] + offset + 1;
                        var b = mesh.faceInfo.cornerIndex[f].index[1] + offset + 1;
                        var c = mesh.faceInfo.cornerIndex[f].index[2] + offset + 1; // +1 for the OBJ

                        // Position + texture + normal

                        objw.WriteLine("f " + a + "/" + a + "/" + a + " "
                                + b + "/" + b + "/" + b + " "
                                + c + "/" + c + "/" + c + "\n");
                        ++f;
                      }
                    }
                    cornerOffset += mesh.cornerCount;
                  }

                  objw.WriteLine();

                  Console.WriteLine("OBJ Finished.");

              //==========================================================

                  Console.WriteLine("Writing MTL...");

                  subMeshes     = model.subMeshes;
                  modelTextures = model.textureNames;

                  mtlw.WriteLine();

                  foreach (var mesh in subMeshes)
                  {
                    for (int i = 0; i < mesh.textureCount; ++i)
                    {
                      var textureName = modelTextures[mesh.matInfo[i].textureIndex];

                      mtlw.WriteLine("newmtl " + textureName);
                      mtlw.WriteLine("Ka 0.00 0.00 0.00"); // Ambient
                      mtlw.WriteLine("Kd 1.00 1.00 1.00"); // Diffuse
                      mtlw.WriteLine("Ks 0.50 0.50 0.50"); // Specular
                      mtlw.WriteLine("Ns 95.00"); // Specular exponent/power
                      mtlw.WriteLine("map_Kd " + (textureName + texFileNameExt));
                    }
                  }

                  mtlw.WriteLine();
                  
                  Console.WriteLine("MTL Finished.");

              //==========================================================

              }//using sw
            }//using sw
          }//using br
      } // foreach (var file in filesName)
    } // public static void Main()
} // sealed class Test {

//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو

sealed class AspModel
{
    private static    int   currentSubMeshIndex;

//  void validateVersion(const char * sectName, int version) const;
//  bool isValid() const;  // Test if this object has valid model data.
//  void dispose();  // Disposes model data, making this class an empty/invalid model.

  // Model data:

    public List<SubMesh>   subMeshes    = new List<SubMesh>();
    public List<BoneInfo>  boneInfos    = new List<BoneInfo>();
    public List<string>    textureNames = new List<string>();

  // Source filename for debug printing.
  // May be empty if the model was loaded from memory.

    public string srcFileName;

    //==========================================================

    public void readBMSH(BinaryReader br)
    {
      Console.WriteLine("====== Reading BMSH section ======");
      var version = br.ReadInt32();
      validateVersion("BMSH", version);

   // Common mesh fields:
      var sizeTextField = br.ReadInt32(); 
      var boneCount     = br.ReadInt32(); 
      var textureCount  = br.ReadInt32(); 
      var vertexCount   = br.ReadInt32(); 
      var subMeshCount  = br.ReadInt32(); 
      var renderFlags   = br.ReadInt32(); 

   // A length this big can only mean a broken file...

      if (sizeTextField >= (1024 * 1024))
      {
          throw new Exception("Bogus text length in BMSH section for ASP file \"" + srcFileName + "\"!");
      }

      // Read the text payload that follows BMSH:

      byte[] rawText = new byte[sizeTextField]; // ByteArray rawText(sizeTextField);
      br.Read(rawText, 0, sizeTextField);        // readBytes(rawText.data(), rawText.size());
			string text_payload = System.Text.Encoding.Default.GetString(rawText);

      // Split textures from bone names.
      // Each string is separated by one or more null bytes.

      int index = 0; // لûë size_t

			for (int oo = 0; oo < textureCount; oo++)
      textureNames.Add(null); // textureNames.resize(textureCount);

      for (int t = 0; t < textureCount; ++t)
      {
        for (; index < rawText.Length; ++index)
        {
          char c = (char)rawText[index];
          if (c == '\0')
          {
            // Skip null padding for the next name:
            while (index < rawText.Length && rawText[index] == 0)
            {
              ++index;
            }
            break;
          }
          textureNames[t] = textureNames[t] + c;
        }
      }

			for (int oo = 0; oo < boneCount; oo++)
      boneInfos.Add(new BoneInfo()); // boneInfos.resize(boneCount);

      for (int b = 0; b < boneCount; ++b)
      {
        for (; index < rawText.Length; ++index)
        {
          char c = (char)rawText[index];
          if (c == '\0')
          {
            // Skip null padding for the next name:
            while (index < rawText.Length && rawText[index] == 0)
            {
              ++index;
            }
            break;
          }
          boneInfos[b].name = boneInfos[b].name + c;
        }
      }

   // Allocate memory for other fields:
			for (int oo = 0; oo < subMeshCount; oo++)
      subMeshes.Add(new SubMesh()); // subMeshes.resize(subMeshCount);

      // Print the struct:

      Console.WriteLine("sizeTextField...: " + sizeTextField);
      Console.WriteLine("boneCount.......: " + boneCount);
      Console.WriteLine("textureCount....: " + textureCount);
      Console.WriteLine("vertexCount.....: " + vertexCount);
      Console.WriteLine("subMeshCount....: " + subMeshCount);
      Console.WriteLine("renderFlags.....: " + renderFlags);

// https://stackoverflow.com/questions/2581325/help-with-0-terminated-strings-in-c-sharp

/*
      // Put a ` in the null bytes so we can easily visualize-it:
      // çàىهيèâ يîëè àïîًٌٍîôîى - ىû ىîوهى âèçَàëèçèًîâàٍü ‎ٍî

      std::transform
      (
      std::begin(rawText), 
      std::end(rawText), 
      std::begin(rawText),
          [](uint8_t b) { return (b != 0) ? b : '`'; });

      // Print it as a null-terminated string:

      rawText.push_back(0);

      Console.WriteLine("rawText.........: " + reinterpret_cast<const char *>(rawText.data()));
*/
      // Print texture names and bone names we've parsed:

      foreach (var texName in textureNames)
      {
          Console.WriteLine("textureName.....: " + texName);
      }

      foreach (var bone in boneInfos)
      {
          Console.WriteLine("boneName........: " + bone.name);
      }
    }
//=======================================================================================

		public void readBONH(BinaryReader br)		{
        Console.WriteLine("====== Reading BONH section ======");
        var version = br.ReadInt32();
        validateVersion("BONH", version);
        
        // A tuple of [bone_index, parent_index, bone_flags]
        // for every bone of the mesh. Indexes are zero based.

        for (long b = 0; b < boneInfos.Count; b++)
        {
          var boneIndex   = br.ReadInt32();
          var parentIndex = br.ReadInt32();
          var boneFlags   = br.ReadInt32();

          boneInfos[boneIndex].parentIndex = parentIndex;
          boneInfos[boneIndex].flags       = boneFlags;

          Console.WriteLine("bone[" + boneIndex + "].name.........: " + boneInfos[boneIndex].name);
          Console.WriteLine("bone[" + boneIndex + "].parentIndex..: " + boneInfos[boneIndex].parentIndex);
          Console.WriteLine("bone[" + boneIndex + "].flags........: " + boneInfos[boneIndex].flags);
        }
    }
//=======================================================================================

		public void readBSUB(BinaryReader br)		{
        Console.WriteLine("====== Reading BSUB section ======");

        var version = br.ReadInt32();
        validateVersion("BSUB", version);

        // Zero based index if v > 40

        currentSubMeshIndex = br.ReadInt32();

        if (versionOf(version) <= 40)
        {
          currentSubMeshIndex += 1; // Convert -1 based index to 0 based index.
        }

        // Why is this stored twice? I have no idea...

        var textureCount = br.ReadInt32();
        
        if (textureCount != textureNames.Count)
        {
            throw new Exception("Texture count mismatch in BSUB section for ASP file \"" + srcFileName + "\"!");
        }

        var mesh = subMeshes[currentSubMeshIndex];

        mesh.vertexCount = br.ReadInt32();
        mesh.cornerCount = br.ReadInt32();
        mesh.faceCount   = br.ReadInt32();

        Console.WriteLine("subMeshIndex....: " + currentSubMeshIndex);
        Console.WriteLine("textureCount....: " + textureCount);
        Console.WriteLine("vertexCount.....: " + mesh.vertexCount);
        Console.WriteLine("cornerCount.....: " + mesh.cornerCount);
        Console.WriteLine("faceCount.......: " + mesh.faceCount);
		}
//=======================================================================================

		public void readBSMM(BinaryReader br)		{
        Console.WriteLine("====== Reading BSMM section ======");

        var version = br.ReadInt32();
        validateVersion("BSMM", version);

        var mesh = subMeshes[currentSubMeshIndex];
        mesh.textureCount = br.ReadInt32();

  			for (int oo = 0; oo < mesh.textureCount; oo++)
        mesh.matInfo.Add(new MatInfo()); // mesh.matInfo.resize(mesh.textureCount);

        for (int t = 0; t < mesh.textureCount; ++t)
        {
            mesh.matInfo[t].textureIndex = br.ReadInt32();
            mesh.matInfo[t].faceSpan     = br.ReadInt32();

            Console.WriteLine("mat[" + t + "].textureIndex.: " + mesh.matInfo[t].textureIndex);
            Console.WriteLine("mat[" + t + "].faceSpan.....: " + mesh.matInfo[t].faceSpan);
        }
		}
//=======================================================================================

		public void readBVTX(BinaryReader br)		{
        Console.WriteLine("====== Reading BVTX section ======");

        var version = br.ReadInt32();
        validateVersion("BVTX", version);

        var mesh = subMeshes[currentSubMeshIndex];

        if (mesh.vertexCount != br.ReadInt32())
        {
            throw new Exception("Vertex count mismatch in BVTX section for ASP file \"" + srcFileName + "\"!");
        }

  			for (int oo = 0; oo < mesh.vertexCount; oo++)
        mesh.positions.Add(new Vec3()); // mesh.positions.resize(mesh.vertexCount);

        for (int v = 0; v < mesh.vertexCount; ++v)
        {
            mesh.positions[v] = readVec3(br);
        }
          Console.WriteLine("vertexCount.....: " + mesh.vertexCount);
		}
//=======================================================================================

		public void readBCRN(BinaryReader br)		{
        Console.WriteLine("====== Reading BCRN section ======");

        var version = br.ReadInt32();
        validateVersion("BCRN", version);

        var mesh = subMeshes[currentSubMeshIndex];

        if (mesh.cornerCount != br.ReadInt32())
        {
            throw new Exception("Corner/edge count mismatch in BCRN section for ASP file \"" + srcFileName + "\"!");
        }

  			for (int oo = 0; oo < mesh.cornerCount; oo++)
        mesh.corners.Add(new CornerInfo()); // mesh.corners.resize(mesh.cornerCount);

        for (int c = 0; c < mesh.cornerCount; ++c)
        {
            var corner = mesh.corners[c];

            // Vertex position:

            corner.vtxIndex = br.ReadInt32();

            if (corner.vtxIndex > mesh.positions.Count)
            {
                Console.WriteLine("Out-of-bounds vertex index in BCRN section! Clamping it...");
                corner.vtxIndex = (int)(mesh.positions.Count - 1);
            }

            // Vertex normal, color:

            corner.normal = readVec3(br);
            corner.color  = readColor(br);

            // Why did they leave this unused field here in the middle?
            var unused = br.ReadInt32();

            // Float UVs:

            corner.texCoord = readTexCoord(br);
        }
          Console.WriteLine("cornerCount.....: " + mesh.cornerCount);
		}
//=======================================================================================

		public void readWCRN(BinaryReader br)		{
        Console.WriteLine("====== Reading WCRN section ======");
        var version = br.ReadInt32();
        validateVersion("WCRN", version);
        var mesh = subMeshes[currentSubMeshIndex];

        if (mesh.cornerCount != br.ReadInt32())
        {
            throw new Exception("Corner/edge count mismatch in WCRN section for ASP file \"" + srcFileName + "\"!");
        }

  			for (int oo = 0; oo < mesh.cornerCount; oo++)
        mesh.wCorners.Add(new WCornerInfo()); // mesh.wCorners.resize(mesh.cornerCount);

        for (int c = 0; c < mesh.cornerCount; ++c)
        {
          var wCorner = mesh.wCorners[c];

          wCorner.pos      = readVec3(br);
          wCorner.weight   = readVec4(br);
          wCorner.bone     = readColor(br);

          // TODO
          //if (versionOf(version) > 40)
          //	bone = ReadFourBB2
          //else
          //	bone = ReadFourBB

          wCorner.normal   = readVec3(br);
          wCorner.color    = readColor(br);
          wCorner.texCoord = readTexCoord(br);

          /* TODO
          // remove null bone/weights
          // This is a reverse iteration, I guess, from 4 to 1 (or 0)

          for i = 4 to 1 by -1 do
          if (w[i] == 0) do
          (
            deleteItem w i
            deleteItem b i
          )
          */
        }
          Console.WriteLine("cornerCount.....: " + mesh.cornerCount);
		}
//=======================================================================================
    public void readBVMP(BinaryReader br)    {
      Console.WriteLine("====== Reading BVMP section ======");
      var version = br.ReadInt32();
      validateVersion("BVMP", version);       }    // TODO
//=======================================================================================

		public void readBTRI(BinaryReader br)		{
        Console.WriteLine("====== Reading BTRI section ======");
        var version = br.ReadInt32();
        validateVersion("BTRI", version);

        var mesh = subMeshes[currentSubMeshIndex];

        if (mesh.faceCount != br.ReadInt32())
        {
            throw new Exception("Face count mismatch in BTRI section for ASP file \"" + srcFileName + "\"!");
        }

        if (versionOf(version) == 22)
        {
            Console.WriteLine("BTRI version == 2.2");

            for (int oo = 0; oo < mesh.textureCount; oo++)
            mesh.faceInfo.cornerSpan.Add(new int()); 
         // mesh.faceInfo.cornerSpan.resize(mesh.textureCount);

            for (int i = 0; i < mesh.textureCount; ++i)
            {
                mesh.faceInfo.cornerSpan[i] = br.ReadInt32();
            }

            for (int oo = 0; oo < mesh.textureCount; oo++)
            mesh.faceInfo.cornerStart.Add(new int()); 
         // mesh.faceInfo.cornerStart.resize(mesh.textureCount);

            mesh.faceInfo.cornerStart[0] = 0;

            for (int i = 0; i < mesh.textureCount - 1; ++i)
            {
                mesh.faceInfo.cornerStart[i + 1] 
                = mesh.faceInfo.cornerStart[i] 
                + mesh.faceInfo.cornerSpan[i];
            }
        }
        else if (versionOf(version) > 22)
        {
            Console.WriteLine("BTRI version > 2.2");

            for (int oo = 0; oo < mesh.textureCount; oo++)
            mesh.faceInfo.cornerStart.Add(new int()); 
         // mesh.faceInfo.cornerStart.resize(mesh.textureCount);

            for (int oo = 0; oo < mesh.textureCount; oo++)
            mesh.faceInfo.cornerSpan.Add(new int()); 
         // mesh.faceInfo.cornerSpan.resize(mesh.textureCount);

            for (int i = 0; i < mesh.textureCount; ++i)
            {
                mesh.faceInfo.cornerStart[i] = br.ReadInt32();
                mesh.faceInfo.cornerSpan[i]  = br.ReadInt32();
            }
        }
        else
        {
            Console.WriteLine("BTRI version < 2.2");

            for (int oo = 0; oo < mesh.textureCount; oo++)
            mesh.faceInfo.cornerStart.Add(new int()); 
         // mesh.faceInfo.cornerStart.resize(mesh.textureCount);

            for (int oo = 0; oo < mesh.textureCount; oo++)
            mesh.faceInfo.cornerSpan.Add(new int()); 
         // mesh.faceInfo.cornerSpan.resize(mesh.textureCount);

            for (int i = 0; i < mesh.textureCount; ++i)
            {
                mesh.faceInfo.cornerStart[i] = 0;
                mesh.faceInfo.cornerSpan[i]  = mesh.cornerCount;
            }
        }

        for (int oo = 0; oo < mesh.faceCount; oo++)
        mesh.faceInfo.cornerIndex.Add(new TriIndex()); 
     // mesh.faceInfo.cornerIndex.resize(mesh.faceCount);

        for (int f = 0; f < mesh.faceCount; ++f)
        {
            mesh.faceInfo.cornerIndex[f].index[0] = br.ReadInt32();
            mesh.faceInfo.cornerIndex[f].index[1] = br.ReadInt32();
            mesh.faceInfo.cornerIndex[f].index[2] = br.ReadInt32();
        }
          Console.WriteLine("faceCount.......: " + mesh.faceCount);
		}
//=======================================================================================
    public void readBVWL(BinaryReader br)    {
      Console.WriteLine("====== Reading BVWL section ======");
      var version = br.ReadInt32();
      validateVersion("BVWL", version);       }    // TODO
//=======================================================================================
    public void readSTCH(BinaryReader br)    {
      Console.WriteLine("====== Reading STCH section ======");
      var version = br.ReadInt32();
      validateVersion("STCH", version);       }    // TODO
//=======================================================================================
    public void readRPOS(BinaryReader br)    {
      Console.WriteLine("====== Reading RPOS section ======");
      var version = br.ReadInt32();
      validateVersion("RPOS", version);       }    // TODO
//=======================================================================================
    public void readBBOX(BinaryReader br)    {
      Console.WriteLine("====== Reading BBOX section ======");
      var version = br.ReadInt32();
      validateVersion("BBOX", version);       }    // TODO
//=======================================================================================

		public void readBEND(BinaryReader br)		{
        Console.WriteLine("====== Reading BEND section ======");

        // 'INFO' section follows immediately.

        FourCC infoCC = new FourCC();
        
        if (!readFourCC(br, ref infoCC) || infoCC.ToString() != "INFO")
        {
            Console.WriteLine("Missing INFO section after BEND!");
            return;
        }

        // We only read and print these. This data has no other use.

        string info = "";

        var infoEntryCount = br.ReadInt32();

        for (int i = 0; i < infoEntryCount; ++i)
        {
          for (;;)
          {
            // Strings are separated by null bytes.
            char c = '\0';
            c = (char)br.ReadByte(); // readBytes(&c, 1);
            if (c == '\0')
            {
              break;
            }
            info = info + c;
          }
          Console.WriteLine(info);
          info = "";
        }
            Console.ReadLine();
		}
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو

    public Vec4b readColor(BinaryReader br)
    {
      Vec4b c = new Vec4b();
      c.X = br.ReadByte();
      c.Y = br.ReadByte();
      c.Z = br.ReadByte();
      c.W = br.ReadByte();
      return c;
    }
//==========================================================

    public Vec2 readTexCoord(BinaryReader br)
    {
      Vec2 t = new Vec2();
      t.U = br.ReadSingle();
      t.V = br.ReadSingle();
      return t;
    }
//==========================================================

    public Vec3 readVec3(BinaryReader br)
    {
      Vec3 v = new Vec3();
      v.X = br.ReadSingle();
      v.Y = br.ReadSingle();
      v.Z = br.ReadSingle();
      return v;
    }
//==========================================================

    public Vec4 readVec4(BinaryReader br)
    {

      Vec4 v = new Vec4();
      v.X = br.ReadSingle();
      v.Y = br.ReadSingle();
      v.Z = br.ReadSingle();
      v.W = br.ReadSingle();
      return v;
    }
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو

    public bool readFourCC(BinaryReader br, ref FourCC fcc)
    {
        if ((br.BaseStream.Position + 4) > br.BaseStream.Length) return false;
        if (br.BaseStream.Position == br.BaseStream.Length) return false; // End of file reached.

        fcc.c0 = br.ReadByte();
        fcc.c1 = br.ReadByte();
        fcc.c2 = br.ReadByte();
        fcc.c3 = br.ReadByte();

        return true;
    }
//==========================================================

    public void validateVersion(string sectName, int version)
    {
        if (versionOf(version) == 0)
        {
            throw new Exception("Got unexpected version " + version 
            + " for " + sectName + " section of ASP file \"" + srcFileName + "\"!");
        }
    }
//==========================================================
    
    public int versionOf(int v)
    {
        switch (v)
        {
            case 513 : return 12; 
            case 769 : return 13;
            case 2 : return 20;
            case 258 : return 21;
            case 514 : return 22;
            case 770 : return 23;
            case 1026 : return 24;
            case 1282 : return 25;
            case 4 : return 40;
            case 260 : return 41;
            case 5 : return 50;
            default: return 0;
        }
    }
//==========================================================

}

//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو

  public class FourCC // final
  {
      public byte /*char*/ c0, c1, c2, c3;
/*
      public override string ToString() {
        return string.Format(Encoding.Default.GetString(new byte[] {c0 , c1 , c2 , c3 })); }
        */

      public override string ToString() {
        return Encoding.UTF8.GetString(new byte[] {c0 , c1 , c2 , c3 }, 0, 4); }
  }
//==========================================================

  // Indexes into the shared corner (vertex) array for a face triangle.

  public class TriIndex
  {
      // Unlike the ASP models, Siege Nodes use 16bits indexes.
      public int[] index = new int[3];
  }
//==========================================================
  // A model vertex, which can be thought of as a "corner"...
  // "Corner" is the term used in the 3DMax export scripts.

  public class WCornerInfo
  {
      public Vec3  pos = new Vec3();
      public Vec3  normal = new Vec3();
      public Vec4  weight = new Vec4();
      public Vec2  texCoord = new Vec2();
      public Vec4b color = new Vec4b();
      public Vec4b bone = new Vec4b();
  };
//==========================================================
  // A simpler model vertex (corner), without animation data.
  // This was probably used for static geometry.

  public class CornerInfo
  {
      public int vtxIndex;
      public Vec3  normal = new Vec3();
      public Vec2  texCoord = new Vec2();
      public Vec4b color = new Vec4b();
  };
//==========================================================

  public class MatInfo
  {
      public int textureIndex;
      public int faceSpan;
  };
//==========================================================

  public class FaceInfo
  {
      public List<int> cornerStart = new List<int>();
      public List<int> cornerSpan = new List<int>();
      public List<TriIndex> cornerIndex = new List<TriIndex>();
  };
//==========================================================

  public class BoneInfo
  {
      public int parentIndex;
      public int flags;
      public string name;
  };
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو

  public class SubMesh
  {
      public int textureCount = 0;
      public int vertexCount  = 0;
      public int cornerCount  = 0;
      public int faceCount    = 0;
      public int stitchCount  = 0;

      public List<MatInfo>     matInfo   = new List<MatInfo>();     // BSMM
      public List<Vec3>        positions = new List<Vec3>();        // BVTX
      public List<CornerInfo>  corners   = new List<CornerInfo>();  // BCRN
      public List<WCornerInfo> wCorners  = new List<WCornerInfo>(); // WCRN
      public FaceInfo          faceInfo  = new FaceInfo();          // BTRI
  };
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو

  public sealed class Vec2
  {
      public float U { get; set; }
      public float V { get; set; }
  }
// ========================================================

  public sealed class Vec3
  {
      public float X { get; set; }
      public float Y { get; set; }
      public float Z { get; set; }
  }
// ========================================================

  public sealed class Vec4
  {
      public float X { get; set; }
      public float Y { get; set; }
      public float Z { get; set; }
      public float W { get; set; }
  }
// ========================================================

  public sealed class Vec4b
  {
      public byte X { get; set; }
      public byte Y { get; set; }
      public byte Z { get; set; }
      public byte W { get; set; }
  }
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو

public static class Str
{
    public static string readString(this BinaryReader input)
    {
    List<byte> strBytes = new List<byte>();
    int b;
    while ((b = input.ReadByte()) != 0x00)
        strBytes.Add((byte)b);
    return Encoding.ASCII.GetString(strBytes.ToArray());
    }
}
//ووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووووو
