using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using NServiceKit.Common.Extensions;
using NServiceKit.Logging;
using NServiceKit.Logging.Support.Logging;
using NServiceKit.Service;
using NServiceKit.ServiceClient.Web;
using NServiceKit.ServiceModel.Serialization;
using NServiceKit.Text;
using NServiceKit.WebHost.Endpoints.Tests.Support.Host;

namespace NServiceKit.WebHost.Endpoints.Tests
{
    /// <summary>
    /// These tests fail with Unauthorized exception when left last to run, 
    /// so prefixing with '_' to hoist its priority till we find out wtf is up
    /// </summary>
    public abstract class SyncRestClientTests : IDisposable
    {
        /// <summary>The listening on.</summary>
        protected string ListeningOn = "http://localhost:";

        ExampleAppHostHttpListener appHost;

        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.Tests.SyncRestClientTests class.</summary>
        ///
        /// <param name="port">The port.</param>
        protected SyncRestClientTests(int port)
        {
            ListeningOn += port + "/";
        }

        /// <summary>Executes the test fixture set up action.</summary>
        [TestFixtureSetUp]
        public void OnTestFixtureSetUp()
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            appHost = new ExampleAppHostHttpListener();
            appHost.Init();
            appHost.Start(ListeningOn);
        }

        /// <summary>Executes the test fixture tear down action.</summary>
        [TestFixtureTearDown]
        public void OnTestFixtureTearDown()
        {
            Dispose();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (appHost == null) return;
            appHost.Dispose();
            appHost = null;
        }

        /// <summary>Creates rest client.</summary>
        ///
        /// <returns>The new rest client.</returns>
        protected abstract IRestClient CreateRestClient();
        //protected virtual IRestClient CreateRestClient()
        //{
        //    return new XmlServiceClient(ListeningOn);
        //}

        static object[] GetFactorialRoutes = { "factorial/3", "fact/3" };

        /// <summary>Can get factorial using rest client.</summary>
        ///
        /// <param name="path">Full pathname of the file.</param>
        [TestCase("factorial/3")]
        [TestCase("fact/3")]
        public void Can_GET_GetFactorial_using_RestClient(string path)
        {
            var restClient = CreateRestClient();

            var response = restClient.Get<GetFactorialResponse>(path);

            Assert.That(response, Is.Not.Null, "No response received");
            Assert.That(response.Result, Is.EqualTo(GetFactorialService.GetFactorial(3)));
        }

        /// <summary>Can get movies using rest client.</summary>
        ///
        /// <param name="path">Full pathname of the file.</param>
        [TestCase("movies")]
        [TestCase("custom-movies")]
        public void Can_GET_Movies_using_RestClient(string path)
        {
            var restClient = CreateRestClient();

            var response = restClient.Get<MoviesResponse>(path);

            Assert.That(response, Is.Not.Null, "No response received");
            Assert.That(response.Movies.EquivalentTo(ResetMoviesService.Top5Movies));
        }

        /// <summary>Can get single movie using rest client.</summary>
        ///
        /// <param name="path">Full pathname of the file.</param>
        [TestCase("movies/1")]
        [TestCase("custom-movies/1")]
        public void Can_GET_single_Movie_using_RestClient(string path)
        {
            var restClient = CreateRestClient();

            var response = restClient.Get<MovieResponse>(path);

            Assert.That(response, Is.Not.Null, "No response received");
            Assert.That(response.Movie.Id, Is.EqualTo(1));
        }

        /// <summary>Can post to add new movie using rest client.</summary>
        ///
        /// <param name="path">Full pathname of the file.</param>
        [TestCase("movies")]
        [TestCase("custom-movies")]
        public void Can_POST_to_add_new_Movie_using_RestClient(string path)
        {
            var restClient = CreateRestClient();

            var newMovie = new Movie {
                ImdbId = "tt0450259",
                Title = "Blood Diamond",
                Rating = 8.0m,
                Director = "Edward Zwick",
                ReleaseDate = new DateTime(2007, 1, 26),
                TagLine = "A fisherman, a smuggler, and a syndicate of businessmen match wits over the possession of a priceless diamond.",
                Genres = new List<string> { "Adventure", "Drama", "Thriller" },
            };

            var response = restClient.Post<MovieResponse>(path, newMovie);

            Assert.That(response, Is.Not.Null, "No response received");

            var createdMovie = response.Movie;
            Assert.That(createdMovie.Id, Is.GreaterThan(0));
            Assert.That(createdMovie.ImdbId, Is.EqualTo(newMovie.ImdbId));
        }

