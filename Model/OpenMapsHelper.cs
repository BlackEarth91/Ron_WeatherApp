using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ron_WeatherApp.Model
{
    class OpenMapsHelper
    {

        public class Rootobject
        {
            public Coord coord { get; set; }
            public List<Weather> weather { get; set; }
            public string _base { get; set; }
            public Main main { get; set; }
            public int visibility { get; set; }
            public Wind wind { get; set; }
            public Clouds clouds { get; set; }
            public int dt { get; set; }
            public Sys sys { get; set; }
            public int timezone { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public int cod { get; set; }
        }

        public class Coord
        {
            public float lon { get; set; }
            public float lat { get; set; }
        }

        public class Main
        {
            public float temp { get; set; }
            public float feels_like { get; set; }
            public float temp_min { get; set; }
            public float temp_max { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
        }

        public class Wind
        {
            public float speed { get; set; }
            public int deg { get; set; }
        }

        public class Clouds
        {
            public int all { get; set; }
        }

        public class Sys
        {
            public int type { get; set; }
            public int id { get; set; }
            public string country { get; set; }
            public int sunrise { get; set; }
            public int sunset { get; set; }
        }

        public class Weather
        {
            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }

        public class CompiledWeather
        {
            public String temp { get; set; }
            public String temp_min { get; set; }
            public String temp_max { get; set; }
            public int humidity { get; set; }
            public String sunrise { get; set; }
            public String sunset { get; set; }
            public string name { get; set; }
            public string icon { get; set; }
            public int cod { get; set; }
            public string last_update { get; set; }
        }

        public static String UnixTimeToDateTime(long unixtime)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

            dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();

            return dtDateTime.ToString("HH:mm:ss");
        }

        public static String KevDegConvert(float kev)
        {
            float retval = kev - 273.15f;
            return retval.ToString("##") + " Deg C";
        }




    }

}
