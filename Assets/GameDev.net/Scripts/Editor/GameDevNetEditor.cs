using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using gamedev.net.data;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web;


/// <summary>
/// Written by Cedric Riesner Consulting for GameDev.net, 2018
/// Edited by Kevin Hawkins of GameDev.net, 2018
/// (c) 2018 GameDev.net, LLC
/// </summary>

namespace gamedev.net.editor
{
    enum SearchFilterEnum
    {
        All = 0,
        blog_entry = 1 << 0,
        cms_records1 = 1 << 1,
        forums_topic = 1 << 2,
        cms_records2 = 1 << 3,
        cms_records3 = 1 << 4,
        cms_records12 = 1 << 5,
        calendar_event = 1 << 6,
        gallery_image = 1 << 7,
        links_link = 1 << 8,
        gamedevprojects_project = 1 << 9
    }

    enum SearchFilterTagsEnum
    {
        All = 0,
        Blogs = 1 << 0,
        Articles = 1 << 1,
        Forums = 1 << 2,
        News = 1 << 3,
        Contractors = 1 << 4,
        Events = 1 << 5,
        Calendar = 1 << 6,
        Images = 1 << 7,
        Links = 1 << 8,
        Projects = 1 << 9
    }

    public class GameDevNetEditor : EditorWindow
    {
        // Settings for the editor window.
        private static string EDITOR_TITLE = "GameDev.net";
        private static string EDITOR_LOGO = "Images/gamedevnetlogo";
        private static float WINDOW_MINIMUM_WIDTH = 300;
        private static float WINDOW_MINIMUM_HEIGHT = 500;
        private static Color TITLE_COLOR = new Color32(0x18, 0x75, 0xa8, 0xff);//Color.blue;
        private static string SEARCH_PLACEHOLDER = "Search...";
        private static string SEARCH_BUTTON_ICON = "ViewToolZoom";
        private static string SEARCH_BUTTON_TOOLTIP = "Search";
        private static string NO_AVATAR_ICON = "Images/gamedevnetlogo";
        private static int RESULTS_PER_PAGE = 20;
        private static int MAX_CACHE = 50;
        private static bool DEBUG_MESSAGES = false;

        #region Private static variables
        // A reference to the open editor window.
        private static EditorWindow _myWindow;

        // The API Key.
        private static string _API_KEY = "a5a66ac304a7f36ca7703f5ca3e3ed48";

        // The API URL.
        private static string _API_URL = "https://www.gamedev.net/api/";

        // The current scroll position of the results scroll view.
        private static Vector2 _scrollPosition;

        // The current search web request.
        // Handles the REST API Get requests.
        private static UnityWebRequest _searchWebRequest;

        // List of thumbnail URLs to be downloaded.
        private static List<string> _thumbnailDownloadQueue = new List<string>();

        // Dictionary of thumbnails that have finished download.
        // The key is the thumbnail URL. The value is the thumbnail itself.
        private static Dictionary<string, Texture> _thumbnailFinishedDownloads = new Dictionary<string, Texture>();

        // Dictionary of cached search results.
        // The key is the search string, the value is the cached search result.
        private static Dictionary<string, string> _resultCache = new Dictionary<string, string>();

        // The current search result.
        private static Search _currentSearch;

        // Determines a search is currently in progress.
        private static bool _isSearching = false;

        // Determines which filters are selected
        private static SearchFilterTagsEnum filterEnum = 0;

        #endregion

        #region Private variables
        // The current search term to search for.
        private string searchTerm = "";

        // The term from the previous search.
        // This is to determine whether or not to
        // reset the current page to 1.
        private string oldSearchTerm = "";

        // The page we are currently seeing.
        private int currentPage = 1;

        // If true, we only search in titles.
        private bool titlesOnly = false;

        // If true, we sort by creation date.
        // TODO: Implement
        private bool newest = true;

        // TODO: Implement
        private bool relevant = false;

