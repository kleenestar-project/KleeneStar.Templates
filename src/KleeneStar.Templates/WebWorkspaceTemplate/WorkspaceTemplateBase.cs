using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;
using System.Linq;
using WebExpress.WebCore.WebIcon;
using WebExpress.WebUI.WebIcon;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// The common shape of the templates shipped here: the naming convention that ties a
    /// template's key to its translations, and a shorthand for declaring a class.
    /// </summary>
    /// <remarks>
    /// Every template in this plugin names itself once, in <see cref="Slug"/>, and everything
    /// else follows from it - the key, the two i18n keys and the icon path. A template that
    /// spelled all four out would let them drift apart, and a drifted i18n key shows up as the
    /// raw key on a card rather than as an error.
    /// </remarks>
    public abstract class WorkspaceTemplateBase : IWorkspaceTemplate
    {
        /// <summary>
        /// Gets the short name of the template, e.g. <c>servicedesk</c>. Lower case, no dots.
        /// </summary>
        protected abstract string Slug { get; }

        /// <summary>
        /// Gets the stable key of the template.
        /// </summary>
        public string Key => "kleenestar.templates." + Slug;

        /// <summary>
        /// Gets the internationalization key of the display name.
        /// </summary>
        public string Name => "kleenestar.templates:template." + Slug + ".name";

        /// <summary>
        /// Gets the internationalization key of the description.
        /// </summary>
        public string Description => "kleenestar.templates:template." + Slug + ".description";

        /// <summary>
        /// Gets the icon of the template. The icons are the host's own workspace icons, so a
        /// workspace created from a template looks like the one the template was modelled on.
        /// </summary>
        public virtual IIcon Icon => ImageIcon.FromString("/kleenestar/assets/icons/" + IconName + ".svg");

        /// <summary>
        /// Gets the file name of the icon, without path or extension. Defaults to the slug.
        /// </summary>
        protected virtual string IconName => Slug;

        /// <summary>
        /// Gets the suggested key of the workspace.
        /// </summary>
        public abstract string SuggestedKey { get; }

        /// <summary>
        /// Gets the categories the workspace is filed under.
        /// </summary>
        public virtual IEnumerable<string> Categories => [];

        /// <summary>
        /// Gets the display order among the templates offered.
        /// </summary>
        public virtual int Order => 100;

        /// <summary>
        /// Gets the classes the workspace starts with.
        /// </summary>
        public abstract IEnumerable<WorkspaceTemplateClass> Classes { get; }

        /// <summary>
        /// Declares one class of the template, with its structure.
        /// </summary>
        /// <remarks>
        /// Every structural part left <see langword="null"/> is taken from the defaults of the
        /// class's kind (<see cref="TemplateStructure.Resolve"/>); an empty collection says the
        /// class has none of it. <paramref name="fields"/> is merged into the base fields of the
        /// kind rather than replacing them, so a template names only what is particular to the
        /// class.
        /// </remarks>
        /// <param name="name">The class name.</param>
        /// <param name="icon">The file name of the icon, without path or extension.</param>
        /// <param name="kind">The kind of object the class holds.</param>
        /// <param name="renderer">The renderer its objects are read and written through, or null
        /// to follow the default of the kind - which is what a template that does not care
        /// leaves it at.</param>
        /// <param name="portalVisible">Whether objects of the class are offered in the customer
        /// portal.</param>
        /// <param name="sealed">Whether the class may not be specialized further.</param>
        /// <param name="fields">The fields particular to the class.</param>
        /// <param name="priorities">The priority scale, or null for the kind's.</param>
        /// <param name="workflow">The lifecycle, or null for the kind's.</param>
        /// <param name="calendars">The calendars, or null for the kind's.</param>
        /// <param name="slas">The service-level agreements, or null for the kind's.</param>
        /// <returns>The declared class.</returns>
        protected WorkspaceTemplateClass Class
        (
            string name,
            string icon,
            string kind = ObjectKind.Issue,
            string renderer = null,
            bool portalVisible = false,
            bool @sealed = false,
            IReadOnlyList<WorkspaceTemplateField> fields = null,
            IReadOnlyList<WorkspaceTemplatePriority> priorities = null,
            WorkspaceTemplateWorkflow workflow = null,
            IReadOnlyList<WorkspaceTemplateCalendar> calendars = null,
            IReadOnlyList<WorkspaceTemplateSla> slas = null,
            IReadOnlyList<WorkspaceTemplateApproval> approvals = null
        )
        {
            return TemplateStructure.Resolve
            (
                new WorkspaceTemplateClass
                {
                    Name = name,
                    Description = "kleenestar.templates:template." + Slug + ".class." + name.ToLowerInvariant(),
                    Icon = "/kleenestar/assets/icons/" + icon + ".svg",
                    Kind = kind,
                    Renderer = renderer,
                    PortalVisible = portalVisible,
                    Sealed = @sealed
                },
                fields,
                priorities,
                workflow,
                calendars,
                slas,
                approvals
            );
        }

        /// <summary>
        /// Gets what the home page of a workspace of this template says; the generic page unless
        /// the template says otherwise.
        /// </summary>
        public virtual WorkspaceTemplateHome Home => null;

        /// <summary>
        /// Declares the home page of the template. The texts are English and shipped as keys,
        /// like every other text of a template; they are translated once, when the workspace is
        /// created.
        /// </summary>
        /// <param name="lead">What the workspace is for.</param>
        /// <param name="sections">The sections below it, built with <see cref="Section"/> and
        /// <see cref="Steps"/>.</param>
        /// <returns>The home page.</returns>
        protected static WorkspaceTemplateHome HomePage(string lead, params WorkspaceTemplateHomeSection[] sections)
        {
            return new WorkspaceTemplateHome
            {
                Lead = TemplateText.Key(lead),
                Sections = sections ?? []
            };
        }

        /// <summary>
        /// Declares a section of the home page with a list of points.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="text">The paragraph under the heading, or <see langword="null"/>.</param>
        /// <param name="items">The points.</param>
        /// <returns>The section.</returns>
        protected static WorkspaceTemplateHomeSection Section(string heading, string text, params string[] items)
        {
            return new WorkspaceTemplateHomeSection
            {
                Heading = TemplateText.Key(heading),
                Text = TemplateText.Key(text),
                Items = [.. (items ?? []).Select(TemplateText.Key)]
            };
        }

        /// <summary>
        /// Declares a section of the home page with a sequence of numbered steps.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="text">The paragraph under the heading, or <see langword="null"/>.</param>
        /// <param name="steps">The steps, in order.</param>
        /// <returns>The section.</returns>
        protected static WorkspaceTemplateHomeSection Steps(string heading, string text, params string[] steps)
        {
            return new WorkspaceTemplateHomeSection
            {
                Heading = TemplateText.Key(heading),
                Text = TemplateText.Key(text),
                Items = [.. (steps ?? []).Select(TemplateText.Key)],
                Numbered = true
            };
        }

        /// <summary>
        /// Gets the automation rules a workspace of this template starts with; none unless the
        /// template says otherwise.
        /// </summary>
        public virtual IEnumerable<WorkspaceTemplateAutomation> Automations => [];

        /// <summary>
        /// Declares an approval policy of a class. The texts are English and shipped as keys;
        /// the states are named as the class's workflow names them.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">What it is for.</param>
        /// <param name="steps">The steps, built with <see cref="Step"/>.</param>
        /// <param name="request">The state that requests it.</param>
        /// <param name="approved">The state an approved object moves to.</param>
        /// <param name="rejected">The state a rejected object moves to.</param>
        /// <returns>The policy.</returns>
        protected static WorkspaceTemplateApproval Approval(string name, string description, IEnumerable<string> steps, string request, string approved, string rejected)
        {
            return new WorkspaceTemplateApproval
            {
                Name = name,
                Description = description,
                Steps = string.Join("\n", steps),
                RequestState = request,
                ApprovedState = approved,
                RejectedState = rejected
            };
        }

        /// <summary>
        /// Writes a step of an approval: its name, translated when the workspace is created, and
        /// the rule of the policy syntax (<c>any {creator} within 48h</c>).
        /// </summary>
        /// <param name="name">The English name of the step.</param>
        /// <param name="rule">Who decides, how many, by when.</param>
        /// <returns>The step line.</returns>
        protected static string Step(string name, string rule)
        {
            return Say(name) + ": " + rule;
        }

        /// <summary>
        /// Declares an automation rule. The name and description are English and shipped as
        /// keys; texts inside the actions are written with <see cref="Say"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">What it does.</param>
        /// <param name="class">The class it is narrowed to, or <see langword="null"/>.</param>
        /// <param name="trigger">The trigger key.</param>
        /// <param name="parameter">The trigger parameter, or <see langword="null"/>.</param>
        /// <param name="actions">The action lines.</param>
        /// <returns>The rule.</returns>
        protected static WorkspaceTemplateAutomation Automation(string name, string description, string @class, string trigger, string parameter, params string[] actions)
        {
            return new WorkspaceTemplateAutomation
            {
                Name = TemplateText.Key(name),
                Description = TemplateText.Key(description),
                Class = @class,
                Trigger = trigger,
                Parameter = parameter,
                Actions = string.Join("\n", actions)
            };
        }

        /// <summary>
        /// Embeds an English text in a step or an action line as a placeholder the core replaces
        /// with its translation when the workspace is created.
        /// </summary>
        /// <param name="text">The English text.</param>
        /// <returns>The placeholder.</returns>
        protected static string Say(string text)
        {
            return "{" + TemplateText.Key(text) + "}";
        }
    }
}
