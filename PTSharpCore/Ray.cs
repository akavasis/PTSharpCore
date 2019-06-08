using System;

namespace PTSharpCore
{
    public class Ray
    {
        internal Vector Origin, Direction;
        internal bool reflect;

        internal Ray(Vector Origin_, Vector Direction_)
        {
            Origin = Origin_;
            Direction = Direction_;
        }

        internal Vector Position(double t) => Origin.Add(Direction.MulScalar(t));

        internal Ray Reflect(Ray i) => new Ray(Origin, Direction.Reflect(i.Direction));

        public Ray Refract(Ray i, double n1, double n2) => new Ray(Origin, Direction.Refract(i.Direction, n1, n2));

        public double Reflectance(Ray i, double n1, double n2) => Direction.Reflectance(i.Direction, n1, n2);

        public Ray WeightedBounce(double u, double v, Random rand)
        {
            var radius = Math.Sqrt(u);
            var theta = 2 * Math.PI * v;
            var s = Direction.Cross(Vector.RandomUnitVector(rand)).Normalize();
            var t = Direction.Cross(s);
            var d = new Vector();
            d = d.Add(s.MulScalar(radius * Math.Cos(theta)));
            d = d.Add(t.MulScalar(radius * Math.Sin(theta)));
            d = d.Add(Direction.MulScalar(Math.Sqrt(1 - u)));
            return new Ray(Origin, d);
        }

        public Ray ConeBounce(double theta, double u, double v, Random rand)
        {
           return new Ray(Origin, Util.Cone(Direction, theta, u, v, rand));
        }

        public (Ray, bool, double) Bounce(HitInfo info, double u, double v, BounceType bounceType, Random rand)
        {
            var n = info.Ray;
            var material = info.material;
            
            (var n1, var n2) = (1.0, material.Index);
            if (info.inside)
            {
                (n1, n2) = (n2, n1);
            }
            double p;
            if (material.Reflectivity >= 0)
            {
                p = material.Reflectivity;
            }
            else
            {
                p = n.Reflectance(this, n1, n2);
            }

            //bool reflect = false;
            if (bounceType == BounceType.BounceTypeAny)
            {
                reflect = rand.NextDouble() < p;
            } else if (bounceType == BounceType.BounceTypeDiffuse)
            {
                reflect = false;
            } else if (bounceType == BounceType.BounceTypeSpecular)
            {
                reflect = true;
            }

            if (reflect)
            {
                var reflected = n.Reflect(this);
                return (reflected.ConeBounce(material.Gloss, u, v, rand), true, p);
            }
            else if (material.Transparent)
            {
                var refracted = n.Refract(this, n1, n2);
                refracted.Origin = refracted.Origin.Add(refracted.Direction.MulScalar(1e-4));
                return (refracted.ConeBounce(material.Gloss, u, v, rand), true, 1 - p);
            }
            else
            {
                return (n.WeightedBounce(u, v, rand), false, 1 - p);
            }
        }
    }
}
