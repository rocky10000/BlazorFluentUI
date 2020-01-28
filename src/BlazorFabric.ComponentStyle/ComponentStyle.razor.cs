using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Reflection;
using System;

namespace BlazorFabric
{
    public partial class ComponentStyle : ComponentBase, IComponentStyleSheet
    {
        public static ICollection<IComponentStyleSheet> CStyleSheets { get; set; } = new HashSet<IComponentStyleSheet>();

        private string css;
        private ICollection<UniqueRule> rules;

        [Parameter]
        public ICollection<UniqueRule> Rules
        {
            get => rules;
            set
            {
                if (value == rules)
                {
                    return;
                }
                rules = value;
                RulesChanged.InvokeAsync(value);
            }
        }
        [Parameter] public EventCallback<ICollection<UniqueRule>> RulesChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CStyleSheets.Add(this);
            SetSelectorNames();
            await base.OnInitializedAsync();
        }

        protected override void OnParametersSet()
        {
            PrintRules();
            base.OnParametersSet();

        }

        //Private functionality

        private void SetSelectorNames()
        {
            foreach (var rule in rules)
            {
                if (!rule.Selector.UniqueName)
                    continue;
                if (string.IsNullOrWhiteSpace(rule.Selector.SelectorName))
                {
                    rule.Selector.SelectorName = $"css-{CStyleSheets.ToList().IndexOf(this)}-{rules.ToList().IndexOf(rule)}";
                }
                else
                {
                    rule.Selector.SelectorName = $"{rule.Selector.SelectorName}-{CStyleSheets.ToList().IndexOf(this)}-{rules.ToList().IndexOf(rule)}";
                }
            }
            RulesChanged.InvokeAsync(rules);
        }

        private void PrintRules()
        {
            css = "";
            foreach (var rule in rules)
            {
                css += $"{rule.Selector.GetSelectorAsString()}{{";
                foreach (var property in rule.Properties.GetType().GetProperties())
                {
                    string cssProperty = "";
                    string cssValue = "";
                    Attribute attribute = null;

                    //Catch Ignore Propertie
                    attribute = property.GetCustomAttribute(typeof(CsIgnoreAttribute));
                    if (attribute != null)
                        continue;

                    attribute = property.GetCustomAttribute(typeof(CsPropertyAttribute));
                    if (attribute != null)
                    {
                        if ((attribute as CsPropertyAttribute).IsCssStringProperty)
                        {
                            css += property.GetValue(rule.Properties)?.ToString();
                            continue;
                        }

                        cssProperty = (attribute as CsPropertyAttribute).PropertyName;
                    }
                    else
                    {
                        cssProperty = property.Name;
                    }

                    cssValue = property.GetValue(rule.Properties)?.ToString();
                    if (cssValue != null)
                    {
                        css += $"{cssProperty.ToLower()}:{(string.IsNullOrEmpty(cssValue) ? "\"\"" : cssValue)};";
                    }
                }
                css += "}";
            }
        }
    }
}