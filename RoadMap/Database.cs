using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Windows;

namespace RoadMap
{
    public class Database
    {
        public string IP { get; set; } = "212.152.179.117"; //"212.152.179.117" "192.168.128.152"
        private OracleConnection connection;
        private static Database database;
        private OracleTransaction transaction;
        private readonly string StreetsSelect = "SELECT teilstrecke.ID, von_Ort, bis_Ort, transportnr, t.X, t.Y FROM teilstrecke, TABLE(SDO_UTIL.GETVERTICES(street)) t";
        private readonly string StreetsOfRouteSelect = "SELECT rid2 id, teilstrecke.von_ort von, teilstrecke.bis_ort bis FROM routennetz LEFT JOIN teilstrecke ON routennetz.rid2 = teilstrecke.id WHERE connect_by_isleaf = 1 connect by prior rid2 = rid1 start with rid1 = :rid";
        private readonly string StreetsOfTransportSelect = "SELECT teilstrecke.ID, von_Ort, bis_Ort, transportnr, t.X, t.Y FROM teilstrecke, TABLE(SDO_UTIL.GETVERTICES(street)) t WHERE transportnr = :tnr ";
        private readonly string TransportsSelect = "SELECT tnr, rstatus, transport.rid, abschnitt_bezeichnung FROM transport INNER JOIN route ON transport.rid = route.RID";
        private readonly string CountStreetHasAlreadyTransportNr = "SELECT COUNT(*) result from teilstrecke where transportnr is not null and id IN ";
        private readonly string SelectForUpdate = "SELECT * from teilstrecke where id in (xxx) for update NOWAIT";
        private readonly string TransportInsert = "INSERT INTO transport VALUES(transport_seq.nextval, :rid, 1)";
        private readonly string StreetsUpdate = "UPDATE teilstrecke SET transportnr = transport_seq.currval WHERE id IN (xxx)";
        private readonly string TransportUpdate = "UPDATE transport SET rstatus = 0 WHERE tnr = :tnr";
        private readonly string StreetsRemoveTransportNr = "UPDATE teilstrecke SET transportnr = '' WHERE id IN (xxx)";
        private readonly string RoutesSelect = "SELECT DISTINCT rid1, route.ABSCHNITT_BEZEICHNUNG from routennetz INNER JOIN route on route.RID = routennetz.RID1";
        private readonly string RouteByIdSelect = "SELECT rid, route.ABSCHNITT_BEZEICHNUNG from route where RID = :rid";

        private Database()
        {
            CreateConnection();
        }

        public void CreateConnection()
        {
            connection = new OracleConnection(@"user id=d4b26;password=d4b;data source=" +
                                                     "(description=(address=(protocol=tcp)" +
                                                     "(host=" + IP + ")(port=1521))(connect_data=" +
                                                     "(service_name=ora11g)))");
            connection.Open();
        }

        public static Database GetInstance()
        {
            if (database == null) database = new Database();
            return database;
        }

        public IList<Street> GetStreets()
        {
            SortedList<string, Street> streets = new SortedList<string, Street>();
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
                    if (!streets.ContainsKey(id))
                        streets.Add(id, new Street(id, von_Ort, bis_Ort, transportnr, givenPoint, givenPoint));
                    else
                        streets[id].ToPoint = givenPoint;
                }
            return streets.Values;
        }

        public IList<Street> GetStreetsOfRoute(Route route)
        {
            IList<Street> streets = new List<Street>();
            OracleCommand cmd = new OracleCommand(StreetsOfRouteSelect, connection);
            cmd.Parameters.Add("rid", route.RID);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                {
                    string id = reader["id"].ToString();
                    string von_Ort = reader["von"].ToString();
                    string bis_Ort = reader["bis"].ToString();
                    streets.Add(new Street(id, von_Ort, bis_Ort));
                }
            return streets;
        }

