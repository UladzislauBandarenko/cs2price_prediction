# Database Design & Implementation

## 1. Overview and Logical Layers
The database is structured into several conceptual layers, reflecting different aspects of CS2 skin characteristics:
Reference Dictionaries
weapon_types – types of weapons.
weapons – individual weapon models.
wear_tiers – skin wear conditions.
doppler_phases – Doppler phases.
stickers – sticker names.
Skins and Their Properties
skins – the core skin entity.
skin_wear_tiers – mapping of which wear tiers are available for each skin.
Pattern tables:
case_hardened_gun_patterns
case_hardened_knife_patterns
fade_gun_patterns
fade_knife_patterns
doppler_skin_phases
Prices and Market Data
sticker_prices – observed prices for stickers.
## 2. Data Dictionary
Below is a formal description of all tables, columns, data types, keys, and quality expectations.
## 2.1 weapon_types
Purpose: Stores weapon type categories.
Data quality expectations:
code must be lowercase and from a predefined list.

## 2.2 weapons
Purpose: Stores weapon models.
Data quality:
Referenced weapon type must exist.

## 2.3 wear_tiers
Purpose: Stores skin wear conditions.

## 2.4 skins
Purpose: Stores skins for weapons.


Indexes:
(weapon_id, name) UNIQUE — skin names are unique per weapon.
Data quality:
pattern_style must match exactly one allowed value.


## 2.5 skin_wear_tiers
Purpose: Defines which wear tiers are available for each skin

Indexes:
(skin_id, wear_tier_id) UNIQUE — no duplicates.
Data quality:
All combinations must reflect real in-game availability.

## 2.6 case_hardened_gun_patterns
Purpose: Stores pattern data for Case Hardened gun skins.
Indexes:
(skin_id, pattern) UNIQUE
Data quality:
Percentage values expected to be within valid ranges (0–100).

## 2.7 case_hardened_knife_patterns
Purpose: Pattern data for Case Hardened knife skins.

Indexes:
(skin_id, pattern) UNIQUE
Quality:
Color segments should not exceed 100% combined.
## 2.8 fade_gun_patterns
Indexes:
(skin_id, pattern) UNIQUE

## 2.9 fade_knife_patterns
Indexes:
(skin_id, pattern) UNIQUE
## 2.10 doppler_phases
Purpose: Doppler phases dictionary.

## 2.11 doppler_skin_phases
Purpose: Links doppler skins to supported phases
Indexes:
(skin_id, phase_id) UNIQUE
## 2.12 stickers
Purpose: Sticker dictionary.

## 2.13 sticker_prices
Purpose: Observed sticker prices.

Data quality:
price > 0 required.



3. Data Integrity
3.1 Entity Integrity
All tables use an auto-increment primary key.
Unique constraints prevent duplicate reference or pattern records.
3.2 Referential Integrity
Maintained by foreign keys:
Weapons must reference an existing weapon type.
Skins must reference a valid weapon.
Pattern tables must reference valid skins.
Sticker prices must reference an existing sticker.
Recommended delete behavior:
RESTRICT for most dictionary tables.
CASCADE only if business logic explicitly allows removing full hierarchies.
3.3 Business Rules
Not all rules are enforced by the DB engine; some depend on application-level validation:
pattern_style must determine exactly one valid pattern table for the skin.
Doppler skins must not have entries in non-Doppler pattern tables.
Pattern numbers must fall within valid ranges.
Percentage values must be realistic.

4. Transactions
The system relies on standard ACID-compliant database transactions.
Typical transaction example (creating a weapon + skin):
BEGIN;

INSERT INTO weapons (name, weapon_type_id)
VALUES ('AK-47', 1)
RETURNING id;

INSERT INTO skins (weapon_id, name, pattern_style)
VALUES (/* returned id */, 'Case Hardened', 'ch_gun')
RETURNING id;

INSERT INTO skin_wear_tiers (skin_id, wear_tier_id)
VALUES (/* skin id */, 1), (/* skin id */, 2);

COMMIT;

solation level
Recommended:
READ COMMITTED for operational work.
REPEATABLE READ for analytic workloads requiring consistent snapshots.

