using System;
using System.Collections.Generic;
using System.IO;

namespace PTSharpCore
{

    // OBJ loader modified using source from:
    // Mathematical tools in Computer Graphics with C# implementations 
    // by Alexandre Hardy and Will-Hans Steeb
    // on Nov 2020
    class OBJ
    {
        static Dictionary<string, Material> matList = new Dictionary<string, Material>();

        internal static Mesh Load(string file, Material parent)
        {
            Vector[] v;
            Vector[] n;
            int nv, nn, nt;
            nv = nn = nt = 0;
            List<Triangle> triangles = new List<Triangle>();

            try
            {
                //count number of vertices, triangles and normals
                using (StreamReader sr = new StreamReader(file))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Length >= 2)
                        {
                            if (line[0] == 'v')
                            {
                                if (line[1] == 'n')
                                {
                                    //vertex normal
                                    nn++;
                                }
                                else
                                {
                                    //vertex
                                    nv++;
                                }
                            }
                            if (line[0] == 'f')
                            {
                                nt++;
                            }
                        }
                    }
                }
                //Create an extra normal for each triangle
                // for per triangle information
                n = new Vector[nn + nt];
                int basen = nn;
                v = new Vector[nv];
                nn = nv = nt = 0;
                char[] sep = new char[1];
                char[] subsep = new char[1];
                String[] parts, subparts;
                sep[0] = ' ';
                subsep[0] = '/';
                double[] d = new double[5];
                int[] vnum = new int[5];
                int[] nnum = new int[5];
                int c;
                int i;

                using (StreamReader sr = new StreamReader(file))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Length >= 2)
                        {
                            if (line[0] == 'v')
                            {
                                if (line[1] == 'n')
                                {
                                    //vertex normal
                                    parts = line.Split(sep);
                                    c = 0;
                                    for (i = 1; i < parts.GetLength(0); i++)
                                    {
                                        if (parts[i].Length > 0)
                                            d[c++] = Double.Parse(parts[i]);
                                    }
                                    n[nn] = new Vector(d[0], d[1], d[2], 0.0);
                                    nn++;
                                }
                                else
                                {
                                    //vertex
                                    parts = line.Split(sep);
                                    c = 0;
                                    for (i = 1; i < parts.GetLength(0); i++)
                                    {
                                        if (parts[i].Length > 0)
                                            d[c++] = Double.Parse(parts[i]);
                                    }
                                    v[nv] = new Vector(d[0], d[1], d[2], 1.0);
                                    nv++;
                                }
                            }
                            if (line[0] == 'f')
                            {
                                parts = line.Split(sep);
                                c = 0;
                                for (i = 1; i < parts.GetLength(0); i++)
                                {
                                    if (parts[i].Length > 0)
                                    {
                                        subparts = parts[i].Split(subsep);
                                        vnum[c] = Int32.Parse(subparts[0]) - 1;
                                        nnum[c] = Int32.Parse(subparts[2]) - 1;
                                        c++;
                                    }
                                }
                                for (i = 0; i < 3; i++)
                                {
                                    if (nnum[i] < 0) nnum[i] = basen + nt;
                                }
                                var t = new Triangle();
                                t.Material = parent;

                                n[basen + nt] = (v[vnum[1]] - v[vnum[0]]) ^ (v[vnum[2]] - v[vnum[0]]);
                                t.V1 = v[vnum[0]];
                                t.V2 = v[vnum[1]];
                                t.V3 = v[vnum[2]];
                                t.N1 = n[nnum[0]];
                                t.N2 = n[nnum[1]];
                                t.N3 = n[nnum[2]];
                                t.FixNormals();
                                triangles.Add(t);
                                nt++;

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return Mesh.NewMesh(triangles.ToArray());
        }

        public static void LoadMTL(string path, Material parent)
        {
            Console.WriteLine("Loading MTL:" + path);
            var parentCopy = parent;
            var material = parentCopy;
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
            }
            using (StreamReader streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    string[] words = streamReader.ReadLine().Split(' ');
                    switch (words[0])
                    {
                        case "newmtl":
                            parentCopy = parent;
                            material = parentCopy;
                            matList[words[1]] = material;
                            break;
                        case "Ke":
                            var max = Math.Max(Math.Max(float.Parse(words[1]), float.Parse(words[2])), float.Parse(words[3]));
                            if (max > 0)
                            {
                                material.Color = new Colour(float.Parse(words[1]) / max, float.Parse(words[2]) / max, float.Parse(words[3]) / max);
                                material.Emittance = max;
                            }
                            break;
                        case "Kd":
                            material.Color = new Colour(float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
                            break;
                        case "map_Kd":
                            Console.WriteLine("map_Kd: " + Directory.GetCurrentDirectory() + "\\" + words[1]);
                            var kdmap = Directory.GetCurrentDirectory() + "\\" + words[1];
                            material.Texture = ColorTexture.GetTexture(kdmap);
                            break;
                        case "map_bump":
                            Console.WriteLine("map_bump: " + Directory.GetCurrentDirectory() + "\\" + words[3]);
                            var bumpmap = Directory.GetCurrentDirectory() + "\\" + words[3];
                            material.NormalTexture = ColorTexture.GetTexture(bumpmap).Pow(1 / 2.2);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
