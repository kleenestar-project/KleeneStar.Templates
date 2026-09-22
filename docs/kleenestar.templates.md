![KleeneStar](https://raw.githubusercontent.com/kleenestar-project/.github/main/docs/assets/img/banner.png)

# KleeneStar Workspace Template Catalogue Concept

In **KleeneStar**, a workspace is created *from a template*: a description of what the workspace is for and which classes it starts with. The mechanism — the contract, the discovery, the application of a template to a new workspace — belongs to the core and is specified in [Workspace Templates](https://github.com/kleenestar-project/KleeneStar.Core/blob/HEAD/docs/kleenestar.workspacetemplate.md). This document specifies the other half: the **catalogue** an administrator actually picks from, and the plugin that ships it.

The distinction matters, because the two are deliberately separable. The core knows how to find a template and how to apply one; it ships none. `KleeneStar.Templates` ships seven, and nothing else — no page, no endpoint, no entity, no manager. An installation that wants different shapes drops this plugin and ships its own beside the core, and the catalogue is exactly as long as the set of installed plugins says it is.

The catalogue bundles:
- Seven workspace shapes, each stating what the work in it is, which key it proposes and which categories it belongs under.
- The classes each shape starts with, with the object kind they hold, their portal visibility and their sealing.
- A common base class that derives a template's key, both internationalization keys and its icon path from one slug, so the four cannot drift apart.
- The translations of everything the catalogue says, in two cultures, embedded in the assembly.

## Lifecycle and States

A template has no lifecycle in the installation's data, because it is not in the installation's data. It is a type in an assembly, and its states are the states of the plugin that carries it: absent before the plugin is installed, registered while it is, and unknown again once it is removed.

Registration happens twice over, from two different directions, and this is load-bearing rather than defensive. The manager is constructed while the **core's** own components register — before every other plugin has arrived — so its constructor sweeps the plugins already known and its `AddPlugin` subscription catches the rest. A manager written with only one of the two passes would work in exactly half the installations, and would look correct in both halves of the source.

Applying a template is not a state transition of the template. It is a one-way projection: the classes it describes are created in a workspace, and nothing links that workspace back to the template afterwards. A workspace that has drifted from the shape it was created in is not out of date, it is finished — which is why removing the plugin costs the workspaces it created nothing at all.

The following state diagram visualizes the lifecycle:

```
╔══════════════════════════════════════════════════════════════════════════════════════╗
║                    KleeneStar Workspace Template Lifecycle Diagram                   ║
╠══════════════════════════════════════════════════════════════════════════════════════╣
║                                                                                      ║
║                       install plugin                                                 ║
║                    ┌─────────────────────┐        ╔══════════════╗                   ║
║                    │                     └────────► registered   ║                   ║
║              ┌─────┴────┐                         ╚═══╤══════╤═══╝                   ║
║              │  absent  │   uninstall plugin          │      │                       ║
║              └─────▲────┘◄────────────────────────────┘      │ apply                 ║
║                    │                                         │                       ║
║                    │                            ┌────────────▼─────────────┐         ║
║                    │                            │  classes in a workspace  │         ║
║                    │                            └────────────┬─────────────┘         ║
║                    │        a recorded key naming nothing    │                       ║
║                    └─────────────────────────────────────────┘                       ║
║                             (ordinary, not an error)                                 ║
║                                                                                      ║
╚══════════════════════════════════════════════════════════════════════════════════════╝
```

## The Catalogue

The seven templates mirror the workspaces the installation seeds, which is not a coincidence: the seeded workspaces were the first answer to "what does a workspace of this kind consist of", and a template is that answer written down where it can be installed rather than migrated.

|Template                |Key    |Order|Categories                 |Classes
|------------------------|-------|-----|---------------------------|--------------------------------------------------------
|Service Desk            |`SD`   |10   |Support                    |Ticket, Incident, ServiceRequest, Problem, Change, Knowledge, Announcement
|Software Development    |`DEV`  |20   |Engineering                |Task, Bug, Sprint, Repository, BuildPipeline, Documentation, Specification, Release
|Configuration Database  |`CMDB` |30   |Infrastructure, Compliance |Asset, Relationship, ChangeRequest, Vulnerability, Compliance, Policy, Approval
|Finance and Controlling |`FIN`  |40   |Finance                    |Budget, Invoice, CostCenter, Contract, Forecast, Approval
|Human Resources         |`HR`   |50   |HumanResources             |Employee, OrganizationUnit, Position, Onboarding, Absence, Training
|Product Management      |`PM`   |60   |Development                |Feature, Requirement, Roadmap, ReleasePlan, UseCase, Stakeholder
|Procurement             |`PROC` |70   |Operations                 |PurchaseOrder, Supplier, Contract, Tender, Delivery, Invoice

The key in the table is the *suggested* workspace key, a proposal the wizard fills the key field with rather than a constraint — a second workspace from the same template needs a different one anyway. The order is what decides the sequence of the cards; equal orders would fall back to the template key, so they are kept distinct on purpose.

Four properties are set by only a few templates, and each of the four is a decision rather than a detail.

**Portal visibility — the service desk, three classes.** `Ticket`, `Incident` and `ServiceRequest` are marked `PortalVisible`, and nothing else in the catalogue is. A service desk is the one workspace whose work is *started by the people outside it*: those three are the request types customers file, and the rest of the workspace — the problem behind a run of incidents, the change that fixes it — is what the team does with them. Portal visibility is what decides whether people outside the organization can file and read objects of a class, so a template that set it by accident would publish internal work to customers the moment the workspace was created.

**Sealing — the configuration database, one class.** `Policy` is sealed, which means it may not be specialized further. That is the right answer for the governance a configuration is held to, and the wrong answer for everything a workspace is expected to grow.

**Object kinds beyond the issue — the service desk, development, and the CMDB.** Most classes in the catalogue hold issues, because most work is work items. Three templates deliberately do not stop there: `Knowledge`, `Documentation` and `Specification` hold documents, `Announcement` and `Release` hold posts, and `Asset` holds assets. The kind decides which overview view presents the class — documents form a page tree, posts a timeline, issues a filterable work-item list — so a template spanning three kinds is the useful example of a workspace that is more than a ticket list.

**An explicit renderer — development, one class.** `Specification` is a document like `Documentation` and stands in the same page tree, but names the **form** renderer: it opens as the structured input mask of its class's forms instead of as prose. Every other class in the catalogue leaves the renderer unset and follows its kind (prose for documents and posts, form for issues and assets). It is the one place a fresh workspace shows both surfaces of the document kind side by side — see [Object Renderers](https://github.com/kleenestar-project/KleeneStar.Core/blob/HEAD/docs/kleenestar.renderer.md).

|Class                                  |Template|Kind      |Note
|---------------------------------------|--------|----------|-------------------------------------------
|`Ticket`, `Incident`, `ServiceRequest` |SD      |issue     |portal-visible — filed by customers
|`Knowledge`                            |SD      |document  |the written answer, so the next person need not ask
|`Announcement`                         |SD      |blog      |maintenance, outages and news for the people served
|`Documentation`                        |DEV     |document  |specifications, guides and decisions worth keeping
|`Specification`                        |DEV     |document  |form renderer — structured sheets, captured field by field
|`Release`                              |DEV     |blog      |what shipped, and what changed with it
|`Asset`                                |CMDB    |asset     |the central class — a thing the organization runs
|`Policy`                               |CMDB    |issue     |sealed — the governance a configuration is held to

## Data Model

The catalogue has no data model, and that is the design. `IWorkspaceTemplate` and `WorkspaceTemplateClass` are contracts in the core, implemented by types in this plugin; neither is an entity, neither has an id, a workspace or a timestamp, and nothing here is ever written.

`WorkspaceTemplateClass` is deliberately *not* the `Class` entity. A template describes a class that does not exist yet and will exist many times over — once per workspace created from it — so an entity handed around with its id, workspace and timestamps left empty would invite exactly the bug of saving it. What the descriptor carries instead is the part somebody actually decided: the name, what it is for, which kind of object it holds, whether customers may see it, and whether it may be specialized.

The projection from the one to the other runs in one direction, once, at creation:

```
╔══════════════════════════════════════════════════════════════════════════════════════╗
║                      KleeneStar Workspace Template Data Model                        ║
╠══════════════════════════════════════════════════════════════════════════════════════╣
║                                                                                      ║
║  ┌KleeneStar.Templates (code)───────────────┐   ┌the installation (data)───────────┐ ║
║  │                                          │   │                                  │ ║
║  │           ┌────────────────────┐         │   │        ┌─────────────┐           │ ║
║  │           │ WorkspaceTemplate  │         │   │        │  Workspace  │           │ ║
║  │           │ Base               │         │   │        └──────┬──────┘           │ ║
║  │           └──────────Δ─────────┘         │   │               │ 1                │ ║
║  │                      ¦                   │   │               │                  │ ║
║  │        ┌─────────────┴─────────────┐     │   │               │ *                │ ║
║  │        │ ServiceDeskTemplate       │     │   │        ┌──────▼──────┐           │ ║
║  │        │ SoftwareDevelopment…      │     │   │        │    Class    │           │ ║
║  │        │ ConfigurationDatabase…    │     │   │        └──────┬──────┘           │ ║
║  │        │ FinanceTemplate           │     │   │               │ 1                │ ║
║  │        │ HumanResourcesTemplate    │     │   │               │ *                │ ║
║  │        │ ProductManagement…        │     │   │        ┌──────▼──────┐           │ ║
║  │        │ ProcurementTemplate       │     │   │        │   Object    │           │ ║
║  │        └─────────────┬─────────────┘     │   │        └─────────────┘           │ ║
║  │                      │ *                 │   │                                  │ ║
║  │        ┌─────────────▼─────────────┐     │   │                                  │ ║
║  │        │ WorkspaceTemplateClass    │     │   │                                  │ ║
║  │        │  Name, Description, Icon  ├─────┼───┼──── Apply ───────┐               │ ║
║  │        │  Kind, PortalVisible      │     │   │                  │               │ ║
║  │        │  Sealed, AccessModifier   │     │   │   one way, once, │               │ ║
║  │        └───────────────────────────┘     │   │   never linked ◄─┘               │ ║
║  │                                          │   │   back                           │ ║
║  └──────────────────────────────────────────┘   └──────────────────────────────────┘ ║
║                                                                                      ║
╚══════════════════════════════════════════════════════════════════════════════════════╝
```

## Software Architecture

This plugin contributes types, not behaviour. It has no manager of its own: `WorkspaceTemplateManager` in the core owns discovery, ordering and application, and reaches into this assembly rather than the other way round. The only code here beyond the seven templates is `WorkspaceTemplateBase`, and it exists for one reason.

A template states four things that all say the same word: its key (`kleenestar.templates.servicedesk`), the internationalization key of its name (`kleenestar.templates:template.servicedesk.name`), the one of its description, and — usually — its icon path. Spelled out four times they drift, and a drifted internationalization key does not fail: it shows up as the raw key printed on the card where the name should be. The base class therefore derives all four from a single `Slug`, and a template overrides `IconName` only where the icon is abbreviated (`sd` for `servicedesk`, `dev` for `development`, `fin`, `hr`, `pm`, `proc`).

`WorkspaceTemplateBase` is a convenience of *this* plugin, not part of the contract. A template shipped elsewhere may implement `IWorkspaceTemplate` directly, and the manager will not know the difference.

```
╔KleeneStar.Templates══════════════════════════════════════════════════════════════════╗
║                                                                                      ║
║                    ┌─────────────────────────────────────────┐                       ║
║                    │ <<Interface>>                           │                       ║
║                    │ IWorkspaceTemplate  (KleeneStar.Core)   │                       ║
║                    ├─────────────────────────────────────────┤                       ║
║                    │ Key:String                              │                       ║
║                    │ Name:String                             │                       ║
║                    │ Description:String                      │                       ║
║                    │ Icon:IIcon                              │                       ║
║                    │ SuggestedKey:String                     │                       ║
║                    │ Categories:IEnumerable<String>          │                       ║
║                    │ Order:Int32                             │                       ║
║                    │ Classes:IEnumerable<                    │                       ║
║                    │   WorkspaceTemplateClass>               │                       ║
║                    └────────────────Δ────────────────────────┘                       ║
║                                     ¦                                                ║
║                                     ¦                                                ║
║              ┌──────────────────────┴──────────────────────┐                         ║
║              │ <<abstract>>                                │                         ║
║              │ WorkspaceTemplateBase                       │                         ║
║              ├─────────────────────────────────────────────┤                         ║
║              │ Slug:String                     <<abstract>>│                         ║
║              │ IconName:String                  <<virtual>>│                         ║
║              ├─────────────────────────────────────────────┤                         ║
║              │ Key         => "kleenestar.templates."+Slug │                         ║
║              │ Name        => "…:template."+Slug+".name"   │                         ║
║              │ Description => "…:template."+Slug+".descr…" │                         ║
║              │ Icon        => "/…/icons/"+IconName+".svg"  │                         ║
║              │ Class(name,icon,kind,portal,sealed):        │                         ║
║              │   WorkspaceTemplateClass                    │                         ║
║              └──────────────────────Δ──────────────────────┘                         ║
║                                     ¦                                                ║
║      ┌──────────────────┬───────────┼───────────┬──────────────────┐                 ║
║      ¦                  ¦           ¦           ¦                  ¦                 ║
║ ┌────┴──────┐  ┌────────┴──────┐ ┌──┴─────┐ ┌───┴────────┐  ┌──────┴──────┐          ║
║ │ServiceDesk│  │SoftwareDevelo…│ │Configu…│ │Finance     │  │HumanResourc…│          ║
║ │ SD  10    │  │ DEV  20       │ │CMDB 30 │ │ FIN  40    │  │ HR  50      │          ║
║ └───────────┘  └───────────────┘ └────────┘ └────────────┘  └─────────────┘          ║
║                        ┌───────────────────┐  ┌──────────────────┐                   ║
║                        │ProductManagement… │  │Procurement       │                   ║
║                        │ PM  60            │  │ PROC  70         │                   ║
║                        └───────────────────┘  └──────────────────┘                   ║
║                                                                                      ║
╚══════════════════════════════════════════════════════════════════════════════════════╝
```

The plugin itself, `KleeneStarPlugin`, contributes no application, no page and no endpoint. It nonetheless **names** an application — the core's `KleeneStarApplication` — because the framework refuses a plugin that belongs to nothing. Naming it only associates the two; the application is defined in the core. It also declares dependencies on `webexpress.webapp` and `kleenestar.core`, which is what puts this plugin after the core in the load order: scanning it before the core has registered its component managers would find the templates and have nowhere to put them.

## Internationalization

Everything the catalogue says is a key. Class **names** are the single exception: a class name is data an administrator renames after creation, not a caption of the product, so `Incident` is written once and shown as it stands.

|What                |Key                                                     |Source
|--------------------|--------------------------------------------------------|----------------------------
|Plugin name         |`kleenestar.templates:plugin.name`                      |`[Name]` on the plugin
|Plugin description  |`kleenestar.templates:plugin.description`               |`[Description]` on the plugin
|Template name       |`kleenestar.templates:template.{slug}.name`             |derived from `Slug`
|Template description|`kleenestar.templates:template.{slug}.description`      |derived from `Slug`
|Class description   |`kleenestar.templates:template.{slug}.class.{name}`     |derived from `Slug` and the class name, lower-cased whole
|Class name          |—                                                       |literal, never translated

The prefix before the colon is the plugin id, which the framework derives from the assembly name — so renaming the assembly silently unhooks every translation in it. The class part of the key lower-cases the whole name rather than splitting it: `UseCase` is keyed `usecase`, and a language file spelling it `use.case` would leave the key standing on the card.

The two language files (`Internationalization/de`, `Internationalization/en`) are **embedded resources**. An embedded resource that stops being included is not a build error; it is an installation in which the whole plugin speaks in keys. Neither is a key defined twice an error: the first definition wins and the second is never reached, so correcting a translation by appending a line below leaves the wrong one in the interface with no trace of why.

## UI Concepts and Pages

This plugin ships no user interface. Its templates surface in exactly one place — the first step of the workspace creation wizard — and that fragment lives in the core. The mockups below therefore show where the catalogue *appears*, not what it contributes.

### Workspace Creation - Template (Wizard Step)

The step presents each template as a card in a two-column tile control: the icon the workspace will be created with, the template's name, its description, the suggested key as a badge, and — in the footer — the names of the classes it starts with. The classes are on the card because they are what is actually being chosen; a card that only named the template would ask the administrator to pick a shape sight unseen. Choosing a card projects its suggested key into the key field of the next step.

The cards are searchable by name and description. An **empty workspace** card is always present besides whatever the plugins offer, and it is shown whatever the search says: a workspace set up by hand has to stay one click away, and on an installation with no template plugin it is the only way through.

```
╔WebAppPage════════════════════════════════════════════════════════════════════════════╗
║┌Header──────────────────────────────────────────────────────────────────────────────┐║
║│ * KleeneStar     Workspace ▼   Dashboard ▼       [+ AddObject]         [Search]    │║
║└────────────────────────────────────────────────────────────────────────────────────┘║
║┌Breadcrumb──────────────────────────────────────────────────────────────────────────┐║
║│ / Workspaces / New                                                                 │║
║└────────────────────────────────────────────────────────────────────────────────────┘║
║┌Wizard──────────────────────────────────────────────────────────────────────────────┐║
║│  ● Template ─────────────── ○ Details                                              │║
║│                                                                                    │║
║│  What kind of workspace is this?                                    [Search     ]  │║
║│                                                                                    │║
║│  ┌──────────────────────────────────┐  ┌──────────────────────────────────┐        │║
║│  │ [sd]  Service Desk        ( SD ) │  │ [dev] Software Development (DEV) │        │║
║│  │                                  │  │                                  │        │║
║│  │ Take what users report, work out │  │ Where the code lives, what is    │        │║
║│  │ what it is, and do something     │  │ built from it, and what is still │        │║
║│  │ about it. Three of its classes   │  │ to be done to it. Spans work     │        │║
║│  │ are open to the customer portal. │  │ items, documents and posts.      │        │║
║│  ├──────────────────────────────────┤  ├──────────────────────────────────┤        │║
║│  │ Ticket, Incident, ServiceRequest,│  │ Task, Bug, Sprint, Repository,   │        │║
║│  │ Problem, Change, Knowledge, …    │  │ BuildPipeline, Documentation, …  │        │║
║│  └──────────────────────────────────┘  └──────────────────────────────────┘        │║
║│  ┌──────────────────────────────────┐  ┌──────────────────────────────────┐        │║
║│  │ [cmdb] Configuration Db  (CMDB)  │  │ [fin] Finance and Contr.  (FIN)  │        │║
║│  │ …                                │  │ …                                │        │║
║│  └──────────────────────────────────┘  └──────────────────────────────────┘        │║
║│  ┌──────────────────────────────────┐                                              │║
║│  │ [+]  Empty workspace             │  ← always offered, whatever the search says  │║
║│  │ Start with nothing and add the   │                                              │║
║│  │ classes yourself.                │                                              │║
║│  └──────────────────────────────────┘                                              │║
║│                                                                                    │║
║│                                                        [Cancel]     [Next ›]       │║
║└────────────────────────────────────────────────────────────────────────────────────┘║
║┌Footer──────────────────────────────────────────────────────────────────────────────┐║
║│ [Documentation]        |        KleeneStar v1.2.3        |      [Report a problem] │║
║└────────────────────────────────────────────────────────────────────────────────────┘║
╚══════════════════════════════════════════════════════════════════════════════════════╝
```

### Workspace Creation - Details (Wizard Step)

The second step is the paperwork: the name, the key, the categories, the description and who may see the workspace. The key field arrives pre-filled with the suggested key of the chosen template — the one field nobody has an opinion about until they have had to invent one — and stays editable, because a second workspace from the same template needs a different one.

The steps are in this order because the first is the decision and the second is the paperwork. Submitting the step creates the workspace and, immediately afterwards, the classes of the chosen template.

```
╔WebAppPage════════════════════════════════════════════════════════════════════════════╗
║┌Wizard──────────────────────────────────────────────────────────────────────────────┐║
║│  ✓ Template ─────────────── ● Details                                              │║
║│                                                                                    │║
║│  Name         [ IT Service Desk                                              ]     │║
║│  Key          [ SD                        ]  ← proposed by the chosen template     │║
║│  Categories   [ Support                   ▼]                                       │║
║│  Description  [                                                              ]     │║
║│               [                                                              ]     │║
║│  Visibility   ( ) Private   (•) Public                                             │║
║│                                                                                    │║
║│  Creates: Ticket, Incident, ServiceRequest, Problem, Change, Knowledge,            │║
║│           Announcement                                                             │║
║│                                                                                    │║
║│                                          [‹ Back]  [Cancel]     [Create]           │║
║└────────────────────────────────────────────────────────────────────────────────────┘║
╚══════════════════════════════════════════════════════════════════════════════════════╝
```

## Sitemap

This plugin registers no routes. The catalogue is reachable through the core's workspace creation route only, and is listed here for completeness:

|Path                          |Page                |Description
|------------------------------|--------------------|------------------------------------------------------------
|`/workspaces`                 |Workspace Management|Overview of the workspaces of the installation.
|`/workspaces/add`             |Workspace Creation  |The two-step wizard. Step one is the template catalogue.

A template has no route of its own, and deliberately so: there is nothing to link to. It has no detail view, no edit form and no delete dialog, because it is read-only at runtime — what an administrator changes is the workspace a template produced, never the template.

## API Interfaces (REST Endpoints)

This plugin exposes no endpoints. A template reaches the API as a value rather than as a resource: the wizard sends the chosen key in the `TemplateKey` field of the workspace creation payload, and the create endpoint applies it after the workspace has been stored.

|Endpoint                      |HTTP Method|Description
|------------------------------|-----------|-------------------------------------------------------------
|`/api/1/workspaces`           |POST       |Creates a workspace. A `TemplateKey` in the payload creates the template's classes afterwards.
|`/api/1/workspaces?id={id}`   |GET        |Retrieves a workspace. It does not report the template it came from — nothing is recorded.

Three behaviours of the create call are contractual rather than incidental:

- **A payload without `TemplateKey`, or carrying the wizard's `none` value, creates no classes.** That is the ordinary case for every caller that is not the wizard — the REST API itself, an import, a scripted setup — and it stays the default rather than an error.
- **An unknown key creates nothing and raises nothing.** A payload naming a template that has since been uninstalled must still produce a workspace.
- **Applying a template twice adds what is missing rather than a second set of everything.** A name the workspace already carries is skipped, so a retried create — or a template applied to a workspace somebody had already set up by hand — does the useful thing instead of the destructive one.

The classes are created **after** the workspace, because they belong to it: there is nothing to attach them to until it exists. A workspace stored while its classes failed is one an administrator can finish by hand; the other order would leave classes belonging to nothing.

## Template Events

This plugin raises no events. The two events that concern templates are raised by the core's manager when a plugin's contribution is registered or dropped, and are listed here because they are how the rest of the installation learns that this plugin arrived:

|Event Name               |Description
|-------------------------|-----------------------------------------------------------------------
|`AddWorkspaceTemplate`   |Raised once per template after a plugin's registrations have been filed.
|`RemoveWorkspaceTemplate`|Raised once per template when the plugin that defined it is removed.

Each event carries an `IWorkspaceTemplateContext`: the template itself, the type that defined it, and the plugin it came from. The registration is keyed by plugin precisely so that removal can be exact — a plugin that goes away takes its own templates with it and nobody else's.

The events fire *after* the registry has been filled rather than during, so a handler is entitled to ask the manager what it now holds.

## Permissions Model

Templates carry no permissions and are not administered. There is nothing to grant on a template: it cannot be read into anything, changed or deleted, and every installed one is offered to whoever can reach the creation wizard.

What is governed is the act the catalogue leads to. Creating a workspace, and with it the classes a template describes, is a workspace-scope decision, and the classes created inherit the ordinary chain — a grant on the workspace governs everything filed in it. The relevant permissions are specified in [Workspaces](https://github.com/kleenestar-project/KleeneStar.Core/blob/HEAD/docs/kleenestar.workspace.md) and [Classes](https://github.com/kleenestar-project/KleeneStar.Core/blob/HEAD/docs/kleenestar.class.md); the catalogue adds none of its own.

Two consequences are worth stating explicitly, because both are easy to expect otherwise:

- **`PortalVisible` on a template class is not a permission.** It is a property of the class that is created, and it decides whether objects of that class are offered in the customer portal. The service desk's three request types carry it; the permission model is what then decides who may see which of them.
- **`AccessModifier` is `Public` for every class in the catalogue.** A template describes a workspace somebody is setting up, not the permissions they will give it. Narrowing a class is a decision for afterwards — a template that made it in advance would hide classes from the very administrator who had just created them.

## Testing

`KleeneStar.Templates.Test` covers the catalogue rather than the mechanism; the manager's own behaviour — applying, refusing an unknown key, idempotence — is tested in `KleeneStar.Core.Test` against a probe template.

What is tested here is what the compiler cannot see. A template is discovered by interface and built by reflection, so the rules that decide whether it is offered at all hold at run time or not at all, and the rules that decide whether it is offered *correctly* fail as a raw key on a card rather than as an error.

|Suite                                |Covers
|-------------------------------------|--------------------------------------------------------
|`UnitTestWorkspaceTemplateCatalog`   |The shipped keys; the discovery contract (public, concrete, sealed, parameterless); unique keys, orders and suggested keys; icon paths; the derivation of both i18n keys from the key.
|`UnitTestWorkspaceTemplateClasses`   |Class names, compared the way `Apply` compares them; known object kinds; description keys; `AccessModifier`; that only the service desk is portal-visible and only `Policy` is sealed.
|`UnitTestWorkspaceTemplateBase`      |The slug convention itself, driven by probes rather than by the catalogue.
|`UnitTestTemplateTranslations`       |Every stated key translated in both cultures; nothing empty; no key defined twice; no orphaned key; both cultures carrying the same keys.
|`UnitTestWorkspaceTemplateDiscovery` |The real `WorkspaceTemplateManager` scanning this assembly — the half the core's tests leave out, because the core ships no templates to find.
|`UnitTestKleeneStarPlugin`           |That the plugin names an application, declares its dependencies, and that its id matches the translation prefix.

The catalogue tests read the templates through the **manager's own discovery filter** rather than through a hand-written list. A list would keep answering with a template the installation no longer sees.

## Related Concepts

- [Workspace Templates](https://github.com/kleenestar-project/KleeneStar.Core/blob/HEAD/docs/kleenestar.workspacetemplate.md) — the contract, the manager and the wizard. The authority on everything this document treats as given.
- [Workspaces](https://github.com/kleenestar-project/KleeneStar.Core/blob/HEAD/docs/kleenestar.workspace.md) — what a template creates.
- [Classes](https://github.com/kleenestar-project/KleeneStar.Core/blob/HEAD/docs/kleenestar.class.md) — what a template fills it with.
- [Customer Portal](https://github.com/kleenestar-project/KleeneStar.Protal/blob/HEAD/docs/kleenestar.portal.md) — what `PortalVisible` opens the service desk's three request types to.

## Conclusion

The document "KleeneStar Workspace Template Catalogue" specifies the seven workspace shapes shipped with **KleeneStar** and the plugin that carries them. It is deliberately a document about content rather than about machinery: the contract, the discovery and the application belong to the core and are specified there, and everything stated here is a decision about what a service desk, a development workspace or a configuration database consists of.

That content is the part with no compiler behind it. A template that stops satisfying the discovery filter is dropped without a word; a class name that drifts from its translation prints the key; a portal flag set by accident publishes internal work. The catalogue is therefore pinned by tests rather than by types, and the tests are named after the decisions rather than after the methods.

As a specification the document leaves the obvious extension open: an installation that needs a shape not offered here ships a plugin beside this one rather than editing it. Nothing in the catalogue is privileged — `KleeneStar.Templates` is discovered by the same sweep as any other plugin, and dropping it costs the installation its templates and nothing else.
