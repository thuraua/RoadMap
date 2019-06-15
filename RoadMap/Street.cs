using System;
using System.Collections.Generic;
using System.Windows;

namespace RoadMap
{
    /// <summary>
    /// Represents table 'Teilstrecke'
    /// </summary>
    public class Street : IEquatable<Street>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as Street);
        }

        public bool Equals(Street other)
        {
            return other != null &&
                   ID == other.ID;
        }

        public override int GetHashCode()
        {
            return 1213502048 + EqualityComparer<string>.Default.GetHashCode(ID);
        }

        public static bool operator ==(Street left, Street right)
        {
            return EqualityComparer<Street>.Default.Equals(left, right);
        }

        public static bool operator !=(Street left, Street right)
        {
            return !(left == right);
        }
    }
}