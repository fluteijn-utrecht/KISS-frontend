using Kiss.Bff.Beheer.Verwerking;
using Microsoft.Extensions.Logging;
using static Kiss.Bff.Test.UnitTest1;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            //  var c = new HttpRequestMessage(HttpMethod.Post, "https://www.nu.nl");
            //  //var nepdb =....

            //  var x = new  Kiss.Bff.Beheer.Verwerking.VerwerkingMiddleware.VerwerkingsHttpClientMiddleware(null, nepdb, new dummyLogger());

            ////  x.SendAsync(null,c,null);


            //  var record = nepdb.verwerkingslog.ToList()
            //assert er is maar 1 item 
            //assert dat ene item bevat .Post, "https://www.nu.nl", en de juiste user..
            //  Assert.AreEqual(record.url , 1);

        }



    }

    internal class dummyLogger : ILogger<VerwerkingMiddleware.VerwerkingsHttpClientMiddleware>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            throw new NotImplementedException();
        }
    }
}
