using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Windows;

namespace RoadMap
{
    public class Database
    {
        public string IP { get; } = "192.168.128.152"; //"212.152.179.117" "192.168.128.152"
        private OracleConnection connection;
        private static Database database;
        private OracleCommand cmd;
        private OracleTransaction trx;
        private readonly string StreetsSelect = "SELECT teilstrecke.ID, von_Ort, bis_Ort, transportnr, t.X, t.Y FROM teilstrecke, TABLE(SDO_UTIL.GETVERTICES(street)) t";
        private readonly string TeilstreckenOfTransportSelect = "SELECT teilstrecke.ID, von_Ort, bis_Ort, transportnr, t.X, t.Y FROM teilstrecke, TABLE(SDO_UTIL.GETVERTICES(street)) t WHERE transportnr = :tnr ";
        private readonly string TransporteSelect = "SELECT tnr, rstatus, transport.rid, abschnitt_bezeichnung FROM transport INNER JOIN route ON transport.rid = route.RID";
        private readonly string RoutenSelect = "select distinct rid1, route.ABSCHNITT_BEZEICHNUNG from routennetz INNER JOIN route on route.RID = routennetz.RID1";

        private Database()
        {
            connection = new OracleConnection(@"user id=d4b26;password=d4b;data source=" +
                                                     "(description=(address=(protocol=tcp)" +
                                                     "(host=" + IP + ")(port=1521))(connect_data=" +
                                                     "(service_name=ora11g)))");
            connection.Open();
            cmd = new OracleCommand();
            cmd.Connection = connection;
            //trx = conn.BeginTransaction();
            //cmd.Transaction = trx;
        }

        public static Database GetInstance()
        {
            if (database == null) database = new Database();
            return database;
        }

        public IList<Transport> ReadTransports()
        {
            IList<Transport> transports = new List<Transport>();
            OracleCommand cmd = new OracleCommand(TransporteSelect, connection);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                {
                    int tnr = Convert.ToInt32(reader["tnr"].ToString());
                    string rid = reader["rid"].ToString();
                    //string name = reader["abschnitt_bezeichnung"].ToString();
                    TransportStatus status = (TransportStatus)Convert.ToInt32(reader["rstatus"].ToString());
                    transports.Add(new Transport(tnr, getRoute(rid), status));
                }
            return transports;
        }

        public IList<Street> GetStreets()
        {
            SortedList<string, Street> teilstreckes = new SortedList<string, Street>();
            OracleCommand cmd = new OracleCommand(StreetsSelect, connection);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                {
                    string id = reader["id"].ToString();
                    string von_Ort = reader["von_Ort"].ToString();
                    string bis_Ort = reader["bis_Ort"].ToString();
                    int transportnr = Convert.ToInt32(reader["transportnr"].ToString() == "" ? "0" : reader["transportnr"].ToString());
                    Point givenPoint = new Point(Convert.ToDouble(reader["X"]), Convert.ToDouble(reader["Y"]));
                    if (!teilstreckes.ContainsKey(id))
                        teilstreckes.Add(id, new Street(id, von_Ort, bis_Ort, transportnr, givenPoint, givenPoint));
                    else
                        teilstreckes[id].bisPoint = givenPoint;
                }
            return teilstreckes.Values;
        }

        public Route getRoute(string rid)
        {
            Route route = null;
            List<Route> routes = GetRoutes();
            foreach (Route r in routes)
            {
                if (r.RID == rid)
                    route = r;
            }
            return route;
        }

