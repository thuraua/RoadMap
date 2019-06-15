using System;
using System.Collections.Generic;

namespace RoadMap
{
    public class Route : IEquatable<Route>
    {
        public string RID { get; set; }
        public string AbschnittBezeichnung { get; set; }

        public Route(string rID, string abschnittBezeichnung)
        {
            RID = rID;
            AbschnittBezeichnung = abschnittBezeichnung;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Route);
        }

        public bool Equals(Route other)
        {
            return other != null && RID == other.RID;
        }

        public override int GetHashCode()
        {
            var hashCode = -646879647;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AbschnittBezeichnung);
            return hashCode;
        }

        public override string ToString()
        {
            return RID + ", " + AbschnittBezeichnung;
        }
    }
}