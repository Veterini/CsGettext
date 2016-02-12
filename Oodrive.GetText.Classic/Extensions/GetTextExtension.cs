using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using NString;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Oodrive.GetText.Classic.Extensions
{
    [ExcludeFromCodeCoverage] // Not testable: relies on dependency property mechanisms, won't work in unit test environment
    [MarkupExtensionReturnType(typeof(string))]
    public class GetTextExtension : MarkupExtension, IResourceDependent
    {
        public GetTextExtension()
        {
        }

        public GetTextExtension(string getTextKey)
        {
            GetTextKey = getTextKey;
        }

        [ConstructorArgument("getTextKey")]
        public string GetTextKey { get; set; }

        public int? Occurence { get; set; }

        public string Context { get; set; }

        public IValueConverter Converter { get; set; }

        private DependencyProperty _targetProperty;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (GetTextKey.IsNullOrEmpty())
                throw new InvalidOperationException("GetTextKey is not set");

            var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (target == null)
                return null;

            // Happens if evaluating in a template; return the markup extension itself so that it is reevaluated later
            if (target.TargetObject != null && target.TargetObject.GetType().FullName == "System.Windows.SharedDp")
            {
                return this;
            }

            _targetProperty = target.TargetProperty as DependencyProperty;
            return ProvideValueCore(target.TargetObject, true);
        }

        private object ProvideValueCore(object target, bool register)
        {
            var targetObj = target as DependencyObject;
            if (targetObj != null && register)
                GetTextResources.RegisterDependentObject(targetObj, this);

            string rmKey = null;
            string resourceKey;
            var parts = GetTextKey.Split(':');
            if (parts.Length > 1)
            {
                rmKey = parts[0];
                resourceKey = parts[1];
            }
            else
            {
                resourceKey = parts[0];
            }

            var resourceManager = GetTextResources.GetResourceManager(rmKey);

            if (resourceManager == null) return $"[{GetTextKey}]";
            var tempValue = resourceManager.GetString(resourceKey, Occurence, Context);
            var value = Converter?.Convert(tempValue, typeof(string), null, null).ToString() ?? tempValue;

            return value ?? $"[{GetTextKey}]";
        }

        public void Invalidate(DependencyObject obj)
        {
            if (_targetProperty == null)
                return;
            obj.SetValue(_targetProperty, ProvideValueCore(obj, false));
        }
    }
}
