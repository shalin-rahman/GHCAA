export type MembershipStatus = 'Applied' | 'Active' | 'InactivePayment' | 'InactiveResigned' | 'Terminated';
export type MembershipType = 'Founding' | 'Executive' | 'General' | 'Associate' | 'Honorary' | 'Advisory';
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

    photoPath?: string;
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
}

export interface PaymentHistory {
    id: number;
    memberId: number;
    transactionId: string;
    amount: number;
    paidAt: string | Date;
    status: PaymentStatus;
    financialCategory: FinancialCategory;
    notes?: string;
    paymentMethod: PaymentMethod;
    receiptPath?: string;
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


export interface NewsPost {
    id: number;
    title: string;
    content: string;
    articleCategory: ArticleCategory;
    status: SubmissionStatus;
    imageUrl?: string;
    isActive: boolean;
    authorName?: string;
    createdAt: string | Date;
}

export interface CreateNewsDto {
    title: string;
    content: string;
    articleCategory: ArticleCategory;
    status?: SubmissionStatus;
    imageUrl?: string;
    isActive?: boolean;
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
    date: string; // MM-DD
    primaryColor: string;
    accentColor: string;
    logoSecondary?: string;
    greetingMessage: string;
    isActive: boolean;
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
    date: string | Date;
    location: string;
    registrationFee?: number | null;
    requiresPayment: boolean;
    isActive: boolean;
    imageUrl?: string;
    registrationDeadline?: string | Date;
    adminNote?: string;
    allowNonMembers: boolean;
    participantCount?: number;
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

    // Info
    photoPath?: string;
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
}

export interface MemberSearchFilter {
    query?: string;
    passingYear?: number;
    bloodGroup?: BloodGroup;
    professionalSector?: string;
    designation?: string;
    category?: MemberCategory;
}


