from pathlib import Path
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4, landscape
from reportlab.pdfbase.pdfmetrics import stringWidth
from reportlab.pdfgen import canvas


OUT = Path(__file__).resolve().parents[2] / "output" / "pdf" / "election-mechanism-diagrams-a4.pdf"
PAGE_W, PAGE_H = landscape(A4)
NAVY = colors.HexColor("#17324D")
INK = colors.HexColor("#1F2937")
MUTED = colors.HexColor("#526273")
LINE = colors.HexColor("#8293A2")
BLUE = colors.HexColor("#E8F1FA")
GOLD = colors.HexColor("#FFF3D7")
GREEN = colors.HexColor("#E5F3EA")
PURPLE = colors.HexColor("#F0EAF7")
RED = colors.HexColor("#FBE9E7")
WHITE = colors.white


def wrap(text, font, size, width):
    words = text.split()
    lines, line = [], ""
    for word in words:
        candidate = word if not line else f"{line} {word}"
        if stringWidth(candidate, font, size) <= width:
            line = candidate
        else:
            if line:
                lines.append(line)
            line = word
    if line:
        lines.append(line)
    return lines


class Sheet:
    def __init__(self, path):
        self.c = canvas.Canvas(str(path), pagesize=landscape(A4), pageCompression=1)
        self.c.setTitle("GHCAA Election Mechanism - A4 Diagrams")
        self.page = 0

    def header(self, title, subtitle):
        self.page += 1
        c = self.c
        c.setFillColor(NAVY)
        c.rect(0, PAGE_H - 54, PAGE_W, 54, fill=1, stroke=0)
        c.setFillColor(WHITE)
        c.setFont("Helvetica-Bold", 17)
        c.drawString(30, PAGE_H - 28, title)
        c.setFont("Helvetica", 8.5)
        c.drawString(30, PAGE_H - 43, subtitle)

    def footer(self, label):
        c = self.c
        c.setStrokeColor(colors.HexColor("#CBD5DF"))
        c.line(28, 25, PAGE_W - 28, 25)
        c.setFillColor(MUTED)
        c.setFont("Helvetica", 7)
        c.drawString(30, 13, f"GHCAA election mechanism | {label} | Reviewed 25 Oct 2026")
        c.drawRightString(PAGE_W - 30, 13, f"A4 landscape | {self.page} / 6")

    def box(self, x, y, w, h, title, rows, fill=BLUE, font=7.0):
        c = self.c
        c.setFillColor(fill)
        c.setStrokeColor(LINE)
        c.setLineWidth(.65)
        c.roundRect(x, y, w, h, 4, fill=1, stroke=1)
        c.setFillColor(NAVY)
        c.roundRect(x, y + h - 17, w, 17, 4, fill=1, stroke=0)
        c.rect(x, y + h - 17, w, 4, fill=1, stroke=0)
        c.setFillColor(WHITE)
        c.setFont("Helvetica-Bold", 8)
        c.drawString(x + 6, y + h - 11.5, title)
        c.setFillColor(INK)
        cy = y + h - 27
        leading = font + 2
        for row in rows:
            lines = wrap(row, "Helvetica", font, w - 12)
            for line in lines:
                if cy < y + 5:
                    break
                c.setFont("Helvetica", font)
                c.drawString(x + 6, cy, line)
                cy -= leading
        return (x, y, w, h)

    def uml(self, x, y, w, h, name, attributes=(), operations=(), stereotype="entity", fill=BLUE, font=6.5):
        c = self.c
        c.setFillColor(fill)
        c.setStrokeColor(LINE)
        c.setLineWidth(.75)
        c.rect(x, y, w, h, fill=1, stroke=1)
        c.setFillColor(NAVY)
        c.rect(x, y + h - 23, w, 23, fill=1, stroke=0)
        c.setFillColor(WHITE)
        c.setFont("Helvetica-Oblique", 6.2)
        c.drawCentredString(x + w / 2, y + h - 8, f"<<{stereotype}>>")
        c.setFont("Helvetica-Bold", 8)
        c.drawCentredString(x + w / 2, y + h - 19, name)
        attr_h = max(18, (h - 23) * .52)
        c.setStrokeColor(LINE)
        c.line(x, y + h - 23 - attr_h, x + w, y + h - 23 - attr_h)
        c.setFillColor(INK)
        cy = y + h - 31
        for entry in attributes:
            for line in wrap(entry, "Helvetica", font, w - 10):
                if cy < y + h - 23 - attr_h + 3:
                    break
                c.setFont("Helvetica", font)
                c.drawString(x + 5, cy, line)
                cy -= font + 1.2
        cy = y + h - 23 - attr_h - 9
        for entry in operations:
            for line in wrap(entry, "Helvetica", font, w - 10):
                if cy < y + 3:
                    break
                c.setFont("Helvetica", font)
                c.drawString(x + 5, cy, line)
                cy -= font + 1.2

    def uml_relation(self, start, end, label="", kind="association", start_mult="", end_mult="", label_offset=4):
        c = self.c
        x1, y1 = start
        x2, y2 = end
        c.setStrokeColor(LINE)
        c.setFillColor(LINE)
        c.setLineWidth(.8)
        c.setDash(3, 2) if kind in ("dependency", "realization") else c.setDash()
        c.line(x1, y1, x2, y2)
        c.setDash()
        angle = __import__("math").atan2(y2 - y1, x2 - x1)
        if kind == "composition":
            length, half = 7, 3
            ux, uy = __import__("math").cos(angle), __import__("math").sin(angle)
            px, py = -uy, ux
            tip = (x1, y1)
            base = (x1 - length * ux, y1 - length * uy)
            p = c.beginPath()
            p.moveTo(*tip)
            p.lineTo(base[0] + half * px, base[1] + half * py)
            p.lineTo(base[0] - half * px, base[1] - half * py)
            p.close()
            c.drawPath(p, fill=1, stroke=1)
        elif kind in ("realization", "generalization"):
            length, half = 9, 4
            ux, uy = __import__("math").cos(angle), __import__("math").sin(angle)
            px, py = -uy, ux
            base = (x2 - length * ux, y2 - length * uy)
            p = c.beginPath()
            p.moveTo(x2, y2)
            p.lineTo(base[0] + half * px, base[1] + half * py)
            p.lineTo(base[0] - half * px, base[1] - half * py)
            p.close()
            c.setFillColor(WHITE)
            c.drawPath(p, fill=1, stroke=1)
        elif kind == "dependency":
            self.arrow(start, end)
        if label:
            c.setFillColor(INK)
            c.setFont("Helvetica", 6)
            c.drawCentredString((x1 + x2) / 2, (y1 + y2) / 2 + label_offset, label)
        if start_mult:
            c.setFillColor(INK)
            c.setFont("Helvetica", 6)
            c.drawString(x1 + 2, y1 + 3, start_mult)
        if end_mult:
            c.setFillColor(INK)
            c.setFont("Helvetica", 6)
            c.drawRightString(x2 - 2, y2 + 3, end_mult)

    def note(self, x, y, w, h, text, fill=GOLD, size=8):
        c = self.c
        c.setFillColor(fill)
        c.setStrokeColor(LINE)
        c.roundRect(x, y, w, h, 4, fill=1, stroke=1)
        yy = y + h - 12
        for ln in wrap(text, "Helvetica-Bold", size, w - 12):
            c.setFillColor(INK)
            c.setFont("Helvetica-Bold", size)
            c.drawString(x + 6, yy, ln)
            yy -= size + 2

    def arrow(self, a, b, label="", dashed=False, color=LINE, bend=None):
        c = self.c
        x1, y1 = a
        x2, y2 = b
        c.setStrokeColor(color)
        c.setFillColor(color)
        c.setLineWidth(.8)
        if dashed:
            c.setDash(3, 2)
        if bend is None:
            c.line(x1, y1, x2, y2)
        else:
            p = c.beginPath()
            p.moveTo(x1, y1)
            p.curveTo(bend[0], bend[1], bend[2], bend[3], x2, y2)
            c.drawPath(p)
        c.setDash()
        angle = __import__("math").atan2(y2 - y1, x2 - x1)
        size = 4
        left = (x2 - size * __import__("math").cos(angle - .5), y2 - size * __import__("math").sin(angle - .5))
        right = (x2 - size * __import__("math").cos(angle + .5), y2 - size * __import__("math").sin(angle + .5))
        p = c.beginPath()
        p.moveTo(x2, y2)
        p.lineTo(*left)
        p.lineTo(*right)
        p.close()
        c.drawPath(p, fill=1, stroke=0)
        if label:
            c.setFillColor(INK)
            c.setFont("Helvetica", 6.2)
            c.drawCentredString((x1 + x2) / 2, (y1 + y2) / 2 + 3, label)

    def legend(self, x, y):
        self.note(x, y, 186, 33, "Solid arrow = persisted FK. Dashed line = actor reference or process-only association.", fill=WHITE, size=7)

    def show(self):
        self.c.showPage()


