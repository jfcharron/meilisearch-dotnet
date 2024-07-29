using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Meilisearch
{
    /// <summary>
    /// Represents the search similar document api route request body.
    /// </summary>
    /// <seealso>https://www.meilisearch.com/docs/reference/api/similar</seealso>
    public class SearchSimilarDocumentsQuery
    {
        /// <summary>
        /// Creates a new instance of <see cref="SearchSimilarDocumentsQuery"/> with the target
        /// document identifier  and default value for the remaining properties.
        /// </summary>
        /// <param name="id">identifier of the target document</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="id"/> is null, empty or only whitespaces
        /// </exception>
        public SearchSimilarDocumentsQuery(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Value cannot be null, empty or only whitespaces.",
                    nameof(id));
            }

            Id = id;
        }

        /// <summary>
        /// Gets or sets the identifier of the target document
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the embedder to use when computing recommendations
        /// </summary>
        [JsonPropertyName("embedder")]
        public string Embedder { get; set; }

        /// <summary>
        /// Gets or sets the filter to apply to the query.
        /// </summary>
        [JsonPropertyName("filter")]
        public dynamic Filter { get; set; }

        /// <summary>
        /// Gets or sets attributes to retrieve.
        /// </summary>
        [JsonPropertyName("attributesToRetrieve")]
        public IEnumerable<string> AttributesToRetrieve { get; set; }
        // pagination:

        /// <summary>
        /// Gets or sets offset for the Query.
        /// </summary>
        [JsonPropertyName("offset")]
        public int? Offset { get; set; }

        /// <summary>
        /// Gets or sets limits the number of results.
        /// </summary>
        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// Gets or sets if the response should include documents' global ranking score.
        /// </summary>
        [JsonPropertyName("showRankingScore")]
        public bool ShowRankingScore { get; set; }

        /// <summary>
        /// Gets or sets if the response should include detailed ranking score information
        /// </summary>
        [JsonPropertyName("showRankingScoreDetails")]
        public bool ShowRankingScoreDetails { get; set; }

        /// <summary>
        /// Gets or sets if the server should exclude results with a ranking score lower than
        /// this property's value.
        /// </summary>
        [JsonPropertyName("rankingScoreThreshold")]
        public int RankingScoreThreshold { get; set; }

        /// <summary>
        /// Gets or sets if the response should include documents vector data.
        /// </summary>
        [JsonPropertyName("retrieveVectors")]
        public bool RetrieveVectors { get; set; }
    }
}
