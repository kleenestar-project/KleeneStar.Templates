![KleeneStar](https://raw.githubusercontent.com/kleenestar-project/.github/main/docs/assets/img/banner.png)

# KleeneStar.Templates

The workspace templates shipped with **KleeneStar**: the shapes a new workspace can be created
in, and the classes each of them starts with.

A template is a **class**, not a row. What a service desk workspace consists of — a ticket, an
incident, a service request, a problem, a change, a knowledge base and an announcement channel,
three of them open to the customer portal — is knowledge somebody wrote down once, and it belongs
where the rest of that knowledge lives: in an assembly that can be installed, versioned and
removed. Kept in the installation's own data instead, it would have to be seeded, migrated and
held in step with every deployment by hand.

That is also why the templates are a plugin of their own rather than part of the core. The
catalogue an administrator picks from is exactly as long as the set of installed plugins says,
so an installation adds its own shapes, or drops the ones shipped here, without touching
anything else.

## What is in it

| Template               | Key    | Starts with
|------------------------|--------|--------------------------------------------------------
| Service Desk           | `SD`   | Ticket, Incident, ServiceRequest, Problem, Change, Knowledge, Announcement
| Software Development   | `DEV`  | Task, Bug, Sprint, Repository, BuildPipeline, Documentation, Release
| Configuration Database | `CMDB` | Asset, Relationship, ChangeRequest, Vulnerability, Compliance, Policy, Approval
| Finance and Controlling| `FIN`  | Budget, Invoice, CostCenter, Contract, Forecast, Approval
| Human Resources        | `HR`   | Employee, OrganizationUnit, Position, Onboarding, Absence, Training
| Product Management     | `PM`   | Feature, Requirement, Roadmap, ReleasePlan, UseCase, Stakeholder
| Procurement            | `PROC` | PurchaseOrder, Supplier, Contract, Tender, Delivery, Invoice

The key is a proposal the creation wizard fills in, not a constraint — a second workspace from
the same template needs a different one anyway.

## Writing a template

Implement `KleeneStar.Core.WebWorkspaceTemplate.IWorkspaceTemplate` in a public, non-abstract
class with a parameterless constructor. The core's `WorkspaceTemplateManager` discovers it by
scanning the assembly of every installed plugin, so there is nothing to register:

```csharp
public sealed class ResearchTemplate : WorkspaceTemplateBase
{
    protected override string Slug => "research";

    public override string SuggestedKey => "RES";

    public override IEnumerable<WorkspaceTemplateClass> Classes =>
    [
        Class("Study", "task"),
        Class("Dataset", "asset", ObjectKind.Asset),
        Class("Paper", "doc", ObjectKind.Document)
    ];
}
```

`WorkspaceTemplateBase` is the convenience of this plugin, not of the contract: it derives the
key, both i18n keys and the icon path from `Slug`, so the four cannot drift apart. A template
outside this plugin may implement the interface directly.

Names of classes are data an administrator renames and are therefore **not** translated; their
descriptions are, under `kleenestar.templates:template.{slug}.class.{name}`.

## Building

```
dotnet build KleeneStar.Templates/src/KleeneStar.Templates/KleeneStar.Templates.csproj
```

The host references the project, so the plugin is loaded with it; nothing has to be deployed
separately in a development checkout.

## License

MIT.
