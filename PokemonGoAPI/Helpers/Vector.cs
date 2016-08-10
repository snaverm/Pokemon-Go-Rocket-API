using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Helpers
{
    public class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void NormalizeVector(double normalValue)
        {
            double length = Math.Sqrt((X * X) + (Y * Y) + (Z * Z)) / normalValue;
            X = Math.Abs(X / length);
            Y = Math.Abs(Y / length);
            Z = Math.Abs(Z / length);
        }

        public void Round(int places)
        {
            X = Math.Round(X, places);
            Y = Math.Round(Y, places);
            Z = Math.Round(Z, places);
        }
    }
}
