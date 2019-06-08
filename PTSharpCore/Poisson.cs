using System;
using System.Collections.Generic;
using System.Linq;

namespace PTSharpCore
{
    class Poisson
    {
        double r, size;
        Dictionary<Vector, Vector> cells;
        
        Poisson(double r, double size, Dictionary<Vector, Vector> hmap)
        {
            this.r = r;
            this.size = size;
            this.cells = hmap;
        }

        Poisson newPoissonGrid(double r)
        {
            double gridsize = r / Math.Sqrt(2);
            return new Poisson(r, gridsize, new Dictionary<Vector, Vector>());
        }
        
        Vector normalize(Vector v)
        {
            var i = Math.Floor(v.X / size);
            var j = Math.Floor(v.Y / size);
            return new Vector(i, j, 0);
        }

        bool insert(Vector v)
        {
            Vector n = normalize(v);

            for (double i = n.X - 2; i < n.X + 3; i++)
            {
                for (double j = n.Y - 2; j < n.Y + 3; j++)
                {
                    if(cells.ContainsKey(new Vector(i, j, 0)))
                    {
                        Vector m = cells[new Vector(i, j, 0)];

                        if(Math.Sqrt(Math.Pow(m.X-v.X, 2) + Math.Pow(m.Y-v.Y, 2)) < r)
                        {
                            return false;
                        }
                    }
                }
            }
            cells[n] = v;
            return true;
        }
        
        Vector[] PoissonDisc(double x1, double y1, double x2, double y2, double r, int n)
        {
            var rand = new Random();
            List<Vector> result;
            var x = x1 + (x2 - x1) / 2;
            var y = y1 + (y2 - y1) / 2;
            var v = new Vector(x, y, 0);
            List<Vector> active = new List<Vector>();
            var grid = newPoissonGrid(r);
            grid.insert(v);
            active.Add(v);
            result = active;

            while (active.Count != 0)
            {
                // Need non-negative random integers
                // must be a non-negative pseudo-random number in [0,n).
                int index = rand.Next(active.Count);
                Vector point = active.ElementAt(index);
                bool ok = false;
                for (int i = 0; i < n; i++)
                {
                    double a = rand.NextDouble() * 2 * Math.PI;
                    double d = rand.NextDouble() * r + r;
                    x = point.X + Math.Cos(a) * d;
                    y = point.Y + Math.Sin(a) * d;
                    if (x < x1 || y < y1 || x > x2 || y > y2)
                    {
                        continue;
                    }
                    v = new Vector(x, y, 0);
                    if (!grid.insert(v))
                    {
                        continue;
                    }
                    result.Add(v);
                    active.Add(v);
                    ok = true;
                    break;
                }
                
                if (!ok)
                {
                    active.Add(active.ElementAt(active.Count));
                }
            }
            return result.ToArray();
        }
    }
}
