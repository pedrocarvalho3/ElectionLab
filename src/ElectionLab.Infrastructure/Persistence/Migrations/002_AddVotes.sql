-- Supports a foreign key that keeps a vote's candidate in the same election.
ALTER TABLE candidates
    ADD CONSTRAINT "AK_candidates_ElectionId_Id" UNIQUE ("ElectionId", "Id");

CREATE TABLE votes (
    "Id" uuid NOT NULL,
    "ElectionId" uuid NOT NULL,
    "CandidateId" uuid NULL,
    "Type" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_votes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_votes_elections_ElectionId"
        FOREIGN KEY ("ElectionId") REFERENCES elections ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_votes_candidates_ElectionId_CandidateId"
        FOREIGN KEY ("ElectionId", "CandidateId")
        REFERENCES candidates ("ElectionId", "Id") ON DELETE RESTRICT,
    -- VoteType: Candidate = 0, Blank = 1, Null = 2.
    CONSTRAINT "CK_votes_Type_CandidateId" CHECK (
        ("Type" = 0 AND "CandidateId" IS NOT NULL)
        OR ("Type" IN (1, 2) AND "CandidateId" IS NULL)
    )
);

CREATE INDEX "IX_votes_ElectionId_CandidateId" ON votes ("ElectionId", "CandidateId");
