namespace KRouter.Core.DRC
{
    using KRouter.Core.DRC.Models;

    public interface IRuleEngine
    {
        void LoadRules(DesignRules rules);
    }

    public class RuleEngine : IRuleEngine
    {
        public void LoadRules(DesignRules rules)
        {
            // Placeholder implementation
        }
    }
}