def erd_page_one(s):
    s.header("Election ERD | 1. Governance, eligibility and nominations", "Persisted election setup and candidate preparation. PK/FK labels reflect current domain and EF configuration.")
    b = {}
    b["period"] = s.box(30, 350, 125, 92, "ECPeriod", ["PK Id", "term dates / status"], fill=GREEN)
    b["election"] = s.box(200, 326, 203, 140, "Election", ["PK Id | FK ECPeriodId", "Phase, IsTest, IsActive", "stage dates, tie rule", "BallotPublicKey + fingerprint", "BallotSealVersion"], fill=BLUE)
    b["seat"] = s.box(450, 359, 150, 92, "ElectionSeat", ["PK Id | FK ElectionId", "FK Position -> EcPosition", "SeatCount", "UQ (ElectionId, Position)"], fill=BLUE)
    b["position"] = s.box(648, 359, 150, 92, "EcPosition", ["PK Id", "position name / active"], fill=GREEN)
    b["persona"] = s.box(30, 170, 150, 105, "ElectionPersona", ["PK Id", "name, group, capacity", "permissions, declaration", "active / eligibility rules"], fill=PURPLE)
    b["appointment"] = s.box(225, 155, 190, 135, "ElectionAppointment", ["PK Id | FK ElectionId", "FK PersonaId, UserId", "optional MemberId", "accepted + signed declaration", "revoked / expires / RO flag", "UQ live RO per election"], fill=PURPLE)
    b["approval"] = s.box(466, 162, 174, 122, "ElectionApproval", ["PK Id | FK ElectionId", "action + payload", "requester / approver / executor", "expiry, rejection, consumed", "UQ OpenKey while pending"], fill=PURPLE)
    b["audit"] = s.box(683, 170, 150, 105, "ElectionAuditEntry", ["PK Id | FK ElectionId", "Seq, actor, action, detail", "PrevHash -> Hash", "UQ (ElectionId, Seq)"], fill=PURPLE)
    s.box(648, 285, 185, 64, "ElectionRulesUnlock (site-wide)", ["PK Id | SuperAdmin, reason", "OpenedAt / ExpiresAt / ClosedAt"], fill=RED, font=6.5)
    b["voter"] = s.box(30, 45, 165, 84, "VoterRoll", ["PK Id | FK ElectionId", "FK MemberId", "eligibility + reason", "FrozenAt, VotedAt (date only)", "UQ (ElectionId, MemberId)"], fill=GOLD, font=6.5)
    b["nom"] = s.box(250, 35, 190, 105, "Nomination", ["PK Id | FK ElectionId, ElectionSeatId", "candidate / proposer / seconder", "all three identities must differ", "status, consent, ballot order", "UQ (ElectionSeatId, CandidateMemberId)"], fill=GOLD, font=6.2)
    b["decision"] = s.box(493, 46, 145, 84, "ScrutinyDecision", ["PK Id | FK NominationId", "FK DecidedByUserId", "accepted, reason, decided at"], fill=GOLD)
    b["checks"] = s.box(682, 46, 151, 84, "NominationScrutinyCheck", ["PK Id | FK NominationId", "check item + checked state", "set by / set at", "UQ (NominationId, Item)"], fill=GOLD, font=6.5)

    s.arrow((155, 395), (200, 395), "1 : many")
    s.arrow((403, 395), (450, 395), "1 : many")
    s.arrow((600, 404), (648, 404), "Position FK")
    s.arrow((300, 326), (320, 290), "appointments")
    s.arrow((180, 220), (225, 220), "persona")
    s.arrow((300, 326), (532, 284), "approval history", bend=(360, 320, 450, 300))
    s.arrow((403, 350), (683, 235), "hash chain", bend=(480, 300, 590, 260))
    s.arrow((300, 326), (105, 129), "roll snapshot", bend=(240, 245, 135, 180))
    s.arrow((330, 326), (344, 140), "candidate records", bend=(350, 265, 360, 195))
    s.arrow((450, 390), (395, 140), "seat", bend=(500, 320, 430, 215))
    s.arrow((440, 88), (493, 88), "decisions")
    s.arrow((440, 75), (682, 75), "checklist", bend=(520, 30, 610, 30))
    s.note(30, 302, 125, 32, "Member and User are existing platform tables; referenced by FK columns shown here.", fill=WHITE, size=6.7)
    s.note(648, 238, 185, 30, "Solid = persisted FK. Dashed = process or actor reference.", fill=WHITE, size=6.4)
    s.footer("ERD 1 of 2")
    s.show()


