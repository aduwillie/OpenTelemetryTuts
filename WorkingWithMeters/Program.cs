using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using System.Security.Cryptography;

/**
 * 
 * EventCounters are the .NET API for performance metric collection.
 * They publish basic diagnostic information.
 * 
 * 2 categories of EventCounters:
 *  - rate: how often certain events occur over time eg. requests per second
 *  - snapshot: picture taken at a point in time eg. memory usage
 *  
 *  Each category has 2 different types of counters:
 *  - Polling counters: retrive their value via a callback
 *  - Non-polling counters: has value directly set on the counter instance
 *  
 *  Counters represented as following:
 *  1. EventCounter
 *  2. IncrementingEventCounter: for counters that only increase
 *  3. PollingCounter
 *  4. IncrementingPollingCounter
 * 
 */


var storeMeter = new Meter("Store");
var booksSold = storeMeter.CreateCounter<int>("Store.booksSold", description: "Number of books sold");

Console.WriteLine("Press any key to exit...");
while (!Console.KeyAvailable)
{
    booksSold.Add(RandomNumberGenerator.GetInt32(10));
    await Task.Delay(1000);

    BooksSoldEventCounterSource.Log.Sell(RandomNumberGenerator.GetInt32(100));

    Console.WriteLine("...");
}

[EventSource(Name = "Store2")]
sealed class BooksSoldEventCounterSource: EventSource
{
    private EventCounter booksSold;

    private BooksSoldEventCounterSource()
    {
        booksSold = new EventCounter("Store2.books-sold-2", this)
        {
            DisplayName = "Books Sold",
        };
    }

    public static BooksSoldEventCounterSource Log = new BooksSoldEventCounterSource();

    public void Sell(int count)
    {
        WriteEvent(eventId: 1, "Book sold", count);
        booksSold?.WriteMetric(count);
    }

    protected override void Dispose(bool disposing)
    {
        booksSold?.Dispose();

        base.Dispose(disposing);
    }
}
