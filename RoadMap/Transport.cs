using System;
using System.Collections.Generic;

namespace RoadMap
{
    public class Transport
    {
        public int TNR { get; set; }
        //public string RID { get; set; }
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
    }

    public enum TransportStatus
    {
        Completed, Active
    }
}