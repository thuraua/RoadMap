using System;
using System.Collections.Generic;

namespace RoadMap
{
    public class Route : IEquatable<Route>, IComparable<Route>
    {
        public string RID { get; set; }
        public string AbschnittBezeichnung { get; set; }
        public List<Route> children { get; set; }
        public Route route { get; set; }

        public Route(string rID, string abschnittBezeichnung)
        {
            RID = rID ?? throw new ArgumentNullException(nameof(rID));
            AbschnittBezeichnung = abschnittBezeichnung ?? throw new ArgumentNullException(nameof(abschnittBezeichnung));
            children = new List<Route>();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Route);
        }

        public bool Equals(Route other)
        {
            return other != null &&
                   RID == other.RID &&
                   AbschnittBezeichnung == other.AbschnittBezeichnung;
        }

        public override int GetHashCode()
        {
            var hashCode = -646879647;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AbschnittBezeichnung);
            return hashCode;
        }

        public static bool operator ==(Route route1, Route route2)
        {
            return EqualityComparer<Route>.Default.Equals(route1, route2);
        }

        public static bool operator !=(Route route1, Route route2)
        {
            return !(route1 == route2);
        }

        public override string ToString()
        {
            return RID + ", " + AbschnittBezeichnung;
        }

        public int CompareTo(Route other)
        {
            return int.Parse(RID.Substring(1)).CompareTo(int.Parse(other.RID.Substring(1)));
        }
    }
}