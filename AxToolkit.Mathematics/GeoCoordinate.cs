using System.Text.Json.Serialization;

namespace AxToolkit.Mathematics;

public struct GeoCoordinate  : IEquatable<GeoCoordinate>
{
    public double BodyRadius { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public double Azimuth { get; set; }
    public double Elevation { get; set; }
    public double Rolling { get; set; }

    public GeoCoordinate(double bodyradius, double altitude, double latitude, double longitude, double azimuth)
        : this(bodyradius, altitude, latitude, longitude, azimuth, true) { }

    public GeoCoordinate(double bodyradius, double altitude, double latitude, double longitude, double azimuth, bool rising)
    {
        BodyRadius = bodyradius;
        Altitude = altitude;
        Latitude = latitude;
        Longitude = longitude;
        Azimuth = azimuth;
        Elevation = rising ?  90 : 0;
        Rolling = 0;
    }

    [JsonIgnore]
    public SphericalCoordinate SphericalCoordinate => new SphericalCoordinate
    {
        Radius = Altitude + BodyRadius,
        Teta = Longitude * Math.PI / 180,
        Phi = Math.PI / 2 - (Latitude * Math.PI / 180)
    };


    [JsonIgnore]
    public Vector Position => SphericalCoordinate.Vector;
    [JsonIgnore]
    public Vector Heading => UnitLocalToGlobal(new Vector(1, 0, 0));
    [JsonIgnore]
    public Vector Above => UnitLocalToGlobal(new Vector(0, 1, 0));

    [JsonIgnore]
    public Quaternion ToLocal => ToGlobal.Conjugate;
    [JsonIgnore]
    public Quaternion ToGlobal => QuaternionToGlobal();
    public Vector UnitLocalToGlobal(Vector u)
    {
        var sph = SphericalCoordinate;
        var az = Math.PI / 2 - Azimuth * Math.PI / 180;
        var el = Math.PI / 2 - Elevation * Math.PI / 180;
        var ro = Math.PI + Rolling * Math.PI / 180;
        var u1 = u;
        var u2 = u1.RotateX(ro);
        var u3 = u2.RotateZ(el);
        var u4 = u3.RotateX(az);
        var u5 = u4.RotateY(sph.Phi - Math.PI / 2);
        var u6 = u5.RotateZ(sph.Teta);
        return u6;
    }

    public Quaternion QuaternionToGlobal()
    {
        var sph = SphericalCoordinate;
        var az = Math.PI / 2 - Azimuth * Math.PI / 180;
        var el = Math.PI / 2 - Elevation * Math.PI / 180;
        var ro = Math.PI + Rolling * Math.PI / 180;

        var ux = new Vector(1, 0, 0);
        var uy = new Vector(0, 1, 0);
        var uz = new Vector(0, 0, 1);

        return Quaternion.Concat(new Quaternion(uz, sph.Teta),
            Quaternion.Concat(new Quaternion(uy, sph.Phi - Math.PI / 2),
                Quaternion.Concat(new Quaternion(ux, az),
                    Quaternion.Concat(new Quaternion(uz, el), new Quaternion(ux, ro))
                    )
                )
            );
    }

    public Vector UnitLocalToSurface(Vector u)
    {
        var az = Math.PI / 2 - Azimuth * Math.PI / 180;
        var el = Elevation * Math.PI / 180;
        var ro = Rolling * Math.PI / 180;
        var u1 = u;
        var u2 = u1.RotateX(ro);
        var u3 = u2.RotateZ(el);
        var u4 = u3.RotateY(az);
        return u4;
    }



    public static GeoCoordinate From(double bodyRadius, Vector position, Vector heading, Vector above)
    {
        var geo = new GeoCoordinate();
        var sph = new SphericalCoordinate(position);
        geo.BodyRadius = bodyRadius;

        geo.Altitude = sph.Radius - bodyRadius;
        geo.Latitude = (Math.PI / 2 - sph.Phi) * 180 / Math.PI;
        geo.Longitude = sph.Teta * 180 / Math.PI;

        var u = heading.Norm
            .RotateZ(-sph.Teta)
            .RotateY(Math.PI / 2 - sph.Phi);

        var az = Math.Atan2(u.Y, u.Z);
        var el = Math.Atan2(u.X, new Vector(u.Y, u.Z, 0).Length);
        geo.Azimuth = az * 180 / Math.PI;
        geo.Elevation = el * 180 / Math.PI;

        var v = above.Norm
            .RotateZ(-sph.Teta)
            .RotateY(Math.PI / 2 - sph.Phi)
            .RotateX(az - Math.PI / 2)
            .RotateZ(el - Math.PI / 2);

        var roll = Math.Atan2(-v.Y, v.Z) - Math.PI / 2;
        if (roll <= -Math.PI)
            roll += 2 * Math.PI;
        geo.Rolling = roll * 180 / Math.PI;

        if (u.Y == 0 && u.Z == 0)
        {
            geo.Azimuth = -geo.Rolling;
            geo.Rolling = 0;
        }

        return geo;
    }

    public static double GeodesicLength(GeoCoordinate geo1, GeoCoordinate geo2)
    {
        if (geo1.BodyRadius != geo2.BodyRadius)
            throw new Exception();

        var geo1Lat = geo1.Latitude * Math.PI / 180;
        var geo2Lat = geo2.Latitude * Math.PI / 180;
        var geo1Long = geo1.Longitude * Math.PI / 180;
        var geo2Long = geo2.Longitude * Math.PI / 180;

        var diffLong = geo1Long - geo2Long;

        var d = Math.Cos(geo2Lat) * Math.Sin(diffLong);
        var e = Math.Cos(geo1Lat) * Math.Sin(geo2Lat)
            - Math.Sin(geo1Lat) * Math.Cos(geo2Lat) * Math.Cos(diffLong);

        var a = Math.Sqrt(Math.Pow(d, 2) + Math.Pow(e, 2));
        var b = Math.Sin(geo1Lat) * Math.Sin(geo2Lat)
            + Math.Cos(geo1Lat) * Math.Cos(geo2Lat) * Math.Cos(diffLong);

        return Math.Atan2(a, b) * geo1.BodyRadius;
    }

    public static double CompareDifference(double a, double b)
        => Math.Log10(Math.Abs(a - b) / Math.Pow(10, Math.Log10(a + b)));
    
    public static bool CompareIsEqual(double a, double b)
        => Math.Log10(Math.Abs(a - b) / Math.Pow(10, Math.Log10(a + b))) < -6.0;

    public bool Equals(GeoCoordinate other)
        => CompareIsEqual(BodyRadius, other.BodyRadius) && CompareIsEqual(Altitude, other.Altitude) &&
        CompareIsEqual(Latitude, other.Latitude) && CompareIsEqual(Longitude, other.Longitude) &&
        (
            (CompareIsEqual(Azimuth, other.Azimuth) && CompareIsEqual(Elevation, other.Elevation) && CompareIsEqual(Rolling, other.Rolling)) ||
            (CompareIsEqual(Elevation, other.Elevation) && CompareIsEqual(Math.Abs(Elevation), 90) && CompareIsEqual(Azimuth + Rolling, other.Azimuth + other.Rolling))
        );
    // Technically: If elevation is +/- 90...  CAn't Azimuth and Rolling can be summed !?
}
