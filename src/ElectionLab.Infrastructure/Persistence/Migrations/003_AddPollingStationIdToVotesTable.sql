-- Existing votes must be backfilled with a valid station before enforcing NOT NULL.
ALTER TABLE votes
    ADD COLUMN "PollingStationId" uuid NOT NULL;

-- Keeps the vote's polling station in the same election.
ALTER TABLE polling_stations
    ADD CONSTRAINT "AK_polling_stations_ElectionId_Id" UNIQUE ("ElectionId", "Id");

ALTER TABLE votes
    ADD CONSTRAINT "FK_votes_polling_stations_ElectionId_PollingStationId"
        FOREIGN KEY ("ElectionId", "PollingStationId")
        REFERENCES polling_stations ("ElectionId", "Id") ON DELETE RESTRICT;

CREATE INDEX "IX_votes_ElectionId_PollingStationId"
    ON votes ("ElectionId", "PollingStationId");