        /// <summary>Can deserialize XML movie response.</summary>
        ///
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        [Test]
        public void Can_Deserialize_Xml_MovieResponse()
        {
            try
            {
                var xml = "<MovieResponse xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.NServiceKit.net/types\"><Movie><Director>Edward Zwick</Director><Genres xmlns:d3p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\"><d3p1:string>Adventure</d3p1:string><d3p1:string>Drama</d3p1:string><d3p1:string>Thriller</d3p1:string></Genres><Id>6</Id><ImdbId>tt0450259</ImdbId><Rating>8</Rating><ReleaseDate>2007-01-26T00:00:00+00:00</ReleaseDate><TagLine>A fisherman, a smuggler, and a syndicate of businessmen match wits over the possession of a priceless diamond.</TagLine><Title>Blood Diamond</Title></Movie></MovieResponse>";
                var response = DataContractDeserializer.Instance.Parse<MovieResponse>(xml);
                var toXml = DataContractSerializer.Instance.Parse(response);
                Console.WriteLine("XML: " + toXml);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <summary>Can delete movie using rest client.</summary>
        ///
        /// <param name="postPath">  Full pathname of the post file.</param>
        /// <param name="deletePath">Full pathname of the delete file.</param>
        [TestCase("movies", "movies/")]
        [TestCase("custom-movies", "custom-movies/")]
        public void Can_DELETE_Movie_using_RestClient(string postPath, string deletePath)
        {
            var restClient = CreateRestClient();

            var newMovie = new Movie {
                ImdbId = "tt0450259",
                Title = "Blood Diamond",
                Rating = 8.0m,
                Director = "Edward Zwick",
                ReleaseDate = new DateTime(2007, 1, 26),
                TagLine = "A fisherman, a smuggler, and a syndicate of businessmen match wits over the possession of a priceless diamond.",
                Genres = new List<string> { "Adventure", "Drama", "Thriller" },
            };

            var response = restClient.Post<MovieResponse>(postPath, newMovie);
            var createdMovie = response.Movie;
            response = restClient.Delete<MovieResponse>(deletePath + createdMovie.Id);

            Assert.That(createdMovie, Is.Not.Null);
            Assert.That(response.Movie, Is.Null);
        }

        /// <summary>Can put complex type with custom path.</summary>
        [Test]
        public void Can_PUT_complex_type_with_custom_path()
        {
            var client = CreateRestClient();

            var request = new InboxPostResponseRequest {
                Id = 123,
                Responses = new List<PageElementResponseDTO> {
                    new PageElementResponseDTO {
                        PageElementId = 123,
                        PageElementResponse = "something",
                        PageElementType = "Question"
                    }
                }
            };

            var response = client.Put<InboxPostResponseRequestResponse>(
                "inbox/123/responses",
                request);

            Assert.That(response.Id, Is.EqualTo(request.Id));
            Assert.That(response.Responses[0].PageElementId,
                Is.EqualTo(request.Responses[0].PageElementId));
        }

        /// <summary>Does throw 400 for argument exceptions.</summary>
        [Test]
        public void Does_throw_400_for_Argument_exceptions()
        {
            var client = CreateRestClient();

            try
            {
                var response = client.Put<InboxPostResponseRequestResponse>(
                    "inbox/123/responses",
                    new InboxPostResponseRequest());

                response.PrintDump();

                Assert.Fail("Should throw");
            }
            catch (WebServiceException webEx)
            {
                Assert.That(webEx.StatusCode, Is.EqualTo(400));
            }
        }

        /// <summary>Does throw 400 for argument exceptions without response dt operating system.</summary>
        [Test]
        public void Does_throw_400_for_Argument_exceptions_without_response_DTOs()
        {
            var client = CreateRestClient();

            try
            {
                var response = client.Put<InboxPost>(
                    "inbox/123/responses",
                    new InboxPost { Throw = true });

                response.PrintDump();

                Assert.Fail("Should throw");
            }
            catch (WebServiceException webEx)
            {
                Assert.That(webEx.StatusCode, Is.EqualTo(400));
            }
        }
    }

    /// <summary>A JSON synchronise rest client tests.</summary>
    [TestFixture]
    public class JsonSyncRestClientTests : SyncRestClientTests
    {
        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.Tests.JsonSyncRestClientTests class.</summary>
        public JsonSyncRestClientTests() : base(8090)
        {
        }

        /// <summary>Creates rest client.</summary>
        ///
        /// <returns>The new rest client.</returns>
        protected override IRestClient CreateRestClient()
        {
            return new JsonServiceClient(ListeningOn);
        }

        /// <summary>Can use response filters.</summary>
        [Test]
        public void Can_use_response_filters()
        {
            var isActioncalledGlobal = false;
            var isActioncalledLocal = false;
            ServiceClientBase.HttpWebResponseFilter = r => isActioncalledGlobal = true;
            var restClient = (JsonServiceClient)CreateRestClient();
            restClient.LocalHttpWebResponseFilter = r => isActioncalledLocal = true;
            restClient.Get<MoviesResponse>("movies");
            Assert.That(isActioncalledGlobal, Is.True);
            Assert.That(isActioncalledLocal, Is.True);

            ServiceClientBase.HttpWebResponseFilter = null;
        }
    }

    /// <summary>A jsv synchronise rest client tests.</summary>
    [TestFixture]
    public class JsvSyncRestClientTests : SyncRestClientTests
    {
        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.Tests.JsvSyncRestClientTests class.</summary>
        public JsvSyncRestClientTests()
            : base(8093)
        {
        }

        /// <summary>Creates rest client.</summary>
        ///
        /// <returns>The new rest client.</returns>
        protected override IRestClient CreateRestClient()
        {
            return new JsvServiceClient(ListeningOn);
        }
    }

    /// <summary>An XML synchronise rest client tests.</summary>
    [TestFixture]
    public class XmlSyncRestClientTests : SyncRestClientTests
    {
        /// <summary>Initializes a new instance of the NServiceKit.WebHost.Endpoints.Tests.XmlSyncRestClientTests class.</summary>
        public XmlSyncRestClientTests()
            : base(8092)
        {
        }

        /// <summary>Creates rest client.</summary>
        ///
        /// <returns>The new rest client.</returns>
        protected override IRestClient CreateRestClient()
        {
            return new XmlServiceClient(ListeningOn);
        }
    }
}