        public IList<Street> GetStreetsOfTransport(Transport selectedTransport, out double distance)
        {
            SortedList<string, Street> teilstreckes = new SortedList<string, Street>();
            distance = 0;
            OracleCommand cmd = new OracleCommand(StreetsOfTransportSelect, connection);
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
                        teilstreckes[id].ToPoint = givenPoint;
                }
            foreach (Street teilstrecke in teilstreckes.Values)
            {
                double distanceX = teilstrecke.ToPoint.X - teilstrecke.FromPoint.X < 0 ? (teilstrecke.ToPoint.X - teilstrecke.FromPoint.X) * (-1) : (teilstrecke.ToPoint.X - teilstrecke.FromPoint.X);
                double distanceY = teilstrecke.ToPoint.Y - teilstrecke.FromPoint.Y < 0 ? (teilstrecke.ToPoint.Y - teilstrecke.FromPoint.Y) * (-1) : (teilstrecke.ToPoint.Y - teilstrecke.FromPoint.Y);
                distance += Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
            }
            return teilstreckes.Values;
        }

        public IList<Transport> GetTransports()
        {
            IList<Transport> transports = new List<Transport>();
            OracleCommand cmd = new OracleCommand(TransportsSelect, connection);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                {
                    int tnr = Convert.ToInt32(reader["tnr"].ToString());
                    string rid = reader["rid"].ToString();
                    TransportStatus status = (TransportStatus)Convert.ToInt32(reader["rstatus"].ToString());
                    transports.Add(new Transport(tnr, GetRouteById(rid), status));
                }
            return transports;
        }

        private Route GetRouteById(string rid)
        {
            string description;
            OracleCommand cmd = new OracleCommand(RouteByIdSelect, connection);
            cmd.Parameters.Add(new OracleParameter("rid", rid));
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                description = reader["ABSCHNITT_BEZEICHNUNG"].ToString();
            }
            else throw new Exception("Could not find route with ID " + rid);
            return new Route(rid, description);
        }

        public void TryLockStreets(IList<Street> streetsToLock)
        {
            //look up if there isn't a transportNr somewhere
            string str = "(";
            foreach (Street street in streetsToLock) str += "'" + street.ID + "', ";
            str = str.Substring(0, str.Length - 2);
            str += ")";
            OracleCommand cmd = new OracleCommand(CountStreetHasAlreadyTransportNr + str, connection);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                int count = Convert.ToInt32(reader["result"]);
                if (count != 0) throw new Exception(count + " streets are not available due to another transport!");
            }
            else throw new Exception("Could not determine if all streets are free!");
            //try select for update --> fails if any other client has already locked a record
            str = "";
            foreach (Street street in streetsToLock) str += "'" + street.ID + "', ";
            str = str.Substring(0, str.Length - 2);
            transaction = connection.BeginTransaction();
            OracleCommand cmd1 = new OracleCommand(SelectForUpdate.Replace("xxx", str), connection);
            cmd1.ExecuteNonQuery();
        }

        public void InsertTransport(IList<Street> lockedStreets, Route route)
        {
            OracleCommand cmd = new OracleCommand(TransportInsert, connection);
            cmd.Parameters.Add(new OracleParameter("rid", route.RID));
            cmd.ExecuteNonQuery();
            string str = "";
            foreach (Street street in lockedStreets) str += "'" + street.ID + "', ";
            str = str.Substring(0, str.Length - 2);
            OracleCommand cmd1 = new OracleCommand(StreetsUpdate.Replace("xxx", str), connection);
            cmd1.ExecuteNonQuery();
            transaction.Commit();
        }

        public void FinishTransport(Transport selectedTransport, IList<Street> streets)
        {
            OracleCommand cmd = new OracleCommand(TransportUpdate, connection);
            cmd.Parameters.Add(new OracleParameter("tnr", selectedTransport.TNR));
            cmd.ExecuteNonQuery();
            string str = "";
            foreach (Street street in streets) str += "'" + street.ID + "', ";
            str = str.Substring(0, str.Length - 2);
            OracleCommand cmd1 = new OracleCommand(StreetsRemoveTransportNr.Replace("xxx", str), connection);
            cmd1.ExecuteNonQuery();
        }

        public IList<Route> GetRoutes()
        {
            IList<Route> routes = new List<Route>();
            OracleCommand cmd = new OracleCommand(RoutesSelect, connection);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                {
                    string id = reader["rid1"].ToString();
                    string description = reader["ABSCHNITT_BEZEICHNUNG"].ToString();
                    routes.Add(new Route(id, description));
                }
            return routes;
        }
    }
}