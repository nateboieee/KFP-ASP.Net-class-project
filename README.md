# 🍗 KFP — Kentucky Fried Pork

A parody online food-ordering website built with **ASP.NET Core MVC (.NET 8)**, styled after
a well-known fried chicken chain but re-branded as a fictional fried **pork** brand.
Built as a demo / portfolio project — not affiliated with any real restaurant chain.

## Features

- **Browsable menu** with categories (Buckets, Combos, Sandwiches, Sides, Drinks, Desserts),
  search, and item detail pages.
- **Shopping cart** ("Bucket") stored in session — add, update quantity, remove, clear.
- **User authentication** via ASP.NET Core Identity — register, log in, log out, manage account
  (email/password), all backed by the SQL database. Also supports optional **Google /
  Facebook / Twitter login** (see "External logins" below) once you add your own API keys.
- **Checkout & orders** — logged-in users can check out, and view their order history and
  order details. Checkout requires login.
- **Real photo uploads** — Admins can upload an actual photo (JPG/PNG/GIF/WEBP, up to 5MB)
  for each menu item from the Admin panel. Photos are stored in `wwwroot/images/menu/` and
  referenced from the database by path. Items without a photo automatically fall back to
  their emoji icon everywhere (menu grid, details page, cart, home page).
- **Admin area** (role-protected) — an Admin user can add/edit/delete menu items (including
  uploading/removing photos) and update order statuses (Placed → Preparing → Out for
  Delivery → Delivered).
- **SQL database** — uses SQLite (a real relational/SQL database, stored as a single `.db`
  file) via Entity Framework Core. Easy to swap for SQL Server if you prefer (see below).
- **KFC-inspired styling** — red/black/white theme, hero banner, pill-shaped category filters,
  card-based menu grid, etc. (`wwwroot/css/site.css`).

## Project structure

```
Controllers/        MVC controllers (Home, Menu, Cart, Orders, Admin)
Models/              Entity classes (MenuItem, MenuCategory, Order, OrderItem, ApplicationUser)
Models/ViewModels/   View models (Checkout, Menu listing)
Data/                EF Core DbContext + DbInitializer (seed data & admin account)
Services/            CartService (session-based shopping cart)
Views/                Razor views, including the shared KFC-style layout
Areas/Identity/       ASP.NET Core Identity UI hook-up (login/register pages come from
                       the Microsoft.AspNetCore.Identity.UI package)
wwwroot/css/site.css  All custom KFP theming
```

## Getting started

**Requirements:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and internet
access (to restore NuGet packages the first time).

```bash
cd KFP
dotnet restore
dotnet run
```

Then open the URL shown in the console (e.g. `https://localhost:7271`).

On first run, the app automatically:
- Creates the SQLite database file (`app.db`) in the project folder.
- Seeds menu categories and menu items.
- Creates an **Admin** role/account and a **Customer** role.

### Default admin login

```
Email:    admin@kfp.local
Password: Admin@123
```

Log in with this account and visit **Admin** in the nav bar to manage the menu and orders.
⚠️ This is a demo credential — change the password (or the seed logic in
`Data/DbInitializer.cs`) before using this anywhere real.

Any other visitor can just click **Register** to create a normal customer account.

## External logins (Google / Facebook / Twitter)

The app supports "Log in with Google/Facebook/Twitter" in addition to email+password. These
show up automatically on the Login page once you supply real credentials - with no keys
configured, the app just runs normally with email/password only.

**Never put real secrets in `appsettings.json`** (it gets committed to source control). Use
one of these instead:

- **Local development — .NET User Secrets** (recommended):
  ```bash
  cd KFP
  dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
  dotnet user-secrets set "Authentication:Google:ClientSecret" "your-client-secret"
  dotnet user-secrets set "Authentication:Facebook:AppId" "your-app-id"
  dotnet user-secrets set "Authentication:Facebook:AppSecret" "your-app-secret"
  dotnet user-secrets set "Authentication:Twitter:ConsumerKey" "your-consumer-key"
  dotnet user-secrets set "Authentication:Twitter:ConsumerSecret" "your-consumer-secret"
  ```
- **Production** — environment variables (e.g. `Authentication__Google__ClientId`) or your
  host's secret manager. The `__` (double underscore) maps to the `:` in config keys.

### Getting credentials for each provider

For all three, the app runs at `https://localhost:7271` by default in development — swap in
your real domain for production.

- **Google**
  1. [Google Cloud Console](https://console.cloud.google.com/) → APIs & Services →
     Credentials → *Create Credentials* → *OAuth client ID* → *Web application*.
  2. Authorized redirect URI: `https://localhost:7271/signin-google`
  3. Copy the **Client ID** and **Client secret**.
- **Facebook**
  1. [Facebook for Developers](https://developers.facebook.com/) → create an app → add the
     *Facebook Login* product.
  2. Valid OAuth Redirect URI: `https://localhost:7271/signin-facebook`
  3. Copy the **App ID** and **App Secret** (Settings → Basic).
- **Twitter / X**
  1. [X Developer Portal](https://developer.twitter.com/) → create a project/app with
     **user authentication** (OAuth 1.0a) enabled.
  2. Callback URL: `https://localhost:7271/signin-twitter`
  3. Copy the **API Key** (Consumer Key) and **API Key Secret** (Consumer Secret).
  4. Note: X's free API tier is limited, and OAuth 1.0a sign-in support has been reduced
     over time on X's side - if `AddTwitter` gives you trouble, this is the most likely
     reason, and X's own docs are the source of truth on current availability.

Each provider is only registered in `Program.cs` if both its keys are non-empty, so partially
configuring one provider (e.g. just Google) is fine.

## Database notes

- The app uses **SQLite** (`Microsoft.EntityFrameworkCore.Sqlite`) with the connection string
  in `appsettings.json` (`DataSource=app.db`). SQLite is a full SQL database engine — all
  queries go through EF Core / LINQ-to-SQL under the hood.
- The schema is created automatically via `Database.EnsureCreatedAsync()` in
  `Data/DbInitializer.cs` — no manual migration step is required to get started.
- If you'd rather use **SQL Server** instead of SQLite:
  1. Swap the package in `KFP.csproj`: remove `Microsoft.EntityFrameworkCore.Sqlite`, add
     `Microsoft.EntityFrameworkCore.SqlServer`.
  2. In `Program.cs`, change `options.UseSqlite(connectionString)` to
     `options.UseSqlServer(connectionString)`.
  3. Update the `DefaultConnection` string in `appsettings.json` to a SQL Server connection
     string (e.g. LocalDB: `Server=(localdb)\\mssqllocaldb;Database=KFP;Trusted_Connection=True;`).
  4. If you want real EF Core migrations instead of `EnsureCreated`, run:
     `dotnet ef migrations add InitialCreate` then `dotnet ef database update`.

## A note on how this project was built

This project was generated and hand-assembled in a sandboxed environment that did not have
access to nuget.org, so the code could not be compiled/tested there. Everything was written
carefully by hand against the standard ASP.NET Core 8 / EF Core 8 / Identity APIs, but please
run `dotnet build` after restoring on your own machine and let me know if anything needs
fixing — happy to patch it up.

## Ideas for extending it

- Real payment integration (Stripe/PayPal) instead of the "payment method" dropdown.
- Product images instead of emoji icons.
- Email confirmation for new accounts (currently disabled for easier local testing).
- Order status emails/notifications.
- Pagination for large menus.
