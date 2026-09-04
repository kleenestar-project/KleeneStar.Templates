using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using System.Collections.Generic;

namespace KleeneStar.Templates.WebWorkspaceTemplate
{
    /// <summary>
    /// A workspace for running a service desk: what users report, what it turns out to be, and
    /// what is done about it.
    /// </summary>
    /// <remarks>
    /// Three of its classes are marked portal-visible, because a service desk is the one
    /// workspace whose work is started by the people outside it: a ticket, an incident and a
    /// service request are the request types customers file, and the rest of the workspace is
    /// what the team does with them.
    /// </remarks>
    public sealed class ServiceDeskTemplate : WorkspaceTemplateBase
    {
        /// <inheritdoc/>
        protected override string Slug => "servicedesk";

        /// <inheritdoc/>
        protected override string IconName => "sd";

        /// <inheritdoc/>
        public override string SuggestedKey => "SD";

        /// <inheritdoc/>
        public override int Order => 10;

        /// <inheritdoc/>
        public override IEnumerable<string> Categories => ["Support"];

        /// <inheritdoc/>
        public override IEnumerable<WorkspaceTemplateClass> Classes =>
        [
            Class("Ticket", "ticket", portalVisible: true),
            Class("Incident", "incident", portalVisible: true),
            Class("ServiceRequest", "servicerequest", portalVisible: true),
            Class("Problem", "problem"),
            Class("Change", "change"),
            Class("Knowledge", "knowledge", ObjectKind.Document),
            Class("Announcement", "release", ObjectKind.Blog)
        ];
    }
}