        public double getFullLength(Route route)
        {
            double length = 0;
            string regex = _childrenToRegex(route);
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT SUM(SDO_GEOM.SDO_LENGTH(t.street, m.diminfo)) as length FROM teilstrecke t, user_sdo_geom_metadata m WHERE m.table_name = 'TEILSTRECKE' AND m.column_name = 'STREET' AND REGEXP_LIKE (ID, :regex)";
            cmd.Parameters.Add("regex", regex);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    length = double.Parse(reader["length"].ToString());
                }
            }
            return Math.Round(length, 2); // 2 decimal point
        }

        private string _childrenToRegex(Route route) // takes children and generates regex for query
        {
            string regex = ""; // should look something like this: 'T1$|T2$|T3$'
            for (int i = 0; i < route.children.Count; i++)
            {
                if (i != 0)
                    regex += "|";
                regex += route.children[i].RID + "$";
                //regex += route.children.ElementAt(i).RId + "$";
            }
            return regex;
        }

        public List<Route> GetRoutes()
        {
            List<Route> routes = new List<Route>();
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT rnetz.rid1 as parent, rnetz.rid2 as child, r1.abschnitt_bezeichnung as bez1, r2.abschnitt_bezeichnung as bez2 FROM routennetz rnetz INNER JOIN route r1 ON r1.RID = rnetz.RID1 INNER JOIN route r2 ON r2.RID = rnetz.RID2 CONNECT BY PRIOR rnetz.RID2 = rnetz.RID1"; ;
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string parent = reader["parent"].ToString();
                    string child = reader["child"].ToString();
                    string bezParent = reader["bez1"].ToString();
                    string bezChild = reader["bez2"].ToString();

                    Route parRoute = routes.Find(r => r.RID == parent);
                    if (parRoute == null)
                    {
                        parRoute = new Route(parent, bezParent);
                        parRoute.children.Add(new Route(child, bezChild));
                        routes.Add(parRoute);
                    }
                    else
                    {
                        Route childRoute = parRoute.children.Find(r => r.RID == child);
                        if (childRoute == null)
                            parRoute.children.Add(new Route(child, bezChild));
                    }
                }
            }

            for (int i = routes.Count - 1; i >= 0; i--)
            {
                Route route = routes[i];
                for (int j = route.children.Count - 1; j >= 0; j--)
                {
                    Route childRoute = route.children[j];
                    if (childRoute.RID.StartsWith("R"))
                    {
                        Route r1 = routes.Find(r => r.RID == childRoute.RID);
                        route.children.AddRange(r1.children);
                        route.children.Remove(childRoute);
                    }
                }
            }

            return routes;
        }

        public void AddTransport(Route route)
        {
            Tuple<bool, string> check = IsAllowedToInsert(route);
            if (!check.Item1)
            {
                throw new Exception("Cannot insert transport because a section or the full route is already closed (" + check.Item2 + ")");
            }
            cmd.Parameters.Clear();
            cmd.CommandText = "INSERT INTO transport VALUES(transport_seq.nextval, :rid, 1)";
            cmd.Parameters.Add("rid", route.RID);
            cmd.ExecuteNonQuery();
            lockStreets(route);
        }

        private Tuple<bool, string> IsAllowedToInsert(Route route)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT ID FROM teilstrecke WHERE REGEXP_LIKE(ID, :regex) AND transportnr IS NOT NULL";
            cmd.Parameters.Add("regex", _childrenToRegex(route));
            OracleDataReader reader = cmd.ExecuteReader();
            bool isAllowedToInsert = true;
            string closedRoutes = "";
            if (reader.HasRows)
            {
                isAllowedToInsert = false;
                while (reader.Read())
                {
                    if (closedRoutes != "")
                        closedRoutes += ", ";
                    closedRoutes += reader["id"].ToString();
                }
            }
            return Tuple.Create<bool, string>(isAllowedToInsert, closedRoutes);
        }

        private void lockStreets(Route route)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE teilstrecke SET transportnr = transport_seq.currval WHERE REGEXP_LIKE (ID, :regex)";
            cmd.Parameters.Add("regex", _childrenToRegex(route));
            cmd.ExecuteNonQuery();
        }

        public void finishTransport(Transport t)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE transport SET rstatus = 0 WHERE TNR = :tnr";
            cmd.Parameters.Add("tnr", t.TNR);
            cmd.ExecuteNonQuery();
        }

        public void unlockStreets(Transport t)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE teilstrecke SET transportnr = NULL WHERE REGEXP_LIKE (ID, :regex)";
            cmd.Parameters.Add("regex", _childrenToRegex(t.Route));
            cmd.ExecuteNonQuery();
        }

        public void rollback()
        {
            trx.Rollback();
        }

        public static Database NewInstance()
        {
            if (database == null)
                database = new Database();
            return database;
        }

        public IList<Street> ReadTeilstreckenOfTransport(Transport selectedTransport, out double distance)
        {
            SortedList<string, Street> teilstreckes = new SortedList<string, Street>();
            distance = 0;
            OracleCommand cmd = new OracleCommand(TeilstreckenOfTransportSelect, connection);
            cmd.Parameters.Add(new OracleParameter("tnr", selectedTransport.TNR));
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                {
                    string id = reader["id"].ToString();
                    string von_Ort = reader["von_Ort"].ToString();
                    string bis_Ort = reader["bis_Ort"].ToString();
                    int transportnr = Convert.ToInt32(reader["transportnr"].ToString() == "" ? "0" : reader["transportnr"].ToString());
                    Point givenPoint = new Point(Convert.ToDouble(reader["X"]), Convert.ToDouble(reader["Y"]));
                    if (!teilstreckes.ContainsKey(id))
                        teilstreckes.Add(id, new Street(id, von_Ort, bis_Ort, transportnr, givenPoint, givenPoint));
                    else
                        teilstreckes[id].bisPoint = givenPoint;
                }
            foreach (Street teilstrecke in teilstreckes.Values)
            {
                double distanceX = teilstrecke.bisPoint.X - teilstrecke.vonPoint.X < 0 ? (teilstrecke.bisPoint.X - teilstrecke.vonPoint.X) * (-1) : (teilstrecke.bisPoint.X - teilstrecke.vonPoint.X);
                double distanceY = teilstrecke.bisPoint.Y - teilstrecke.vonPoint.Y < 0 ? (teilstrecke.bisPoint.Y - teilstrecke.vonPoint.Y) * (-1) : (teilstrecke.bisPoint.Y - teilstrecke.vonPoint.Y);
                distance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
            }
            return teilstreckes.Values;
        }

        #region unused

        //public Tuple<List<Transport>, List<Transport>> ReadTransports()
        //{
        //    List<Transport> transports = new List<Transport>();
        //    cmd.Parameters.Clear();
        //    cmd.CommandText = "SELECT tnr, rid, rstatus FROM transport ORDER BY tnr";
        //    OracleDataReader reader = cmd.ExecuteReader();
        //    if (reader.HasRows)
        //    {
        //        while (reader.Read())
        //        {
        //            int TNr = int.Parse(reader["TNr"].ToString());
        //            string RId = reader["RId"].ToString();
        //            TransportStatus RStatus = (TransportStatus)Convert.ToInt32(reader["rstatus"].ToString());
        //            transports.Add(new Transport(TNr, getRoute(RId), RStatus));
        //        }
        //    }
        //    return Tuple.Create<List<Transport>, List<Transport>>(transports.FindAll(t => t.Status == TransportStatus.Active), transports.FindAll(t => t.Status == 0));
        //}

        //public IList<Teilstrecke> ReadTeilstrecken()
        //{
        //    SortedList<string, Teilstrecke> teilstreckes = new SortedList<string, Teilstrecke>();
        //    OracleCommand cmd = new OracleCommand(TeilstreckenSelect, connection);
        //    OracleDataReader reader = cmd.ExecuteReader();
        //    if (reader.HasRows)
        //        while (reader.Read())
        //        {
        //            string id = reader["id"].ToString();
        //            string von_Ort = reader["von_Ort"].ToString();
        //            string bis_Ort = reader["bis_Ort"].ToString();
        //            int transportnr = Convert.ToInt32(reader["transportnr"].ToString() == "" ? "0" : reader["transportnr"].ToString());
        //            Point givenPoint = new Point(Convert.ToDouble(reader["X"]), Convert.ToDouble(reader["Y"]));
        //            if (!teilstreckes.ContainsKey(id))
        //                teilstreckes.Add(id, new Teilstrecke(id, von_Ort, bis_Ort, transportnr, givenPoint, givenPoint));
        //            else
        //                teilstreckes[id].bisPoint = givenPoint;
        //        }
        //    return teilstreckes.Values;
        //}

        #endregion

    }
}