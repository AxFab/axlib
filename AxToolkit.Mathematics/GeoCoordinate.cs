// This file is part of AxLib.
// 
// AxLib is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software 
// Foundation, either version 3 of the License, or (at your option) any later 
// version.
// 
// AxLib is distributed in the hope that it will be useful, but WITHOUT ANY 
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
// details.
// 
// You should have received a copy of the GNU General Public License along 
// with AxLib. If not, see <https://www.gnu.org/licenses/>.
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
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
        Teta = AxMath.Degree2Radian(Longitude),
        Phi = AxMath.HalfPI - AxMath.Degree2Radian(Latitude),
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
        var az = AxMath.HalfPI - AxMath.Degree2Radian(Azimuth);
        var el = AxMath.HalfPI - AxMath.Degree2Radian(Elevation);
        var ro = Math.PI + AxMath.Degree2Radian(Rolling);
        var u1 = u;
        var u2 = u1.RotateX(ro);
        var u3 = u2.RotateZ(el);
        var u4 = u3.RotateX(az);
        var u5 = u4.RotateY(sph.Phi - AxMath.HalfPI);
        var u6 = u5.RotateZ(sph.Teta);
        return u6;
    }

    public Quaternion QuaternionToGlobal()
    {
        var sph = SphericalCoordinate;
        var az = AxMath.HalfPI - AxMath.Degree2Radian(Azimuth);
        var el = AxMath.HalfPI - AxMath.Degree2Radian(Elevation);
        var ro = Math.PI + AxMath.Degree2Radian(Rolling);

        var ux = new Vector(1, 0, 0);
        var uy = new Vector(0, 1, 0);
        var uz = new Vector(0, 0, 1);

        return Quaternion.Concat(new Quaternion(uz, sph.Teta),
            Quaternion.Concat(new Quaternion(uy, sph.Phi - AxMath.HalfPI),
                Quaternion.Concat(new Quaternion(ux, az),
                    Quaternion.Concat(new Quaternion(uz, el), new Quaternion(ux, ro))
                    )
                )
            );
    }

    public Vector UnitLocalToSurface(Vector u)
    {
        var az = AxMath.HalfPI - AxMath.Degree2Radian(Azimuth);
        var el = AxMath.Degree2Radian(Elevation);
        var ro = AxMath.Degree2Radian(Rolling);
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
        geo.Latitude = AxMath.Radian2Degree(AxMath.HalfPI - sph.Phi);
        geo.Longitude = AxMath.Radian2Degree(sph.Teta);
        
        var u = heading.Norm
            .RotateZ(-sph.Teta)
            .RotateY(AxMath.HalfPI - sph.Phi);

        var az = Math.Atan2(u.Y, u.Z);
        var el = Math.Atan2(u.X, new Vector(u.Y, u.Z, 0).Length);
        geo.Azimuth = AxMath.Radian2Degree(az);
        geo.Elevation = AxMath.Radian2Degree(el);

        var v = above.Norm
            .RotateZ(-sph.Teta)
            .RotateY(AxMath.HalfPI - sph.Phi)
            .RotateX(az - AxMath.HalfPI)
            .RotateZ(el - AxMath.HalfPI);

        var roll = Math.Atan2(-v.Y, v.Z) - AxMath.HalfPI;
        if (roll <= -Math.PI)
            roll += AxMath.TwoPI;
        geo.Rolling = AxMath.Radian2Degree(roll);

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

        var geo1Lat = AxMath.Degree2Radian(geo1.Latitude);
        var geo2Lat = AxMath.Degree2Radian(geo2.Latitude);
        var geo1Long = AxMath.Degree2Radian(geo1.Longitude);
        var geo2Long = AxMath.Degree2Radian(geo2.Longitude);

        var diffLong = geo1Long - geo2Long;

        var d = Math.Cos(geo2Lat) * Math.Sin(diffLong);
        var e = Math.Cos(geo1Lat) * Math.Sin(geo2Lat)
            - Math.Sin(geo1Lat) * Math.Cos(geo2Lat) * Math.Cos(diffLong);

        var a = Math.Sqrt(Math.Pow(d, 2) + Math.Pow(e, 2));
        var b = Math.Sin(geo1Lat) * Math.Sin(geo2Lat)
            + Math.Cos(geo1Lat) * Math.Cos(geo2Lat) * Math.Cos(diffLong);

        return Math.Atan2(a, b) * geo1.BodyRadius;
    }

    public bool Equals(GeoCoordinate other)
    {
        var samePos = AxMath.AlmostEqual(Latitude, other.Latitude) && AxMath.AlmostEqual(Longitude, other.Longitude);
        if (!samePos)
            return false;
        var sameAlt = AxMath.AlmostEqual(BodyRadius, other.BodyRadius) && AxMath.AlmostEqual(Altitude, other.Altitude);
        if (!sameAlt)
            return false;
        var sameOri = AxMath.AlmostEqual(Azimuth, other.Azimuth) && AxMath.AlmostEqual(Elevation, other.Elevation) && AxMath.AlmostEqual(Rolling, other.Rolling);
        // Technically: If elevation is +/- 90...  Can't Azimuth and Rolling be summed !?
        if (!sameOri && AxMath.AlmostEqual(Elevation, other.Elevation) && AxMath.AlmostEqual(Math.Abs(Elevation), 90))
            sameOri = AxMath.AlmostEqual(Azimuth + Rolling, other.Azimuth + other.Rolling);
        return sameOri;
    }
    
}
