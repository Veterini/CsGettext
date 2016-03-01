using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using Oodrive.GetText.Classic.Resources;

namespace Oodrive.GetText.Classic.Extensions
{
    /// <summary>
    /// This class supports text localization in XAML.
    /// </summary>
    /// <remarks>
    /// <see cref="PoResourceManager"/>s used in XAML via the <see cref="GetTextExtension"/> class must be registered
    /// with the <see cref="GetTextResources"/> class.
    /// If you change the current culture at runtime, call the <see cref="Invalidate()"/> method to refresh all instances
    /// of <see cref="GetTextExtension"/>.
    /// </remarks>
    [ExcludeFromCodeCoverage] // Not testable: relies on dependency property mechanisms, won't work in unit test environment
    public static class GetTextResources
    {
        // DependencyObjects that have dependent IResourceDependent objects
        private static readonly HashSet<WeakReference> AllWithDependentObjects
            = new HashSet<WeakReference>(new WeakReferenceComparer<DependencyObject>());

        private static readonly Dictionary<string, PoResourceManager> PoResourceManagers = new Dictionary<string, PoResourceManager>();

        public static void AddResourceManager(string key, PoResourceManager manager)
        {
            PoResourceManagers.Add(key, manager);
            Invalidate();
        }

        private static CultureInfo _language;
        public static CultureInfo Language {
            get { return _language; }
            set
            {
                if (Equals(_language, value)) return;

                _language = value;

                foreach (var poResourceManager in PoResourceManagers.Values)
                {
                    poResourceManager.Language = _language;
                }
                Invalidate();
            }
        }

        public static void RemoveResourceManager(string key)
        {
            PoResourceManagers.Remove(key);
            Invalidate();
        }

        public static PoResourceManager GetResourceManager(string key)
        {
            key = key ?? DefaultResourceManagerKey ?? string.Empty;
            PoResourceManager rm;
            return PoResourceManagers.TryGetValue(key, out rm) ? rm : null;
        }

        private static string _defaultResourceManagerKey;
        public static string DefaultResourceManagerKey
        {
            get { return _defaultResourceManagerKey; }
            set
            {
                _defaultResourceManagerKey = value;
                Invalidate();
            }
        }

        public static void Invalidate()
        {
            AllWithDependentObjects.RemoveWhere(r => !Invalidate(r));
        }

        private static bool Invalidate(WeakReference r)
        {
            var target = r.Target as DependencyObject;
            if (target == null)
                return false;
            Invalidate(target);
            return true;
        }

        static void Invalidate(DependencyObject obj)
        {
            var list = GetDependentObjects(obj, false);

            // Invalidates live objects and removes garbage collected objects
            list?.ForEach(rd => rd.Invalidate(obj));
        }

        internal static void RegisterDependentObject(DependencyObject obj, IResourceDependent rd)
        {
            var list = GetDependentObjects(obj, true);
            list.Add(rd);
            AllWithDependentObjects.Add(new WeakReference(obj));
        }

        internal static void UnregisterDependentObject(DependencyObject obj, IResourceDependent rd)
        {
            var list = GetDependentObjects(obj, false);
            if (list == null)
                return;
            list.RemoveAll(r => r == rd);
            if (list.Count == 0)
                SetDependentObjects(obj, null);
        }

        private static List<IResourceDependent> GetDependentObjects(DependencyObject obj, bool create)
        {
            var list = GetDependentObjects(obj);
            if (list != null || !create) return list;
            list = new List<IResourceDependent>();
            SetDependentObjects(obj, list);
            return list;
        }

        private static List<IResourceDependent> GetDependentObjects(DependencyObject obj)
        {
            return (List<IResourceDependent>)obj.GetValue(DependentObjectsPropertyKey.DependencyProperty);
        }

        private static void SetDependentObjects(DependencyObject obj, List<IResourceDependent> value)
        {
            obj.SetValue(DependentObjectsPropertyKey, value);
        }

        private static readonly DependencyPropertyKey DependentObjectsPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("DependentObjects", typeof(List<IResourceDependent>), typeof(GetTextResources), new UIPropertyMetadata(null));

        private class WeakReferenceComparer<T> : IEqualityComparer<WeakReference>
            where T : class
        {
            private readonly IEqualityComparer<T> _valueComparer;

            public WeakReferenceComparer(IEqualityComparer<T> valueComparer = null)
            {
                _valueComparer = valueComparer ?? EqualityComparer<T>.Default;
            }

            public bool Equals(WeakReference x, WeakReference y)
            {
                return _valueComparer.Equals(x.Target as T, y.Target as T);
            }

            public int GetHashCode(WeakReference obj)
            {
                var target = obj.Target as T;
                if (target != null)
                    return _valueComparer.GetHashCode(target);
                return 0;
            }
        }
    }

    internal interface IResourceDependent
    {
        void Invalidate(DependencyObject obj);
    }
}
