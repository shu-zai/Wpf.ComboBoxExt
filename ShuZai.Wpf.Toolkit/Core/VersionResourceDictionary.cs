using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace ShuZai.Wpf.Toolkit.Core
{
    public class VersionResourceDictionary : ResourceDictionary, ISupportInitialize
    {
        private int _initializingCount;
        private string _assemblyName;
        private string _sourcePath;


        public VersionResourceDictionary() { }

        public VersionResourceDictionary(string assemblyName, string sourcePath)
        {
            ((ISupportInitialize)this).BeginInit();
            AssemblyName = assemblyName;
            SourcePath = sourcePath;
            ((ISupportInitialize)this).EndInit();
        }

        public string AssemblyName
        {
            get { return _assemblyName; }
            set
            {
                EnsureInitialization();
                _assemblyName = value;
            }
        }

        public string SourcePath
        {
            get { return _sourcePath; }
            set
            {
                EnsureInitialization();
                _sourcePath = value;
            }
        }

        private void EnsureInitialization()
        {
            if (_initializingCount <= 0)
                throw new InvalidOperationException("VersionResourceDictionary properties can only be set while initializing.");
        }

        void ISupportInitialize.BeginInit()
        {
            base.BeginInit();
            _initializingCount++;
        }

        void ISupportInitialize.EndInit()
        {
            _initializingCount--;
            Debug.Assert(_initializingCount >= 0);

            if (_initializingCount <= 0)
            {
                if (Source != null)
                    throw new InvalidOperationException("Source property cannot be initialized on the VersionResourceDictionary");

                if (string.IsNullOrEmpty(AssemblyName) || string.IsNullOrEmpty(SourcePath))
                    throw new InvalidOperationException("AssemblyName and SourcePath must be set during initialization");

                //Using an absolute path is necessary in VS2015 for themes different than Windows 8.
                string uriStr = string.Format(@"pack://application:,,,/{0};v{1};component/{2}", AssemblyName, Assembly.GetExecutingAssembly().GetName().Version, SourcePath);
                Source = new Uri(uriStr, UriKind.Absolute);
            }

            base.EndInit();
        }


        private enum InitState
        {
            NotInitialized,
            Initializing,
            Initialized
        };
    }
}
