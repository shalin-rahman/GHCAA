export type MembershipStatus = 'Applied' | 'Active' | 'InactivePayment' | 'InactiveResigned' | 'Terminated';
export type MembershipType = 'Founding' | 'Executive' | 'General' | 'Associate' | 'Honorary' | 'Advisory';
export type MemberCategory = 'None' | 'Lifelong' | 'Donor' | 'Patron';
export type ECPosition = 'None' | 'President' | 'VicePresident' | 'GeneralSecretary' | 'OfficeSecretary' | 'JointSecretary1' | 'JointSecretary2' | 'Treasurer' | 'MediaCulturalAndSportsSecretary' | 'OrganizationalSecretary' | 'InformationAndTechnologySecretary' | 'Member1' | 'Member2' | 'LawSecretary' | 'ImmediatePastPresident' | 'InstitutionalRepresentative';
export type Gender = 'Male' | 'Female' | 'Other';
export type BloodGroup = 'APositive' | 'ANegative' | 'BPositive' | 'BNegative' | 'OPositive' | 'ONegative' | 'ABPositive' | 'ABNegative';
export type RecordType = 'Income' | 'Expense';
export type FinancialCategory = 'MembershipFee' | 'Donation' | 'Event' | 'Maintenance' | 'Salary' | 'Utilities' | 'Other';

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
    hscAdmissionYear?: number;
    highestCertificate: string;
    highestCertificateGroup: string;
    highestCertificateSubject: string;
    highestCertificatePassingYear: number;

    ghcAdmissionYear?: number;
    ghcLastCertificate: string;
    ghcLastCertificateGroup: string;
    ghcLastCertificateSubject: string;
    ghcLastCertificatePassingYear: number;
    professionalSector: string;
    designation: string;
    photoPath?: string;
    certificatePath?: string;
    paymentProofPath?: string;
    status: MembershipStatus;
    appliedDate: string | Date;
    approvedDate?: string | Date;
    membershipNumber?: string;
    membershipType: MembershipType;
    category: MemberCategory;
    ecPosition: ECPosition;
    ecHistory?: ECMember[];
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
    category: FinancialCategory;
    date: string | Date;
    amount: number;
    description: string;
    reference?: string;
    createdAt: string | Date;
}

export interface NewsPost {
    id: number;
    title: string;
    content: string;
    category: string;
    publishDate: string | Date;
    imageUrl?: string;
    isPublished: boolean;
}

export interface JobOpportunity {
    id: number;
    title: string;
    company: string;
    location: string;
    description: string;
    postedDate: string | Date;
    expiryDate?: string | Date;
    jobType: string;
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
    eventDate: string | Date;
    location: string;
    registrationFee: number;
    isRegistrationOpen: boolean;
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

export interface EventGallery {
    id: number;
    eventId?: number;
    title: string;
    imagePath: string;
    uploadedAt: string | Date;
}

export interface EventRegistration {
    id: number;
    eventId: number;
    memberId: number;
    registrationDate: string | Date;
    paymentStatus: string;
    transactionId?: string;
}

export interface LookupItem {
    id: number;
    category: string;
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

