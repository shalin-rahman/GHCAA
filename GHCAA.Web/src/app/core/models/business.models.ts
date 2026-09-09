export type MembershipStatus = 'Applied' | 'Active' | 'InactivePayment' | 'InactiveResigned' | 'Terminated';
// 35.4: keep in sync with MembershipType in GHCAA.Domain/Enums.cs (Guest is index 6).
export type MembershipType = 'Founding' | 'Executive' | 'General' | 'Associate' | 'Honorary' | 'Advisory' | 'Guest';
export type MemberCategory = 'None' | 'LifelongPatron' | 'Sponsor' | 'Advisor' | 'Mentor' | 'Recruiter' | 'Active' | 'Volunteer' | 'Contributor' | 'Guest' | 'Student';
export type ECPosition = 'None' | 'President' | 'VicePresident' | 'GeneralSecretary' | 'OfficeSecretary' | 'JointSecretary1' | 'JointSecretary2' | 'Treasurer' | 'MediaCulturalAndSportsSecretary' | 'OrganizationalSecretary' | 'InformationAndTechnologySecretary' | 'Member1' | 'Member2' | 'LawSecretary' | 'ImmediatePastPresident' | 'InstitutionalRepresentative';
export type Gender = 'Male' | 'Female' | 'Other';
export type BloodGroup = 'APositive' | 'ANegative' | 'BPositive' | 'BNegative' | 'OPositive' | 'ONegative' | 'ABPositive' | 'ABNegative';
export type RecordType = 'Income' | 'Expense';
export type FinancialCategory = 'MembershipFee' | 'RegistrationFee' | 'Donation' | 'Event' | 'Maintenance' | 'Salary' | 'Utilities' | 'Other';
export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded';
export type EventRegistrationStatus = 'Pending' | 'Approved' | 'Rejected';
export type PaymentMethod = 'ManualReceipt' | 'BKash' | 'Nagad' | 'Rocket' | 'CreditCard' | 'BankTransfer' | 'CashOnHand';
export type JobCategory = 'IT' | 'Finance' | 'Engineering' | 'Marketing' | 'Education' | 'Health' | 'PublicSector' | 'Mentorship' | 'Other';
export type SubmissionStatus = 'Draft' | 'Pending' | 'Approved' | 'Rejected';
export type ArticleCategory = 'Event' | 'Magazine' | 'Regular';

export enum PaymentGateway {
    None = 0,
    Stripe = 1,
    PayPal = 2,
    SSLCommerz = 3,
    BkashGateway = 4,
    NagadGateway = 5,
    RocketGateway = 6,
    BankTransferGateway = 7,
    Manual = 8,
    DGePay = 9
}
export interface Member {
    id: number;
    fullName: string;
    fatherName: string;
    motherName: string;
    dateOfBirth: string | Date;
    gender: Gender;
    bloodGroup: BloodGroup;
    nid: string;
    mobileNo: string;
    email: string;
    emailVerified: boolean;
    presentAddress: string;
    permanentAddress: string;
    emergencyContactName: string;
    emergencyContactRelation: string;
    emergencyContactPhone: string;
    tShirtSize?: string;
    isNIDPublic?: boolean;

    photoPath?: string;
    signaturePath?: string;
    status: MembershipStatus;
    appliedDate: string | Date;
    approvedDate?: string | Date;
    membershipNumber?: string;
    membershipType: MembershipType;
    category: MemberCategory;
    
    academicHistory?: AcademicRecord[];
    professionalHistory?: ProfessionalRecord[];
    ecHistory?: ECMember[];
    paymentHistories?: PaymentHistory[];
    
    // Gamification & Health
    contributionPoints?: number;
    rank?: number;
    categoryBadge?: string;
    profileCompletionPercentage?: number;
}

export interface PaymentHistory {
    id: number;
    // null for a guest event payment (no member record).
    memberId: number | null;
    transactionId: string;
    amount: number;
    paidAt: string | Date;
    status: PaymentStatus;
    financialCategory: FinancialCategory;
    notes?: string;
    paymentMethod: PaymentMethod;
    receiptPath?: string;
}

export interface SavedPaymentMethod {
    id: number;
    memberId: number;
    displayName: string;
    method: string;
    accountNumber: string;
    icon?: string;
    isDefault: boolean;
    lastUsedAt?: string | Date;
}

export interface ECPeriod {
    id: number;
    title: string;
    startDate: string | Date;
    endDate?: string | Date;
    isActive: boolean;
    ecMembers?: ECMember[];
}