5. Logical Schema (Conceptual Overview)
The logical structure can be described in interconnected blocks:
## 1. Weapon Layer
weapon_types (1) —(N)→ weapons
## 2. Skin Layer
weapons (1) —(N)→ skins
skins (M) —(N)→ wear_tiers via skin_wear_tiers
3. Pattern Layer
Each skin has only one valid pattern table, depending on pattern_style:
Case Hardened (guns): → case_hardened_gun_patterns
Case Hardened (knives): → case_hardened_knife_patterns
Fade guns: → fade_gun_patterns
Fade knives: → fade_knife_patterns
Doppler knives: → doppler_skin_phases
4. Doppler Layer
doppler_phases (1) —(N)→ doppler_skin_phases
5. Sticker & Pricing Layer
stickers (1) —(N)→ sticker_prices
A visual ER diagram can easily highlight these blocks and relationships.

6. ER
![ER](docs/assets/images/BD/er.jpg)

Database Design and Deployment
## 1. Database Design
## 1.1 Choice of DBMS
The project uses a modern relational database management system — PostgreSQL — which provides strong ACID guarantees, advanced indexing mechanisms, support for complex queries, JSON extensions, and robust tooling for development and deployment. PostgreSQL is widely adopted in production environments and is well suited for analytical workloads, making it an appropriate choice for storing structured skin and pattern data.
## 1.2 Schema Design and Normalization
The database schema was designed following practices of relational modeling and normalized up to the Third Normal Form (3NF):
1NF: All tables contain atomic values, no repeating groups, and consistent row structures.
2NF: No partial dependencies exist in any table — all non-key attributes depend on the full primary key.
3NF: No transitive dependencies — all non-key attributes describe only the entity represented by the table.
As a result, the schema avoids data duplication, update anomalies, and ensures consistent referential and structural integrity.
## 1.3 Schema Composition
The final schema includes a diverse set of entities grouped into several logical domains: weapon dictionaries, skins, wear tiers, specialized pattern tables, Doppler phases, stickers, and pricing information. The design incorporates one-to-many, many-to-many, and reference mappings, demonstrating versatility of relational modeling.
Primary Keys
All tables use an auto-incrementing integer primary key (id), ensuring row uniqueness and simplifying indexing.
Foreign Keys
Foreign key relationships enforce referential integrity, for example:
weapons.weapon_type_id → weapon_types.id
skins.weapon_id → weapons.id
Pattern tables reference skins.id
sticker_prices.sticker_id → stickers.id
Deletion behavior is defined to prevent orphaned records (typically ON DELETE RESTRICT or application-level soft deletion).
Constraints
To ensure data quality and enforce business rules, the schema includes:
NOT NULL constraints for all mandatory fields
UNIQUE constraints for dictionary values and composite keys
CHECK constraints (where needed) for:
percentage ranges
pattern number ranges
positive pricing values
Data Types
Appropriate, meaningful data types were selected:
varchar for names and codes
int for identifiers and pattern IDs
float for numeric properties such as color distribution or prices
If prices are extended to historical data, timestamp would be used for time-series entries
These choices ensure both correctness and performance efficiency.
## 2. Database Deployment
## 2.1 Automated Schema Creation
The database is deployed using SQL migration scripts, rather than manual execution. This ensures reproducibility, traceability, and consistency across environments (local development, testing, production).
Migrations include:
Creation of all tables
Definition of primary and foreign keys
Indexes and constraints
Optional seed data for reference tables
## 2.2 Version Control
All SQL scripts and migration files are stored in a Git repository, which enables:
Full version history of schema changes
Team collaboration
Safe rollback and forward migration
Automated deployment in CI/CD pipelines if needed
This approach aligns with modern software engineering practices.
3. Test Data
3.1 Purpose and Coverage
A comprehensive set of test records is included to validate:
Correct functioning of the application
Accuracy of joins and relationships
Behavior under a variety of inputs
Processing of edge cases
extreme pattern values
rare Doppler phases
skins with multiple or minimal wear tiers
price values at realistic boundaries
3.2 Reference Data Scripts
The project provides dedicated SQL scripts to populate fixed reference tables, including:
weapon_types
wear_tiers
doppler_phases
stickers (optional minimal set)
These values are static and required for the domain to function correctly, so they are included as part of seed data.
3.3 Demonstration Data
Additional sample records are included for:
Weapons and skins
Pattern definitions across all pattern tables
Sticker prices
Wear tier mappings
Together, these datasets ensure the database is fully operational immediately after deployment, and allow the application, API endpoints, and analytical workflows to be meaningfully demonstrated.

