# Heuristic walkthrough

Run by the author alone, with no participant. It covers the flows the task scripts do not reach, per
§4.5.5. It is an inspection, not a test. It finds problems the designer can see, which are not the
same as the problems a member ran into.

Nielsen's ten heuristics, one pass per flow.

1. Visibility of system status
2. Match between the system and the real world
3. User control and freedom
4. Consistency and standards
5. Error prevention
6. Recognition rather than recall
7. Flexibility and efficiency of use
8. Aesthetic and minimalist design
9. Help users recognise, diagnose and recover from errors
10. Help and documentation

## Flows to walk

The task scripts already cover registration, profile, payment declaration, event registration,
directory, constitution, member approval, notices and fee configuration. This walkthrough takes what
is left:

- Password reset and the one-time code path when the code does not arrive
- Identity card generation and download
- Job board: posting, approval, and applying
- Gallery: album creation, photo upload, approval
- Polls and the forum
- The assistant
- Contact messages, from the public form to the administrator's reply
- Site content editing
- Organisation configuration and theming
- Audit log
- Role and permission administration
- Member import from a spreadsheet
- The mobile application's equivalent of each flow above that it implements

## Recording a finding

```
Flow:
Heuristic violated:
What is wrong:
Where (route or screen):
Severity:  cosmetic / minor / major / blocks the task
Who would hit it:
```

Judge severity against a member, not against yourself. You know where everything is, so this method
finds less than the sessions do. Report it as the weaker evidence it is.

Findings that overlap with something a participant hit are worth marking, because a problem found
both by inspection and in a session is better evidenced than either alone.
