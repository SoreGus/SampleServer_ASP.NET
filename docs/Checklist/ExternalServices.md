# Checklist — External Services and Costs

Last reviewed: November 21, 2026.

This document inventories every external service required by the product, when it enters the architecture, its free allowance, and its expected cost. Prices may change and must be reviewed before enabling a paid service.

Unless stated otherwise, prices are in USD, exclude taxes and currency conversion, and use São Paulo (`southamerica-east1`) for Google Cloud estimates.

## Adoption Phases

- **Development:** local application infrastructure. Only development secrets are stored in Google Cloud.
- **Launch:** production is available, but no customer is active yet.
- **First Customer:** backups and the protections required for live customer data are enabled before onboarding.
- **Growth:** services are enabled only when measured volume, security, or availability requirements justify their cost.

## Cost Summary

| Phase | Estimated fixed cost | Other costs |
| --- | ---: | --- |
| Development | **$0.00–$0.12/month** | Development secrets can remain free with at most six active versions. |
| Launch | **$15.01/month** in Google Cloud | Apple renewal: $99/year; domain: estimated $10–$20/year; Google Play registration: $25 once. |
| First Customer | **$17.56/month** in Google Cloud | Same annual costs, plus Stripe fees per successful payment. |
| Growth | Not fixed | Added only after its activation criteria are met. |

### Consolidated Totals

| Scenario | Monthly economic equivalent | First year | Following years |
| --- | ---: | ---: | ---: |
| Launch for the entire year | **$24.09–$24.93** | **$314.11–$324.11** | **$289.11–$299.11** |
| First Customer protections active for the entire year | **$26.64–$27.48** | **$344.71–$354.71** | **$319.71–$329.71** |

The first-year totals include Google Play's $25 registration. Apple is included as an economic cost even while the existing membership remains paid through its current renewal date. If the first customer arrives during the year, the actual total falls between the two scenarios.

The totals exclude taxes, foreign-exchange variation, payment fees, outbound network traffic, additional stored data, promotional credits, and any store commission that a future in-app purchase model may require.

## Accounts, Distribution, and Collaboration

