# Accessibility checklist for the election vote screens

**Feature Branch**: 023-election-ballot-secrecy-and-standards
**Tracker**: `docs/TODO.md` item 37.13g
**Standard**: WCAG 2.2 Level AA; `docs/Elections/00-election-standards-requirements.md` section 13
**Status**: Draft, not yet run

| Field | Entry |
|---|---|
| Tester | |
| Date | |
| Browser / Device | |
| Screen reader | |
| App version / commit | |

Screens covered: the member vote screen, the receipt and print step, the tracking-code check, and
the public results page. Web is `GHCAA.Web/src/app/member/election/` and
`GHCAA.Web/src/app/public/elections/`. Mobile is `GHCAA.Mobile/lib/screens/member/election_screen.dart`
and the results screen it opens.

## How to run it

- Run each table once on web (Chrome or Edge, with NVDA) and once on Android (TalkBack). Use a
  test election in the Polling phase with at least two seats, one of them choose-many.
- Keyboard only on web: Tab, Shift+Tab, arrow keys, Space, Enter and Escape. Do not touch the mouse.
- Mobile: an external keyboard where available, otherwise TalkBack swipe and double-tap.
- Also run once at 200 percent browser zoom and at a 320 px wide window (web), and at the largest
  system font size (mobile).
- Write Pass, Fail or N/A in Result. In Notes give the step, what was heard or seen, and the
  screen. A Fail needs a tracker item before this checklist is called done.
- The screens are being edited. Findings are recorded by screen and task, not by line.

## Member vote screen, web and mobile (seat choices, review and confirm)

| Criterion | Check with keyboard only and with a screen reader | Result | Notes |
|---|---|---|---|
| 1.1.1 Non-text content | Candidate photos have a text alternative that names the candidate, or are marked decorative when the name is next to them. Nothing is announced twice. | | |
| 1.3.1 Info and relationships | Each seat is announced as a group with its position title. Each option is announced with its candidate name. Radio versus checkbox is clear, and the seat rule (choose one, or choose up to N) is read with the group. | | |
| 1.3.1 Abstain option | Every seat ends with "Abstain on this seat", read with its help line "Your ballot still counts, but no candidate on this seat gets your vote." Web reads the line through `aria-describedby`; mobile reads it as the option's subtitle. | | |
| 1.3.2 Meaningful sequence | Reading order is seat title, instructions, candidates, abstain option, then the review button. It matches the visual order. | | |
| 1.4.1 Use of color | A selected option, a disabled option and a reached limit are told apart without color. | | |
| 1.4.3 Contrast (minimum) | Text and the primary button meet 4.5:1 in the light and dark themes. | | |
| 1.4.4 Resize text | At 200 percent zoom or the largest font size, nothing is cut off and no button is hidden. | | |
| 1.4.10 Reflow | At 320 px wide there is no two-way scrolling for the ballot. | | |
| 1.4.11 Non-text contrast | Radio and checkbox outlines, focus rings and the selected state meet 3:1. | | |
| 2.1.1 Keyboard | Every candidate, the abstain option, Review ballot, Back and Confirm and cast ballot can be reached and used. On mobile every control works with TalkBack double-tap. | | |
| 2.1.2 No keyboard trap | Focus can leave every control and group with the keyboard. | | |
| 2.4.3 Focus order | Tab order follows the reading order, including after moving between the paper and the review step. | | |
| 2.4.6 Headings and labels | Seat titles are headings. Button labels say what happens, for example Review ballot and Confirm and cast ballot. | | |
| 2.4.7 Focus visible | A visible focus ring shows on every control, in both themes. | | |
| 2.4.11 Focus not obscured (minimum) | A sticky header, banner or on-screen keyboard never covers the focused control. | | |
| 2.5.3 Label in name | The spoken name of each control contains its visible label. | | |
| 2.5.7 Dragging movements | N/A unless a control needs a drag. Note any found. | | |
| 2.5.8 Target size (minimum) | Options and buttons are at least 24 by 24 CSS px, or have enough spacing. On mobile aim for 48 dp. | | |
| 3.2.2 On input | Choosing an option does not move focus or change the page. Reaching the limit blocks further picks without a surprise. | | |
| 3.3.1 Error identification | While any seat is undecided, Review ballot is unavailable and the line "Pick a candidate or abstain on every seat to continue." is shown and read. A seat the voter has not touched is never read or shown as abstained. Note whether the missing seats are easy to find. | | |
| 3.3.2 Labels or instructions | The rule for each seat and the warning that a vote cannot be changed after confirming are read before the Confirm button. | | |
| 3.3.4 Error prevention (legal, financial, data) | The review step shows every choice, with Abstained for blank seats, before the ballot is final. Back returns to the paper with choices kept. | | |
| 4.1.2 Name, role, value | Each option reports name, role and checked state. A disabled option reports as unavailable. The Confirm button shows a busy state to the screen reader while recording. | | |
| 4.1.3 Status messages | Loading, the empty states, a failed vote and a recorded vote are announced without moving focus. | | |
| 4.1.3 Ineligible notice | A member who cannot vote hears "You cannot vote in this election" and the reason when the screen loads, and every option reports as unavailable. | | |

