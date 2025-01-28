using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace BiliExtract.Controls.Custom;

public class FluentIconButton : Wpf.Ui.Controls.Button
{
    protected override AutomationPeer OnCreateAutomationPeer() => new FluentIconButtonAutomationPeer(this);

    private class FluentIconButtonAutomationPeer(FluentIconButton owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(FluentIconButton);

        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;

        public override object? GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ItemContainer)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetNameCore()
        {
            var result = base.GetNameCore() ?? string.Empty;

            if (result == string.Empty)
            {
                result = AutomationProperties.GetName(owner);
            }

            return result;
        }
    }
}
