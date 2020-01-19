using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADPolyline
{ 
    public class PolylineModel
    {
        public PolylineModel()
        {
            Vertexes = new List<Vertex>();
            Vertexes.Add(new Vertex {X = 0, Y = 0, Bulge = 0});
            IsSmoothing = false;
            IsClose = false;
            Thickness = 1;
            Color = (255, 255, 255);
        }

        public bool IsSmoothing { get; set; }
        public bool IsClose { get; set; }
        public double Thickness { get; set; }
        public (byte R, byte G, byte B) Color { get; set; }
        public List<Vertex> Vertexes { get; set; }

        public bool IsValid
        {
            get
            {
                if (Vertexes.Count() <= 1)
                    return false;
                return true;
            }
        }

        public class Vertex
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Bulge { get; set; }
        }
    }
}
