﻿using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace BiliExtract.Controls.Custom;

public class Badge : Wpf.Ui.Controls.Badge
{
    protected override AutomationPeer OnCreateAutomationPeer() => new BadgeAutomationPeer(this);

    private class BadgeAutomationPeer(Badge owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(Badge);

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;

        public override object? GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ItemContainer)
                return this;

            return base.GetPattern(patternInterface);
        }

        protected override AutomationPeer? GetLabeledByCore()
        {
            if (owner.Content is UIElement element)
                return CreatePeerForElement(element);

            return base.GetLabeledByCore();
        }

        protected override string GetNameCore()
        {
            var result = base.GetNameCore() ?? string.Empty;

            if (result == string.Empty)
                result = AutomationProperties.GetName(owner);

            if (result == string.Empty && owner.Content is DependencyObject d)
                result = AutomationProperties.GetName(d);

            if (result == string.Empty && owner.Content is string s)
                result = s;

            return result;
        }
    }
}