def erd_page_two(s):
    s.header("Election ERD | 2. Participation, sealed ballots and outcomes", "Ballot secrecy depends on keeping voter-side evidence separate from anonymous counted choices.")
    b = {}
    b["election"] = s.box(35, 350, 145, 100, "Election", ["PK Id", "phase + key fingerprint", "one live real election / period"], fill=BLUE)
    b["seat"] = s.box(35, 184, 145, 88, "ElectionSeat", ["PK Id | FK ElectionId", "FK Position", "SeatCount"], fill=BLUE)
    b["member"] = s.box(35, 55, 145, 80, "Member", ["PK Id", "platform identity"], fill=GREEN)
    b["roll"] = s.box(235, 328, 170, 103, "VoterRoll", ["FK ElectionId, MemberId", "IsEligible + reason", "FrozenAt, VotedAt", "UQ election + member"], fill=GOLD)
    b["seatvote"] = s.box(235, 185, 170, 90, "SeatVote", ["FK ElectionId, ElectionSeatId", "MemberId value, VotedAt (date only)", "UQ election + seat + member", "Participation evidence only"], fill=GOLD, font=6.4)
    b["pending"] = s.box(470, 365, 165, 86, "PendingBallot", ["PK Id | FK ElectionId", "SealedChoices ciphertext", "no member or cast time"], fill=RED)
    b["receipt"] = s.box(690, 365, 145, 86, "BallotReceipt", ["PK Id | FK ElectionId", "TrackingCode", "UQ election + code", "no ballot FK"], fill=RED)
    b["ballot"] = s.box(470, 222, 165, 92, "Ballot", ["random PK Guid", "FK ElectionId", "IsSpoiled", "no voter or cast time"], fill=GREEN)
    b["vote"] = s.box(690, 211, 145, 112, "BallotVote", ["random PK Guid", "FK BallotId", "FK ElectionSeatId", "FK NominationId", "no member", "no timestamp"], fill=GREEN)
    b["result"] = s.box(235, 45, 170, 93, "ElectionResult", ["FK ElectionId", "ElectionSeatId + NominationId values", "VoteCount, IsElected, IsTie", "UQ election + seat + nomination"], fill=GREEN, font=6.2)
    b["recount"] = s.box(470, 43, 165, 102, "ElectionRecountRequest", ["FK ElectionId + ElectionSeatId", "applicant, grounds, decision", "recorded / decided / recounted by"], fill=PURPLE, font=6.5)
    b["figure"] = s.box(690, 56, 145, 78, "ElectionRecountFigure", ["FK RecountRequestId", "FK NominationId", "VoteCount", "UQ request + nomination"], fill=PURPLE, font=6.3)

    s.arrow((180, 400), (235, 382), "1 : many")
    s.arrow((180, 386), (470, 407), "sealed rows", bend=(270, 440, 370, 440))
    s.arrow((180, 375), (690, 407), "receipt rows", bend=(340, 460, 560, 460))
    s.arrow((180, 230), (235, 230), "seat participation")
    s.arrow((180, 90), (235, 350), "MemberId FK", bend=(200, 155, 200, 280))
    s.arrow((180, 210), (690, 250), "seat", bend=(350, 180, 520, 180))
    s.arrow((180, 390), (470, 275), "count writes shuffled ballots", bend=(300, 330, 390, 300))
    s.arrow((635, 266), (690, 266), "1 : many")
    s.arrow((180, 375), (235, 90), "results", bend=(190, 270, 210, 130))
    s.arrow((180, 220), (470, 95), "recount request", bend=(290, 185, 390, 110))
    s.arrow((635, 94), (690, 94), "figures")
    s.note(470, 150, 365, 48, "No FK or join connects Member, VoterRoll or SeatVote to PendingBallot, BallotReceipt, Ballot or BallotVote. SeatVote.MemberId and ElectionResult seat/nomination ids are stored values without configured FKs.", fill=GOLD, size=6.5)
    s.note(35, 300, 370, 34, "One ballot may omit a seat's BallotVote rows: omission represents abstention. SeatVote records participation per seat.", fill=WHITE, size=7)
    s.footer("ERD 2 of 2")
    s.show()


