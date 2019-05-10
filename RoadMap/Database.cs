using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Windows;

namespace RoadMap
{
    public class Database
    {
        public string IP { get; } = "212.152.179.117"; //"212.152.179.117" "192.168.128.152"
        private OracleConnection connection;
        private static Database database;
        private readonly string TeilstreckenSelect = "SELECT teilstrecke.ID, von_Ort, bis_Ort, transportnr, t.X, t.Y FROM teilstrecke, TABLE(SDO_UTIL.GETVERTICES(street)) t";
        private readonly string TransporteSelect = "SELECT tnr, rstatus, transport.rid, abschnitt_bezeichnung FROM transport INNER JOIN route ON transport.rid = route.RID";

        private Database()
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

        public IList<Teilstrecke> ReadTeilstrecken()
        {
            SortedList<string, Teilstrecke> teilstreckes = new SortedList<string, Teilstrecke>();
            OracleCommand cmd = new OracleCommand(TeilstreckenSelect, connection);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                {
                    string id = reader["id"].ToString();
                    string von_Ort = reader["von_Ort"].ToString();
                    string bis_Ort = reader["bis_Ort"].ToString();
                    int transportnr = Convert.ToInt32(reader["transportnr"].ToString()==""?"0": reader["transportnr"].ToString());
                    Point givenPoint = new Point(Convert.ToDouble(reader["X"]), Convert.ToDouble(reader["Y"]));
                    if (!teilstreckes.ContainsKey(id))                
                        teilstreckes.Add(id, new Teilstrecke(id, von_Ort, bis_Ort, transportnr, givenPoint, givenPoint));
                    else
                        teilstreckes[id].bisPoint = givenPoint;
                }
            return teilstreckes.Values;
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
                    string rid= reader["rid"].ToString();
                    string name = reader["abschnitt_bezeichnung"].ToString();
                    TransportStatus status = (TransportStatus) Convert.ToInt32(reader["rstatus"].ToString());
                    transports.Add(new Transport(tnr, rid, status, name));
                }
            return transports;
        }
    }
}