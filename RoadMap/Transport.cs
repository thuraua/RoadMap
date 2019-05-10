using System;
using System.Collections.Generic;

namespace RoadMap
{
    public class Transport
    {
        public int TNR { get; set; }
        public string RID { get; set; }
        public TransportStatus Status { get; set; }
        public string Name { get; set; }

        public Transport(int tNR, string rID, TransportStatus status, string name) : this(tNR, rID, status)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public Transport(int tNR, string rID, TransportStatus status)
        {
            TNR = tNR;
            RID = rID ?? throw new ArgumentNullException(nameof(rID));
            Status = status;
        }

        public override bool Equals(object obj)
        {
            var transport = obj as Transport;
            return transport != null &&
                   TNR == transport.TNR &&
                   RID == transport.RID &&
                   Status == transport.Status;
        }

        public override int GetHashCode()
        {
            var hashCode = 1205930254;
            hashCode = hashCode * -1521134295 + TNR.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RID);
            hashCode = hashCode * -1521134295 + Status.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public enum TransportStatus
    {
        Completed, Active
    }
}