        // Defines whether newest or relevant
        // is selected in the toggle group.
        private int selectedBool = 0;

        // Defines whether or not the currently
        // displayed search result is cached or not.
        private bool isCached = false;

        // The GUIStyle for the result titles.
        private GUIStyle linkStyle;
        #endregion

        /// <summary>
        /// Opens the editor window or focuses on it if already opened,
        /// and set the window title and dimensions.
        /// </summary>
        [MenuItem("Window/GameDev.net/Search")]
        public static void ShowWindow()
        {
            _myWindow = GetWindow(typeof(GameDevNetEditor));
            _myWindow.titleContent.text = EDITOR_TITLE;
            _myWindow.titleContent.tooltip = "Search GameDev.net";

            Texture editorIcon = Resources.Load<Texture>(EDITOR_LOGO);
            if (editorIcon != null)
            {
                _myWindow.titleContent.image = editorIcon;
            }

            _myWindow.minSize = new Vector2(WINDOW_MINIMUM_WIDTH, WINDOW_MINIMUM_HEIGHT);
            ((GameDevNetEditor)_myWindow).Search();
        }

        /// <summary>
        /// Remove HTML from string with Regex.
        /// </summary>
        public static string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        /// <summary>
        /// Cuts a texture into a circle given certain parameters.
        /// </summary>
        /// <param name="h">Source Height</param>
        /// <param name="w">Source Width</param>
        /// <param name="r">Radius</param>
        /// <param name="cx">Center X</param>
        /// <param name="cy">Center Y</param>
        /// <param name="sourceTex">Source texture</param>
        /// <returns></returns>
        static Texture2D CalculateTexture(int h, int w, float r, float cx, float cy, Texture2D sourceTex)
        {
            Color[] c = sourceTex.GetPixels(0, 0, sourceTex.width, sourceTex.height);
            Texture2D b = new Texture2D(h, w);
            for (int i = 0; i < (h * w); i++)
            {
                int y = Mathf.FloorToInt(((float)i) / ((float)w));
                int x = Mathf.FloorToInt(((float)i - ((float)(y * w))));
                if (r * r >= (x - cx) * (x - cx) + (y - cy) * (y - cy))
                {
                    b.SetPixel(x, y, c[i]);
                }
                else
                {
                    b.SetPixel(x, y, Color.clear);
                }
            }
            b.Apply();
            return b;
        }

