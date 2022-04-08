﻿using System;

namespace StackExchange.Redis
{
    /// <summary>
    /// GeoRadius command options.
    /// </summary>
    [Flags]
    public enum GeoRadiusOptions
    {
        /// <summary>
        /// No Options.
        /// </summary>
        None = 0,
        /// <summary>
        /// Redis will return the coordinates of any results.
        /// </summary>
        WithCoordinates = 1,
        /// <summary>
        /// Redis will return the distance from center for all results.
        /// </summary>
        WithDistance = 2,
        /// <summary>
        /// Redis will return the geo hash value as an integer. (This is the score in the sorted set).
        /// </summary>
        WithGeoHash = 4,
        /// <summary>
        /// Populates the commonly used values from the entry (the integer hash is not returned as it is not commonly useful).
        /// </summary>
        Default = WithCoordinates | GeoRadiusOptions.WithDistance
    }

    /// <summary>
    /// The result of a GeoRadius command.
    /// </summary>
    public readonly struct GeoRadiusResult
    {
        /// <summary>
        /// Indicate the member being represented.
        /// </summary>
        public override string ToString() => Member.ToString();

        /// <summary>
        /// The matched member.
        /// </summary>
        public RedisValue Member { get; }

        /// <summary>
        /// The distance of the matched member from the center of the geo radius command.
        /// </summary>
        public double? Distance { get; }

        /// <summary>
        /// The hash value of the matched member as an integer. (The key in the sorted set).
        /// </summary>
        /// <remarks>Note that this is not the same as the hash returned from GeoHash</remarks>
        public long? Hash { get; }

        /// <summary>
        /// The coordinates of the matched member.
        /// </summary>
        public GeoPosition? Position { get; }

        /// <summary>
        /// Returns a new GeoRadiusResult.
        /// </summary>
        /// <param name="member">The value from the result.</param>
        /// <param name="distance">The distance from the result.</param>
        /// <param name="hash">The hash of the result.</param>
        /// <param name="position">The GeoPosition of the result.</param>
        public GeoRadiusResult(in RedisValue member, double? distance, long? hash, GeoPosition? position)
        {
            Member = member;
            Distance = distance;
            Hash = hash;
            Position = position;
        }
    }

    /// <summary>
    /// Describes the longitude and latitude of a GeoEntry.
    /// </summary>
    public readonly struct GeoPosition : IEquatable<GeoPosition>
    {
        internal static string GetRedisUnit(GeoUnit unit) => unit switch
        {
            GeoUnit.Meters => "m",
            GeoUnit.Kilometers => "km",
            GeoUnit.Miles => "mi",
            GeoUnit.Feet => "ft",
            _ => throw new ArgumentOutOfRangeException(nameof(unit)),
        };

        /// <summary>
        /// The Latitude of the GeoPosition.
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// The Longitude of the GeoPosition.
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Creates a new GeoPosition.
        /// </summary>
        public GeoPosition(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        /// <summary>
        /// A "{long} {lat}" string representation of this position.
        /// </summary>
        public override string ToString() => string.Format("{0} {1}", Longitude, Latitude);

        /// <summary>
        /// See <see cref="object.GetHashCode"/>.
        /// Diagonals not an issue in the case of lat/long.
        /// </summary>
        /// <remarks>
        /// Diagonals are not an issue in the case of lat/long.
        /// </remarks>
        public override int GetHashCode() => Longitude.GetHashCode() ^ Latitude.GetHashCode();

        /// <summary>
        /// Compares two values for equality.
        /// </summary>
        /// <param name="obj">The <see cref="GeoPosition"/> to compare to.</param>
        public override bool Equals(object? obj) => obj is GeoPosition gpObj && Equals(gpObj);

        /// <summary>
        /// Compares two values for equality.
        /// </summary>
        /// <param name="other">The <see cref="GeoPosition"/> to compare to.</param>
        public bool Equals(GeoPosition other) => this == other;

        /// <summary>
        /// Compares two values for equality.
        /// </summary>
        /// <param name="x">The first position to compare.</param>
        /// <param name="y">The second position to compare.</param>
        public static bool operator ==(GeoPosition x, GeoPosition y) => x.Longitude == y.Longitude && x.Latitude == y.Latitude;

        /// <summary>
        /// Compares two values for non-equality.
        /// </summary>
        /// <param name="x">The first position to compare.</param>
        /// <param name="y">The second position to compare.</param>
        public static bool operator !=(GeoPosition x, GeoPosition y) => x.Longitude != y.Longitude || x.Latitude != y.Latitude;
    }

    /// <summary>
    /// Describes a GeoEntry element with the corresponding value.
    /// GeoEntries are stored in redis as SortedSetEntries.
    /// </summary>
    public readonly struct GeoEntry : IEquatable<GeoEntry>
    {
        /// <summary>
        /// The name of the GeoEntry.
        /// </summary>
        public RedisValue Member { get; }

        /// <summary>
        /// Describes the longitude and latitude of a GeoEntry.
        /// </summary>
        public GeoPosition Position { get; }

        /// <summary>
        /// Initializes a GeoEntry value.
        /// </summary>
        /// <param name="longitude">The longitude position to use.</param>
        /// <param name="latitude">The latitude position to use.</param>
        /// <param name="member">The value to store for this position.</param>
        public GeoEntry(double longitude, double latitude, RedisValue member)
        {
            Member = member;
            Position = new GeoPosition(longitude, latitude);
        }

        /// <summary>
        /// The longitude of the GeoEntry.
        /// </summary>
        public double Longitude => Position.Longitude;

        /// <summary>
        /// The latitude of the GeoEntry.
        /// </summary>
        public double Latitude => Position.Latitude;

        /// <summary>
        /// A "({Longitude},{Latitude})={Member}" string representation of this entry.
        /// </summary>
        public override string ToString() => $"({Longitude},{Latitude})={Member}";

        /// <inheritdoc/>
        public override int GetHashCode() => Position.GetHashCode() ^ Member.GetHashCode();

        /// <summary>
        /// Compares two values for equality.
        /// </summary>
        /// <param name="obj">The <see cref="GeoEntry"/> to compare to.</param>
        public override bool Equals(object? obj) => obj is GeoEntry geObj && Equals(geObj);

        /// <summary>
        /// Compares two values for equality.
        /// </summary>
        /// <param name="other">The <see cref="GeoEntry"/> to compare to.</param>
        public bool Equals(GeoEntry other) => this == other;

        /// <summary>
        /// Compares two values for equality.
        /// </summary>
        /// <param name="x">The first entry to compare.</param>
        /// <param name="y">The second entry to compare.</param>
        public static bool operator ==(GeoEntry x, GeoEntry y) => x.Position == y.Position && x.Member == y.Member;

        /// <summary>
        /// Compares two values for non-equality.
        /// </summary>
        /// <param name="x">The first entry to compare.</param>
        /// <param name="y">The second entry to compare.</param>
        public static bool operator !=(GeoEntry x, GeoEntry y) => x.Position != y.Position || x.Member != y.Member;
    }
}
