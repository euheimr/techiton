using UnityEngine;
using System;
using UnityEditor;

/// <summary>
/// Original code by SirGru @ http://www.ennoble-studios.com/tuts/making-unitys-www-class-work-in-editor-scripts.html
/// </summary>

namespace gamedev.net.editor
{
    public class EditorWWWImageDownloader
    {
        private WWW _www;
        private Action<string, Texture> _operateWWWResult;
        private object[] _arguments;

        public void StartWWW(string path, WWWForm form, Action<string, Texture> operateWWWResult, params object[] arguments)
        {
            _operateWWWResult = operateWWWResult;
            _arguments = arguments;
            if (form != null)
            {
                _www = new WWW(path, form);
            }
            else
            {
                _www = new WWW(path);
            }
            EditorApplication.update += Tick;
        }

        public void Tick()
        {
            if (_www.isDone)
            {
                EditorApplication.update -= Tick;
                if (!string.IsNullOrEmpty(_www.error))
                {
                    Debug.LogError("Error during WWW process:\n" + _www.error);
                }
                else
                {
                    if (_operateWWWResult != null) _operateWWWResult(_www.url, _www.texture);
                    else Debug.LogError("No result for " + _www.url);
                }
                _www.Dispose();
            }
        }

        public void StopWWW()
        {
            EditorApplication.update -= Tick;
            if (_www != null) _www.Dispose();
        }
    }
}