## Receipt and print step, web and mobile

| Criterion | Check with keyboard only and with a screen reader | Result | Notes |
|---|---|---|---|
| 1.3.1 Info and relationships | The recorded message, tracking code and each seat line are read in order with their labels. | | |
| 1.4.3 Contrast (minimum) | Tracking code and receipt text meet 4.5:1 on screen and in the printout. | | |
| 2.1.1 Keyboard | Print receipt and Download receipt work with Enter and Space. On mobile the Download receipt button opens the share sheet with TalkBack. | | |
| 2.4.3 Focus order | After a successful vote, focus lands on the confirmation heading or message, not on a hidden or removed control. | | |
| 2.4.7 Focus visible | Focus ring on Print receipt, Download receipt and on any link to results. | | |
| 2.5.8 Target size (minimum) | The print and download buttons meet the target size. | | |
| 3.2.2 On input | Print receipt opens the print dialog, and Download receipt saves the file or opens the share sheet, only on activation and without leaving the screen. | | |
| 3.3.1 Error identification | If printing fails, or a saved receipt on the web cannot be read, the message is text and is announced. | | |
| 4.1.2 Name, role, value | The tracking code can be selected and copied with the keyboard, and the screen reader can read it character by character. | | |
| 4.1.3 Status messages | The message that the ballot was recorded is announced as a status. The note that the printed receipt can be printed only here, and that the download has no choices, is read. | | |
| Print output | The printed page has a readable title, date, tracking code and marked ballot. Page chrome, buttons and the vote form do not print. Text is at least 11 pt. | | |
| Downloaded PDF | The PDF shows the election title, the tracking code and the time of voting, with no choices. A Bengali title reads correctly. | | |

## Tracking-code check, web and mobile

| Criterion | Check with keyboard only and with a screen reader | Result | Notes |
|---|---|---|---|
| 1.1.1 Non-text content | Any icon beside the code search has a text alternative or is hidden from the screen reader. | | |
| 1.3.1 Info and relationships | The tracking-code search has a visible label, not only a placeholder. The code list is a list or table with a header. | | |
| 1.4.3 Contrast (minimum) | Placeholder text, the code list and the no-match message meet 4.5:1. | | |
| 1.4.10 Reflow | A long list of codes wraps or scrolls inside its own area at 320 px. | | |
| 2.1.1 Keyboard | The search field and the list can be reached and read with the keyboard. A long list does not need a mouse. | | |
| 2.4.3 Focus order | Focus moves from the results summary to the search field to the list. | | |
| 2.4.7 Focus visible | Focus ring on the field and on any row or button in the list. | | |
| 3.3.2 Labels or instructions | The field says what to enter, for example the code printed on the receipt. | | |
| 4.1.2 Name, role, value | The search field exposes name and value. The list exposes its item count. | | |
| 4.1.3 Status messages | The match count, no match, codes not published yet and a load error are announced without moving focus. | | |

## Public results page, web and the mobile results screen

| Criterion | Check with keyboard only and with a screen reader | Result | Notes |
|---|---|---|---|
| 1.1.1 Non-text content | Any chart has a text alternative or a table beside it. Elected markers use text, not only an icon or color. | | |
| 1.3.1 Info and relationships | Results are a table or list with headers: seat, candidate, votes, elected. Ties are stated in text. | | |
| 1.4.1 Use of color | Elected, tie and not elected are told apart without color. | | |
| 1.4.3 Contrast (minimum) | Table text, badges and links meet 4.5:1 in both themes. | | |
| 1.4.10 Reflow | The tables reflow or scroll inside their own container at 320 px. | | |
| 2.1.1 Keyboard | All links, filters and the tracking-code search work with the keyboard. | | |
| 2.4.2 Page titled | The page title names the election and says it is the results page. | | |
| 2.4.6 Headings and labels | One page heading, then a heading for each seat and for the tracking codes. | | |
| 2.4.7 Focus visible | Focus ring on every interactive element. | | |
| 3.1.1 Language of page | The page language is set, so NVDA uses the right voice. | | |
| 4.1.2 Name, role, value | Table cells are read with their row and column headers by NVDA, and by TalkBack in the mobile layout. | | |
| 4.1.3 Status messages | Not counted yet, not published yet and a load error are announced as text. | | |

## Sign-off

| Item | Entry |
|---|---|
| Fails found | |
| Tracker items raised | |
| Re-test needed | |
| Signed off by | |
| Date | |
