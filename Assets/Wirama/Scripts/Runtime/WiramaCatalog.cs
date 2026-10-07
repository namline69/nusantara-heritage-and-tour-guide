using UnityEngine;

namespace Wirama.Splash
{
    /// <summary>One featured destination (a pin on the splash map).</summary>
    [System.Serializable]
    public class DestinationInfo
    {
        public string id;
        public string name;
        public string region;
        public float lat;
        public float lon;
        public string description;

        public DestinationInfo(string id, string name, string region, float lat, float lon, string description)
        {
            this.id = id; this.name = name; this.region = region;
            this.lat = lat; this.lon = lon; this.description = description;
        }
    }

    /// <summary>One island group = one PNG layer cropped from id.svg.</summary>
    [System.Serializable]
    public class IslandInfo
    {
        public string id;
        public string displayName;
        /// <summary>Bounds of the cropped sprite, in SVG units of the original 1000x368 map.</summary>
        public int x0, y0, x1, y1;
        public DestinationInfo[] destinations;

        public IslandInfo(string id, string displayName, int x0, int y0, int x1, int y1, DestinationInfo[] destinations)
        {
            this.id = id; this.displayName = displayName;
            this.x0 = x0; this.y0 = y0; this.x1 = x1; this.y1 = y1;
            this.destinations = destinations;
        }

        public string SpriteAssetPath { get { return "Assets/Wirama/Art/Islands/island_" + id + ".png"; } }
    }

    /// <summary>
    /// Single source of truth for the splash map. Edit the destinations here, then run
    /// Tools > Wirama > 1. Build Splash Scene again to regenerate the pins.
    /// The map is stylised, so pin positions are approximate (snapped onto land).
    /// </summary>
    public static class WiramaCatalog
    {
        public static readonly IslandInfo[] Islands = new[]
        {
            new IslandInfo("sumatera", "Sumatera", 42, 13, 328, 255, new[]
            {
                new DestinationInfo("toba", "Danau Toba", "Sumatera Utara", 2.65f, 98.88f,
                    "Danau vulkanik terbesar di dunia, terbentuk dari letusan super sekitar 74.000 tahun lalu. Rumah budaya Batak dengan Pulau Samosir di tengahnya."),
                new DestinationInfo("jamgadang", "Jam Gadang", "Bukittinggi, Sumatera Barat", -0.305f, 100.369f,
                    "Menara jam ikonik di jantung Bukittinggi, pintu masuk menjelajahi budaya Minangkabau, rumah gadang, dan Ngarai Sianok."),
                new DestinationInfo("ampera", "Jembatan Ampera", "Palembang, Sumatera Selatan", -2.99f, 104.76f,
                    "Jembatan megah di atas Sungai Musi, ikon Palembang, kota pempek dan kain songket."),
            }),
            new IslandInfo("jawa", "Jawa", 242, 244, 462, 312, new[]
            {
                new DestinationInfo("kotatua", "Kota Tua Jakarta", "DKI Jakarta", -6.135f, 106.813f,
                    "Kawasan bersejarah peninggalan Batavia dengan Museum Fatahillah, bangunan kolonial, dan kafe-kafe di Jakarta."),
                new DestinationInfo("borobudur", "Candi Borobudur", "Magelang, Jawa Tengah", -7.608f, 110.204f,
                    "Candi Buddha abad ke-9 terbesar di dunia, Warisan Dunia UNESCO dengan ribuan panel relief dan stupa."),
                new DestinationInfo("bromo", "Gunung Bromo", "Jawa Timur", -7.942f, 112.953f,
                    "Gunung api aktif di Taman Nasional Bromo Tengger Semeru, terkenal dengan lautan pasir dan panorama matahari terbit."),
            }),
            new IslandInfo("kalimantan", "Kalimantan", 312, 43, 524, 233, new[]
            {
                new DestinationInfo("khatulistiwa", "Tugu Khatulistiwa", "Pontianak, Kalimantan Barat", 0.0f, 109.33f,
                    "Monumen penanda garis khatulistiwa yang melintas tepat di Pontianak, dua belahan bumi dalam satu kota."),
                new DestinationInfo("tanjungputing", "Tanjung Puting", "Kalimantan Tengah", -2.78f, 111.8f,
                    "Taman nasional di Kalimantan Tengah, rumah orangutan yang dijelajahi dengan perahu klotok menyusuri sungai."),
                new DestinationInfo("mahakam", "Sungai Mahakam", "Samarinda, Kalimantan Timur", -0.5f, 117.15f,
                    "Sungai besar Kalimantan Timur, jalur kehidupan masyarakat Dayak dan Kutai serta habitat pesut mahakam."),
            }),
            new IslandInfo("balinusra", "Bali & Nusa Tenggara", 426, 290, 645, 355, new[]
            {
                new DestinationInfo("tanahlot", "Pura Tanah Lot", "Tabanan, Bali", -8.544f, 115.188f,
                    "Pura laut di atas batu karang di Tabanan, Bali, terkenal dengan panorama matahari terbenam."),
                new DestinationInfo("rinjani", "Gunung Rinjani", "Lombok, Nusa Tenggara Barat", -8.411f, 116.457f,
                    "Gunung api setinggi 3.726 mdpl di Lombok dengan danau kaldera Segara Anak yang menawan."),
                new DestinationInfo("labuanbajo", "Labuan Bajo", "Flores, Nusa Tenggara Timur", -8.497f, 119.888f,
                    "Gerbang menuju Taman Nasional Komodo, habitat asli komodo dan pulau-pulau savana yang eksotis."),
            }),
            new IslandInfo("sulawesi", "Sulawesi", 512, 20, 685, 284, new[]
            {
                new DestinationInfo("makassar", "Benteng Rotterdam", "Makassar, Sulawesi Selatan", -5.134f, 119.406f,
                    "Benteng peninggalan Kerajaan Gowa-Tallo di tepi pantai Makassar, kemudian dikuasai VOC Belanda."),
                new DestinationInfo("toraja", "Tana Toraja", "Sulawesi Selatan", -3.05f, 119.85f,
                    "Negeri tongkonan, rumah adat beratap melengkung seperti perahu, dengan upacara adat dan tradisi leluhur yang memukau."),
                new DestinationInfo("bunaken", "Taman Laut Bunaken", "Manado, Sulawesi Utara", 1.47f, 124.84f,
                    "Taman nasional laut di lepas pantai Manado dengan terumbu karang dan dinding laut yang kaya kehidupan."),
            }),
            new IslandInfo("maluku", "Maluku", 621, 78, 838, 303, new[]
            {
                new DestinationInfo("ternate", "Ternate", "Maluku Utara", 0.664f, 127.633f,
                    "Kota kepulauan rempah di kaki Gunung Gamalama, pusat kesultanan dan sejarah perdagangan cengkih."),
                new DestinationInfo("ambon", "Kota Ambon", "Maluku", -3.695f, 128.18f,
                    "Ibu kota Maluku yang dikenal sebagai kota musik, dengan pantai, benteng kuno, dan sejarah rempah."),
                new DestinationInfo("banda", "Banda Neira", "Kepulauan Banda, Maluku", -4.52f, 129.9f,
                    "Kepulauan Banda, tanah asal pala yang dulu diperebutkan bangsa Eropa, kini surga selam dan situs sejarah."),
            }),
            new IslandInfo("papua", "Papua", 728, 109, 958, 318, new[]
            {
                new DestinationInfo("rajaampat", "Raja Ampat", "Papua Barat Daya", -0.172f, 130.549f,
                    "Gugusan pulau karang dengan keanekaragaman hayati laut yang termasuk tertinggi di dunia."),
                new DestinationInfo("baliem", "Lembah Baliem", "Wamena, Papua", -4.09f, 138.94f,
                    "Lembah di Pegunungan Tengah Papua, tempat tinggal suku Dani dan lokasi Festival Lembah Baliem."),
                new DestinationInfo("sentani", "Danau Sentani", "Jayapura, Papua", -2.62f, 140.52f,
                    "Danau luas di dekat Jayapura yang dikelilingi perbukitan, dikenal lewat Festival Danau Sentani."),
            }),
        };

