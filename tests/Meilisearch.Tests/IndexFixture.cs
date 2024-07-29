using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Meilisearch.Extensions;

using Xunit;

namespace Meilisearch.Tests
{
    public abstract class IndexFixture : IAsyncLifetime
    {
        public IndexFixture()
        {
            DefaultClient = new MeilisearchClient(MeilisearchAddress(), ApiKey);
            var httpClient = new HttpClient(new MeilisearchMessageHandler(new HttpClientHandler())) { BaseAddress = new Uri(MeilisearchAddress()) };
            ClientWithCustomHttpClient = new MeilisearchClient(httpClient, ApiKey);
        }

        private const string ApiKey = "masterKey";

        public virtual string MeilisearchAddress()
        {
            throw new InvalidOperationException("Please override the MeilisearchAddress property in inhereted class.");
        }

        public MeilisearchClient DefaultClient { get; private set; }
        public MeilisearchClient ClientWithCustomHttpClient { get; private set; }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await DeleteAllIndexes(); // Let a clean Meilisearch instance, for maintainers convenience only.

        public async Task<Index> SetUpEmptyIndex(string indexUid, string primaryKey = default)
        {
            var task = await DefaultClient.CreateIndexAsync(indexUid, primaryKey);

            // Check the index has been created
            var finishedTask = await DefaultClient.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception("The index was not created in SetUpEmptyIndex. Impossible to run the tests.");
            }

            return DefaultClient.Index(indexUid);
        }

        public async Task<Index> SetUpBasicIndex(string indexUid)
        {
            var index = DefaultClient.Index(indexUid);
            var movies = await JsonFileReader.ReadAsync<List<Movie>>(Datasets.MoviesWithStringIdJsonPath);
            var task = await index.AddDocumentsAsync(movies);

            // Check the documents have been added
            var finishedTask = await index.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception("The documents were not added during SetUpBasicIndex. Impossible to run the tests.");
            }

            return index;
        }

        public async Task<Index> SetUpBasicIndexWithIntId(string indexUid)
        {
            var index = DefaultClient.Index(indexUid);
            var movies = await JsonFileReader.ReadAsync<List<MovieWithIntId>>(Datasets.MoviesWithIntIdJsonPath);
            var task = await index.AddDocumentsAsync(movies);

            // Check the documents have been added
            var finishedTask = await index.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception("The documents were not added during SetUpBasicIndexWithIntId. Impossible to run the tests.");
            }

            return index;
        }

        public async Task<Index> SetUpIndexForFaceting(string indexUid)
        {
            var index = DefaultClient.Index(indexUid);

            // Add documents
            var movies = await JsonFileReader.ReadAsync<List<Movie>>(Datasets.MoviesForFacetingJsonPath);
            var task = await index.AddDocumentsAsync(movies);

            // Check the documents have been added
            var finishedTask = await index.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception("The documents were not added during SetUpIndexForFaceting. Impossible to run the tests.");
            }

            // task settings
            var settings = new Settings
            {
                FilterableAttributes = new string[] { "genre" },
            };
            task = await index.UpdateSettingsAsync(settings);

            // Check the settings have been added
            finishedTask = await index.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception("The settings were not added during SetUpIndexForFaceting. Impossible to run the tests.");
            }

            return index;
        }

        public async Task<Index> SetUpIndexForNestedSearch(string indexUid)
        {
            var index = DefaultClient.Index(indexUid);
            var movies = await JsonFileReader.ReadAsync<List<MovieWithInfo>>(Datasets.MoviesWithInfoJsonPath);
            var task = await index.AddDocumentsAsync(movies);

            // Check the documents have been added
            var finishedTask = await index.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception("The documents were not added during SetUpIndexForNestedSearch. Impossible to run the tests.");
            }

            return index;
        }
        public async Task<Index> SetUpIndexForDistinctProductsSearch(string indexUid)
        {
            var index = DefaultClient.Index(indexUid);
            var products = await JsonFileReader.ReadAsync<List<Product>>(Datasets.ProductsForDistinctJsonPath);
            var task = await index.AddDocumentsAsync(products, primaryKey: "id");
            // Check the documents have been added
            var finishedTask = await index.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception($"The documents were not added during SetUpIndexForDistinctProductsSearch.\n" +
                    $"Impossible to run the tests.\n" +
                    $"{JsonSerializer.Serialize(finishedTask.Error)}");
            }

            var settings = new Settings
            {
                FilterableAttributes = new string[] { "product_id" },
            };
            task = await index.UpdateSettingsAsync(settings);

            // Check the settings have been added
            finishedTask = await index.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception($"The documents were not added during SetUpIndexForDistinctProductsSearch.\n" +
                    $"Impossible to run the tests.\n" +
                    $"{JsonSerializer.Serialize(finishedTask.Error)}");
            }

            return index;
        }

        public async Task<Index> SetUpIndexForSimilarSearch(string indexUid)
        {

            var content
                = new Dictionary<string, object> { { "vectorStore", true } };

            var http = new HttpClient();
            http.BaseAddress = new Uri( MeilisearchAddress());
            http.AddApiKeyToHeader(ApiKey);
            http.AddDefaultUserAgent();
            await http.PatchAsJsonAsync("experimental-features", content);

            // var settings = new ExpandoObject();
            //
            // await http.PatchAsJsonAsync($"indexes/{indexUid}/settings", new ExpandoObject
            // {
            //     embedders {
            //
            //     }
            // })

            var index = DefaultClient.Index(indexUid);


            var brand1 = "luxe";
            var brand2 = "frime";

            var products = new []
            {
                // brand1
                new Product { Id = 1, ProductId = "1", Brand = brand1, Color = "bleu" },
                new Product { Id = 2, ProductId = "2", Brand = brand1, Color = "blanc" },
                new Product { Id = 3, ProductId = "3", Brand = brand1, Color = "noir" },
                // brand2
                new Product { Id = 4, ProductId = "4", Brand = brand2, Color = "violet" },
                new Product { Id = 5, ProductId = "5", Brand = brand2, Color = "rouge" }
            };

            var task = await index.AddDocumentsAsync(products, primaryKey: "id");
            // Check the documents have been added
            var finishedTask = await index.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception($"The documents were not added during SetUpIndexForSimilarSearch.\n" +
                                    $"Impossible to run the tests.\n" +
                                    $"{JsonSerializer.Serialize(finishedTask.Error)}");
            }

            var settings = new Settings
            {
                FilterableAttributes = new [] { "product_id" },
            };
            task = await index.UpdateSettingsAsync(settings);

            // Check the settings have been added
            finishedTask = await index.WaitForTaskAsync(task.TaskUid);
            if (finishedTask.Status != TaskInfoStatus.Succeeded)
            {
                throw new Exception($"The documents were not added during SetUpIndexForSimilarSearch.\n" +
                                    $"Impossible to run the tests.\n" +
                                    $"{JsonSerializer.Serialize(finishedTask.Error)}");
            }

            return index;
        }

        public async Task DeleteAllIndexes()
        {
            var indexes = await DefaultClient.GetAllIndexesAsync();
            foreach (var index in indexes.Results)
            {
                await index.DeleteAsync();
            }
        }
    }
}
