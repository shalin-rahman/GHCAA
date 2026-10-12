CREATE TABLE users (
    id             SERIAL PRIMARY KEY,
    username       TEXT    NOT NULL UNIQUE,
    set_ballot_key BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE elections (
    id              SERIAL PRIMARY KEY,
    title           TEXT        NOT NULL,
    choices         JSONB       NOT NULL,
    public_key_spki TEXT        NOT NULL,
    fingerprint     CHAR(64)    NOT NULL,
    status          TEXT        NOT NULL DEFAULT 'open' CHECK (status IN ('open', 'tallied')),
    created_by      INTEGER     NOT NULL REFERENCES users (id),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    tallied_at      TIMESTAMPTZ,
    result          JSONB
);

-- A ballot row holds no user id, so a stored ballot cannot be tied to a voter.
CREATE TABLE ballots (
    id          SERIAL PRIMARY KEY,
    election_id INTEGER     NOT NULL REFERENCES elections (id),
    sealed      TEXT        NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Who has voted, kept apart from what was cast. The primary key allows one vote per voter.
CREATE TABLE voters_voted (
    election_id INTEGER NOT NULL REFERENCES elections (id),
    user_id     INTEGER NOT NULL REFERENCES users (id),
    PRIMARY KEY (election_id, user_id)
);

CREATE INDEX ballots_election_idx ON ballots (election_id);

INSERT INTO users (username, set_ballot_key) VALUES
    ('returning.officer', TRUE),
    ('voter.one', FALSE),
    ('voter.two', FALSE),
    ('voter.three', FALSE);
