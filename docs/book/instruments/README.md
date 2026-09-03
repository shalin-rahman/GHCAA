# Evaluation instruments

These are the forms and scripts used to collect the human-participant evidence reported in the
dissertation. They are delivered beside the book rather than bound into it, because §4.5.5 promises
that the criterion can be checked against what participants actually saw.

Nothing in this folder contains results. Responses go in `responses/`, which is empty until sessions
have run. A file here that carries an answer is a defect.

## What each file is for

| File | Used with | Feeds |
| --- | --- | --- |
| `consent-and-briefing.md` | Every participant, before anything else | §4.9 |
| `task-script-member.md` | Ordinary members | NFR-U2 completion criterion, §12.6 |
| `task-script-treasurer.md` | The Treasurer | §12.6, §12.7 |
| `task-script-secretary.md` | The General Secretary | §12.6, §12.7 |
| `task-script-president.md` | The President | §12.7 |
| `sus-questionnaire.md` | Every participant, after their tasks | NFR-U3, §12.6 |
| `role-questions.md` | Office holders, after the SUS | §12.7 |
| `observation-checklist.md` | The person running the session | §12.6 |
| `heuristic-walkthrough.md` | The author alone, no participant | §12.6 |
| `response-log-template.md` | One copy per participant | All of the above |

## How to run a session

Allow forty minutes. Twenty-five for the tasks, five for the questionnaire, ten for the role
questions and anything the participant wants to say.

Read the briefing aloud rather than paraphrasing it, so every participant hears the same framing.
The framing matters more than it looks: participants who think their own competence is being judged
report higher satisfaction than they felt, and this study cannot afford that bias with a sample this
small.

Hand over the device already signed in where the task says so, and signed out where it does not. The
registration task tells you nothing if the participant starts from a session someone else opened.

**Do not help.** The quickest way to ruin a session is to answer a question. If the participant asks
where something is, say you cannot help, and record the task as failed. That failure is the result you
came for. Note what they tried instead, because that is usually the fix.

Record time from when the participant starts reading the task to when they say they are finished, or
to when they give up. If they believe they finished but did not, that is a failure, and it is worth
more than a slow success.

Do not run a session on the author's own machine with the author's own account. Use the
pre-production deployment, on a handset for at least half the member sessions, since §3.5 assumes
mobile-first use and a study run entirely on a desktop would not test the assumption.

## Scoring

The SUS scoring rule is in `sus-questionnaire.md`. Score each response separately, report the mean
with the sample size beside it, and never report a percentage. With a sample this size, a percentage
claims a precision the study does not have.

Task completion is reported as a count out of the number attempted, per task, not as a single
overall rate. One task everybody failed is a design finding; the same failure averaged into an
overall rate disappears.

## What this study cannot support

Stated plainly here so that no chapter overstates it later. The sample is purposive and small, drawn
from members already engaged enough to agree. The author is the sole maintainer and is known to every
participant, so people will tend to be kind about it, and nothing here stops that. There is no control
condition and no comparison system, so nothing in this folder can support a claim that the platform
is better than an alternative, only that people could or could not complete the tasks.
