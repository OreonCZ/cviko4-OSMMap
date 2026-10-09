using System.Security.Cryptography.X509Certificates;
using System.Text;
using MapRendererLib;

namespace OSMMapLib;

public class Tile
{
    public int X {get; private set;}
    public int Y {get; private set;}

    private int _zoom;
    public int Zoom {
        get { return _zoom; } 
        set {
            if(value < 1) _zoom = 1;
            else _zoom = value;
        }
    }

    public string Url {get; private set;}

    public Tile(int x, int y, int zoom, string url){
        X = x;
        Y = y;
        Zoom = zoom;
        Url = url;
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("[");
        stringBuilder.Append(X);
        stringBuilder.Append(",");
        stringBuilder.Append(Y);
        stringBuilder.Append(",");
        stringBuilder.Append(Zoom);
        stringBuilder.Append("]: ").Append(Url);
        return stringBuilder.ToString();
    }

}

public class Layer
{
    public string UrlTemplate { get; private set; }
    public int MaxZoom { get; private set; }

    // nova vec
    public Layer(
        string urlTemplate = "https://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        int maxZoom = 10
        )
    {
        UrlTemplate = urlTemplate;
        MaxZoom = maxZoom;
    }

    public string FormatUrl(int x, int y, int zoom)
    {
        Random rand = new Random();
        int n = rand.Next(3);
        char c = (char)('a' + n);

        string url = UrlTemplate.Replace("{x}", x.ToString());
        url = url.Replace("{y}", y.ToString());
        url = url.Replace("{z}", zoom.ToString());
        url = url.Replace("{c}", c.ToString());
        
        return url;
    }


public Tile this[int x, int y, int zoom]
{
    get { return new Tile(x, y, zoom, FormatUrl(x, y, zoom));}        
}
}



public class Map
{
    // nebude to delat problem
    public Layer Layer {get; set;}
    public double Lat {get; set;}
    public double Lon {get; set;}
    
    public int _zoom;
    public int Zoom {
        get {
            if(_zoom < 1) return 1;
            if(_zoom > Layer.MaxZoom) return Layer.MaxZoom;
            return _zoom;
        } 
        set { _zoom = value; }
    }

    private int CenterTileX {
        get {
            return (int)((Lon + 180.0) / 360.0 * (1 << Zoom));
        }
    }

    private int CenterTileY {
        get {
            return (int)((1.0 - Math.Log(Math.Tan(Lat * Math.PI / 180.0) + 1.0 / 
            Math.Cos(Lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << Zoom));
        }
    }

    public void Render(string fileName)
    {
        MapRenderer mapRenderer = new MapRenderer(4, 4);

        for (int x = -2; x < 2; x++)
        {
            for (int y = -2; y < 2; y++)
            {
            Tile tile = this.Layer[this.CenterTileX + x, this.CenterTileY + y, this.Zoom];

            Console.WriteLine(tile);

            mapRenderer.Set(x + 2, y + 2, tile.Url);
            }
        }

        mapRenderer.Flush();
        mapRenderer.Render(fileName);
    }
}
