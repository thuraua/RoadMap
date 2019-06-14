using System.Windows;

namespace RoadMap
{
    /// <summary>
    /// Represents table 'Teilstrecke'
    /// </summary>
    public class Street
    {
        public string ID { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public int TransportNr { get; set; }
        public Point FromPoint { get; set; }
        public Point ToPoint { get; set; }

        /// <summary>
        /// Full constructor, required for drawing
        /// </summary>
        /// <param name="id"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="transportNr"></param>
        /// <param name="fromPoint"></param>
        /// <param name="toPoint"></param>
        public Street(string id, string from, string to, int transportNr, Point fromPoint, Point toPoint)
        {
            ID = id;
            From = from;
            To = to;
            TransportNr = transportNr;
            FromPoint = fromPoint;
            ToPoint = toPoint;
        }

        /// <summary>
        /// Lite constructor, only for street highlighting and description in datagrid
        /// </summary>
        /// <param name="id"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public Street(string id, string from, string to)
        {
            ID = id;
            From = from;
            To = to;
        }

        public override string ToString()
        {
            return ID + ", From: " + From + ", To:" + To;
        }
    }
}