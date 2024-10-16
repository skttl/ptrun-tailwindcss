using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using Community.PowerToys.Run.Plugin.TailwindCSS.Models;
using ManagedCommon;
using System;
using System.Collections.Generic;
using Wox.Infrastructure;
using Wox.Plugin;
using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;

namespace Community.PowerToys.Run.Plugin.TailwindCSS
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IDisposable
    {

        private SearchClient AlgoliaClient => new SearchClient(Constants.Settings.AlgoliaAppId, Constants.Settings.AlgoliaApiKey);

        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "C300A599B0534FF2942FB26B38CBE2B5";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "TailwindCSS";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "TailwindCSS Description";

        private PluginInitContext? Context { get; set; }

        private string? IconPath { get; set; }

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var search = query.Search;
            var results = new List<Result>();

            if (string.IsNullOrWhiteSpace(search) == false)
            {
                var response = AlgoliaClient.Search<SearchResult>(
                    new SearchMethodParams
                    {
                        Requests = new List<SearchQuery>
                        {
                            new SearchQuery(new SearchForHits { IndexName = Constants.Settings.AlgoliaIndexName, Query = search, HitsPerPage = 12 }),
                        }
                    }
                );

                foreach (var result in response.Results)
                {
                    var hits = result.AsSearchResponse().Hits;
                    foreach (var hit in hits)
                    {
                        results.Add(ResultFromHit(hit, search));
                    }
                };
            }

            if (results.Count == 0)
            {
                results.Add(new Result
                {
                    QueryTextDisplay = search,
                    IcoPath = IconPath,
                    Title = "No results for " + search,
                    ContextData = search,
                });
            }

            return results;
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/tailwindcss.light.png" : "Images/tailwindcss.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);

        private Result ResultFromHit(SearchResult hit, string search)
        {
            var result = new Result
            {
                QueryTextDisplay = search,
                IcoPath = IconPath,
                ContextData = search,
                Title = hit.Hierarchy?.ToString(),
                SubTitle = hit.Content,
                Action = action => Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, hit.Url),
            };

            return result;
        }
    }
}
