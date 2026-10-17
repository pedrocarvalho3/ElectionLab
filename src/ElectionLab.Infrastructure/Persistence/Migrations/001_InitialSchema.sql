-- Matches the schema previously created by the three EF migrations.
-- Existing tables and their data are retained when adopting this baseline.
CREATE TABLE IF NOT EXISTS elections (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "PK_elections" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS candidates (
    "Id" uuid NOT NULL,
    "ElectionId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Number" integer NOT NULL,
    CONSTRAINT "PK_candidates" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_candidates_elections_ElectionId"
        FOREIGN KEY ("ElectionId") REFERENCES elections ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_candidates_ElectionId_Number"
    ON candidates ("ElectionId", "Number");

CREATE TABLE IF NOT EXISTS polling_stations (
    "Id" uuid NOT NULL,
    "ElectionId" uuid NOT NULL,
    "Code" character varying(4) NOT NULL,
    "Status" integer NOT NULL,
    "OpenedAt" timestamp with time zone NULL,
    "ClosedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_polling_stations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_polling_stations_elections_ElectionId"
        FOREIGN KEY ("ElectionId") REFERENCES elections ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_polling_stations_ElectionId_Code"
    ON polling_stations ("ElectionId", "Code");
