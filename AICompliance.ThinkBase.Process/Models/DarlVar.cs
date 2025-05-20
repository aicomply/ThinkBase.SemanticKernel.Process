// Copyright (c) 2025 AI Compliance inc. Licensed under the MIT License.

namespace AICompliance.ThinkBase.Process.Models
{
    /// <summary>
    /// A generic representation for a single data item
    /// permitting a wide range of data types and supporting 
    /// possibilistic uncertainty as well as a confidence figure.
    /// </summary>
    public class DarlVar
    {
        /// <summary>
        /// The type of data stored in the DarlVar
        /// </summary>
        public enum DataType
        {
            /// <summary>
            /// Numeric including fuzzy
            /// </summary>
            numeric,
            /// <summary>
            /// One or more categories with confidences
            /// </summary>
            categorical,
            /// <summary>
            /// Textual
            /// </summary>
            textual,
            /// <summary>
            /// a text sequence
            /// </summary>
            sequence,
            /// <summary>
            /// A date with optional time
            /// </summary>
            date,
            /// <summary>
            /// A time 
            /// </summary>
            time,
            /// <summary>
            /// a time span
            /// </summary>
            duration,
            /// <summary>
            /// A geographical location
            /// </summary>
            location,
            /// <summary>
            /// A url/Uri
            /// </summary>
            link,
            /// <summary>
            /// The uri of an image
            /// </summary>
            image,
            /// <summary>
            /// The uri of a video
            /// </summary>
            video,
            /// <summary>
            /// a username and password
            /// </summary>
            credentials,
            /// <summary>
            /// A personal name
            /// </summary>
            name,
            /// <summary>
            /// An organization name
            /// </summary>
            organization,
            /// <summary>
            /// details of a financial transaction
            /// </summary>
            payment,
            /// <summary>
            /// details of a ruleset to call
            /// </summary>
            ruleset,
            /// <summary>
            /// signals a process is complete
            /// </summary>
            complete
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string name { get; set; }

        /// <summary>
        /// This result is unknown if true.
        /// </summary>
        /// <value><c>true</c> if unknown; otherwise, <c>false</c>.</value>
        public bool unknown { get; set; } = false;
        /// <summary>
        /// The confidence placed in this result
        /// </summary>
        /// <value>The weight.</value>
        public double weight { get; set; } = 1.0;

        /// <summary>
        /// The array containing the up to 4 values representing the fuzzy number.
        /// </summary>
        /// <value>The values.</value>
        /// <remarks>Since all fuzzy numbers used by DARL are convex, i,e. their envelope doesn't have any in-folding
        /// sections, the user can specify numbers with a simple sequence of doubles.
        /// So 1 double represents a crisp or singleton value.
        /// 2 doubles represent an interval,
        /// 3 a triangular fuzzy set,
        /// 4 a trapezoidal fuzzy set.
        /// The values must be ordered in ascending value, but it is permissible for two or more to hold the same value.</remarks>
        public List<double> values { get; set; }

        /// <summary>
        /// list of categories, each indexed against a truth value.
        /// </summary>
        /// <value>The categories.</value>
        //public Dictionary<string, double> categories { get; set; }
        public List<NameValuePair> categories { get; set; }


        public List<DateTime> times { get; set; }

        /// <summary>
        /// Indicates approximation has taken place in calculating the values.
        /// </summary>
        /// <value><c>true</c> if approximate; otherwise, <c>false</c>.</value>
        /// <remarks>Under some circumstances the coordinates of the fuzzy number
        /// in "values" may not exactly represent the "cuts" values.</remarks>
        public bool approximate { get; set; }


        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        public DataType dataType { get; set; }

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        /// <value>The sequence.</value>
        public List<List<string>> sequence { get; set; }

        /// <summary>
        /// Single central or most confident value, expressed as a string or double.
        /// </summary>
        /// <value>The value.</value>
        public string value { get; set; } = string.Empty;
    }

    public class NameValuePair
    {
        public string name { get; set; }
        public double value { get; set; }
    }
}