        public static IslandInfo FindIsland(string id)
        {
            foreach (var island in Islands) if (island.id == id) return island;
            return null;
        }

        public static DestinationInfo FindDestination(string id)
        {
            foreach (var island in Islands)
                foreach (var d in island.destinations)
                    if (d.id == id) return d;
            return null;
        }
    }

    /// <summary>Conversions between lat/lon, the SVG space of id.svg and the local space of the map RectTransform.</summary>
    public static class MapMath
    {
        public const float SvgWidth = 1000f;
        public const float SvgHeight = 368f;
        /// <summary>Width of the map RectTransform in canvas units (reference canvas is 1284 wide).</summary>
        public const float MapWidth = 1240f;
        public const float Scale = MapWidth / SvgWidth;
        public const float MapHeight = SvgHeight * Scale;

        // Equirectangular calibration taken from the three reference points inside id.svg.
        const double Bx = 19.77836772434149, By = -19.85550545664345;
        const double X0 = 90.9, Y0 = 334.2, Lon0 = 97.31095197695628, Lat0 = -10.080985198323209;

        public static Vector2 LatLonToSvg(float lat, float lon)
        {
            return new Vector2((float)(X0 + Bx * (lon - Lon0)), (float)(Y0 + By * (lat - Lat0)));
        }

        /// <summary>SVG space (origin top-left, y down) to map-local space (origin centre, y up).</summary>
        public static Vector2 SvgToLocal(Vector2 svg)
        {
            return new Vector2((svg.x - SvgWidth * 0.5f) * Scale, (SvgHeight * 0.5f - svg.y) * Scale);
        }

        public static Vector2 LatLonToLocal(float lat, float lon)
        {
            return SvgToLocal(LatLonToSvg(lat, lon));
        }

        /// <summary>Area the camera should frame when zooming into an island: its pins plus a margin.</summary>
        public static Rect FocusRect(IslandInfo island, float marginSvg = 36f)
        {
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var d in island.destinations)
            {
                Vector2 s = LatLonToSvg(d.lat, d.lon);
                minX = Mathf.Min(minX, s.x); maxX = Mathf.Max(maxX, s.x);
                minY = Mathf.Min(minY, s.y); maxY = Mathf.Max(maxY, s.y);
            }
            Vector2 a = SvgToLocal(new Vector2(minX - marginSvg, maxY + marginSvg));   // bottom-left
            Vector2 b = SvgToLocal(new Vector2(maxX + marginSvg, minY - marginSvg));   // top-right
            return new Rect(a.x, a.y, b.x - a.x, b.y - a.y);
        }
    }
}
