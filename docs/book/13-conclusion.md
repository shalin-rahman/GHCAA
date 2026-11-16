# Chapter 13 — Conclusion and Future Work

**What this chapter owns.** What was done, what was learned, what it is worth and what should
happen next.

**What it must not repeat.** No new evidence appears here. Anything that would need to be defended
belongs in the chapter that carries its evidence, and is cited from here rather than restated.

## 13.1 Summary of the Work and its Contributions

The research questions are answered in §12.6, Answering the Research Questions, and the answers
are not repeated here.

This project set out to give one alumni association a platform that follows its own constitution.
The requirements were taken from the governing documents and from the office holders. The design
keeps the rules in one place, behind an API that a web client and a mobile client share. The system
was built, tested at four levels and deployed to a shared test environment.

The work offers three things. The first is the system itself, which the Association can use. The
second is the five-class scheme of §3.7, domain constraints, which separates the rules software can
enforce from the rules that must stay with a person. The third is a plain record of what a single
maintainer could and could not do, including the three evaluation commitments that §4.5, the
evaluation strategy, records as reduced or not run.

## 13.2 Practical Implications for Similar Institutions

An association thinking of doing the same should start with its constitution, not with software.
Sorting the rules into what a program can decide and what an office must decide costs little and
shows early where the hard parts are.

It should also count the person before the hosting. The monthly bill is the small part of the cost.
The large part is someone who can maintain the code, and §12.9, threats to validity, explains why
this project's cost does not transfer to an institution without such a person. An association
without one would be better served by a general product and a paper election.

## 13.3 Lessons Learned

On the technical side, the choices that paid off were the dull ones: one deployment, one database
and rules kept in the service layer. The choices that cost time were those that tied the system to
a single instance, as §10.1, deployment architecture, environments and containers, describes.

On method, writing the rules catalogue before the code found faults that tests would not have
found, because the tests would have been written from the same misunderstanding. Reviews found a
different kind of fault from tests, and I needed both. I also promised more evaluation than one
person could deliver, and it was better to say so in §4.5, the evaluation strategy, than to report
less and hope nobody compared.

On the organisational side, office holders change every term. A system that depends on what one
office holder remembers will fail at the next election. The handbook and the task scripts matter as
much as the screens.

## 13.4 Future Work

The work that remains falls into three groups.

### 13.4.1 Near-term functional roadmap

The open items are kept in the project tracker with their priorities. The nearest ones finish
workflows that already exist and do not add new ones.

*[List needed: the five or six open tracker items the Association ranks highest, taken from
`docs/TODO.md` on the day this section is finalised.]*

### 13.4.2 Technical debt and reengineering priorities

Four debts stand out, in the order I would pay them. A database backup with a tested restore comes
first, because without it one failure loses the Association's records. Alerting comes second.
Automated security scanning comes third: it was left out on cost, as §10.2.2, security in the
pipeline, explains. Moving the caches out of the process comes last, because a single instance is
enough for an association of this size.

*[Costs needed: the price of each remedy per month or per year, from the provider's price list with
the date.]*

### 13.4.3 Research directions opened by this work

Two questions follow from this work. The first is whether the five-class scheme holds for another
institution's constitution, which needs a second case. The second is what the missing evaluations
would show: a real load test, an independent security audit and a usability study with a control
condition would each test a claim this dissertation could only state.

## 13.5 Concluding Remarks

A constitution is a set of promises about who may decide what. Software can keep some of those
promises better than paper can, and others it cannot keep at all. Most of the work in this project
was in finding out which was which.

## Figures and Tables

*[Not drawn. The outline specifies 2 artefacts for this chapter. Each is added here with its caption as it is made, then `renumber.py --apply` is run.]*
