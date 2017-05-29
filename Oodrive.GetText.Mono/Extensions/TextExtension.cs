using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using NString;

namespace Oodrive.GetText.Mono.Extensions
{
    [ExcludeFromCodeCoverage] // Not testable: relies on dependency property mechanisms, won't work in unit test environment
    [MarkupExtensionReturnType(typeof(string))]
    public class TextExtension : MarkupExtension, IResourceDependent
    {
        public TextExtension()
        {
        }

        public TextExtension(string getTextKey)
        {
            TextKey = getTextKey;
        }

        [ConstructorArgument("TextKey")]
        public string TextKey { get; set; }

        public int? Occurence { get; set; }

        public string Context { get; set; }

        public IValueConverter Converter { get; set; }

        private DependencyProperty _targetProperty;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (TextKey.IsNullOrEmpty())
                throw new InvalidOperationException("TextKey is not set");

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
                TextResources.RegisterDependentObject(targetObj, this);

            string rmKey = null;
            string resourceKey;
            var parts = TextKey.Split(':');
            if (parts.Length > 1)
            {
                rmKey = parts[0];
                resourceKey = parts[1];
            }
            else
            {
                resourceKey = parts[0];
            }

            var resourceManager = TextResources.GetResourceManager(rmKey);

            if (resourceManager == null) return GetBaseText(resourceKey);

            object tempValue = string.Empty;

            if (!Occurence.HasValue && Context.IsNullOrEmpty())
                tempValue = resourceManager.GetString(resourceKey);

            if (Occurence.HasValue && Context.IsNullOrEmpty())
                tempValue = resourceManager.GetStringPlur(resourceKey, Occurence ?? -1);

            if (!Occurence.HasValue && !Context.IsNullOrEmpty())
                tempValue = resourceManager.GetStringCtxt(resourceKey, Context);

            if (Occurence.HasValue && !Context.IsNullOrEmpty())
                tempValue = resourceManager.GetStringPlurCtxt(resourceKey, Occurence ?? -1, Context);

            var value = Converter.Convert(tempValue, typeof(string), null, null)?.ToString() ?? tempValue;

            return value ?? GetBaseText(resourceKey);
        }

        private string GetBaseText(string resourceKey)
        {
            return Occurence.HasValue ? resourceKey : StringTemplate.Format(resourceKey, new { Occurence = Occurence ?? -1 });
        }

        public void Invalidate(DependencyObject obj)
        {
            if (_targetProperty == null)
                return;
            obj.SetValue(_targetProperty, ProvideValueCore(obj, false));
        }
    }
}