def uml_service_page(s):
    s.header("UML class diagram | API and application services", "Class compartments show visibility, key fields and operations. Dashed realization implements an interface; dashed arrows are dependencies. Client UI is omitted.")
    s.uml(35, 352, 210, 105, "ElectionsController", ["- elections: IElectionService", "- approvals: IElectionApprovalService"], ["+ Vote(...) : IActionResult", "+ Results(id) : IActionResult"], stereotype="controller", fill=PURPLE)
    s.uml(285, 352, 220, 105, "AdminElectionsController", ["- elections: IElectionService", "- approvals / access / forms"], ["+ Create(...) : IActionResult", "+ AddCandidate(...) : IActionResult"], stereotype="controller", fill=PURPLE, font=6.2)
    s.uml(560, 352, 255, 105, "IElectionService", [], ["+ FreezeVoterRollAsync(id) : Task<int>", "+ CastBallotAsync(...) : Task<Result>", "+ CountAsync(...) : Task<Results>", "+ DeclareAsync(id) : Task<bool>"], stereotype="interface", fill=BLUE, font=6.2)
    s.uml(560, 183, 255, 118, "ElectionService", ["- db: ApplicationDbContext", "- audit: IElectionAuditWriter"], ["+ FreezeVoterRollAsync(id)", "+ SubmitNominationAsync(...)", "+ CastBallotAsync(...)", "+ CountAsync(...)", "+ DeclareAsync(...)"], stereotype="service", fill=GOLD, font=6.1)
    s.uml(285, 183, 220, 118, "IElectionApprovalService", [], ["+ RunOrRequestAsync(...) : RunResult", "+ RequestAsync(...) : Approval", "+ ApproveAsync(...) : Result", "+ CountAsync(...) : CountResult"], stereotype="interface", fill=BLUE, font=6.0)
    s.uml(35, 183, 210, 118, "ElectionApprovalService", ["- elections: IElectionService", "- access: IElectionAccessService", "- orgConfig: IOrgConfigService", "- appointments: IElectionAppointmentService"], ["+ RunOrRequestAsync(...) ", "+ RequestAsync(...) ", "+ ApproveAsync(...) ", "+ CountAsync(...)"], stereotype="service", fill=GOLD, font=5.6)
    s.uml(35, 48, 175, 83, "ElectionAuditWriter", [], ["+ AppendAsync(...) : Task", "+ VerifyAsync(...) : Task<bool>"], stereotype="service", fill=GOLD, font=6.1)
    s.uml(235, 48, 175, 83, "ElectionAccessService", [], ["+ HasPermissionAsync(...) : Task<bool>", "+ GetPermissionsAsync(...)"], stereotype="service", fill=GOLD, font=5.9)
    s.uml(435, 48, 175, 83, "ElectionAppointmentService", [], ["+ AcceptAsync(...) : Task", "+ RevokeAsync(...) : Task"], stereotype="service", fill=GOLD, font=6.1)
    s.uml(635, 48, 175, 83, "ApplicationDbContext", ["+ Elections, Seats, Ballots..."], [], stereotype="persistence", fill=GREEN, font=6)
    s.uml_relation((245, 420), (560, 420), "uses", "dependency")
    s.uml_relation((505, 417), (560, 400), "uses", "dependency")
    s.uml_relation((395, 352), (395, 301), "uses", "dependency")
    s.uml_relation((680, 301), (680, 352), "", "realization")
    s.uml_relation((245, 242), (285, 242), "", "realization")
    s.uml_relation((245, 275), (560, 371), "uses", "dependency")
    s.uml_relation((610, 183), (560, 131), "audit", "dependency")
    s.uml_relation((245, 207), (322, 131), "permissions", "dependency")
    s.uml_relation((245, 222), (522, 131), "appointments", "dependency")
    s.uml_relation((690, 183), (722, 131), "EF Core", "dependency")
    s.footer("UML services")
    s.show()