Security, Users and Access Control
## 1. Data Sensitivity
The database stores only technical and market data related to CS2 weapon skins (weapons, skins, patterns, wear tiers, sticker prices, etc.).
No personally identifiable information (PII), financial information, or other confidential user data is stored. This significantly reduces regulatory and security risks and simplifies the access model.
All Create / Update / Delete operations on domain entities are exposed only via an administrative interface, while the public API is effectively read-only.
## 2. Role-Based Access Control in PostgreSQL
Access control is implemented using role-based security in PostgreSQL. A dedicated initialization script (init.sql) creates logical roles, users, and grants based on the principle of least privilege.
## 2.1 High-Level Application Roles
Three high-level group roles are defined:
app_read – read-only access;
app_write – read + write (inherits app_read);
app_admin – administrative role (inherits app_write).

CREATE ROLE app_read NOLOGIN;
CREATE ROLE app_write NOLOGIN;
CREATE ROLE app_admin NOLOGIN;

GRANT app_read TO app_write;
GRANT app_write TO app_admin;

These roles are NOLOGIN roles and serve as permission bundles which can be granted to real users.
All three roles are granted basic permissions on the application database:
GRANT CONNECT, TEMP ON DATABASE cs2db
TO app_read, app_write, app_admin;

## 2.2 Application Users
Two real PostgreSQL users are created:
cs2_user – read-only user, used by the application/API/ML services;
cs2_admin – administrative user, used for migrations, schema changes and seeding.
CREATE USER cs2_user  ...;
GRANT app_read TO cs2_user;

CREATE USER cs2_admin ...;
GRANT app_admin TO cs2_admin;

The application never runs under a PostgreSQL superuser, which is an explicit design decision to minimize the damage that can be caused by a compromised application account.
3. Schema Design for Security
All application tables are located in a dedicated schema cs2 owned by cs2_admin:
CREATE SCHEMA IF NOT EXISTS cs2 AUTHORIZATION cs2_admin;
Permissions on the schema are granted as follows:
app_read and app_write get USAGE on schema cs2 (they can access objects, but not create them);
app_admin gets USAGE, CREATE on schema cs2 (can create and manage objects);
direct permissions are also granted to cs2_user and cs2_admin for clarity.
GRANT USAGE ON SCHEMA cs2 TO app_read, app_write;
GRANT USAGE, CREATE ON SCHEMA cs2 TO app_admin;

GRANT USAGE ON SCHEMA cs2 TO cs2_user;
GRANT USAGE, CREATE ON SCHEMA cs2 TO cs2_admin;

The default public schema is restricted to avoid uncontrolled object creation by arbitrary roles:
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
Only administrative roles and cs2_admin are allowed to create objects in public (for example, the __EFMigrationsHistory table used by Entity Framework).
4. Default Privileges for New Objects
To ensure that new tables automatically receive correct permissions, ALTER DEFAULT PRIVILEGES is used for the owner role cs2_admin within schema cs2:
ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA cs2
  GRANT SELECT ON TABLES TO app_read;

ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA cs2
  GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES, TRIGGER
  ON TABLES TO app_admin;

ALTER DEFAULT PRIVILEGES FOR ROLE cs2_admin IN SCHEMA cs2
  GRANT USAGE, SELECT ON SEQUENCES TO app_admin;
This guarantees that:
all newly created tables are readable by app_read (and therefore by cs2_user);
full DML and reference rights are granted to app_admin (and therefore to cs2_admin);
permissions do not have to be manually updated every time a new table is added, but can still be reviewed and adjusted if business requirements change.
5. Application-Level Permissions
On the application side:
Endpoints for Create / Update / Delete operations are exposed only through the administrative interface, bound to users with elevated privileges.
Regular API consumers work with read-only operations, typically mapped to the cs2_user database account (via a connection string with limited credentials).
This enforces a clear separation between read-only consumers (analytics, ML, dashboards) and administrative users (who can change schema or seed data).
6. Password Handling and Encryption
Although the database in this project does not store end-user credentials or sensitive personal data, the general security model assumes that:
Application-level passwords (for admin accounts or future user accounts) are stored only in hashed form using modern algorithms (e.g. bcrypt / Argon2), not in plain text.
Legacy or weak hashing algorithms such as MD5 or SHA-1 are not used for password storage.
Database connection passwords (cs2_user, cs2_admin) are managed via the application’s configuration system and not hardcoded into the source code.
This approach follows current best practices for authentication and credential management.
7. Data Integrity and Transactions
Data integrity is ensured at two levels:
Database-level constraints
Primary keys, foreign keys, NOT NULL, UNIQUE and appropriate data types.
Optional CHECK constraints for value ranges (e.g. percentages, positive prices).
Transactional consistency
All write operations (especially those touching multiple related tables) are executed inside ACID transactions.
In case of any constraint violation or error, the transaction is rolled back and the database is never left in a partially updated state.
Together with the role-based access model, this provides a robust foundation for consistent, secure, and controlled data manipulation.