export interface ECMember {
    id: number;
    ecPeriodId: number;
    memberId: number;
    position: ECPosition | number;
    startDate: string | Date;
    endDate?: string | Date;
    changeReason?: string;
    member?: Member;
    ecPeriod?: ECPeriod;
}

export interface EmailTemplate {
    id: number;
    code: string;
    subject: string;
    body: string;
    description: string;
    variables?: string; // JSON string
    lastUpdated: string | Date;
}

export interface FinancialRecord {
    id: number;
    year: number;
    recordType: RecordType;
    financialCategory: FinancialCategory;
    date: string | Date;
    amount: number;
    description: string;
    reference?: string;
    createdAt?: string | Date;
}

export interface LedgerSummary {
    year: number;
    totalIncome: number;
    totalExpense: number;
    netBalance: number;
    details?: LedgerCategorySummary[];
}

export interface LedgerCategorySummary {
    type: RecordType;
    financialCategory: FinancialCategory;
    total: number;
}


export type PostType = 'News' | 'Notice';

export interface NewsPost {
    id: number;
    title: string;
    content: string;
    articleCategory: ArticleCategory;
    postType: PostType;
    status: SubmissionStatus;
    imageUrl?: string;
    attachmentUrl?: string;
    attachmentFileName?: string;
    isActive: boolean;
    authorName?: string;
    createdAt: string | Date;
    collaborators: string[];
}

export interface SiteContent {
    id: number;
    key: string;
    group: string;
    title: string;
    bodyHtml: string;
    displayOrder: number;
    isActive: boolean;
    lastModified?: string | Date;
}

export interface UpsertSiteContentDto {
    key: string;
    group: string;
    title: string;
    bodyHtml: string;
    displayOrder: number;
    isActive: boolean;
}

export interface CreateNewsDto {
    title: string;
    content: string;
    articleCategory: ArticleCategory;
    postType?: PostType;
    status?: SubmissionStatus;
    publishDate?: string | Date;
    imageUrl?: string;
    attachmentUrl?: string;
    attachmentFileName?: string;
    isActive?: boolean;
    collaborators?: string[];
}

export interface UpdateNewsDto extends Partial<CreateNewsDto> {
    id: number;
}


export interface Job {
    id: number;
    title: string;
    companyName: string;
    location: string;
    description: string;
    requirements: string;
    applicationEmail?: string;
    applicationLink?: string;
    postedDate: string | Date;
    applicationDeadline?: string | Date;
    jobCategory: JobCategory;
    isActive: boolean;
    postedByMemberId: number;
    postedByMemberName?: string;
    status?: SubmissionStatus | string;
    rejectionReason?: string;
}

export interface CreateJobDto {
    title: string;
    companyName: string;
    location: string;
    description: string;
    requirements: string;
    applicationEmail?: string;
    applicationLink?: string;
    applicationDeadline?: string | Date;
    jobCategory: JobCategory;
}

export interface UpdateJobDto extends Partial<CreateJobDto> {
    id: number;
}


export interface SpecialDayTheme {
    id: number;
    title: string;
    startDate: string | Date;
    endDate: string | Date;
    backgroundColor: string;
    textColor: string;
    announcementText: string;
    animatedTexts: string[];
    animationStyle: string; // Fade, 3D, Typewriter, None
    imageUrl: string;
    sidebarColor: string;
    enableGradientFading: boolean;
    isActive: boolean;
    isEnabled: boolean; // mapped from backend
}

export interface ActivityLog {
    id: number;
    memberId?: number;
    action: string;
    details: string;
    ipAddress?: string;
    performedBy?: number | string;
    createdAt: string | Date;
}

export interface AlumniEvent {
    id: number;
    title: string;
    description: string;
    startDate: string | Date;
    endDate: string | Date;
    location: string;
    registrationFee?: number | null;
    requiresPayment: boolean;
    isActive: boolean;
    imageUrl?: string;
    registrationStartDate?: string | Date;
    registrationEndDate?: string | Date;
    adminNote?: string;
    allowNonMembers: boolean;
    participantCount?: number;
    requiresRegistration: boolean;
    participantLimit?: number | null;
    hasWaitlist?: boolean;
    // 82.52: write-only — admin's per-save choice of whether this create/update broadcasts a
    // notification. Not returned by GET, so both are optional here.
    notifyMembers?: boolean;
    notifyOnUpdate?: boolean;
}