def uml_governance_page(s):
    s.header("UML class diagram | Election governance and nominations", "Domain classes with key attributes, behavior and association multiplicities.")
    s.uml(330, 376, 190, 100, "Election", ["- id: int", "- phase: ElectionPhase", "- ECPeriodId: int", "- stageDates: DateTimeSet", "- ballotKey / fingerprint / IsTest"], [], fill=BLUE, font=6.1)
    s.uml(30, 385, 145, 62, "EcPosition", ["- id: int", "- name: string"], [], fill=GREEN, font=6.2)
    s.uml(30, 270, 150, 78, "ECPeriod", ["- id: int", "- startDate: Date"], [], fill=GREEN, font=6.3)
    s.uml(230, 270, 175, 78, "ElectionSeat", ["- id: int", "- Position: int", "- SeatCount: int"], [], fill=BLUE, font=6.3)
    s.uml(455, 270, 175, 78, "ElectionPersona", ["- id: int", "- permissions", "- declarationText"], [], fill=PURPLE, font=6.3)
    s.uml(675, 260, 155, 95, "ElectionAppointment", ["- userId: int", "- acceptedAt / revokedAt", "- isReturningOfficer"], ["+ IsLive(now) : bool"], fill=PURPLE, font=5.9)
    s.uml(35, 90, 175, 100, "VoterRoll", ["- memberId: int", "- isEligible: bool", "- frozenAt", "- votedAt (date)"], [], fill=GOLD, font=6.1)
    s.uml(250, 80, 190, 110, "Nomination", ["- candidate / proposer / seconder", "- status, consentAt", "- ballotOrder"], [], fill=GOLD, font=6.1)
    s.uml(485, 90, 155, 100, "ScrutinyDecision", ["- nominationId: int", "- accepted: bool", "- reason / decidedAt"], [], fill=GOLD, font=5.9)
    s.uml(680, 90, 150, 100, "ElectionApproval", ["- action / payload", "- requestedBy / approvedBy", "- expiresAt / executedAt"], ["+ IsOpenAt(now) : bool"], fill=PURPLE, font=5.8)
    s.uml_relation((180, 309), (330, 421), "period", "association", "1", "0..*")
    s.uml_relation((425, 390), (330, 348), "seats", "composition", "1", "1..*")
    s.uml_relation((230, 310), (175, 415), "position", "association", "0..*", "1")
    s.uml_relation((630, 309), (675, 309), "appointments", "composition", "1", "0..*")
    s.uml_relation((675, 283), (630, 283), "persona", "association", "0..*", "1")
    s.uml_relation((365, 390), (122, 190), "voter roll", "composition", "1", "0..*")
    s.uml_relation((425, 390), (345, 190), "nominations", "composition", "1", "0..*")
    s.uml_relation((440, 134), (485, 134), "decisions", "association", "1", "0..*")
    s.uml_relation((520, 390), (720, 355), "approval records", "composition", "1", "0..*")
    s.note(35, 42, 795, 30, "Multiplicity: 1 = exactly one; 0..* = zero or more; 1..* = one or more. Candidate, proposer and seconder differ. ElectionRulesUnlock is site-wide. ERDs show database constraints.", fill=WHITE, size=6.8)
    s.footer("UML governance")
    s.show()


