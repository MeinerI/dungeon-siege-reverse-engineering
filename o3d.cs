//Console.WriteLine("*");

//����������������������������������������������������������������������������������������������
  using System; using System.IO; using System.Linq; using System.Text; using System.Collections; 
  using System.Collections.Generic; using System.Text.RegularExpressions; using System.Globalization; 
//����������������������������������������������������������������������������������������������

sealed class Test
{
    static uint vertexCount; 
    static uint faceCount;

    public static void Main()
    {
        SearchOption SOAD = SearchOption.AllDirectories;
        string[] filesName = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.o3d",  SOAD); 
        System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

        foreach (var fileIn in filesName)
        {
						using (BinaryReader br = new BinaryReader(File.Open(fileIn, FileMode.Open)))
						{
								using (StreamWriter sw = new StreamWriter(fileIn + ".obj"))
								{
                    O3dModel o3d = new O3dModel();

                    vertexCount = br.ReadUInt32(); 
                    faceCount   = br.ReadUInt32(); 
                //  Two 32bit words of unknown contents follow.
                    br.ReadInt32();
                    br.ReadInt32();

                    o3d.vertexes = new o3d_vertex[vertexCount]; // malloc(vertexCount * sizeof(o3d.vertexes[0]))
                //  fread(o3d.vertexes, sizeof(o3d.vertexes[0]), vertexCount, fileIn); // == vertexCount
                    for (int i = 0; i < vertexCount; i++)
                    {
                      o3d.vertexes[i] = new o3d_vertex();

                      o3d.vertexes[i].x = br.ReadSingle();
                      o3d.vertexes[i].y = br.ReadSingle();
                      o3d.vertexes[i].z = br.ReadSingle();
                    }

                    o3d.faces = new o3d_face[faceCount]; // malloc(faceCount * sizeof(o3d.faces[0]));
                //  fread(o3d.faces, sizeof(o3d.faces[0]), faceCount, fileIn); // == faceCount

                    for (int i = 0; i < faceCount; i++)
                    {
                      o3d.faces[i] = new o3d_face();
                      
                      o3d.faces[i].color = new o3d_color();
                      o3d.faces[i].color.b = br.ReadByte();
                      o3d.faces[i].color.g = br.ReadByte();
                      o3d.faces[i].color.r = br.ReadByte();
                      o3d.faces[i].color.a = br.ReadByte();

                      for (int ii = 0; ii < 4; ii++)
                      {
                          o3d.faces[i].texCoords[ii] = new o3d_texcoord();
                          o3d.faces[i].texCoords[ii].u = br.ReadSingle();
                          o3d.faces[i].texCoords[ii].v = br.ReadSingle();
                      }
                      
                      for (int ii = 0; ii < 4; ii++)
                      {
                          o3d.faces[i].index[ii] = br.ReadUInt16();
                      }

                      o3d.faces[i].unknown = br.ReadUInt32();

                      o3d.faces[i].texNumber = br.ReadUInt16();
                    }
								}
            }
        } // foreach (var file in filesName)
    } // public static void Main()
} // sealed class Test {

//����������������������������������������������������������������������������������������������
//����������������������������������������������������������������������������������������������
//����������������������������������������������������������������������������������������������

public class O3dModel
{
    public o3d_vertex[]  vertexes;
    public o3d_face[]    faces;
}

public class o3d_vertex 
{
  public float x, y, z; // Model vertex position (XYZ)
}

// ======================================================== 
 
public class o3d_texcoord 
{
  public float u, v; // Model texture coordinates (UV).
}

// ======================================================== 

public class o3d_color 
{
  public byte b, g, r, a; // Model vertex color (BGRA-U8)
}

// ======================================================== 
 
public class o3d_face // �����/������� (����������� ��� ���������������)
{
  public o3d_color color = new o3d_color();  //  BGR(A) face color, ������.
  public o3d_texcoord[] texCoords = new o3d_texcoord[4];
  public ushort[] index = new ushort[4];
  public uint unknown;
  public ushort texNumber;
}

// ======================================================== 
