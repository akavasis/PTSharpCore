using System;

namespace PTSharpCore
{
    class Cube : IShape
    {
        internal Vector Min;
        internal Vector Max;
        internal Material Material;
        internal Box Box;

        Cube(Vector min, Vector max, Material material, Box box)
        {
            Min = min;
            Max = max;
            Material = material;
            Box = box;
        }

        internal static Cube NewCube(Vector min, Vector max, Material material)
        {
            Box box = new Box(min, max);
            return new Cube(min, max, material, box);
        }

        void IShape.Compile() { }

        Box IShape.BoundingBox() => Box;

        Hit IShape.Intersect(Ray r)
        {
            var n = Min.Sub(r.Origin).Div(r.Direction);
            var f = Max.Sub(r.Origin).Div(r.Direction);
            (n,f) = (n.Min(f),n.Max(f));
            var t0 = Math.Max(Math.Max(n.X, n.Y), n.Z);
            var t1 = Math.Min(Math.Min(f.X, f.Y), f.Z);
            if (t0 > 0 && t0 < t1)
                return new Hit(this, t0, null);
            return Hit.NoHit;
        }

        Vector IShape.UV(Vector p)
        {
            p = p.Sub(Min).Div(Max.Sub(Min));
            return new Vector(p.X, p.Z, 0);
        }

        Material IShape.MaterialAt(Vector p) => Material;

        Vector IShape.NormalAt(Vector p)
        {
            switch (p)
            {
                case Vector d when p.X < Min.X + Util.EPS:
                    return new Vector(-1, 0, 0);
                case Vector e when p.X > Max.X - Util.EPS:
                    return new Vector(1, 0, 0);
                case Vector d when p.Y < Min.Y + Util.EPS:
                    return new Vector(0, -1, 0);
                case Vector d when p.Y > Max.Y - Util.EPS:
                    return new Vector(0, 1, 0);
                case Vector d when p.Z < Min.Z + Util.EPS:
                    return new Vector(0, 0, -1);
                case Vector d when p.Z > Max.Z - Util.EPS:
                    return new Vector(0, 0, 1);
            }
            return new Vector(0, 1, 0);
        }
        
        Mesh CubeMesh()
        {
            var a = Min;
            var b = Max;
            var z = new Vector();
            var m = Material;
            var v000 = new Vector(a.X, a.Y, a.Z);
            var v001 = new Vector(a.X, a.Y, b.Z);
            var v010 = new Vector(a.X, b.Y, a.Z);
            var v011 = new Vector(a.X, b.Y, b.Z);
            var v100 = new Vector(b.X, a.Y, a.Z);
            var v101 = new Vector(b.X, a.Y, b.Z);
            var v110 = new Vector(b.X, b.Y, a.Z);
            var v111 = new Vector(b.X, b.Y, b.Z);
            Triangle[] triangles = {
                Triangle.NewTriangle(v000, v100, v110, z, z, z, m),
                Triangle.NewTriangle(v000, v110, v010, z, z, z, m),
                Triangle.NewTriangle(v001, v101, v111, z, z, z, m),
                Triangle.NewTriangle(v001, v111, v011, z, z, z, m),
                Triangle.NewTriangle(v000, v100, v101, z, z, z, m),
                Triangle.NewTriangle(v000, v101, v001, z, z, z, m),
                Triangle.NewTriangle(v010, v110, v111, z, z, z, m),
                Triangle.NewTriangle(v010, v111, v011, z, z, z, m),
                Triangle.NewTriangle(v000, v010, v011, z, z, z, m),
                Triangle.NewTriangle(v000, v011, v001, z, z, z, m),
                Triangle.NewTriangle(v100, v110, v111, z, z, z, m),
                Triangle.NewTriangle(v100, v111, v101, z, z, z, m)
            };
            return Mesh.NewMesh(triangles);
        }
    }
}