def uml_ballot_page(s):
    s.header("UML class diagram | Participation, ballots and outcomes", "No identity-to-choice association. SeatVote stores a MemberId value and date only; the ERD shows the full electionId links.")
    s.uml(345, 400, 155, 70, "Election", ["- id: int", "- phase: ElectionPhase"], [], fill=BLUE, font=6.2)
    s.uml(30, 280, 145, 78, "ElectionSeat", ["- id: int", "- seatCount: int"], [], fill=BLUE, font=6.2)
    s.uml(235, 280, 145, 78, "Nomination", ["- id: int", "- candidateMemberId", "- ballotOrder"], [], fill=GOLD, font=6.0)
    s.uml(455, 280, 145, 78, "Member", ["- id: int", "- platform member"], [], fill=GREEN, font=6.2)
    s.uml(665, 280, 165, 78, "ElectionResult", ["- electionId / seatId", "- nominationId / voteCount", "- isElected / isTie"], [], fill=GREEN, font=6.0)
    s.uml(25, 125, 150, 82, "VoterRoll", ["- electionId / memberId", "- isEligible", "- votedAt (date)"], [], fill=GOLD, font=6.1)
    s.uml(195, 125, 150, 82, "SeatVote", ["- electionId / seatId", "- memberId value", "- votedAt (date)"], [], fill=GOLD, font=6.0)
    s.uml(365, 125, 150, 82, "PendingBallot", ["- sealedChoices", "- electionId: int"], [], fill=RED, font=6.1)
    s.uml(535, 125, 140, 82, "BallotReceipt", ["- trackingCode", "- electionId: int"], [], fill=RED, font=6.1)
    s.uml(695, 125, 135, 82, "Ballot", ["- id: Guid", "- electionId", "- isSpoiled"], [], fill=GREEN, font=6.1)
    s.uml(205, 35, 155, 72, "ElectionRecountRequest", ["- seatId / grounds", "- allowed / decidedAt"], [], fill=PURPLE, font=5.8)
    s.uml(435, 35, 165, 72, "BallotVote", ["- ballotId: Guid", "- seatId / nominationId"], [], fill=GREEN, font=5.9)
    s.uml(665, 35, 165, 72, "ElectionAuditEntry", ["- electionId / seq", "- action", "- prevHash / hash"], [], fill=PURPLE, font=5.9)
    s.uml_relation((345, 445), (100, 358), "seats", "composition", "1", "1..*")
    s.uml_relation((380, 400), (100, 207), "voter roll", "composition", "1", "0..*")
    s.uml_relation((175, 319), (235, 319), "for seat", "association", "1", "0..*")
    s.uml_relation((380, 319), (665, 319), "candidate result", "association", "0..1", "1")
    s.uml_relation((760, 125), (515, 107), "choices", "composition", "1", "0..*")
    s.uml_relation((175, 300), (515, 107), "seat reference", "association", "1", "0..*")
    s.uml_relation((300, 280), (515, 107), "nomination reference", "association", "1", "0..*")
    s.uml_relation((100, 280), (270, 207), "seat participation", "association", "1", "0..*")
    s.uml_relation((100, 280), (280, 107), "recount requests", "association", "1", "0..*")
    s.uml_relation((455, 300), (175, 185), "member", "association", "1", "0..*")
    s.footer("UML ballots")
    s.show()


