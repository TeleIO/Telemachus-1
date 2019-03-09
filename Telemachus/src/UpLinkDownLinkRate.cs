//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.Text;

namespace Telemachus
{
    /// <summary>
    /// Class to keep track of the Uplink and Downlink data rates. 
    /// </summary>
    public class UpLinkDownLinkRate
    {
        #region Constants

        /// <summary>
        /// Number of data points to be held in the <see cref="upLinkRate"/> and <see cref="downLinkRate"/> lists.
        /// </summary>
        public const int DEFAULT_AVERAGE_SIZE = 20;

        #endregion

        #region Fields

        // Why exactly five and not some other interval?
        private static TimeSpan TIME_SPAN_5_SECONDS = new TimeSpan(0, 0, 5);
        private static DateTime TIME_ARBITRARY = System.DateTime.Now;

        /// <summary>
        /// The number of data points to be held in the <see cref="upLinkRate"/> and <see cref="downLinkRate"/> lists
        /// over which <see cref="average"/> performs its average.
        /// </summary>
        private int averageSize = DEFAULT_AVERAGE_SIZE;

        /// <summary>
        /// The list that will hold how much data we recieved from the client in bits
        /// </summary>
        private LinkedList<KeyValuePair<DateTime, int>> upLinkRate = new LinkedList<KeyValuePair<DateTime, int>>();
        /// <summary>
        /// The list that will hold how much data we sent to the client in bits
        /// </summary>
        private LinkedList<KeyValuePair<DateTime, int>> downLinkRate = new LinkedList<KeyValuePair<DateTime, int>>();

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Empty constructor.</para>
        /// <para><see cref="averageSize"/> == <see cref="DEFAULT_AVERAGE_SIZE"/></para>
        /// </summary>
        public UpLinkDownLinkRate()
        {

        }

        /// <summary>
        /// Parametrized constructor sets <see cref="averageSize"/> to a custom value.
        /// </summary>
        /// <param name="averageSize">The desired number of data points in the LinkedLists.</param>
        public UpLinkDownLinkRate(int averageSize)
        {
            this.averageSize = averageSize;
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Adds a data point for bits recieved from the client (UPlink). Performs internal bytes*8 conversion.
        /// </summary>
        public void RecieveDataFromClient(int bytes)
        {
            // Convert to bits for the data rate.
            addGuardedPoint(DateTime.Now, bytes*8, upLinkRate);
        }

        /// <summary>
        /// Adds a data point for bits sent to the client (DOWNlink). Performs internal bytes*8 conversion.
        /// </summary>
        public void SendDataToClient(int bytes)
        {
            // Convert to bits for the data rate.
            addGuardedPoint(DateTime.Now, bytes*8, downLinkRate);
        }

        /// <summary>
        /// Gets the average Downlink rate over the last 5 seconds computed by <see cref="average"/>
        /// </summary>
        /// <returns>double: Average Downlink rate over the last 5 seconds in bits/s</returns>
        public double getDownLinkRate()
        {
            return average(downLinkRate);
        }

        /// <summary>
        /// Gets the average Uplink rate over the last 5 seconds computed by <see cref="average"/>
        /// </summary>
        /// <returns>double: Average Uplink rate over the last 5 seconds in bits/s</returns>
        public double getUpLinkRate()
        {
            return average(upLinkRate);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a data point to the list passed in as <paramref name="rate"/> argument.
        /// <para>Keeps the number of data points in the <paramref name="rate"/> list at or below the number of points specified in <see cref="averageSize"/>.</para>
        /// </summary>
        /// <param name="time">The DateTime for the point to add, will normally be DateTime.Now.</param>
        /// <param name="bits">The number of bits for the point to add.</param>
        /// <param name="rate">The LinkedList where the data point is added.</param>
        private void addGuardedPoint(DateTime time, int bits, LinkedList<KeyValuePair<DateTime, int>> rate)
        {
            // Locks the LinkedList from being accessed simulatneously by another method.
            lock (rate)
            {
                // Adds a new item to the top of the list.
                rate.AddFirst(new KeyValuePair<DateTime, int>(time, bits));
                
                // Checks if the list has grown beyond averageSize.
                if (rate.Count >= averageSize)
                {
                    // Removes the last (oldest) element from the list.
                    rate.RemoveLast();
                }
            }
        }

        /// <summary>
        /// Computes the average data rate over the last 5 seconds or over the entire rate list, whichever is strictest.
        /// </summary>
        /// <param name="rate">The LinkedList of data points to average.</param>
        /// <returns>The datarate in bits/s as a double.</returns>
        private double average(LinkedList<KeyValuePair<DateTime, int>> rate)
        {
            // Locks the LinkedList from being accessed simulatneously by another method.
            lock (rate)
            {
                // Returns if we don't have at least two data points to average over a time interval.
                if(rate.Count < 2)
                {
                    return 0;
                }
                // Otherwise proceeds with the averaging.
                else
                {
                    // Two DateTimes that will hold the boundaries of our average.
                    DateTime newestTime = TIME_ARBITRARY;
                    DateTime lastTime = TIME_ARBITRARY;

                    // A time 5 seconds ago. Method will only average over the last 5 seconds.
                    DateTime thresholdTime = System.DateTime.Now.Subtract(TIME_SPAN_5_SECONDS);

                    // Accumulator.
                    long totalBits = 0;
                    // Tracks times through the following foreach loop.
                    int irel = 0;

                    // Cycles through the list in newest to oldest order.
                    foreach (KeyValuePair<DateTime, int> point in rate)
                    {
                        // Sets the first boundary for the average.
                        if (totalBits == 0)
                        {
                            newestTime = point.Key;
                        }

                        // Accumulates the amount of data.
                        totalBits += point.Value;
                        // Updates the last boundary.
                        lastTime = point.Key;

                        // Breaks if we've gone beyond the desired time span.
                        if (point.Key < thresholdTime)
                        {
                            break;
                        }

                        // Updates number of times we've been through the foreach.
                        irel++;
                    }

                    // Returns 0 if we don't have any datapoints.
                    if(irel <= 0)
                    {
                        return 0;
                    }
                    // Otherwise proceeds with averaging.
                    else
                    {
                        // Calculates the time interval.
                        double delta = newestTime.Subtract(lastTime).TotalSeconds;

                        if (delta == 0)
                        {
                            // If the interval is zero avoids dividing by zero and returns total bits.
                            return (double)totalBits;
                        }
                        else
                        {
                            // Returns the average data rate.
                            return ((double)totalBits) / delta;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
