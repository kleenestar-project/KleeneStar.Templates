namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// An automation rule a template sets up in the workspace it creates
    /// (<see cref="WorkspaceTemplateBase.Automations"/>, set up by <see cref="TemplateAutomations"/>).
    /// </summary>
    /// <remarks>
    /// The rule acts as the workspace's creator; created without one, it is stored disabled.
    /// The actions follow the automation script syntax
    /// (<c>KleeneStar.Automation/docs/kleenestar.automation.md</c>) and may carry the placeholders
    /// of an approval step: <c>{creator}</c> for the creator's user name and
    /// <c>{namespace:key}</c> for a translated text, so a comment a rule writes arrives in the
    /// creator's language and a state it moves to matches the translated workflow.
    /// </remarks>
    public sealed class WorkspaceTemplateAutomation
    {
        /// <summary>Gets the name.</summary>
        public string Name { get; init; }

        /// <summary>Gets the description.</summary>
        public string Description { get; init; }

        /// <summary>Gets the class the rule is narrowed to, by its name, or <c>null</c>.</summary>
        public string Class { get; init; }

        /// <summary>Gets the trigger key.</summary>
        public string Trigger { get; init; }

        /// <summary>Gets the trigger parameter.</summary>
        public string Parameter { get; init; }

        /// <summary>Gets the WQL condition.</summary>
        public string Condition { get; init; }

        /// <summary>Gets the actions, one per line.</summary>
        public string Actions { get; init; }
    }
}