def flow_page(s):
    s.header("Election workflow | Full lifecycle and control gates", "End-to-end flow from election creation through appeals and archive. Approval is separate from execution.")
    x0, boxw, gap, ytop, boxh = 33, 185, 21, 440, 78
    stages = [
        ("1. Configure", BLUE, ["Admin -> API -> ElectionService", "Create for ECPeriod; set dates / tie rule", "Add seats, personas, appointments", "DB prevents live-period collision"]),
        ("2. Freeze + prepare", GOLD, ["Freeze eligible-member snapshot before or during Nomination", "Readiness checks roll, order, key, declarations", "Attestations recorded before Polling", "Approval gates follow organisation config"]),
        ("3. Nominate", PURPLE, ["Member submits candidate + proposer + seconder", "All three identities must differ", "Officials scrutinize; consent and withdrawal", "CandidateList locks and orders nominations"]),
        ("4. Open polling", GOLD, ["Phase gate; second official if configured", "Polling gate checks readiness + schedule", "Election settings and appointments freeze", "Members receive complete seat ballot"]),
        ("5. Cast ballot", RED, ["Member reviews and submits once", "Server checks roll, phase, seat/candidate limits", "Serializable transaction writes SeatVote + ciphertext", "Receipt code returned separately"]),
        ("6. Close + count", GOLD, ["Polling closes; count approval if configured", "Older configs without Count use one approval", "Returning Officer key opens PendingBallot", "Shuffle choices and codes independently"]),
        ("7. Declare + publish", GREEN, ["Persist anonymous Ballot / BallotVote + results", "Clear pending ciphertext; minimum-count rule", "Winners join this election's ECPeriod", "Publish results, codes, audit / allowed ballots"]),
        ("8. Review + archive", BLUE, ["Receive and decide appeals / recount requests", "Record recount figures and audit events", "Respect recount window before key destruction", "Expire appointments; archive election records"]),
    ]
    xs = [x0 + i * (boxw + gap) for i in range(4)]
    topy = ytop - boxh
    for i in range(4):
        title, fill, rows = stages[i]
        s.box(xs[i], topy, boxw, boxh, title, rows, fill=fill, font=6.05)
    for i in range(3):
        s.arrow((xs[i] + boxw, topy + boxh / 2), (xs[i + 1], topy + boxh / 2), str(i + 1))

    row2_y = 252
    xs2 = list(reversed(xs))
    for j in range(4):
        title, fill, rows = stages[4 + j]
        s.box(xs2[j], row2_y, boxw, boxh, title, rows, fill=fill, font=6.05)
    for i in range(3):
        s.arrow((xs2[i], row2_y + boxh / 2), (xs2[i + 1] + boxw, row2_y + boxh / 2), str(5 + i))
    s.arrow((xs[3] + boxw / 2, topy), (xs2[0] + boxw / 2, row2_y + boxh), "4", bend=(xs[3] + boxw / 2, 330, xs2[0] + boxw / 2, 330))
    s.note(35, 105, 810, 106, "Control gates and data transitions\n• Phase changes are validated by ElectionService. TwoPersonActions is organisation-configured; a distinct second official is required only for configured actions, subject to the SuperAdmin exception. Older configs without Count require one approval for counting.\n• VoterRoll may be frozen before or during Nomination. A vote is one full submission, while SeatVote proves participation separately and stores the vote date without time.\n• PendingBallot holds ciphertext only. At count, the service decrypts using the Returning Officer's supplied private key, shuffles ballots and receipt codes separately, writes anonymous counted records and removes pending rows.\n• Declaration writes winners into the EC period selected when the election was created. A failed readiness gate prevents Polling; a failed count leaves the election undeclared. Audit entries form a per-election hash chain.", fill=WHITE, size=6.7)
    s.footer("Full flow")
    s.show()


def main():
    OUT.parent.mkdir(parents=True, exist_ok=True)
    s = Sheet(OUT)
    erd_page_one(s)
    erd_page_two(s)
    uml_service_page(s)
    uml_governance_page(s)
    uml_ballot_page(s)
    flow_page(s)
    s.c.save()
    print(OUT)


if __name__ == "__main__":
    main()
