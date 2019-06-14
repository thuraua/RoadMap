using System.Windows;

namespace RoadMap
{
    public class Street
    {
        public string ID { get; set; }
        public string vonOrt { get; set; }
        public string bisOrt { get; set; }
        public int transportNr { get; set; }
        public Point vonPoint { get; set; }
        public Point bisPoint { get; set; }

        public Street(string iD, string vonOrt, string bisOrt, int transportNr, Point vonPoint, Point bisPoint)
        {
            ID = iD;
            this.vonOrt = vonOrt;
            this.bisOrt = bisOrt;
            this.transportNr = transportNr;
            this.vonPoint = vonPoint;
            this.bisPoint = bisPoint;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}