        /// <summary>
        /// Draw the window.
        /// </summary>
        private void OnGUI()
        {
            // Initiate the title skin.
            linkStyle = new GUIStyle(GUI.skin.label);
            linkStyle.normal.textColor = TITLE_COLOR;
            linkStyle.alignment = TextAnchor.UpperLeft;
            linkStyle.fontStyle = FontStyle.Bold;
            linkStyle.fontSize = 13;

            // Draw search bar.
            GUIStyle searchTextStyle = new GUIStyle(GUI.skin.textField);
            searchTextStyle.fontSize = 13;
            searchTextStyle.alignment = TextAnchor.MiddleLeft;

            GUIStyle resultBoxStyle = new GUIStyle(GUI.skin.label);

            GUIStyle searchResultTextStyle = new GUIStyle(GUI.skin.label);
            searchResultTextStyle.fontSize = 13;
            searchResultTextStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                // When pressing enter and focused on the search bar,
                // initiate a search.
                if (Event.current.Equals(Event.KeyboardEvent("return")) && GUI.GetNameOfFocusedControl() == "searchBar")
                {
                    GUI.FocusControl("");
                    Search();
                }

                GUI.SetNextControlName("searchBar");
                searchTerm = EditorGUILayout.TextField(searchTerm, searchTextStyle, GUILayout.ExpandWidth(true), 
                                                                    GUILayout.MinWidth(200), 
                                                                    GUILayout.Height(26));
                searchTerm.PadLeft(5);
                searchTerm.PadRight(5);

                // Draw search placeholder.
                if (string.IsNullOrEmpty(searchTerm))
                {
                    GUI.Label(GUILayoutUtility.GetLastRect(), SEARCH_PLACEHOLDER, searchTextStyle);
                }

                // Draw the search button.
                if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent(SEARCH_BUTTON_ICON).image, SEARCH_BUTTON_TOOLTIP), GUILayout.Width(36), GUILayout.Height(24)))
                {
                    Search();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Draw filter bar.
            EditorGUILayout.BeginHorizontal();
            {
                titlesOnly = EditorGUILayout.ToggleLeft("Titles Only", titlesOnly, GUILayout.Width(80), GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                float oldLabelWidth = EditorGUIUtility.labelWidth;
                float oldFieldWidth = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.labelWidth = 35;
                EditorGUIUtility.fieldWidth = 90;
                filterEnum = (SearchFilterTagsEnum)EditorGUILayout.EnumPopup("Filter", filterEnum);
                EditorGUIUtility.labelWidth = oldLabelWidth;
                EditorGUIUtility.fieldWidth = oldFieldWidth;
                GUILayout.FlexibleSpace();
                selectedBool = GUILayout.SelectionGrid(selectedBool, new string[] { "Newest", "Relevant" }, 2, "toggle", GUILayout.ExpandWidth(false));
                newest = selectedBool == 0;
                relevant = !newest;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Draw results box.
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "Box");
            {
                if(!_isSearching && _currentSearch != null && (_currentSearch.results == null || _currentSearch.results.Length == 0))
                {
                    GUILayout.Label("No search results.", searchResultTextStyle);
                } else if(_isSearching)
                {
                    GUILayout.Label("Searching...", searchResultTextStyle);
                }

                if (_currentSearch != null && _currentSearch.results != null)
                {
                    // If we aren't currently searching, draw all results.
                    if (!_isSearching) { 
                        foreach (Result result in _currentSearch.results)
                        {
                            GUILayout.BeginHorizontal(resultBoxStyle);
                            {
                                // Draw the avatar thumbnail.
                                if (_thumbnailFinishedDownloads.ContainsKey(result.authorPhotoThumbnail))
                                {
                                    GUILayout.Label(new GUIContent("", _thumbnailFinishedDownloads[result.authorPhotoThumbnail], result.author), GUILayout.Width(40), GUILayout.Height(40));
                                }
                                else
                                {
                                    Texture noAvatarTexture = Resources.Load<Texture>(NO_AVATAR_ICON);
                                    noAvatarTexture.filterMode = FilterMode.Trilinear;
                                   
                                    GUILayout.Label(new GUIContent("", noAvatarTexture == null ? EditorGUIUtility.IconContent(NO_AVATAR_ICON, "").image : noAvatarTexture, result.author), GUILayout.Width(40), GUILayout.Height(40));
                                }

                                // Draw the title with a link to the post.
                                GUILayout.BeginVertical();
                                {
                                    if(GUILayout.Button(new GUIContent(result.title, WWW.UnEscapeURL((result.content.Length > 300 ? result.content.Remove(300) : result.content).Replace("\n", "")) + "..."), linkStyle))
                                    {
                                        // Opens the post in the browser.
                                        Application.OpenURL(result.objectUrl);
                                    }
                                    GUILayout.Label(result.GetDateFormatted());
                                }
                                GUILayout.EndVertical();
                            }
                            GUILayout.EndHorizontal();

                            // Draw the "results" section.
                            GUI.skin.label.alignment = TextAnchor.UpperLeft;
                            string text = result.comments + " replies";
                            GUI.Label(new Rect(Screen.width - GUI.skin.label.CalcSize(new GUIContent(text)).x * 2 + 10 + _scrollPosition.x, GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height - 20, 100, 22), text);
                            EditorGUILayout.Separator();
                        }
                    }
                }
            }
            GUILayout.EndScrollView();

            // Draw page bar
            GUILayout.Space(30);

            // Draw the page number.
            // If the current page is cached, display the "Cached" suffix.
            if(_currentSearch != null && _currentSearch.results != null && _currentSearch.results.Length > 0)
            {
                GUI.Label(new Rect(10, Screen.height - 48, 100, 22), "  " + currentPage + " / " + _currentSearch.totalPages + (isCached ? " (Cached)" : ""));
            } else
            {
                GUI.Label(new Rect(10, Screen.height - 48, 100, 22), "  - / -");
            }

            // Draw the "preview page" button.
            // Only enable it if we can go to the previous page.
            GUI.enabled = _currentSearch != null && _currentSearch.page > 1;
            if(GUI.Button(new Rect(Screen.width - 50, Screen.height - 48, 22, 22), "<"))
            {
                currentPage--;
                Search();
            }

            // Draw the "next page" button.
            // Only enable it if we can go to the next page.
            GUI.enabled = _currentSearch != null && _currentSearch.page < _currentSearch.totalPages;
            if(GUI.Button(new Rect(Screen.width - 27, Screen.height - 48, 22, 22), ">")) {
                currentPage++;
                Search();
            }

            // Re-enable the UI.
            GUI.enabled = true;
        }

        /// <summary>
        /// Handles search result fetching.
        /// </summary>
        static void EditorUpdate()
        {
            // If we are still fetching the result, wait.
            while (!_searchWebRequest.isDone)
            {
                return;
            }

            // If we have an error, display the error message.
            // Otherwise, process the search result.
            if (_searchWebRequest.isNetworkError)
            {
                Debug.LogError(_searchWebRequest.error);
            }
            else
            {
                // Deserialize the search result.
                _currentSearch = JsonConvert.DeserializeObject<Search>(_searchWebRequest.downloadHandler.text);

                // If the page isn't cached, cach it.
                if(!_resultCache.ContainsKey(_searchWebRequest.url + "%%%" + _currentSearch.page))
                {
                    _resultCache.Add(_searchWebRequest.url + "%%%" + _currentSearch.page, _searchWebRequest.downloadHandler.text);

                    // If the cache is full, remove the oldest cached page.
                    if(_resultCache.Count > MAX_CACHE)
                    {
                        List<string> keys = new List<string>(_resultCache.Keys);
                        _resultCache.Remove(keys[0]);
                    }
                }

                // Initiate the humbnail download for all results.
                foreach (Result r in _currentSearch.results)
                {
                    EditorWWWImageDownloader eWWW = new EditorWWWImageDownloader();
                    eWWW.StartWWW(r.authorPhotoThumbnail, null, RegisterThumbnail);
                }

                if (DEBUG_MESSAGES)
                {
                    Debug.Log(_searchWebRequest.downloadHandler.text);
                }
            }
            
            // After the search is completed, stop polling search updates.
            EditorApplication.update -= EditorUpdate;
            _isSearching = false;

            // Repaint the window to display new results.
            if (_myWindow != null)
            {
                _myWindow.Repaint();
            }
        }

        /// <summary>
        /// Registers (caches) a thumbnail that has finished downloading.
        /// </summary>
        /// <param name="avatarURL">The URL of the thumbnail.</param>
        /// <param name="thumbnail">The thumbnail itself.</param>
        public static void RegisterThumbnail(string avatarURL, Texture thumbnail)
        {
            if (!_thumbnailFinishedDownloads.ContainsKey(avatarURL))
            {
                _thumbnailFinishedDownloads.Add(avatarURL, CalculateTexture(thumbnail.height, thumbnail.width , thumbnail.width / 2, thumbnail.width/2, thumbnail.height/2, thumbnail as Texture2D));
            }

            // Try to re-capture the window isntance in case it is lost.
            if(_myWindow == null)
            {
                _myWindow = GetWindow(typeof(GameDevNetEditor));
            }
        }

        /// <summary>
        /// Commence a search with the currently entered search term.
        /// </summary>
        private void Search()
        {
            _isSearching = true;
            if (!oldSearchTerm.Equals(searchTerm))
            {
                currentPage = 1;
            }
            string typeFilter = "";
            if (((int)filterEnum & (int)SearchFilterEnum.blog_entry) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.blog_entry;
            }
            if (((int)filterEnum & (int)SearchFilterEnum.cms_records1) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.cms_records1;
            }
            if (((int)filterEnum & (int)SearchFilterEnum.forums_topic) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.forums_topic;
            }
            if (((int)filterEnum & (int)SearchFilterEnum.cms_records2) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.cms_records2;
            }
            if (((int)filterEnum & (int)SearchFilterEnum.cms_records3) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.cms_records3;
            }
            if (((int)filterEnum & (int)SearchFilterEnum.cms_records12) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.cms_records12;
            }
            if (((int)filterEnum & (int)SearchFilterEnum.calendar_event) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.calendar_event;
            }
            if (((int)filterEnum & (int)SearchFilterEnum.gallery_image) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.gallery_image;
            }
            if (((int)filterEnum & (int)SearchFilterEnum.links_link) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.links_link;
            }
            if (((int)filterEnum & (int)SearchFilterEnum.gamedevprojects_project) != 0)
            {
                typeFilter += "&type=" + SearchFilterEnum.gamedevprojects_project;
            }
            if(!string.IsNullOrEmpty(searchTerm))
            {
                //MakeAPIRequest("core/search/?", string.Format((titlesOnly ? "&search_in=titles" : "") + "&q={0}&page={1}&perPage={2}&type={3}&sortby=" + (newest ? "newest" : "relevancy") + typeFilter, WWW.EscapeURL(searchTerm), currentPage, RESULTS_PER_PAGE, "IPS%5cforums%5cTopic%5cPost"));
                MakeAPIRequest("core/search/?", string.Format((titlesOnly ? "&search_in=titles" : "") + "&q={0}&page={1}&perPage={2}&sortby=" + (newest ? "newest" : "relevancy") + typeFilter, WWW.EscapeURL(searchTerm), currentPage, RESULTS_PER_PAGE));
            }
            else
            {
                //MakeAPIRequest("core/search/?", string.Format((titlesOnly ? "&search_in=titles" : "") + "&page={0}&perPage={1}&type={2}&sortby=" + (newest ? "newest" : "relevancy") + typeFilter, currentPage, RESULTS_PER_PAGE, "IPS%5cforums%5cTopic%5cPost"));
                MakeAPIRequest("core/search/?", string.Format((titlesOnly ? "&search_in=titles" : "") + "&page={0}&perPage={1}&sortby=" + (newest ? "newest" : "relevancy") + typeFilter, currentPage, RESULTS_PER_PAGE));        
            }

            oldSearchTerm = searchTerm;
        }

        /// <summary>
        /// Makes an API request.
        /// </summary>
        /// <param name="requestString">Target endpoint.</param>
        /// <param name="parameters">Parameters.</param>
        private void MakeAPIRequest(string requestString, string parameters)
        {
            if(DEBUG_MESSAGES)
            {
                Debug.Log(_API_URL + requestString + "key=" + _API_KEY + parameters);
            }

            // Create the new get request.
            _searchWebRequest = UnityWebRequest.Get(_API_URL + requestString + "key=" + _API_KEY + parameters);

            // Determine whether or not to use a cached page.
            if(_resultCache.ContainsKey(_searchWebRequest.url + "%%%" + currentPage))
            {
                if (DEBUG_MESSAGES)
                {
                    Debug.Log("Using cached result");
                }
                _currentSearch = JsonConvert.DeserializeObject<Search>(_resultCache[_searchWebRequest.url + "%%%" + currentPage]);
                _isSearching = false;
                isCached = true;
            } else { 
                // Fetch the search results.
                // Start polling search updates.
                _searchWebRequest.SendWebRequest();
                EditorApplication.update += EditorUpdate;
                isCached = false;
            }
        }
    }
}