export interface EventParticipantSummary {
    id: number;
    memberId?: number;
    guestName?: string;
    guestEmail?: string;
    guestMobile?: string;
    isNonMember: boolean;
    registrationDate: string | Date;
}

export interface EventTask {
    id: number;
    eventId: number;
    title: string;
    description?: string;
    assignedToMemberId?: number;
    isCompleted: boolean;
    dueDate?: string | Date;
}

export interface EventExpense {
    id: number;
    budgetId: number;
    title: string;
    amount: number;
    expenseDate: string | Date;
    notes?: string;
}

export interface EventBudget {
    id: number;
    eventId: number;
    allocatedAmount: number;
    spentAmount: number;
    expenses: EventExpense[];
}

export interface PagedRegistrations {
    items: EventRegistration[];
    totalCount: number;
    page: number;
    pageSize: number;
}


export interface ChatMessage {
    id: number;
    senderId: number;
    receiverId?: number;
    groupId?: string;
    message: string;
    isAnnouncement: boolean;
    sentAt: string | Date;
    senderName?: string;
}

export interface ContactMessage {
    id: number;
    name: string;
    email: string;
    subject: string;
    message: string;
    submittedAt: string | Date;
    isRead: boolean;
}

export interface EmailLog {
    id: number;
    recipientEmail: string;
    subject: string;
    body: string;
    sentDate: string | Date;
    status: 'Sent' | 'Failed';
    errorMessage?: string;
    targetAudience?: string;
}

export interface EventPhoto {
    id: number;
    eventGalleryId: number;
    photoPath: string;
    caption?: string;
    uploadedAt: string | Date;
    uploadedByMemberId?: number;
    status?: SubmissionStatus | string;
    rejectionReason?: string;
}

export interface EventGallery {
    id: number;
    title: string;
    description?: string;
    eventDate: string | Date;
    location?: string;
    createdAt: string | Date;
    createdByAdminId?: number;
    isActive: boolean;
    isFeatured: boolean;
    photos: EventPhoto[];
    ownerMemberId?: number;
    status?: SubmissionStatus | string;
    rejectionReason?: string;
}



export interface EventRegistration {
    id: number;
    eventId: number;
    eventTitle: string;
    memberId?: number;
    memberName?: string;
    isNonMember: boolean;
    guestName?: string;
    guestEmail?: string;
    guestMobile?: string;
    paymentReference: string;
    receiptPath?: string;
    paymentMethod: PaymentMethod;
    status: EventRegistrationStatus;
    registeredAt: string | Date;
    approvedAt?: string | Date;
}


export interface LookupItem {
    id: number;
    lookupGroup: string;
    code: string;
    value: string;
    order: number;
}

export interface Notification {
    id: number;
    memberId: number;
    title: string;
    message: string;
    type: string;
    isRead: boolean;
    relatedLink?: string;
    createdAt: string | Date;
}

export interface Role {
    id: number;
    name: string;
    userId: number;
}

export interface User {
    id: number;
    username: string;
    memberId?: number;
    isActive: boolean;
    createdAt: string | Date;
    roles?: string[];
}


export interface AcademicRecord {
    id?: number;
    institutionName: string;
    degree: string;
    subject: string;
    admissionYear?: number;
    passingYear: number;
    isGHC: boolean;
    result?: string;
    certificatePath?: string;
}

export interface ProfessionalRecord {
    id?: number;
    organizationName: string;
    designation: string;
    sector?: string;
    location?: string;
    startDate: string | Date;
    endDate?: string | Date;
    isCurrent: boolean;
}

export interface ECHistoryRecord {
    periodTitle: string;
    position: ECPosition;
    startDate: string | Date;
    endDate?: string | Date;
    changeReason?: string;
    isCurrent: boolean;
}

export interface MemberProfile {
    id: number;
    fullName: string;
    email: string;
    mobileNo: string;
    membershipNumber?: string;
    status: MembershipStatus;
    membershipType: MembershipType;
    category: MemberCategory;

    // Personal
    fatherName?: string;
    motherName?: string;
    dateOfBirth?: string | Date;
    gender: Gender;
    bloodGroup: BloodGroup;
    nid?: string;
    emergencyContactName?: string;
    emergencyContactRelation?: string;
    emergencyContactPhone?: string;
    tShirtSize?: string;
    isNIDPublic?: boolean;

