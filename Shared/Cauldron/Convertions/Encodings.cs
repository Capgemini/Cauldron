﻿namespace Cauldron
{
    /// <summary>
    /// Provides predefined encodings
    /// </summary>
    public enum Encodings
    {
        /// <summary>
        ///  Encoding for the ASCII (7-bit) character set.
        /// </summary>
        ASCII,

        /// <summary>
        /// Encoding for the UTF-16 format that uses the big endian byte order.
        /// </summary>
        BigEndianUnicode,

        /// <summary>
        /// Encoding for the UTF-16 format using the little endian byte order.
        /// </summary>
        Unicode,

        /// <summary>
        /// Encoding for the UTF-32 format using the little endian byte order.
        /// </summary>
        UTF32,

        /// <summary>
        /// Encoding for the ISO-8859-1 format. Used by default in the legacy components of Microsoft Windows.
        /// </summary>
        ANSI,

        /// <summary>
        /// Encoding for the UTF-7 format.
        /// </summary>
        UTF7,

        /// <summary>
        /// Encoding for the UTF-8 format.
        /// </summary>
        UTF8,

        /// <summary>
        /// Encoding for the IBM EBCDIC format (US-Canada).
        /// </summary>
        EBCDIC_IBM037,

        /// <summary>
        /// Encoding for the IBM EBCDIC format (IBM Latin-1).
        /// </summary>
        EBCDIC_IBM01047,

        /// <summary>
        /// Encoding for the IBM EBCDIC format (US-Canada-Euro).
        /// </summary>
        EBCDIC_IBM01140,

        /// <summary>
        /// Encoding for the IBM EBCDIC format (Germany-Euro).
        /// </summary>
        EBCDIC_IBM01141,
    }
}