using KleeneStar.Core.WebWorkspaceTemplate;
using KleeneStar.Model.Entities;
using KleeneStar.Templates.WebWorkspaceTemplate;
using WebExpress.WebUI.WebIcon;

namespace KleeneStar.Templates.Test.WebWorkspaceTemplate
{
    /// <summary>
    /// Provides unit tests for <see cref="WorkspaceTemplateBase"/> - the convention that lets a
    /// template name itself once and have its key, its two internationalization keys and its icon
    /// follow.
    /// </summary>
    /// <remarks>
    /// The shipped templates are asserted against the convention elsewhere; here it is the
    /// convention itself under test, driven by probes rather than by the catalogue. That keeps
    /// the two apart: a change to the derivation should fail here once, and not seven times over
    /// in a test that was meant to be about the service desk.
    /// </remarks>
    public class UnitTestWorkspaceTemplateBase
    {
        /// <summary>
        /// A template that overrides nothing it does not have to - the defaults under test.
        /// </summary>
        private sealed class ProbeTemplate : WorkspaceTemplateBase
        {
            protected override string Slug => "probe";

            public override string SuggestedKey => "PRB";

            public override IEnumerable<WorkspaceTemplateClass> Classes =>
            [
                Class("Ticket", "ticket"),
                Class("UseCase", "usecase", ObjectKind.Document, portalVisible: true, @sealed: true)
            ];
        }

        /// <summary>
        /// A template whose icon is not named after its slug, the way the shipped ones abbreviate.
        /// </summary>
        private sealed class AbbreviatedProbeTemplate : WorkspaceTemplateBase
        {
            protected override string Slug => "servicedesk";

            protected override string IconName => "sd";

            public override string SuggestedKey => "SD";

            public override IEnumerable<WorkspaceTemplateClass> Classes => [];
        }

        /// <summary>
        /// The key, the name and the description are all built from the one slug.
        /// </summary>
        [Fact]
        public void TheSlugDrivesTheKeys()
        {
            var template = new ProbeTemplate();

            Assert.Equal("kleenestar.templates.probe", template.Key);
            Assert.Equal("kleenestar.templates:template.probe.name", template.Name);
            Assert.Equal("kleenestar.templates:template.probe.description", template.Description);
        }

        /// <summary>
        /// The icon defaults to the slug and is taken from the host's workspace icons.
        /// </summary>
        [Fact]
        public void TheIconDefaultsToTheSlug()
        {
            var icon = Assert.IsType<ImageIcon>(new ProbeTemplate().Icon);

            Assert.Equal("/kleenestar/assets/icons/probe.svg", icon.Uri.ToString());
        }

        /// <summary>
        /// A template whose icon is named differently keeps its own keys.
        /// </summary>
        /// <remarks>
        /// The abbreviation is the reason the icon name is a separate override rather than the
        /// slug itself: the icon is <c>sd</c> and the translations are keyed
        /// <c>servicedesk</c>, and collapsing the two would rename one of them.
        /// </remarks>
        [Fact]
        public void TheIconNameIsIndependentOfTheKeys()
        {
            var template = new AbbreviatedProbeTemplate();
            var icon = Assert.IsType<ImageIcon>(template.Icon);

            Assert.Equal("/kleenestar/assets/icons/sd.svg", icon.Uri.ToString());
            Assert.Equal("kleenestar.templates.servicedesk", template.Key);
            Assert.Equal("kleenestar.templates:template.servicedesk.name", template.Name);
        }

        /// <summary>
        /// A template that declares nothing further is filed under no category and offered last.
        /// </summary>
        [Fact]
        public void TheDefaultsAreLastAndUncategorized()
        {
            var template = new ProbeTemplate();

            Assert.Equal(100, template.Order);
            Assert.Empty(template.Categories);
        }

        /// <summary>
        /// A declared class takes its description key from the template's slug and its own name,
        /// lower cased, and is otherwise an ordinary public issue class.
        /// </summary>
        [Fact]
        public void ADeclaredClassTakesTheDefaults()
        {
            var ticket = new ProbeTemplate().Classes.First();

            Assert.Equal("Ticket", ticket.Name);
            Assert.Equal("kleenestar.templates:template.probe.class.ticket", ticket.Description);
            Assert.Equal("/kleenestar/assets/icons/ticket.svg", ticket.Icon);
            Assert.Equal(ObjectKind.Issue, ticket.Kind);
            Assert.False(ticket.PortalVisible);
            Assert.False(ticket.Sealed);
            Assert.Equal(AccessModifier.Public, ticket.AccessModifier);
        }

        /// <summary>
        /// A multi-word class name is lower cased whole for its key, and the declared kind,
        /// visibility and sealing are carried through.
        /// </summary>
        /// <remarks>
        /// Whole rather than per word: <c>UseCase</c> is keyed <c>usecase</c>, and a language
        /// file that spelled it <c>use.case</c> would leave the key on the card.
        /// </remarks>
        [Fact]
        public void ADeclaredClassCarriesWhatItWasGiven()
        {
            var useCase = new ProbeTemplate().Classes.Last();

            Assert.Equal("kleenestar.templates:template.probe.class.usecase", useCase.Description);
            Assert.Equal(ObjectKind.Document, useCase.Kind);
            Assert.True(useCase.PortalVisible);
            Assert.True(useCase.Sealed);
        }
    }
}
