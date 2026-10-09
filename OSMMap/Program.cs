using OSMMapLib;

namespace OSMMap;

class Program
{
    static void Main(string[] args)
    {
        /*
        Tile tile = new Tile(4, 5, 2, "kanfkaknf");
        Console.WriteLine(tile);

        Layer layer = new Layer();
        Console.WriteLine(layer.FormatUrl(10, 20, 30));
        */

        Layer layer = new Layer(maxZoom: 17);
        Map map = new Map();
        map.Layer = layer;
        map.Zoom = 15;
        map.Lat = 50.274932;
        map.Lon = 14.952389;
        map.Render("map.png");
    }
}