Constraints and Prohibited Practices
The database design follows industry-standard security and data-management principles. Several restrictions were intentionally introduced to ensure data integrity, maintainability, and compliance with best practices in relational database engineering.
## 1. Password Storage
In this project, no user-facing authentication data is stored inside the database, which further reduces the risk of credential exposure.
## 2. Prohibition on Using CSV/Excel Files as the Primary Data Store
CSV, Excel or similar flat files cannot be used as the main storage layer for application data.
They lack:
concurrency control,
transactional guarantees,
constraints,
indexing,
and reliable update semantics.
Such files are acceptable only for importing or exporting data, never as a substitute for a relational database.
In this project, PostgreSQL serves as the authoritative source of truth, with schema-based validation and ACID guarantees.

3. No Unstructured JSON Storage Without a Defined Model
It is prohibited to store large, unstructured JSON blobs in a relational database without a clear and documented model.
Doing so leads to:
loss of queryability,
poor performance,
inability to enforce constraints,
and difficult long-term maintenance.
JSON fields may be used only when:
Their internal structure is documented;
Expected keys, formats, and constraints are described;
The choice is justified in the architectural documentation.
In this project, all entities are modeled with explicit tables and columns — no unstructured JSON storage is used.

4. Data Integrity Must Not Be Ignored
Every table has a primary key.
All relationships are implemented through foreign keys.
Business rules are implemented via constraints and schema normalization up to 3NF.
Administrative operations are executed inside ACID transactions.

Schema Versioning and Change Management
The database schema is versioned using a migration-based approach rather than manual SQL changes. This provides a clear, reproducible history of structural changes and aligns with modern DevOps practices.
The application uses database migrations (e.g., Entity Framework Core migrations) to:
create and modify tables, constraints, and indexes;
safely evolve the schema as the project grows.
Each migration:
is stored as a dedicated file in the source code repository (Git);
has a unique identifier and a descriptive name (e.g., AddSkinsTable, AddPatternIndexes);
contains forward (upgrade) and backward (downgrade) steps.
The current schema version is tracked directly in the database in a special system table (for example, __EFMigrationsHistory in PostgreSQL’s public schema).
This table stores:
which migrations have already been applied,
in which order,
and allows the system to:
bring a new database instance up to the latest version,
or roll back to a previous version if necessary.
Only the administrative database user (cs2_admin), which has the app_admin role, is allowed to run migrations. This ensures:
ordinary application users cannot change the schema;
all structural changes are controlled, intentional, and auditable.
Because all migration files are tracked in Git, the schema has full version history together with the application code. Any change to the data model can be traced back to a specific commit, author, and rationale.

Indexing Strategy and Justification (Short Version)
The database uses a set of primary, unique, and composite indexes designed to support data integrity and typical query patterns of the application.
Primary Keys
Each table includes an auto-increment primary key (id), which creates a B-tree index used for fast lookups and efficient joins. This ensures entity integrity and optimal performance for the most common access path: WHERE id = ….
Unique Indexes on Reference Tables
Tables such as weapon_types, weapons, wear_tiers, doppler_phases, and stickers include UNIQUE constraints on names or codes.
These indexes:
prevent duplicate dictionary entries,
accelerate lookups by name/code during validation and API operations.
Composite Index on skins
A unique index on (weapon_id, name) enforces that each skin name is unique within a weapon.
It also speeds up queries that search for a skin by weapon and name—one of the most common usage patterns.
Composite Indexes in Relationship Tables
Tables such as skin_wear_tiers and doppler_skin_phases use (skin_id, wear_tier_id) or (skin_id, phase_id) as unique composite indexes.
These:
prevent duplicate relationships,
support efficient retrieval of wear tiers or phases for a skin.
Composite Indexes in Pattern Tables
Pattern tables (Case Hardened, Fade, etc.) use (skin_id, pattern) as a unique index.
This ensures:
each pattern number exists only once per skin,
fast access to all patterns of a skin or a specific (skin_id, pattern) pair.