| Service | Phase | Purpose | Free allowance | Price and decision |
| --- | --- | --- | --- | --- |
| [Apple Developer Program](https://developer.apple.com/programs/whats-included/) | Launch | iOS distribution, TestFlight, Sign in with Apple, and Apple platform credentials. | Existing membership includes 25 Xcode Cloud compute hours/month, but Xcode Cloud is not planned. | **$99/year** or local equivalent. The account already exists, but its renewal remains part of the budget. |
| [Google Play Console](https://support.google.com/googleplay/android-developer/answer/6112435) | Launch | Android distribution and release management. | No registration waiver assumed. | **$25 once**. Android builds and publication are performed manually from the owner's Mac. |
| [Google Identity / OAuth](https://developers.google.com/identity/protocols/oauth2) | Development | Google sign-in, with a separate client for each supported platform. | No usage charge is listed for ordinary OAuth client registration. | **$0** under the planned usage. OAuth quotas and consent-screen requirements still apply. |
| [GitHub Free](https://docs.github.com/en/billing/reference/product-usage-included) | Development | Private source repository and CI/CD for backend and web. | **$12/month** of Actions compute, 500 MB artifact storage, and 10 GB cache per repository. This is approximately 2,000 Linux two-core minutes at $0.006/minute. | **$0** while within the allowance. Paid usage must stop when the budget is reached. Mobile builds do not run in CI. |
| [GitHub Team](https://github.com/team) | Growth | Additional collaboration and repository controls if the team requires them. | Not applicable. | **$4/user/month**. Enable only when Free no longer meets collaboration requirements. |
| Domain registrar | Launch | Public product and API domain. | Registrar DNS is commonly included. | Exact TLD is not selected. Reserve **$10–$20/year** for a regular, non-premium domain and record the exact renewal price after purchase. |

Apple and Google may charge store commissions if the commercial model later uses in-app purchases. Apple generally applies a **30%** commission, reduced to **15%** for eligible developers in the [App Store Small Business Program](https://developer.apple.com/app-store/small-business-program/). [Google Play](https://support.google.com/googleplay/android-developer/answer/112622) applies **15%** to automatically renewing subscriptions. The current plan uses external B2B subscriptions through Stripe, so store commissions are excluded from the estimate. Store policies must be reviewed before each mobile release.

## Payments

| Service | Phase | Purpose | Free allowance | Price and decision |
| --- | --- | --- | --- | --- |
| [Stripe Payments Brazil](https://stripe.com/br/pricing) | Development / First Customer | Test payments in Development and collect production subscriptions after customer activation. | Test Mode, account creation, and standard integration have no monthly fee. | Domestic cards: **3.99% + R$0.39** per successful transaction on the general pricing page. International cards add **2%**. |
| [Stripe Billing](https://stripe.com/br/billing/pricing) | First Customer | Subscription lifecycle, invoices, and customer self-service. | No fixed monthly fee with pay-as-you-go pricing. | **0.7% of Billing volume**, in addition to payment processing. Its pricing page currently displays **3.99% + R$0.50** for cards. |

The two official Stripe pages currently show different fixed card fees. Until the account's contracted price is confirmed in the Stripe Dashboard, budgeting uses the conservative **4.69% + R$0.50** per domestic subscription charge.

Example: a R$100 domestic subscription produces an estimated **R$5.19** Stripe fee and **R$94.81** net before taxes, refunds, disputes, or other adjustments. An international card would add approximately R$2.00.

Stripe's optional custom domain for hosted Checkout and customer portal pages costs **$10/month** and is not required initially.

## Email

| Service | Phase | Purpose | Free allowance | Price and decision |
| --- | --- | --- | --- | --- |
| [Brevo SMTP](https://www.brevo.com/products/transactional-email/) | Development / Launch | Email confirmation, password recovery, invitations, security notifications, and other transactional email. | **300 emails/day**, approximately 9,000/month subject to the daily cap; SMTP and transactional email are included. The free plan has no time limit and may include Brevo branding. | Start at **$0**. The application remains SMTP-provider-neutral. |
| [Brevo paid plans](https://help.brevo.com/hc/en-us/articles/208589409-About-Brevo-s-pricing-plans) | Growth | Remove the daily limit, branding, or increase delivery capacity. | Not applicable. | Starter begins at **$9/month** for 5,000 emails; removing branding on Starter adds $9/month. Standard begins at **$18/month**. A dedicated IP is **$251/year** and is unnecessary initially. |

Development uses the same real SMTP delivery path as production so complete email flows can be tested without adding another provider. Test recipients and sending volume must remain controlled.

## Google Cloud — Initial Services

| Service | Phase | Purpose | Free allowance | Baseline estimate |
| --- | --- | --- | --- | ---: |
| [Cloud Run](https://cloud.google.com/run/pricing) | Launch | Backend API and web workloads. | Per month: 180,000 vCPU-seconds, 360,000 GiB-seconds, and 2 million requests for request-based billing. Free-tier value is calculated from `us-central1` prices. | **$0.00/month** at low traffic with scale-to-zero and no minimum instances. Actual regional use is monitored. |
| [Cloud Run Jobs](https://cloud.google.com/run/pricing) | Launch | Migrations, maintenance, and finite batch workloads. | Per month: 240,000 vCPU-seconds and 450,000 GiB-seconds. | **$0.00/month** initially. Jobs run only on demand or on a defined schedule. |
| [Cloud SQL for PostgreSQL](https://cloud.google.com/sql/pricing) | Launch | Production relational data. | No permanent production free tier. The separate 30-day trial instance has no backups and is not a production strategy. | One shared-core `db-f1-micro`: about **$11.53/month**; 10 GiB SSD: about **$2.55/month**. Total: **$14.08/month**. |
| [Cloud Storage](https://cloud.google.com/storage/pricing) | Launch | Application files and low-volume infrastructure state. | The 5 GiB Always Free allowance applies only to selected US regions, not São Paulo. | 5 GiB Standard storage at about $0.035/GiB-month: **$0.18/month**, excluding operations and transfer. |
| [Artifact Registry](https://cloud.google.com/artifact-registry/pricing) | Launch | Private production container images. | First 0.5 GiB-month per billing account. | 2 GiB retained: **$0.15/month** after the allowance. Image cleanup policies are mandatory. |
| [Secret Manager](https://cloud.google.com/secret-manager/pricing) | Development / Launch | Development bootstrap secrets and production runtime secrets. | Six active secret versions and 10,000 access operations/month per billing account. | Eight Development versions: up to **$0.12/month**. Sixteen total versions: about **$0.60/month**. Destroy superseded versions after the recovery period. |
| [Cloud Tasks](https://cloud.google.com/tasks/pricing) | Launch | Durable asynchronous commands and retries. | First 1 million operations/month. | **$0.00/month** initially; then $0.40 per million operations. |
| [Cloud Scheduler](https://cloud.google.com/scheduler/pricing) | Launch | Trigger scheduled maintenance and business jobs. | Three jobs/month per billing account. | **$0.00/month** with at most three consolidated jobs; then $0.10/job-month. |
| [Cloud Logging, Monitoring, and Error Reporting](https://cloud.google.com/products/observability/pricing) | Launch | Logs, metrics, dashboards, uptime checks, alerts, and exception diagnosis. | Logging: 50 GiB/project-month with 30-day retention. Google Cloud metrics are non-chargeable. Custom metrics include 150 MiB/billing-account-month. Uptime checks include 1 million executions/project-month. Error Reporting has no separate charge; its underlying log ingestion counts. | **$0.00/month** initially with log exclusions and retention controls. |
| [Cloud Trace](https://cloud.google.com/products/observability/pricing) | Launch | Distributed tracing and performance diagnosis. | 2.5 million spans/month. | **$0.00/month** initially. |
| IAM and Workload Identity Federation | Development / Launch | Least-privilege access and keyless GitHub deployment. | No separate usage charge. | **$0.00/month**. Production secrets are not granted to developers or CI. CI receives deployment permissions only. |

The Cloud SQL baseline uses 730 hours/month. Shared-core instances have no SLA and are acceptable only for the initial low-volume stage. Promotional Google Cloud credits are excluded because they are temporary.

Cloud Storage operations are usage-based: Standard storage Class A operations begin at approximately $0.005/1,000 and Class B at $0.0004/1,000. Internet transfer and Cloud Run transfer are variable and excluded from the fixed total.

## First Customer Protections

| Service | Activation rule | Expected cost |
| --- | --- | ---: |
| Cloud SQL automated backups and point-in-time recovery | Must be enabled and recovery-tested before the first customer is onboarded. | Reserve **$2.55/month** for an initial 10 GiB backup footprint. Actual incremental backup storage may differ. Point-in-time recovery logs are included for up to seven days on Enterprise edition. |
| Production monitoring alerts | Must cover availability, errors, latency, database capacity, and billing before onboarding. | Expected within the initial free allowances. |
| Stripe live payments | Enabled only after products, prices, webhooks, access transitions, and failure paths are validated in Test Mode. | Variable: conservatively **4.69% + R$0.50** per domestic subscription charge. |

Backups remain disabled before the first active customer, but their activation is a hard prerequisite for storing customer data.

## Growth Services

| Service | Activation criteria | Official price reference |
| --- | --- | --- |
| Cloud SQL dedicated machine and high availability | Enable when measured capacity, uptime objectives, or customer commitments exceed the shared-core limits. High availability approximately doubles compute and storage resources. | [Cloud SQL pricing](https://cloud.google.com/sql/pricing) |
| [Artifact Analysis](https://cloud.google.com/artifact-analysis/pricing) | Enable managed container scanning when deployment volume and security requirements justify it. | **$0.26 per newly scanned container image**. |
| [External Application Load Balancer](https://cloud.google.com/load-balancing/pricing) | Add only when a custom edge architecture, Cloud Armor, or routing requirement cannot be met directly by Cloud Run. | First five forwarding rules: **$0.025/hour each**, about $18.25/month for one, plus data processing. |
| [Cloud Armor Standard](https://cloud.google.com/armor/pricing) | Add with the load balancer when measured threats or contractual controls require managed edge protection. | Policy: about **$5/month**; each rule: about **$1/month**; global requests: **$0.75/million**. Load balancer charges also apply. |
| [Cloud DNS](https://cloud.google.com/dns/pricing) | Use only if registrar-provided DNS becomes insufficient. | No free tier. One managed zone: **$0.20/month**; queries begin at $0.40/million. |
| Paid email plan | Upgrade when the free daily limit, branding, deliverability, or support becomes insufficient. | From **$9/month**, according to the selected Brevo plan. |
| GitHub Team | Upgrade when team administration or repository controls require it. | **$4/user/month**. |
| Additional Cloud Run, Storage, observability, Tasks, and Scheduler usage | Pay only after their respective free allowances are exceeded. | Usage-based according to the tables above. |

Growth services are not part of the baseline and must not be enabled merely as future-proofing. Their need must be supported by operational evidence or a customer requirement.

## Free Development Dependencies

The following external registries are required only to restore public development dependencies and have no planned product subscription cost:

- NuGet.org for .NET packages.
- npm registry for web packages.
- crates.io for Rust packages.
- Terraform Registry for infrastructure providers and modules.

Package versions and integrity metadata must remain locked in the repositories. A private package registry is not required initially.

## Explicitly Excluded Services

- Docker Desktop and Docker Hub: Development runs natively without Docker; production images use Artifact Registry.
- Cloud Build: GitHub Actions performs backend and web CI/CD.
- Xcode Cloud and hosted macOS CI: mobile builds and publication are performed manually on the owner's Mac.
- Firebase Authentication, Auth0, and managed identity providers: ASP.NET Core Identity owns user identities.
- SMS and Twilio: authentication and notifications use email only.
- Google Maps and other location APIs: not required by the current use cases.
- Managed container scanning, Cloud Armor, external load balancing, Cloud DNS, and Cloud SQL high availability: growth options only.
- Push notification providers: current notifications are in-app and email.

## Ownership and Access

- The owner controls billing accounts, Apple and Google distribution accounts, production IAM, recovery methods, and break-glass access.
- Developers receive only Development access and cannot read production secrets or production customer data.
- GitHub Actions deploys through Workload Identity Federation and receives deployment permissions, not production secret-reading permissions.
- Production workloads read their own secrets through dedicated runtime identities.
- Development bootstrap retrieves only Development secrets and stores them in the local .NET user-secrets store. It runs once and is repeated only when secrets change.
- Store certificates, signing credentials, recovery codes, and account ownership material remain under owner-controlled secure storage.

## Cost Controls

- Configure GitHub Actions to stop paid usage when its budget is exhausted.
- Create Google Cloud budget alerts before production. Budgets alert but do not automatically cap usage.
- Set Cloud Run maximum instances and keep minimum instances at zero initially.
- Apply Artifact Registry cleanup policies and retain only deployable or rollback-relevant images.
- Destroy obsolete Secret Manager versions after the recovery period.
- Keep Cloud Scheduler within three consolidated jobs while practical.
- Apply Cloud Storage lifecycle rules and monitor outbound transfer.
- Exclude noisy, low-value logs and retain default logs for 30 days unless compliance requires more.
- Keep Stripe's optional custom domain disabled initially.
- Enable Cloud SQL backups only at the First Customer gate, then continuously verify them.
- Review this document quarterly and before enabling any paid service.

## Estimate Basis

The fixed estimates use:

- 730 service hours/month.
- One Cloud SQL `db-f1-micro` instance.
- 10 GiB of Cloud SQL SSD storage.
- 5 GiB of application and infrastructure files in Cloud Storage.
- 2 GiB of retained container images.
- Eight active Development secret versions and sixteen total Development plus Production versions.
- Cloud Run, Tasks, Scheduler, email, observability, and GitHub usage within their free allowances.
- One ordinary non-premium domain costing $10–$20/year.

Any change to these assumptions requires recalculating the totals.
