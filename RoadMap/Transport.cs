using System;
using System.Collections.Generic;

namespace RoadMap
{
    public class Transport : IEquatable<Transport>
    {
        public int TNR { get; set; }
        public Route Route { get; set; }
        public TransportStatus Status { get; set; }

        public Transport(int tNR, Route r, TransportStatus status)
        {
            TNR = tNR;
            Route = r;
            Status = status;
        }

        public override string ToString()
        {
            return "Transportnummer: " + TNR + ", Route: " + Route.RID + ", " + ((Status == TransportStatus.Active) ? "aktiv" : "finished");
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Transport);
        }

        public bool Equals(Transport other)
        {
            return other != null &&
                   TNR == other.TNR;
        }

        public override int GetHashCode()
        {
            return -680258025 + TNR.GetHashCode();
        }

        public static bool operator ==(Transport left, Transport right)
        {
            return EqualityComparer<Transport>.Default.Equals(left, right);
        }

        public static bool operator !=(Transport left, Transport right)
        {
            return !(left == right);
        }
    }

    public enum TransportStatus
    {
        Completed, Active
    }
}