    // Info
    photoPath?: string;
    signaturePath?: string;
    presentAddress: string;
    permanentAddress: string;

    // Privacy
    isMobilePublic: boolean;
    isEmailPublic: boolean;
    isAddressPublic: boolean;
    hasAcceptedTerms?: boolean;

    academicHistory: AcademicRecord[];
    professionalHistory: ProfessionalRecord[];
    ecHistory?: ECHistoryRecord[];
    paymentHistories?: PaymentHistory[];

    // Gamification & Health
    contributionPoints?: number;
    rank?: number;
    profileCompletionPercentage?: number;
}

export interface MemberSearchFilter {
    query?: string;
    passingYear?: number;
    bloodGroup?: BloodGroup;
    professionalSector?: string;
    designation?: string;
    category?: MemberCategory;
}

export interface MembershipHistory {
    id: number;
    memberId: number;
    oldType: string;
    newType: string;
    changeDate: string | Date;
    reason?: string;
    changedByAdminId?: number;
}

export interface MembershipFeeConfig {
    id: number;
    category: FinancialCategory;
    membershipType: string;
    amount: number;
    effectiveDate: string | Date;
    effectiveTo?: string | Date;
    isActive: boolean;
    description: string;
}

export interface CreateMembershipFeeConfig {
    category: FinancialCategory;
    membershipType: string;
    amount: number;
    effectiveDate: string | Date;
    effectiveTo?: string | Date;
    isActive: boolean;
    description: string;
}

export interface UpdateMembershipFeeConfig extends Partial<CreateMembershipFeeConfig> {
    id: number;
}

// ── Forum / Community Groups (3.7) ──────────────────────────────────────────

export interface ForumCategory {
    id: number;
    name: string;
    description?: string;
    sortOrder: number;
    topicCount: number;
    postCount: number;
}

export interface ForumTopic {
    id: number;
    categoryId: number;
    title: string;
    content: string;
    authorId: number;
    authorName: string;
    authorPhotoUrl?: string;
    createdAt: string | Date;
    lastUpdatedAt?: string | Date;
    viewCount: number;
    isPinned: boolean;
    isLocked: boolean;
    replyCount: number;
}

export interface ForumPost {
    id: number;
    topicId: number;
    content: string;
    authorId: number;
    authorName: string;
    authorPhotoUrl?: string;
    createdAt: string | Date;
    updatedAt?: string | Date;
    parentPostId?: number;
}

export interface CreateForumTopicDto {
    categoryId: number;
    title: string;
    content: string;
}

export interface CreateForumPostDto {
    topicId: number;
    content: string;
    parentPostId?: number;
}

// TODO 37.3: fundraising campaigns + donor honour roll.
export type PledgeStatus = 'Pledged' | 'PartiallyPaid' | 'Paid' | 'Lapsed' | 'Cancelled';

export interface Campaign {
    id: number;
    title: string;
    slug: string;
    story: string;
    coverImagePath?: string;
    targetAmount: number;
    amountReceived: number;
    startsOn: string | Date;
    endsOn?: string | Date;
    isActive: boolean;
    isArchived?: boolean;
}

export interface CreateCampaignPayload {
    title: string;
    slug: string;
    story: string;
    coverImagePath?: string;
    targetAmount: number;
    startsOn: string | Date;
    endsOn?: string | Date;
    isActive: boolean;
}

export interface UpdateCampaignPayload extends CreateCampaignPayload {
    id: number;
    isArchived: boolean;
}

export interface CampaignPledge {
    id: number;
    campaignId: number;
    memberId?: number | null;
    donorName: string;
    amount: number;
    amountReceived: number;
    status: PledgeStatus;
    isAnonymous: boolean;
    message?: string;
    pledgedAt: string | Date;
}

export interface CreatePledgePayload {
    amount: number;
    donorName?: string;
    donorEmail?: string;
    donorPhone?: string;
    isAnonymous: boolean;
    message?: string;
}

export interface DonorRecognitionTier {
    id: number;
    name: string;
    minimumAmount: number;
    description?: string;
}

export interface HonourRollEntry {
    displayName: string;
    amountReceived: number;
    message?: string;
}

export interface HonourRollTier {
    tierName: string;
    minimumAmount: number;
    donors: HonourRollEntry[];
}

export interface CampaignHonourRoll {
    targetAmount: number;
    totalReceived: number;
    progressPercent: number;
    donorCount: number;
    tiers: HonourRollTier[];
    untiered: HonourRollEntry[];
}
