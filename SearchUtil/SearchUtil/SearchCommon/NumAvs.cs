namespace SearchCommon
{
    /// <summary>
    /// Class used to caulculate approximate time needed for manual collection of file list.
    /// Represents the average of a collection of numbers, with a multiple applied to the average.
    /// </summary>
    public class NumAvs
    {
        private double total, amt;

        /// <summary>
        /// Gets or sets the multiple applied to the average.
        /// </summary>
        public double Multiple;

        /// <summary>
        /// Gets the number of entries in the collection.
        /// </summary>
        public double Count
        {
            get
            {
                return amt;
            }
        }

        public NumAvs()
        {
            Multiple = 1.0;
            total = 0.0;
            amt = 0.0;
        }

        /// <summary>
        /// Adds a value to the collection
        /// </summary>
        /// <param name="val">The value to add to the collection.</param>
        public void Add(double value)
        {
            total += value;
            amt += 1.0;
        }

        /// <summary>
        /// Gets the average of the values in the collection, multiplied by the 'Multiple' property.
        /// </summary>
        /// <returns>The average of the values in the collection, multiplied by the 'Multiple' property.</returns>
        public double getAvg()
        {
            if (amt == 0)
                return 0;
            return Multiple * total / amt;
        }
    }
}